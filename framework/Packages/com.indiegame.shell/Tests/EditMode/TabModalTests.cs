using System.Linq;
using IndieGame.Shell.Unity.Screens;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Tests
{
    public sealed class TabModalTests
    {
        PopupFixture f; TabModals modals;
        [SetUp] public void Setup() { f=new PopupFixture(); modals=new TabModals(f.Host,f.Factory); }
        [TearDown] public void Cleanup() { f.Dispose(); }
        [Test] public void ModalTabsAreOnByDefaultAndNeedHomeChrome()
        {
            var style=ScriptableObject.CreateInstance<ShellStyle>();
            Assert.That(style.TabsAsModals,Is.True); Assert.That(style.ModalTabs,Is.False,"no chrome means no tab bar");
            style.EnableHomeChrome=true; Assert.That(style.ModalTabs,Is.True);
            style.TabsAsModals=false; Assert.That(style.ModalTabs,Is.False);
            Object.DestroyImmediate(style);
        }
        [Test] public void FrameLeavesTheTabBarStripOutsideTheScrim()
        {
            var frame=modals.CreateFrame(ShellScreen.Quests,"Close");
            Assert.That(((RectTransform)frame.transform).offsetMin.y,Is.EqualTo(160f)); Assert.That(frame.Policy,Is.EqualTo(DismissalPolicy.Dismissible));
            Assert.That(frame.Panel.anchorMin.x,Is.GreaterThan(0f)); Assert.That(frame.Panel.anchorMax.x,Is.LessThan(1f));
        }
        [Test] public void ListInFrameUsesThePopupTitleAndHasNoBackButtonOrOwnHeading()
        {
            var frame=modals.CreateFrame(ShellScreen.Quests,"Close"); var view=new QuestsView(f.Factory,f.Root.transform,frame);
            view.Render(new ListDisplay("Quests",new[]{new RowDisplay("q","Quest","Claim",true)}));
            Assert.That(view.Root.parent,Is.SameAs(frame.Content)); Assert.That(frame.Panel.Find("Title").GetComponent<TMP_Text>().text,Is.EqualTo("Quests"));
            Assert.That(view.Root.GetComponentsInChildren<ShellAppearance>(true).Any(a=>a.Role=="screenTitle"),Is.False);
            Assert.That(view.Root.GetComponentsInChildren<ActionAccessibility>(true).Any(a=>a.Key=="back"),Is.False);
        }
        [Test] public void RowsInsideAFrameStillRaiseClaims()
        {
            var frame=modals.CreateFrame(ShellScreen.Quests,"Close"); var view=new QuestsView(f.Factory,f.Root.transform,frame); string claimed=null;
            view.ClaimRequested+=id=>claimed=id;
            view.Render(new ListDisplay("Quests",new[]{new RowDisplay("q","Quest","Claim",true)}));
            var button=view.Root.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b=>b.GetComponent<ActionAccessibility>()!=null);
            button.onClick.Invoke(); Assert.That(claimed,Is.EqualTo("q"));
        }
        [Test] public void ShopRowsInsideAFrameStillRaiseBuys()
        {
            var frame=modals.CreateFrame(ShellScreen.Shop,"Close"); var view=new ShopView(f.Factory,f.Root.transform,f.Host,frame); string bought=null;
            view.BuyRequested+=id=>bought=id;
            view.Render(new ListDisplay("Shop",new[]{new RowDisplay("p","Product","Buy",true,icon:"coin")}));
            var button=view.Root.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b=>b.GetComponent<ActionAccessibility>()!=null);
            button.onClick.Invoke(); Assert.That(bought,Is.EqualTo("p"));
        }
        [Test] public void OpeningASecondTabReplacesTheFirstAndBackClosesIt()
        {
            modals.CreateFrame(ShellScreen.CheckIn,"Close"); modals.CreateFrame(ShellScreen.Quests,"Close");
            modals.Open(ShellScreen.CheckIn); Assert.That(modals.Active,Is.EqualTo(ShellScreen.CheckIn));
            modals.Open(ShellScreen.Quests); Assert.That(modals.Active,Is.EqualTo(ShellScreen.Quests)); Assert.That(f.Host.Top,Is.SameAs(modals.Frame(ShellScreen.Quests)));
            Assert.That(f.Host.Back(),Is.True); Assert.That(modals.Active,Is.Null);
        }
        const float Area=1440, RowHeight=120;
        static void SetArea(PopupView frame,float height=Area)
        {
            var root=(RectTransform)frame.transform; root.anchorMin=root.anchorMax=Vector2.zero; root.pivot=Vector2.zero;
            root.anchoredPosition=Vector2.zero; root.sizeDelta=new Vector2(720,height);
        }
        static Rect InPanel(PopupView frame,RectTransform rect)
        {
            var corners=new Vector3[4]; rect.GetWorldCorners(corners);
            var a=frame.Panel.InverseTransformPoint(corners[0]); var b=frame.Panel.InverseTransformPoint(corners[2]);
            return Rect.MinMaxRect(a.x,a.y,b.x,b.y);
        }
        static ListDisplay QuestRows(int count)=>new ListDisplay("Quests",Enumerable.Range(0,count).Select(i=>new RowDisplay("q"+i,"Quest "+i,"Claim",true,.5f,detail:"1/2 · +7")).ToArray());
        float[] PanelHeights(ShellScreen screen,params int[] counts)
        {
            var frame=modals.CreateFrame(screen,"Close"); SetArea(frame); var view=new QuestsView(f.Factory,f.Root.transform,frame);
            modals.Open(screen); var heights=new float[counts.Length];
            for(int i=0;i<counts.Length;i++) { view.Render(QuestRows(counts[i])); heights[i]=frame.Panel.rect.height; }
            return heights;
        }
        [Test] public void PanelHeightGrowsWithRowCountAndIsClamped()
        {
            var modal=f.Factory.Style.Modal; float min=modal.MinHeightFraction*Area, max=modal.MaxHeightFraction*Area;
            var h=PanelHeights(ShellScreen.Quests,1,3,6,40);
            Assert.That(h[0],Is.EqualTo(min).Within(.5f),"one row sits on the minimum");
            Assert.That(h[1],Is.GreaterThan(h[0])); Assert.That(h[2],Is.GreaterThan(h[1]));
            Assert.That(h[2]-h[1],Is.EqualTo(3*(RowHeight+f.Factory.Style.Spacing)).Within(.5f),"each row adds one row height plus spacing");
            Assert.That(h[3],Is.EqualTo(max).Within(.5f),"many rows stop at the maximum and scroll");
        }
        [Test] public void ModalLimitsAreStyleData()
        {
            f.Factory.Style.Modal.MinHeightFraction=.2f; f.Factory.Style.Modal.MaxHeightFraction=.5f; f.Factory.Style.Modal.WidthFraction=.7f;
            var h=PanelHeights(ShellScreen.Quests,1,40);
            Assert.That(h[0],Is.GreaterThanOrEqualTo(.2f*Area-.5f)); Assert.That(h[1],Is.EqualTo(.5f*Area).Within(.5f));
            Assert.That(modals.Frame(ShellScreen.Quests).Panel.rect.width,Is.EqualTo(.7f*720).Within(.5f));
        }
        [Test] public void PanelIsCentredAboveTheTabBarAndUsesTheMeasuredWidth()
        {
            var frame=modals.CreateFrame(ShellScreen.Quests,"Close"); SetArea(frame); var view=new QuestsView(f.Factory,f.Root.transform,frame);
            modals.Open(ShellScreen.Quests); view.Render(QuestRows(3));
            Assert.That(frame.Panel.anchorMin.y,Is.EqualTo(.5f)); Assert.That(frame.Panel.anchorMax.y,Is.EqualTo(.5f)); Assert.That(frame.Panel.anchoredPosition.y,Is.Zero);
            Assert.That(frame.Panel.rect.width/720,Is.EqualTo(f.Factory.Style.Modal.WidthFraction).Within(.002f));
            Assert.That(f.Factory.Style.Modal.WidthFraction,Is.EqualTo(.894f).Within(1e-4f),"965/1080 [measured: Settings panel]");
        }
        [Test] public void RowsStayInsideThePanelWithoutOverlap()
        {
            var frame=modals.CreateFrame(ShellScreen.Quests,"Close"); SetArea(frame); var view=new QuestsView(f.Factory,f.Root.transform,frame);
            modals.Open(ShellScreen.Quests); view.Render(QuestRows(3));
            var scroll=frame.Content.GetComponentInChildren<ScrollRect>(); LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
            var panel=frame.Panel.rect; var rows=view.Root.GetComponentsInChildren<Image>(true).Where(i=>i.name=="List Row").Select(i=>InPanel(frame,i.rectTransform)).ToArray();
            Assert.That(rows.Length,Is.EqualTo(3));
            foreach(var row in rows) { Assert.That(row.xMin,Is.GreaterThanOrEqualTo(panel.xMin-.5f)); Assert.That(row.xMax,Is.LessThanOrEqualTo(panel.xMax+.5f)); Assert.That(row.yMin,Is.GreaterThanOrEqualTo(panel.yMin-.5f)); Assert.That(row.yMax,Is.LessThanOrEqualTo(panel.yMax+.5f)); }
            for(int i=1;i<rows.Length;i++) Assert.That(rows[i].yMax,Is.LessThanOrEqualTo(rows[i-1].yMin+.5f),"rows do not overlap");
            Assert.That(rows[2].yMin-panel.yMin,Is.LessThan(120f),"no empty area below the last row");
        }
        [Test] public void QuestProgressTrackSitsInsideTheCardAndAboveTheDetailLine()
        {
            var frame=modals.CreateFrame(ShellScreen.Quests,"Close"); SetArea(frame); var view=new QuestsView(f.Factory,f.Root.transform,frame);
            modals.Open(ShellScreen.Quests); view.Render(QuestRows(1));
            var card=view.Root.GetComponentsInChildren<Image>(true).First(i=>i.name=="List Row").rectTransform; var track=(RectTransform)card.Find("Progress"); var detail=(RectTransform)card.Find("Detail");
            var cardRect=InPanel(frame,card); var trackRect=InPanel(frame,track); var detailRect=InPanel(frame,detail);
            Assert.That(cardRect.Contains(trackRect.min)&&cardRect.Contains(trackRect.max),Is.True,"track inside the card");
            Assert.That(trackRect.yMax,Is.LessThanOrEqualTo(detailRect.yMin+.5f),"track does not overlap the detail line");
            var fill=track.GetChild(0).GetComponent<Image>().color; var trackColor=track.GetComponent<Image>().color; var row=f.Factory.Style.ListRowColor; row.a=1;
            Assert.That(ShellStyle.Contrast(trackColor,row),Is.GreaterThanOrEqualTo(1.8f),"track visible against the card"); Assert.That(ShellStyle.Contrast(fill,trackColor),Is.GreaterThanOrEqualTo(3f),"fill visible against the track");
        }
        static readonly string[] SevenLabels={"Day 1","Day 2","Day 3","Day 4","Day 5","Day 6","Day 7"};
        static readonly string[] SevenRewards={"10","20","30","40","50","60","100"};
        static readonly DayState[] SevenStates={DayState.Collected,DayState.Today,DayState.Locked,DayState.Locked,DayState.Locked,DayState.Locked,DayState.Locked};
        [Test] public void CheckInCellsFillThePanelInFourPlusThreeRowsWithEqualGapsAndLeaveNoVoid()
        {
            f.Factory.Style.EnableHomeChrome=true; var frame=modals.CreateFrame(ShellScreen.CheckIn,"Close"); SetArea(frame);
            var view=new CheckInView(f.Factory,f.Root.transform,frame); modals.Open(ShellScreen.CheckIn);
            view.RenderDays(SevenLabels,SevenRewards,SevenStates);
            var cells=view.Root.GetComponentsInChildren<ShellAppearance>(true).Where(a=>a.Role.StartsWith("day")).Select(a=>InPanel(frame,(RectTransform)a.transform.parent)).ToArray();
            var content=InPanel(frame,frame.Content); Assert.That(cells.Length,Is.EqualTo(CheckInPack.Days));
            foreach(var c in cells.Take(6)) { Assert.That(c.width,Is.EqualTo(cells[0].width).Within(.5f)); Assert.That(c.height,Is.EqualTo(cells[0].height).Within(.5f)); }
            for(int i=0;i<4;i++) Assert.That(cells[i].yMax,Is.EqualTo(cells[0].yMax).Within(.5f),"first row");
            for(int i=4;i<7;i++) Assert.That(cells[i].yMax,Is.EqualTo(cells[4].yMax).Within(.5f),"second row");
            Assert.That(cells[4].yMax,Is.LessThan(cells[0].yMin),"rows do not overlap");
            float gap=cells[1].xMin-cells[0].xMax; Assert.That(gap,Is.GreaterThan(0));
            for(int i=1;i<4;i++) Assert.That(cells[i].xMin-cells[i-1].xMax,Is.EqualTo(gap).Within(.5f),"equal gaps in row one");
            for(int i=5;i<7;i++) Assert.That(cells[i].xMin-cells[i-1].xMax,Is.EqualTo(gap).Within(.5f),"equal gaps in row two");
            Assert.That(cells[6].width,Is.EqualTo(2*cells[0].width+gap).Within(.5f),"day 7 spans two columns");
            Assert.That(cells[0].xMin,Is.EqualTo(content.xMin).Within(.5f)); Assert.That(cells[3].xMax,Is.EqualTo(content.xMax).Within(.5f),"row one spans the content width");
            Assert.That(cells[4].xMin,Is.EqualTo(content.xMin).Within(.5f)); Assert.That(cells[6].xMax,Is.EqualTo(content.xMax).Within(.5f),"row two spans the content width");
            Assert.That(view.Root.GetComponentsInChildren<Button>(true).Length,Is.EqualTo(CheckInPack.Days),"no separate claim button");
            var panel=frame.Panel.rect; Assert.That(cells[4].yMin-panel.yMin,Is.LessThanOrEqualTo(40+24+1),"only the error line and bottom inset remain below the cells");
            Assert.That(panel.height,Is.LessThan(f.Factory.Style.Modal.MaxHeightFraction*Area));
        }
        [Test] public void CheckInEmphasisesTodayOnly()
        {
            f.Factory.Style.EnableHomeChrome=true; var frame=modals.CreateFrame(ShellScreen.CheckIn,"Close"); SetArea(frame);
            var view=new CheckInView(f.Factory,f.Root.transform,frame); modals.Open(ShellScreen.CheckIn);
            view.RenderDays(SevenLabels,SevenRewards,SevenStates);
            var scales=view.Root.GetComponentsInChildren<ShellAppearance>(true).Where(a=>a.Role.StartsWith("day")).Select(a=>a.transform.localScale.x).ToArray();
            Assert.That(scales[1],Is.GreaterThan(1f)); foreach(int i in new[]{0,2,3,4,5,6}) Assert.That(scales[i],Is.EqualTo(1f));
        }
        [Test] public void CheckInOnlyTodaysCellIsInteractiveAndRaisesClaim()
        {
            f.Factory.Style.EnableHomeChrome=true; var frame=modals.CreateFrame(ShellScreen.CheckIn,"Close"); SetArea(frame);
            var view=new CheckInView(f.Factory,f.Root.transform,frame); modals.Open(ShellScreen.CheckIn);
            view.Render(new ListDisplay("Check in",new[]{new RowDisplay("1","Slot 1","Claim",true)}));
            view.RenderDays(SevenLabels,SevenRewards,SevenStates,null,"Claim"); int claims=0; view.ClaimRequested+=()=>claims++;
            var buttons=view.Root.GetComponentsInChildren<Button>(true);
            Assert.That(buttons.Select(b=>b.interactable).ToArray(),Is.EqualTo(new[]{false,true,false,false,false,false,false}));
            Assert.That(buttons[1].GetComponent<ActionAccessibility>().Label,Is.EqualTo("Claim")); Assert.That(buttons[2].GetComponent<ActionAccessibility>().Label,Is.EqualTo("Day 3"));
            buttons[1].onClick.Invoke(); Assert.That(claims,Is.EqualTo(1));
        }
        [Test] public void LeaderboardEmptyStateIsACompactCardAndOwnRowsAreHighlighted()
        {
            var frame=modals.CreateFrame(ShellScreen.Leaderboard,"Close"); SetArea(frame); var view=new LeaderboardView(f.Factory,f.Root.transform,frame); var text=new ScreenText(f.Session,ScreenTestData.Text());
            modals.Open(ShellScreen.Leaderboard); view.Render(new LeaderboardSnapshot{State=LeaderboardState.Empty,MessageKey="shell.leaderboard.empty"},text);
            var card=(RectTransform)view.Root.GetComponentsInChildren<RectTransform>(true).First(r=>r.name=="Empty card");
            Assert.That(card.gameObject.activeSelf,Is.True); Assert.That(InPanel(frame,card).height,Is.EqualTo(RowHeight).Within(.5f));
            Assert.That(frame.Panel.rect.height,Is.EqualTo(f.Factory.Style.Modal.MinHeightFraction*Area).Within(.5f));
            view.Render(new LeaderboardSnapshot{State=LeaderboardState.Populated,Entries=new[]{new LeaderboardEntry("ann",90,1),new LeaderboardEntry("me",70,2),new LeaderboardEntry("bob",50,3)},MyRank=new LeaderboardEntry("me",70,2)},text);
            var rows=view.Root.GetComponentsInChildren<Image>(true).Where(i=>i.name=="List Row").ToArray(); Assert.That(rows.Length,Is.EqualTo(3),"own rank row is not repeated when the player is in the list");
            Assert.That(rows.Select(r=>r.GetComponentInChildren<TMP_Text>().fontStyle.HasFlag(FontStyles.Bold)).ToArray(),Is.EqualTo(new[]{false,true,false}),"own entry is bold");
            Assert.That(view.Root.GetComponentsInChildren<RectTransform>(true).Any(r=>r.name=="Highlight"),Is.False,"no accent bar");
        }
    }
}
