using System;
using System.IO;
using IndieGame.Shell;
using NUnit.Framework;
using UnityEngine;

namespace IndieGame.Shell.Tests
{
    public sealed class SaveSmokeTests
    {
        string directory;
        string SavePath => Path.Combine(directory, "save.json");
        [SetUp] public void SetUp()
        {
            directory = Path.Combine(Application.temporaryCachePath, "shell-smoke-" + Guid.NewGuid());
            Directory.CreateDirectory(directory);
        }
        [TearDown] public void TearDown() => Directory.Delete(directory, true);
        void Seed(string fixture, string suffix = "") => File.Copy(
            Path.GetFullPath(Path.Combine(Application.dataPath, "../../fixtures/save", fixture + ".json")),
            SavePath + suffix);
        public sealed class Wallet { public long Coins { get; set; } = 800; }
        [Test] public void ValidV1Loads()
        {
            Seed("valid-v1");
            var result = new SaveStore().Load(SavePath);
            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(result.Value.SchemaVersion, Is.EqualTo(1));
            Assert.That(result.Value.ReadSection("wallet", () => new Wallet()).Value.Coins, Is.EqualTo(long.MaxValue));
        }
        [Test] public void CorruptFallsBackToBackup()
        {
            Seed("corrupt"); Seed("valid-v1", ".bak");
            var result = new SaveStore().Load(SavePath);
            Assert.That(result.Error, Is.EqualTo(SaveError.InvalidData));
            Assert.That(result.Recovered, Is.True);
            Assert.That(result.Reset, Is.False);
            Assert.That(result.Value.ReadSection("wallet", () => new Wallet()).Value.Coins, Is.EqualTo(long.MaxValue));
        }
        [Test] public void FutureReportsVersionError()
        {
            Seed("future");
            var before = File.ReadAllText(SavePath);
            var store = new SaveStore();
            var result = store.Load(SavePath);
            Assert.That(result.Error, Is.EqualTo(SaveError.FutureVersion));
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Reset, Is.False);
            Assert.That(store.Save(SavePath, new SaveDocument()).Error, Is.EqualTo(SaveError.FutureVersion));
            Assert.That(File.ReadAllText(SavePath), Is.EqualTo(before));
        }
    }
}
