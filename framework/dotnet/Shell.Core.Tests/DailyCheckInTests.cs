using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IndieGame.Shell;
using Xunit;
namespace IndieGame.Shell.Tests;
internal sealed class DailyFakeClock : IClock
{ public DateTime UtcNow { get; set; } = new DateTime(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc); }
internal sealed class DailyFakeGrantor : IRewardGrantor
{
    public readonly Dictionary<string, Reward> Granted = new(StringComparer.Ordinal);
    public int Calls;
    public bool Reject;
    public GrantOutcome Grant(string id, Reward reward)
    { Calls++; if (Reject) return GrantOutcome.Rejected; if (Granted.ContainsKey(id)) return GrantOutcome.AlreadyClaimed; Granted.Add(id, reward); return GrantOutcome.Granted; }
}
internal static class DailyTestData
{
    public static DailyCheckInConfig Config() => new(new IReadOnlyList<Reward>[] {
        Enumerable.Range(1, 5).Select(i => new Reward(i * 11)).ToArray(),
        Enumerable.Range(1, 5).Select(i => new Reward(i * 101)).ToArray() });
    public static SaveDocument RoundTrip(SaveDocument doc)
    {
        var path = Path.Combine(Path.GetTempPath(), "daily-test-" + Guid.NewGuid() + ".json");
        try { var store = new SaveStore(); Assert.True(store.Save(path, doc).Success); var loaded = store.Load(path); Assert.True(loaded.Success); return loaded.Value!; }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
}
public sealed class DailyCheckInTests
{
    readonly DailyFakeClock clock = new(); readonly DailyFakeGrantor grantor = new();
    DailyCheckInService Create() => new(DailyTestData.Config(), clock, grantor);
    [Fact] public void OneClaimPerUtcDay_DuplicateReturnsAlreadyClaimedAndGrantsOnce()
    {
        var s = Create(); Assert.True(s.Claim().Success); Assert.Equal(DailyError.AlreadyClaimed, s.Claim().Error);
        Assert.Single(grantor.Granted); Assert.Equal(1, grantor.Calls); Assert.Contains("checkin:2026-11-10", grantor.Granted.Keys);
    }
    [Fact] public void MissingDayPreservesNextSlotAndUtcDateAlternatesConfiguredRewardTables()
    {
        var s = Create(); Assert.True(s.Claim().Success); int first = (int)(clock.UtcNow.Date.Ticks / TimeSpan.TicksPerDay) % 2;
        Assert.Equal(first == 0 ? 11L : 101L, grantor.Granted.Values.Single().Gold);
        clock.UtcNow = clock.UtcNow.AddDays(3); Assert.True(s.Claim().Success);
        Assert.Equal(first == 0 ? 202L : 22L, grantor.Granted["checkin:2026-11-13"].Gold); Assert.Equal(2, s.NextSlot);
    }
    [Fact] public void FiveSlotTrackWrapsAfterFiveSuccessfulClaims()
    {
        var s = Create(); for (int i = 0; i < 5; i++) { Assert.True(s.Claim().Success); clock.UtcNow = clock.UtcNow.AddDays(1); }
        Assert.Equal(0, s.NextSlot); Assert.True(s.Claim().Success); Assert.Equal(1, s.NextSlot);
    }
    [Fact] public void UtcRolloverAt235959Versus000000OpensExactlyOneNewClaim()
    {
        clock.UtcNow = new DateTime(2026, 11, 10, 23, 59, 59, DateTimeKind.Utc); var s = Create(); Assert.True(s.Claim().Success);
        Assert.Equal(DailyError.AlreadyClaimed, s.Claim().Error); clock.UtcNow = clock.UtcNow.AddSeconds(1);
        Assert.True(s.Claim().Success); Assert.Equal(2, grantor.Granted.Count);
    }
    [Fact] public void ClockBackwardNeverReopensOrDoubleClaimsEvenAcrossSaveRoundTrip()
    {
        var s = Create(); s.Claim(); clock.UtcNow = clock.UtcNow.AddDays(2); s.Claim(); var doc = new SaveDocument(); Assert.True(s.Save(doc).Success);
        var restored = Create(); Assert.True(restored.Load(DailyTestData.RoundTrip(doc)).Success); clock.UtcNow = clock.UtcNow.AddDays(-1);
        Assert.Equal(DailyError.ClockReversed, restored.Claim().Error); clock.UtcNow = clock.UtcNow.AddDays(1);
        Assert.Equal(DailyError.AlreadyClaimed, restored.Claim().Error); Assert.Equal(2, grantor.Calls);
    }
    [Fact] public void SaveRoundTripPreservesSlotClaimDateAndOtherSections()
    {
        var s = Create(); s.Claim(); var doc = new SaveDocument(); doc.WriteSection("game", 42); Assert.True(s.Save(doc).Success);
        doc = DailyTestData.RoundTrip(doc); var restored = Create(); Assert.True(restored.Load(doc).Success);
        Assert.Equal(1, restored.NextSlot); Assert.Equal(DailyError.AlreadyClaimed, restored.Claim().Error); Assert.Equal(42, doc.ReadSection("game", () => 0).Value);
        clock.UtcNow = clock.UtcNow.AddDays(1); Assert.True(restored.Claim().Success); Assert.Equal(2, restored.NextSlot);
    }
    [Theory] [InlineData("wrong")] [InlineData("missing")] [InlineData("negative")] [InlineData("overflow")] [InlineData("version")]
    public void CorruptSectionFallsBackToDefaultsWithErrorCode(string kind)
    {
        object state = kind switch { "wrong" => "broken", "missing" => new { Version = 1 },
            "negative" => new { Version = 1, Day = -2, ClaimedDay = -2, NextSlot = -1 },
            "overflow" => new { Version = 1, Day = 999999999999999999m, ClaimedDay = 0, NextSlot = 0 },
            _ => new { Version = 2, Day = -1, ClaimedDay = -1, NextSlot = 0 } };
        var doc = new SaveDocument(); doc.WriteSection("dailyCheckIn", state); var s = Create(); s.Claim();
        Assert.Equal(DailyError.CorruptSection, s.Load(doc).Error); Assert.Equal(0, s.NextSlot); Assert.Equal(-1, s.ObservedDay);
    }
    [Fact] public void MissingSectionUsesDefaultsWithoutCorruption()
    { var s = Create(); Assert.True(s.Load(new SaveDocument()).Success); Assert.Equal(0, s.NextSlot); }
    [Fact] public void RejectedRewardLeavesClaimAndSlotUnchangedAndCanRetry()
    {
        var s = Create(); grantor.Reject = true; Assert.Equal(DailyError.GrantRejected, s.Claim().Error); Assert.Equal(0, s.NextSlot);
        grantor.Reject = false; Assert.True(s.Claim().Success);
    }
    [Fact] public void GrantorAlreadyClaimedReconcilesLocalClaimWithoutGrantingAgain()
    {
        grantor.Granted.Add("checkin:2026-11-10", new Reward()); var s = Create(); Assert.Equal(DailyError.AlreadyClaimed, s.Claim().Error);
        Assert.Equal(1, s.NextSlot); Assert.Equal(DailyError.AlreadyClaimed, s.Claim().Error); Assert.Equal(1, grantor.Calls);
    }
    [Fact] public void InvalidConfigurationAndNullDocumentsReturnErrorsWithoutThrowing()
    {
        var bad = new DailyCheckInService(new DailyCheckInConfig(Array.Empty<IReadOnlyList<Reward>>()), clock, grantor);
        Assert.Equal(DailyError.InvalidConfig, bad.Claim().Error); Assert.Equal(DailyError.InvalidArgument, Create().Load(null!).Error); Assert.Equal(DailyError.InvalidArgument, Create().Save(null!).Error);
        var rows = new IReadOnlyList<Reward>[] { Enumerable.Repeat(new Reward(-1), 5).ToArray(), Enumerable.Repeat(new Reward(), 5).ToArray() };
        Assert.Equal(DailyError.InvalidConfig, new DailyCheckInService(new DailyCheckInConfig(rows), clock, grantor).Claim().Error);
    }
    [Fact] public void ConstructorCopiesConfiguredRewardsAndAcceptsMaximumTypedAmounts()
    {
        var boosters = new Dictionary<string, int> { ["bomb"] = int.MaxValue }; var row = Enumerable.Repeat(new Reward(long.MaxValue, int.MaxValue, boosters), 5).ToArray();
        var s = new DailyCheckInService(new DailyCheckInConfig(new[] { row, row }), clock, grantor); boosters["bomb"] = -1; row[0] = new Reward(-1);
        Assert.True(s.Claim().Success); var reward = grantor.Granted.Values.Single(); Assert.Equal(long.MaxValue, reward.Gold); Assert.Equal(int.MaxValue, reward.Hearts); Assert.Equal(int.MaxValue, reward.Boosters["bomb"]);
    }
}
