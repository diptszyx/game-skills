using System;

namespace IndieGame.Shell.LevelPlay
{
    [Serializable]
    public sealed class LevelPlayAdsSettings
    {
        public string AppKey;
        public string RewardedAdUnitId;
        public bool AllowTestKey;

        public void Validate(bool isReleaseBuild)
        {
            if (string.IsNullOrWhiteSpace(AppKey)) throw new InvalidOperationException("LevelPlay AppKey is required.");
            if (string.IsNullOrWhiteSpace(RewardedAdUnitId)) throw new InvalidOperationException("LevelPlay RewardedAdUnitId is required.");
            if (AppKey == "TEST_KEY_REPLACE_ME" && (isReleaseBuild || !AllowTestKey))
                throw new InvalidOperationException("LevelPlay placeholder AppKey requires AllowTestKey and an Editor/development build; it is forbidden in release builds.");
        }
    }
}
