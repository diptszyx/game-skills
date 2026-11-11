using System;
using System.Collections.Generic;
using System.IO;
using IndieGame.Shell;
using Xunit;

namespace IndieGame.Shell.Tests;

public sealed class SaveTests : IDisposable
{
    readonly string directory = Path.Combine(Path.GetTempPath(), "shell-tests-" + Guid.NewGuid());
    string PathName => Path.Combine(directory, "save.json");
    public SaveTests() => Directory.CreateDirectory(directory);
    public void Dispose() => Directory.Delete(directory, true);
    static string Fixture(string name) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "fixtures", name + ".json"));
    void Seed(string fixture, string suffix = "") => File.WriteAllText(PathName + suffix, Fixture(fixture));
    static Wallet WalletOf(SaveDocument doc) => doc.ReadSection("wallet", () => new Wallet()).Value;
    public sealed class Wallet
    {
        public long Coins { get; set; } = 800;
        public string Label { get; set; } = "default";
        public decimal Price { get; set; }
    }
    public sealed class Settings { public bool Sound { get; set; } = true; }

    [Fact] public void ReadsValidV1()
    {
        Seed("valid-v1");
        var result = new SaveStore().Load(PathName);
        Assert.True(result.Success, result.Message);
        Assert.Equal(1, result.Value!.SchemaVersion);
        Assert.Equal(long.MaxValue, WalletOf(result.Value).Coins);
    }
    [Fact] public void MissingFieldsAndSectionsUseDefaults()
    {
        Seed("missing-fields");
        var result = new SaveStore().Load(PathName);
        Assert.True(result.Success, result.Message);
        Assert.Equal(1, result.Value!.SchemaVersion);
        Assert.Equal(800, WalletOf(result.Value).Coins);
        Assert.True(result.Value.ReadSection("settings", () => new Settings()).Value.Sound);
    }
    [Fact] public void CorruptPrimaryRecoversBackup()
    {
        Seed("corrupt"); Seed("valid-v1", ".bak");
        var result = new SaveStore().Load(PathName);
        Assert.True(result.Recovered); Assert.False(result.Reset);
        Assert.Equal(SaveError.InvalidData, result.Error); Assert.NotEmpty(result.Message);
        Assert.Equal(long.MaxValue, WalletOf(result.Value!).Coins);
    }
    [Fact] public void BothCorruptResetAndReport()
    {
        Seed("corrupt"); Seed("corrupt", ".bak");
        var result = new SaveStore().Load(PathName);
        Assert.True(result.Reset); Assert.False(result.Recovered);
        Assert.Equal(SaveError.InvalidData, result.Error); Assert.Contains("Reset", result.Message);
        Assert.Equal(800, WalletOf(result.Value!).Coins);
    }
    [Theory] [InlineData("")] [InlineData(".bak")]
    public void FutureVersionRefusesOverwrite(string suffix)
    {
        Seed("valid-v1"); Seed("future", suffix);
        var before = File.ReadAllText(PathName + suffix);
        var store = new SaveStore();
        if (suffix == "")
        {
            var load = store.Load(PathName);
            Assert.Equal(SaveError.FutureVersion, load.Error);
            Assert.Null(load.Value); Assert.False(load.Reset);
        }
        var write = store.Save(PathName, new SaveDocument());
        Assert.False(write.Success); Assert.Equal(SaveError.FutureVersion, write.Error);
        Assert.Equal(before, File.ReadAllText(PathName + suffix));
        Assert.False(File.Exists(PathName + ".tmp"));
    }
    [Fact] public void ArbitrarilyLargeFutureVersionIsNeverTreatedAsCorruption()
    {
        Seed("future-large"); Seed("valid-v1", ".bak"); var store = new SaveStore();
        var load = store.Load(PathName);
        Assert.Equal(SaveError.FutureVersion, load.Error); Assert.False(load.Reset); Assert.False(load.Recovered);
        Assert.Equal(SaveError.FutureVersion, store.Save(PathName, new SaveDocument()).Error);
        Assert.Equal(Fixture("future-large"), File.ReadAllText(PathName));
    }
    [Fact] public void MigrationV1ToV2PreservesOtherSections()
    {
        Seed("valid-v1");
        var store = new SaveStore(2);
        Assert.True(store.RegisterMigration(1, doc =>
        {
            var wallet = WalletOf(doc); wallet.Label = "migrated";
            Assert.True(doc.WriteSection("wallet", wallet).Success);
        }).Success);
        var result = store.Load(PathName);
        Assert.True(result.Success, result.Message);
        Assert.Equal(2, result.Value!.SchemaVersion);
        Assert.Equal("migrated", WalletOf(result.Value).Label);
        Assert.False(result.Value.ReadSection("settings", () => new Settings()).Value.Sound);
        Assert.True(store.Save(PathName, result.Value).Success);
        Assert.Equal(2, store.Load(PathName).Value!.SchemaVersion);
        Assert.Equal(Fixture("valid-v1"), File.ReadAllText(PathName + ".bak"));
    }
    [Fact] public void MissingMigrationIsErrorWithoutReset()
    {
        Seed("valid-v1");
        var result = new SaveStore(2).Load(PathName);
        Assert.Equal(SaveError.Migration, result.Error); Assert.Null(result.Value);
        Assert.False(result.Reset); Assert.Contains("Missing migration", result.Message);
    }
    [Fact] public void ThrowingMigrationReportsErrorAndLeavesDiskUntouched()
    {
        Seed("valid-v1"); var store = new SaveStore(2);
        store.RegisterMigration(1, _ => throw new InvalidOperationException("migration failed"));
        var result = store.Load(PathName);
        Assert.Equal(SaveError.Migration, result.Error); Assert.Contains("migration failed", result.Message);
        Assert.Equal(Fixture("valid-v1"), File.ReadAllText(PathName));
    }
    [Fact] public void InterruptedTempWriteKeepsPrimaryAndBackup()
    {
        var fake = new FakeFiles(); fake.Data["save"] = Fixture("valid-v1"); fake.Data["save.bak"] = Fixture("missing-fields");
        fake.FailAfterTempWrite = true;
        var result = new SaveStore(fileSystem: fake).Save("save", new SaveDocument());
        Assert.Equal(SaveError.Io, result.Error); Assert.Contains("interrupted", result.Message);
        Assert.Equal(Fixture("valid-v1"), fake.Data["save"]);
        Assert.Equal(Fixture("missing-fields"), fake.Data["save.bak"]);
        Assert.True(fake.Data.ContainsKey("save.tmp")); Assert.Equal(0, fake.ReplaceCount);
    }
    [Fact] public void ReplaceFailureKeepsPrimary()
    {
        var fake = new FakeFiles { FailReplace = true }; fake.Data["save"] = Fixture("valid-v1");
        var result = new SaveStore(fileSystem: fake).Save("save", new SaveDocument());
        Assert.Equal(SaveError.Io, result.Error); Assert.Contains("replace failed", result.Message);
        Assert.Equal(Fixture("valid-v1"), fake.Data["save"]);
    }
    [Fact] public void IndependentSectionsAndUnknownGameSectionSurvive()
    {
        Seed("valid-v1"); var store = new SaveStore(); var doc = store.Load(PathName).Value!;
        Assert.True(doc.WriteSection("wallet", new Wallet { Coins = 123 }).Success);
        Assert.True(store.Save(PathName, doc).Success);
        var loaded = store.Load(PathName).Value!;
        Assert.Equal(123, WalletOf(loaded).Coins);
        Assert.False(loaded.ReadSection("settings", () => new Settings()).Value.Sound);
        Assert.Equal(42, loaded.ReadSection("game.custom", () => new Dictionary<string, int>()).Value["stage"]);
    }
    [Fact] public void RoundTripPreservesLargeNumbersAndUnicode()
    {
        Seed("valid-v1"); var store = new SaveStore(); var original = store.Load(PathName).Value!;
        Assert.True(store.Save(PathName, original).Success);
        var wallet = WalletOf(store.Load(PathName).Value!);
        Assert.Equal(long.MaxValue, wallet.Coins);
        Assert.Equal(12345678901234567890.123456789m, wallet.Price);
        Assert.Equal("Tiếng Việt 🎮 漢字", wallet.Label);
    }
    [Fact] public void FirstWriteThenReplaceCreatesBackupOfPreviousSave()
    {
        var store = new SaveStore(); var doc = new SaveDocument();
        doc.WriteSection("wallet", new Wallet { Coins = 1 });
        Assert.True(store.Save(PathName, doc).Success); var previous = File.ReadAllText(PathName);
        doc.WriteSection("wallet", new Wallet { Coins = 2 });
        Assert.True(store.Save(PathName, doc).Success);
        Assert.Equal(previous, File.ReadAllText(PathName + ".bak"));
        Assert.Equal(2, WalletOf(store.Load(PathName).Value!).Coins);
    }
    [Fact] public void SavingRecoveredDocumentPreservesGoodBackup()
    {
        Seed("corrupt"); Seed("valid-v1", ".bak"); var store = new SaveStore();
        var loaded = store.Load(PathName);
        Assert.True(store.Save(PathName, loaded.Value!).Success);
        Assert.Equal(Fixture("valid-v1"), File.ReadAllText(PathName + ".bak"));
        Assert.True(store.Load(PathName).Success);
    }
    [Fact] public void NewPathReturnsEmptyWithoutRecoveryFlags()
    {
        var result = new SaveStore().Load(PathName);
        Assert.True(result.Success); Assert.False(result.Reset); Assert.False(result.Recovered);
        Assert.NotNull(result.Value);
    }
    [Fact] public void MissingPrimaryRecoversBackup()
    {
        Seed("valid-v1", ".bak"); var result = new SaveStore().Load(PathName);
        Assert.True(result.Recovered); Assert.Equal(long.MaxValue, WalletOf(result.Value!).Coins);
    }
    [Fact] public void InvalidSectionReportsConversionError()
    {
        Seed("invalid-section"); var doc = new SaveStore().Load(PathName).Value!;
        var result = doc.ReadSection("wallet", () => new Wallet());
        Assert.Equal(SaveError.InvalidData, result.Error); Assert.Contains("wallet", result.Message);
    }
    [Fact] public void ReadIoFailureIsNotResetOrOverwritePermission()
    {
        var fake = new FakeFiles { FailRead = true }; fake.Data["save"] = Fixture("valid-v1");
        var store = new SaveStore(fileSystem: fake);
        var load = store.Load("save");
        Assert.Equal(SaveError.Io, load.Error); Assert.False(load.Reset); Assert.Null(load.Value);
        Assert.Equal(SaveError.Io, store.Save("save", new SaveDocument()).Error);
        Assert.Equal(Fixture("valid-v1"), fake.Data["save"]);
    }
    [Fact] public void InvalidArgumentsAndDocumentVersionReturnErrors()
    {
        var store = new SaveStore();
        Assert.Equal(SaveError.InvalidArgument, store.Load("").Error);
        Assert.Equal(SaveError.InvalidArgument, store.Save("", new SaveDocument()).Error);
        Assert.Equal(SaveError.FutureVersion, store.Save(PathName, new SaveDocument(2)).Error);
        Assert.Equal(SaveError.InvalidArgument, store.RegisterMigration(1, _ => { }).Error);
        Assert.Equal(SaveError.InvalidArgument, new SaveDocument().WriteSection("", 1).Error);
    }
    sealed class FakeFiles : ISaveFileSystem
    {
        public Dictionary<string, string> Data { get; } = new();
        public bool FailAfterTempWrite, FailReplace, FailRead;
        public int ReplaceCount;
        public bool Exists(string path) => Data.ContainsKey(path);
        public string ReadAllText(string path) => FailRead ? throw new IOException("read failed") : Data[path];
        public void WriteAllText(string path, string text)
        { Data[path] = text; if (FailAfterTempWrite) throw new IOException("interrupted after temp write"); }
        public void Replace(string source, string destination, string? backup)
        {
            ReplaceCount++;
            if (FailReplace) throw new IOException("replace failed");
            if (backup != null) Data[backup] = Data[destination];
            Data[destination] = Data[source]; Data.Remove(source);
        }
        public void Move(string source, string destination)
        { Data.Add(destination, Data[source]); Data.Remove(source); }
    }
}
