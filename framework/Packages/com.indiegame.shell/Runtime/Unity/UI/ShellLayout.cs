using UnityEngine;
namespace IndieGame.Shell.Unity.UI
{
    // Shell geometry is independent of appearance assets and cannot be overridden per game.
    public sealed class ShellLayout
    {
        public static ShellLayout Standard { get; } = new ShellLayout();
        ShellLayout() { }
        public float SessionPopupMinHeight => 240;
        public float SessionPopupMaxHeight => 1200;
        public Vector2 IconButton => new Vector2(64,64);
        public Vector2 IconAction => new Vector2(140,72);
        public Vector2 IconAmount => new Vector2(160,72);
        public Vector2 BackPosition => new Vector2(16,-16);
        public Vector2 HeaderRightPosition => new Vector2(-16,-16);
        public float IconHitSize => 96;
        public float PopupActionSpacing => 12;
        public float ToggleRowWidth => 288;
        public Vector2 ButtonStandard => new Vector2(280,80);
        public Vector2 PopupAction => new Vector2(400,76);
        public Vector2 Row => new Vector2(600,120);
        public Vector2 CloseHit => Vector2.one*IconHitSize;
        public Vector2 CloseHitPosition => new Vector2(-36,-40);
        public float BackBottomOffset(bool homeChrome) => homeChrome ? 200 : 68;
    }
}
