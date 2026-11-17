using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

namespace IndieGame.Shell.AdMob
{
    /// <summary>Own one instance per SDK session; dispose it on shutdown.
    /// Create and all service calls use the Unity main thread.</summary>
    public sealed class AdMobAds : IDisposable
    {
        RewardedAdService service;
        AdMobRewardedBackend backend;
        AdMobTicker ticker;
        bool disposed, initFailed;

        public RewardedAdService Create(AdMobAdsSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (disposed) throw new ObjectDisposedException(nameof(AdMobAds));
            if (service != null) throw new InvalidOperationException("Create may only be called once per AdMobAds instance.");
            settings.Validate(!Debug.isDebugBuild && !Application.isEditor);
            var adUnitId = settings.RewardedAdUnitId;
            service = new RewardedAdService(() => Time.realtimeSinceStartup);
            ticker = AdMobTicker.Create(service.Tick);
            try
            {
                MobileAds.Initialize(status => MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    if (disposed || backend != null || initFailed) return;
                    if (status == null) { LogInitFailure("No initialization status returned."); return; }
                    backend = new AdMobRewardedBackend(adUnitId);
                    service.Attach(backend);
                    service.Load();
                }));
            }
            catch (Exception error) { LogInitFailure(error.Message); }
            return service;
        }

        void LogInitFailure(string error)
        {
            if (initFailed) return;
            initFailed = true;
            Debug.LogError("AdMob initialization failed: " + error);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            if (ticker != null) UnityEngine.Object.Destroy(ticker.gameObject);
            try { service?.Dispose(); }
            finally { backend?.Dispose(); }
        }
    }
}
