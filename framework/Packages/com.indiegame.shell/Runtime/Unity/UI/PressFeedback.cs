using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace IndieGame.Shell.Unity.UI
{
    // Squash and darken while a button is held; the tint uses the CanvasRenderer so ShellAppearance colours stay untouched.
    [RequireComponent(typeof(Button))]
    public sealed class PressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        float pressScale = .94f, darken = .88f, seconds = .12f;
        Button button; Graphic graphic;
        bool pressed, animating; float amount, from, elapsed;
        public void Configure(ShellStyle style) { pressScale = style.PressScale; darken = style.PressDarken; seconds = style.PressSeconds; }
        void Awake() { button = GetComponent<Button>(); graphic = button.targetGraphic; }
        public void OnPointerDown(PointerEventData e) { if (button.IsInteractable()) Begin(true); }
        public void OnPointerUp(PointerEventData e) { if (pressed) Begin(false); }
        public void OnPointerExit(PointerEventData e) { if (pressed) Begin(false); }
        void Begin(bool down) { pressed = down; from = amount; elapsed = 0; animating = true; }
        void Update()
        {
            if (!animating) return;
            elapsed += Time.unscaledDeltaTime;
            amount = PressFeedbackCurve.Amount(from, pressed, elapsed, seconds);
            Apply();
            if (!pressed && elapsed >= seconds) { amount = 0; animating = false; Apply(); }
        }
        void Apply()
        {
            transform.localScale = Vector3.one * PressFeedbackCurve.Scale(amount, pressScale);
            if (graphic == null) return;
            var tint = PressFeedbackCurve.Tint(amount, darken);
            graphic.canvasRenderer.SetColor(new Color(tint, tint, tint, 1));
        }
        void OnDisable() { pressed = animating = false; amount = 0; Apply(); }
    }
}
