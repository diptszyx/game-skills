using System.IO;
using System.Linq;
using GameKit.UI.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameKit.UI.Tests
{
    public sealed class UiValidatorSpriteRulesTests
    {
        private const string Folder = "Assets/_KitTmp";

        [TestCase(1.2f, false, true)]
        [TestCase(1.05f, false, false)]
        [TestCase(1.2f, true, false)]
        [TestCase(1f, false, false)]
        public void SimpleStretch(float ratio, bool preserve, bool expected) =>
            WithImage(SpriteImportMode.Single, Vector4.zero, image =>
            {
                image.rectTransform.sizeDelta = new Vector2(100 * ratio, 100);
                image.preserveAspect = preserve;
                Assert.AreEqual(expected, UiValidator.Check(image.gameObject, new UiValidatorOptions())
                    .Any(i => i.Rule == UiRule.SpriteStretch));
            });

        [TestCase(Image.Type.Sliced, 39f, 1f, true)]
        [TestCase(Image.Type.Sliced, 40f, 1f, false)]
        [TestCase(Image.Type.Sliced, 20f, 2f, false)]
        [TestCase(Image.Type.Sliced, 200f, 1f, false)]
        [TestCase(Image.Type.Tiled, 39f, 1f, true)]
        [TestCase(Image.Type.Tiled, 40f, 1f, false)]
        public void BorderMinimum(Image.Type type, float width, float multiplier, bool expected) =>
            WithImage(SpriteImportMode.Single, new Vector4(20, 10, 20, 10), image =>
            {
                image.type = type;
                image.pixelsPerUnitMultiplier = multiplier;
                image.rectTransform.sizeDelta = new Vector2(width, 100);
                Assert.AreEqual(expected, UiValidator.Check(image.gameObject, new UiValidatorOptions())
                    .Any(i => i.Rule == UiRule.SpriteStretch));
            });

        [TestCase(SpriteImportMode.Multiple, true)]
        [TestCase(SpriteImportMode.Single, false)]
        public void ImportModeWarning(SpriteImportMode mode, bool expected) =>
            WithImage(mode, Vector4.zero, image =>
            {
                var issues = UiValidator.Check(image.gameObject, new UiValidatorOptions())
                    .Where(i => i.Rule == UiRule.SpriteImportNotSingle).ToArray();
                Assert.AreEqual(expected ? 1 : 0, issues.Length);
                if (expected) Assert.AreEqual(UiSeverity.Warning, issues[0].Severity);
                Assert.AreEqual(mode, ((TextureImporter)AssetImporter.GetAtPath(
                    AssetDatabase.GetAssetPath(image.sprite))).spriteImportMode);
            });

        private static void WithImage(SpriteImportMode mode, Vector4 border, System.Action<Image> check)
        {
            GameObject root = null;
            bool ownsFolder = false;
            Texture2D texture = null;
            try
            {
                Assert.IsFalse(AssetDatabase.IsValidFolder(Folder), "Temporary fixture folder already exists.");
                AssetDatabase.CreateFolder("Assets", "_KitTmp");
                ownsFolder = true;
                texture = new Texture2D(100, 100);
                string path = Folder + "/sprite.png";
                File.WriteAllBytes(path, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = mode;
                importer.spriteBorder = border;
                if (mode == SpriteImportMode.Multiple)
                {
#pragma warning disable CS0618
                    importer.spritesheet = new[] { new SpriteMetaData {
                        name = "fixture", rect = new Rect(0, 0, 100, 100), border = border } };
#pragma warning restore CS0618
                }
                importer.SaveAndReimport();
                Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Single();
                root = new GameObject("Image", typeof(RectTransform), typeof(Image));
                Image image = root.GetComponent<Image>();
                image.sprite = sprite;
                image.rectTransform.sizeDelta = new Vector2(100, 100);
                check(image);
            }
            finally
            {
                if (root != null) Object.DestroyImmediate(root);
                if (texture != null) Object.DestroyImmediate(texture);
                if (ownsFolder) AssetDatabase.DeleteAsset(Folder);
            }
        }
    }
}
