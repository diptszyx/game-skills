using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;

namespace IndieGame.Shell.AdMob
{
    public sealed class AdMobRewardedBackend : IRewardedAdBackend, IDisposable
    {
        readonly string adUnitId;
        RewardedAd ad;
        bool disposed, loading;
        public event Action Loaded;
        public event Action<string> LoadFailed;
        public event Action Rewarded;
        public event Action Closed;
        public event Action<string> ShowFailed;
        public bool IsReady => ad != null && ad.CanShowAd();

        public AdMobRewardedBackend(string adUnitId) { this.adUnitId = adUnitId; }

        public void Load()
        {
            if (disposed || loading) return;
            DestroyAd();
            loading = true;
            try
            {
                RewardedAd.Load(adUnitId, new AdRequest(), (loaded, error) =>
                    MobileAdsEventExecutor.ExecuteInUpdate(() =>
                    {
                        loading = false;
                        if (disposed) { loaded?.Destroy(); return; }
                        if (error != null || loaded == null)
                        {
                            loaded?.Destroy();
                            LoadFailed?.Invoke(error == null ? "AdMob returned no rewarded ad." : error.ToString());
                            return;
                        }
                        ad = loaded;
                        loaded.OnAdFullScreenContentClosed += () => MobileAdsEventExecutor.ExecuteInUpdate(() =>
                        {
                            if (disposed || ad != loaded) return;
                            DestroyAd();
                            Closed?.Invoke();
                        });
                        loaded.OnAdFullScreenContentFailed += errorInfo => MobileAdsEventExecutor.ExecuteInUpdate(() =>
                        {
                            if (disposed || ad != loaded) return;
                            DestroyAd();
                            ShowFailed?.Invoke(errorInfo.ToString());
                        });
                        Loaded?.Invoke();
                    }));
            }
            catch (Exception error)
            {
                loading = false;
                LoadFailed?.Invoke(error.Message);
            }
        }

        public void Show(string placementId)
        {
            // placementId is ignored: AdMob uses one ad unit per format.
            if (disposed) throw new ObjectDisposedException(nameof(AdMobRewardedBackend));
            if (!IsReady) { ShowFailed?.Invoke("AdMob rewarded ad is not ready."); return; }
            var shown = ad;
            try
            {
                shown.Show(reward => MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    if (!disposed && ad == shown) Rewarded?.Invoke();
                }));
            }
            catch (Exception error)
            {
                DestroyAd();
                ShowFailed?.Invoke(error.Message);
            }
        }

        void DestroyAd()
        {
            var previous = ad;
            ad = null;
            previous?.Destroy();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            DestroyAd();
        }
    }
}
