using System;
using System.Collections.Generic;
using System.Linq;
using IndieGame.Shell;
using Newtonsoft.Json;
using Xunit;

namespace IndieGame.Shell.Tests;

public sealed class DailyCheckInPreviewTests
{
    readonly DailyFakeClock clock = new();
    readonly DailyFakeGrantor grantor = new();
    DailyCheckInService Create() => new(DailyTestData.Config(), clock, grantor);
    static string Snapshot(DailyCheckInService s)
    {
        var doc = new SaveDocument(); Assert.True(s.Save(doc).Success);
        return JsonConvert.SerializeObject(doc.ReadSection("dailyCheckIn", () => new DailyCheckInService.State()).Value);
    }
    [Fact] public void PreviewMatchesClaimRewardAcrossBothTablesMissedDaysAndSlotWrapWithoutMutation()
    {
        var s = Create();
        for (int i = 0; i < 12; i++)
        {
            string before = Snapshot(s); int calls = grantor.Calls;
            var preview = s.Preview(); var repeated = s.Preview();
            Assert.True(preview.Success); Assert.True(preview.CanClaimNow); Assert.Equal(i % 5, preview.NextSlot);
            Assert.Equal(before, Snapshot(s)); Assert.Equal(calls, grantor.Calls); Assert.Equal(preview.Reward!.Gold, repeated.Reward!.Gold);
            Assert.True(s.Claim().Success); var actual = grantor.Granted.Values.Last();
            Assert.Equal(preview.Reward.Gold, actual.Gold); Assert.Equal(preview.Reward.Hearts, actual.Hearts); Assert.Equal(preview.Reward.Boosters, actual.Boosters);
            clock.UtcNow = clock.UtcNow.AddDays(i == 2 ? 3 : 1);
        }
    }
    [Fact] public void PreviewAfterClaimCannotClaimSameUtcDayAndNextClaimAtIsExactlyNextUtcMidnight()
    {
        clock.UtcNow = new DateTime(2026, 11, 10, 23, 59, 59, DateTimeKind.Utc);
        var s = Create(); Assert.Equal(clock.UtcNow.Date.AddDays(1), s.Preview().NextClaimAtUtc);
        s.Claim(); string state = Snapshot(s); var preview = s.Preview();
        Assert.True(preview.Success); Assert.False(preview.CanClaimNow); Assert.Equal(DailyError.AlreadyClaimed, preview.Error);
        Assert.Equal(new DateTime(2026, 11, 11, 0, 0, 0, DateTimeKind.Utc), preview.NextClaimAtUtc);
        Assert.Equal(DateTimeKind.Utc, preview.NextClaimAtUtc!.Value.Kind); Assert.Equal(state, Snapshot(s));
        clock.UtcNow = clock.UtcNow.AddSeconds(1); Assert.True(s.Preview().CanClaimNow); Assert.True(s.Claim().Success);
    }
    [Fact] public void PreviewClockReversedCannotClaimAndDoesNotChangePersistedHighWaterDate()
    {
        var s = Create(); s.Claim(); string state = Snapshot(s); clock.UtcNow = clock.UtcNow.AddDays(-2);
        var preview = s.Preview(); Assert.False(preview.CanClaimNow); Assert.Equal(DailyError.ClockReversed, preview.Error);
        Assert.Equal(new DateTime(2026, 11, 11, 0, 0, 0, DateTimeKind.Utc), preview.NextClaimAtUtc);
        Assert.Equal(DailyError.ClockReversed, s.Claim().Error); Assert.Equal(state, Snapshot(s)); Assert.Equal(1, grantor.Calls);
    }
    [Fact] public void PreviewFutureDateDoesNotAdvanceObservedDayOrBlockAnEarlierValidClaim()
    {
        var s = Create(); s.Claim(); clock.UtcNow = clock.UtcNow.AddDays(10); string state = Snapshot(s);
        Assert.True(s.Preview().CanClaimNow); Assert.Equal(state, Snapshot(s));
        clock.UtcNow = clock.UtcNow.AddDays(-9); Assert.True(s.Claim().Success);
    }
    [Fact] public void PreviewInvalidConfigReturnsErrorWithoutGrantingOrThrowing()
    {
        var s = new DailyCheckInService(new DailyCheckInConfig(Array.Empty<IReadOnlyList<Reward>>()), clock, grantor);
        var preview = s.Preview(); Assert.False(preview.Success); Assert.False(preview.CanClaimNow);
        Assert.Equal(DailyError.InvalidConfig, preview.Error); Assert.Null(preview.Reward); Assert.Null(preview.NextClaimAtUtc); Assert.Equal(0, grantor.Calls);
    }
    [Fact] public void PreviewAtLastRepresentableDayReturnsNoNextMidnightWithoutOverflow()
    {
        clock.UtcNow = DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc); var s = Create();
        Assert.True(s.Preview().CanClaimNow); Assert.Null(s.Preview().NextClaimAtUtc); Assert.True(s.Claim().Success);
        Assert.False(s.Preview().CanClaimNow); Assert.Null(s.Preview().NextClaimAtUtc);
    }
}
