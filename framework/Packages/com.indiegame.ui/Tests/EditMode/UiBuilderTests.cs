using System.Linq;
using GameKit.UI;
using GameKit.UI.Editor;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameKit.UI.Tests
{
    public sealed class UiBuilderTests
    {
        private const string Temp = "Assets/_KitTmp";

        [Test]
        public void BuildModal_DemoTheme_ProducesStructureAndNoValidatorIssues()
        {
            try
            {
                AssetDatabase.DeleteAsset(Temp);
                AssetDatabase.CreateFolder("Assets", "_KitTmp");
                TMP_FontAsset font = TMP_Settings.defaultFontAsset;
                if (font == null) font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                if (font == null) Assert.Ignore("TMP Essentials missing");
                UiTheme theme = DemoThemeFactory.Create(Temp, font);
                string path = Temp + "/Demo.prefab";
                UiBuilder.BuildModal(theme, new ModalSpec { Name = "Demo", Title = "Demo", Body = "Body", Buttons = new[] {
                    new ModalButton { Label = "Yes", Kind = UiButtonKind.Primary },
                    new ModalButton { Label = "No", Kind = UiButtonKind.Secondary }
                } }, path);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.NotNull(prefab.transform.Find("Panel/Title"));
                Assert.NotNull(prefab.transform.Find("Panel/Body"));
                Assert.NotNull(prefab.transform.Find("Panel/Close"));
                Assert.AreEqual(2, prefab.transform.Find("Panel").Cast<Transform>().Count(x => x.name.StartsWith("Button")));
                UiModal modal = prefab.GetComponent<UiModal>();
                Assert.NotNull(modal);
                var serialized = new SerializedObject(modal);
                Assert.NotNull(serialized.FindProperty("_panel").objectReferenceValue);
                Assert.NotNull(serialized.FindProperty("_scrim").objectReferenceValue);
                Assert.NotNull(serialized.FindProperty("_theme").objectReferenceValue);
                Assert.IsFalse(prefab.GetComponentsInChildren<Component>(true).Any(c => c != null && c.GetType().Namespace.StartsWith("Game2.")));
                var issues = UiValidator.Check(prefab, new UiValidatorOptions { ManagedNamespacePrefixes = new[] { "GameKit." }, SkipLabelCheck = _ => false, FaceCentreFromTop = theme.Primary.FaceCentreFromTop });
                Assert.AreEqual(0, issues.Count, string.Join("\n", issues.Select(i => i.Message)));
                foreach (Button button in prefab.GetComponentsInChildren<Button>(true).Where(b => b.name.StartsWith("Button")))
                {
                    var style = button.name.Contains("Primary") ? theme.Primary : theme.Secondary;
                    RectTransform label = button.transform.Find("Label") as RectTransform;
                    float expected = -((RectTransform)button.transform).rect.height * style.FaceCentreFromTop;
                    Assert.AreEqual(expected, label.anchoredPosition.y, UiValidator.LabelTolerance);
                }
            }
            finally { AssetDatabase.DeleteAsset(Temp); }
        }

        [Test]
        public void BuildModal_UsesIdsAndAllThemeGeometry()
        {
            try
            {
                AssetDatabase.DeleteAsset(Temp); AssetDatabase.CreateFolder("Assets", "_KitTmp");
                TMP_FontAsset font = TMP_Settings.defaultFontAsset;
                if (font == null) font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                if (font == null) Assert.Ignore("TMP Essentials missing");
                UiTheme theme = DemoThemeFactory.Create(Temp, font);
                theme.TitleFontSize = 47f; theme.BodyFontSize = 37f;
                theme.TitleSize = new Vector2(461f, 101f); theme.BodySize = new Vector2(501f, 121f);
                theme.BodyOffsetY = 32f; theme.ButtonRowOffsetY = -151f;
                theme.TwoButtonOffsetX = 146f; theme.PanelPixelsPerUnitMultiplier = 1.1f;
                string path = Temp + "/Geometry.prefab";
                UiBuilder.BuildModal(theme, new ModalSpec { Title = "Title", Body = "Body", Buttons = new[] {
                    new ModalButton { Id = "Stay", Label = "Stay", Kind = UiButtonKind.Primary },
                    new ModalButton { Label = "Leave", Kind = UiButtonKind.Secondary }
                } }, path);
                Transform panel = AssetDatabase.LoadAssetAtPath<GameObject>(path).transform.Find("Panel");
                Assert.AreEqual(theme.PanelPixelsPerUnitMultiplier, panel.GetComponent<Image>().pixelsPerUnitMultiplier);
                TMP_Text title = panel.Find("Title").GetComponent<TMP_Text>();
                TMP_Text body = panel.Find("Body").GetComponent<TMP_Text>();
                Assert.AreEqual(theme.TitleFontSize, title.fontSize);
                Assert.AreEqual(theme.TitleSize, title.rectTransform.sizeDelta);
                Assert.AreEqual(theme.BodyFontSize, body.fontSize);
                Assert.AreEqual(theme.BodySize, body.rectTransform.sizeDelta);
                Assert.AreEqual(theme.BodyOffsetY, body.rectTransform.anchoredPosition.y);
                Assert.AreEqual(new Vector2(-146f, -151f), ((RectTransform)panel.Find("Stay")).anchoredPosition);
                Assert.AreEqual(new Vector2(146f, -151f), ((RectTransform)panel.Find("ButtonSecondary_2")).anchoredPosition);
            }
            finally { AssetDatabase.DeleteAsset(Temp); }
        }

        [Test]
        public void BuildModal_StraightTitle_HasNoCurvedText()
        {
            try
            {
                AssetDatabase.DeleteAsset(Temp); AssetDatabase.CreateFolder("Assets", "_KitTmp");
                TMP_FontAsset font = TMP_Settings.defaultFontAsset;
                if (font == null) font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                if (font == null) Assert.Ignore("TMP Essentials missing");
                UiTheme theme = DemoThemeFactory.Create(Temp, font); theme.CurvedTitle = false;
                string path = Temp + "/Straight.prefab";
                UiBuilder.BuildModal(theme, new ModalSpec { Name = "Straight", Title = "Straight" }, path);
                Assert.IsNull(AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponentInChildren<CurvedText>(true));
            }
            finally { AssetDatabase.DeleteAsset(Temp); }
        }

        [Test]
        public void BuildModal_NullSpriteInTheme_ThrowsNamedField()
        {
            try
            {
                AssetDatabase.DeleteAsset(Temp); AssetDatabase.CreateFolder("Assets", "_KitTmp");
                TMP_FontAsset font = TMP_Settings.defaultFontAsset;
                if (font == null) font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                if (font == null) Assert.Ignore("TMP Essentials missing");
                UiTheme theme = DemoThemeFactory.Create(Temp, font); theme.PanelSprite = null;
                var ex = Assert.Throws<System.InvalidOperationException>(() => UiBuilder.BuildModal(theme,
                    new ModalSpec { Name = "Invalid", Title = "Invalid" }, Temp + "/Bad.prefab"));
                StringAssert.Contains("PanelSprite", ex.Message);
                Assert.IsNull(AssetDatabase.LoadAssetAtPath<GameObject>(Temp + "/Bad.prefab"));
            }
            finally { AssetDatabase.DeleteAsset(Temp); }
        }
    }
}
