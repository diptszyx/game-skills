using System;
using UnityEngine;

namespace IndieGame.Shell.LevelPlay
{
    sealed class LevelPlayTicker : MonoBehaviour
    {
        Action tick;

        internal static LevelPlayTicker Create(Action tick)
        {
            var go = new GameObject("LevelPlayTicker") { hideFlags = HideFlags.HideAndDontSave };
            if (Application.isPlaying) DontDestroyOnLoad(go);
            var ticker = go.AddComponent<LevelPlayTicker>();
            ticker.tick = tick;
            return ticker;
        }

        void Update() { tick(); }
    }
}
