using GameKit.UI.Editor;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace GameKit.UI.Tests
{
    public sealed class DemoThemeTests
    {
        [Test]
        public void DemoTheme_SpritesAreSingleWithBorder()
        {
            const string folder = "Assets/_KitTmp";
            try
            {
                AssetDatabase.DeleteAsset(folder); AssetDatabase.CreateFolder("Assets", "_KitTmp");
                TMP_FontAsset font = TMP_Settings.defaultFontAsset;
                if (font == null) font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                if (font == null) Assert.Ignore("TMP Essentials missing");
                var theme = DemoThemeFactory.Create(folder, font);
                Assert.AreEqual(42f, theme.TitleFontSize);
                Assert.AreEqual(30f, theme.BodyFontSize);
                Assert.AreEqual(-45f, theme.BodyOffsetY);
                Assert.AreEqual(-460f * .27f, theme.ButtonRowOffsetY);
                Assert.AreEqual(145f, theme.TwoButtonOffsetX);
                Assert.AreEqual(256f / 460f, theme.PanelPixelsPerUnitMultiplier);
                Assert.AreEqual(new Vector2(480f, 100f), theme.TitleSize);
                Assert.AreEqual(new Vector2(460f, 170f), theme.BodySize);
                foreach (Sprite sprite in new[] { theme.PanelSprite, theme.Primary.Enabled, theme.Secondary.Enabled, theme.Neutral.Enabled, theme.CloseSprite })
                {
                    string path = AssetDatabase.GetAssetPath(sprite);
                    var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                    Assert.AreEqual(SpriteImportMode.Single, importer.spriteImportMode, path);
                    Assert.AreNotEqual(Vector4.zero, sprite.border, path);
                }
            }
            finally { AssetDatabase.DeleteAsset(folder); }
        }
    }
}
