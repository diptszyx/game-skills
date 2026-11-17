using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using IndieGame.Shell.Unity.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

namespace IndieGame.Shell.TestKit
{
    /// <summary>Queues real mouse input for the player loop; never pumps InputSystem.Update.</summary>
    public sealed class RealPointer
    {
        readonly Mouse mouse;
        public RealPointer(Mouse mouse) { this.mouse = mouse ?? throw new ArgumentNullException(nameof(mouse)); }

        public IEnumerator Click(Button button, float timeoutSeconds = 10)
        {
            if (timeoutSeconds <= 0 || float.IsNaN(timeoutSeconds) || float.IsInfinity(timeoutSeconds))
                throw new ArgumentOutOfRangeException(nameof(timeoutSeconds));
            var deadline = Time.realtimeSinceStartup + timeoutSeconds;
            string path = button != null ? Path(button.transform) : "<null/destroyed>";
            bool clicked = false;
            UnityEngine.Events.UnityAction listener = () => clicked = true;
            Vector2 position = default;
            string details = "";
            yield return null; // Let pending UI changes reach layout before testing the surface.
            yield return SettlePopups(deadline, path); // An opening popup scales around its pivot and moves the button.
            while (!Ready(button, out position, out details))
            {
                Check(deadline, "waiting for raycastable button", path, details);
                yield return null;
            }
            button.onClick.AddListener(listener);
            try
            {
                InputSystem.QueueStateEvent(mouse, new MouseState { position = position });
                yield return null;
                while (mouse.leftButton.isPressed || Vector2.Distance(mouse.position.ReadValue(), position) > .1f)
                {
                    Check(deadline, "waiting for move/release consumption", path, details);
                    yield return null;
                }
                // Re-evaluate after moving: a transition may have moved or covered the target.
                while (!Ready(button, out var current, out details) || Vector2.Distance(current, position) > .1f)
                {
                    Check(deadline, "waiting for stable target after move", path, details);
                    if (button != null)
                    {
                        position = current;
                        InputSystem.QueueStateEvent(mouse, new MouseState { position = position });
                    }
                    yield return null;
                }
                InputSystem.QueueStateEvent(mouse, new MouseState { position = position }.WithButton(MouseButton.Left));
                yield return null;
                while (!mouse.leftButton.isPressed)
                {
                    Check(deadline, "waiting for press consumption", path, details);
                    yield return null;
                }
                yield return null; // EventSystem must process down before release is queued.
                InputSystem.QueueStateEvent(mouse, new MouseState { position = position });
                yield return null;
                while (!clicked || mouse.leftButton.isPressed)
                {
                    Ready(button, out _, out details);
                    Check(deadline, "waiting for onClick/release consumption", path, details + " " + Input(mouse));
                    yield return null;
                }
                yield return null; // Prevent the next gesture sharing the release processing frame.
                yield return SettlePopups(deadline, path);
            }
            finally
            {
                if (button != null) button.onClick.RemoveListener(listener);
                if (mouse.added && mouse.leftButton.isPressed)
                    InputSystem.QueueStateEvent(mouse, new MouseState { position = position });
            }
        }

        static string Input(Mouse mouse)
        {
            var es = EventSystem.current;
            return "focused=" + Application.isFocused + " timeScale=" + Time.timeScale
                + " module=" + (es != null && es.currentInputModule != null ? es.currentInputModule.GetType().Name : "<none>")
                + " mouseAdded=" + mouse.added + " pressed=" + mouse.leftButton.isPressed + " current=" + (Mouse.current == mouse)
                + " mice=" + InputSystem.devices.Count(d => d is Mouse) + " update=" + InputSystem.settings.updateMode
                + " background=" + InputSystem.settings.backgroundBehavior;
        }

        static IEnumerator SettlePopups(float deadline, string path)
        {
            var popups = UnityEngine.Object.FindFirstObjectByType<PopupHost>();
            while (popups != null && popups.IsTransitioning)
            {
                Check(deadline, "waiting for popup transition to settle", path, "");
                yield return null;
            }
        }

        static void Check(float deadline, string phase, string path, string details)
        {
            Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline),
                "RealPointer timeout " + phase + ": button=" + path + " " + details);
        }

        static bool Ready(Button button, out Vector2 position, out string details)
        {
            Canvas.ForceUpdateCanvases();
            position = default;
            if (button == null) { details = "active=false interactable=false rect=<destroyed> hit0=<none>"; return false; }
            var rect = (RectTransform)button.transform;
            var canvas = button.GetComponentInParent<Canvas>();
            var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            position = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
            var es = EventSystem.current;
            var hits = new List<RaycastResult>();
            if (es != null && es.isActiveAndEnabled)
                es.RaycastAll(new PointerEventData(es) { position = position }, hits);
            details = "active=" + button.IsActive() + " interactable=" + button.IsInteractable()
                + " rect=" + rect.rect + " pos=" + position + " screen=" + Screen.width + "x" + Screen.height
                + " hit0=" + (hits.Count > 0 ? Path(hits[0].gameObject.transform) : "<none>");
            return button.IsActive() && button.IsInteractable() && es != null && es.currentInputModule != null
                && es.currentInputModule.isActiveAndEnabled && position.x >= 0 && position.y >= 0
                && position.x < Screen.width && position.y < Screen.height
                && hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Button>() == button;
        }

        static string Path(Transform transform)
        {
            string path = "";
            for (; transform != null; transform = transform.parent)
                path = transform.name + (transform.gameObject.activeSelf ? "+/" : "-/") + path;
            return path;
        }
    }
}
