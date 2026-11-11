using System.Collections.Generic;
using IndieGame.Shell;
using Newtonsoft.Json.Linq;
using Xunit;

namespace IndieGame.Shell.Tests;

public sealed class ConfigTests
{
    static RemoteConfigService Create() => new(new RemoteConfigRules(new RemoteConfig(10, 60), 1000, 5, 3600));
    const string Valid = "{\"RewardGold\":25,\"RefreshSeconds\":30}";
    [Fact] public void ValidRemoteConfigParsesTypedValuesAndReportsRemoteSource()
    {
        var c = Create(); var r = c.Apply(Valid); Assert.True(r.Success); Assert.Equal(ConfigSource.Remote, r.Source);
        Assert.Equal(25, r.Value.RewardGold); Assert.Equal(30, r.Value.RefreshSeconds); Assert.Same(r.Value, c.Current);
    }
    [Theory]
    [InlineData("{")]
    [InlineData("[]")]
    [InlineData("null")]
    public void MalformedJsonFallsBackToDefaultsThenLastValidCache(string json) => CheckFallback(json);
    [Theory]
    [InlineData("{\"RewardGold\":-1,\"RefreshSeconds\":30}")]
    [InlineData("{\"RewardGold\":1001,\"RefreshSeconds\":30}")]
    [InlineData("{\"RewardGold\":25,\"RefreshSeconds\":4}")]
    [InlineData("{\"RewardGold\":25,\"RefreshSeconds\":3601}")]
    public void OutOfRangeValuesFallBackWithoutChangingCachedConfig(string json) => CheckFallback(json);
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void EmptyOrMissingRemoteConfigFallsBackToCacheThenDefaults(string? json) => CheckFallback(json!);
    [Theory]
    [InlineData("{}")]
    [InlineData("{\"RewardGold\":25}")]
    [InlineData("{\"RewardGold\":\"25\",\"RefreshSeconds\":30}")]
    [InlineData("{\"RewardGold\":25.1,\"RefreshSeconds\":30}")]
    public void RequiredKeysAndIntegerTypesAreValidated(string json) => CheckFallback(json);
    [Theory]
    [InlineData("{\"RewardGold\":9223372036854775808,\"RefreshSeconds\":30}")]
    [InlineData("{\"RewardGold\":25,\"RefreshSeconds\":2147483648}")]
    [InlineData("{\"RewardGold\":99999999999999999999999999999999999,\"RefreshSeconds\":30}")]
    public void OverflowBeyondLongOrIntIsRejectedWithoutChangingState(string json) => CheckFallback(json);
    static void CheckFallback(string json)
    {
        var c = Create(); var first = c.Apply(json); Assert.False(first.Success); Assert.Equal(ConfigSource.Defaults, first.Source);
        Assert.Equal(10, first.Value.RewardGold); Assert.Equal(60, first.Value.RefreshSeconds);
        var valid = c.Apply(Valid).Value; var cached = c.Apply(json); Assert.False(cached.Success); Assert.Equal(ConfigSource.Cache, cached.Source);
        Assert.Same(valid, cached.Value); Assert.Same(valid, c.Current);
    }
    [Fact] public void UnknownExtraKeysAreIgnoredAndValidRemoteWins()
    {
        var r = Create().Apply("{\"RewardGold\":25,\"RefreshSeconds\":30,\"future\":{\"any\":true}}");
        Assert.True(r.Success); Assert.Equal(ConfigSource.Remote, r.Source); Assert.Equal(25, r.Value.RewardGold);
    }
    [Fact] public void LastValidCachePersistsInOneVersionedSectionAndSurvivesReload()
    {
        var c = Create(); c.Apply(Valid); var d = new SaveDocument(); d.WriteSection("other", new { Keep = 42 });
        Assert.True(c.Save(d).Success); var copy = Create(); Assert.True(copy.Load(d).Success);
        var r = copy.Apply(""); Assert.Equal(ConfigSource.Cache, r.Source); Assert.Equal(25, r.Value.RewardGold);
        var s = d.ReadSection("config", () => new JObject()).Value; Assert.Equal(1, s["Version"]); Assert.Equal(2, s.Count);
        Assert.Equal(42, d.ReadSection("other", () => new JObject()).Value["Keep"]);
    }
    [Fact] public void MissingCacheUsesDefaultsAndInvalidOrFutureCacheLeavesLastValidStateUntouched()
    {
        var c = Create(); var d = new SaveDocument(); Assert.True(c.Load(d).Success); Assert.Equal(ConfigSource.Defaults, c.Apply("").Source);
        var valid = c.Apply(Valid).Value;
        foreach (var payload in new object[] { "bad", new { Version = 2 }, new { Version = 1, Cached = new { RewardGold = -1, RefreshSeconds = 30 } } })
        { d.WriteSection("config", payload); Assert.False(c.Load(d).Success); Assert.Same(valid, c.Current); }
        Assert.False(c.Load(null!).Success); Assert.False(c.Save(null!).Success);
    }
    [Fact] public void ConfiguredBoundsAcceptLongAndIntMaximaWithoutOverflow()
    {
        var c = new RemoteConfigService(new RemoteConfigRules(new RemoteConfig(0, 0), long.MaxValue, 0, int.MaxValue));
        var r = c.Apply("{\"RewardGold\":9223372036854775807,\"RefreshSeconds\":2147483647}");
        Assert.True(r.Success); Assert.Equal(long.MaxValue, r.Value.RewardGold); Assert.Equal(int.MaxValue, r.Value.RefreshSeconds);
    }
    [Fact] public void FailedConfigSourceCannotReplaceValidCacheEvenWhenFailureContainsValidJson()
    {
        var c = Create(); var f = new FakeConfigSource();
        f.Enqueue(Valid); Assert.Equal(ConfigSource.Remote, c.Refresh(f).Source); var cached = c.Current;
        f.Enqueue("{\"RewardGold\":99,\"RefreshSeconds\":90}", ServiceError.Failed);
        var failed = c.Refresh(f); Assert.False(failed.Success); Assert.Equal(ConfigSource.Cache, failed.Source); Assert.Same(cached, c.Current);
        Assert.Equal(ConfigSource.Cache, c.Refresh(f).Source); Assert.Same(cached, c.Current);
        Assert.False(c.Refresh(null!).Success); Assert.Same(cached, c.Current);
    }
    [Fact] public void SettingsAndConfigRoundTripThroughSaveStoreAsIndependentVersionedSections()
    {
        var files = new MemoryFiles(); var store = new SaveStore(fileSystem: files); var d = new SaveDocument();
        var settings = new SettingsService(new SettingsConfig(true, true, true, "en", new[] { "en", "vi" }));
        settings.SetSound(false); settings.SetMusic(false); settings.SetHaptics(false); settings.SetLanguage("vi");
        var c = Create(); c.Apply(Valid); settings.Save(d); c.Save(d); d.WriteSection("game", new { Keep = 7 });
        Assert.True(store.Save("save", d).Success); var loaded = store.Load("save"); Assert.True(loaded.Success);
        var restored = new SettingsService(new SettingsConfig(true, true, true, "en", new[] { "en", "vi" }));
        var config = Create(); Assert.True(restored.Load(loaded.Value!).Success); Assert.True(config.Load(loaded.Value!).Success);
        Assert.False(restored.Sound); Assert.False(restored.Music); Assert.False(restored.Haptics); Assert.Equal("vi", restored.Language);
        Assert.Equal(ConfigSource.Cache, config.Apply("").Source); Assert.Equal(25, config.Current.RewardGold);
        var sections = JObject.Parse(files.Data["save"])["Sections"]!; Assert.Equal(3, ((JObject)sections).Count);
        Assert.Equal(1, sections["settings"]!["Version"]); Assert.Equal(1, sections["config"]!["Version"]); Assert.Equal(7, sections["game"]!["Keep"]);
    }
    [Fact] public void SavingWithoutValidRemoteConfigDoesNotCreateAValidCache()
    {
        var c = Create(); var d = new SaveDocument(); Assert.True(c.Save(d).Success);
        var copy = Create(); copy.Apply(Valid); Assert.True(copy.Load(d).Success); Assert.Equal(ConfigSource.Defaults, copy.Apply("").Source);
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
