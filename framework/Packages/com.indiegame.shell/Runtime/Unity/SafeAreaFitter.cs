using UnityEngine;

namespace IndieGame.Shell.Unity
{
    public static class SafeAreaMath
    {
        // Empty safe areas or uninitialized screen dimensions fall back to full screen.
        public static Rect ToAnchors(Rect safe, Vector2 screen)
        {
            if (screen.x <= 0 || screen.y <= 0 || safe.width <= 0 || safe.height <= 0)
                return new Rect(0, 0, 1, 1);
            var min = new Vector2(Mathf.Clamp01(safe.xMin / screen.x), Mathf.Clamp01(safe.yMin / screen.y));
            var max = new Vector2(Mathf.Clamp01(safe.xMax / screen.x), Mathf.Clamp01(safe.yMax / screen.y));
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
    }

    // Attach to a child of a full-screen RectTransform (not the root Canvas).
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        RectTransform target;
        Rect lastSafe;
        Vector2 lastScreen;
        ScreenOrientation lastOrientation;
        void OnEnable() { target = GetComponent<RectTransform>(); Apply(); }
        void Update()
        {
            if (lastSafe != Screen.safeArea || lastScreen != new Vector2(Screen.width, Screen.height)
                || lastOrientation != Screen.orientation) Apply();
        }
        void Apply()
        {
            lastSafe = Screen.safeArea;
            lastScreen = new Vector2(Screen.width, Screen.height);
            lastOrientation = Screen.orientation;
            var anchors = SafeAreaMath.ToAnchors(lastSafe, lastScreen);
            target.anchorMin = anchors.min; target.anchorMax = anchors.max;
            target.offsetMin = Vector2.zero; target.offsetMax = Vector2.zero;
        }
    }
}
