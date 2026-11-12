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
        public RectTransform Root { get; }
        public event Action<string> TabSelected;
        readonly ShellUiFactory factory;
        sealed class Tab { public Button Button; public Image Icon, Cell; public GameObject Badge; }
        readonly Dictionary<string,Tab> tabs=new Dictionary<string,Tab>();
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
            layout.padding=new RectOffset(16,16,20,20); layout.spacing=4;
            layout.childControlWidth=layout.childControlHeight=true; layout.childForceExpandWidth=layout.childForceExpandHeight=true;
            foreach(var definition in definitions)
            {
                var r=ShellUiFactory.Node("Tab "+definition.Key,content);
                var target=factory.AppearanceImage(r,"Cell","tabCell",true); ShellUiFactory.Stretch(target.rectTransform); target.raycastTarget=true;
                var button=r.gameObject.AddComponent<Button>(); button.targetGraphic=target; button.transition=Selectable.Transition.None;
                var size=r.gameObject.AddComponent<LayoutElement>(); size.minWidth=size.minHeight=44; size.flexibleWidth=1;
                var icon=factory.AppearanceImage(r,"Icon","tabIcon",iconKey:definition.IconKey);
                TopBarView.Place(icon.rectTransform,new Vector2(.26f,.43f),new Vector2(.74f,.94f));
                var label=factory.Label(r,definition.Label,role:"tabLabel"); label.fontSize=20; label.fontSizeMax=20; label.fontSizeMin=16; label.maxVisibleLines=2;
                TopBarView.Place(label.rectTransform,new Vector2(.06f,.06f),new Vector2(.94f,.43f));
                var badge=factory.AppearanceImage(icon.transform,"Badge","badge");
                badge.rectTransform.anchorMin=badge.rectTransform.anchorMax=Vector2.one;
                badge.rectTransform.sizeDelta=new Vector2(28,28); badge.rectTransform.anchoredPosition=new Vector2(-2,-2);
                badge.gameObject.SetActive(false);
                tabs.Add(definition.Key,new Tab{Button=button,Icon=icon,Cell=target,Badge=badge.gameObject});
                var key=definition.Key; button.onClick.AddListener(()=>{ SetSelected(key); TabSelected?.Invoke(key); });
                r.gameObject.SetActive(true);
            }
            content.gameObject.SetActive(true);
        }
        public void SetSelected(string key)
        {
            if(!tabs.ContainsKey(key)) throw new ArgumentException("Unknown tab: "+key,nameof(key));
            foreach(var pair in tabs)
            {
                bool selected=pair.Key==key; var appearance=pair.Value.Icon.GetComponent<ShellAppearance>();
                appearance.Selected=selected; appearance.Apply(factory.Style);
                var cell=pair.Value.Cell.GetComponent<ShellAppearance>(); cell.Selected=selected; cell.Apply(factory.Style);
                pair.Value.Icon.transform.localScale=Vector3.one*(selected?1.15f:1);
            }
        }
        public void SetLabel(string key,string label) { tabs[key].Button.GetComponentInChildren<TMPro.TMP_Text>().text=label; }
        public void SetBadge(string key,bool visible) { tabs[key].Badge.SetActive(visible); }
    }
}
