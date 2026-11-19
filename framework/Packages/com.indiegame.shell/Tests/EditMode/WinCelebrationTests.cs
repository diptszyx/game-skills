using IndieGame.Shell.Unity.Screens;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using UnityEngine;
namespace IndieGame.Shell.Tests
{
    public sealed class WinCelebrationTests
    {
        sealed class Recorder : IResultCelebration
        {
            public int Plays,Stops; public RectTransform Stage;
            public void Play(RectTransform stage) { Plays++; Stage=stage; }
            public void Stop() { Stops++; }
        }
        PopupFixture fixture; SessionPopupPresenter presenter; Recorder recorder,loseRecorder;
        [SetUp] public void Setup()
        {
            fixture=new PopupFixture(); recorder=new Recorder(); loseRecorder=new Recorder();
            presenter=new SessionPopupPresenter(fixture.Session,fixture.Factory,fixture.Host,ScreenTestData.Text(),fixture.Game,fixture.Ads,new SessionPopupConfig(new Reward(12),20,"revive","double"),null,recorder,loseRecorder);
        }
        [TearDown] public void Cleanup() { presenter.Dispose(); fixture.Dispose(); }
        ResultCelebrationDriver Show(string round="r1") { presenter.ShowWin(round); return presenter.Popups[SessionPopupKind.Win].Celebration; }
        ResultCelebrationDriver ShowLose() { presenter.ShowLose(); return presenter.Popups[SessionPopupKind.Lose].Celebration; }
        static float Opened(ResultCelebrationDriver d)=>d.GetComponent<PopupView>().Motion.OpenSeconds;

