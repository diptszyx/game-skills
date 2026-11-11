using System;
using IndieGame.Shell.Unity;
using NUnit.Framework;
using UnityEngine;

namespace IndieGame.Shell.Tests
{
    public sealed class UnityAdapterTests
    {
        static SettingsService Settings() => new SettingsService(new SettingsConfig(true, true, true, "en", new[] { "en" }));
        sealed class Clock : IClock
        {
            public DateTime UtcNow { get; private set; } = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            public void Advance(double seconds) { UtcNow = UtcNow.AddSeconds(seconds); }
        }
        sealed class Haptics : IHapticsPlayer { public int Calls; public void Play() { Calls++; } }
        sealed class Pool : IAudioSourcePool
        {
            public int Calls;
            public string Key;
            public bool Music;
            public float Volume, SoundGain, MusicGain;
            public bool Play(string key, bool music, float volume)
            { Calls++; Key = key; Music = music; Volume = volume; return key != "missing"; }
            public void SetVolumes(float sound, float music) { SoundGain = sound; MusicGain = music; }
            public void StopAll() { Calls = 0; }
        }
        static void Anchors(Rect safe, Rect expected)
        {
            var result = SafeAreaMath.ToAnchors(safe, new Vector2(100, 200));
            Assert.That(result.x, Is.EqualTo(expected.x).Within(0.00001));
            Assert.That(result.y, Is.EqualTo(expected.y).Within(0.00001));
            Assert.That(result.width, Is.EqualTo(expected.width).Within(0.00001));
            Assert.That(result.height, Is.EqualTo(expected.height).Within(0.00001));
        }
        [Test] public void SafeAreaTopNotch() => Anchors(new Rect(0, 0, 100, 180), new Rect(0, 0, 1, 0.9f));
        [Test] public void SafeAreaBottomGestureBar() => Anchors(new Rect(0, 20, 100, 180), new Rect(0, 0.1f, 1, 0.9f));
        [Test] public void SafeAreaZero()
        {
            Anchors(new Rect(), new Rect(0, 0, 1, 1));
            Assert.That(SafeAreaMath.ToAnchors(new Rect(0, 0, 10, 10), Vector2.zero), Is.EqualTo(new Rect(0, 0, 1, 1)));
        }
        [Test] public void SafeAreaFullScreen() => Anchors(new Rect(0, 0, 100, 200), new Rect(0, 0, 1, 1));
        [Test] public void LoadingMinimumDisplayTime()
        {
            var clock = new Clock(); var c = new LoadingScreenController(clock, 2, 0);
            c.Show(); clock.Advance(2); c.Hide();
            Assert.That(c.State, Is.EqualTo(LoadingScreenState.Hidden));
            Assert.That(c.Alpha, Is.Zero);
        }
        [Test] public void LoadingProgressMonotonic()
        {
            var c = new LoadingScreenController(new Clock()); c.Show();
            c.SetProgress(0.7f); c.SetProgress(0.2f); c.SetProgress(float.NaN);
            Assert.That(c.Progress, Is.EqualTo(0.7f));
        }
        [Test] public void LoadingHideBeforeMinimumWaits()
        {
            var clock = new Clock(); var c = new LoadingScreenController(clock, 2, 0);
            c.Show(); c.Hide(); clock.Advance(1.99); c.Tick();
            Assert.That(c.State, Is.EqualTo(LoadingScreenState.WaitingToHide));
            Assert.That(c.Alpha, Is.EqualTo(1));
            clock.Advance(0.01); c.Tick(); Assert.That(c.State, Is.EqualTo(LoadingScreenState.Hidden));
        }
        [Test] public void LoadingDoubleShowIdempotent()
        {
            var clock = new Clock(); var c = new LoadingScreenController(clock, 2, 0);
            c.Show(); c.SetProgress(0.6f); clock.Advance(1); c.Show();
            Assert.That(c.Progress, Is.EqualTo(0.6f));
            c.Hide(); c.Show(); clock.Advance(1); c.Tick();
            Assert.That(c.State, Is.EqualTo(LoadingScreenState.Hidden));
        }
        [Test] public void LoadingProgressClamp()
        {
            var c = new LoadingScreenController(new Clock()); c.Show(); c.SetProgress(-2);
            Assert.That(c.Progress, Is.Zero); c.SetProgress(2); Assert.That(c.Progress, Is.EqualTo(1));
        }
        [Test] public void LoadingFadeAndLateHide()
        {
            var clock = new Clock(); var c = new LoadingScreenController(clock, 1, 0.5);
            c.Show(); Assert.That(c.Alpha, Is.Zero);
            clock.Advance(0.25); c.Tick(); Assert.That(c.Alpha, Is.EqualTo(0.5f));
            clock.Advance(5); c.Tick(); Assert.That(c.State, Is.EqualTo(LoadingScreenState.Visible));
            c.Hide(); Assert.That(c.Alpha, Is.EqualTo(1));
            clock.Advance(0.25); c.Tick(); Assert.That(c.Alpha, Is.EqualTo(0.5f));
            c.Hide(); clock.Advance(0.25); c.Tick();
            Assert.That(c.State, Is.EqualTo(LoadingScreenState.Hidden));
        }
        [Test] public void LoadingNewCycleResetsAndEarlyHideFades()
        {
            var clock = new Clock(); var c = new LoadingScreenController(clock, 1, 0.5);
            c.Hide(); Assert.That(c.State, Is.EqualTo(LoadingScreenState.Hidden));
            c.Show(); c.SetProgress(1); c.Hide(); clock.Advance(1.25); c.Tick();
            Assert.That(c.State, Is.EqualTo(LoadingScreenState.FadingOut));
            Assert.That(c.Alpha, Is.EqualTo(0.5f));
            clock.Advance(0.25); c.Tick(); c.Show();
            Assert.That(c.Progress, Is.Zero); Assert.That(c.State, Is.EqualTo(LoadingScreenState.FadingIn));
        }
        [Test] public void HapticsFlagOff()
        {
            var settings = Settings(); var player = new Haptics(); var gate = new HapticsGate(settings, player);
            settings.SetHaptics(false); gate.Play(); Assert.That(player.Calls, Is.Zero);
        }
        [Test] public void HapticsFlagOn()
        {
            var settings = Settings(); var player = new Haptics(); var gate = new HapticsGate(settings, player);
            settings.SetHaptics(false); gate.Play(); settings.SetHaptics(true); gate.Play();
            Assert.That(player.Calls, Is.EqualTo(1));
        }
        [Test] public void HapticsNullPlayerSafe() => Assert.DoesNotThrow(() => new HapticsGate(Settings(), null).Play());
        [Test] public void AudioRoutesKeysAndIndependentVolumes()
        {
            var pool = new Pool(); IAudioService service = new AudioService(Settings(), pool);
            service.SoundVolume = 0.3f; service.MusicVolume = 0.8f;
            Assert.That(service.PlaySound("tap"), Is.True); Assert.That(pool.Key, Is.EqualTo("tap"));
            Assert.That(pool.Music, Is.False); Assert.That(pool.Volume, Is.EqualTo(0.3f));
            Assert.That(service.PlayMusic("theme"), Is.True); Assert.That(pool.Key, Is.EqualTo("theme"));
            Assert.That(pool.Music, Is.True); Assert.That(pool.Volume, Is.EqualTo(0.8f));
            Assert.That(service.PlaySound("missing"), Is.False);
            var calls = pool.Calls; Assert.That(service.PlaySound(null), Is.False); Assert.That(pool.Calls, Is.EqualTo(calls));
            service.SoundVolume = -1; service.MusicVolume = 2;
            Assert.That(pool.SoundGain, Is.Zero); Assert.That(pool.MusicGain, Is.EqualTo(1));
            service.StopAll(); Assert.That(pool.Calls, Is.Zero);
        }
        [Test] public void AudioFlagsGatePoolAndRefreshActiveVolumes()
        {
            var settings = Settings(); var pool = new Pool(); IAudioService service = new AudioService(settings, pool);
            service.PlaySound("tap"); service.PlayMusic("theme"); settings.SetSound(false); service.RefreshSettings();
            Assert.That(pool.SoundGain, Is.Zero); Assert.That(pool.MusicGain, Is.EqualTo(1));
            Assert.That(service.PlaySound("tap"), Is.False); Assert.That(pool.Calls, Is.EqualTo(2));
            Assert.That(service.PlayMusic("theme"), Is.True);
            settings.SetMusic(false); service.RefreshSettings(); Assert.That(pool.MusicGain, Is.Zero);
            Assert.That(service.PlayMusic("theme"), Is.False); Assert.That(pool.Calls, Is.EqualTo(3));
            settings.SetSound(true); Assert.That(service.PlaySound("tap"), Is.True);
            Assert.That(pool.SoundGain, Is.EqualTo(1)); Assert.That(pool.MusicGain, Is.Zero);
        }
    }
}
