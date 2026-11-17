using System;
using Unity.Services.LevelPlay;
using UnityEngine;

namespace IndieGame.Shell.LevelPlay
{
    public sealed class LevelPlayRewardedBackend : IRewardedAdBackend, IDisposable
    {
        readonly LevelPlayRewardedAd ad;
        public event Action Loaded;
        public event Action<string> LoadFailed;
        public event Action Rewarded;
        public event Action Closed;
        public event Action<string> ShowFailed;
        public bool IsReady => ad.IsAdReady();
        public LevelPlayRewardedBackend(string adUnitId)
        {
            ad = new LevelPlayRewardedAd(adUnitId);
            ad.OnAdLoaded += OnLoaded;
            ad.OnAdLoadFailed += OnLoadFailed;
            ad.OnAdRewarded += OnRewarded;
            ad.OnAdClosed += OnClosed;
            ad.OnAdDisplayFailed += OnShowFailed;
        }
        public void Load() { ad.LoadAd(); }
        public void Show(string placementId) { ad.ShowAd(placementName: placementId); }
        void OnLoaded(LevelPlayAdInfo info) { Loaded?.Invoke(); }
        void OnLoadFailed(LevelPlayAdError error) { LoadFailed?.Invoke(error.ErrorMessage); }
        void OnRewarded(LevelPlayAdInfo info, LevelPlayReward reward) { Rewarded?.Invoke(); }
        void OnClosed(LevelPlayAdInfo info) { Closed?.Invoke(); }
        void OnShowFailed(LevelPlayAdInfo info, LevelPlayAdError error)
        {
            Debug.LogError("LevelPlay rewarded show failed: " + error.ErrorMessage);
            ShowFailed?.Invoke(error.ErrorMessage);
        }
        public void Dispose()
        {
            ad.OnAdLoaded -= OnLoaded;
            ad.OnAdLoadFailed -= OnLoadFailed;
            ad.OnAdRewarded -= OnRewarded;
            ad.OnAdClosed -= OnClosed;
            ad.OnAdDisplayFailed -= OnShowFailed;
            ad.Dispose();
        }
    }
}
