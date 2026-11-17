using GameKit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Unity.UI
{
    public enum DismissalPolicy { Dismissible, Mandatory }
    public sealed class PopupView : MonoBehaviour
    {
        public CanvasGroup Group { get; private set; }
        public RectTransform Panel { get; private set; }
        public RectTransform Content { get; private set; }
        internal bool FitContent;
        public Button CloseButton { get; internal set; }
        public Selectable InitialFocus;
        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("Policy")] DismissalPolicy policy;
        public DismissalPolicy Policy { get=>policy; set { policy=value; RefreshCloseTarget(); } }
        ShellStyle style;
        public void Configure(ShellStyle value) { style=value; RefreshCloseTarget(); }
        void RefreshCloseTarget()
        {
            if(Panel==null) return;
            if(style!=null)
                Panel.GetComponent<Image>().sprite=policy==DismissalPolicy.Mandatory && style.MandatoryPanelSprite!=null
                    ? style.MandatoryPanelSprite : style.Theme.PanelSprite;
            var close=CloseButton!=null?CloseButton.transform:null; if(close==null) return;
            bool enabled=policy==DismissalPolicy.Dismissible && (style==null||style.ShowCloseHotspot);
            close.GetComponent<Button>().interactable=enabled;
            close.GetComponent<Image>().raycastTarget=enabled;
            // The style's sliced panel may already contain the close circle and X.
            // Keep one accessible, expanded hit target aligned with that artwork.
            bool baked=style!=null&&style.PanelHasBakedCloseIcon;
            close.GetComponent<Image>().color=baked?Color.clear:style!=null?style.ActionSurfaceColor:Color.white;
            foreach(var icon in close.GetComponentsInChildren<ShellAppearance>(true))
                if(icon.Role=="actionIcon") icon.GetComponent<Image>().enabled=!baked;
            close.gameObject.SetActive(enabled);
        }
        void OnEnable() { RefreshCloseTarget(); Reflow(); }
        int layoutSignature;
        void LateUpdate()
        {
            int signature=LayoutSignature();
            if(signature==layoutSignature) return;
            Reflow();
            layoutSignature=LayoutSignature();
        }
        int LayoutSignature()
        {
            if(Content==null||Panel==null) return 0;
            int hash=Mathf.RoundToInt(Panel.rect.width);
            foreach(Transform child in Content)
            {
                if(!child.gameObject.activeSelf) continue;
                var label=child.GetComponent<TMP_Text>();
                hash=hash*31+(label!=null?(label.text??"").GetHashCode():child.GetInstanceID());
            }
            return hash;
        }
        // Measure at the available width before asking Unity's layout system for height.
        // This also handles text changes and retry/error controls appearing after Open.
        public void Reflow()
        {
            if(style==null||Content==null||style.PopupLayout.PanelMaxHeight<=0&&!FitContent) return;
            const float padding=48, headingBand=132;
            float width=Mathf.Max(1,Panel.rect.width-2*padding);
            var layout=Content.GetComponent<VerticalLayoutGroup>();
            if(layout==null) layout=Content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childControlHeight=true; layout.childForceExpandHeight=false;
            layout.childControlWidth=false; layout.childForceExpandWidth=false;
            layout.childAlignment=TextAnchor.UpperCenter; layout.spacing=style.Spacing;
            foreach(Transform child in Content)
            {
                if(!child.gameObject.activeSelf) continue;
                var rect=(RectTransform)child;
                var element=child.GetComponent<LayoutElement>()??child.gameObject.AddComponent<LayoutElement>();
                var label=child.GetComponent<TMP_Text>();
                element.ignoreLayout=label!=null&&string.IsNullOrEmpty(label.text);
                if(element.ignoreLayout) { rect.sizeDelta=Vector2.zero; continue; }
                bool button=child.GetComponent<Button>()!=null;
                var category=child.GetComponent<LayoutCategory>();
                bool icon=category!=null&&(category.Category==ShellLayoutCategory.IconButton||category.Category==ShellLayoutCategory.IconAction||category.Category==ShellLayoutCategory.IconAmount);
                var buttonSize=icon?ShellUiFactory.IconSize(category.Category):ShellLayout.Standard.PopupAction;
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,button?Mathf.Min(buttonSize.x,width):width);
                if(label!=null) element.preferredHeight=Mathf.Max(32,label.GetPreferredValues(label.text,width,float.PositiveInfinity).y);
                else if(button) element.preferredHeight=buttonSize.y;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(Content);
            float preferred=LayoutUtility.GetPreferredHeight(Content);
            var sizing=style.PopupLayout;
            float min=sizing.PanelMinHeight>0?sizing.PanelMinHeight:FitContent?ShellLayout.Standard.SessionPopupMinHeight:style.Theme.PanelSize.y;
            float max=sizing.PanelMaxHeight>0?sizing.PanelMaxHeight:FitContent?ShellLayout.Standard.SessionPopupMaxHeight:style.Theme.PanelSize.y;
            Panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,Mathf.Clamp(headingBand+preferred+padding,min,Mathf.Max(min,max)));
            LayoutRebuilder.ForceRebuildLayoutImmediate(Content);
        }
        public UiMotion Motion { get; internal set; }
        internal PopupHost Host;
        internal GameObject PreviousFocus;
        internal bool Closing;
        internal float Progress;
        internal float StartAlpha;
        internal Vector3 StartScale;
        public void Initialize(CanvasGroup group, RectTransform panel, RectTransform content, UiMotion motion)
        { Group = group; Panel = panel; Content = content; Motion = motion; }
        public bool RequestDismiss() => Host != null && Host.RequestDismiss(this);
        void OnDisable() { if (Host != null) Host.Forget(this); }
    }
}
