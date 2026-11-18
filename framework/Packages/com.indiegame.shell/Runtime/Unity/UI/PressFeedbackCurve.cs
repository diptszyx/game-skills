using UnityEngine;
namespace IndieGame.Shell.Unity.UI
{
    public static class PressFeedbackCurve
    {
        const float Overshoot = 1.70158f;
        public const float DownFraction = .5f;

        // Press amount: 0 at rest, 1 fully pressed; release overshoots slightly below 0.
        public static float Amount(float from, bool pressed, float elapsed, float seconds)
        {
            float t = seconds <= 0 ? 1 : Mathf.Clamp01(elapsed / (pressed ? seconds * DownFraction : seconds));
            return pressed ? from + (1 - from) * (1 - (1 - t) * (1 - t)) : from * (1 - EaseOutBack(t));
        }
        public static float Scale(float amount, float pressScale) => 1 - amount * (1 - pressScale);
        public static float Tint(float amount, float darken) => Mathf.Lerp(1, darken, Mathf.Clamp01(amount));
        static float EaseOutBack(float t)
        { float u = t - 1; return 1 + (Overshoot + 1) * u * u * u + Overshoot * u * u; }
    }
}
