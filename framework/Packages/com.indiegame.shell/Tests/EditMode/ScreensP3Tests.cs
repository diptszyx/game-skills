using System;
using System.Linq;
using IndieGame.Shell.Unity.Screens;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Tests
{
    public sealed class ScreensP3Tests
    {
        const float ScreenW=720, ScreenH=1600, MinRowFont=26, MinRowOpacity=.9f;
        static readonly Rect Bear=Rect.MinMaxRect(0,.32f,1,.67f);
        DemoStyles demos; GameObject root; RectTransform screen; ShellUiFactory f; ScreenClock clock; ScreenFiles files; ShellSession session; LocalizationService text;
        readonly System.Collections.Generic.List<IDisposable> owned=new System.Collections.Generic.List<IDisposable>();
        [SetUp] public void Setup()
        {
            demos=new DemoStyles(); f=new ShellUiFactory(demos.Make(false)); f.Style.EnableHomeChrome=true; f.Style.ShowScreenTitleWithChrome=false;
            root=new GameObject("UI",typeof(RectTransform),typeof(Canvas)); screen=(RectTransform)new GameObject("Screen",typeof(RectTransform)).transform;
            screen.SetParent(root.transform,false); screen.anchorMin=screen.anchorMax=Vector2.zero; screen.pivot=Vector2.zero; screen.sizeDelta=new Vector2(ScreenW,ScreenH);
            clock=new ScreenClock(); files=new ScreenFiles(); session=ScreenTestData.Session(clock,files); session.LoadAll(); text=ScreenTestData.Text();
        }
        [TearDown] public void Cleanup() { foreach(var p in owned) p.Dispose(); owned.Clear(); UnityEngine.Object.DestroyImmediate(root); demos.Dispose(); }
        Rect Normalized(RectTransform r)
        {
            var c=new Vector3[4]; r.GetWorldCorners(c); var origin=screen.position;
            return Rect.MinMaxRect((c[0].x-origin.x)/ScreenW,(c[0].y-origin.y)/ScreenH,(c[2].x-origin.x)/ScreenW,(c[2].y-origin.y)/ScreenH);
        }
        static bool Overlaps(Rect a,Rect b) => a.xMin<b.xMax-1e-4f && b.xMin<a.xMax-1e-4f && a.yMin<b.yMax-1e-4f && b.yMin<a.yMax-1e-4f;
        static int TitleCount(RectTransform view) => view.GetComponentsInChildren<ShellAppearance>(false).Count(a=>a.Role=="screenTitle");
        static float EffectiveFont(TMP_Text t) => t.enableAutoSizing?t.fontSizeMin:t.fontSize;

        [Test] public void ScreenTitleHiddenWithChromeWhenFlagIsFalse()
        {
            var quests=new QuestsView(f,screen); var shop=new ShopView(f,screen,root.AddComponent<PopupHost>());
            var board=new LeaderboardView(f,screen); var check=new CheckInView(f,screen);
            foreach(var view in new[]{quests.Root,shop.Root,board.Root,check.Root}) Assert.That(TitleCount(view),Is.Zero,view.name);
        }
        [Test] public void ScreenTitleStaysByDefault()
        {
            f.Style.ShowScreenTitleWithChrome=true;
            Assert.That(TitleCount(new QuestsView(f,screen).Root),Is.EqualTo(1));
            f.Style.EnableHomeChrome=false; f.Style.ShowScreenTitleWithChrome=false;
            Assert.That(TitleCount(new ShopView(f,screen,root.AddComponent<PopupHost>()).Root),Is.EqualTo(1),"no chrome keeps the title");
        }
        [Test] public void ListGainsTheFreedTitleSpace()
        {
            var titled=ListTop(true); var untitled=ListTop(false);
            Assert.That(untitled,Is.GreaterThan(titled+.05f));
        }
        float ListTop(bool showTitle)
        {
            f.Style.ShowScreenTitleWithChrome=showTitle; var v=new QuestsView(f,screen);
            return Normalized(v.Root.GetComponentInChildren<ScrollRect>().GetComponent<RectTransform>()).yMax;
        }
        [Test] public void CheckInCellsAreOneEqualRowBelowTheBear()
        {
            var view=new CheckInView(f,screen); var p=new CheckInPresenter(session,view,text,clock); owned.Add(p);
            var cells=view.Root.GetComponentsInChildren<ShellAppearance>().Where(a=>a.Role.StartsWith("day")).Select(a=>Normalized((RectTransform)a.transform.parent)).ToArray();
            Assert.That(cells.Length,Is.EqualTo(CheckInPack.Days));
            foreach(var c in cells)
            {
                Assert.That(c.yMin,Is.EqualTo(cells[0].yMin).Within(1e-4f)); Assert.That(c.height,Is.EqualTo(cells[0].height).Within(1e-4f)); Assert.That(c.width,Is.EqualTo(cells[0].width).Within(1e-4f));
                Assert.That(Overlaps(c,Bear),Is.False,"day cell overlaps the bear"); Assert.That(c.xMin,Is.GreaterThanOrEqualTo(0)); Assert.That(c.xMax,Is.LessThanOrEqualTo(1)); Assert.That(c.width*ScreenW,Is.GreaterThanOrEqualTo(84),"cell wide enough at 720 wide");
            }
            for(int i=1;i<cells.Length;i++) Assert.That(cells[i].xMin,Is.GreaterThanOrEqualTo(cells[i-1].xMax));
        }
        [Test] public void DayCellShowsTheCurrencyIconOnlyForGoldOnlyRewards()
        {
            var cell=new DayCellView(f,screen);
            var icon=cell.Root.GetComponentsInChildren<ShellAppearance>(true).First(a=>a.Role=="currencyIcon");
            cell.Render("Day 1","25",DayState.Today,true);
            Assert.That(icon.gameObject.activeSelf,Is.True,"gold-only reward shows the coin icon");
            cell.Render("Day 1","Gold 25, Heart 1",DayState.Today,false);
            Assert.That(icon.gameObject.activeSelf,Is.False,"mixed reward keeps the text");
        }
        [Test] public void CheckInHasNoSeparateClaimButtonAndTappingTodaysCellClaims()
        {
            var view=new CheckInView(f,screen); var p=new CheckInPresenter(session,view,text,clock); owned.Add(p);
            var buttons=view.Root.GetComponentsInChildren<Button>(true);
            Assert.That(buttons.Length,Is.EqualTo(CheckInPack.Days),"only the day cells are buttons");
            Assert.That(buttons.All(b=>b.GetComponent<ActionAccessibility>().Key==DayCellView.AccessibilityKey&&b.GetComponent<PressFeedback>()!=null),Is.True);
            var live=buttons.Where(b=>b.interactable).ToArray(); Assert.That(live.Length,Is.EqualTo(1),"locked and collected cells are not interactive");
            Assert.That(live[0].GetComponent<ActionAccessibility>().Label,Is.EqualTo("Claim"));
            Assert.That(live[0].GetComponentsInChildren<ShellAppearance>().Any(a=>a.Role=="dayToday"),Is.True,"the tappable cell is today's");
            long gold=session.Economy.Gold; live[0].onClick.Invoke();
            Assert.That(session.Economy.Gold,Is.EqualTo(gold+25)); Assert.That(buttons.Any(b=>b.interactable),Is.False,"nothing to claim after collecting");
            live[0].onClick.Invoke(); Assert.That(session.Economy.Gold,Is.EqualTo(gold+25),"second tap grants nothing");
        }
        static Rect InCell(RectTransform r,RectTransform cell)
        {
            var c=new Vector3[4]; r.GetWorldCorners(c); var lo=cell.InverseTransformPoint(c[0]); var hi=cell.InverseTransformPoint(c[2]);
            return Rect.MinMaxRect(lo.x,lo.y,hi.x,hi.y);
        }
        [Test] public void CheckInDayCellStacksLabelIconAndRewardInsideEachStateWithoutOverlap()
        {
            var cell=new DayCellView(f,screen);
            foreach(float width in new[]{122f,260f})
            foreach(DayState state in Enum.GetValues(typeof(DayState)))
                foreach(bool gold in new[]{true,false})
                {
                    cell.Root.sizeDelta=new Vector2(width,160); var bounds=cell.Root.rect;
                    cell.Render("Day 1","25",state,gold);
                    var label=cell.Root.Find("Day label").GetComponent<TMP_Text>(); var reward=cell.Root.Find("Reward card/Reward text").GetComponent<TMP_Text>();
                    var rects=new[]{InCell(label.rectTransform,cell.Root),InCell((RectTransform)cell.Root.Find("State"),cell.Root),InCell((RectTransform)cell.Root.Find("Reward card"),cell.Root)};
                    foreach(var r in rects) { Assert.That(r.xMin>=bounds.xMin-1e-3f&&r.yMin>=bounds.yMin-1e-3f&&r.xMax<=bounds.xMax+1e-3f&&r.yMax<=bounds.yMax+1e-3f,Is.True,state+" rect outside cell: "+r+" bounds "+bounds); }
                    Assert.That(rects[0].yMin,Is.GreaterThanOrEqualTo(rects[1].yMax-1e-3f),state+" label overlaps icon"); Assert.That(rects[1].yMin,Is.GreaterThanOrEqualTo(rects[2].yMax-1e-3f),state+" icon overlaps reward");
                    Assert.That(label.fontSizeMin,Is.GreaterThanOrEqualTo(14)); Assert.That(reward.alignment,Is.EqualTo(TextAlignmentOptions.Center));
                    Assert.That(InCell(reward.rectTransform,cell.Root).center.x,Is.EqualTo(bounds.center.x).Within(.5f),state+" reward text off-centre");
                    Assert.That(reward.alpha,Is.EqualTo(state==DayState.Locked?DayCellView.LockedRewardAlpha:1f).Within(1e-4f)); Assert.That(reward.alpha,Is.GreaterThanOrEqualTo(.7f),state+" reward too faint");
                    Assert.That(reward.fontSizeMin,Is.GreaterThanOrEqualTo(14)); Assert.That(cell.Root.Find("Reward card").gameObject.activeSelf,Is.True,state+" hides its reward");
                    Assert.That(cell.Root.Find("State").GetComponent<Image>().preserveAspect,Is.True,"state sprite must fit its zone");
                }
        }
        [Test] public void CheckInLockedCellsStillShowTheirRewardAmount()
        {
            var cell=new DayCellView(f,screen); cell.Render("Day 3","100",DayState.Locked,true);
            Assert.That(cell.Root.Find("Reward card").gameObject.activeSelf,Is.True); Assert.That(cell.Root.Find("Reward card/Reward text").GetComponent<TMP_Text>().text,Is.EqualTo("100"));
        }
        [Test] public void QuestRowShowsTitleProgressAndRewardAmountOnce()
        {
            f.Style.ListRowColor=new Color(1,1,1,.4f);
            var v=new QuestsView(f,screen); var p=new QuestsPresenter(session,v,text,clock); owned.Add(p);
            var row=RowLabels(v.Root); Assert.That(row.Select(t=>t.text).ToArray(),Is.EqualTo(new[]{"Quest","0/2","7"}));
            AssertRowReadable(v.Root);
        }
        [Test] public void ShopRowShowsTitleAndRewardAmountOnly()
        {
            f.Style.ListRowColor=new Color(1,1,1,.4f);
            var v=new ShopView(f,screen,root.AddComponent<PopupHost>());
            var p=new ShopPresenter(session,new ShopService(ScreenTestData.Catalog(),session,new FakePurchaseService()),v,text); owned.Add(p);
            var labels=RowLabels(v.Root); Assert.That(labels.Length,Is.EqualTo(2)); Assert.That(labels[0].text,Does.Not.Contain("10")); Assert.That(labels[1].text,Is.EqualTo("10"));
            AssertRowReadable(v.Root);
        }
        static TMP_Text[] RowLabels(RectTransform view)
        {
            var row=view.GetComponentsInChildren<Image>(true).First(i=>i.name=="List Row");
            return row.GetComponentsInChildren<TMP_Text>(true).Where(t=>t.GetComponentInParent<Button>(true)==null).ToArray();
        }
        static void AssertRowReadable(RectTransform view)
        {
            var row=view.GetComponentsInChildren<Image>(true).First(i=>i.name=="List Row");
            Assert.That(row.color.a,Is.GreaterThanOrEqualTo(MinRowOpacity));
            foreach(var t in RowLabels(view)) Assert.That(EffectiveFont(t),Is.GreaterThanOrEqualTo(MinRowFont),t.text);
        }
        [Test] public void LeaderboardEmptyStateIsACardInTheLowerMeadow()
        {
            var v=new LeaderboardView(f,screen);
            v.Render(new LeaderboardSnapshot{State=LeaderboardState.Empty,MessageKey="shell.leaderboard.empty"},new ScreenText(session,text));
            var card=(RectTransform)v.Root.Find("Empty card"); Assert.That(card.gameObject.activeInHierarchy,Is.True);
            var rect=Normalized(card); Assert.That(Overlaps(rect,Bear),Is.False); Assert.That(rect.yMin,Is.GreaterThanOrEqualTo(160/ScreenH));
            Assert.That(card.GetComponentInChildren<Image>().color.a,Is.GreaterThanOrEqualTo(MinRowOpacity)); Assert.That(EffectiveFont(card.GetComponentInChildren<TMP_Text>()),Is.GreaterThanOrEqualTo(MinRowFont));
            var refresh=Normalized((RectTransform)v.Root.GetComponentsInChildren<Button>(true).Single().transform); Assert.That(Overlaps(refresh,rect),Is.False);
        }
    }
}
