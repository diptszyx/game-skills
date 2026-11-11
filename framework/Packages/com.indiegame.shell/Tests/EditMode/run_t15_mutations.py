"""Targeted assertion-failure proofs; always restores exact production bytes."""
import pathlib, subprocess, xml.etree.ElementTree as ET
root=pathlib.Path(__file__).resolve().parents[4]
source=root/'Packages/com.indiegame.shell/Runtime/Unity/Screens'
mutations=[('LeaderboardView.cs','if(disposed||id!=requestId) return;','if(disposed) return;','LeaderboardIgnoresStaleResponse'),
 ('SessionPopupPresenter.cs','if(!ad.Success) { Error(p,ad.Error==ServiceError.None?ad.Outcome.ToString():ad.Error.ToString()); return; }','if(false) { Error(p,"Failed"); return; }','ReviveAdSkippedLeavesStateUnchanged')]
for file,before,after,test in mutations:
    path=source/file; original=path.read_bytes(); assert before in original.decode()
    result=root/'unity-smoke/Logs'/('T15-mutation-'+test+'.xml')
    try:
        path.write_text(original.decode().replace(before,after))
        assert subprocess.run(['pgrep','-f','Unity.app/Contents/MacOS/Unity'],capture_output=True).returncode==1
        process=subprocess.run([str(pathlib.Path.home()/'.unity/bin/unity'),'test',str(root/'unity-smoke'),'--mode','EditMode','--filter',test,'--output',str(result),'--timeout','180','--non-interactive'],capture_output=True,text=True)
        cases=list(ET.parse(result).getroot().iter('test-case'))
        assert len(cases)==1 and cases[0].get('result')=='Failed', process.stdout+process.stderr
        print(test, 'DELIBERATE FAILURE:', cases[0].findtext('.//message'), flush=True)
    finally:
        path.write_bytes(original)
