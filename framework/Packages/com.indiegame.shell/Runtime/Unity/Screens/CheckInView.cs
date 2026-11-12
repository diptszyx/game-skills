using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IndieGame.Shell.Unity.UI;
namespace IndieGame.Shell.Unity.Screens
{
    public sealed class CheckInView
    {
        readonly ListSurface surface;
        readonly RectTransform homeRoot; readonly ScreenLifetime homeLifetime;
        readonly DayCellView[] cells; readonly Button claim; readonly TMP_Text heading,error;
        public bool UsesDayCells => cells!=null;
        public RectTransform Root => homeRoot!=null?homeRoot:surface.Root;
        public ScreenLifetime Lifetime => homeLifetime!=null?homeLifetime:surface.Lifetime;
        public ListDisplay Snapshot { get; private set; }
        public string Error { get; private set; }
        public event Action ClaimRequested;
        public CheckInView(ShellUiFactory factory, Transform parent) {
            if(!factory.Style.EnableHomeChrome) { surface=new ListSurface(factory,parent,"CheckIn"); return; }
            homeRoot=factory.ScreenRoot(parent,"CheckIn"); homeLifetime=homeRoot.gameObject.AddComponent<ScreenLifetime>();
            var content=ShellUiFactory.Node("Check-in content",homeRoot); ShellUiFactory.Stretch(content);
            content.offsetMin=new Vector2(24,240); content.offsetMax=new Vector2(-24,-110); content.gameObject.SetActive(true);
            heading=factory.Label(content,"",true,"screenTitle"); TopBarView.Place(heading.rectTransform,new Vector2(0,.82f),Vector2.one);
            cells=new DayCellView[5];
            for(int i=0;i<5;i++)
            {
                cells[i]=new DayCellView(factory,content); int col=i<3?i:i-3;
                float shift=i<3?0f:.165f; TopBarView.Place(cells[i].Root,new Vector2(.02f+col*.33f+shift,i<3?.49f:.20f),new Vector2(.31f+col*.33f+shift,i<3?.78f:.49f));
            }
            claim=factory.Button(content,"",clicked:RequestClaim);
            var label=claim.GetComponentInChildren<TMP_Text>(); label.fontSize=label.fontSizeMax=40; label.fontStyle=FontStyles.Bold; TopBarView.Place((RectTransform)claim.transform,new Vector2(.17f,.05f),new Vector2(.83f,.17f));
            error=factory.Label(content,""); TopBarView.Place(error.rectTransform,Vector2.zero,new Vector2(1,.05f));
        }
        public void Render(ListDisplay display) { Snapshot=display; if(surface!=null) surface.Render(display,_=>RequestClaim());
            else { heading.text=display.Title; claim.GetComponentInChildren<TMP_Text>().text=System.Linq.Enumerable.First(display.Rows,r=>!string.IsNullOrEmpty(r.Action)).Action; claim.interactable=false;
                foreach(var row in display.Rows) if(row.Enabled) { claim.interactable=true; break; } } }
        public void RenderDays(string[] labels,string[] rewards,DayState[] states)
        { if(cells!=null) for(int i=0;i<cells.Length;i++) cells[i].Render(labels[i],rewards[i],states[i]); }
        public void RequestClaim() { if(Snapshot!=null) foreach(var r in Snapshot.Rows) if(r.Enabled) { ClaimRequested?.Invoke(); break; } }
        public void ShowError(string error) { Error=error; if(surface!=null) surface.Error(error); else this.error.text=error; }
    }
}
