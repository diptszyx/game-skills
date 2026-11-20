using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GameKit.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameKit.UI.Editor
{
    public enum UiRule
    {
        SlicedWithoutBorder,
        LabelOffFace,
        TextMissingFontOrMaterial,
        NullObjectReference,
        TouchTargetTooSmall,
        MissingScript,
        SpriteStretch,
        SpriteImportNotSingle,
        ThemeFieldMissing
    }

    public enum UiSeverity { Error, Warning }

    public sealed class UiIssue
    {
        public UiSeverity Severity { get; }
        public UiRule Rule { get; }
        public string Path { get; }
        public string Message { get; }

        public UiIssue(UiRule rule, string path, string message, UiSeverity? severity = null)
        {
            Severity = severity ?? (rule == UiRule.SpriteStretch || rule == UiRule.SpriteImportNotSingle
                ? UiSeverity.Warning : UiSeverity.Error);
            Rule = rule;
            Path = path;
            Message = message;
        }
    }

    public sealed class UiValidatorOptions
    {
        public float MinTouchPx = 80f;
        public Dictionary<UiRule, UiSeverity> SeverityOverrides;
        public string[] ManagedNamespacePrefixes;
        public Func<Button, bool> SkipLabelCheck;
        public Func<string, bool> IsUiPrefabPath;
        public float FaceCentreFromTop = FaceLabel.FaceCentreFromTop;
    }

    public static class UiValidator
    {
        // Tự đặt, chưa có nguồn: minimum target at reference resolution 720x1600.
        public const float MinTouchPx = 80f;
        // Tự đặt, chưa có nguồn: label centre tolerance in button-local UI units.
        public const float LabelTolerance = 2f;

        public static IReadOnlyList<UiIssue> Check(GameObject root, UiValidatorOptions options)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (options == null) throw new ArgumentNullException(nameof(options));
            var issues = new List<UiIssue>();
            foreach (Transform node in root.GetComponentsInChildren<Transform>(true))
            {
                string path = PathFromRoot(node, root.transform);
                foreach (Component component in node.GetComponents<Component>())
                {
                    if (component == null)
                    {
                        issues.Add(new UiIssue(UiRule.MissingScript, path, "Missing component script."));
                        continue;
                    }
                    if (component is Image image && image.type == Image.Type.Sliced && image.sprite != null &&
                        image.sprite.border == Vector4.zero)
                        issues.Add(new UiIssue(UiRule.SlicedWithoutBorder, path,
                            $"Sprite {image.sprite.name} has zero border."));
                    if (component is Image spriteImage) CheckSprite(spriteImage, path, issues);
                    if (component is TMP_Text text && (text.font == null || text.fontSharedMaterial == null))
                        issues.Add(new UiIssue(UiRule.TextMissingFontOrMaterial, path,
                            $"font missing={text.font == null}; material missing={text.fontSharedMaterial == null}.",
                            text.font == null ? UiSeverity.Error : UiSeverity.Warning));
                    if (component is Button button)
                    {
                        CheckTarget(button, path, issues, options.MinTouchPx);
                        if (options.SkipLabelCheck == null || !options.SkipLabelCheck(button)) CheckLabels(button, root.transform, issues, options.FaceCentreFromTop);
                    }
                    if (component is Toggle toggle) CheckTarget(toggle, path, issues, options.MinTouchPx);
                    string componentNamespace = component.GetType().Namespace;
                    if (componentNamespace == null || options.ManagedNamespacePrefixes == null ||
                        !options.ManagedNamespacePrefixes.Any(prefix => componentNamespace.StartsWith(prefix,
                            StringComparison.Ordinal))) continue;
                    using (var serialized = new SerializedObject(component))
                    {
                        SerializedProperty property = serialized.GetIterator();
                        while (property.Next(true))
                        {
                            if (property.propertyType == SerializedPropertyType.ObjectReference &&
                                property.objectReferenceValue is UiTheme theme && AssetDatabase.Contains(theme))
                                foreach (string missing in UiThemeRequirements.MissingRequiredFields(theme))
                                    issues.Add(new UiIssue(UiRule.ThemeFieldMissing, path,
                                        $"{component.GetType().FullName}.{property.propertyPath}.{missing} is missing."));
                            if (property.propertyType == SerializedPropertyType.ObjectReference &&
                                property.objectReferenceValue == null &&
                                IsManagedField(component.GetType(), property.propertyPath, options.ManagedNamespacePrefixes))
                                issues.Add(new UiIssue(UiRule.NullObjectReference, path,
                                    $"{component.GetType().FullName}.{property.propertyPath} is null."));
                        }
                    }
                }
            }
            if (options.SeverityOverrides != null)
                for (int i = 0; i < issues.Count; i++)
                    if (options.SeverityOverrides.TryGetValue(issues[i].Rule, out UiSeverity severity))
                        issues[i] = new UiIssue(issues[i].Rule, issues[i].Path, issues[i].Message, severity);
            return issues;
        }

        public static string RunAll(UiValidatorOptions options, string reportPath)
        {
            string[] paths = AssetDatabase.FindAssets("t:Prefab").Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => options.IsUiPrefabPath != null && options.IsUiPrefabPath(path)).OrderBy(path => path, StringComparer.Ordinal).ToArray();
            var lines = new List<string>();
            var totals = new int[Enum.GetValues(typeof(UiRule)).Length];
            int errors = 0, warnings = 0;
            foreach (string path in paths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                foreach (UiIssue issue in Check(prefab, options))
                {
                    lines.Add($"{path}\t{issue.Rule}\t{issue.Path}\t{issue.Message}\t{issue.Severity}");
                    totals[(int)issue.Rule]++;
                    if (issue.Severity == UiSeverity.Error) errors++; else warnings++;
                }
            }
            lines.Add($"Prefabs checked: {paths.Length}");
            foreach (UiRule rule in Enum.GetValues(typeof(UiRule))) lines.Add($"TOTAL\t{rule}\t{totals[(int)rule]}");
            lines.Add($"TOTAL\tError\t{errors}");
            lines.Add($"TOTAL\tWarning\t{warnings}");
            string report = System.IO.Path.GetFullPath(reportPath);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(report));
            File.WriteAllLines(report, lines);
            return report;
        }

        // TỰ ĐẶT: warn above 10% anisotropic scaling.
        public const float SpriteStretchThreshold = 1.10f;

        private static void CheckSprite(Image image, string path, List<UiIssue> issues)
        {
            Sprite sprite = image.sprite;
            if (sprite == null) return;
            var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite)) as TextureImporter;
            if (importer != null && importer.spriteImportMode != SpriteImportMode.Single)
                issues.Add(new UiIssue(UiRule.SpriteImportNotSingle, path,
                    $"Sprite {sprite.name} import mode={importer.spriteImportMode}; expected Single."));
            Vector2 size = image.rectTransform.rect.size;
            if (image.type == Image.Type.Simple && !image.preserveAspect && size.x > 0 && size.y > 0)
            {
                float sx = size.x / sprite.rect.width;
                float sy = size.y / sprite.rect.height;
                float ratio = Mathf.Max(sx / sy, sy / sx);
                if (ratio > SpriteStretchThreshold)
                    issues.Add(new UiIssue(UiRule.SpriteStretch, path,
                        $"Sprite {sprite.name} stretch={ratio:F4}; threshold={SpriteStretchThreshold:F2} (TỰ ĐẶT)."));
            }
            else if (image.type == Image.Type.Sliced || image.type == Image.Type.Tiled)
            {
                Vector4 border = sprite.border / image.pixelsPerUnitMultiplier;
                if (size.x < border.x + border.z || size.y < border.y + border.w)
                    issues.Add(new UiIssue(UiRule.SpriteStretch, path,
                        $"Sprite {sprite.name} rect={size.x:F2}x{size.y:F2}; minimum border=" +
                        $"{border.x + border.z:F2}x{border.y + border.w:F2}."));
            }
        }

        private static void CheckLabels(Button button, Transform root, List<UiIssue> issues, float faceCentreFromTop)
        {
            var rect = button.transform as RectTransform;
            if (rect == null || button.GetComponentsInChildren<Selectable>(true).Any(child => child != button)) return;
            Vector2 face = new Vector2(rect.rect.center.x,
                rect.rect.yMax - rect.rect.height * faceCentreFromTop);
            TMP_Text[] labels = button.GetComponentsInChildren<TMP_Text>(true)
                .Where(label => label.GetComponentInParent<Button>(true) == button).ToArray();
            // Icon-and-caption rows have multiple deliberate horizontal text positions.
            bool compoundRow = labels.Count(label => !string.IsNullOrWhiteSpace(label.text)) > 1 &&
                button.GetComponentsInChildren<Image>(true).Any(image => image.transform != button.transform);
            foreach (TMP_Text label in labels)
            {
                Vector3 centre = rect.InverseTransformPoint(label.rectTransform.TransformPoint(
                    label.rectTransform.rect.center));
                float offset = compoundRow ? Mathf.Abs(centre.y - face.y) : Vector2.Distance(centre, face);
                if (offset > LabelTolerance)
                    issues.Add(new UiIssue(UiRule.LabelOffFace, PathFromRoot(label.transform, root),
                        $"Label offset={offset:F2}; tolerance={LabelTolerance:F2} local UI units (tự đặt); " +
                        $"axes={(compoundRow ? "vertical (compound icon/caption row)" : "both")}."));
            }
        }

        private static void CheckTarget(Selectable target, string path, List<UiIssue> issues, float minTouchPx)
        {
            var rect = target.transform as RectTransform;
            if (rect == null) return;
            Vector2 size = EffectiveSize(rect);
            Vector4 padding = target.targetGraphic == null ? Vector4.zero : target.targetGraphic.raycastPadding;
            float scale = ReferenceScale(rect);
            float width = (size.x - padding.x - padding.z) * Mathf.Abs(rect.lossyScale.x) * scale;
            float height = (size.y - padding.y - padding.w) * Mathf.Abs(rect.lossyScale.y) * scale;
            if (width < minTouchPx || height < minTouchPx)
                issues.Add(new UiIssue(UiRule.TouchTargetTooSmall, path,
                    $"Target={width:F2}x{height:F2}px at 720x1600; minimum={minTouchPx:F0}px (tự đặt)."));
        }

        private static bool IsManagedField(Type type, string path, string[] managedNamespacePrefixes)
        {
            string name = path.Split('.')[0];
            while (type != null && type.Namespace != null && managedNamespacePrefixes != null &&
                managedNamespacePrefixes.Any(prefix => type.Namespace.StartsWith(prefix, StringComparison.Ordinal)))
            {
                if (type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly) != null) return true;
                type = type.BaseType;
            }
            return false;
        }

        private static Vector2 EffectiveSize(RectTransform rect)
        {
            Vector2 parentSize = rect.parent is RectTransform parent ? EffectiveSize(parent) : new Vector2(720, 1600);
            Vector2 size = rect.sizeDelta + Vector2.Scale(parentSize, rect.anchorMax - rect.anchorMin);
            AspectRatioFitter fitter = rect.GetComponent<AspectRatioFitter>();
            if (fitter != null && fitter.enabled && fitter.aspectRatio > 0f)
            {
                if (fitter.aspectMode == AspectRatioFitter.AspectMode.HeightControlsWidth)
                    size.x = size.y * fitter.aspectRatio;
                else if (fitter.aspectMode == AspectRatioFitter.AspectMode.WidthControlsHeight)
                    size.y = size.x / fitter.aspectRatio;
            }
            return size;
        }

        private static float ReferenceScale(RectTransform rect)
        {
            Canvas canvas = rect.GetComponentInParent<Canvas>(true);
            if (canvas == null) return 1f;
            Canvas root = canvas.rootCanvas;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            float units = 1f / Mathf.Abs(root.transform.lossyScale.x);
            if (scaler == null) return units * root.scaleFactor;
            if (scaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPixelSize) return units * scaler.scaleFactor;
            if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize) return units * root.scaleFactor;
            float x = 720f / scaler.referenceResolution.x;
            float y = 1600f / scaler.referenceResolution.y;
            float factor = scaler.screenMatchMode == CanvasScaler.ScreenMatchMode.Expand ? Mathf.Min(x, y) :
                scaler.screenMatchMode == CanvasScaler.ScreenMatchMode.Shrink ? Mathf.Max(x, y) :
                Mathf.Pow(2f, Mathf.Lerp(Mathf.Log(x, 2f), Mathf.Log(y, 2f), scaler.matchWidthOrHeight));
            return units * factor;
        }

        private static string PathFromRoot(Transform node, Transform root)
        {
            var names = new List<string>();
            for (Transform current = node; current != null; current = current.parent)
            {
                names.Add(current.name);
                if (current == root) break;
            }
            names.Reverse();
            return string.Join("/", names);
        }
    }
}
