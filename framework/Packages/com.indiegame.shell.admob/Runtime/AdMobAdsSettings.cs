using System;

namespace IndieGame.Shell.AdMob
{
    [Serializable]
    public sealed class AdMobAdsSettings
    {
        public const string TestRewardedAndroid = "ca-app-pub-3940256099942544/5224354917";
        public const string TestRewardedIos = "ca-app-pub-3940256099942544/1712485313";
#if UNITY_ANDROID
        public const string TestRewarded = TestRewardedAndroid;
#elif UNITY_IPHONE
        public const string TestRewarded = TestRewardedIos;
#else
        public const string TestRewarded = "unused";
#endif
        public string RewardedAdUnitId;
        public bool AllowTestAdUnit;

        public void Validate(bool isReleaseBuild)
        {
            if (string.IsNullOrWhiteSpace(RewardedAdUnitId))
                throw new InvalidOperationException("AdMob RewardedAdUnitId is required.");
            if ((RewardedAdUnitId == TestRewardedAndroid || RewardedAdUnitId == TestRewardedIos || RewardedAdUnitId == "unused")
                && (isReleaseBuild || !AllowTestAdUnit))
                throw new InvalidOperationException("AdMob test ad units require AllowTestAdUnit and an Editor/development build; they are forbidden in release builds.");
        }
    }
}
