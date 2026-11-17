using System;

namespace IndieGame.Shell.LevelPlay
{
    /// <summary>Main-thread service. Tick must run every frame, including while ads are unavailable.</summary>
    public sealed class LevelPlayRewardedService : IRewardedAdService, IDisposable
    {
        IRewardedAdBackend backend;
        readonly Func<float> clock;
        Action<AdResult> pending;
        bool rewarded, loading, disposed;
        float retryAt = float.PositiveInfinity;
        float retryDelay = 5f;

        internal LevelPlayRewardedService(Func<float> clock)
        { this.clock = clock ?? throw new ArgumentNullException(nameof(clock)); }

        public LevelPlayRewardedService(IRewardedAdBackend backend, Func<float> clock) : this(clock)
        { Attach(backend ?? throw new ArgumentNullException(nameof(backend))); }

        internal void Attach(IRewardedAdBackend value)
        {
            if (disposed) throw new ObjectDisposedException(nameof(LevelPlayRewardedService));
            if (backend != null) throw new InvalidOperationException("Backend already attached.");
            backend = value;
            backend.Loaded += OnLoaded;
            backend.LoadFailed += OnLoadFailed;
            backend.Rewarded += OnRewarded;
            backend.Closed += OnClosed;
            backend.ShowFailed += OnShowFailed;
        }

        public void Show(string placementId, Action<AdResult> onDone)
        {
            if (onDone == null) throw new ArgumentNullException(nameof(onDone));
            if (string.IsNullOrWhiteSpace(placementId))
            { onDone(new AdResult(AdOutcome.Failed, ServiceError.InvalidArgument)); return; }
            if (disposed) throw new ObjectDisposedException(nameof(LevelPlayRewardedService));
            if (pending != null)
            { onDone(new AdResult(AdOutcome.Failed, ServiceError.Conflict)); return; }
            if (backend == null || !backend.IsReady)
            {
                try { Load(); }
                finally { onDone(new AdResult(AdOutcome.NotReady)); }
                return;
            }
            rewarded = false;
            pending = onDone;
            backend.Show(placementId);
        }

        internal void Load()
        {
            if (disposed || backend == null || loading || retryAt != float.PositiveInfinity) return;
            loading = true;
            backend.Load();
        }

        public void Tick()
        {
            if (disposed || clock() < retryAt) return;
            retryAt = float.PositiveInfinity;
            Load();
        }

        void OnLoaded() { loading = false; retryDelay = 5f; retryAt = float.PositiveInfinity; }
        void OnLoadFailed(string error)
        {
            loading = false;
            retryAt = clock() + retryDelay;
            retryDelay = Math.Min(60f, retryDelay * 2f);
        }
        void OnRewarded() { if (pending != null) rewarded = true; }
        void OnClosed() { Finish(new AdResult(rewarded ? AdOutcome.Completed : AdOutcome.Skipped)); }
        void OnShowFailed(string error) { Finish(new AdResult(AdOutcome.Failed, ServiceError.Failed)); }
        void Finish(AdResult result)
        {
            var callback = pending;
            pending = null;
            rewarded = false;
            // Reload before invoking client code so reentrant Show cannot reuse the old ad.
            try { Load(); }
            finally { callback?.Invoke(result); }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            if (backend != null)
            {
                backend.Loaded -= OnLoaded;
                backend.LoadFailed -= OnLoadFailed;
                backend.Rewarded -= OnRewarded;
                backend.Closed -= OnClosed;
                backend.ShowFailed -= OnShowFailed;
            }
            var callback = pending; pending = null;
            callback?.Invoke(new AdResult(AdOutcome.Failed, ServiceError.Unavailable));
        }
    }
}
