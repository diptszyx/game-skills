using UnityEngine;

namespace GameKit.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class UiSafeArea : MonoBehaviour
    {
        private Rect _lastSafe;
        private Vector2 _lastScreen;
        private bool _hasLast;

        private void OnEnable() => ApplyIfChanged();
        private void Update() => ApplyIfChanged();

        private void ApplyIfChanged()
        {
            Rect safe = Screen.safeArea;
            Vector2 screen = new Vector2(Screen.width, Screen.height);
            if (_hasLast && safe == _lastSafe && screen == _lastScreen) return;
            (Vector2 min, Vector2 max) = AnchorsFor(safe, screen);
            RectTransform rect = (RectTransform)transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            _lastSafe = safe;
            _lastScreen = screen;
            _hasLast = true;
        }

        public static (Vector2 min, Vector2 max) AnchorsFor(Rect safe, Vector2 screen)
        {
            if (screen.x <= 0f || screen.y <= 0f) return (Vector2.zero, Vector2.one);
            return (new Vector2(safe.xMin / screen.x, safe.yMin / screen.y),
                new Vector2(safe.xMax / screen.x, safe.yMax / screen.y));
        }
    }
}
