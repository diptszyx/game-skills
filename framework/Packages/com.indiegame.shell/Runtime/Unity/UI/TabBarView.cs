using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Unity.UI
{
    public sealed class TabDefinition
    {
        public string Key { get; }
        public string Label { get; }
        public string IconKey { get; }
        public TabDefinition(string key, string label, string iconKey) { Key=key; Label=label; IconKey=iconKey; }
    }
    public sealed class TabBarView
    {
        public const int LabelFontSize=22;
        public RectTransform Root { get; }
        public event Action<string> TabSelected;
        readonly ShellUiFactory factory;
        sealed class Tab { public Button Button; public Image Icon, Cell; public GameObject Badge; }
        readonly Dictionary<string,Tab> tabs=new Dictionary<string,Tab>();
        readonly List<string> order=new List<string>();
        public string Selected { get; private set; }
        public TabBarView(ShellUiFactory factory, Transform parent, IReadOnlyList<TabDefinition> definitions)
        {
            if (definitions == null || definitions.Count == 0) throw new ArgumentException("Tabs are required",nameof(definitions));
            var keys=new HashSet<string>();
            foreach(var definition in definitions)
                if(definition == null || string.IsNullOrEmpty(definition.Key) || !keys.Add(definition.Key)) throw new ArgumentException("Tab keys must be nonempty and unique",nameof(definitions));
            this.factory=factory;
            Root=factory.AppearanceImage(parent,"TabBar","tabBar",true).rectTransform;
            Root.anchorMin=Vector2.zero; Root.anchorMax=new Vector2(1,0); Root.pivot=new Vector2(.5f,0);
            Root.sizeDelta=new Vector2(0,160); Root.anchoredPosition=Vector2.zero;
            var content=ShellUiFactory.Node("Tabs",Root); ShellUiFactory.Stretch(content);
            var layout=content.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding=new RectOffset(10,10,16,16); layout.spacing=2;
            layout.childControlWidth=layout.childControlHeight=true; layout.childForceExpandWidth=layout.childForceExpandHeight=true;
            foreach(var definition in definitions)
            {
                order.Add(definition.Key);
                var r=ShellUiFactory.Node("Tab "+definition.Key,content);
                var target=factory.AppearanceImage(r,"Cell","tabCell",true); ShellUiFactory.Stretch(target.rectTransform); target.raycastTarget=true;
                r.gameObject.AddComponent<ActionAccessibility>().Set(definition.Key,definition.Label);
                var button=r.gameObject.AddComponent<Button>(); button.targetGraphic=target; button.transition=Selectable.Transition.None;
                var size=r.gameObject.AddComponent<LayoutElement>(); size.minWidth=size.minHeight=44; size.flexibleWidth=1;
                var icon=factory.AppearanceImage(r,"Icon","tabIcon",iconKey:definition.IconKey);
                TopBarView.Place(icon.rectTransform,new Vector2(.28f,.5f),new Vector2(.72f,.96f));
                var label=factory.Label(r,definition.Label,role:"tabLabel"); label.fontSize=LabelFontSize; label.fontSizeMax=LabelFontSize; label.fontSizeMin=LabelFontSize; label.enableAutoSizing=true; label.maxVisibleLines=2;
                TopBarView.Place(label.rectTransform,new Vector2(.02f,.03f),new Vector2(.98f,.46f));
                var badge=factory.AppearanceImage(icon.transform,"Badge","badge");
                badge.rectTransform.anchorMin=badge.rectTransform.anchorMax=Vector2.one;
                badge.rectTransform.sizeDelta=new Vector2(28,28); badge.rectTransform.anchoredPosition=new Vector2(-2,-2);
                badge.gameObject.SetActive(false);
                tabs.Add(definition.Key,new Tab{Button=button,Icon=icon,Cell=target,Badge=badge.gameObject});
                var key=definition.Key; button.onClick.AddListener(()=>Select(key));
                r.gameObject.SetActive(true);
            }
            content.gameObject.SetActive(true);
        }
        public void Select(string key) { SetSelected(key); TabSelected?.Invoke(key); }
        public bool Step(int direction)
        {
            int target=order.IndexOf(Selected)+direction;
            if(Selected==null||target<0||target>=order.Count) return false;
            Select(order[target]); return true;
        }
        public void SetSelected(string key)
        {
            if(!tabs.ContainsKey(key)) throw new ArgumentException("Unknown tab: "+key,nameof(key));
            Selected=key;
            foreach(var pair in tabs)
            {
                bool selected=pair.Key==key; var appearance=pair.Value.Icon.GetComponent<ShellAppearance>();
                appearance.Selected=selected; appearance.Apply(factory.Style);
                var cell=pair.Value.Cell.GetComponent<ShellAppearance>(); cell.Selected=selected; cell.Apply(factory.Style);
                pair.Value.Icon.transform.localScale=Vector3.one*(selected?1.15f:1);
            }
        }
        public Button ButtonFor(string key)=>tabs[key].Button;
        public void SetLabel(string key,string label) { tabs[key].Button.GetComponent<ActionAccessibility>().Set(key,label); tabs[key].Button.GetComponentInChildren<TMPro.TMP_Text>().text=label; }
        public void SetBadge(string key,bool visible) { tabs[key].Badge.SetActive(visible); }
    }
}
