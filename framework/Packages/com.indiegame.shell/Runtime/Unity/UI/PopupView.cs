using GameKit.UI;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Unity.UI
{
    public enum DismissalPolicy { Dismissible, Mandatory }
    public sealed class PopupView : MonoBehaviour
    {
        public CanvasGroup Group { get; private set; }
        public RectTransform Panel { get; private set; }
        public RectTransform Content { get; private set; }
        public Selectable InitialFocus;
        public DismissalPolicy Policy;
        public UiMotion Motion { get; internal set; }
        internal PopupHost Host;
        internal GameObject PreviousFocus;
        internal bool Closing;
        internal float Progress;
        internal float StartAlpha;
        internal Vector3 StartScale;
        public void Initialize(CanvasGroup group, RectTransform panel, RectTransform content, UiMotion motion)
        { Group = group; Panel = panel; Content = content; Motion = motion; }
        public bool RequestDismiss() => Host != null && Host.RequestDismiss(this);
        void OnDisable() { if (Host != null) Host.Forget(this); }
    }
}
