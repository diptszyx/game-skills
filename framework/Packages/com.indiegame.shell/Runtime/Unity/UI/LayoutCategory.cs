using UnityEngine;
namespace IndieGame.Shell.Unity.UI
{
    public enum ShellLayoutCategory { Standard, IconButton, IconAction, IconAmount, PopupAction, Row, RowAction, CloseHit, PopupScrim, PopupPanel }
    // Semantic identity survives sprite/style changes, including caller-owned row anchors.
    [DisallowMultipleComponent]
    public sealed class LayoutCategory : MonoBehaviour
    {
        public ShellLayoutCategory Category { get; private set; }
        internal static void Record(Component target, ShellLayoutCategory category)
        { target.gameObject.AddComponent<LayoutCategory>().Category=category; }
    }
}
