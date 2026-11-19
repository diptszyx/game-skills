using System.Linq;
using IndieGame.Shell.Unity.Screens;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
namespace IndieGame.Shell.Tests
{
    public sealed class RewardDisplayTests
    {
        const float Seconds=.6f, Arc=.2f;
        DemoStyles demos; GameObject root; ShellUiFactory factory; ScreenClock clock; ScreenFiles files; ShellSession session; LocalizationService text;
        readonly System.Collections.Generic.List<System.IDisposable> owned=new System.Collections.Generic.List<System.IDisposable>();
        [SetUp] public void Setup()
        {
            demos=new DemoStyles(); factory=new ShellUiFactory(demos.Make(false)); root=new GameObject("UI",typeof(RectTransform),typeof(Canvas));
            clock=new ScreenClock(); files=new ScreenFiles(); session=ScreenTestData.Session(clock,files); session.LoadAll(); text=ScreenTestData.Text();
        }
        [TearDown] public void Cleanup() { foreach(var p in owned) p.Dispose(); owned.Clear(); Object.DestroyImmediate(root); demos.Dispose(); }
        static bool HasIcon(Component c,string role)=>c.GetComponentsInChildren<ShellAppearance>(true).Any(a=>a.Role==role);
        RewardFlyLayer FlyLayerWithGoldTarget()
        {
            var layer=RewardFlyLayer.Create(factory,root.transform);
            var target=ShellUiFactory.Node("Target",root.transform); target.gameObject.SetActive(true);
            layer.SetTarget(RewardPart.GoldIcon,target); return layer;
        }
        RectTransform Source() { var r=ShellUiFactory.Node("Source",root.transform); r.gameObject.SetActive(true); return r; }

        [Test] public void FlyPositionStartsAtTheSourceAndLandsOnTheTarget()
        {
            var from=new Vector2(10,20); var to=new Vector2(300,900);
            Assert.That(RewardFlyCurve.Position(from,to,Arc,0),Is.EqualTo(from));
            Assert.That((RewardFlyCurve.Position(from,to,Arc,1)-to).magnitude,Is.LessThan(1e-3f));
        }
        [Test] public void FlyPathBowsUpwardBetweenTheEnds()
        {
            var from=new Vector2(0,0); var to=new Vector2(400,0);
            Assert.That(RewardFlyCurve.Position(from,to,Arc,.5f).y,Is.GreaterThan(0));
            Assert.That(RewardFlyCurve.Position(from,to,0,.5f).y,Is.EqualTo(0).Within(1e-6f));
        }
        [Test] public void FlyProgressWaitsForItsDelayThenClamps()
        {
            Assert.That(RewardFlyCurve.Progress(.05f,.1f,Seconds),Is.EqualTo(0));
            Assert.That(RewardFlyCurve.Progress(.1f+Seconds/2,.1f,Seconds),Is.EqualTo(.5f).Within(1e-6f));
            Assert.That(RewardFlyCurve.Progress(10,.1f,Seconds),Is.EqualTo(1));
        }
        [Test] public void FlyIconIsHiddenBeforeLaunchAndFadesOnLanding()
        {
            Assert.That(RewardFlyCurve.Alpha(0),Is.EqualTo(0));
            Assert.That(RewardFlyCurve.Alpha(.5f),Is.EqualTo(1));
            Assert.That(RewardFlyCurve.Alpha(1),Is.EqualTo(0).Within(1e-6f));
            Assert.That(RewardFlyCurve.Scale(1),Is.EqualTo(RewardFlyCurve.LandingScale).Within(1e-6f));
        }
        [Test] public void FlyDurationCoversTheLastStaggeredIcon()
        {
            Assert.That(RewardFlyCurve.Duration(6,.06f,Seconds),Is.EqualTo(RewardFlyCurve.LaunchDelay+5*.06f+Seconds).Within(1e-6f));
        }
        [Test] public void StyleRejectsInvalidRewardFlyValues()
        {
            foreach(var apply in new System.Action<ShellStyle>[]{s=>s.RewardFlyCount=0,s=>s.RewardFlySeconds=0,s=>s.RewardIconSize=0,s=>s.RewardFlyArc=-1})
            {
                var style=demos.Make(false); apply(style);
                Assert.Throws<System.InvalidOperationException>(()=>style.Validate());
            }
        }

        [Test] public void RewardPartsUseIconsForGoldAndHeartsWithoutAPlusSign()
        {
            var parts=new ScreenText(session,text).RewardParts(new Reward(10,2));
            Assert.That(parts.Select(p=>p.Icon),Is.EqualTo(new[]{RewardPart.GoldIcon,RewardPart.HeartIcon}));
            Assert.That(parts.Select(p=>p.Text),Is.EqualTo(new[]{"10","2"}));
        }
        [Test] public void RewardPartsKeepBoostersAsPlainText()
        {
            var booster=new Reward(boosters:new System.Collections.Generic.Dictionary<string,int>{{"hint",1}});
            var parts=new ScreenText(session,text).RewardParts(booster);
            Assert.That(parts.Single().Icon,Is.Null); Assert.That(parts.Single().Text,Is.Not.Empty);
        }
        [Test] public void RewardStripShowsOneIconAndAmountPerPiece()
        {
            var strip=factory.RewardStrip(root.transform,new ScreenText(session,text).RewardParts(new Reward(10,2)),"rowText");
            Assert.That(HasIcon(strip,RewardPart.GoldIcon),Is.True); Assert.That(HasIcon(strip,RewardPart.HeartIcon),Is.True);
            Assert.That(strip.GetComponentsInChildren<TMP_Text>(true).Select(t=>t.text),Is.EqualTo(new[]{"10","2"}));
        }

