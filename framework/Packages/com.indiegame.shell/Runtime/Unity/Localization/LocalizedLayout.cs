using TMPro;
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
            row.GetComponent<LayoutElement>().preferredHeight=120;
            var rect=label.rectTransform; rect.anchorMin=Vector2.zero;
            rect.anchorMax=new Vector2(action?.64f:1,1);
            rect.offsetMin=new Vector2(12,8); rect.offsetMax=new Vector2(-8,-8);
        }
        internal static void Popup(RectTransform panel, RectTransform heading, RectTransform content)
        {
            heading.anchorMin=heading.anchorMax=new Vector2(.5f,1);
            heading.anchoredPosition=new Vector2(0,-64); heading.sizeDelta=new Vector2(panel.sizeDelta.x-48,104);
            if(content==null) return;
            content.anchorMin=Vector2.zero; content.anchorMax=Vector2.one; content.pivot=new Vector2(.5f,1);
            content.offsetMin=new Vector2(24,20); content.offsetMax=new Vector2(-24,-132);
        }
        internal static void PopupChild(RectTransform rect, bool button)
        { rect.sizeDelta=new Vector2(rect.sizeDelta.x,button?80:64); }
        internal static void LoadingTip(RectTransform rect)
        {
            rect.anchorMin=new Vector2(0,.5f); rect.anchorMax=new Vector2(1,.5f);
            rect.sizeDelta=new Vector2(-48,120); rect.anchoredPosition=new Vector2(0,-90);
        }
    }
}
