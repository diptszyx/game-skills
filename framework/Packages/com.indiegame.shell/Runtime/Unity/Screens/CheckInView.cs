using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IndieGame.Shell.Unity.UI;
namespace IndieGame.Shell.Unity.Screens
{
    public sealed class CheckInView
    {
        const int Days=CheckInPack.Days, ModalColumns=4, FinalDaySpan=2;
        internal const float CellsMargin=.035f, CellGap=.013f, CellsBottom=.21f, CellsTop=.31f;
        internal const float ModalCellGap=.02f, ModalCellHeight=160, ModalRowGap=24, ModalErrorHeight=40, ModalTodayScale=1.08f;
        internal static float CellWidth=>(1-2*CellsMargin-(Days-1)*CellGap)/Days;
        internal static float ModalCellWidth=>(1-(ModalColumns-1)*ModalCellGap)/ModalColumns;
        internal static float ModalContentHeight=>2*ModalCellHeight+ModalRowGap+ModalErrorHeight;
        readonly ListSurface surface;
        readonly RectTransform homeRoot; readonly ScreenLifetime homeLifetime;
        readonly DayCellView[] cells; readonly TMP_Text heading,error;
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
            cells=new DayCellView[Days];
            for(int i=0;i<Days;i++)
            {
                cells[i]=new DayCellView(factory,content,modal!=null?ModalTodayScale:1,RequestClaim);
                if(modal!=null) PlaceModalCell(cells[i].Root,i);
                else { float left=CellsMargin+i*(CellWidth+CellGap); TopBarView.Place(cells[i].Root,new Vector2(left,CellsBottom),new Vector2(left+CellWidth,CellsTop)); }
            }
            error=factory.Label(content,"");
            if(modal!=null)
            {
                var errorRect=error.rectTransform; errorRect.anchorMin=Vector2.zero; errorRect.anchorMax=new Vector2(1,0); errorRect.pivot=new Vector2(.5f,0);
                errorRect.anchoredPosition=Vector2.zero; errorRect.sizeDelta=new Vector2(0,ModalErrorHeight);
                modal.FitModal(ModalContentHeight);
            }
            else TopBarView.Place(error.rectTransform,new Vector2(.04f,.105f),new Vector2(.96f,.15f));
        }
        // Four columns; the final day spans the last two columns of the second row.
        static void PlaceModalCell(RectTransform cell,int index)
        {
            int row=index/ModalColumns, column=index%ModalColumns, span=index==Days-1?FinalDaySpan:1;
            float left=column*(ModalCellWidth+ModalCellGap), right=left+span*ModalCellWidth+(span-1)*ModalCellGap;
            cell.anchorMin=new Vector2(left,1); cell.anchorMax=new Vector2(right,1); cell.pivot=new Vector2(.5f,1);
            cell.anchoredPosition=new Vector2(0,-row*(ModalCellHeight+ModalRowGap)); cell.sizeDelta=new Vector2(0,ModalCellHeight);
        }
        public void Render(ListDisplay display) { Snapshot=display; if(surface!=null) surface.Render(new ListDisplay(display.Title,System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(display.Rows,r=>new RowDisplay(r.Id,r.Text,r.Action,r.Enabled,r.Progress,"gift"))),display.Empty),_=>RequestClaim());
            else heading.text=display.Title; }
        public void RenderDays(string[] labels,string[] rewards,DayState[] states,bool[] goldOnly=null,string claimLabel=null)
        { if(cells!=null) for(int i=0;i<cells.Length;i++) cells[i].Render(labels[i],rewards[i],states[i],goldOnly!=null && goldOnly[i],claimLabel); }
        public void RequestClaim() { if(Snapshot!=null) foreach(var r in Snapshot.Rows) if(r.Enabled) { ClaimRequested?.Invoke(); break; } }
        public RectTransform SlotRect(int slot)=>cells!=null?cells[slot].Root:surface.RowRect(slot.ToString());
        public void ShowError(string error) { Error=error; if(surface!=null) surface.Error(error); else this.error.text=error; }
    }
}
