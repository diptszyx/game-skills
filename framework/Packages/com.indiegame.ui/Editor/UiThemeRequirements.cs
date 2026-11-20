using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameKit.UI.Editor
{
    public static class UiThemeRequirements
    {
        public static string[] MissingRequiredFields(UiTheme theme)
        {
            if (theme == null) throw new ArgumentNullException(nameof(theme));
            var missing = new List<string>();
            void Require(UnityEngine.Object value, string path) { if (value == null) missing.Add(path); }
            void Style(UiButtonStyle style, string path)
            {
                Require(style?.Enabled, path + ".Enabled");
                Require(style?.LabelMaterial, path + ".LabelMaterial");
            }
            Require(theme.Font, nameof(theme.Font));
            Require(theme.BodyMaterial, nameof(theme.BodyMaterial));
            Require(theme.TitleMaterial, nameof(theme.TitleMaterial));
            Require(theme.PanelSprite, nameof(theme.PanelSprite));
            Require(theme.CloseSprite, nameof(theme.CloseSprite));
            Style(theme.Primary, nameof(theme.Primary));
            Style(theme.Secondary, nameof(theme.Secondary));
            Style(theme.Neutral, nameof(theme.Neutral));
            return missing.ToArray();
        }
    }
}
