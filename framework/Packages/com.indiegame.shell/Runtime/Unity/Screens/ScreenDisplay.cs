using System;
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
        public RowDisplay(string id, string text, string action = "", bool enabled = false, float progress = -1)
        { Id=id; Text=text; Action=action; Enabled=enabled; Progress=progress; }
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
        internal static RectTransform Body(ShellUiFactory f, RectTransform root)
        {
            var body=ShellUiFactory.Node("Body",root); ShellUiFactory.Stretch(body);
            body.offsetMin=new Vector2(f.Style.Spacing,f.Style.Spacing); body.offsetMax=new Vector2(-f.Style.Spacing,-120);
            body.gameObject.SetActive(true); return body;
        }
        internal static TMP_Text Heading(ShellUiFactory f, RectTransform root)
        { var t=f.Label(root,"",true); t.rectTransform.anchorMin=t.rectTransform.anchorMax=new Vector2(.5f,1); t.rectTransform.anchoredPosition=new Vector2(0,-50); return t; }
        internal static void Rows(ShellUiFactory f, RectTransform content, IReadOnlyList<RowDisplay> rows, Action<string> clicked)
        {
            for(int i=content.childCount-1;i>=0;i--) { var child=content.GetChild(i); child.gameObject.SetActive(false); if(Application.isPlaying) UnityEngine.Object.Destroy(child.gameObject); else UnityEngine.Object.DestroyImmediate(child.gameObject); }
            foreach(var row in rows)
            {
                var r=f.ListRow(content,""); var text=r.GetComponentInChildren<TMP_Text>(); text.text=row.Text;
                text.rectTransform.anchorMax=new Vector2(.64f,1); text.alignment=TextAlignmentOptions.Left;
                text.rectTransform.offsetMin=new Vector2(f.Style.Spacing,0); text.rectTransform.offsetMax=Vector2.zero;
                if(row.Progress>=0)
                {
                    var track=ShellUiFactory.Node("Progress",r); track.anchorMin=new Vector2(0,0); track.anchorMax=new Vector2(.64f,0); track.sizeDelta=new Vector2(0,8);
                    track.gameObject.AddComponent<Image>().color=f.Style.ProgressTrackColor;
                    var fill=ShellUiFactory.Node("Fill",track); fill.anchorMin=Vector2.zero; fill.anchorMax=new Vector2(Mathf.Clamp01(row.Progress),1); fill.offsetMin=fill.offsetMax=Vector2.zero;
                    fill.gameObject.AddComponent<Image>().color=f.Style.ProgressFillColor; fill.gameObject.SetActive(true); track.gameObject.SetActive(true);
                }
                if(!string.IsNullOrEmpty(row.Action))
                { var b=f.Button(r,row.Action,clicked:()=>clicked(row.Id)); b.interactable=row.Enabled;
                  var rt=(RectTransform)b.transform; rt.anchorMin=new Vector2(.66f,.1f); rt.anchorMax=new Vector2(.98f,.9f); rt.offsetMin=rt.offsetMax=Vector2.zero;
                  ShellUiFactory.Stretch(b.GetComponentInChildren<TMP_Text>().rectTransform); }
            }
        }
    }
    internal sealed class ListSurface
    {
        internal readonly RectTransform Root;
        internal readonly ScreenLifetime Lifetime;
        readonly ShellUiFactory factory;
        readonly TMP_Text title,empty,error;
        readonly ScrollRect list;
        internal ListSurface(ShellUiFactory factory, Transform parent, string name)
        {
            this.factory=factory; Root=factory.ScreenRoot(parent,name); Lifetime=Root.gameObject.AddComponent<ScreenLifetime>();
            title=ScreenWidgets.Heading(factory,Root); var body=ScreenWidgets.Body(factory,Root);
            list=factory.ScrollList(body); list.GetComponent<RectTransform>().offsetMin=new Vector2(0,100);
            empty=factory.Label(body,""); error=factory.Label(body,""); error.rectTransform.anchorMin=error.rectTransform.anchorMax=new Vector2(.5f,0);
            error.rectTransform.anchoredPosition=new Vector2(0,45);
        }
        internal void Render(ListDisplay display, Action<string> clicked)
        { title.text=display.Title; empty.text=display.Empty; empty.gameObject.SetActive(display.Rows.Count==0); ScreenWidgets.Rows(factory,list.content,display.Rows,clicked); }
        internal void Error(string text) { error.text=text; }
    }
}
