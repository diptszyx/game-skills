using System;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Unity.UI
{
    public sealed class GameViewportView : IDisposable
    {
        public const float DefaultOrthographicSize = 5f;
        const float Tolerance = 1e-4f;
        static readonly Vector3 WorldOffset = new Vector3(10000f, 0f, 0f);
        readonly RenderTexture target;
        readonly GameObject world;
        readonly RectTransform area;
        public Camera Camera { get; }
        public Transform Content { get; }
        public GameViewportView(RectTransform frame, Vector2 inset, int resolutionScale = 1)
        {
            var size = frame.sizeDelta - inset * 2f;
            var image = new GameObject("Game viewport", typeof(RectTransform), typeof(RawImage), typeof(ViewportCameraGate)).GetComponent<RawImage>();
            area = image.rectTransform; area.SetParent(frame, false);
            area.anchorMin = area.anchorMax = new Vector2(.5f, .5f); area.sizeDelta = size; area.anchoredPosition = Vector2.zero;
            image.raycastTarget = false;
            target = new RenderTexture(Mathf.Max(1, Mathf.RoundToInt(size.x * resolutionScale)), Mathf.Max(1, Mathf.RoundToInt(size.y * resolutionScale)), 16) { name = "Game viewport" };
            image.texture = target;
            world = new GameObject("Game viewport world");
            world.transform.position = WorldOffset;
            Camera = new GameObject("Game viewport camera", typeof(Camera)).GetComponent<Camera>();
            Camera.transform.SetParent(world.transform, false); Camera.transform.localPosition = new Vector3(0, 0, -10);
            Camera.orthographic = true; Camera.orthographicSize = DefaultOrthographicSize; Camera.targetTexture = target;
            Camera.clearFlags = CameraClearFlags.SolidColor; Camera.backgroundColor = Color.clear;
            Content = new GameObject("Content").transform; Content.SetParent(world.transform, false);
            image.GetComponent<ViewportCameraGate>().Camera = Camera; Camera.enabled = image.gameObject.activeInHierarchy;
        }
        public bool TryScreenToWorld(Vector2 screenPoint, out Vector3 world)
        {
            world = default;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(area, screenPoint, null, out var local)) return false;
            var rect = area.rect;
            var normalized = new Vector2((local.x - rect.xMin) / rect.width, (local.y - rect.yMin) / rect.height);
            if (normalized.x < -Tolerance || normalized.x > 1 + Tolerance || normalized.y < -Tolerance || normalized.y > 1 + Tolerance) return false;
            world = Camera.ViewportToWorldPoint(new Vector3(normalized.x, normalized.y, -Camera.transform.localPosition.z));
            return true;
        }
        public Vector2 WorldToScreen(Vector3 worldPoint)
        {
            var viewport = Camera.WorldToViewportPoint(worldPoint);
            var rect = area.rect;
            var local = new Vector2(rect.xMin + viewport.x * rect.width, rect.yMin + viewport.y * rect.height);
            return RectTransformUtility.WorldToScreenPoint(null, area.TransformPoint(local));
        }
        public void Fit(Bounds worldBounds)
        {
            Camera.transform.position = new Vector3(worldBounds.center.x, worldBounds.center.y, Camera.transform.position.z);
            Camera.orthographicSize = Mathf.Max(worldBounds.extents.y, worldBounds.extents.x / Camera.aspect);
        }
        public void Dispose()
        {
            if (Camera != null) Camera.targetTexture = null;
            if (target != null) target.Release();
            if (world != null) { if (Application.isPlaying) UnityEngine.Object.Destroy(world); else UnityEngine.Object.DestroyImmediate(world); }
        }
    }
    sealed class ViewportCameraGate : MonoBehaviour
    {
        public Camera Camera;
        void OnEnable() { if (Camera != null) Camera.enabled = true; }
        void OnDisable() { if (Camera != null) Camera.enabled = false; }
    }
}
