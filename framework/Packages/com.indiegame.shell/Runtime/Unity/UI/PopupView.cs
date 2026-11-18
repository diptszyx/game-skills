using System.Collections.Generic;
using GameKit.UI;
using IndieGame.Shell.Unity.Localization;
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
        internal bool ManagedContent;
        const float ManagedSideInset=24, ManagedBottomInset=24;
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
            var heading=Panel.Find("Title") as RectTransform;
            var close=CloseButton!=null?CloseButton.transform:null;
            bool enabled=close!=null && policy==DismissalPolicy.Dismissible && (style==null||style.ShowCloseHotspot);
            if(heading!=null) LayoutHeading(heading,enabled);
            if(close==null) return;
            close.GetComponent<Button>().interactable=enabled;
            close.GetComponent<Image>().raycastTarget=enabled;
            // The style's sliced panel may already contain the close circle and X.
            // Keep one accessible, expanded hit target aligned with that artwork.
            bool baked=style!=null&&style.PanelHasBakedCloseIcon;
            PlaceClose((RectTransform)close,baked);
            var surface=close.GetComponent<ShellAppearance>();
            if(surface!=null && style!=null) surface.Apply(style);
            if(baked) close.GetComponent<Image>().color=Color.clear;
            foreach(var icon in close.GetComponentsInChildren<ShellAppearance>(true))
                if(icon.Role=="actionIcon") icon.GetComponent<Image>().enabled=!baked;
            close.gameObject.SetActive(enabled);
        }
        bool DrawsOwnClose=>style!=null&&!style.PanelHasBakedCloseIcon&&style.CloseVisibleSize>0;
        float LayoutHeading(RectTransform heading,bool closeVisible)
        {
            float band;
            if(style==null) band=LocalizedLayout.Popup(Panel,heading,Content);
            else
            {
                float clear=closeVisible&&DrawsOwnClose?style.CloseVisibleSize+ShellLayout.Standard.TitleInset*2:0;
                band=LocalizedLayout.Popup(Panel,heading,Content,style.PopupHeaderRows(policy==DismissalPolicy.Mandatory),clear);
            }
            if(ManagedContent&&Content!=null)
            { Content.offsetMin=new Vector2(ManagedSideInset,ManagedBottomInset); Content.offsetMax=new Vector2(-ManagedSideInset,Content.offsetMax.y); }
            return band;
        }
        void PlaceClose(RectTransform close,bool baked)
        {
            bool drawn=!baked&&DrawsOwnClose;
            var size=drawn?Vector2.one*style.CloseVisibleSize:ShellLayout.Standard.IconButton;
            close.sizeDelta=size;
            close.anchoredPosition=drawn?ShellLayout.Standard.CloseDrawnPosition(size.x):ShellLayout.Standard.CloseHitPosition;
            float padding=Mathf.Max(0,(ShellLayout.Standard.IconHitSize-size.x)/2);
            close.GetComponent<Image>().raycastPadding=new Vector4(-padding,-padding,-padding,-padding);
            foreach(var icon in close.GetComponentsInChildren<ShellAppearance>(true))
                if(icon.Role=="actionIcon") ((RectTransform)icon.transform).sizeDelta=ShellUiFactory.GlyphSize(size);
        }
        internal const float ModalFallbackAreaHeight=1440;
        float modalContentHeight=-1, modalArea;
        // Sizes the panel to header + content + bottom inset, clamped by style.Modal; the caller supplies content height in UI units.
        public float FitModal(float contentHeight)
        {
            modalContentHeight=contentHeight; return ApplyModalFit();
        }
        float ModalAreaHeight()
        {
            var area=Panel.parent as RectTransform; float height=area!=null?area.rect.height:0;
            return height>1?height:ModalFallbackAreaHeight;
        }
        float ApplyModalFit()
        {
            if(style==null||Panel==null) return 0;
            var heading=(RectTransform)Panel.Find("Title"); bool closeVisible=CloseButton!=null&&CloseButton.gameObject.activeSelf;
            modalArea=ModalAreaHeight();
            float band=LayoutHeading(heading,closeVisible);
            float min=style.Modal.MinHeightFraction*modalArea, max=style.Modal.MaxHeightFraction*modalArea;
            float height=Mathf.Clamp(band+modalContentHeight+ManagedBottomInset,min,Mathf.Max(min,max));
            Panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,height);
            LayoutHeading(heading,closeVisible);
            return height;
        }
        void OnEnable() { RefreshCloseTarget(); Reflow(); }
        int layoutSignature;
        RectTransform actionRow;
        void LateUpdate()
        {
            if(ManagedContent)
            { if(modalContentHeight>=0 && !Mathf.Approximately(ModalAreaHeight(),modalArea)) ApplyModalFit(); return; }
            int signature=LayoutSignature();
            if(signature==layoutSignature) return;
            Reflow();
            layoutSignature=LayoutSignature();
        }
        int LayoutSignature()
        {
            if(Content==null||Panel==null) return 0;
            int hash=Mathf.RoundToInt(Panel.rect.width);
            foreach(var child in Content.GetComponentsInChildren<RectTransform>(true))
            {
                hash=hash*31+(child.gameObject.activeSelf?1:0);
                var label=child.GetComponent<TMP_Text>();
                hash=hash*31+(label!=null?(label.text??"").GetHashCode():child.GetInstanceID());
            }
            return hash;
        }
        // Measure at the available width before asking Unity's layout system for height.
        // This also handles text changes and retry/error controls appearing after Open.
        public void Reflow()
        {
            if(style==null||Content==null) return;
            const float padding=48;
            float headingBand=LayoutHeading((RectTransform)Panel.Find("Title"),CloseButton!=null&&CloseButton.gameObject.activeSelf);
            if(ManagedContent)
            { if(modalContentHeight>=0) ApplyModalFit(); return; }
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
            var actions=new List<RectTransform>();
            foreach(Transform child in Content)
            {
                if(child==actionRow) {
                    foreach(Transform action in actionRow) if(action.gameObject.activeSelf) actions.Add((RectTransform)action);
                }
                else if(child.gameObject.activeSelf && child.GetComponent<Button>()!=null && IsIconAction(child)) actions.Add((RectTransform)child);
            }
            if(actions.Count>0 && actions.Count<=3)
            {
                int index=actionRow!=null?actionRow.GetSiblingIndex():actions[0].GetSiblingIndex();
                if(actionRow==null) {
                    actionRow=ShellUiFactory.Node("Actions",Content);
                    var row=actionRow.gameObject.AddComponent<HorizontalLayoutGroup>();
                    row.childAlignment=TextAnchor.MiddleCenter; row.childControlWidth=false; row.childControlHeight=false;
                    row.childForceExpandWidth=false; row.childForceExpandHeight=false;
                    row.spacing=ShellLayout.Standard.PopupActionSpacing;
                    actionRow.gameObject.AddComponent<LayoutElement>();
                }
                float desiredWidth=0,height=0;
                foreach(var action in actions) {
                    var category=action.GetComponent<LayoutCategory>();
                    var size=category!=null && category.Category!=ShellLayoutCategory.PopupAction?ShellUiFactory.IconSize(category.Category):ShellLayout.Standard.PopupAction;
                    desiredWidth=Mathf.Max(desiredWidth,size.x); height=Mathf.Max(height,size.y);
                }
                float actionWidth=Mathf.Min(desiredWidth,(width-(actions.Count-1)*ShellLayout.Standard.PopupActionSpacing)/actions.Count);
                foreach(var action in actions) {
                    action.SetParent(actionRow,false);
                    var element=action.GetComponent<LayoutElement>()??action.gameObject.AddComponent<LayoutElement>();
                    element.preferredWidth=actionWidth; element.preferredHeight=height;
                    action.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,actionWidth);
                    action.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,height);
                }
                actionRow.SetSiblingIndex(index); actionRow.gameObject.SetActive(true);
                actionRow.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,width);
                actionRow.GetComponent<LayoutElement>().preferredHeight=height;
            }
            else if(actionRow!=null)
            {
                int index=actionRow.GetSiblingIndex();
                foreach(var action in actions) { action.SetParent(Content,false); action.SetSiblingIndex(index++); }
                actionRow.gameObject.SetActive(false);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(Content);
            float preferred=LayoutUtility.GetPreferredHeight(Content);
            var sizing=style.PopupLayout;
            float min=sizing.PanelMinHeight>0?sizing.PanelMinHeight:FitContent?ShellLayout.Standard.SessionPopupMinHeight:style.Theme.PanelSize.y;
            float max=sizing.PanelMaxHeight>0?sizing.PanelMaxHeight:FitContent?ShellLayout.Standard.SessionPopupMaxHeight:style.Theme.PanelSize.y;
            if(style.PopupLayout.PanelMaxHeight>0||FitContent)
                Panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,Mathf.Clamp(headingBand+preferred+padding,min,Mathf.Max(min,max)));
            LayoutHeading((RectTransform)Panel.Find("Title"),CloseButton!=null&&CloseButton.gameObject.activeSelf);
            LayoutRebuilder.ForceRebuildLayoutImmediate(Content);
        }
        static bool IsIconAction(Transform button)
        {
            var category=button.GetComponent<LayoutCategory>();
            return category!=null && (category.Category==ShellLayoutCategory.IconButton||category.Category==ShellLayoutCategory.IconAction||category.Category==ShellLayoutCategory.IconAmount);
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
