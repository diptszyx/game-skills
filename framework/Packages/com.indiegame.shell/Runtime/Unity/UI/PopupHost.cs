using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
namespace IndieGame.Shell.Unity.UI
{
    // One host owns both display order and Back routing. Closing retains the input barrier.
    public sealed class PopupHost : MonoBehaviour
    {
        public bool HandlesOwnBackKey = true;
        readonly List<PopupView> stack = new List<PopupView>();
        public IReadOnlyList<PopupView> Stack => stack;
        public PopupView Top => stack.Count == 0 ? null : stack[stack.Count - 1];
        public void Open(PopupView view)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));
            if (view.Host != null && view.Host != this) throw new InvalidOperationException("Popup belongs to another host");
            if (!stack.Contains(view)) view.PreviousFocus = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            stack.Remove(view); stack.Add(view); view.Host = this;
            view.transform.SetParent(transform, false); view.transform.SetAsLastSibling();
            view.Closing = false; view.Progress = 0; view.StartAlpha = view.Group.alpha;
            view.StartScale = view.gameObject.activeSelf ? view.Panel.localScale : Vector3.one * view.Motion.OpenStartScale;
            view.Panel.localScale = view.StartScale;
            view.gameObject.SetActive(true); RefreshInput(); Focus(view);
        }
        public bool Back() => Top != null && RequestDismiss(Top);
        public bool RequestDismiss(PopupView view)
        {
            if (view != Top || view.Policy == DismissalPolicy.Mandatory || view.Closing) return false;
            return Close(view);
        }
        // Explicit owner completion can close a mandatory popup after its decision succeeds.
        public bool Close(PopupView view)
        {
            if (view != Top || view.Closing) return false;
            view.Closing = true; view.Progress = 0; view.StartAlpha = view.Group.alpha;
            view.StartScale = view.Panel.localScale; RefreshInput(); return true;
        }
        void Update()
        {
            var keyboard = Keyboard.current;
            if (HandlesOwnBackKey && keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.backspaceKey.wasPressedThisFrame)) Back();
            for (int i = stack.Count - 1; i >= 0; i--)
            {
                var view = stack[i]; view.Progress += Time.unscaledDeltaTime;
                var motion = view.Motion;
                float duration = view.Closing ? motion.CloseSeconds : motion.OpenSeconds;
                float t = duration <= 0 ? 1 : Mathf.Clamp01(view.Progress / duration);
                view.Group.alpha = Mathf.Lerp(view.StartAlpha, view.Closing ? 0 : 1, view.Closing ? t : motion.ScrimFadeSeconds <= 0 ? 1 : Mathf.Clamp01(view.Progress / motion.ScrimFadeSeconds));
                if (view.Closing) view.Panel.localScale = Vector3.Lerp(view.StartScale, Vector3.one * motion.CloseEndScale, t);
                else
                {
                    float split = Mathf.Clamp(motion.OvershootAt, .001f, .999f);
                    view.Panel.localScale = t < split ? Vector3.Lerp(view.StartScale, Vector3.one * motion.OpenOvershoot, t / split) : Vector3.Lerp(Vector3.one * motion.OpenOvershoot, Vector3.one, (t - split) / (1 - split));
                }
                if (view.Closing && t >= 1)
                { Forget(view); view.gameObject.SetActive(false); }
            }
        }
        internal void Forget(PopupView view)
        {
            bool wasTop = view == Top;
            if (!stack.Remove(view)) return;
            view.Host = null; RefreshInput();
            if (wasTop && EventSystem.current != null)
            {
                var previous = view.PreviousFocus;
                if (previous != null && previous.activeInHierarchy && (Top == null || previous.transform.IsChildOf(Top.transform)))
                    EventSystem.current.SetSelectedGameObject(previous);
                else Focus(Top);
            }
        }
        void RefreshInput()
        {
            foreach (var view in stack)
            {
                view.Group.ignoreParentGroups = false;
                view.Group.interactable = view == Top && !view.Closing;
                view.Group.blocksRaycasts = view == Top;
            }
        }
        static void Focus(PopupView view)
        {
            if (EventSystem.current == null) return;
            EventSystem.current.SetSelectedGameObject(view != null && view.InitialFocus != null ? view.InitialFocus.gameObject : null);
        }
        void OnDisable()
        {
            while (stack.Count > 0) { var view = Top; Forget(view); if (view != null) view.gameObject.SetActive(false); }
        }
    }
}
