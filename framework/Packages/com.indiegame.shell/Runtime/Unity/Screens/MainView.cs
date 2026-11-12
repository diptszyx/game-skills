using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IndieGame.Shell.Unity.UI;
namespace IndieGame.Shell.Unity.Screens
{
    public interface IGameSessionActions { void Play(); void Retry(); void Revive(); void NextLevel(); void Leave(); void Pause(); void Resume(); }
    public enum ShellScreen { CheckIn, Quests, Shop, Leaderboard }
    public sealed class MainDisplay
    {
        public string Title,Gold,Hearts,Countdown,Play,CheckIn,Quests,Shop;
    }
    public sealed class MainView
    {
        public RectTransform Root { get; }
        public ScreenLifetime Lifetime { get; }
        public MainDisplay Snapshot { get; private set; }
        public string Error { get; private set; }
        public event Action PlayRequested;
        public event Action<ShellScreen> OpenRequested;
        readonly TMP_Text title,gold,hearts,countdown,error;
        readonly Button play,checkIn,quests,shop;
        public MainView(ShellUiFactory f, Transform parent)
        {
            Root=f.ScreenRoot(parent,"Main"); Lifetime=Root.gameObject.AddComponent<ScreenLifetime>(); if(f.Style.EnableHomeChrome)
            {
                var content=ShellUiFactory.Node("Home content",Root); ShellUiFactory.Stretch(content);
                content.offsetMin=new Vector2(24,160); content.offsetMax=new Vector2(-24,-110); content.gameObject.SetActive(true);
                HomeArt(f,content,"Logo",f.Style.Logo,new Vector2(.12f,.72f),new Vector2(.88f,.97f));
                HomeArt(f,content,"Mascot",f.Style.Mascot,new Vector2(.22f,.30f),new Vector2(.78f,.68f));
                play=f.Button(content,"",clicked:RequestPlay);
                var label=play.GetComponentInChildren<TMP_Text>(); label.fontSize=label.fontSizeMax=48; label.fontStyle=FontStyles.Bold;
                var rect=(RectTransform)play.transform;
                rect.anchorMin=new Vector2(.15f,.08f); rect.anchorMax=new Vector2(.85f,.23f); rect.offsetMin=rect.offsetMax=Vector2.zero;
                error=f.Label(content,""); error.rectTransform.anchorMin=new Vector2(0,0); error.rectTransform.anchorMax=new Vector2(1,.07f); error.rectTransform.offsetMin=error.rectTransform.offsetMax=Vector2.zero;
                return;
            }
            title=ScreenWidgets.Heading(f,Root);
            var body=ScreenWidgets.Body(f,Root); var layout=body.gameObject.AddComponent<VerticalLayoutGroup>(); layout.spacing=f.Style.Spacing; layout.childControlWidth=true; layout.childForceExpandWidth=true; layout.childControlHeight=false; layout.childForceExpandHeight=false;
            gold=f.Label(body,""); hearts=f.Label(body,""); countdown=f.Label(body,"");
            play=f.Button(body,"",clicked:RequestPlay); checkIn=f.Button(body,"",clicked:()=>RequestOpen(ShellScreen.CheckIn));
            quests=f.Button(body,"",clicked:()=>RequestOpen(ShellScreen.Quests)); shop=f.Button(body,"",clicked:()=>RequestOpen(ShellScreen.Shop)); error=f.Label(body,"");
        }
        public void Render(MainDisplay d)
        { Snapshot=d; play.GetComponentInChildren<TMP_Text>().text=d.Play; if(title==null) return; title.text=d.Title; gold.text=d.Gold; hearts.text=d.Hearts; countdown.text=d.Countdown;
          play.GetComponentInChildren<TMP_Text>().text=d.Play; checkIn.GetComponentInChildren<TMP_Text>().text=d.CheckIn;
          quests.GetComponentInChildren<TMP_Text>().text=d.Quests; shop.GetComponentInChildren<TMP_Text>().text=d.Shop; }
        static void HomeArt(ShellUiFactory f,Transform parent,string name,Sprite sprite,Vector2 min,Vector2 max)
        {
            var rect=ShellUiFactory.Node(name,parent); TopBarView.Place(rect,min,max);
            var image=rect.gameObject.AddComponent<Image>(); image.sprite=sprite; image.preserveAspect=true; image.raycastTarget=false;
            rect.gameObject.SetActive(sprite!=null);
        }
        public void RequestPlay() => PlayRequested?.Invoke();
        public void RequestOpen(ShellScreen screen) => OpenRequested?.Invoke(screen);
        public void ShowError(string text) { Error=text; error.text=text; }
    }
}
