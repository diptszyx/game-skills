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
        public bool Highlight { get; }
        public ScoreColumns Columns { get; }
        public RowDisplay(string id, string text, string action = "", bool enabled = false, float progress = -1,string icon="check",string amount=null,string detail=null,bool highlight=false,ScoreColumns columns=null)
        { Id=id; Text=text; Action=action; Enabled=enabled; Progress=progress; Icon=icon; Amount=amount; Detail=detail; Highlight=highlight; Columns=columns; }
    }
    public sealed class ListDisplay
    {
        public string Title { get; }
        public string Empty { get; }
        public IReadOnlyList<RowDisplay> Rows { get; }
        public RowDisplay Pinned { get; }
        public string Caption { get; }
        public ListDisplay(string title, IReadOnlyList<RowDisplay> rows, string empty = "", RowDisplay pinned = null, string caption = null) { Title=title; Rows=rows; Empty=empty; Pinned=pinned; Caption=caption; }
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
        internal const float EmptyCardBottom=.17f, EmptyCardTop=.29f, ModalFooterErrorHeight=56;
        internal const float TrackMinContrast=1.8f, TrackBlend=.4f, FillMinContrast=3f;
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
            bool bar=data.Progress>=0;
            StyleRowLabel(title,RowTitleFont,RowTitleMinFont,bar?.56f:.5f,.96f,xMax);
            var detail=f.Label(row,data.Detail,role:"rowText"); detail.gameObject.name="Detail";
            StyleRowLabel(detail,RowDetailFont,RowDetailFont,bar?.27f:.1f,bar?.56f:.52f,xMax);
        }
        internal static RectTransform EmptyCard(ShellUiFactory f, RectTransform root, bool topAnchored=false)
        {
            var card=ShellUiFactory.Node("Empty card",root);
            if(topAnchored)
            {
                card.anchorMin=new Vector2(0,1); card.anchorMax=Vector2.one; card.pivot=new Vector2(.5f,1);
                card.anchoredPosition=Vector2.zero; card.sizeDelta=new Vector2(0,ShellLayout.Standard.Row.y);
            }
            else { card.anchorMin=new Vector2(.04f,EmptyCardBottom); card.anchorMax=new Vector2(.96f,EmptyCardTop); card.offsetMin=card.offsetMax=Vector2.zero; }
            var image=f.AppearanceImage(card,"Surface","row",true); ShellUiFactory.Stretch(image.rectTransform); OpaqueCard(image);
            var label=f.Label(card,"",role:"rowText"); ShellUiFactory.Stretch(label.rectTransform);
            label.rectTransform.offsetMin=new Vector2(24,12); label.rectTransform.offsetMax=new Vector2(-24,-12);
            label.enableAutoSizing=true; label.fontSizeMax=34; label.fontSizeMin=RowTitleMinFont; label.fontSize=34;
            card.gameObject.SetActive(false); return card;
        }
        const float ProgressInset=16,ProgressBottom=12,ProgressHeight=12;
        // Falls back to a blend towards the row text colour when the style's bar colours are too faint on the row.
        internal static void ProgressColors(ShellStyle style,out Color track,out Color fill)
        {
            var row=new Color(style.ListRowColor.r,style.ListRowColor.g,style.ListRowColor.b,1);
            var text=new Color(style.ListRowTextColor.r,style.ListRowTextColor.g,style.ListRowTextColor.b,1);
            track=style.ProgressTrackColor; track.a=1;
            if(ShellStyle.Contrast(track,row)<TrackMinContrast) track=Color.Lerp(row,text,TrackBlend);
            fill=style.ProgressFillColor; fill.a=1;
            if(ShellStyle.Contrast(fill,track)<FillMinContrast) fill=text;
        }
        internal static void OpaqueCard(Image card)
        { var c=card.color; c.a=Mathf.Max(c.a,MinRowOpacity); card.color=c; }
        internal static void Clear(Transform parent)
        {
            for(int i=parent.childCount-1;i>=0;i--) { var child=parent.GetChild(i); child.gameObject.SetActive(false); if(Application.isPlaying) UnityEngine.Object.Destroy(child.gameObject); else UnityEngine.Object.DestroyImmediate(child.gameObject); }
        }
        internal static void Rows(ShellUiFactory f, RectTransform content, IReadOnlyList<RowDisplay> rows, Action<string> clicked)
        {
            Clear(content);
            foreach(var row in rows) AddRow(f,content,row,clicked);
        }
        internal static RectTransform AddRow(ShellUiFactory f, RectTransform content, RowDisplay row, Action<string> clicked)
        {
            var r=f.ListRow(content,""); var text=r.GetComponentInChildren<TMP_Text>(); text.text=row.Text;
            if(row.Columns!=null) { ScoreRow.Fill(f,r,text,row.Columns); return r; }
            OpaqueCard(r.GetComponent<Image>()); LayoutRowText(r,text,f,row);
            if(row.Progress>=0)
            {
                ProgressColors(f.Style,out var trackColor,out var fillColor);
                var track=ShellUiFactory.Node("Progress",r); track.anchorMin=Vector2.zero; track.anchorMax=new Vector2(.64f,0); track.pivot=Vector2.zero; track.offsetMin=new Vector2(ProgressInset,ProgressBottom); track.offsetMax=new Vector2(-8,ProgressBottom+ProgressHeight);
                track.gameObject.AddComponent<Image>().color=trackColor;
                var fill=ShellUiFactory.Node("Fill",track); fill.anchorMin=Vector2.zero; fill.anchorMax=new Vector2(Mathf.Clamp01(row.Progress),1); fill.offsetMin=fill.offsetMax=Vector2.zero;
                fill.gameObject.AddComponent<Image>().color=fillColor; fill.gameObject.SetActive(true); track.gameObject.SetActive(true);
            }
            if(row.Highlight) text.fontStyle=FontStyles.Bold;
            if(!string.IsNullOrEmpty(row.Action))
            { var b=f.IconButton(r,row.Icon,row.Action,()=>clicked(row.Id),row.Amount!=null?ShellLayoutCategory.IconAmount:ShellLayoutCategory.IconAction,row.Amount); b.interactable=row.Enabled;
              var rt=(RectTransform)b.transform; rt.anchorMin=rt.anchorMax=new Vector2(1,.5f); rt.pivot=new Vector2(1,.5f); rt.anchoredPosition=new Vector2(-12,0);
            }
            return r;
        }
    }
    internal sealed class ListSurface
    {
        internal readonly RectTransform Root;
        internal readonly ScreenLifetime Lifetime;
        readonly ShellUiFactory factory;
        readonly TMP_Text title,empty,error;
        readonly ScrollRect list; readonly RectTransform emptyCard; readonly PopupView modal;
        const float CaptionHeight=40,CaptionFont=24,CaptionInset=16;
        RectTransform pinnedHost; TMP_Text caption;
        int rowCount; float footerHeight,errorHeight;
        internal ListSurface(ShellUiFactory factory, Transform parent, string name, PopupView modal=null)
        {
            this.factory=factory; this.modal=modal;
            RectTransform body;
            if(modal!=null)
            {
                Root=ModalRoot(modal,name); title=modal.Panel.Find("Title").GetComponent<TMP_Text>();
                body=ShellUiFactory.Node("Body",Root); ShellUiFactory.Stretch(body); body.gameObject.SetActive(true);
            }
            else { Root=factory.ScreenRoot(parent,name); title=ScreenWidgets.Heading(factory,Root); body=ScreenWidgets.Body(factory,Root); }
            Lifetime=Root.gameObject.AddComponent<ScreenLifetime>();
            list=factory.ScrollList(body);
            list.gameObject.AddComponent<SwipeForwarder>();
            if(factory.Style.EnableHomeChrome && modal==null) { emptyCard=ScreenWidgets.EmptyCard(factory,Root); empty=emptyCard.GetComponentInChildren<TMP_Text>(true); }
            else if(modal!=null) { emptyCard=ScreenWidgets.EmptyCard(factory,body,true); empty=emptyCard.GetComponentInChildren<TMP_Text>(true); }
            else empty=factory.Label(body,"");
            error=factory.Label(body,""); error.rectTransform.anchorMin=error.rectTransform.anchorMax=new Vector2(.5f,0);
            ApplyBottom();
        }
        float BaseBottom=>modal!=null?0:100;
        void ApplyBottom()
        {
            list.GetComponent<RectTransform>().offsetMin=new Vector2(0,BaseBottom+footerHeight+errorHeight);
            error.rectTransform.anchoredPosition=new Vector2(0,(modal!=null?ScreenWidgets.ModalFooterErrorHeight/2:45)+footerHeight);
        }
        internal static RectTransform ModalRoot(PopupView modal,string name)
        { var root=ShellUiFactory.Node(name,modal.Content); ShellUiFactory.Stretch(root); root.gameObject.SetActive(true); return root; }
        internal void Render(ListDisplay display, Action<string> clicked)
        {
            title.text=display.Title; empty.text=display.Empty;
            bool showEmpty=display.Rows.Count==0; (emptyCard!=null?emptyCard.gameObject:empty.gameObject).SetActive(showEmpty);
            ScreenWidgets.Rows(factory,list.content,display.Rows,clicked);
            rowCount=display.Rows.Count; ShowFooter(display.Pinned,display.Caption); FitModal();
        }
        void ShowFooter(RowDisplay pinned,string captionText)
        {
            bool hasCaption=!string.IsNullOrEmpty(captionText);
            if(pinned==null&&!hasCaption&&pinnedHost==null) return;
            if(pinnedHost==null) BuildFooter();
            ScreenWidgets.Clear(pinnedHost); pinnedHost.gameObject.SetActive(true);
            if(pinned!=null) ShellUiFactory.Stretch(ScreenWidgets.AddRow(factory,pinnedHost,pinned,_=>{}));
            else pinnedHost.gameObject.SetActive(false);
            caption.text=hasCaption?captionText:""; caption.gameObject.SetActive(hasCaption);
            float captionHeight=hasCaption?CaptionHeight:0;
            pinnedHost.anchoredPosition=new Vector2(0,BaseBottom+captionHeight);
            footerHeight=captionHeight+(pinned!=null?ShellLayout.Standard.Row.y+factory.Style.Spacing:0);
            ApplyBottom();
        }
        void BuildFooter()
        {
            var body=(RectTransform)list.transform.parent;
            pinnedHost=ShellUiFactory.Node("Pinned",body); pinnedHost.anchorMin=Vector2.zero; pinnedHost.anchorMax=new Vector2(1,0); pinnedHost.pivot=new Vector2(.5f,0);
            pinnedHost.sizeDelta=new Vector2(0,ShellLayout.Standard.Row.y);
            caption=factory.Label(body,""); caption.gameObject.name="Caption";
            caption.alignment=TextAlignmentOptions.MidlineLeft; caption.enableAutoSizing=false; caption.fontSize=CaptionFont;
            caption.textWrappingMode=TextWrappingModes.NoWrap; caption.overflowMode=TextOverflowModes.Ellipsis;
            var rect=caption.rectTransform; rect.anchorMin=Vector2.zero; rect.anchorMax=new Vector2(1,0); rect.pivot=new Vector2(.5f,0);
            rect.offsetMin=new Vector2(CaptionInset,BaseBottom); rect.offsetMax=new Vector2(-CaptionInset,BaseBottom+CaptionHeight);
        }
        internal void Error(string text)
        {
            error.text=text; if(modal==null) return;
            errorHeight=string.IsNullOrEmpty(text)?0:ScreenWidgets.ModalFooterErrorHeight;
            ApplyBottom(); FitModal();
        }
        void FitModal()
        {
            if(modal==null) return;
            float rowHeight=ShellLayout.Standard.Row.y;
            float rows=rowCount==0?rowHeight:rowCount*rowHeight+(rowCount-1)*factory.Style.Spacing;
            modal.FitModal(rows+list.GetComponent<RectTransform>().offsetMin.y);
        }
    }
}
