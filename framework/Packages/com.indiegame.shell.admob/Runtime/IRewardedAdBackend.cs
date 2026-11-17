using System;

namespace IndieGame.Shell.AdMob
{
    public interface IRewardedAdBackend
    {
        bool IsReady { get; }
        void Load();
        void Show(string placementId);
        event Action Loaded;
        event Action<string> LoadFailed;
        event Action Rewarded;
        event Action Closed;
        event Action<string> ShowFailed;
    }
}