        [Test] public void ProviderIsNotPlayedBeforeThePopupFinishesOpening()
        {
            var driver=Show(); driver.Evaluate(Opened(driver)*.5f,false);
            Assert.That(recorder.Plays,Is.EqualTo(0));
        }
        [Test] public void ProviderPlaysOnceWithAStageWhenOpeningFinishes()
        {
            var driver=Show(); driver.Evaluate(Opened(driver),false); driver.Evaluate(Opened(driver)+1,false);
            Assert.That(recorder.Plays,Is.EqualTo(1)); Assert.That(recorder.Stage,Is.SameAs(driver.Stage));
        }
        [Test] public void RepeatedShowWinDoesNotPlayTwice()
        {
            var driver=Show(); driver.Evaluate(Opened(driver),false);
            Assert.That(Show(),Is.SameAs(driver)); driver.Evaluate(Opened(driver)+1,false);
            Assert.That(recorder.Plays,Is.EqualTo(1));
        }
        [Test] public void ProviderStopsWhenThePopupStartsClosing()
        {
            var driver=Show(); driver.Evaluate(Opened(driver),false); driver.Evaluate(0,true); driver.Evaluate(0,true);
            Assert.That(recorder.Stops,Is.EqualTo(1)); Assert.That(driver.Confetti.Running,Is.False);
        }
        [Test] public void ProviderStopsWhenThePresenterIsDisposed()
        {
            var driver=Show(); driver.Evaluate(Opened(driver),false); presenter.Dispose();
            Assert.That(recorder.Stops,Is.EqualTo(1));
        }
        [Test] public void NextLevelStopsTheCelebration()
        {
            var driver=Show(); driver.Evaluate(Opened(driver),false); presenter.Act(SessionPopupKind.Win,"next");
            Assert.That(recorder.Stops,Is.EqualTo(1)); Assert.That(fixture.Game.Nexts,Is.EqualTo(1));
        }
        [Test] public void DoubleActionStopsTheCelebration()
        {
            fixture.Ads.Enqueue(AdOutcome.Completed);
            var driver=Show(); driver.Evaluate(Opened(driver),false); presenter.Act(SessionPopupKind.Win,"double");
            Assert.That(recorder.Stops,Is.EqualTo(1));
        }
        [Test] public void StoppedBeforeOpeningFinishesNeverPlays()
        {
            var driver=Show(); presenter.Act(SessionPopupKind.Win,"next"); driver.Evaluate(Opened(driver)+1,false);
            Assert.That(recorder.Plays,Is.EqualTo(0)); Assert.That(recorder.Stops,Is.EqualTo(0));
        }
        [Test] public void DefaultConfettiIsCreatedAndRunsEvenWithoutAProvider()
        {
            fixture.Presenter.ShowWin("r2"); var driver=fixture.Presenter.Popups[SessionPopupKind.Win].Celebration;
            Assert.That(driver.Confetti.PieceCount,Is.EqualTo(ConfettiConfig.Default.Count));
            Assert.DoesNotThrow(()=>driver.Evaluate(Opened(driver),false));
            Assert.That(driver.Confetti.Running,Is.True);
        }
        [Test] public void ConfettiIsLayeredAbovePanelAndStageSitsAbovePanelTop()
        {
            var driver=Show(); var view=driver.GetComponent<PopupView>();
            Assert.That(driver.Confetti.transform.GetSiblingIndex(),Is.GreaterThan(view.Panel.GetSiblingIndex()));
            Assert.That(driver.Stage.parent,Is.SameAs(view.Panel)); Assert.That(driver.Stage.anchorMin.y,Is.EqualTo(1));
        }
        [Test] public void ConfettiStepsToTheEndOfItsLifetimeAndStops()
        {
            var sim=new ConfettiSimulation(ConfettiConfig.Default,7); sim.Start(Vector2.zero);
            for(int i=0;i<sim.Count;i++) Assert.That(sim.Alpha(i),Is.EqualTo(1));
            sim.Step(.1f); Assert.That(sim.Position(0),Is.Not.EqualTo(Vector2.zero));
            sim.Step(ConfettiConfig.Default.Lifetime);
            Assert.That(sim.Running,Is.False); Assert.That(sim.Alpha(0),Is.EqualTo(0));
        }
        [Test] public void ConfettiFadesDuringTheFinalSeconds()
        {
            var c=ConfettiConfig.Default; var sim=new ConfettiSimulation(c,7); sim.Start(Vector2.zero);
            sim.Step(c.Lifetime-c.FadeSeconds/2);
            Assert.That(sim.Alpha(0),Is.EqualTo(.5f).Within(1e-3f));
        }
        [Test] public void ConfettiFallsUnderGravityAfterRising()
        {
            var sim=new ConfettiSimulation(ConfettiConfig.Default,7); sim.Start(Vector2.zero);
            float peak=0; for(int i=0;i<60;i++) { sim.Step(.02f); peak=Mathf.Max(peak,sim.Position(0).y); }
            Assert.That(peak,Is.GreaterThan(0)); Assert.That(sim.Position(0).y,Is.LessThan(peak));
        }
        [Test] public void ConfettiIsDeterministicForASeed()
        {
            var a=new ConfettiSimulation(ConfettiConfig.Default,3); var b=new ConfettiSimulation(ConfettiConfig.Default,3);
            a.Start(Vector2.zero); b.Start(Vector2.zero); a.Step(.3f); b.Step(.3f);
            Assert.That(a.Position(5),Is.EqualTo(b.Position(5)));
        }
        [Test] public void ConfettiConfigRejectsInvalidValues()
        {
            Assert.Throws<System.ArgumentException>(()=>new ConfettiConfig(0,1,0,0,0,1,2,10,0,1,2,new[]{Color.red}));
            Assert.Throws<System.ArgumentException>(()=>new ConfettiConfig(1,1,2,0,0,1,2,10,0,1,2,new[]{Color.red}));
        }
        [Test] public void LoseProviderIsNotPlayedBeforeOpeningFinishes()
        {
            var driver=ShowLose(); driver.Evaluate(Opened(driver)*.5f,false);
            Assert.That(loseRecorder.Plays,Is.EqualTo(0)); Assert.That(recorder.Plays,Is.EqualTo(0));
        }
        [Test] public void LoseProviderPlaysOnceWithAStageAndNoConfetti()
        {
            var driver=ShowLose(); driver.Evaluate(Opened(driver),false); driver.Evaluate(Opened(driver)+1,false);
            Assert.That(loseRecorder.Plays,Is.EqualTo(1)); Assert.That(loseRecorder.Stage,Is.SameAs(driver.Stage));
            Assert.That(driver.Confetti,Is.Null); Assert.That(recorder.Plays,Is.EqualTo(0));
            Assert.That(driver.Stage.parent,Is.SameAs(driver.GetComponent<PopupView>().Panel));
        }
        [Test] public void RetryStopsTheLoseProvider()
        {
            var driver=ShowLose(); driver.Evaluate(Opened(driver),false); presenter.Act(SessionPopupKind.Lose,"retry");
            Assert.That(loseRecorder.Stops,Is.EqualTo(1)); Assert.That(fixture.Game.Retries,Is.EqualTo(1));
        }
        [Test] public void ReviveStopsTheLoseProvider()
        {
            var driver=ShowLose(); driver.Evaluate(Opened(driver),false); presenter.Act(SessionPopupKind.Lose,"revive");
            Assert.That(loseRecorder.Stops,Is.EqualTo(1));
        }
        [Test] public void LoseProviderStopsOnceWhenThePopupStartsClosingAndThePresenterIsDisposed()
        {
            var driver=ShowLose(); driver.Evaluate(Opened(driver),false); driver.Evaluate(0,true); driver.Evaluate(0,true); presenter.Dispose();
            Assert.That(loseRecorder.Stops,Is.EqualTo(1));
        }
        [Test] public void LoseWithoutAProviderHasNoDriverAndActionsStillWork()
        {
            fixture.Presenter.ShowLose();
            Assert.That(fixture.Presenter.Popups[SessionPopupKind.Lose].Celebration,Is.Null);
            Assert.DoesNotThrow(()=>fixture.Presenter.Act(SessionPopupKind.Lose,"retry"));
            Assert.That(fixture.Game.Retries,Is.EqualTo(1));
        }
        [Test] public void ShouldPlayNeedsFinishedOpeningAndNoClosing()
        {
            Assert.That(ResultCelebrationDriver.ShouldPlay(.1f,.2f,false),Is.False);
            Assert.That(ResultCelebrationDriver.ShouldPlay(.2f,.2f,false),Is.True);
            Assert.That(ResultCelebrationDriver.ShouldPlay(1,.2f,true),Is.False);
        }
    }
}
