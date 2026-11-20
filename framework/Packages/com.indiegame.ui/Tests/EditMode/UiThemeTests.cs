using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace GameKit.UI.Tests
{
    public sealed class UiThemeTests
    {
        private static string PackagePath => Path.GetFullPath(
            Path.Combine(Application.dataPath, "../Packages/com.indiegame.ui"));

        [Test]
        public void Defaults_MatchGame2Values()
        {
            UiTheme theme = ScriptableObject.CreateInstance<UiTheme>();
            try
            {
                // Game2 sources: Assets/Scripts/Editor/UiKit.cs:12-14,113-119; View/CurvedText.cs:9.
                Check(theme, "Font", null);
                Check(theme, "BodyMaterial", null);
                Check(theme, "TitleMaterial", null);
                Check(theme, "CurvedTitle", true);
                Check(theme, "TitleCurveRadius", 810f);
                Check(theme, "TitleOffsetY", -54f);
                Check(theme, "TitleFontSize", 46f);
                Check(theme, "BodyFontSize", 36f);
                Check(theme, "BodyOffsetY", 40f);
                Check(theme, "ButtonRowOffsetY", -150f);
                Check(theme, "TwoButtonOffsetX", 145f);
                Check(theme, "PanelPixelsPerUnitMultiplier", 1f);
                Check(theme, "TitleSize", new Vector2(420f, 100f));
                Check(theme, "BodySize", new Vector2(500f, 120f));
                // Game2 source: Assets/Scripts/Editor/LeaveConfirmSheetPrefabBuilder.cs:23-38.
                Check(theme, "BodyColor", (Color)new Color32(30, 58, 138, 255));
                Check(theme, "PanelSprite", null);
                Check(theme, "PanelSize", new Vector2(620f, 520f));
                Check(theme, "CloseSprite", null);
                Check(theme, "CloseSize", new Vector2(84f, 84f));
                Check(theme, "CloseOffset", new Vector2(262f, 262f));
                // Game2 source: Assets/Scripts/Editor/SettingsSheetPrefabBuilder.cs:14-15.
                Check(theme, "ToggleOn", null);
                Check(theme, "ToggleOff", null);
                foreach (string name in new[] { "Primary", "Secondary", "Neutral" })
                {
                    object style = Read(theme, name);
                    // Game2 sources: View/UiButton.cs:14-15; Editor/UiKit.cs:57,85-86;
                    // Editor/LeaveConfirmSheetPrefabBuilder.cs:34-37; View/FaceLabel.cs:7.
                    Check(style, "Enabled", null);
                    Check(style, "Locked", null);
                    Check(style, "LabelMaterial", null);
                    Check(style, "LabelColor", Color.white);
                    Check(style, "FontSize", 34f);
                    Check(style, "FaceCentreFromTop", 0.41f);
                    Check(style, "DefaultSize", new Vector2(246f, 88f));
                    Check(style, "LabelPadding", new Vector2(24f, 16f));
                }
                // Game2 source: Assets/Scripts/View/UiSheet.cs:60-68,81-85,93.
                object motion = Read(theme, "Motion");
                Check(motion, "OpenSeconds", 0.22f);
                Check(motion, "ScrimFadeSeconds", 0.15f);
                Check(motion, "OpenStartScale", 0.86f);
                Check(motion, "OpenOvershoot", 1.04f);
                Check(motion, "OvershootAt", 0.65f);
                Check(motion, "CloseSeconds", 0.12f);
                Check(motion, "CloseEndScale", 0.92f);
                Check(theme, "ScrimColor", new Color(0f, 0.04f, 0.12f, 0.72f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(theme);
            }
        }

        [Test]
        public void Asmdef_ReferencesOnlyAllowedAssemblies()
        {
            string json = File.ReadAllText(Path.Combine(PackagePath, "Runtime/GameKit.UI.asmdef"));
            AssemblyDefinition definition = JsonUtility.FromJson<AssemblyDefinition>(json);
            Assert.That(definition.references, Is.Not.Null);
            Assert.That(definition.references, Is.SubsetOf(new[]
                { "UnityEngine.UI", "Unity.TextMeshPro", "Unity.InputSystem" }));
            Assert.That(json, Does.Not.Contain("Game" + "2"));
        }

        [Test]
        public void Package_ContainsNoGame2Text()
        {
            foreach (string path in Directory.GetFiles(Path.Combine(PackagePath, "Runtime"), "*.cs",
                SearchOption.AllDirectories))
                Assert.That(File.ReadAllText(path), Does.Not.Contain("Game" + "2"), path);
        }

        private static object Read(object target, string name)
        {
            Assert.That(target, Is.Not.Null, name);
            var field = target.GetType().GetField(name);
            Assert.That(field, Is.Not.Null, name);
            return field.GetValue(target);
        }

        private static void Check(object target, string name, object expected) =>
            Assert.That(Read(target, name), Is.EqualTo(expected), name);

        [Serializable]
        private sealed class AssemblyDefinition
        {
            public string[] references = Array.Empty<string>();
        }
    }
}
