"""Validate T15 results without modifying the restricted T14 smoke runner."""
import pathlib, sys, json, subprocess, xml.etree.ElementTree as ET
root=pathlib.Path(__file__).resolve().parents[4]
new={'EditMode':('SessionScreenTests',['LeaderboardRendersEmptyErrorAndRank','LeaderboardIgnoresStaleResponse','ReviveGrantsOnlyOnCompletedAd','ReviveAdSkippedLeavesStateUnchanged','DoubleClickExecutesOnce','SettingsToggleCommitsAndSurfacesError']),
     'PlayMode':('SessionFlowTests',['LoadingBlocksInputUntilHidden','LoadingProgressIsRendered','AllFivePopupFlows','LeaveCancelKeepsPause','EveryPauseExitRestoresPriorState','MandatoryPopupsRejectBack'])}
runner=(root/'tests/run_unity_smoke.sh').read_text()
prefix=runner.split("<<'PY'\n",1)[1].split("print('mode='",1)[0]
for mode,(fixture,names) in new.items():
    scope={}; saved=sys.argv; sys.argv=['validate','','','',''+mode]
    try: exec(prefix,scope)
    finally: sys.argv=saved
    expected=scope['expected']|{'IndieGame.Shell.Tests.'+fixture+'.'+n for n in names}
    cases=list(ET.parse(root/f'unity-smoke/Logs/{mode}.xml').getroot().iter('test-case'))
    actual={c.get('fullname') for c in cases}
    errors=sum('error CS' in l for l in (root/f'unity-smoke/Logs/{mode}-Editor.log').read_text().splitlines())
    assert actual==expected,(mode,expected-actual,actual-expected)
    assert len(cases)==len(expected) and all(c.get('result')=='Passed' for c in cases)
    assert errors==0
    print(f'{mode}: passed={len(cases)} expected={len(expected)} exact names PASS; failed=0 skipped=0 error CS=0')
core=root/'Packages/com.indiegame.shell/Runtime/Core'
assert json.loads((core/'IndieGame.Shell.Core.asmdef').read_text())['noEngineReferences'] is True
matches=subprocess.run(['rg','UnityEngine',str(core)],capture_output=True,text=True)
assert matches.returncode==1
print('Core: noEngineReferences=true; UnityEngine matches=0 PASS')
