using System;
using System.Linq;
using GameKit.UI.Editor;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace GameKit.UI.Tests
{
    public sealed class UiValidatorThemeTests
    {
        private const string Folder = "Assets/_KitTmp";

        [TestCase(false)]
        [TestCase(true)]
        public void ThemeReferenceChecksRequiredFields(bool missingFont)
        {
            WithTheme((root, theme) =>
            {
                if (missingFont) theme.Font = null;
                var issues = UiValidator.Check(root, new UiValidatorOptions
                {
                    ManagedNamespacePrefixes = new[] { "GameKit." }
                })
                    .Where(i => i.Rule == UiRule.ThemeFieldMissing).ToArray();
                Assert.AreEqual(missingFont ? 1 : 0, issues.Length);
                if (missingFont)
                {
                    StringAssert.Contains("._theme.Font", issues[0].Message);
                    Assert.AreEqual(UiSeverity.Error, issues[0].Severity);
                }
                Assert.IsFalse(UiValidator.Check(root, new UiValidatorOptions
                {
                    ManagedNamespacePrefixes = new[] { "Other." }
                })
                    .Any(i => i.Rule == UiRule.ThemeFieldMissing));
            });
        }

        [Test]
        public void BuilderAndValidatorReportSameRequiredFields()
        {
            WithTheme((root, theme) =>
            {
                theme.Font = null;
                theme.BodyMaterial = null;
                theme.TitleMaterial = null;
                theme.PanelSprite = null;
                theme.CloseSprite = null;
                theme.Primary = null;
                theme.Secondary.Enabled = null;
                theme.Secondary.LabelMaterial = null;
                theme.Neutral.Enabled = null;
                theme.Neutral.LabelMaterial = null;
                string[] expected = { "Font", "BodyMaterial", "TitleMaterial", "PanelSprite", "CloseSprite",
                    "Primary.Enabled", "Primary.LabelMaterial", "Secondary.Enabled", "Secondary.LabelMaterial",
                    "Neutral.Enabled", "Neutral.LabelMaterial" };
                CollectionAssert.AreEqual(expected, UiThemeRequirements.MissingRequiredFields(theme));
                var issues = UiValidator.Check(root, new UiValidatorOptions
                {
                    ManagedNamespacePrefixes = new[] { "GameKit." }
                })
                    .Where(i => i.Rule == UiRule.ThemeFieldMissing).ToArray();
                Assert.AreEqual(expected.Length, issues.Length);
                var error = Assert.Throws<InvalidOperationException>(() => UiBuilder.BuildModal(theme,
                    new ModalSpec(), Folder + "/Invalid.prefab"));
                foreach (string field in expected)
                {
                    Assert.IsTrue(issues.Any(i => i.Message.EndsWith("._theme." + field + " is missing.")), field);
                    StringAssert.Contains(field, error.Message);
                }
                Assert.IsNull(AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/Invalid.prefab"));
            });
        }

        private static void WithTheme(Action<GameObject, UiTheme> check)
        {
            GameObject root = null;
            bool ownsFolder = false;
            try
            {
                Assert.IsFalse(AssetDatabase.IsValidFolder(Folder), "Temporary fixture folder already exists.");
                AssetDatabase.CreateFolder("Assets", "_KitTmp");
                ownsFolder = true;
                TMP_FontAsset font = TMP_Settings.defaultFontAsset;
                Assert.NotNull(font);
                UiTheme theme = DemoThemeFactory.Create(Folder, font);
                root = new GameObject("Root", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                var modal = root.AddComponent<UiModal>();
                using (var serialized = new SerializedObject(modal))
                {
                    serialized.FindProperty("_theme").objectReferenceValue = theme;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
                check(root, theme);
            }
            finally
            {
                if (root != null) UnityEngine.Object.DestroyImmediate(root);
                if (ownsFolder) AssetDatabase.DeleteAsset(Folder);
            }
        }
    }
}
