using System;
using System.Linq;
using IndieGame.Shell.Unity.Screens;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using UnityEngine;
namespace IndieGame.Shell.Tests
{
    public sealed class DataScreenTests
    {
        DemoStyles demos; GameObject root; ShellUiFactory f; ScreenClock clock; ScreenFiles files; ShellSession session; LocalizationService text;
        readonly System.Collections.Generic.List<IDisposable> owned=new System.Collections.Generic.List<IDisposable>();
        [SetUp] public void Setup() { demos=new DemoStyles(); f=new ShellUiFactory(demos.Make(false)); root=new GameObject("UI",typeof(RectTransform),typeof(Canvas)); clock=new ScreenClock(); files=new ScreenFiles(); session=ScreenTestData.Session(clock,files); session.LoadAll(); text=ScreenTestData.Text(); }
        [TearDown] public void Cleanup() { foreach(var p in owned) p.Dispose(); owned.Clear(); UnityEngine.Object.DestroyImmediate(root); demos.Dispose(); }
        CheckInView CheckIn(out CheckInPresenter p) { var v=new CheckInView(f,root.transform); p=new CheckInPresenter(session,v,text,clock); owned.Add(p); return v; }
        [Test] public void CheckInSnapshotMatchesClaim()
        {
            var v=CheckIn(out var p); var preview=session.DailyCheckIn.Preview(); Assert.That(v.Snapshot.Rows.Count,Is.EqualTo(5));
            Assert.That(v.Snapshot.Rows[preview.NextSlot].Text,Does.Contain(preview.Reward.Gold.ToString()));
            Assert.That(v.Snapshot.Rows.Count(r=>r.Enabled),Is.EqualTo(1)); v.RequestClaim();
            Assert.That(session.Economy.Gold,Is.EqualTo(100+preview.Reward.Gold)); Assert.That(v.Snapshot.Rows.Any(r=>r.Enabled),Is.False);
        }
        [Test] public void MidnightRefreshesOpenCheckInView()
        {
            var v=CheckIn(out var p); v.RequestClaim(); Assert.That(v.Snapshot.Rows.Any(r=>r.Enabled),Is.False);
            clock.UtcNow=clock.UtcNow.AddSeconds(1); p.Tick(); Assert.That(v.Snapshot.Rows[1].Enabled,Is.True);
        }
        [Test] public void DuplicateClaimDoesNotGrantAgain()
        { var v=CheckIn(out var p); v.RequestClaim(); v.RequestClaim(); Assert.That(session.Economy.Gold,Is.EqualTo(125)); Assert.That(files.Writes,Is.EqualTo(1)); }
        [Test] public void ClaimAndWalletPersistTogether()
        {
            var v=CheckIn(out var p); v.RequestClaim(); var restored=ScreenTestData.Session(clock,files); Assert.That(restored.LoadAll().Success,Is.True);
            Assert.That(restored.Economy.Gold,Is.EqualTo(125)); Assert.That(restored.DailyCheckIn.Preview().CanClaimNow,Is.False); Assert.That(restored.Lives.Hearts,Is.EqualTo(2));
        }
        [Test] public void QuestClaimOnlyWhenComplete()
        {
            var v=new QuestsView(f,root.transform); var p=new QuestsPresenter(session,v,text,clock); owned.Add(p);
            v.RequestClaim("q"); Assert.That(session.Economy.Gold,Is.EqualTo(100)); session.DailyQuests.RecordEvent("event",1); p.Refresh(); Assert.That(v.Snapshot.Rows[0].Progress,Is.EqualTo(.5f));
            v.RequestClaim("q"); Assert.That(session.Economy.Gold,Is.EqualTo(100)); session.DailyQuests.RecordEvent("event",1); p.Refresh(); v.RequestClaim("q"); v.RequestClaim("q");
            Assert.That(session.Economy.Gold,Is.EqualTo(107)); Assert.That(v.Snapshot.Rows[0].Enabled,Is.False);
            var empty=new QuestsView(f,root.transform); var ep=new QuestsPresenter(ScreenTestData.Session(clock,files,true),empty,text,clock); owned.Add(ep); Assert.That(empty.Snapshot.Rows,Is.Empty); Assert.That(empty.Snapshot.Empty,Is.EqualTo("No quests"));
        }
        sealed class Game : IGameSessionActions { public int Plays; public void Play()=>Plays++; }
        [Test] public void MainRefreshesLivesOnResume()
        {
            var v=new MainView(f,root.transform); var game=new Game(); ShellScreen? opened=null; var p=new MainPresenter(session,v,text,clock,game,s=>opened=s); owned.Add(p);
            Assert.That(v.Snapshot.Hearts,Is.EqualTo("Hearts 1")); Assert.That(v.Snapshot.Countdown,Is.EqualTo("Full in 120s"));
            clock.UtcNow=clock.UtcNow.AddSeconds(60); p.Resume(); Assert.That(v.Snapshot.Hearts,Is.EqualTo("Hearts 3")); Assert.That(v.Snapshot.Countdown,Is.EqualTo("Full in 60s"));
            v.RequestPlay(); v.RequestOpen(ShellScreen.Shop); Assert.That(game.Plays,Is.EqualTo(1)); Assert.That(opened,Is.EqualTo(ShellScreen.Shop));
        }
        [Test] public void SaveFailureIsSurfacedToView()
        {
            session.Commit(); var v=CheckIn(out var p); files.Fail=true; v.RequestClaim(); Assert.That(v.Error,Is.EqualTo("Save failed: Storage error "));
            Assert.That(session.Economy.Gold,Is.EqualTo(125)); var restored=ScreenTestData.Session(clock,files); restored.LoadAll(); Assert.That(restored.Economy.Gold,Is.EqualTo(100)); Assert.That(restored.DailyCheckIn.Preview().CanClaimNow,Is.True);
            files.Fail=false; p.RetrySave(); restored.LoadAll(); Assert.That(restored.Economy.Gold,Is.EqualTo(125)); Assert.That(restored.DailyCheckIn.Preview().CanClaimNow,Is.False);
        }
        [Test] public void ShopFailedPurchaseShowsErrorPopup()
        {
            var host=root.AddComponent<PopupHost>(); var v=new ShopView(f,root.transform,host); var fake=new FakePurchaseService(); fake.Enqueue(PurchaseOutcome.Failed);
            var p=new ShopPresenter(session,new ShopService(ScreenTestData.Catalog(),session,fake),v,text); owned.Add(p); v.RequestBuy("p");
            Assert.That(v.Error,Is.EqualTo("Purchase failed")); Assert.That(host.Top,Is.Not.Null); Assert.That(host.Top.gameObject.activeSelf,Is.True); Assert.That(session.Economy.Gold,Is.EqualTo(100));
        }
    }
}
