using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IndieGame.Shell.Unity.UI;
namespace IndieGame.Shell.Unity.Screens
{
    public sealed class CheckInView
    {
        internal const float CellsMargin=.035f, CellWidth=.17f, CellStep=.19f, CellsBottom=.21f, CellsTop=.31f, ClaimCenterY=.17f;
        internal const float ModalCellsBottom=.52f, ModalCellsTop=.64f, ModalClaimCenterY=.38f, ModalErrorBottom=.12f, ModalErrorTop=.20f;
        readonly ListSurface surface;
        readonly RectTransform homeRoot; readonly ScreenLifetime homeLifetime;
        readonly DayCellView[] cells; readonly Button claim; readonly TMP_Text heading,error;
        public bool UsesDayCells => cells!=null;
        public RectTransform Root => homeRoot!=null?homeRoot:surface.Root;
        public ScreenLifetime Lifetime => homeLifetime!=null?homeLifetime:surface.Lifetime;
        public ListDisplay Snapshot { get; private set; }
        public string Error { get; private set; }
        public event Action ClaimRequested;
        public CheckInView(ShellUiFactory factory, Transform parent, PopupView modal=null) {
            if(!factory.Style.EnableHomeChrome) { surface=new ListSurface(factory,parent,"CheckIn"); return; }
            homeRoot=modal!=null?ListSurface.ModalRoot(modal,"CheckIn"):factory.ScreenRoot(parent,"CheckIn"); homeLifetime=homeRoot.gameObject.AddComponent<ScreenLifetime>();
            var content=ShellUiFactory.Node("Check-in content",homeRoot); ShellUiFactory.Stretch(content); content.gameObject.SetActive(true);
            if(modal!=null) heading=modal.Panel.Find("Title").GetComponent<TMP_Text>();
            else
            {
                heading=factory.Label(content,"",true,"screenTitle"); TopBarView.Place(heading.rectTransform,new Vector2(0,.82f),new Vector2(1,.90f));
                if(ScreenWidgets.HidesTitle(factory.Style)) heading.gameObject.SetActive(false);
            }
            float cellsBottom=modal!=null?ModalCellsBottom:CellsBottom, cellsTop=modal!=null?ModalCellsTop:CellsTop, claimY=modal!=null?ModalClaimCenterY:ClaimCenterY;
            cells=new DayCellView[5];
            for(int i=0;i<5;i++)
            {
                cells[i]=new DayCellView(factory,content); float left=CellsMargin+i*CellStep;
                TopBarView.Place(cells[i].Root,new Vector2(left,cellsBottom),new Vector2(left+CellWidth,cellsTop));
            }
            claim=factory.IconButton(content,"gift","Claim",RequestClaim,ShellLayoutCategory.IconAction);
            var claimRect=(RectTransform)claim.transform; claimRect.anchorMin=claimRect.anchorMax=new Vector2(.5f,claimY);
            error=factory.Label(content,""); if(modal!=null) TopBarView.Place(error.rectTransform,new Vector2(.04f,ModalErrorBottom),new Vector2(.96f,ModalErrorTop));
            else TopBarView.Place(error.rectTransform,new Vector2(.04f,.105f),new Vector2(.96f,.15f));
        }
        public void Render(ListDisplay display) { Snapshot=display; if(surface!=null) surface.Render(new ListDisplay(display.Title,System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(display.Rows,r=>new RowDisplay(r.Id,r.Text,r.Action,r.Enabled,r.Progress,"gift"))),display.Empty),_=>RequestClaim());
            else { heading.text=display.Title; claim.GetComponent<ActionAccessibility>().Set("gift",System.Linq.Enumerable.First(display.Rows,r=>!string.IsNullOrEmpty(r.Action)).Action); claim.interactable=false;
                foreach(var row in display.Rows) if(row.Enabled) { claim.interactable=true; break; } } }
        public void RenderDays(string[] labels,string[] rewards,DayState[] states)
        { if(cells!=null) for(int i=0;i<cells.Length;i++) cells[i].Render(labels[i],rewards[i],states[i]); }
        public void RequestClaim() { if(Snapshot!=null) foreach(var r in Snapshot.Rows) if(r.Enabled) { ClaimRequested?.Invoke(); break; } }
        public void ShowError(string error) { Error=error; if(surface!=null) surface.Error(error); else this.error.text=error; }
    }
}
