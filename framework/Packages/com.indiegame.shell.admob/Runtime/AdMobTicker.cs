using System;
using UnityEngine;

namespace IndieGame.Shell.AdMob
{
    sealed class AdMobTicker : MonoBehaviour
    {
        Action tick;

        internal static AdMobTicker Create(Action tick)
        {
            var go = new GameObject("AdMobTicker") { hideFlags = HideFlags.HideAndDontSave };
            if (Application.isPlaying) DontDestroyOnLoad(go);
            var ticker = go.AddComponent<AdMobTicker>();
            ticker.tick = tick;
            return ticker;
        }

        void Update() { tick(); }
    }
}
