using System;
using NUnit.Framework;
using IndieGame.Shell;

namespace IndieGame.Shell.LevelPlay.Tests
{
    sealed class FakeRewardedAdBackend : IRewardedAdBackend
    {
        public bool IsReady { get; set; } = true;
        public int Loads, Shows;
        public string Placement;
        public event Action Loaded;
        public event Action<string> LoadFailed;
        public event Action Rewarded;
        public event Action Closed;
        public event Action<string> ShowFailed;
        public void Load() { Loads++; }
        public void Show(string placementId) { Shows++; Placement = placementId; }
        public void RaiseLoaded() { Loaded?.Invoke(); }
        public void FailLoad() { LoadFailed?.Invoke("load error"); }
        public void Reward() { Rewarded?.Invoke(); }
        public void Close() { Closed?.Invoke(); }
        public void FailShow() { ShowFailed?.Invoke("show error"); }
    }

    public sealed class LevelPlayRewardedServiceTests
    {
        FakeRewardedAdBackend backend;
        LevelPlayRewardedService service;
        float now;
        [SetUp] public void SetUp()
        {
            now = 0; backend = new FakeRewardedAdBackend();
            service = new LevelPlayRewardedService(backend, () => now);
        }
        [TearDown] public void TearDown() { service.Dispose(); }
        [Test] public void ShowWhenReadyCompletesAfterRewardedThenClosed()
        {
            AdResult result = null;
            service.Show("bonus", r => result = r);
            Assert.That(backend.Placement, Is.EqualTo("bonus"));
            backend.Reward(); Assert.That(result, Is.Null);
            backend.Close(); Assert.That(result.Outcome, Is.EqualTo(AdOutcome.Completed));
            Assert.That(backend.Loads, Is.EqualTo(1));
        }
        [Test] public void ClosedWithoutRewardIsSkipped()
        {
            AdResult result = null; service.Show("bonus", r => result = r);
            backend.Close(); Assert.That(result.Outcome, Is.EqualTo(AdOutcome.Skipped));
        }
        [Test] public void ShowFailureReportsFailedAndReloads()
        {
            AdResult result = null; service.Show("bonus", r => result = r);
            backend.FailShow(); Assert.That(result.Outcome, Is.EqualTo(AdOutcome.Failed));
            Assert.That(result.Error, Is.EqualTo(ServiceError.Failed));
            Assert.That(backend.Loads, Is.EqualTo(1));
        }
        [Test] public void ShowWhileInFlightReturnsConflict()
        {
            int first = 0; AdResult second = null;
            service.Show("one", r => first++); service.Show("two", r => second = r);
            Assert.That(second.Outcome, Is.EqualTo(AdOutcome.Failed));
            Assert.That(second.Error, Is.EqualTo(ServiceError.Conflict));
            Assert.That(backend.Shows, Is.EqualTo(1));
            backend.Reward(); backend.Close(); Assert.That(first, Is.EqualTo(1));
        }
        [Test] public void ShowWhenNotReadyReturnsNotReadyAndStartsLoad()
        {
            backend.IsReady = false; AdResult result = null;
            service.Show("bonus", r => result = r);
            Assert.That(result.Outcome, Is.EqualTo(AdOutcome.NotReady));
            Assert.That(backend.Loads, Is.EqualTo(1));
            service.Show("bonus", r => { }); Assert.That(backend.Loads, Is.EqualTo(1));
        }
        [Test] public void InvalidPlacementFailsWithInvalidArgument()
        {
            foreach (var placement in new[] { null, "", "  " })
            {
                AdResult result = null; service.Show(placement, r => result = r);
                Assert.That(result.Outcome, Is.EqualTo(AdOutcome.Failed));
                Assert.That(result.Error, Is.EqualTo(ServiceError.InvalidArgument));
            }
            Assert.Throws<ArgumentNullException>(() => service.Show("bonus", null));
            Assert.That(backend.Shows, Is.Zero);
        }
        [Test] public void OnDoneInvokedExactlyOnceEvenIfBackendRaisesRewardedTwice()
        {
            int count = 0; AdResult result = null;
            service.Show("bonus", r => { count++; result = r; });
            backend.Reward(); backend.Reward(); backend.Close(); backend.Close(); backend.FailShow();
            Assert.That(count, Is.EqualTo(1));
            Assert.That(result.Outcome, Is.EqualTo(AdOutcome.Completed));
        }
        [Test] public void LoadFailureRetriesWithCappedBackoff()
        {
            backend.IsReady = false; service.Show("bonus", r => { });
            int loads = 1;
            foreach (float delay in new[] { 5f, 10f, 20f, 40f, 60f, 60f })
            {
                backend.FailLoad(); now += delay - 0.25f; service.Tick();
                service.Show("bonus", r => { }); Assert.That(backend.Loads, Is.EqualTo(loads));
                now += 0.25f; service.Tick(); Assert.That(backend.Loads, Is.EqualTo(++loads));
            }
            backend.RaiseLoaded(); backend.FailLoad(); now += 5f; service.Tick();
            Assert.That(backend.Loads, Is.EqualTo(++loads));
        }
        [Test] public void SettingsRejectEmptyKeyAndAdUnit()
        {
            Assert.Throws<InvalidOperationException>(() => new LevelPlayAdsSettings { RewardedAdUnitId = "unit" }.Validate(false));
            Assert.Throws<InvalidOperationException>(() => new LevelPlayAdsSettings { AppKey = "key", RewardedAdUnitId = " " }.Validate(false));
            Assert.DoesNotThrow(() => new LevelPlayAdsSettings { AppKey = "key", RewardedAdUnitId = "unit" }.Validate(true));
        }
        [Test] public void PlaceholderKeyRejectedInReleaseBuildButAllowedInDev()
        {
            var settings = new LevelPlayAdsSettings { AppKey = "TEST_KEY_REPLACE_ME", RewardedAdUnitId = "unit", AllowTestKey = true };
            Assert.Throws<InvalidOperationException>(() => settings.Validate(true));
            Assert.DoesNotThrow(() => settings.Validate(false));
            settings.AllowTestKey = false;
            Assert.Throws<InvalidOperationException>(() => settings.Validate(false));
        }
    }
}
