using System;
using System.Collections.Generic;
using System.Linq;
using GameKit.UI.Editor;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameKit.UI.Tests
{
    public sealed class UiValidatorSeverityTests
    {
        [TestCase(UiRule.MissingScript, UiSeverity.Error)]
        [TestCase(UiRule.NullObjectReference, UiSeverity.Error)]
        [TestCase(UiRule.SlicedWithoutBorder, UiSeverity.Error)]
        [TestCase(UiRule.TouchTargetTooSmall, UiSeverity.Error)]
        [TestCase(UiRule.LabelOffFace, UiSeverity.Error)]
        [TestCase(UiRule.ThemeFieldMissing, UiSeverity.Error)]
        [TestCase(UiRule.TextMissingFontOrMaterial, UiSeverity.Error)]
        [TestCase(UiRule.SpriteStretch, UiSeverity.Warning)]
        [TestCase(UiRule.SpriteImportNotSingle, UiSeverity.Warning)]
        public void DefaultSeverity(UiRule rule, UiSeverity expected) =>
            Assert.AreEqual(expected, new UiIssue(rule, "Root", "fixture").Severity);

        [Test]
        public void OverrideAndMinTouchPxAffectCheck()
        {
            var root = new GameObject("Target", typeof(RectTransform), typeof(Image), typeof(Button));
            try
            {
                ((RectTransform)root.transform).sizeDelta = new Vector2(90, 90);
                var options = new UiValidatorOptions
                {
                    MinTouchPx = 100,
                    SeverityOverrides = new Dictionary<UiRule, UiSeverity> {
                        { UiRule.TouchTargetTooSmall, UiSeverity.Warning } }
                };
                UiIssue issue = UiValidator.Check(root, options).Single(i => i.Rule == UiRule.TouchTargetTooSmall);
                Assert.AreEqual(UiSeverity.Warning, issue.Severity);
                options.MinTouchPx = 80;
                Assert.IsFalse(UiValidator.Check(root, options).Any(i => i.Rule == UiRule.TouchTargetTooSmall));
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        [Test]
        public void FontMissingIsErrorMaterialOnlyIsWarning()
        {
            var root = new GameObject("Text", typeof(RectTransform));
            root.SetActive(false);
            root.AddComponent<TextMeshProUGUI>();
            try
            {
                var text = root.GetComponent<TextMeshProUGUI>();
                text.font = TMP_Settings.defaultFontAsset;
                Assert.NotNull(text.font);
                text.fontSharedMaterial = null;
                Assert.AreEqual(UiSeverity.Warning, UiValidator.Check(root, new UiValidatorOptions())
                    .Single(i => i.Rule == UiRule.TextMissingFontOrMaterial).Severity);
                root.SetActive(false);
                using (var serialized = new SerializedObject(text))
                {
                    serialized.FindProperty("m_fontAsset").objectReferenceValue = null;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
                Assert.AreEqual(UiSeverity.Error, UiValidator.Check(root, new UiValidatorOptions())
                    .Single(i => i.Rule == UiRule.TextMissingFontOrMaterial).Severity);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
    }
}
