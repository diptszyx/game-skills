using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IndieGame.Shell;
using Xunit;

namespace IndieGame.Shell.Tests;

public sealed class SessionTests
{
    sealed class Files : ISaveFileSystem
    {
        public readonly Dictionary<string, string> Data = new();
        public string Fail = "";
        public int Writes, Replacements, Moves, Reads;
        public bool Exists(string path) => Data.ContainsKey(path);
        public string ReadAllText(string path) { Reads++; return Data[path]; }
        void Check(string operation) { if (Fail == operation) throw new IOException("Injected " + operation); }
        public void WriteAllText(string path, string text) { Writes++; Check("write"); Data[path] = text; }
        public void Replace(string source, string destination, string? backup)
        {
            Replacements++; Check("replace");
            if (backup != null) Data[backup] = Data[destination];
            Data[destination] = Data[source]; Data.Remove(source);
        }
        public void Move(string source, string destination)
        { Moves++; Check("move"); Data[destination] = Data[source]; Data.Remove(source); }
    }
    readonly Files files = new();
    readonly DailyFakeClock clock = new();
    ShellSession Create(bool invalidCheckIn = false) => new(new SaveStore(fileSystem: files), "session.json", clock,
        new LivesConfig(5, 1, TimeSpan.FromMinutes(30)), new EconomyConfig(10),
        invalidCheckIn ? new DailyCheckInConfig(Array.Empty<IReadOnlyList<Reward>>()) : CheckInConfig(),
        new DailyQuestsConfig(new[] { new QuestTemplate("win", "win", 2, new Reward(3)) }, 1),
        new SettingsConfig(true, true, true, "en", new[] { "en", "vi" }),
        new RemoteConfigRules(new RemoteConfig(2, 60), 1000, 1, 3600));
    static DailyCheckInConfig CheckInConfig()
    {
        var row = Enumerable.Repeat(new Reward(25, 2, new Dictionary<string, int> { ["bomb"] = 1 }), 5).ToArray();
        return new DailyCheckInConfig(new[] { row, row });
    }
    static void AssertUnclaimed(ShellSession s)
    {
        Assert.Equal(10, s.Economy.Gold); Assert.Empty(s.Economy.Boosters);
        Assert.Equal(1, s.Lives.Hearts); Assert.False(s.Economy.Claims.Contains("checkin:2026-11-10"));
        Assert.Equal(0, s.DailyCheckIn.NextSlot); Assert.True(s.DailyCheckIn.Preview().CanClaimNow);
    }
    static void AssertClaimed(ShellSession s)
    {
        Assert.Equal(35, s.Economy.Gold); Assert.Equal(1, s.Economy.Boosters["bomb"]);
        Assert.Equal(3, s.Lives.Hearts); Assert.True(s.Economy.Claims.Contains("checkin:2026-11-10"));
        Assert.Equal(1, s.DailyCheckIn.NextSlot); Assert.False(s.DailyCheckIn.Preview().CanClaimNow);
        Assert.Equal(DailyError.AlreadyClaimed, s.DailyCheckIn.Claim().Error);
    }
    [Fact] public void LoadAllMissingFileResetsAllModulesToDefaults()
    {
        var s = Create(); s.DailyCheckIn.Claim(); s.DailyQuests.RecordEvent("win", 1);
        s.Settings.SetSound(false); s.Config.Apply("{\"RewardGold\":9,\"RefreshSeconds\":10}");
        var result = s.LoadAll(); Assert.True(result.Success); Assert.False(result.Reset);
        AssertUnclaimed(s); Assert.Empty(s.DailyQuests.Quests); Assert.True(s.Settings.Sound); Assert.Equal(2, s.Config.Current.RewardGold);
        Assert.Empty(files.Data);
    }
    [Fact] public void SuccessfulCommitPersistsClaimRewardLedgerAndEveryModuleWithExactlyOneSave()
    {
        var s = Create(); Assert.True(s.LoadAll().Success); Assert.True(s.DailyCheckIn.Claim().Success);
        s.DailyQuests.RecordEvent("win", 1); s.Settings.SetSound(false); s.Settings.SetLanguage("vi");
        s.Config.Apply("{\"RewardGold\":9,\"RefreshSeconds\":10}");
        Assert.True(s.Commit().Success); Assert.Equal(1, files.Writes); Assert.Equal(1, files.Moves); Assert.Equal(0, files.Replacements);
        var restored = Create(); int reads = files.Reads; Assert.True(restored.LoadAll().Success); Assert.Equal(reads + 1, files.Reads);
        AssertClaimed(restored); Assert.Equal(1, restored.DailyQuests.Quests.Single().Progress);
        Assert.False(restored.Settings.Sound); Assert.Equal("vi", restored.Settings.Language); Assert.Equal(9, restored.Config.Current.RewardGold);
        Assert.Equal(ConfigSource.Cache, restored.Config.Apply("bad").Source);
    }
    [Theory] [InlineData("write", true)] [InlineData("replace", true)] [InlineData("move", false)]
    public void FailedCommitAfterClaimPersistsNeitherRewardNorClaimAndKeepsMemoryForRetry(string operation, bool existing)
    {
        var s = Create(); if (existing) Assert.True(s.Commit().Success);
        Assert.True(s.DailyCheckIn.Claim().Success); files.Fail = operation;
        var result = s.Commit(); Assert.False(result.Success); Assert.Equal(SaveError.Io, result.Error); Assert.Contains("Injected", result.Message);
        AssertClaimed(s);
        var restored = Create(); Assert.True(restored.LoadAll().Success); AssertUnclaimed(restored);
        files.Fail = ""; int writes = files.Writes; Assert.True(s.Commit().Success); Assert.Equal(writes + 1, files.Writes);
        restored = Create(); Assert.True(restored.LoadAll().Success); AssertClaimed(restored);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public void CrashBetweenClaimAndCommitLosesRewardAndClaimTogether(bool existing)
    {
        var s = Create(); if (existing) s.Commit(); Assert.True(s.DailyCheckIn.Claim().Success);
        var restored = Create(); Assert.True(restored.LoadAll().Success); AssertUnclaimed(restored);
        Assert.True(restored.DailyCheckIn.Claim().Success); Assert.True(restored.Commit().Success);
        var again = Create(); again.LoadAll(); AssertClaimed(again);
    }
    [Fact] public void ExplicitLoadAllAfterCommitFailureDiscardsAllPendingChanges()
    {
        var s = Create(); s.Commit(); s.DailyCheckIn.Claim(); files.Fail = "replace";
        Assert.False(s.Commit().Success); Assert.True(s.LoadAll().Success); AssertUnclaimed(s);
    }
    [Theory]
    [InlineData("economy")] [InlineData("lives")] [InlineData("dailyCheckIn")]
    [InlineData("dailyQuests")] [InlineData("settings")] [InlineData("config")]
    public void CorruptSectionResetsOnlyThatModuleAndReportsItsNameEvenOnReusedSession(string section)
    {
        var s = Create(); s.DailyCheckIn.Claim(); s.DailyQuests.RecordEvent("win", 1);
        s.Settings.SetSound(false); s.Config.Apply("{\"RewardGold\":9,\"RefreshSeconds\":10}"); s.Commit();
        var store = new SaveStore(fileSystem: files); var doc = store.Load("session.json").Value!;
        doc.WriteSection(section, "corrupt"); Assert.True(store.Save("session.json", doc).Success);
        var result = s.LoadAll(); Assert.False(result.Success); Assert.Equal(SaveError.None, result.Error);
        Assert.Equal(section, Assert.Single(result.ModuleErrors).Key);
        Assert.Equal(section == "economy" ? 10 : 35, s.Economy.Gold);
        Assert.Equal(section != "economy", s.Economy.Claims.Contains("checkin:2026-11-10"));
        Assert.Equal(section == "lives" ? 1 : 3, s.Lives.Hearts);
        Assert.Equal(section == "dailyCheckIn" ? 0 : 1, s.DailyCheckIn.NextSlot);
        Assert.Equal(section == "dailyQuests" ? 0 : 1, s.DailyQuests.Quests.Count);
        Assert.Equal(section == "settings", s.Settings.Sound);
        Assert.Equal(section == "config" ? 2 : 9, s.Config.Current.RewardGold);
    }
    [Fact] public void CorruptPrimaryFallsBackToBackupAndReportsRecoveryWithoutSplittingClaim()
    {
        var s = Create(); s.Commit(); s.DailyCheckIn.Claim(); s.Commit();
        files.Data["session.json"] = "corrupt";
        var restored = Create(); var result = restored.LoadAll();
        Assert.False(result.Success); Assert.True(result.Recovered); Assert.False(result.Reset);
        Assert.Equal(SaveError.InvalidData, result.Error); Assert.Contains("backup", result.Message);
        AssertUnclaimed(restored); Assert.True(restored.Commit().Success);
        Assert.True(Create().LoadAll().Success);
    }
    [Fact] public void InvalidModuleConfigPreventsAnyPartialDiskCommit()
    {
        var s = Create(true); s.Economy.GrantReward("manual", new Reward(5));
        var result = s.Commit(); Assert.False(result.Success); Assert.Equal("InvalidConfig", result.ModuleErrors["dailyCheckIn"]);
        Assert.Equal(0, files.Writes); Assert.Empty(files.Data);
    }
    [Fact] public void FutureVersionLoadAndCommitAreRejectedWithoutOverwritingDisk()
    {
        files.Data["session.json"] = "{\"SchemaVersion\":99,\"Sections\":{}}"; var s = Create();
        Assert.Equal(SaveError.FutureVersion, s.LoadAll().Error); s.DailyCheckIn.Claim();
        Assert.Equal(SaveError.FutureVersion, s.Commit().Error); Assert.Equal(0, files.Writes);
    }
    [Fact] public void UnownedSectionsSurviveLoadAndCommit()
    {
        var store = new SaveStore(fileSystem: files); var doc = new SaveDocument(); doc.WriteSection("game", 42); store.Save("session.json", doc);
        var s = Create(); s.LoadAll(); s.DailyCheckIn.Claim(); s.Commit();
        Assert.Equal(42, store.Load("session.json").Value!.ReadSection("game", () => 0).Value);
    }
}
