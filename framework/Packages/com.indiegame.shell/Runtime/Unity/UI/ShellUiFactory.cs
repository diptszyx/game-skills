using System;
using System.Collections.Generic;
using IndieGame.Shell.Unity.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Unity.UI
{
    // Bindings only copy appearance; style switching never rebuilds state or hierarchy.
    public sealed class ShellAppearance : MonoBehaviour
    {
        public string Role;
        public string IconKey;
        public bool Selected;
        public ButtonRole ButtonRole;
        public void Apply(ShellStyle s)
        {
            var image = GetComponent<Image>(); var text = GetComponent<TMP_Text>();
            switch (Role)
            {
                case "tabBar": image.sprite = s.TabBarPlainBackground; image.color = Color.white; break;
                case "tabCell": image.sprite = Selected ? s.TabCellSelectedBackground : s.TabCellBackground; image.color = Color.white; break;
                case "currencyPill": image.sprite = s.PlainPillBackground; image.color = Color.white; break;
                case "livesPill": image.sprite = s.PlainPillBackground; image.color = Color.white; break;
                case "currencyIcon": image.sprite = s.CurrencyIcon; image.color = Color.white; break;
                case "heartIcon": image.sprite = s.HeartIcon; image.color = Color.white; break;
                case "gearButton": image.sprite = s.GearButton; image.color = Color.white; break;
                case "boardFrame": image.sprite = s.BoardFrame; image.color = Color.white; break;
                case "badge": image.sprite = s.BadgeSprite; image.color = Color.white; break;
                case "dayCollected": image.sprite = s.DayCellCollected; image.color = Color.white; break;
                case "dayToday": image.sprite = s.DayCellToday; image.color = Color.white; break;
                case "dayLocked": image.sprite = s.DayCellLocked; image.color = Color.white; break;
                case "actionIcon": image.sprite=s.Icon(IconKey); image.color=s.ActionIconColor; break;
                case "iconSurface":
                    var shape=IconKey=="_round"?s.RoundButtonSprite:s.PillButtonSprite;
                    image.sprite=shape!=null?shape:ShellIcons.Get(IconKey);
                    image.type=IconKey=="_round"?Image.Type.Simple:Image.Type.Sliced;
                    image.color=shape!=null?Color.white:s.ActionSurfaceColor; break;
                case "tabIcon": image.sprite = s.TabIcon(IconKey); image.color = Selected ? s.TabSelectedTint : s.TabUnselectedTint; break;
                case "screen": image.sprite = s.ScreenBackground; image.color = s.ScreenColor; break;
                case "row": image.sprite = s.ListRowSprite; image.color = s.ListRowColor; break;
                case "ownRow": image.sprite = s.ListRowSprite; image.color = s.LeaderboardOwnRowColor; break;
                case "progressBar": GetComponent<ProgressBarView>().Apply(s); break;
                case "scoreIcon": image.sprite = s.StyleIcon("scoreIcon"); image.color = Color.white; break;
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
                text.color = Role == "title" ? s.TitleColor : Role == "rowText" ? s.ListRowTextColor : Role == "popupText" && s.PanelTextColor.a > 0 ? s.PanelTextColor : Role == "backdropText" && s.BackdropTextColor.a > 0 ? s.BackdropTextColor : s.Theme.BodyColor;
                if (Role == "buttonText" || Role == "chromeText")
                {
                    var b = ButtonRole == ButtonRole.Secondary ? s.Theme.Secondary : s.Theme.Primary;
                    text.color = Role == "chromeText" ? Color.white : b.LabelColor; if (b.LabelMaterial != null) text.fontSharedMaterial = b.LabelMaterial;
                }
                else if (Role == "screenTitle") { text.color = s.ScreenTitleColorOverride.a > 0 ? s.ScreenTitleColorOverride : s.TitleColor; text.fontStyle = FontStyles.Bold; if (s.Theme.BodyMaterial != null) text.fontSharedMaterial = s.Theme.BodyMaterial; }
                else if (Role == "tabLabel") { text.color = s.TabLabelColor; if (s.Theme.BodyMaterial != null) text.fontSharedMaterial = s.Theme.BodyMaterial; }
                else { var m = Role == "title" ? s.Theme.TitleMaterial : s.Theme.BodyMaterial; if (m != null) text.fontSharedMaterial = m; }
            }
        }
    }
    public sealed class ShellUiFactory
    {
        public ShellStyle Style { get; private set; }
        public IReadOnlyDictionary<string,Sprite> BoosterIcons { get; set; } = new Dictionary<string,Sprite>();
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
        public Image AppearanceImage(Transform parent, string name, string role, bool sliced = false, string iconKey = null)
        {
            var r = Node(name, parent); var image = r.gameObject.AddComponent<Image>();
            image.type = sliced ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
            image.preserveAspect = !sliced; image.raycastTarget = false;
            var binding = r.gameObject.AddComponent<ShellAppearance>(); binding.Role = role; binding.IconKey = iconKey; binding.Apply(Style);
            r.gameObject.SetActive(true); return image;
        }
        // The one progress bar builder; callers place the returned Root and drive SetRatio.
        public ProgressBarView ProgressBar(Transform parent,string name,BarColorSource colors)
        {
            var r=Node(name,parent); r.gameObject.AddComponent<Image>();
            var bar=r.gameObject.AddComponent<ProgressBarView>(); bar.Build(colors);
            var binding=r.gameObject.AddComponent<ShellAppearance>(); binding.Role="progressBar"; binding.Apply(Style);
            r.gameObject.SetActive(true); return bar;
        }
        public RectTransform ScreenRoot(Transform parent, string name = "Screen")
        { var r = Node(name, parent); Stretch(r); Image(r, "screen"); r.gameObject.SetActive(true); return r; }
        string DefaultLabelRole(Transform parent, bool title)
        {
            bool onBackdrop = Style.BackdropTextColor.a > 0 && parent.GetComponentInParent<PopupView>(true) == null;
            return onBackdrop ? "backdropText" : title ? "title" : parent.GetComponentInParent<PopupView>(true)!=null ? "popupText" : "body";
        }
        public TextMeshProUGUI Label(Transform parent, string value, bool title = false, string role = null)
        {
            var r = Node(title ? "Title" : "Label", parent); r.sizeDelta = title ? Style.Theme.TitleSize : Style.Theme.BodySize;
            var t = r.gameObject.AddComponent<TextMeshProUGUI>(); t.text = value; t.fontSize = title ? Style.Theme.TitleFontSize : Style.Theme.BodyFontSize;
            t.alignment = TextAlignmentOptions.Center; t.raycastTarget = false; Bind(t, role ?? DefaultLabelRole(parent, title));
            LocalizedLayout.Text(t,t.fontSize);
            if(title) LocalizedLayout.Heading(r);
            if(parent.GetComponent<PopupView>()==null && parent.parent!=null && parent.parent.GetComponentInParent<PopupView>(true)!=null) LocalizedLayout.PopupChild(r,false);
            r.gameObject.SetActive(true); return t;
        }
        public Button WithPressFeedback(Button button)
        { var f = button.gameObject.AddComponent<PressFeedback>(); f.Configure(Style); return button; }
        public Button Button(Transform parent, string text, ButtonRole role = ButtonRole.Primary, Action clicked = null)
        {
            var r = Node("Button " + role, parent); var b = role == ButtonRole.Secondary ? Style.Theme.Secondary : Style.Theme.Primary;
            r.sizeDelta = ShellLayout.Standard.ButtonStandard;
            var parentCategory=parent.GetComponent<LayoutCategory>();
            LayoutCategory.Record(r,parent.GetComponentInParent<PopupView>(true)!=null ? ShellLayoutCategory.PopupAction
                : parentCategory!=null && parentCategory.Category==ShellLayoutCategory.Row ? ShellLayoutCategory.RowAction : ShellLayoutCategory.Standard); var image = r.gameObject.AddComponent<Image>(); image.type = UnityEngine.UI.Image.Type.Sliced; Bind(image, "button", role);
            var button = r.gameObject.AddComponent<Button>(); button.targetGraphic = image; button.transition = Selectable.Transition.None; WithPressFeedback(button);
            button.interactable = role != ButtonRole.Locked; if (clicked != null) button.onClick.AddListener(() => clicked());
            var label = Label(r, text); var binding = label.GetComponent<ShellAppearance>(); binding.Role = "buttonText"; binding.ButtonRole = role; binding.Apply(Style);
            label.fontSize = b.FontSize; LocalizedLayout.Text(label,b.FontSize);
            LocalizedLayout.ButtonLabel(label.rectTransform);
            if(parent.GetComponentInParent<PopupView>(true)!=null) LocalizedLayout.PopupChild(r,true);
            r.gameObject.SetActive(true); return button;
        }
        public Button IconButton(Transform parent,string key,string label,Action clicked=null,ShellLayoutCategory category=ShellLayoutCategory.IconButton,string amount=null,string secondIcon=null,string accessibilityKey=null)
        {
            var r=Node(label,parent); var size=IconSize(category); r.sizeDelta=size; LayoutCategory.Record(r,category);
            var image=r.gameObject.AddComponent<Image>();
            if(category==ShellLayoutCategory.IconButton) {
                float padding=(ShellLayout.Standard.IconHitSize-size.x)/2;
                image.raycastPadding=new Vector4(-padding,-padding,-padding,-padding);
            }
            var appearance=r.gameObject.AddComponent<ShellAppearance>(); appearance.Role="iconSurface"; appearance.IconKey=category==ShellLayoutCategory.IconButton?"_round":"_pill"; appearance.Apply(Style);
            var b=r.gameObject.AddComponent<Button>(); b.targetGraphic=image; b.transition=Selectable.Transition.None; WithPressFeedback(b);
            if(clicked!=null) b.onClick.AddListener(()=>clicked());
            bool hasIcon=!string.IsNullOrEmpty(key);
            r.gameObject.AddComponent<ActionAccessibility>().Set(accessibilityKey??(hasIcon?key:"text"),label);
            if(hasIcon) {
                var icon=AppearanceImage(r,"Icon "+key,"actionIcon",iconKey:key); icon.rectTransform.sizeDelta=GlyphSize(size);
                if(amount!=null||secondIcon!=null) icon.rectTransform.anchoredPosition=new Vector2(-30,0);
            }
            if(secondIcon!=null) { var second=AppearanceImage(r,"Icon "+secondIcon,"actionIcon",iconKey:secondIcon); second.rectTransform.sizeDelta=GlyphSize(size); second.rectTransform.anchoredPosition=new Vector2(30,0); }
            if(amount!=null) {
                var text=Label(r,amount,role:"chromeText"); text.rectTransform.anchorMin=text.rectTransform.anchorMax=new Vector2(.5f,.5f); text.rectTransform.sizeDelta=hasIcon?new Vector2(76,48):new Vector2(size.x-24,48); text.rectTransform.anchoredPosition=hasIcon?new Vector2(28,0):Vector2.zero;
                if(!hasIcon) text.alignment=TextAlignmentOptions.Center;
                text.fontSize=text.fontSizeMax=30; text.color=Style.ActionIconColor;
            }
            r.gameObject.SetActive(true); return b;
        }
        public static Vector2 GlyphSize(Vector2 button)
        { return Vector2.one*Mathf.Round(Mathf.Min(button.x,button.y)*ShellLayout.Standard.IconGlyphFraction); }
        public static Vector2 IconSize(ShellLayoutCategory category)
        { return category==ShellLayoutCategory.IconButton?ShellLayout.Standard.IconButton:category==ShellLayoutCategory.IconAmount?ShellLayout.Standard.IconAmount:ShellLayout.Standard.IconAction; }
        public void SetIcon(Button button,string key,string label)
        {
            var binding=Array.Find(button.GetComponentsInChildren<ShellAppearance>(true),a=>a.Role=="actionIcon");
            binding.IconKey=key; binding.Apply(Style);
            var accessible=button.GetComponent<ActionAccessibility>(); accessible.Set(accessible.Key,label);
        }
        public void AddPlayIcon(Button button)
        {
            var icon=AppearanceImage(button.transform,"Icon play","actionIcon",iconKey:"play");
            icon.rectTransform.anchorMin=icon.rectTransform.anchorMax=new Vector2(.18f,.5f); icon.rectTransform.sizeDelta=new Vector2(44,44);
            var label=button.GetComponentInChildren<TMP_Text>(); label.rectTransform.offsetMin=new Vector2(64,6);
        }
        // Icon + amount pieces in one line, sized to its content; callers place it.
        public RectTransform RewardStrip(Transform parent,IReadOnlyList<RewardPart> parts,string role,float fontSize=0,string lead=null)
        {
            var strip=Node("Reward strip",parent);
            var layout=strip.gameObject.AddComponent<HorizontalLayoutGroup>(); layout.spacing=Style.RewardGap; layout.childAlignment=TextAnchor.MiddleCenter;
            layout.childControlWidth=layout.childControlHeight=true; layout.childForceExpandWidth=layout.childForceExpandHeight=false;
            var fitter=strip.gameObject.AddComponent<ContentSizeFitter>(); fitter.horizontalFit=fitter.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            if(!string.IsNullOrEmpty(lead)) StripLabel(strip,lead,role,fontSize);
            foreach(var part in parts)
            {
                if(part.BoosterId!=null && BoosterIcons.TryGetValue(part.BoosterId,out var sprite) && sprite!=null)
                {
                    BoosterIconImage(strip,sprite); StripLabel(strip,part.AmountText,role,fontSize); continue;
                }
                if(part.Icon!=null)
                {
                    var icon=AppearanceImage(strip,part.Icon,part.Icon); var size=icon.gameObject.AddComponent<LayoutElement>();
                    size.preferredWidth=size.preferredHeight=Style.RewardIconSize;
                }
                StripLabel(strip,part.Text,role,fontSize);
            }
            strip.gameObject.SetActive(true); return strip;
        }
        void BoosterIconImage(RectTransform strip,Sprite sprite)
        {
            var node=Node("Booster icon",strip); var image=node.gameObject.AddComponent<Image>();
            image.sprite=sprite; image.preserveAspect=true; image.raycastTarget=false;
            var size=node.gameObject.AddComponent<LayoutElement>(); size.preferredWidth=size.preferredHeight=Style.RewardIconSize;
            node.gameObject.SetActive(true);
        }
        void StripLabel(RectTransform strip,string value,string role,float fontSize)
        {
            var label=Label(strip,value,role:role); label.enableAutoSizing=false; label.textWrappingMode=TextWrappingModes.NoWrap;
            if(fontSize>0) label.fontSize=fontSize;
        }
        public RectTransform ListRow(Transform parent, string text)
        {
            var r = Node("List Row", parent); r.sizeDelta = ShellLayout.Standard.Row; LayoutCategory.Record(r,ShellLayoutCategory.Row); Image(r, "row");
            var layout = r.gameObject.AddComponent<LayoutElement>(); layout.preferredHeight = ShellLayout.Standard.Row.y;
            LocalizedLayout.Row(r,Label(r, text, role: "rowText"),false); r.gameObject.SetActive(true); return r;
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
        public PopupView PopupFrame(Transform parent, string title, bool contentSlot = true,string closeLabel="Close")
        {
            var r = Node("Popup", parent); Stretch(r); Image(r, "scrim");
            var group = r.gameObject.AddComponent<CanvasGroup>(); group.alpha = 0; group.interactable = group.blocksRaycasts = false;
            var scrim = r.gameObject.AddComponent<Button>(); scrim.transition = Selectable.Transition.None; LayoutCategory.Record(scrim,ShellLayoutCategory.PopupScrim);
            var panel = Node("Panel", r); panel.sizeDelta = Style.Theme.PanelSize; Image(panel, "panel");
            var panelInput = panel.gameObject.AddComponent<Button>(); panelInput.transition = Selectable.Transition.None; LayoutCategory.Record(panelInput,ShellLayoutCategory.PopupPanel);
            var closeButton=IconButton(panel,"close",closeLabel,category:ShellLayoutCategory.IconButton);
            var closeHit=(RectTransform)closeButton.transform; 
            closeHit.anchorMin=closeHit.anchorMax=new Vector2(1,1); closeHit.anchoredPosition=ShellLayout.Standard.CloseHitPosition;
            var heading = Label(panel, title, true, "title");
            RectTransform slot = null;
            if (contentSlot) { slot = Node("Content", panel); slot.sizeDelta = Style.Theme.BodySize; slot.gameObject.SetActive(true); }
            var view = r.gameObject.AddComponent<PopupView>(); view.Initialize(group, panel, slot, Style.Theme.Motion); view.CloseButton=closeButton; view.Configure(Style);
            scrim.onClick.AddListener(() => view.RequestDismiss()); closeButton.onClick.AddListener(() => view.RequestDismiss()); panel.gameObject.SetActive(true);
            // Remains inactive until a host owns it.
            return view;
        }
        public void SwitchStyle(Transform root, ShellStyle style)
        {
            if (style == null) throw new InvalidOperationException("Missing style role: ShellStyle"); style.Validate(); Style = style;
            foreach (var binding in root.GetComponentsInChildren<ShellAppearance>(true)) binding.Apply(style);
            foreach (var press in root.GetComponentsInChildren<PressFeedback>(true)) press.Configure(style);
            foreach (var view in root.GetComponentsInChildren<PopupView>(true)) { view.Motion = style.Theme.Motion; view.Configure(style); }
        }
    }
}
