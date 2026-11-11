using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Unity.UI
{
    // Bindings only copy appearance; style switching never rebuilds state or hierarchy.
    public sealed class ShellAppearance : MonoBehaviour
    {
        public string Role;
        public ButtonRole ButtonRole;
        public void Apply(ShellStyle s)
        {
            var image = GetComponent<Image>(); var text = GetComponent<TMP_Text>();
            switch (Role)
            {
                case "screen": image.sprite = s.ScreenBackground; image.color = s.ScreenColor; break;
                case "row": image.sprite = s.ListRowSprite; image.color = s.ListRowColor; break;
                case "panel": image.sprite = s.Theme.PanelSprite; image.color = s.SurfaceColor; image.pixelsPerUnitMultiplier = s.Theme.PanelPixelsPerUnitMultiplier; break;
                case "scrim": image.sprite = s.ScreenBackground; image.color = s.Theme.ScrimColor; break;
                case "button":
                    var b = ButtonRole == ButtonRole.Secondary ? s.Theme.Secondary : s.Theme.Primary;
                    image.sprite = ButtonRole == ButtonRole.Locked ? s.LockedButtonSprite : b.Enabled;
                    image.color = s.SurfaceColor; break;
            }
            if (text != null)
            {
                text.font = s.Theme.Font;
                text.color = Role == "title" ? s.TitleColor : Role == "rowText" ? s.ListRowTextColor : s.Theme.BodyColor;
                if (Role == "buttonText")
                {
                    var b = ButtonRole == ButtonRole.Secondary ? s.Theme.Secondary : s.Theme.Primary;
                    text.color = b.LabelColor; if (b.LabelMaterial != null) text.fontSharedMaterial = b.LabelMaterial;
                }
                else { var m = Role == "title" ? s.Theme.TitleMaterial : s.Theme.BodyMaterial; if (m != null) text.fontSharedMaterial = m; }
            }
        }
    }
    public sealed class ShellUiFactory
    {
        public ShellStyle Style { get; private set; }
        public ShellUiFactory(ShellStyle style) { if (style == null) throw new InvalidOperationException("Missing style role: ShellStyle"); style.Validate(); Style = style; }
        public static RectTransform Node(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.SetActive(false);
            var r = (RectTransform)go.transform; r.SetParent(parent, false); return r;
        }
        public static void Stretch(RectTransform r)
        { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }
        void Bind(Component c, string role, ButtonRole button = ButtonRole.Primary)
        { var a = c.gameObject.AddComponent<ShellAppearance>(); a.Role = role; a.ButtonRole = button; a.Apply(Style); }
        Image Image(RectTransform r, string role)
        { var i = r.gameObject.AddComponent<Image>(); i.type = UnityEngine.UI.Image.Type.Sliced; Bind(i, role); return i; }
        public RectTransform ScreenRoot(Transform parent, string name = "Screen")
        { var r = Node(name, parent); Stretch(r); Image(r, "screen"); r.gameObject.SetActive(true); return r; }
        public TextMeshProUGUI Label(Transform parent, string value, bool title = false, string role = null)
        {
            var r = Node(title ? "Title" : "Label", parent); r.sizeDelta = title ? Style.Theme.TitleSize : Style.Theme.BodySize;
            var t = r.gameObject.AddComponent<TextMeshProUGUI>(); t.text = value; t.fontSize = title ? Style.Theme.TitleFontSize : Style.Theme.BodyFontSize;
            t.alignment = TextAlignmentOptions.Center; t.raycastTarget = false; Bind(t, role ?? (title ? "title" : "body"));
            r.gameObject.SetActive(true); return t;
        }
        public Button Button(Transform parent, string text, ButtonRole role = ButtonRole.Primary, Action clicked = null)
        {
            var r = Node("Button " + role, parent); var b = role == ButtonRole.Secondary ? Style.Theme.Secondary : Style.Theme.Primary;
            r.sizeDelta = b.DefaultSize; var image = r.gameObject.AddComponent<Image>(); image.type = UnityEngine.UI.Image.Type.Sliced; Bind(image, "button", role);
            var button = r.gameObject.AddComponent<Button>(); button.targetGraphic = image; button.transition = Selectable.Transition.None;
            button.interactable = role != ButtonRole.Locked; if (clicked != null) button.onClick.AddListener(() => clicked());
            var label = Label(r, text); var binding = label.GetComponent<ShellAppearance>(); binding.Role = "buttonText"; binding.ButtonRole = role; binding.Apply(Style);
            label.fontSize = b.FontSize; label.rectTransform.sizeDelta = b.DefaultSize - b.LabelPadding;
            GameKit.UI.FaceLabel.CentreOnFace(label.rectTransform, b.DefaultSize.y, b.FaceCentreFromTop);
            r.gameObject.SetActive(true); return button;
        }
        public RectTransform ListRow(Transform parent, string text)
        {
            var r = Node("List Row", parent); r.sizeDelta = Style.RowSize; Image(r, "row");
            var layout = r.gameObject.AddComponent<LayoutElement>(); layout.preferredHeight = Style.RowSize.y;
            Stretch(Label(r, text, role: "rowText").rectTransform); r.gameObject.SetActive(true); return r;
        }
        public ScrollRect ScrollList(Transform parent)
        {
            var r = Node("Scroll List", parent); Stretch(r);
            var viewport = Node("Viewport", r); Stretch(viewport); viewport.gameObject.AddComponent<RectMask2D>();
            var content = Node("Content", viewport); content.anchorMin = new Vector2(0,1); content.anchorMax = Vector2.one; content.pivot = new Vector2(.5f,1); content.sizeDelta = Vector2.zero;
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>(); layout.spacing = Style.Spacing; layout.childControlHeight = true; layout.childForceExpandHeight = false;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>(); fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = r.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false;
            content.gameObject.SetActive(true); viewport.gameObject.SetActive(true); r.gameObject.SetActive(true); return scroll;
        }
        public PopupView PopupFrame(Transform parent, string title, bool contentSlot = true)
        {
            var r = Node("Popup", parent); Stretch(r); Image(r, "scrim");
            var group = r.gameObject.AddComponent<CanvasGroup>(); group.alpha = 0; group.interactable = group.blocksRaycasts = false;
            var scrim = r.gameObject.AddComponent<Button>(); scrim.transition = Selectable.Transition.None;
            var panel = Node("Panel", r); panel.sizeDelta = Style.Theme.PanelSize; Image(panel, "panel");
            var panelInput = panel.gameObject.AddComponent<Button>(); panelInput.transition = Selectable.Transition.None;
            var heading = Label(panel, title, true); heading.rectTransform.anchoredPosition = new Vector2(0, Style.Theme.PanelSize.y/2 + Style.Theme.TitleOffsetY);
            RectTransform slot = null;
            if (contentSlot) { slot = Node("Content", panel); slot.sizeDelta = Style.Theme.BodySize; slot.gameObject.SetActive(true); }
            var view = r.gameObject.AddComponent<PopupView>(); view.Initialize(group, panel, slot, Style.Theme.Motion);
            scrim.onClick.AddListener(() => view.RequestDismiss()); panel.gameObject.SetActive(true);
            // Remains inactive until a host owns it.
            return view;
        }
        public void SwitchStyle(Transform root, ShellStyle style)
        {
            if (style == null) throw new InvalidOperationException("Missing style role: ShellStyle"); style.Validate(); Style = style;
            foreach (var binding in root.GetComponentsInChildren<ShellAppearance>(true)) binding.Apply(style);
            foreach (var view in root.GetComponentsInChildren<PopupView>(true)) view.Motion = style.Theme.Motion;
        }
    }
}
