using System;
using System.Collections.Generic;
using System.IO;
using IndieGame.Shell;
using Newtonsoft.Json.Linq;
using Xunit;

namespace IndieGame.Shell.Tests;

public sealed class EconomyTests
{
    static Dictionary<string, int> Items(int value) => new() { ["hammer"] = value };
    static EconomyService Create(long gold = 20, int boosters = 3, LivesService? lives = null) =>
        new(new EconomyConfig(gold, Items(boosters)), lives ?? new LivesService(new LivesConfig(5, 2, TimeSpan.FromMinutes(10)), new TestClock()));
    [Fact] public void NegativeAmountsAndZeroSpendsAreRejectedWithoutChanges()
    {
        var e = Create();
        Assert.Equal(ModuleError.InvalidArgument, e.SpendGold(-1).Error);
        Assert.Equal(ModuleError.InvalidArgument, e.SpendGold(0).Error);
        Assert.Equal(ModuleError.InvalidArgument, e.SpendBooster("hammer", -1).Error);
        Assert.Equal(ModuleError.InvalidArgument, e.SpendBooster("hammer", 0).Error);
        Assert.Equal(ModuleError.InvalidArgument, e.GrantReward("negativeGold", new Reward(-1)).Error);
        Assert.Equal(ModuleError.InvalidArgument, e.GrantReward("negativeHearts", new Reward(hearts: -1)).Error);
        Assert.Equal(ModuleError.InvalidArgument, e.GrantReward("negativeBooster", new Reward(boosters: Items(-1))).Error);
        Assert.Equal(20, e.Gold); Assert.Equal(3, e.Boosters["hammer"]); Assert.Equal(2, e.Hearts);
    }
    [Fact] public void OverflowOnGoldAndBoosterAddIsRejectedNotWrapped()
    {
        var e = Create(long.MaxValue, int.MaxValue);
        Assert.Equal(ModuleError.Overflow, e.GrantReward("gold", new Reward(1)).Error);
        Assert.Equal(ModuleError.Overflow, e.GrantReward("booster", new Reward(boosters: Items(1))).Error);
        Assert.Equal(long.MaxValue, e.Gold); Assert.Equal(int.MaxValue, e.Boosters["hammer"]);
        Assert.False(e.Claims.Contains("gold")); Assert.False(e.Claims.Contains("booster"));
    }
    [Fact] public void FailedMultiItemSpendAndRewardTransactionsLeaveAllStateUnchanged()
    {
        var e = Create();
        Assert.Equal(ModuleError.Insufficient, e.Spend(10, new Dictionary<string, int> { ["hammer"] = 1, ["missing"] = 1 }).Error);
        Assert.Equal(ModuleError.Insufficient, e.Spend(21, Items(1)).Error);
        Assert.Equal(ModuleError.Overflow, e.GrantReward("bad", new Reward(5, int.MaxValue, Items(1))).Error);
        Assert.Equal(20, e.Gold); Assert.Equal(3, e.Boosters["hammer"]); Assert.Equal(2, e.Hearts); Assert.False(e.Claims.Contains("bad"));
        Assert.True(e.Spend(10, Items(2)).Success); Assert.Equal(10, e.Gold); Assert.Equal(1, e.Boosters["hammer"]);
    }
    [Fact] public void DuplicateClaimIdGrantsOnlyOnceThroughExistingContract()
    {
        var e = Create(); IRewardGrantor grantor = e;
        var reward = new Reward(10, 1, Items(2));
        Assert.Equal(GrantOutcome.Granted, grantor.Grant("claim", reward));
        Assert.Equal(GrantOutcome.AlreadyClaimed, grantor.Grant("claim", reward));
        Assert.Equal(30, e.Gold); Assert.Equal(3, e.Hearts); Assert.Equal(5, e.Boosters["hammer"]);
        Assert.Equal(GrantOutcome.Rejected, grantor.Grant("bad", new Reward(-1)));
    }
    [Fact] public void EconomyAndLivesStateAndClaimLedgerSurviveSaveStoreRoundTrip()
    {
        // In-memory filesystem exercises SaveStore's actual JSON serialization without external writes.
        var files = new MemoryFiles(); var store = new SaveStore(fileSystem: files); var doc = new SaveDocument();
        var c = new TestClock(); var lives = new LivesService(new LivesConfig(5, 2, TimeSpan.FromMinutes(10)), c);
        var e = Create(lives: lives); e.GrantReward("saved", new Reward(10, 1, Items(2)));
        e.Save(doc); lives.Save(doc); doc.WriteSection("other", new { Keep = 42 });
        Assert.True(store.Save("save", doc).Success);
        var restoredDoc = store.Load("save").Value!;
        var restoredLives = new LivesService(new LivesConfig(5, 2, TimeSpan.FromMinutes(10)), c);
        var restored = Create(lives: restoredLives);
        Assert.True(restored.Load(restoredDoc).Success); Assert.True(restoredLives.Load(restoredDoc).Success);
        Assert.Equal(30, restored.Gold); Assert.Equal(5, restored.Boosters["hammer"]); Assert.Equal(3, restored.Hearts);
        Assert.Equal(ModuleError.AlreadyClaimed, restored.GrantReward("saved", new Reward(10)).Error);
        Assert.Equal(42, restoredDoc.ReadSection("other", () => new JObject()).Value["Keep"]!.Value<int>());
        var sections = JObject.Parse(files.Data["save"])["Sections"]!;
        Assert.Equal(1, sections["economy"]!["Version"]); Assert.Equal(1, sections["lives"]!["Version"]);
        Assert.Null(sections["claims"]);
    }
    [Fact] public void CorruptEconomySectionFallsBackToDefaultsWithErrorCode()
    {
        var e = Create(); e.GrantReward("old", new Reward(10)); var doc = new SaveDocument();
        doc.WriteSection("economy", "invalid");
        Assert.Equal(ModuleError.CorruptSection, e.Load(doc).Error); Assert.Equal(20, e.Gold); Assert.Equal(3, e.Boosters["hammer"]);
        Assert.False(e.Claims.Contains("old"));
        doc.WriteSection("economy", new { Version = 1, Gold = -1, Boosters = Items(2), Claims = new[] { "x" } });
        Assert.Equal(ModuleError.CorruptSection, e.Load(doc).Error); Assert.Equal(20, e.Gold);
    }
    [Fact] public void MissingSectionsUseDefaultsAndMalformedOrFutureSectionsReportErrors()
    {
        var e = Create(); var doc = new SaveDocument(); Assert.True(e.Load(doc).Success);
        foreach (var payload in new object[] {
            new { Version = 2, Gold = 10, Boosters = Items(2), Claims = new[] { "x" } },
            new { Version = 1, Gold = "9223372036854775808", Boosters = Items(2), Claims = new[] { "x" } },
            new { Version = 1, Gold = 10, Boosters = new { hammer = "2147483648" }, Claims = new[] { "x" } },
            new { Version = 1, Gold = 10, Boosters = Items(2), Claims = new[] { "x", "x" } },
            new { Version = 1 } })
        {
            doc.WriteSection("economy", payload); Assert.Equal(ModuleError.CorruptSection, e.Load(doc).Error); Assert.Equal(20, e.Gold);
        }
    }
    [Fact] public void ConfigAndExposedBalancesCannotMutateEconomyIndirectly()
    {
        var original = Items(3); var config = new EconomyConfig(20, original);
        var e = new EconomyService(config, new LivesService(new LivesConfig(5, 2, TimeSpan.FromMinutes(10)), new TestClock()));
        original["hammer"] = 999; Assert.Equal(3, e.Boosters["hammer"]);
        Assert.Throws<NotSupportedException>(() => ((IDictionary<string, int>)e.Boosters)["hammer"] = 5);
        e.Load(new SaveDocument()); Assert.Equal(3, e.Boosters["hammer"]);
    }
    sealed class MemoryFiles : ISaveFileSystem
    {
        public Dictionary<string, string> Data { get; } = new();
        public bool Exists(string path) => Data.ContainsKey(path);
        public string ReadAllText(string path) => Data[path];
        public void WriteAllText(string path, string text) => Data[path] = text;
        public void Move(string source, string destination) { Data[destination] = Data[source]; Data.Remove(source); }
        public void Replace(string source, string destination, string? backup)
        { if (backup != null) Data[backup] = Data[destination]; Move(source, destination); }
    }
}
