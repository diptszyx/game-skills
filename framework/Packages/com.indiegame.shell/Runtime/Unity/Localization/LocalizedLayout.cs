using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IndieGame.Shell.Unity.UI;
namespace IndieGame.Shell.Unity.Localization
{
    // Call after building/rendering a localized view (and again after replacing rows).
    // Explicit composition helper: does not silently change unrelated application UI.
    public static class LocalizedLayout
    {
        public static void Fit(Transform root)
        {
            foreach(var popup in root.GetComponentsInChildren<PopupView>(true))
            {
                var panel=popup.Panel;
                var heading=panel.Find("Title") as RectTransform;
                if(heading!=null) { heading.anchorMin=heading.anchorMax=new Vector2(.5f,1); heading.anchoredPosition=new Vector2(0,-64); heading.sizeDelta=new Vector2(panel.sizeDelta.x-48,104); }
                if(popup.Content!=null)
                {
                    var content=popup.Content; content.anchorMin=content.anchorMax=new Vector2(.5f,1); content.pivot=new Vector2(.5f,1);
                    content.anchoredPosition=new Vector2(0,-132); content.sizeDelta=new Vector2(panel.sizeDelta.x-48,panel.sizeDelta.y-152);
                    foreach(Transform child in content)
                    {
                        var rect=child as RectTransform; if(rect==null) continue;
                        rect.sizeDelta=new Vector2(rect.sizeDelta.x,child.GetComponent<Button>()!=null?80:64);
                    }
                }
            }
            foreach(var label in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if(!label.enableAutoSizing) { label.fontSizeMax=label.fontSize; label.fontSizeMin=Mathf.Min(24,label.fontSize); }
                label.enableAutoSizing=true; label.textWrappingMode=TextWrappingModes.Normal; label.overflowMode=TextOverflowModes.Ellipsis;
                var rect=label.rectTransform;
                if(label.name=="Title" && label.GetComponentInParent<PopupView>()==null)
                {
                    rect.anchorMin=new Vector2(0,1); rect.anchorMax=Vector2.one; rect.sizeDelta=new Vector2(-48,128); rect.anchoredPosition=new Vector2(0,-76);
                    var body=rect.parent.Find("Body") as RectTransform; if(body!=null) body.offsetMax=new Vector2(body.offsetMax.x,-156);
                }
                if(label.transform.parent.GetComponent<Button>()!=null && label.transform.parent.GetComponent<ShellAppearance>()?.Role=="button")
                { rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one; rect.offsetMin=new Vector2(12,6); rect.offsetMax=new Vector2(-12,-6); }
                if(label.transform.parent.name=="List Row")
                {
                    var row=(RectTransform)label.transform.parent; var element=row.GetComponent<LayoutElement>();
                    if(row.GetComponentInChildren<Button>(true)==null) rect.anchorMax=Vector2.one;
                    if(element!=null) element.preferredHeight=120;
                    rect.offsetMin=new Vector2(12,8); rect.offsetMax=new Vector2(-8,-8);
                }
                if(label.transform.parent.name=="Loading")
                {
                    if(label.name!="Title") { rect.anchorMin=new Vector2(0,.5f); rect.anchorMax=new Vector2(1,.5f); rect.sizeDelta=new Vector2(-48,120); rect.anchoredPosition=new Vector2(0,-90); }
                }
            }
            Canvas.ForceUpdateCanvases();
        }
    }
}
