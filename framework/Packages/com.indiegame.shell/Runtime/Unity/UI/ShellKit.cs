using System;
using TMPro;
using UnityEngine;
using GameKit.UI;

namespace IndieGame.Shell.Unity.UI
{
    public sealed class ShellKitPalette
    {
        public Color Outline = new Color32(0x30, 0x0c, 0x31, 255);
        public Color Cream = new Color32(0xfe, 0xe9, 0xd1, 255);
        public Color Backdrop = new Color32(0xe9, 0xb1, 0xcc, 255);
        public Color TrackColor = new Color32(0xd9, 0xd6, 0xf0, 255);
        public Color FillColor = new Color32(0xd7, 0x44, 0x49, 255);
        public float LabelOutlineWidth = .045f;
        public float TitleOutlineWidth = .06f;
        public static readonly ShellKitPalette Chunky = new ShellKitPalette();
    }

    public static class ShellKit
    {
        public const string DefaultFontResource = "ShellFont";

        public static void Apply(ShellStyle style, Func<string, Sprite> sprite, ShellKitPalette palette, Action<UnityEngine.Object> adopt)
        {
            var theme = style.Theme;
            var font = Resources.Load<TMP_FontAsset>(DefaultFontResource);
            if (font == null) throw new InvalidOperationException("Missing default font resource: " + DefaultFontResource);
            theme.Font = font;
            Sprite Get(string name) => sprite(name);
            style.EnableHomeChrome = true;
            style.Logo = Get("logo"); style.Mascot = Get("penguin_red_idle");
            style.GearButton = Get("gear_button"); style.BoardFrame = Get("board_frame");
            style.TabBarPlainBackground = Get("tab_bar_plain"); style.TabCellBackground = Get("tab_cell"); style.TabCellSelectedBackground = Get("tab_cell_selected");
            style.DayCellCollected = Get("day_cell_1"); style.DayCellToday = Get("day_cell_2"); style.DayCellLocked = Get("day_cell_3");
            style.BadgeSprite = Get("badge_1"); style.PlainPillBackground = Get("pill_plain");
            style.TabIcons.Clear();
            foreach (var key in new[] { "home", "checkin", "quests", "shop", "leaderboard" })
                style.TabIcons.Add(new ShellStyle.TabIconEntry { Key = key, Sprite = Get(key == "leaderboard" ? "tab_trophy" : "tab_" + key) });
            style.MandatoryPanelSprite = Get("panel_popup_nox");
            theme.PanelSprite = Get("panel_popup"); theme.Primary.Enabled = Get("btn_primary"); theme.Secondary.Enabled = Get("btn_secondary");
            theme.Primary.Locked = theme.Secondary.Locked = style.LockedButtonSprite = Get("btn_locked");
            theme.CloseSprite = Get("btn_close"); theme.ToggleOn = Get("toggle_1"); theme.ToggleOff = Get("toggle_2");
            style.ListRowSprite = Get("list_row"); style.ScreenBackground = Get("bg_home");
            style.CurrencyIcon = Get("icons_1"); style.HeartIcon = Get("icons_2"); style.BoosterIcon = Get("icons_5");
            theme.PanelSize = new Vector2(620f, 400f);
            style.PanelHasBakedCloseIcon=true;
            style.PopupLayout.PanelMinHeight=240; style.PopupLayout.PanelMaxHeight=1200;
            style.TitleTextRegion=new Rect(.3f,.84f,.4f,.08f);
            style.TabLabelColor = palette.Outline; style.ScreenTitleColorOverride = palette.Outline; style.TabUnselectedTint = new Color(.92f, .92f, .95f);
            style.SurfaceColor = style.ScreenColor = Color.white; style.TitleColor = style.BestTextColor(palette.Outline, Color.white, theme.PanelSprite, style.SurfaceColor, style.TitleTextRegion);
            style.PanelColor = palette.Cream; style.PanelTextColor = palette.Outline; style.BackdropColor = palette.Backdrop; style.BackdropTextColor = palette.Outline;
            style.ListRowColor = palette.Cream; style.ListRowTextColor = palette.Outline; theme.BodyColor = palette.Outline;
            style.ProgressTrackColor = palette.TrackColor; style.ProgressFillColor = palette.FillColor;
            theme.Primary.LabelColor = theme.Secondary.LabelColor = palette.Outline;
            var label = Outlined(theme.Font.material, palette, palette.LabelOutlineWidth);
            theme.Primary.LabelMaterial = theme.Secondary.LabelMaterial = label; adopt(label);
            var title = Outlined(theme.Font.material, palette, palette.TitleOutlineWidth);
            theme.TitleMaterial = title; adopt(title);
        }

        static Material Outlined(Material source, ShellKitPalette palette, float width)
        {
            var material = new Material(source);
            material.SetColor("_OutlineColor", palette.Outline); material.SetFloat("_OutlineWidth", width); material.EnableKeyword("OUTLINE_ON");
            return material;
        }
    }
}
