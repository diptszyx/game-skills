using System;
using GameKit.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameKit.UI.Editor
{
    public enum UiButtonKind { Primary, Secondary, Neutral }

    [Serializable]
    public sealed class ModalButton
    {
        public string Id;
        public string Label;
        public UiButtonKind Kind;
    }

    [Serializable]
    public sealed class ModalSpec
    {
        public string Name;
        public string Title;
        public string Body;
        public ModalButton[] Buttons = Array.Empty<ModalButton>();
        public bool Close = true;
    }

    public static class UiBuilder
    {
        public static GameObject BuildModal(UiTheme theme, ModalSpec spec, string prefabPath)
        {
            if (theme == null) throw new ArgumentNullException(nameof(theme));
            if (spec == null) throw new ArgumentNullException(nameof(spec));
            if (spec.Buttons == null) spec.Buttons = Array.Empty<ModalButton>();
            if (spec.Buttons.Length > 3) throw new ArgumentException("Modal supports at most 3 buttons.", nameof(spec));
            ValidateTheme(theme, spec);
            GameObject root = null;
            try
            {
                root = new GameObject(string.IsNullOrWhiteSpace(spec.Name) ? "Modal" : spec.Name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(UiModal));
                RectTransform rootRect = (RectTransform)root.transform;
                Stretch(rootRect);
                Image scrim = root.GetComponent<Image>(); scrim.color = theme.ScrimColor; scrim.raycastTarget = true;
                Button rootButton = root.GetComponent<Button>(); rootButton.targetGraphic = scrim;
                RectTransform panel = Node("Panel", root.transform, theme.PanelSize, Vector2.zero);
                Image panelImage = panel.gameObject.AddComponent<Image>(); panelImage.sprite = theme.PanelSprite; panelImage.type = Image.Type.Sliced; panelImage.pixelsPerUnitMultiplier = theme.PanelPixelsPerUnitMultiplier;
                Button panelButton = panel.gameObject.AddComponent<Button>(); panelButton.targetGraphic = panelImage; panelButton.transition = Selectable.Transition.None;
                TMP_Text title = Text(panel, "Title", spec.Title, theme.TitleSize, new Vector2(0f, theme.TitleOffsetY), theme, theme.TitleMaterial, Color.white, theme.TitleFontSize);
                RectTransform titleRect = title.rectTransform;
                titleRect.anchorMin = titleRect.anchorMax = new Vector2(.5f, 1f);
                titleRect.anchoredPosition = new Vector2(0f, theme.TitleOffsetY);
                if (theme.CurvedTitle) { var curved = title.gameObject.AddComponent<CurvedText>(); SetFloat(curved, "_radius", theme.TitleCurveRadius); }
                if (!string.IsNullOrWhiteSpace(spec.Body)) Text(panel, "Body", spec.Body, theme.BodySize, new Vector2(0f, theme.BodyOffsetY), theme, theme.BodyMaterial, theme.BodyColor, theme.BodyFontSize);
                BuildButtons(panel, theme, spec);
                if (spec.Close)
                {
                    RectTransform close = Node("Close", panel, theme.CloseSize, theme.CloseOffset);
                    Image closeImage = close.gameObject.AddComponent<Image>(); closeImage.sprite = theme.CloseSprite; closeImage.preserveAspect = true;
                    Button closeButton = close.gameObject.AddComponent<Button>(); closeButton.targetGraphic = closeImage;
                }
                SetObject(root.GetComponent<UiModal>(), "_panel", panel);
                SetObject(root.GetComponent<UiModal>(), "_scrim", scrim);
                SetObject(root.GetComponent<UiModal>(), "_theme", theme);
                string directory = System.IO.Path.GetDirectoryName(prefabPath);
                if (!string.IsNullOrEmpty(directory)) System.IO.Directory.CreateDirectory(directory);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }
            finally { if (root != null) UnityEngine.Object.DestroyImmediate(root); }
        }

        private static void BuildButtons(RectTransform panel, UiTheme theme, ModalSpec spec)
        {
            int count = spec.Buttons.Length;
            for (int i = 0; i < count; i++)
            {
                ModalButton data = spec.Buttons[i] ?? throw new ArgumentException($"Buttons[{i}] is null.");
                UiButtonStyle style = Style(theme, data.Kind, nameof(UiTheme.Primary));
                Vector2 size = style.DefaultSize;
                float x = count == 1 ? 0f : count == 2 ? (i == 0 ? -theme.TwoButtonOffsetX : theme.TwoButtonOffsetX) : (i - 1) * (theme.PanelSize.x / 3f - 15f);
                if (count == 3) size.x = Mathf.Min(size.x, theme.PanelSize.x / 3f - 18f);
                RectTransform buttonRect = Node(string.IsNullOrEmpty(data.Id) ? $"Button{data.Kind}_{i + 1}" : data.Id, panel, size, new Vector2(x, theme.ButtonRowOffsetY));
                Image image = buttonRect.gameObject.AddComponent<Image>(); image.sprite = style.Enabled; image.type = Image.Type.Sliced; image.pixelsPerUnitMultiplier = style.Enabled.rect.height / size.y;
                Button button = buttonRect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
                TMP_Text label = Text(buttonRect, "Label", data.Label, size - style.LabelPadding, Vector2.zero, theme, style.LabelMaterial, style.LabelColor, style.FontSize);
                FaceLabel.CentreOnFace(label.rectTransform, size.y, style.FaceCentreFromTop);
            }
        }

        private static void ValidateTheme(UiTheme theme, ModalSpec spec)
        {
            string[] missing = UiThemeRequirements.MissingRequiredFields(theme);
            if (missing.Length > 0)
                throw new InvalidOperationException($"Theme fields {string.Join(", ", missing)} are missing.");
            for (int i = 0; i < spec.Buttons.Length; i++)
                if (spec.Buttons[i] == null) throw new InvalidOperationException($"Theme field Buttons[{i}] missing.");
        }
        private static void Require(UnityEngine.Object value, string field) { if (value == null) throw new InvalidOperationException($"Theme field {field} is missing."); }
        private static UiButtonStyle Style(UiTheme theme, UiButtonKind kind, string field) => kind switch
        { UiButtonKind.Primary => theme.Primary ?? throw new InvalidOperationException($"Theme field {field} is missing."), UiButtonKind.Secondary => theme.Secondary ?? throw new InvalidOperationException("Theme field Secondary is missing."), UiButtonKind.Neutral => theme.Neutral ?? throw new InvalidOperationException("Theme field Neutral is missing."), _ => throw new ArgumentOutOfRangeException(nameof(kind)) };
        private static RectTransform Node(string name, Transform parent, Vector2 size, Vector2 pos)
        { var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer)); var rect = (RectTransform)go.transform; rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.sizeDelta = size; rect.anchoredPosition = pos; return rect; }
        private static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
        private static TMP_Text Text(Transform parent, string name, string value, Vector2 size, Vector2 pos, UiTheme theme, Material material, Color color, float fontSize)
        { RectTransform rect = Node(name, parent, size, pos); var text = rect.gameObject.AddComponent<TextMeshProUGUI>(); text.text = value ?? string.Empty; text.font = theme.Font; text.fontSharedMaterial = material; text.fontSize = fontSize; text.color = color; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false; return text; }
        private static void SetObject(UnityEngine.Object target, string field, UnityEngine.Object value) { var so = new SerializedObject(target); SerializedProperty property = so.FindProperty(field); if (property == null) throw new InvalidOperationException($"Serialized field '{field}' was not found on {target.GetType().Name}."); property.objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
        private static void SetFloat(UnityEngine.Object target, string field, float value) { var so = new SerializedObject(target); SerializedProperty property = so.FindProperty(field); if (property == null) throw new InvalidOperationException($"Serialized field '{field}' was not found on {target.GetType().Name}."); property.floatValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
    }
}
