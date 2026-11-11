using System;
using IndieGame.Shell;
using Xunit;

namespace IndieGame.Shell.Tests;

public sealed class TestClock : IClock
{
    public DateTime UtcNow { get; set; } = new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc);
}
public sealed class LivesTests
{
    static LivesService Create(TestClock clock, int initial = 2) => new(new LivesConfig(5, initial, TimeSpan.FromMinutes(10)), clock);
    [Fact] public void PartialIntervalCarriesOverAcrossRefreshAndSpend()
    {
        var c = new TestClock(); var lives = Create(c); var start = c.UtcNow;
        c.UtcNow = start.AddMinutes(15); Assert.True(lives.Refresh().Success); Assert.Equal(3, lives.Hearts);
        Assert.True(lives.SpendHeart().Success);
        c.UtcNow = start.AddMinutes(20); lives.Refresh(); Assert.Equal(3, lives.Hearts);
        Assert.Equal(start.AddMinutes(40), lives.FullAtUtc);
    }
    [Fact] public void ClockGoingBackwardGainsAndLosesNothingIncludingUnlimitedTime()
    {
        var c = new TestClock(); var lives = Create(c); var start = c.UtcNow;
        lives.AddUnlimited(TimeSpan.FromMinutes(20));
        c.UtcNow = start.AddMinutes(20); lives.Refresh(); Assert.False(lives.IsUnlimited); Assert.Equal(4, lives.Hearts);
        var full = lives.FullAtUtc;
        c.UtcNow = start.AddMinutes(5); lives.Refresh(); Assert.Equal(4, lives.Hearts);
        Assert.False(lives.IsUnlimited); Assert.Equal(full, lives.FullAtUtc);
        c.UtcNow = start.AddMinutes(25); lives.Refresh(); Assert.Equal(4, lives.Hearts);
        c.UtcNow = start.AddMinutes(30); lives.Refresh(); Assert.Equal(5, lives.Hearts);
    }
    [Fact] public void LongOfflineGivesAtMostMaxAndAtMaxHasNoTimer()
    {
        var c = new TestClock(); var lives = Create(c, 0);
        c.UtcNow = c.UtcNow.AddYears(100); lives.Refresh(); Assert.Equal(5, lives.Hearts); Assert.Null(lives.FullAtUtc);
        lives.SpendHeart(); Assert.Equal(c.UtcNow.AddMinutes(10), lives.FullAtUtc);
    }
    [Fact] public void UnlimitedWindowsAreAdditiveAndActiveBeforeButExpiredAtBoundary()
    {
        var c = new TestClock(); var lives = Create(c); var start = c.UtcNow;
        lives.AddUnlimited(TimeSpan.FromMinutes(10)); c.UtcNow = start.AddMinutes(5);
        lives.AddUnlimited(TimeSpan.FromMinutes(10)); c.UtcNow = start.AddMinutes(20).AddTicks(-1);
        Assert.True(lives.IsUnlimited); c.UtcNow = start.AddMinutes(20); Assert.False(lives.IsUnlimited);
        lives.AddUnlimited(TimeSpan.FromMinutes(2)); Assert.True(lives.IsUnlimited);
        c.UtcNow = start.AddMinutes(22); Assert.False(lives.IsUnlimited);
    }
    [Fact] public void SpendingHeartDuringUnlimitedDoesNotConsumeEvenAtZeroHearts()
    {
        var c = new TestClock(); var lives = Create(c, 0); lives.AddUnlimited(TimeSpan.FromMinutes(5));
        Assert.True(lives.SpendHeart().Success); Assert.Equal(0, lives.Hearts);
        c.UtcNow = c.UtcNow.AddMinutes(5); Assert.Equal(ModuleError.Insufficient, lives.SpendHeart().Error);
    }
    [Fact] public void RefillTimeQueryReportsExactTimeAndFullHasNoTimer()
    {
        var c = new TestClock(); var lives = Create(c); var start = c.UtcNow;
        Assert.Equal(start.AddMinutes(30), lives.FullAtUtc);
        c.UtcNow = start.AddMinutes(11); lives.Refresh(); Assert.Equal(start.AddMinutes(30), lives.FullAtUtc);
        c.UtcNow = start.AddMinutes(30); lives.Refresh(); Assert.Null(lives.FullAtUtc);
    }
    [Fact] public void LivesSaveRoundTripPreservesPartialIntervalAndUnlimitedWindow()
    {
        var c = new TestClock(); var lives = Create(c); var start = c.UtcNow;
        lives.AddUnlimited(TimeSpan.FromMinutes(40)); c.UtcNow = start.AddMinutes(15); lives.Refresh();
        var doc = new SaveDocument(); Assert.True(lives.Save(doc).Success);
        var restored = Create(c); Assert.True(restored.Load(doc).Success); Assert.Equal(3, restored.Hearts);
        c.UtcNow = start.AddMinutes(20); restored.Refresh(); Assert.Equal(4, restored.Hearts); Assert.True(restored.IsUnlimited);
        c.UtcNow = start.AddMinutes(40); Assert.False(restored.IsUnlimited);
    }
    [Fact] public void NegativeAndOverflowLivesInputsLeaveStateUnchanged()
    {
        var c = new TestClock(); var lives = Create(c); var full = lives.FullAtUtc;
        Assert.Equal(ModuleError.InvalidArgument, lives.AddHearts(-1).Error);
        Assert.Equal(ModuleError.Overflow, lives.AddHearts(int.MaxValue).Error);
        Assert.Equal(ModuleError.InvalidArgument, lives.AddUnlimited(TimeSpan.Zero).Error);
        Assert.Equal(ModuleError.Overflow, lives.AddUnlimited(TimeSpan.MaxValue).Error);
        Assert.Equal(2, lives.Hearts); Assert.Equal(full, lives.FullAtUtc); Assert.False(lives.IsUnlimited);
    }
    [Fact] public void CorruptLivesSectionFallsBackToConfiguredDefaultsWithErrorCode()
    {
        var c = new TestClock(); var lives = Create(c); lives.AddHearts(2);
        var doc = new SaveDocument(); doc.WriteSection("lives", new { Version = 1, Hearts = -1 });
        Assert.Equal(ModuleError.CorruptSection, lives.Load(doc).Error); Assert.Equal(2, lives.Hearts);
    }
}
