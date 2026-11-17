using System;
using System.Collections.Generic;
using System.Linq;
using IndieGame.Shell;
using Xunit;

namespace IndieGame.Shell.Tests;

public sealed class ServicesTests
{
    static AdResult ShowNow(IRewardedAdService ads, string placement) { AdResult result = null!; ads.Show(placement, r => result = r); return result; }
    [Fact] public void RewardedAdFakeReturnsScriptedOutcomesInOrderThenNotReady()
    {
        var f = new FakeRewardedAdService(); IRewardedAdService socket = f;
        var expected = new[] { AdOutcome.Completed, AdOutcome.Skipped, AdOutcome.Failed, AdOutcome.NotReady };
        foreach (var outcome in expected) Assert.True(f.Enqueue(outcome).Success);
        foreach (var outcome in expected) Assert.Equal(outcome, ShowNow(socket, "reward").Outcome);
        Assert.Equal(AdOutcome.NotReady, ShowNow(socket, "reward").Outcome);
    }
    [Fact] public void PurchaseFakeReturnsScriptedOutcomesInOrderAndRestoreReturnsScriptedOutcomesInOrder()
    {
        var f = new FakePurchaseService(); IPurchaseService socket = f;
        f.Enqueue(PurchaseOutcome.Cancelled); f.Enqueue(PurchaseOutcome.Failed); f.Enqueue(PurchaseOutcome.Success, "t1");
        Assert.Equal(PurchaseOutcome.Cancelled, socket.Purchase("pack").Outcome);
        Assert.Equal(PurchaseOutcome.Failed, socket.Purchase("pack").Outcome); Assert.True(socket.Purchase("pack").Success);
        Assert.False(socket.Purchase("pack").Success);
        f.EnqueueRestore(PurchaseOutcome.Cancelled); f.EnqueueRestore(PurchaseOutcome.Failed); f.EnqueueRestore(PurchaseOutcome.Success);
        Assert.Equal(PurchaseOutcome.Cancelled, socket.Restore().Outcome); Assert.Equal(PurchaseOutcome.Failed, socket.Restore().Outcome);
        var restored = socket.Restore(); Assert.True(restored.Success); Assert.Equal("t1", Assert.Single(restored.Purchases).TransactionId);
        Assert.True(restored.Purchases[0].AlreadyProcessed); Assert.False(socket.Restore().Success);
    }
    [Fact] public void ConfigFakeReturnsScriptedOutcomesInOrderThenUnavailable()
    {
        var f = new FakeConfigSource(); IConfigSource socket = f;
        f.Enqueue("first"); f.Enqueue("bad", ServiceError.Failed); f.Enqueue("third");
        Assert.Equal("first", socket.Fetch().Value); Assert.Equal(ServiceError.Failed, socket.Fetch().Error);
        Assert.Equal("third", socket.Fetch().Value); Assert.Equal(ServiceError.Unavailable, socket.Fetch().Error);
    }
    [Fact] public void LeaderboardFakeReturnsScriptedOutcomesInOrderAcrossAllOperations()
    {
        var f = new FakeLeaderboardService("me", new FakeLeaderboardState()); ILeaderboardService socket = f;
        f.Enqueue(ServiceError.Failed); f.Enqueue(ServiceError.Unavailable); f.Enqueue(ServiceError.Failed); f.Enqueue(ServiceError.None);
        Assert.Equal(ServiceError.Failed, socket.SubmitScore("board", 20).Error);
        Assert.Equal(ServiceError.Unavailable, socket.GetTop("board", 2).Error);
        Assert.Equal(ServiceError.Failed, socket.GetMyRank("board").Error);
        Assert.True(socket.SubmitScore("board", 30).Success); Assert.Equal(30, socket.GetMyRank("board").Value!.Score);
    }
    [Theory]
    [InlineData(AdOutcome.Skipped)] [InlineData(AdOutcome.Failed)] [InlineData(AdOutcome.NotReady)]
    public void AdFailurePathsLeaveCallerStateUntouchedAndOnlyCompletedGrantsReward(AdOutcome failure)
    {
        var f = new FakeRewardedAdService(); f.Enqueue(failure); f.Enqueue(AdOutcome.Completed);
        var ledger = new Grantor(); var caller = new RewardCaller(ledger, new Reward(17));
        caller.Watch(f); Assert.Equal(0, ledger.Gold); Assert.Equal(0, ledger.Calls);
        caller.Watch(f); Assert.Equal(17, ledger.Gold); Assert.Equal(1, ledger.Calls);
    }
    [Theory]
    [InlineData(PurchaseOutcome.Cancelled)] [InlineData(PurchaseOutcome.Failed)]
    public void PurchaseFailurePathsLeaveCallerStateUntouchedAndOnlySuccessGrantsReward(PurchaseOutcome failure)
    {
        var f = new FakePurchaseService(); f.Enqueue(failure); f.Enqueue(PurchaseOutcome.Success, "t1");
        var ledger = new Grantor(); var caller = new RewardCaller(ledger, new Reward(23));
        caller.Buy(f); Assert.Equal(0, ledger.Gold); Assert.Equal(0, ledger.Calls);
        caller.Buy(f); Assert.Equal(23, ledger.Gold); Assert.Equal(1, ledger.Calls);
    }
    [Fact] public void PurchaseIsIdempotentPerTransactionIdIncludingRestoreAndConflictingProducts()
    {
        var f = new FakePurchaseService(); var ledger = new Grantor(); var caller = new RewardCaller(ledger, new Reward(23));
        f.Enqueue(PurchaseOutcome.Success, "same"); f.Enqueue(PurchaseOutcome.Success, "same");
        caller.Buy(f); caller.Buy(f); Assert.Equal(23, ledger.Gold); Assert.Equal(1, ledger.Calls);
        f.Enqueue(PurchaseOutcome.Success, "same"); var repeat = f.Purchase("pack"); Assert.True(repeat.Success); Assert.True(repeat.AlreadyProcessed);
        f.Enqueue(PurchaseOutcome.Success, "same"); Assert.Equal(ServiceError.Conflict, f.Purchase("other").Error);
        f.EnqueueRestore(PurchaseOutcome.Success); var restored = f.Restore(); Assert.Single(restored.Purchases);
        foreach (var r in restored.Purchases) ledger.Grant(r.TransactionId, new Reward(23));
        Assert.Equal(23, ledger.Gold);
        f.Enqueue(PurchaseOutcome.Success, "second"); caller.Buy(f); Assert.Equal(46, ledger.Gold);
    }
    [Fact] public void LeaderboardSortsDescendingAndBreaksTiesByEarlierSubmit()
    {
        var state = new FakeLeaderboardState();
        var a = new FakeLeaderboardService("a", state); var b = new FakeLeaderboardService("b", state); var c = new FakeLeaderboardService("c", state);
        a.SubmitScore("board", 50); b.SubmitScore("board", 100); c.SubmitScore("board", 100);
        Assert.Equal(new[] { "b", "c", "a" }, a.GetTop("board", 10).Value.Select(e => e.PlayerId));
        Assert.Equal(new[] { 1, 2, 3 }, a.GetTop("board", 10).Value.Select(e => e.Rank)); Assert.Equal(3, a.GetMyRank("board").Value!.Rank);
        a.SubmitScore("board", 100); Assert.Equal(new[] { "b", "c", "a" }, a.GetTop("board", 10).Value.Select(e => e.PlayerId));
        b.SubmitScore("board", 1); Assert.Equal(100, b.GetMyRank("board").Value!.Score);
        Assert.Equal(2, a.GetTop("board", 2).Value.Count); Assert.Empty(a.GetTop("board", 0).Value);
    }
    [Fact] public void NegativeLeaderboardScoresAndCountsAreRejectedAndFailureLeavesScoresUntouched()
    {
        var f = new FakeLeaderboardService("me", new FakeLeaderboardState()); f.SubmitScore("board", 10);
        f.Enqueue(ServiceError.Failed);
        Assert.Equal(ServiceError.InvalidArgument, f.SubmitScore("board", -1).Error);
        Assert.Equal(ServiceError.InvalidArgument, f.GetTop("board", -1).Error);
        Assert.Equal(ServiceError.Failed, f.SubmitScore("board", 20).Error);
        Assert.Equal(10, f.GetMyRank("board").Value!.Score);
        Assert.True(f.SubmitScore("board", long.MaxValue).Success); Assert.Equal(long.MaxValue, f.GetMyRank("board").Value!.Score);
        Assert.Equal(ServiceError.NotFound, f.GetMyRank("missing").Error); Assert.Empty(f.GetTop("missing", int.MaxValue).Value);
        Assert.Single(f.GetTop("board", int.MaxValue).Value);
    }
    [Fact] public void InvalidServiceArgumentsAndInvalidScriptsDoNotConsumeQueuedOutcomes()
    {
        var ads = new FakeRewardedAdService(); Assert.False(ads.Enqueue((AdOutcome)999).Success); ads.Enqueue(AdOutcome.Completed);
        Assert.False(ShowNow(ads, null!).Success); Assert.True(ShowNow(ads, "ok").Success);
        var purchases = new FakePurchaseService(); Assert.False(purchases.Enqueue(PurchaseOutcome.Success, " ").Success);
        Assert.False(purchases.Enqueue((PurchaseOutcome)999).Success); Assert.False(purchases.EnqueueRestore((PurchaseOutcome)999).Success);
        purchases.Enqueue(PurchaseOutcome.Success, "t"); Assert.False(purchases.Purchase("").Success); Assert.True(purchases.Purchase("pack").Success);
        var config = new FakeConfigSource(); Assert.False(config.Enqueue(null!).Success); Assert.False(config.Enqueue("", (ServiceError)999).Success);
        config.Enqueue("ok"); Assert.Equal("ok", config.Fetch().Value);
        var board = new FakeLeaderboardService("me", new FakeLeaderboardState()); Assert.False(board.Enqueue((ServiceError)999).Success);
        board.Enqueue(ServiceError.Failed); Assert.False(board.SubmitScore("", 1).Success); Assert.False(board.GetTop("", 1).Success);
        Assert.False(board.GetMyRank(null!).Success); Assert.Equal(ServiceError.Failed, board.SubmitScore("b", 1).Error); Assert.Empty(board.GetTop("b", 10).Value);
    }
    [Fact] public void SeparateFakeInstancesDoNotLeakStateAndSnapshotsDoNotChangeAfterLaterWrites()
    {
        var a = new FakeLeaderboardService("me", new FakeLeaderboardState()); var b = new FakeLeaderboardService("me", new FakeLeaderboardState());
        a.SubmitScore("b", 1); var snapshot = a.GetTop("b", 10).Value; a.SubmitScore("b", 2);
        Assert.Equal(1, Assert.Single(snapshot).Score); Assert.Empty(b.GetTop("b", 10).Value);
        var f = new FakePurchaseService(); f.EnqueueRestore(PurchaseOutcome.Success); var restored = f.Restore();
        f.Enqueue(PurchaseOutcome.Success, "t"); f.Purchase("pack"); Assert.Empty(restored.Purchases);
        var other = new FakePurchaseService(); other.Enqueue(PurchaseOutcome.Success, "t"); Assert.False(other.Purchase("pack").AlreadyProcessed);
    }
    sealed class RewardCaller
    {
        readonly IRewardGrantor grantor; readonly Reward reward;
        public RewardCaller(IRewardGrantor grantor, Reward reward) { this.grantor = grantor; this.reward = reward; }
        public void Watch(IRewardedAdService ads) { ads.Show("reward", result => { if (result.Success) grantor.Grant("ad-claim", reward); }); }
        public void Buy(IPurchaseService purchases)
        { var r = purchases.Purchase("pack"); if (r.Success && !r.AlreadyProcessed) grantor.Grant(r.TransactionId, reward); }
    }
    sealed class Grantor : IRewardGrantor
    {
        readonly HashSet<string> claims = new(); public long Gold { get; private set; } public int Calls { get; private set; }
        public GrantOutcome Grant(string claimId, Reward reward)
        {
            if (claims.Contains(claimId)) return GrantOutcome.AlreadyClaimed;
            if (reward.Gold < 0 || reward.Gold > long.MaxValue - Gold) return GrantOutcome.Rejected;
            claims.Add(claimId); Gold += reward.Gold; Calls++; return GrantOutcome.Granted;
        }
    }
}
