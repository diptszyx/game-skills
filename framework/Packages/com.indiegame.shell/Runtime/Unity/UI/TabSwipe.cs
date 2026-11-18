using System;
using UnityEngine;
using UnityEngine.EventSystems;
namespace IndieGame.Shell.Unity.UI
{
    public static class TabSwipeRule
    {
        public const float DistanceFraction = .18f, FlickDistanceFraction = .06f, FlickSpeedPerWidth = 1.5f, DominantAxisRatio = 1.5f;
        // Returns +1 for the next tab (finger moved left), -1 for the previous, 0 for no swipe.
        public static int Decide(Vector2 delta, float seconds, float width)
        {
            if (width <= 0 || Mathf.Abs(delta.x) < Mathf.Abs(delta.y) * DominantAxisRatio) return 0;
            float distance = Mathf.Abs(delta.x);
            bool far = distance >= width * DistanceFraction;
            bool flick = distance >= width * FlickDistanceFraction && distance / Mathf.Max(seconds, .01f) >= width * FlickSpeedPerWidth;
            return far || flick ? (delta.x < 0 ? 1 : -1) : 0;
        }
    }
    // Sits on an ancestor of the screens so drags that no child (e.g. a ScrollRect) consumed bubble up to it.
    public sealed class TabSwipeHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public static TabSwipeHandler Attach(ShellStyle style, ScreenHost host, TabBarView tabBar)
        {
            if (!style.EnableTabSwipe) return null;
            var swipe = host.SafeRoot.gameObject.AddComponent<TabSwipeHandler>();
            swipe.CanSwipe = () => tabBar.Root.gameObject.activeInHierarchy && host.SafeRoot.GetComponentInChildren<PopupView>() == null;
            swipe.Swiped += step => tabBar.Step(step);
            return swipe;
        }
        public event Action<int> Swiped;
        public Func<bool> CanSwipe = () => true;
        public Func<float> Width;
        public Func<float> Clock = () => Time.unscaledTime;
        Vector2 start; float startTime; bool tracking;
        public void OnBeginDrag(PointerEventData e) { tracking = CanSwipe(); start = e.pressPosition; startTime = Clock(); }
        float ScreenWidth() { var rect = (RectTransform)transform; return rect.rect.width * rect.lossyScale.x; }
        public void OnDrag(PointerEventData e) { }
        public void OnEndDrag(PointerEventData e)
        {
            if (!tracking) return; tracking = false;
            float width = Width != null ? Width() : ScreenWidth();
            int step = TabSwipeRule.Decide(e.position - start, Clock() - startTime, width);
            if (step != 0) Swiped?.Invoke(step);
        }
    }
}
