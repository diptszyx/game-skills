using System;
using IndieGame.Shell.Unity.Screens;
using NUnit.Framework;
namespace IndieGame.Shell.Tests
{
    public sealed class AttemptHeartsTests
    {
        static (AttemptHearts attempt, LivesService lives, ScreenClock clock) Make(int hearts)
        {
            var clock = new ScreenClock();
            var lives = new LivesService(new LivesConfig(5, hearts, TimeSpan.FromSeconds(30)), clock);
            return (new AttemptHearts(lives), lives, clock);
        }
        [Test] public void LossSpendsOneHeart()
        { var (a, lives, _) = Make(3); a.Begin(); Assert.That(a.Lost(), Is.True); Assert.That(lives.Hearts, Is.EqualTo(2)); }
        [Test] public void LeavingUnwonAttemptSpendsOneHeart()
        { var (a, lives, _) = Make(3); a.Begin(); Assert.That(a.Left(), Is.True); Assert.That(lives.Hearts, Is.EqualTo(2)); }
        [Test] public void LeavingAfterWinSpendsNothing()
        { var (a, lives, _) = Make(3); a.Begin(); a.Won(); Assert.That(a.Left(), Is.False); Assert.That(lives.Hearts, Is.EqualTo(3)); }
        [Test] public void LoseThenLeaveSpendsOnce()
        { var (a, lives, _) = Make(3); a.Begin(); a.Lost(); a.Left(); Assert.That(lives.Hearts, Is.EqualTo(2)); }
        [Test] public void ReviveThenLoseAgainSpendsOnce()
        { var (a, lives, _) = Make(3); a.Begin(); a.Lost(); a.Lost(); Assert.That(lives.Hearts, Is.EqualTo(2)); }
        [Test] public void ReviveWorksWithZeroHeartsAfterTheLossSpentTheLast()
        {
            var (a, lives, _) = Make(1); a.Begin(); a.Lost();
            Assert.That(lives.Hearts, Is.Zero); a.Lost(); a.Left(); Assert.That(lives.Hearts, Is.Zero);
        }
        [Test] public void CannotStartWithZeroHearts()
        { var (a, _, _) = Make(0); Assert.That(a.CanStart(), Is.False); }
        [Test] public void CanStartWithOneHeart()
        { var (a, _, _) = Make(1); Assert.That(a.CanStart(), Is.True); }
        [Test] public void CanStartAfterRegenerationNotYetApplied()
        { var (a, _, clock) = Make(0); clock.UtcNow = clock.UtcNow.AddSeconds(31); Assert.That(a.CanStart(), Is.True); }
        [Test] public void NothingIsSpentWithoutAnAttempt()
        { var (a, lives, _) = Make(3); Assert.That(a.Lost(), Is.False); Assert.That(a.Left(), Is.False); Assert.That(lives.Hearts, Is.EqualTo(3)); }
        [Test] public void BeginningANewAttemptOwesAnotherHeart()
        { var (a, lives, _) = Make(3); a.Begin(); a.Lost(); a.Begin(); a.Left(); Assert.That(lives.Hearts, Is.EqualTo(1)); }
        [Test] public void UnlimitedHeartsAreAlwaysEnoughAndNeverSpent()
        {
            var (a, lives, _) = Make(0); lives.AddUnlimited(TimeSpan.FromHours(1));
            Assert.That(a.CanStart(), Is.True); a.Begin(); a.Lost(); a.Begin(); a.Left();
            Assert.That(lives.Hearts, Is.Zero); Assert.That(lives.IsUnlimited, Is.True);
        }

        [Test] public void LoseSessionPopupSpendsHeartAndCommitsOnce()
        {
            using (var f = new PopupFixture())
            {
                f.Session.Attempt.Begin(); f.Presenter.ShowLose();
                Assert.That(f.Session.Lives.Hearts, Is.Zero); Assert.That(f.Files.Writes, Is.EqualTo(1));
            }
        }
        [Test] public void WinSessionPopupSpendsNoHeart()
        {
            using (var f = new PopupFixture())
            {
                f.Session.Attempt.Begin(); f.Presenter.ShowWin("round-1");
                Assert.That(f.Session.Lives.Hearts, Is.EqualTo(1));
            }
        }
        [Test] public void LoseThenReviveThenLoseSpendsOneHeartAtPopupLevel()
        {
            using (var f = new PopupFixture())
            {
                f.Session.Attempt.Begin(); f.Presenter.ShowLose();
                f.Presenter.Act(SessionPopupKind.Lose, "revive"); f.Presenter.ShowLose();
                Assert.That(f.Session.Lives.Hearts, Is.Zero);
            }
        }
        [Test] public void RetryAfterLossSpendsNothingExtra()
        {
            using (var f = new PopupFixture())
            {
                f.Session.Attempt.Begin(); f.Presenter.ShowLose();
                f.Presenter.Act(SessionPopupKind.Lose, "retry");
                Assert.That(f.Game.Retries, Is.EqualTo(1)); Assert.That(f.Session.Lives.Hearts, Is.Zero);
            }
        }
    }
}
