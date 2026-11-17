using System;
using Unity.Services.LevelPlay;
using UnityEngine;
using Sdk = Unity.Services.LevelPlay.LevelPlay;

namespace IndieGame.Shell.LevelPlay
{
    /// <summary>Own one instance per SDK session; dispose it on shutdown. A hidden ticker drives
    /// the service retries. Create and all service calls use the Unity main thread.</summary>
    public sealed class LevelPlayAds : IDisposable
    {
        LevelPlayRewardedService service;
        LevelPlayRewardedBackend backend;
        LevelPlayTicker ticker;
        string adUnitId;
        bool disposed;

        public LevelPlayRewardedService Create(LevelPlayAdsSettings s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (disposed) throw new ObjectDisposedException(nameof(LevelPlayAds));
            if (service != null) throw new InvalidOperationException("Create may only be called once per LevelPlayAds instance.");
            s.Validate(!Debug.isDebugBuild && !Application.isEditor);
            adUnitId = s.RewardedAdUnitId;
            service = new LevelPlayRewardedService(() => Time.realtimeSinceStartup);
            ticker = LevelPlayTicker.Create(service.Tick);
            Sdk.OnInitSuccess += OnInitSuccess;
            Sdk.OnInitFailed += OnInitFailed;
            try { Sdk.Init(s.AppKey); }
            catch { Dispose(); throw; }
            return service;
        }
        void UnsubscribeInit()
        {
            Sdk.OnInitSuccess -= OnInitSuccess;
            Sdk.OnInitFailed -= OnInitFailed;
        }
        void OnInitSuccess(LevelPlayConfiguration configuration)
        {
            UnsubscribeInit();
            if (disposed) return;
            backend = new LevelPlayRewardedBackend(adUnitId);
            service.Attach(backend);
            service.Load();
        }
        void OnInitFailed(LevelPlayInitError error)
        {
            Debug.LogError("LevelPlay initialization failed: " + error);
        }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            UnsubscribeInit();
            if (ticker != null) UnityEngine.Object.Destroy(ticker.gameObject);
            try { service?.Dispose(); }
            finally { backend?.Dispose(); }
        }
    }
}
