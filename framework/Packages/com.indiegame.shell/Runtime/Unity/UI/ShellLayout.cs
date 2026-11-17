using UnityEngine;
namespace IndieGame.Shell.Unity.UI
{
    // Shell geometry is independent of appearance assets and cannot be overridden per game.
    public sealed class ShellLayout
    {
        public static ShellLayout Standard { get; } = new ShellLayout();
        ShellLayout() { }
        public Vector2 ButtonStandard => new Vector2(280,80);
        public Vector2 PopupAction => new Vector2(400,76);
        public Vector2 Row => new Vector2(600,120);
        public Vector2 CloseHit => new Vector2(96,96);
        public Vector2 CloseHitPosition => new Vector2(-36,-110);
        public float BackBottomOffset(bool homeChrome) => homeChrome ? 200 : 68;
    }
}
