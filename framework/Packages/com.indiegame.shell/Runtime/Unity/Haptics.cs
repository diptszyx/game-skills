using System;
using UnityEngine;

namespace IndieGame.Shell.Unity
{
    public interface IHapticsPlayer { void Play(); }

    // No engine calls: evaluates the current setting on every request.
    public sealed class HapticsGate : IHapticsPlayer
    {
        readonly SettingsService settings;
        readonly IHapticsPlayer player;
        public HapticsGate(SettingsService settings, IHapticsPlayer player)
        { this.settings = settings ?? throw new ArgumentNullException(nameof(settings)); this.player = player; }
        public void Play() { if (settings.Haptics) player?.Play(); }
    }

    public sealed class UnityHapticsPlayer : IHapticsPlayer
    {
        public void Play()
        {
            if (Application.platform == RuntimePlatform.Android) Handheld.Vibrate();
        }
    }
}
