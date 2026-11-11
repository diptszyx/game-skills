using System;
using System.Linq;
using IndieGame.Shell;
using Xunit;
namespace IndieGame.Shell.Tests;
public sealed class DailyQuestsTests
{
    readonly DailyFakeClock clock = new(); readonly DailyFakeGrantor grantor = new();
    static QuestTemplate Quest(string id, int target = 3, string key = "win") => new(id, key, target, new Reward(77));
    DailyQuestsService Create(bool weekly = false) => new(new DailyQuestsConfig(new[] { Quest("wins") }, 1, weekly ? new[] { Quest("week", 6) } : null, weekly ? 1 : 0), clock, grantor);
    static QuestStatus Status(DailyQuestsService s, string id = "wins") => s.Quests.Single(q => q.Template.Id == id);
    [Fact] public void EventProgressIsCappedAtConfiguredTargetAndUsesMatchingEventKey()
    {
        var s = Create(); Assert.True(s.RecordEvent("lose", 99).Success); Assert.Equal(0, Status(s).Progress);
        s.RecordEvent("win", 2); Assert.Equal(2, Status(s).Progress); s.RecordEvent("win", 20); Assert.Equal(3, Status(s).Progress);
    }
    [Fact] public void NegativeAndOverflowEventsAreRejectedAndLeaveAllStateUnchanged()
    {
        var s = Create(); s.RecordEvent("win", 1); Assert.Equal(DailyError.InvalidArgument, s.RecordEvent("win", -1).Error);
        Assert.Equal(DailyError.Overflow, s.RecordEvent("win", long.MaxValue).Error); Assert.Equal(DailyError.Overflow, s.RecordEvent("win", int.MaxValue).Error); Assert.Equal(1, Status(s).Progress);
        clock.UtcNow = clock.UtcNow.AddDays(1); Assert.Equal(DailyError.InvalidArgument, s.RecordEvent("win", -1).Error);
        Assert.Equal(DailyError.Overflow, s.RecordEvent("win", (long)int.MaxValue + 1).Error); Assert.Equal(1, Status(s).Progress);
        Assert.True(s.Refresh().Success); Assert.Equal(0, Status(s).Progress);
    }
    [Fact] public void OverflowPreflightIsAtomicAcrossAllMatchingDailyAndWeeklyQuests()
    {
        var s = new DailyQuestsService(new DailyQuestsConfig(new[] { Quest("first", int.MaxValue), Quest("second", int.MaxValue) }, 2), clock, grantor);
        s.RecordEvent("win", 1); Assert.Equal(DailyError.Overflow, s.RecordEvent("win", int.MaxValue).Error);
        Assert.All(s.Quests, q => Assert.Equal(1, q.Progress)); var w = Create(true); w.RecordEvent("win", 1); clock.UtcNow = clock.UtcNow.AddDays(1);
        Assert.Equal(DailyError.Overflow, w.RecordEvent("win", int.MaxValue).Error); Assert.Equal(1, Status(w).Progress); Assert.Equal(1, Status(w, "week").Progress);
    }
    [Fact] public void ClaimOnlyWhenCompleteAndDuplicateClaimGrantsOnceWithConfiguredRewardAndId()
    {
        var s = Create(); Assert.Equal(DailyError.Incomplete, s.Claim("wins").Error); Assert.Equal(0, grantor.Calls);
        s.RecordEvent("win", 3); Assert.True(s.Claim("wins").Success); Assert.Equal(DailyError.AlreadyClaimed, s.Claim("wins").Error);
        Assert.Equal(1, grantor.Calls); Assert.Equal(77, grantor.Granted["quest:2026-11-10:wins"].Gold);
    }
    [Fact] public void UtcMidnightResetClearsProgressAndClaimedFlagsAt235959Versus000000()
    {
        clock.UtcNow = new DateTime(2026, 11, 10, 23, 59, 59, DateTimeKind.Utc); var s = Create(); s.RecordEvent("win", 3); s.Claim("wins"); Assert.True(Status(s).Claimed);
        clock.UtcNow = clock.UtcNow.AddSeconds(1); Assert.True(s.Refresh().Success); Assert.Equal(0, Status(s).Progress); Assert.False(Status(s).Claimed);
        s.RecordEvent("win", 3); Assert.True(s.Claim("wins").Success); Assert.Equal(2, grantor.Granted.Count);
    }
    [Fact] public void SameDateAndConfigSelectSameDailySetDeterministicallyRegardlessOfHistory()
    {
        var config = new DailyQuestsConfig(Enumerable.Range(0, 7).Select(i => Quest("q" + i)).ToArray(), 3);
        var a = new DailyQuestsService(config, clock, grantor); var b = new DailyQuestsService(config, clock, grantor);
        a.Refresh(); b.Refresh(); var initial = a.Quests.Select(q => q.Template.Id).ToArray(); Assert.Equal(initial, b.Quests.Select(q => q.Template.Id));
        clock.UtcNow = clock.UtcNow.AddDays(1); a.Refresh(); var fresh = new DailyQuestsService(config, clock, grantor); fresh.Refresh();
        Assert.Equal(a.Quests.Select(q => q.Template.Id), fresh.Quests.Select(q => q.Template.Id)); Assert.NotEqual(string.Join(",", initial), string.Join(",", a.Quests.Select(q => q.Template.Id))); Assert.Equal(3, a.Quests.Select(q => q.Template.Id).Distinct().Count());
    }
    [Fact] public void WeeklyListPersistsWithinWeekAndResetsAtUtcMondayWithNewWeeklyClaimId()
    {
        clock.UtcNow = new DateTime(2026, 11, 14, 23, 59, 59, DateTimeKind.Utc); var s = Create(true); s.RecordEvent("win", 6); Assert.True(s.Claim("week").Success);
        Assert.Contains("quest:2026-11-09:week", grantor.Granted.Keys); clock.UtcNow = clock.UtcNow.AddDays(1); s.Refresh(); Assert.True(Status(s, "week").Claimed); Assert.Equal(6, Status(s, "week").Progress);
        clock.UtcNow = clock.UtcNow.AddSeconds(1); s.Refresh(); Assert.False(Status(s, "week").Claimed); Assert.Equal(0, Status(s, "week").Progress);
        s.RecordEvent("win", 6); Assert.True(s.Claim("week").Success); Assert.Contains("quest:2026-11-16:week", grantor.Granted.Keys);
    }
    [Fact] public void WeeklySelectionChangesOnlyAtMondayAndIsDeterministic()
    {
        var config = new DailyQuestsConfig(Array.Empty<QuestTemplate>(), 0, new[] { Quest("w1"), Quest("w2"), Quest("w3") }, 1);
        clock.UtcNow = new DateTime(2026, 11, 15, 23, 59, 59, DateTimeKind.Utc); var s = new DailyQuestsService(config, clock, grantor); s.Refresh(); string old = s.Quests.Single().Template.Id;
        clock.UtcNow = clock.UtcNow.AddSeconds(1); s.Refresh(); Assert.NotEqual(old, s.Quests.Single().Template.Id);
        var other = new DailyQuestsService(config, clock, grantor); other.Refresh(); Assert.Equal(s.Quests.Single().Template.Id, other.Quests.Single().Template.Id);
    }
    [Fact] public void ClockBackwardDoesNotResetProgressOrReopenClaimedDailyOrWeeklyQuests()
    {
        var s = Create(true); s.RecordEvent("win", 6); s.Claim("wins"); s.Claim("week"); clock.UtcNow = clock.UtcNow.AddDays(-1);
        Assert.Equal(DailyError.ClockReversed, s.Refresh().Error); Assert.Equal(DailyError.ClockReversed, s.RecordEvent("win", 1).Error); Assert.Equal(DailyError.ClockReversed, s.Claim("wins").Error);
        Assert.True(Status(s).Claimed); Assert.True(Status(s, "week").Claimed); clock.UtcNow = clock.UtcNow.AddDays(1); Assert.Equal(DailyError.AlreadyClaimed, s.Claim("wins").Error); Assert.Equal(2, grantor.Calls);
    }
    [Fact] public void SaveRoundTripPreservesProgressClaimedFlagsSelectionAndClockHighWaterMark()
    {
        var s = Create(true); s.RecordEvent("win", 3); s.Claim("wins"); var doc = new SaveDocument(); doc.WriteSection("other", 52); Assert.True(s.Save(doc).Success);
        doc = DailyTestData.RoundTrip(doc); var restored = Create(true); Assert.True(restored.Load(doc).Success); Assert.Equal(3, Status(restored, "week").Progress); Assert.True(Status(restored).Claimed);
        Assert.Equal(DailyError.AlreadyClaimed, restored.Claim("wins").Error); Assert.Equal(52, doc.ReadSection("other", () => 0).Value);
        clock.UtcNow = clock.UtcNow.AddDays(-1); Assert.Equal(DailyError.ClockReversed, restored.Refresh().Error);
        clock.UtcNow = clock.UtcNow.AddDays(2); restored.Refresh(); Assert.Equal(0, Status(restored).Progress); Assert.Equal(3, Status(restored, "week").Progress);
    }
    [Theory] [InlineData("wrong")] [InlineData("negative")] [InlineData("overflow")] [InlineData("incomplete-claim")] [InlineData("missing")]
    public void CorruptQuestSectionFallsBackToDefaultsWithErrorCode(string kind)
    {
        int day = (int)(clock.UtcNow.Date.Ticks / TimeSpan.TicksPerDay);
        object state = kind switch { "wrong" => "broken", "missing" => new { Version = 1 }, _ => new { Version = 1, Day = day, Entries = new[] { new { Id = "wins", Progress = kind == "negative" ? -1m : kind == "overflow" ? 999999999999999999m : 1m, Claimed = kind == "incomplete-claim" } } } };
        var doc = new SaveDocument(); doc.WriteSection("dailyQuests", state); var s = Create(); s.RecordEvent("win", 2);
        Assert.Equal(DailyError.CorruptSection, s.Load(doc).Error); Assert.Empty(s.Quests); Assert.True(s.Refresh().Success); Assert.Equal(0, Status(s).Progress);
    }
    [Fact] public void InvalidArgumentsLeaveStateUnchangedAndInvalidConfigReturnsResult()
    {
        var s = Create(); s.RecordEvent("win", 1); Assert.Equal(DailyError.InvalidArgument, s.Claim(null!).Error); Assert.Equal(DailyError.NotFound, s.Claim("unknown").Error);
        Assert.Equal(DailyError.InvalidArgument, s.RecordEvent("", 1).Error); Assert.Equal(DailyError.InvalidArgument, s.Save(null!).Error); Assert.Equal(DailyError.InvalidArgument, s.Load(null!).Error); Assert.Equal(1, Status(s).Progress);
        var bad = new DailyQuestsService(new DailyQuestsConfig(new[] { Quest("bad", -1) }, 1), clock, grantor);
        Assert.Equal(DailyError.InvalidConfig, bad.Refresh().Error); Assert.Equal(DailyError.InvalidConfig, bad.Claim("bad").Error); Assert.Equal(DailyError.InvalidConfig, bad.RecordEvent("win", 1).Error);
    }
    [Fact] public void RejectedGrantCanRetryAndGrantorAlreadyClaimedReconcilesLocalFlag()
    {
        var s = Create(); s.RecordEvent("win", 3); grantor.Reject = true; Assert.Equal(DailyError.GrantRejected, s.Claim("wins").Error); Assert.False(Status(s).Claimed);
        grantor.Reject = false; grantor.Granted.Add("quest:2026-11-10:wins", new Reward()); Assert.Equal(DailyError.AlreadyClaimed, s.Claim("wins").Error); Assert.True(Status(s).Claimed);
        Assert.Equal(DailyError.AlreadyClaimed, s.Claim("wins").Error); Assert.Equal(2, grantor.Calls);
    }
    [Fact] public void MissingQuestSectionAndEmptyConfigurationUseDefaults()
    {
        var s = new DailyQuestsService(new DailyQuestsConfig(Array.Empty<QuestTemplate>(), 0), clock, grantor);
        Assert.True(s.Load(new SaveDocument()).Success); Assert.True(s.Refresh().Success); Assert.Empty(s.Quests); Assert.True(s.RecordEvent("win", 0).Success);
    }
    [Fact] public void MaximumIntTargetAcceptsBoundaryAndRejectsOverflowWithoutPartialProgress()
    {
        var s = new DailyQuestsService(new DailyQuestsConfig(new[] { Quest("max", int.MaxValue) }, 1), clock, grantor);
        Assert.True(s.RecordEvent("win", int.MaxValue).Success);
        Assert.Equal(int.MaxValue, Status(s, "max").Progress);
        Assert.Equal(DailyError.Overflow, s.RecordEvent("win", 1).Error);
        Assert.Equal(int.MaxValue, Status(s, "max").Progress);
        Assert.True(s.Claim("max").Success);
    }
    [Fact] public void InvalidCountsDuplicateIdsAndNegativeRewardsAreRejectedWithoutGranting()
    {
        var configs = new[] {
            new DailyQuestsConfig(new[] { Quest("q") }, -1),
            new DailyQuestsConfig(new[] { Quest("q") }, 2),
            new DailyQuestsConfig(new[] { Quest("q"), Quest("q") }, 1),
            new DailyQuestsConfig(new[] { Quest("q") }, 1, new[] { Quest("q") }, 1),
            new DailyQuestsConfig(new[] { new QuestTemplate("q", "win", 1, new Reward(-1)) }, 1),
            new DailyQuestsConfig(new[] { new QuestTemplate("q", "win", 1, new Reward(0, -1)) }, 1),
            new DailyQuestsConfig(new[] { new QuestTemplate("q", "win", 1, new Reward(0, 0, new System.Collections.Generic.Dictionary<string, int> { ["b"] = -1 })) }, 1)
        };
        foreach (var config in configs)
        {
            var s = new DailyQuestsService(config, clock, grantor);
            Assert.Equal(DailyError.InvalidConfig, s.Refresh().Error);
            Assert.Empty(s.Quests);
        }
        Assert.Equal(0, grantor.Calls);
    }
    [Fact] public void BothModulesPersistAsIndependentVersionedSectionsInOneDocument()
    {
        var quests = Create(); quests.RecordEvent("win", 2);
        var checkIn = new DailyCheckInService(DailyTestData.Config(), clock, grantor); checkIn.Claim();
        var doc = new SaveDocument(); Assert.True(quests.Save(doc).Success); Assert.True(checkIn.Save(doc).Success);
        doc = DailyTestData.RoundTrip(doc);
        Assert.Equal(1, doc.ReadSection("dailyQuests", () => new DailyQuestsService.State()).Value.Version);
        Assert.Equal(1, doc.ReadSection("dailyCheckIn", () => new DailyCheckInService.State()).Value.Version);
        var restoredQuests = Create(); Assert.True(restoredQuests.Load(doc).Success); Assert.Equal(2, Status(restoredQuests).Progress);
        var restoredCheckIn = new DailyCheckInService(DailyTestData.Config(), clock, grantor);
        Assert.True(restoredCheckIn.Load(doc).Success); Assert.Equal(DailyError.AlreadyClaimed, restoredCheckIn.Claim().Error);
    }

}
