using System;
using System.Linq;
using GameKit.UI.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameKit.UI.Tests
{
    public sealed class UiValidatorOptionsTests
    {
        [Test]
        public void EmptyManagedNamespacePrefixesDoNotReportCustomComponentReferences()
        {
            var root = new GameObject("Root");
            try
            {
                root.AddComponent<CustomComponent>();
                var options = new UiValidatorOptions
                {
                    ManagedNamespacePrefixes = Array.Empty<string>()
                };

                Assert.That(UiValidator.Check(root, options).Any(issue =>
                    issue.Rule == UiRule.NullObjectReference), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void SkipLabelCheckSuppressesLabelOffFace()
        {
            var root = new GameObject("Root", typeof(RectTransform));
            try
            {
                var buttonObject = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
                buttonObject.transform.SetParent(root.transform, false);
                ((RectTransform)buttonObject.transform).sizeDelta = new Vector2(200, 100);
                var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelObject.transform.SetParent(buttonObject.transform, false);
                var label = labelObject.GetComponent<TextMeshProUGUI>();
                label.text = "Label";
                label.rectTransform.anchoredPosition = new Vector2(0, 50);
                var options = new UiValidatorOptions
                {
                    ManagedNamespacePrefixes = Array.Empty<string>(),
                    SkipLabelCheck = _ => true
                };

                Assert.That(UiValidator.Check(root, options).Any(issue =>
                    issue.Rule == UiRule.LabelOffFace), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private sealed class CustomComponent : MonoBehaviour
        {
            [SerializeField] private GameObject _reference;
        }
    }
}