        [Test] public void QuestRowShowsTheRewardAsCoinIconNotBareText()
        {
            var view=new QuestsView(factory,root.transform); var presenter=new QuestsPresenter(session,view,text,clock); owned.Add(presenter);
            var row=view.Snapshot.Rows[0];
            Assert.That(row.Detail,Is.EqualTo("0/2")); Assert.That(row.Rewards.Single().Icon,Is.EqualTo(RewardPart.GoldIcon));
            var detail=view.Root.GetComponentsInChildren<RectTransform>(true).First(r=>r.name=="Detail");
            Assert.That(HasIcon(detail,RewardPart.GoldIcon),Is.True);
            Assert.That(detail.GetComponentsInChildren<TMP_Text>(true).Select(t=>t.text),Is.EqualTo(new[]{"0/2","7"}));
        }
        [Test] public void ShopRowShowsTheRewardAsIconToo()
        {
            var host=root.AddComponent<PopupHost>(); var view=new ShopView(factory,root.transform,host);
            var presenter=new ShopPresenter(session,new ShopService(ScreenTestData.Catalog(),session,new FakePurchaseService()),view,text); owned.Add(presenter);
            Assert.That(view.Snapshot.Rows[0].Detail,Is.Null); Assert.That(view.Snapshot.Rows[0].Rewards.Single().Icon,Is.EqualTo(RewardPart.GoldIcon));
        }
        [Test] public void WinPopupShowsTheRewardAsCoinIconAndAmount()
        {
            using(var f=new PopupFixture())
            {
                f.Presenter.ShowWin("r1"); var strip=f.Presenter.Popups[SessionPopupKind.Win].RewardStrip;
                Assert.That(HasIcon(strip,RewardPart.GoldIcon),Is.True);
                Assert.That(strip.GetComponentsInChildren<TMP_Text>(true).Select(t=>t.text),Is.EqualTo(new[]{"12"}));
            }
        }

        [Test] public void FlyCreatesOneIconPerLaunchForEachRewardPieceWithATarget()
        {
            var layer=FlyLayerWithGoldTarget();
            layer.Fly(Source(),new ScreenText(session,text).RewardParts(new Reward(10,2)));
            Assert.That(layer.ActiveFlights,Is.EqualTo(factory.Style.RewardFlyCount),"hearts have no target so only gold flies");
        }
        [Test] public void FlyIgnoresAMissingSource()
        {
            var layer=FlyLayerWithGoldTarget(); layer.Fly(null,new[]{RewardPart.Gold(5)}); Assert.That(layer.ActiveFlights,Is.EqualTo(0));
        }
        [Test] public void QuestClaimLaunchesTheGoldFly()
        {
            var layer=FlyLayerWithGoldTarget(); var view=new QuestsView(factory,root.transform); var presenter=new QuestsPresenter(session,view,text,clock,layer); owned.Add(presenter);
            session.DailyQuests.RecordEvent("event",2); presenter.Refresh();
            view.RequestClaim("q"); Assert.That(layer.ActiveFlights,Is.EqualTo(factory.Style.RewardFlyCount));
        }
        [Test] public void FailedQuestClaimLaunchesNoFly()
        {
            var layer=FlyLayerWithGoldTarget(); var view=new QuestsView(factory,root.transform); var presenter=new QuestsPresenter(session,view,text,clock,layer); owned.Add(presenter);
            view.RequestClaim("q"); Assert.That(layer.ActiveFlights,Is.EqualTo(0));
        }
        [Test] public void CheckInClaimLaunchesTheGoldFly()
        {
            var layer=FlyLayerWithGoldTarget(); var view=new CheckInView(factory,root.transform); var presenter=new CheckInPresenter(session,view,text,clock,layer); owned.Add(presenter);
            view.RequestClaim(); Assert.That(layer.ActiveFlights,Is.EqualTo(factory.Style.RewardFlyCount));
        }
        [Test] public void FlyHoldsBackTheRewardUntilItsIconsLand()
        {
            var layer=FlyLayerWithGoldTarget(); layer.Fly(Source(),new[]{RewardPart.Gold(100)});
            Assert.That(layer.Held(RewardPart.GoldIcon),Is.EqualTo(100));
            Object.DestroyImmediate(root.transform.Find("Target").gameObject);
            typeof(RewardFlyLayer).GetMethod("Update",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(layer,null);
            Assert.That(layer.Held(RewardPart.GoldIcon),Is.Zero);
        }
        [Test] public void TopBarPillsFadeWithoutBeingDeactivatedSoRewardsStillHaveATarget()
        {
            var top=new TopBarView(factory,root.transform); top.ShowPills(false,false);
            var gold=top.Root.Find("Currency pill"); var lives=top.Root.Find("Lives pill");
            Assert.That(gold.GetComponent<CanvasGroup>().alpha,Is.Zero); Assert.That(lives.GetComponent<CanvasGroup>().alpha,Is.Zero);
            Assert.That(top.GoldIcon.gameObject.activeInHierarchy,Is.True);
            top.ShowPills(true,false);
            Assert.That(gold.GetComponent<CanvasGroup>().alpha,Is.EqualTo(1f)); Assert.That(lives.GetComponent<CanvasGroup>().alpha,Is.Zero);
        }
    }
}
