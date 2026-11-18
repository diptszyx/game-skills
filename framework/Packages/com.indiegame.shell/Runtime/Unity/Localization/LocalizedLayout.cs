using TMPro;
using IndieGame.Shell.Unity.UI;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Unity.Localization
{
    // Construction rules shared by the shell factory and views. TMP recalculates its
    // automatic sizing whenever text, font or available rect changes; no tree scan
    // or application-level fitting pass is needed.
    internal static class LocalizedLayout
    {
        internal static void Text(TMP_Text label, float size)
        {
            label.fontSizeMax=size; label.fontSizeMin=Mathf.Min(24,size);
            label.enableAutoSizing=true; label.textWrappingMode=TextWrappingModes.Normal;
            label.overflowMode=TextOverflowModes.Overflow;
        }
        internal static void Heading(RectTransform rect)
        {
            rect.anchorMin=new Vector2(0,1); rect.anchorMax=Vector2.one;
            rect.sizeDelta=new Vector2(-48,128); rect.anchoredPosition=new Vector2(0,-76);
        }
        internal static void Body(RectTransform rect, float spacing)
        { rect.offsetMin=new Vector2(spacing,spacing); rect.offsetMax=new Vector2(-spacing,-156); }
        internal static void ButtonLabel(RectTransform rect)
        {
            rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one;
            rect.offsetMin=new Vector2(12,6); rect.offsetMax=new Vector2(-12,-6);
        }
        internal static void Row(RectTransform row, TMP_Text label, bool action)
        {
            row.GetComponent<LayoutElement>().preferredHeight=ShellLayout.Standard.Row.y;
            var rect=label.rectTransform; rect.anchorMin=Vector2.zero;
            rect.anchorMax=new Vector2(action?.64f:1,1);
            rect.offsetMin=new Vector2(12,8); rect.offsetMax=new Vector2(-8,-8);
        }
        internal static float Popup(RectTransform panel, RectTransform heading, RectTransform content)
        {
            var image=panel.GetComponent<Image>();
            var sprite=image!=null?image.sprite:null;
            float band=0;
            if(sprite!=null && sprite.border.w>0 && sprite.border.w<sprite.rect.height)
            {
                float units=image.pixelsPerUnit*image.pixelsPerUnitMultiplier;
                float vertical=(sprite.border.y+sprite.border.w)/units;
                band=sprite.border.w/units*Mathf.Min(1,panel.rect.height/vertical);
            }
            var title=heading.GetComponent<TMP_Text>();
            bool derived=title!=null && band>=MinTitleHeight(title);
            if(!derived) band=PopupHeadingBand;
            float contentTop=band+28;
            heading.anchorMin=heading.anchorMax=new Vector2(.5f,1);
            heading.pivot=new Vector2(.5f,1); heading.anchoredPosition=Vector2.zero; heading.sizeDelta=new Vector2(panel.rect.width-48,band);
            if(title!=null) { title.alignment=derived?TextAlignmentOptions.Center:TextAlignmentOptions.Top; title.margin=new Vector4(0,derived?0:8,0,0); }
            if(content==null) return contentTop;
            content.anchorMin=Vector2.zero; content.anchorMax=Vector2.one; content.pivot=new Vector2(.5f,1);
            content.offsetMin=new Vector2(48,48); content.offsetMax=new Vector2(-48,-contentTop);
            return contentTop;
        }
        static float MinTitleHeight(TMP_Text title)
        {
            var face=title.font.faceInfo;
            float lineRatio=face.pointSize>0?face.lineHeight/face.pointSize:1.2f;
            return (title.enableAutoSizing?title.fontSizeMin:title.fontSize)*lineRatio;
        }
        internal static void PopupChild(RectTransform rect, bool button)
        { rect.sizeDelta=button?ShellLayout.Standard.PopupAction:new Vector2(rect.sizeDelta.x,64); }
        const float PopupHeadingBand=104f;
        internal static void LoadingTip(RectTransform rect)
        {
            rect.anchorMin=new Vector2(0,.5f); rect.anchorMax=new Vector2(1,.5f);
            rect.sizeDelta=new Vector2(-48,120); rect.anchoredPosition=new Vector2(0,-90);
        }
    }
}
