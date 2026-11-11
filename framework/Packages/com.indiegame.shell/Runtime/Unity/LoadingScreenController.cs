using System;

namespace IndieGame.Shell.Unity
{
    public enum LoadingScreenState { Hidden, FadingIn, Visible, WaitingToHide, FadingOut }

    // Call Tick each frame. Time and transitions are independent of UnityEngine.
    public sealed class LoadingScreenController
    {
        readonly IClock clock;
        readonly double minimumSeconds;
        readonly double fadeSeconds;
        DateTime shownAt, lastTick;
        double hideAt;
        bool hideRequested;
        public LoadingScreenState State { get; private set; } = LoadingScreenState.Hidden;
        public float Alpha { get; private set; }
        public float Progress { get; private set; }
        public LoadingScreenController(IClock clock, double minimumSeconds = 0.5, double fadeSeconds = 0.2)
        {
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            if (double.IsNaN(minimumSeconds) || double.IsInfinity(minimumSeconds) || minimumSeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(minimumSeconds));
            if (double.IsNaN(fadeSeconds) || double.IsInfinity(fadeSeconds) || fadeSeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(fadeSeconds));
            this.minimumSeconds = minimumSeconds; this.fadeSeconds = fadeSeconds;
        }
        public void Show()
        {
            if (State != LoadingScreenState.Hidden) return;
            shownAt = lastTick = clock.UtcNow; hideRequested = false; Progress = 0;
            Alpha = fadeSeconds == 0 ? 1 : 0;
            State = fadeSeconds == 0 ? LoadingScreenState.Visible : LoadingScreenState.FadingIn;
        }
        public void SetProgress(float value)
        {
            if (State == LoadingScreenState.Hidden || float.IsNaN(value)) return;
            Progress = Math.Max(Progress, Math.Max(0, Math.Min(1, value)));
        }
        public void Hide()
        {
            if (State == LoadingScreenState.Hidden) return;
            if (!hideRequested)
            {
                hideAt = Math.Max(Math.Max(minimumSeconds, fadeSeconds), Math.Max(0, (clock.UtcNow - shownAt).TotalSeconds));
                hideRequested = true;
            }
            Tick();
        }
        public void Tick()
        {
            if (State == LoadingScreenState.Hidden) return;
            var now = clock.UtcNow;
            // A backwards wall-clock adjustment cannot reverse a transition.
            if (now < lastTick) now = lastTick;
            var elapsed = (now - shownAt).TotalSeconds;
            lastTick = now;
            var hideStart = hideAt;
            if (hideRequested && elapsed >= hideStart)
            {
                Alpha = fadeSeconds == 0 ? 0 : (float)Math.Max(0, 1 - (elapsed - hideStart) / fadeSeconds);
                State = Alpha <= 0 ? LoadingScreenState.Hidden : LoadingScreenState.FadingOut;
            }
            else
            {
                Alpha = fadeSeconds == 0 ? 1 : (float)Math.Min(1, elapsed / fadeSeconds);
                State = hideRequested ? LoadingScreenState.WaitingToHide :
                    (Alpha < 1 ? LoadingScreenState.FadingIn : LoadingScreenState.Visible);
            }
        }
    }
}
