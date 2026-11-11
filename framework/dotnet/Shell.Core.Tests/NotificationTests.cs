using System;
using IndieGame.Shell;
using Xunit;

namespace IndieGame.Shell.Tests;

public sealed class NotificationTests
{
    sealed class Daily : IDailyRewardNotificationSource { public DateTime? ReadyAtUtc { get; set; } }
    sealed class OrderedLives : ILivesNotificationSource
    {
        bool refreshed;
        public ModuleResult Refresh() { refreshed = true; return new ModuleResult(); }
        public DateTime? FullAtUtc { get { Assert.True(refreshed); return null; } }
    }
    static NotificationPlanner Create(TestClock clock, int start = 0, int end = 0, string dailyId = "daily") =>
        new(new NotificationConfig("lives", "lives.full", dailyId, "daily.ready", TimeSpan.FromHours(start), TimeSpan.FromHours(end)), clock);
    [Fact] public void LivesFullTimeIsComputedAfterRefillToNow()
    {
        var c = new TestClock(); var planner = Create(c);
        Assert.True(planner.Plan(new OrderedLives(), new Daily()).Success);
        var lives = new LivesService(new LivesConfig(5, 2, TimeSpan.FromMinutes(10)), c); var start = c.UtcNow;
        c.UtcNow = start.AddMinutes(15);
        var plan = planner.Plan(lives, new Daily()); Assert.True(plan.Success); Assert.Equal(3, lives.Hearts);
        Assert.Equal(start.AddMinutes(30), Assert.Single(plan.Notifications).FireAtUtc);
    }
    [Fact] public void AlreadyFullLivesHaveNoNotificationIncludingOfflineRefill()
    {
        var c = new TestClock(); var lives = new LivesService(new LivesConfig(5, 2, TimeSpan.FromMinutes(10)), c);
        c.UtcNow = c.UtcNow.AddDays(1);
        Assert.Empty(Create(c).Plan(lives, new Daily()).Notifications); Assert.Equal(5, lives.Hearts);
    }
    [Fact] public void ReplanningReplacesEarlierPlanAndSameIdNeverDuplicates()
    {
        var c = new TestClock(); var lives = new LivesService(new LivesConfig(5, 2, TimeSpan.FromMinutes(10)), c);
        var daily = new Daily { ReadyAtUtc = c.UtcNow.AddHours(1) }; var planner = Create(c, dailyId: "lives");
        var first = planner.Plan(lives, daily); Assert.Equal(daily.ReadyAtUtc, Assert.Single(first.Notifications).FireAtUtc);
        daily.ReadyAtUtc = c.UtcNow.AddHours(2);
        var second = planner.Plan(lives, daily); Assert.Equal(daily.ReadyAtUtc, Assert.Single(second.Notifications).FireAtUtc);
        Assert.NotEqual(first.Notifications[0].FireAtUtc, second.Notifications[0].FireAtUtc);
        lives.AddHearts(3); daily.ReadyAtUtc = null; Assert.Empty(planner.Plan(lives, daily).Notifications);
    }
    [Theory]
    [InlineData(22, 7, 23, 7, 1)]
    [InlineData(22, 7, 6, 7, 0)]
    [InlineData(22, 7, 22, 7, 1)]
    [InlineData(22, 7, 7, 7, 0)]
    [InlineData(13, 16, 14, 16, 0)]
    [InlineData(13, 16, 13, 16, 0)]
    [InlineData(13, 16, 16, 16, 0)]
    [InlineData(0, 0, 14, 14, 0)]
    public void QuietHoursInUtcDelayUntilEndWithStartInclusiveAndEndExclusive(int start, int end, int hour, int expectedHour, int dayOffset)
    {
        var c = new TestClock { UtcNow = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc) };
        var lives = new LivesService(new LivesConfig(5, 5, TimeSpan.FromMinutes(10)), c);
        var daily = new Daily { ReadyAtUtc = c.UtcNow.AddHours(hour) };
        var plan = Create(c, start, end).Plan(lives, daily); Assert.True(plan.Success);
        var item = Assert.Single(plan.Notifications);
        Assert.Equal(c.UtcNow.AddDays(dayOffset).AddHours(expectedHour), item.FireAtUtc);
        Assert.Equal(DateTimeKind.Utc, item.FireAtUtc.Kind); Assert.Equal("daily.ready", item.TextKey);
    }
    [Fact] public void DailyRewardAlreadyReadySchedulesAtNowAndAbsentInputProducesNone()
    {
        var c = new TestClock(); var lives = new LivesService(new LivesConfig(5, 5, TimeSpan.FromMinutes(10)), c);
        Assert.Equal(c.UtcNow, Assert.Single(Create(c).Plan(lives, new Daily { ReadyAtUtc = c.UtcNow.AddDays(-1) }).Notifications).FireAtUtc);
        Assert.Empty(Create(c).Plan(lives, new Daily()).Notifications);
        Assert.Equal(ModuleError.InvalidArgument, Create(c).Plan(null!, new Daily()).Error);
    }
}
