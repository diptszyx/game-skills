using System;
using IndieGame.Shell.Unity.Localization;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IndieGame.Shell.Unity.UI;

namespace IndieGame.Shell.Unity.Screens
{
    public sealed class RowDisplay
    {
        public string Id { get; }
        public string Text { get; }
        public string Action { get; }
        public bool Enabled { get; }
        public float Progress { get; }
        public string Icon { get; }
        public string Amount { get; }
        public string Detail { get; }
        public RowDisplay(string id, string text, string action = "", bool enabled = false, float progress = -1,string icon="check",string amount=null,string detail=null)
        { Id=id; Text=text; Action=action; Enabled=enabled; Progress=progress; Icon=icon; Amount=amount; Detail=detail; }
    }
    public sealed class ListDisplay
    {
        public string Title { get; }
        public string Empty { get; }
        public IReadOnlyList<RowDisplay> Rows { get; }
        public ListDisplay(string title, IReadOnlyList<RowDisplay> rows, string empty = "") { Title=title; Rows=rows; Empty=empty; }
    }
    // Unity lifetime only; presenters and views remain ordinary objects.
    public sealed class ScreenLifetime : MonoBehaviour
    {
        public event Action Entered, Resumed, Ticked;
        void OnEnable() => Entered?.Invoke();
        void OnApplicationPause(bool paused) { if (!paused) Resumed?.Invoke(); }
        void Update() => Ticked?.Invoke();
    }
    internal static class ScreenWidgets
    {
        internal const float RowTitleFont=32, RowTitleMinFont=26, RowDetailFont=26, MinRowOpacity=.92f;
        internal const float EmptyCardBottom=.17f, EmptyCardTop=.29f;
        const float TopBarHeight=110, TitledBodyTop=266, UntitledBodyGap=12;
        internal const float TabBarHeight=160;
        internal static bool HidesTitle(ShellStyle style) => style.EnableHomeChrome && !style.ShowScreenTitleWithChrome;
        internal static RectTransform Body(ShellUiFactory f, RectTransform root)
        {
            var body=ShellUiFactory.Node("Body",root); ShellUiFactory.Stretch(body);
            LocalizedLayout.Body(body,f.Style.Spacing);
            if(f.Style.EnableHomeChrome)
            {
                body.offsetMin+=new Vector2(0,TabBarHeight);
                body.offsetMax=new Vector2(body.offsetMax.x,-(HidesTitle(f.Style)?TopBarHeight+UntitledBodyGap:TitledBodyTop));
            }
            body.gameObject.SetActive(true); return body;
        }
        internal static TMP_Text Heading(ShellUiFactory f, RectTransform root)
        {
            var label=f.Label(root,"",true,"screenTitle");
            if(f.Style.EnableHomeChrome) label.rectTransform.anchoredPosition+=new Vector2(0,-TopBarHeight);
            if(HidesTitle(f.Style)) label.gameObject.SetActive(false);
            return label;
        }
        static void StyleRowLabel(TMP_Text label,float max,float min,float yMin,float yMax,float xMax)
        {
            label.alignment=TextAlignmentOptions.Left; label.enableAutoSizing=true; label.fontSizeMax=max; label.fontSizeMin=min; label.fontSize=max;
            label.textWrappingMode=TextWrappingModes.NoWrap; label.overflowMode=TextOverflowModes.Ellipsis;
            var rect=label.rectTransform; rect.anchorMin=new Vector2(0,yMin); rect.anchorMax=new Vector2(xMax,yMax);
            rect.offsetMin=new Vector2(16,0); rect.offsetMax=new Vector2(-8,0);
        }
        static void LayoutRowText(RectTransform row,TMP_Text title,ShellUiFactory f,RowDisplay data)
        {
            float xMax=string.IsNullOrEmpty(data.Action)?1:.64f;
            if(data.Detail==null)
            {
                LocalizedLayout.Row(row,title,!string.IsNullOrEmpty(data.Action)); title.alignment=TextAlignmentOptions.Left;
                title.fontSizeMin=RowTitleMinFont; title.fontSizeMax=Mathf.Max(title.fontSizeMax,RowTitleMinFont); return;
            }
            StyleRowLabel(title,RowTitleFont,RowTitleMinFont,.5f,.96f,xMax);
            var detail=f.Label(row,data.Detail,role:"rowText"); detail.gameObject.name="Detail";
            StyleRowLabel(detail,RowDetailFont,RowDetailFont,.1f,.52f,xMax);
        }
        internal static RectTransform EmptyCard(ShellUiFactory f, RectTransform root)
        {
            var card=ShellUiFactory.Node("Empty card",root); card.anchorMin=new Vector2(.04f,EmptyCardBottom); card.anchorMax=new Vector2(.96f,EmptyCardTop);
            card.offsetMin=card.offsetMax=Vector2.zero;
            var image=f.AppearanceImage(card,"Surface","row",true); ShellUiFactory.Stretch(image.rectTransform); OpaqueCard(image);
            var label=f.Label(card,"",role:"rowText"); ShellUiFactory.Stretch(label.rectTransform);
            label.rectTransform.offsetMin=new Vector2(24,12); label.rectTransform.offsetMax=new Vector2(-24,-12);
            label.enableAutoSizing=true; label.fontSizeMax=34; label.fontSizeMin=RowTitleMinFont; label.fontSize=34;
            card.gameObject.SetActive(false); return card;
        }
        const float ProgressInset=14,ProgressHeight=8;
        static void OpaqueCard(Image card)
        { var c=card.color; c.a=Mathf.Max(c.a,MinRowOpacity); card.color=c; }
        internal static void Rows(ShellUiFactory f, RectTransform content, IReadOnlyList<RowDisplay> rows, Action<string> clicked)
        {
            for(int i=content.childCount-1;i>=0;i--) { var child=content.GetChild(i); child.gameObject.SetActive(false); if(Application.isPlaying) UnityEngine.Object.Destroy(child.gameObject); else UnityEngine.Object.DestroyImmediate(child.gameObject); }
            foreach(var row in rows)
            {
                var r=f.ListRow(content,""); var text=r.GetComponentInChildren<TMP_Text>(); text.text=row.Text;
                OpaqueCard(r.GetComponent<Image>()); LayoutRowText(r,text,f,row);
                if(row.Progress>=0)
                {
                    var track=ShellUiFactory.Node("Progress",r); track.anchorMin=Vector2.zero; track.anchorMax=new Vector2(.64f,0); track.pivot=Vector2.zero; track.offsetMin=new Vector2(ProgressInset,ProgressInset); track.offsetMax=new Vector2(0,ProgressInset+ProgressHeight);
                    track.gameObject.AddComponent<Image>().color=f.Style.ProgressTrackColor;
                    var fill=ShellUiFactory.Node("Fill",track); fill.anchorMin=Vector2.zero; fill.anchorMax=new Vector2(Mathf.Clamp01(row.Progress),1); fill.offsetMin=fill.offsetMax=Vector2.zero;
                    fill.gameObject.AddComponent<Image>().color=f.Style.ProgressFillColor; fill.gameObject.SetActive(true); track.gameObject.SetActive(true);
                }
                if(!string.IsNullOrEmpty(row.Action))
                { var b=f.IconButton(r,row.Icon,row.Action,()=>clicked(row.Id),row.Amount!=null?ShellLayoutCategory.IconAmount:ShellLayoutCategory.IconAction,row.Amount); b.interactable=row.Enabled;
                  var rt=(RectTransform)b.transform; rt.anchorMin=rt.anchorMax=new Vector2(1,.5f); rt.pivot=new Vector2(1,.5f); rt.anchoredPosition=new Vector2(-12,0);
 }
            }
        }
    }
    internal sealed class ListSurface
    {
        internal readonly RectTransform Root;
        internal readonly ScreenLifetime Lifetime;
        readonly ShellUiFactory factory;
        readonly TMP_Text title,empty,error;
        readonly ScrollRect list; readonly RectTransform emptyCard;
        internal ListSurface(ShellUiFactory factory, Transform parent, string name, PopupView modal=null)
        {
            this.factory=factory;
            RectTransform body;
            if(modal!=null)
            {
                Root=ModalRoot(modal,name); title=modal.Panel.Find("Title").GetComponent<TMP_Text>();
                body=ShellUiFactory.Node("Body",Root); ShellUiFactory.Stretch(body); body.gameObject.SetActive(true);
            }
            else { Root=factory.ScreenRoot(parent,name); title=ScreenWidgets.Heading(factory,Root); body=ScreenWidgets.Body(factory,Root); }
            Lifetime=Root.gameObject.AddComponent<ScreenLifetime>();
            list=factory.ScrollList(body); list.GetComponent<RectTransform>().offsetMin=new Vector2(0,100);
            list.gameObject.AddComponent<SwipeForwarder>();
            if(factory.Style.EnableHomeChrome && modal==null) { emptyCard=ScreenWidgets.EmptyCard(factory,Root); empty=emptyCard.GetComponentInChildren<TMP_Text>(true); }
            else empty=factory.Label(body,"");
            error=factory.Label(body,""); error.rectTransform.anchorMin=error.rectTransform.anchorMax=new Vector2(.5f,0);
            error.rectTransform.anchoredPosition=new Vector2(0,45);
        }
        internal static RectTransform ModalRoot(PopupView modal,string name)
        { var root=ShellUiFactory.Node(name,modal.Content); ShellUiFactory.Stretch(root); root.gameObject.SetActive(true); return root; }
        internal void Render(ListDisplay display, Action<string> clicked)
        {
            title.text=display.Title; empty.text=display.Empty;
            bool showEmpty=display.Rows.Count==0; (emptyCard!=null?emptyCard.gameObject:empty.gameObject).SetActive(showEmpty);
            ScreenWidgets.Rows(factory,list.content,display.Rows,clicked);
        }
        internal void Error(string text) { error.text=text; }
    }
}
