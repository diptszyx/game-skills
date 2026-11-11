using System;
using System.Collections.Generic;
using UnityEngine;

namespace IndieGame.Shell.Unity
{
    public interface IAudioService
    {
        float SoundVolume { get; set; }
        float MusicVolume { get; set; }
        bool PlaySound(string key);
        bool PlayMusic(string key);
        void RefreshSettings();
        void StopAll();
    }

    public interface IAudioSourcePool
    {
        bool Play(string key, bool music, float volume);
        void SetVolumes(float sound, float music);
        void StopAll();
    }

    public sealed class AudioService : IAudioService
    {
        readonly SettingsService settings;
        readonly IAudioSourcePool pool;
        float soundVolume = 1, musicVolume = 1;
        public AudioService(SettingsService settings, IAudioSourcePool pool)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.pool = pool ?? throw new ArgumentNullException(nameof(pool));
            RefreshSettings();
        }
        public float SoundVolume { get => soundVolume; set { soundVolume = Volume(value); RefreshSettings(); } }
        public float MusicVolume { get => musicVolume; set { musicVolume = Volume(value); RefreshSettings(); } }
        static float Volume(float value) => float.IsNaN(value) ? 0 : Math.Max(0, Math.Min(1, value));
        public bool PlaySound(string key) => Play(key, false);
        public bool PlayMusic(string key) => Play(key, true);
        bool Play(string key, bool music)
        {
            RefreshSettings();
            if (string.IsNullOrWhiteSpace(key) || !(music ? settings.Music : settings.Sound)) return false;
            return pool.Play(key, music, music ? musicVolume : soundVolume);
        }
        // Call after settings changes (or each frame) to also mute already-playing clips.
        public void RefreshSettings() => pool.SetVolumes(settings.Sound ? soundVolume : 0, settings.Music ? musicVolume : 0);
        public void StopAll() => pool.StopAll();
    }

    // Bounded sound pool plus one dedicated looping music source. Owned child object is disposed by the caller.
    public sealed class UnityAudioSourcePool : IAudioSourcePool, IDisposable
    {
        readonly GameObject root;
        readonly Dictionary<string, AudioClip> clips;
        readonly AudioSource[] sounds;
        readonly AudioSource musicSource;
        int next;
        public UnityAudioSourcePool(Transform parent, IDictionary<string, AudioClip> clips, int soundCapacity = 8)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (clips == null) throw new ArgumentNullException(nameof(clips));
            if (soundCapacity < 1) throw new ArgumentOutOfRangeException(nameof(soundCapacity));
            this.clips = new Dictionary<string, AudioClip>(clips, StringComparer.Ordinal);
            root = new GameObject("Shell Audio Pool"); root.transform.SetParent(parent, false);
            sounds = new AudioSource[soundCapacity];
            for (var i = 0; i < sounds.Length; i++) sounds[i] = CreateSource();
            musicSource = CreateSource(); musicSource.loop = true;
        }
        AudioSource CreateSource()
        {
            var source = root.AddComponent<AudioSource>(); source.playOnAwake = false; source.spatialBlend = 0;
            return source;
        }
        public bool Play(string key, bool music, float volume)
        {
            if (root == null || key == null || !clips.TryGetValue(key, out var clip) || clip == null) return false;
            var source = musicSource;
            if (!music)
            {
                source = null;
                foreach (var candidate in sounds) if (!candidate.isPlaying) { source = candidate; break; }
                if (source == null) { source = sounds[next]; next = (next + 1) % sounds.Length; }
            }
            source.Stop(); source.clip = clip; source.volume = Mathf.Clamp01(volume); source.Play();
            return true;
        }
        public void SetVolumes(float sound, float music)
        {
            if (root == null) return;
            foreach (var source in sounds) source.volume = Mathf.Clamp01(sound);
            musicSource.volume = Mathf.Clamp01(music);
        }
        public void StopAll()
        {
            if (root == null) return;
            foreach (var source in sounds) source.Stop();
            musicSource.Stop();
        }
        public void Dispose()
        {
            if (root == null) return;
            StopAll();
            if (Application.isPlaying) UnityEngine.Object.Destroy(root);
            else UnityEngine.Object.DestroyImmediate(root);
        }
    }
}
