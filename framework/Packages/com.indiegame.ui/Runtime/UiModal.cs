using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GameKit.UI
{
    public class UiModal : MonoBehaviour
    {
        private static Sprite _solidSprite;
        private static readonly List<UiModal> Stack = new List<UiModal>();

        [SerializeField] private RectTransform _panel;
        [SerializeField] private Image _scrim;
        [SerializeField] private UiTheme _theme;

        private Coroutine _motion;
        private bool _closing;
        private bool _backWasHeld;
        private UiTheme _fallbackTheme;

        public bool IsOpen => gameObject.activeSelf;
        public RectTransform Panel => _panel;
        public Color ScrimColor => _scrim.color;

        private UiTheme Theme
        {
            get
            {
                if (_theme != null) return _theme;
                if (_fallbackTheme == null) _fallbackTheme = ScriptableObject.CreateInstance<UiTheme>();
                return _fallbackTheme;
            }
        }

        protected virtual void Awake()
        {
            if (_scrim.sprite == null)
            {
                if (_solidSprite == null)
                    _solidSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(.5f, .5f));
                _scrim.sprite = _solidSprite;
            }

            _scrim.type = Image.Type.Simple;
            _scrim.preserveAspect = false;
            _scrim.color = new Color(Theme.ScrimColor.r, Theme.ScrimColor.g, Theme.ScrimColor.b, 0f);
            _panel.GetComponent<Button>().transition = Selectable.Transition.None;
            GetComponent<Button>().onClick.AddListener(Close);
            _panel.Find("Close").GetComponent<Button>().onClick.AddListener(Close);
            _panel.localScale = Vector3.one * Theme.Motion.OpenStartScale;
        }

        public void Open()
        {
            bool reopeningHide = _closing;
            StopMotion();
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            _closing = false;
            if (!Stack.Contains(this)) Stack.Add(this);
            _motion = StartCoroutine(Show(reopeningHide));
        }

        public void Close()
        {
            if (!IsOpen || _closing) return;
            StopMotion();
            _closing = true;
            _motion = StartCoroutine(Hide());
        }

        protected virtual void Update()
        {
            Keyboard keyboard = Keyboard.current;
            bool backHeld = keyboard != null && (keyboard.escapeKey.isPressed || keyboard.backspaceKey.isPressed);
            bool backEdge = backHeld && !_backWasHeld;
            _backWasHeld = backHeld;
            if (backEdge && IsOpen && Stack.Count > 0 && Stack[Stack.Count - 1] == this) Close();
        }

        protected virtual void OnClosed() { }

        private IEnumerator Show(bool fromCurrentScale)
        {
            UiMotion motion = Theme.Motion;
            float elapsed = 0f;
            float duration = Mathf.Max(0.0001f, motion.OpenSeconds);
            float fadeDuration = Mathf.Max(0.0001f, motion.ScrimFadeSeconds);
            float startScale = fromCurrentScale ? _panel.localScale.x : motion.OpenStartScale;
            if (!fromCurrentScale) _panel.localScale = Vector3.one * startScale;
            Color scrim = Theme.ScrimColor;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                _scrim.color = new Color(scrim.r, scrim.g, scrim.b, scrim.a * Mathf.Clamp01(elapsed / fadeDuration));
                float split = Mathf.Clamp(motion.OvershootAt, 0.0001f, 0.9999f);
                float scale = progress < split
                    ? Mathf.Lerp(startScale, motion.OpenOvershoot, progress / split)
                    : Mathf.Lerp(motion.OpenOvershoot, 1f, (progress - split) / (1f - split));
                _panel.localScale = Vector3.one * scale;
                yield return null;
            }

            _panel.localScale = Vector3.one;
            _scrim.color = scrim;
            _motion = null;
        }

        private IEnumerator Hide()
        {
            UiMotion motion = Theme.Motion;
            float elapsed = 0f;
            float duration = Mathf.Max(0.0001f, motion.CloseSeconds);
            Vector3 start = _panel.localScale;
            Color startColor = _scrim.color;
            Color targetColor = Theme.ScrimColor;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                _panel.localScale = Vector3.Lerp(start, Vector3.one * motion.CloseEndScale, progress);
                _scrim.color = new Color(targetColor.r, targetColor.g, targetColor.b, Mathf.Lerp(startColor.a, 0f, progress));
                yield return null;
            }

            _motion = null;
            gameObject.SetActive(false);
            Stack.Remove(this);
            _closing = false;
            OnClosed();
        }

        private void StopMotion()
        {
            if (_motion == null) return;
            StopCoroutine(_motion);
            _motion = null;
        }

        protected virtual void OnDisable()
        {
            StopMotion();
            Stack.Remove(this);
        }

        protected virtual void OnDestroy()
        {
            StopMotion();
            Stack.Remove(this);
            if (_fallbackTheme != null) Destroy(_fallbackTheme);
        }

        internal static void ResetStack() => Stack.Clear();
    }
}
