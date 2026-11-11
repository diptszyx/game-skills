using System;
using GameKit.UI;
using UnityEngine;
namespace IndieGame.Shell.Unity.UI
{
    public enum ButtonRole { Primary, Secondary, Locked }
    [CreateAssetMenu(menuName = "IndieGame/Shell Style")]
    public sealed class ShellStyle : ScriptableObject
    {
        public UiTheme Theme;
        public Sprite ScreenBackground, CurrencyIcon, HeartIcon, BoosterIcon, ListRowSprite, LockedButtonSprite;
        public Color ScreenColor, TitleColor, ListRowColor, ListRowTextColor, ProgressTrackColor, ProgressFillColor;
        public Color SurfaceColor = Color.white;
        public Vector2 RowSize = new Vector2(600, 80);
        public float Spacing = 12;
        public T Require<T>(T value, string role) where T : UnityEngine.Object
        { if (value == null) throw new InvalidOperationException("Missing style role: " + role); return value; }
        public void Validate()
        {
            Require(Theme, "Theme"); Require(Theme.Font, "Theme.Font");
            Require(ScreenBackground, "ScreenBackground"); Require(CurrencyIcon, "CurrencyIcon");
            Require(HeartIcon, "HeartIcon"); Require(BoosterIcon, "BoosterIcon");
            Require(ListRowSprite, "ListRowSprite"); Require(LockedButtonSprite, "LockedButtonSprite");
            Require(Theme.PanelSprite, "Theme.PanelSprite");
            if (Theme.Primary == null) throw new InvalidOperationException("Missing style role: Theme.Primary");
            if (Theme.Secondary == null) throw new InvalidOperationException("Missing style role: Theme.Secondary");
            Require(Theme.Primary.Enabled, "Theme.Primary.Enabled"); Require(Theme.Secondary.Enabled, "Theme.Secondary.Enabled");
            if (Theme.Motion == null) throw new InvalidOperationException("Missing style role: Theme.Motion");
        }
    }
}
