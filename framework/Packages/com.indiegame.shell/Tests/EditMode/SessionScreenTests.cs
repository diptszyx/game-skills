using System.Collections.Generic;
using System.Threading.Tasks;
using IndieGame.Shell.Unity.Screens;
using NUnit.Framework;
namespace IndieGame.Shell.Tests
{
    public sealed class SessionScreenTests
    {
        [Test] public void RewardedAdCallbackIsInvokedExactlyOnce()
        {
            var ads=new FakeRewardedAdService(); int calls=0; int thread=System.Threading.Thread.CurrentThread.ManagedThreadId;
            ads.Enqueue(AdOutcome.Completed);
            ads.Show("revive",result=>{ calls++; Assert.That(System.Threading.Thread.CurrentThread.ManagedThreadId,Is.EqualTo(thread)); Assert.That(result.Success,Is.True); });
            ads.CompletePending(); Assert.That(calls,Is.EqualTo(1));
            ads.DeferCompletion=true;
            ads.Show("double",result=>{ calls++; Assert.That(System.Threading.Thread.CurrentThread.ManagedThreadId,Is.EqualTo(thread)); Assert.That(result.Outcome,Is.EqualTo(AdOutcome.NotReady)); });
            Assert.That(calls,Is.EqualTo(1)); ads.CompletePending(); ads.CompletePending(); Assert.That(calls,Is.EqualTo(2));
            foreach(var placement in new string[]{null,"","  "})
            {
                int invalidCalls=0;
                ads.Show(placement,result=>{ invalidCalls++; Assert.That(result.Outcome,Is.EqualTo(AdOutcome.Failed)); Assert.That(result.Error,Is.EqualTo(ServiceError.InvalidArgument)); });
                ads.CompletePending(); ads.CompletePending(); Assert.That(invalidCalls,Is.EqualTo(1));
            }
        }
        [Test] public void DeferredAdCompletesRevivePopupLater()
        {
            using(var f=new PopupFixture())
            {
                f.Ads.DeferCompletion=true; f.Ads.Enqueue(AdOutcome.Completed); f.Presenter.ShowRevive();
                var p=f.Presenter.Popups[SessionPopupKind.Revive]; f.Presenter.Act(SessionPopupKind.Revive,"ad");
                Assert.That(f.Game.Revives,Is.Zero); Assert.That(f.Files.Writes,Is.Zero);
                Assert.That(p.Buttons["ad"].interactable,Is.False); Assert.That(p.Buttons["gold"].interactable,Is.False);
                f.Ads.CompletePending(); Assert.That(f.Game.Revives,Is.EqualTo(1)); Assert.That(f.Files.Writes,Is.EqualTo(1));
            }
        }
        [Test] public void SecondTapWhileAdPendingDoesNotShowAgain()
        {
            foreach(var kind in new[]{SessionPopupKind.Revive,SessionPopupKind.Win}) using(var f=new PopupFixture())
            {
                var ads=new RepeatingAd();
                using(var presenter=new SessionPopupPresenter(f.Session,f.Factory,f.Host,ScreenTestData.Text(),f.Game,ads,new SessionPopupConfig(new Reward(12),20,"revive","double")))
                {
                    if(kind==SessionPopupKind.Win) presenter.ShowWin("r"); else presenter.ShowRevive();
                    string action=kind==SessionPopupKind.Win?"double":"ad";
                    presenter.Act(kind,action); presenter.Act(kind,action); presenter.Act(kind,kind==SessionPopupKind.Win?"next":"gold");
                    Assert.That(ads.Shows,Is.EqualTo(1)); Assert.That(f.Game.Revives+f.Game.Nexts,Is.Zero);
                    foreach(var button in presenter.Popups[kind].Buttons.Values) Assert.That(button.interactable,Is.False);
                }
            }
        }
        [Test] public void AdCompletingAfterPopupClosedChangesNothing()
        {
            foreach(var kind in new[]{SessionPopupKind.Revive,SessionPopupKind.Win})
            foreach(var exit in new[]{"close","destroy","dispose","reopen"}) using(var f=new PopupFixture())
            {
                f.Ads.DeferCompletion=true; f.Ads.Enqueue(AdOutcome.Completed);
                if(kind==SessionPopupKind.Win) f.Presenter.ShowWin("r"); else f.Presenter.ShowRevive();
                var p=f.Presenter.Popups[kind]; f.Presenter.Act(kind,kind==SessionPopupKind.Win?"double":"ad");
                long gold=f.Session.Economy.Gold; int writes=f.Files.Writes;
                if(exit=="close") f.Host.Close(p.View);
                else if(exit=="destroy") UnityEngine.Object.DestroyImmediate(p.View.gameObject);
                else if(exit=="dispose") f.Presenter.Dispose();
                else { p.View.gameObject.SetActive(false); if(kind==SessionPopupKind.Win) f.Presenter.ShowWin("r"); else f.Presenter.ShowRevive(); writes=f.Files.Writes; }
                Assert.DoesNotThrow(()=>f.Ads.CompletePending());
                Assert.That(f.Game.Revives,Is.Zero); Assert.That(f.Session.Economy.Gold,Is.EqualTo(gold)); Assert.That(f.Files.Writes,Is.EqualTo(writes));
            }
        }
        [Test] public void FailedAdKeepsPopupOpenAndReEnablesButtons()
        {
            foreach(var kind in new[]{SessionPopupKind.Revive,SessionPopupKind.Win}) using(var f=new PopupFixture())
            {
                f.Ads.DeferCompletion=true; f.Ads.Enqueue(AdOutcome.Failed);
                if(kind==SessionPopupKind.Win) f.Presenter.ShowWin("r"); else f.Presenter.ShowRevive();
                var p=f.Presenter.Popups[kind]; f.Presenter.Act(kind,kind==SessionPopupKind.Win?"double":"ad"); f.Ads.CompletePending();
                Assert.That(f.Host.Top,Is.SameAs(p.View)); Assert.That(p.Error,Is.Not.Null.And.Not.Empty);
                foreach(var button in p.Buttons.Values) Assert.That(button.interactable,Is.True);
                f.Ads.Enqueue(AdOutcome.Completed); f.Presenter.Act(kind,kind==SessionPopupKind.Win?"double":"ad"); f.Ads.CompletePending();
                Assert.That(kind==SessionPopupKind.Win?f.Session.Economy.Gold:f.Game.Revives,Is.EqualTo(kind==SessionPopupKind.Win?124:1));
            }
        }
        sealed class RepeatingAd : IRewardedAdService
        {
            public int Shows; public System.Action<AdResult> Callback;
            public void Show(string placementId,System.Action<AdResult> onDone) { Shows++; Callback=onDone; }
        }
        [Test] public void DoubleRewardGrantedOnlyOnceAcrossRepeatedCompletion()
        {
            using(var f=new PopupFixture())
            {
                var ads=new RepeatingAd();
                using(var presenter=new SessionPopupPresenter(f.Session,f.Factory,f.Host,ScreenTestData.Text(),f.Game,ads,new SessionPopupConfig(new Reward(12),20,"revive","double")))
                {
                    presenter.ShowWin("r"); presenter.Act(SessionPopupKind.Win,"double");
                    ads.Callback(new AdResult(AdOutcome.Completed)); int writes=f.Files.Writes;
                    ads.Callback(new AdResult(AdOutcome.Completed)); presenter.Act(SessionPopupKind.Win,"double");
                    Assert.That(f.Session.Economy.Gold,Is.EqualTo(124)); Assert.That(f.Files.Writes,Is.EqualTo(writes)); Assert.That(ads.Shows,Is.EqualTo(1));
                    Assert.That(presenter.Popups[SessionPopupKind.Win].Buttons["next"].interactable,Is.True);
                    Assert.That(presenter.Popups[SessionPopupKind.Win].Buttons["double"].interactable,Is.False);
                }
            }
        }
        [Test] public void LeaderboardRendersEmptyErrorAndRank()
        {
            using(var f=new PopupFixture())
            {
                var service=new FakeLeaderboardService("me",new FakeLeaderboardState());
                var view=new LeaderboardView(f.Factory,f.Root.transform);
                using(var p=new LeaderboardPresenter(f.Session,view,ScreenTestData.Text(),service,"board",3))
                {
                    Assert.That(view.Snapshot.State,Is.EqualTo(LeaderboardState.Empty));
                    service.Enqueue(ServiceError.Unavailable); p.Refresh(); Assert.That(view.Snapshot.State,Is.EqualTo(LeaderboardState.Error));
                    Assert.That(view.Snapshot.MessageKey,Is.EqualTo("shell.error.Unavailable"));
                    service.SubmitScore("board",42); p.Refresh(); Assert.That(view.Snapshot.State,Is.EqualTo(LeaderboardState.Populated));
                    Assert.That(view.Snapshot.Entries.Count,Is.EqualTo(1)); Assert.That(view.Snapshot.MyRank.Rank,Is.EqualTo(1));
                }
            }
        }
        sealed class Delayed : ILeaderboardRequests
        {
            public readonly Queue<TaskCompletionSource<ServiceResult<IReadOnlyList<LeaderboardEntry>>>> Top=new Queue<TaskCompletionSource<ServiceResult<IReadOnlyList<LeaderboardEntry>>>>();
            public Task<ServiceResult<IReadOnlyList<LeaderboardEntry>>> GetTop(string board,int count) { var t=new TaskCompletionSource<ServiceResult<IReadOnlyList<LeaderboardEntry>>>(); Top.Enqueue(t); return t.Task; }
            public Task<ServiceResult<LeaderboardEntry>> GetMyRank(string board)=>Task.FromResult(new ServiceResult<LeaderboardEntry>(null,ServiceError.NotFound));
        }
        [Test] public async Task LeaderboardIgnoresStaleResponse()
        {
            using(var f=new PopupFixture())
            {
                var requests=new Delayed(); var view=new LeaderboardView(f.Factory,f.Root.transform);
                using(var p=new LeaderboardPresenter(f.Session,view,ScreenTestData.Text(),requests,"board",3))
                {
                    var oldTask=p.Pending; var old=requests.Top.Dequeue(); var current=p.RefreshAsync(); var latest=requests.Top.Dequeue();
                    latest.SetResult(new ServiceResult<IReadOnlyList<LeaderboardEntry>>(new[]{new LeaderboardEntry("new",20,1)})); await current;
                    old.SetResult(new ServiceResult<IReadOnlyList<LeaderboardEntry>>(new[]{new LeaderboardEntry("old",10,1)})); await oldTask;
                    Assert.That(view.Snapshot.Entries[0].PlayerId,Is.EqualTo("new"));
                }
            }
        }
        [Test] public void ReviveGrantsOnlyOnCompletedAd()
        {
            foreach(var outcome in new[]{AdOutcome.Failed,AdOutcome.NotReady,AdOutcome.Completed}) using(var f=new PopupFixture())
            {
                f.Presenter.ShowRevive(); f.Ads.Enqueue(outcome); f.Presenter.Act(SessionPopupKind.Revive,"ad");
                Assert.That(f.Game.Revives,Is.EqualTo(outcome==AdOutcome.Completed?1:0));
                Assert.That(f.Session.Economy.Gold,Is.EqualTo(100));
                Assert.That(f.Files.Writes,Is.EqualTo(outcome==AdOutcome.Completed?1:0));
            }
        }
        [Test] public void ReviveAdSkippedLeavesStateUnchanged()
        {
            using(var f=new PopupFixture())
            {
                f.Presenter.ShowRevive(); f.Ads.Enqueue(AdOutcome.Skipped); f.Presenter.Act(SessionPopupKind.Revive,"ad");
                Assert.That(f.Game.Revives,Is.Zero); Assert.That(f.Session.Economy.Gold,Is.EqualTo(100)); Assert.That(f.Files.Writes,Is.Zero);
                Assert.That(f.Host.Top,Is.SameAs(f.Presenter.Popups[SessionPopupKind.Revive].View));
            }
        }
        [Test] public void DoubleClickExecutesOnce()
        {
            using(var f=new PopupFixture())
            {
                f.Presenter.ShowRevive(); f.Presenter.Act(SessionPopupKind.Revive,"gold"); f.Presenter.Act(SessionPopupKind.Revive,"gold");
                Assert.That(f.Game.Revives,Is.EqualTo(1)); Assert.That(f.Session.Economy.Gold,Is.EqualTo(80));
            }
            using(var f=new PopupFixture())
            {
                f.Presenter.ShowWin("r"); f.Ads.Enqueue(AdOutcome.Completed); f.Ads.Enqueue(AdOutcome.Completed);
                f.Presenter.Act(SessionPopupKind.Win,"double"); f.Presenter.Act(SessionPopupKind.Win,"double");
                Assert.That(f.Session.Economy.Gold,Is.EqualTo(124));
                f.Presenter.Act(SessionPopupKind.Win,"next"); f.Presenter.Act(SessionPopupKind.Win,"next"); Assert.That(f.Game.Nexts,Is.EqualTo(1));
            }
        }
        [Test] public void SettingsToggleCommitsAndSurfacesError()
        {
            using(var f=new PopupFixture())
            {
                f.Presenter.ShowPause(); var writes=f.Files.Writes; f.Presenter.Act(SessionPopupKind.Pause,"sound");
                Assert.That(f.Session.Settings.Sound,Is.False); Assert.That(f.Files.Writes,Is.GreaterThan(writes));
                f.Files.Fail=true; f.Presenter.Act(SessionPopupKind.Pause,"music");
                Assert.That(f.Session.Settings.Music,Is.False); Assert.That(f.Presenter.Popups[SessionPopupKind.Pause].Error,Does.Contain("Save failed"));
                f.Files.Fail=false; Assert.That(f.Presenter.RetrySave(SessionPopupKind.Pause),Is.True);
                f.Session.Settings.SetSound(true); f.Session.LoadAll(); Assert.That(f.Session.Settings.Sound,Is.False);
            }
            using(var f=new PopupFixture())
            {
                f.Presenter.ShowRevive(); f.Files.Fail=true; f.Presenter.Act(SessionPopupKind.Revive,"gold");
                Assert.That(f.Presenter.Popups[SessionPopupKind.Revive].Error,Does.Contain("Save failed"));
                Assert.That(f.Host.Top.Group.interactable,Is.True);
                f.Presenter.Act(SessionPopupKind.Revive,"gold"); Assert.That(f.Game.Revives,Is.EqualTo(1));
                f.Files.Fail=false; Assert.That(f.Presenter.RetrySave(SessionPopupKind.Revive),Is.True);
                Assert.That(f.Session.Economy.Gold,Is.EqualTo(80));
            }
        }
    }
}
