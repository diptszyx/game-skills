using System;
using System.Collections.Generic;
using GameKit.UI;
using UnityEngine;
namespace IndieGame.Shell.Unity.UI
{
    public enum ButtonRole { Primary, Secondary, Locked }
    [CreateAssetMenu(menuName = "IndieGame/Shell Style")]
    public sealed class ShellStyle : ScriptableObject
    {
        public bool EnableHomeChrome;
        public Sprite Logo, Mascot;
        public Sprite TabBarPlainBackground, DayCellCollected, DayCellToday, DayCellLocked, BadgeSprite, PlainPillBackground, TabCellBackground, TabCellSelectedBackground;
        [Serializable] public sealed class TabIconEntry { public string Key; public Sprite Sprite; }
        public List<TabIconEntry> TabIcons = new List<TabIconEntry>();
        public Color TabSelectedTint = Color.white, TabUnselectedTint = new Color(.7f,.7f,.8f); public Color TabLabelColor = Color.white;
        public Sprite TabIcon(string key)
        {
            var entry = TabIcons.Find(e => e != null && e.Key == key);
            return entry == null ? null : entry.Sprite;
        }
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
            if (EnableHomeChrome)
            {
                Require(TabBarPlainBackground, nameof(TabBarPlainBackground)); Require(DayCellCollected, nameof(DayCellCollected));
                Require(DayCellToday, nameof(DayCellToday)); Require(DayCellLocked, nameof(DayCellLocked));
                Require(BadgeSprite, nameof(BadgeSprite)); Require(PlainPillBackground, nameof(PlainPillBackground));
                Require(TabCellBackground, nameof(TabCellBackground)); Require(TabCellSelectedBackground, nameof(TabCellSelectedBackground));
                foreach (var key in new[] { "home", "checkin", "quests", "shop", "leaderboard" }) Require(TabIcon(key), "TabIcons." + key);
            }
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
