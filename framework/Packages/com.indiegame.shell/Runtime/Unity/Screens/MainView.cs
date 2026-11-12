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
            Root=f.ScreenRoot(parent,"Main"); Lifetime=Root.gameObject.AddComponent<ScreenLifetime>(); title=ScreenWidgets.Heading(f,Root);
            var body=ScreenWidgets.Body(f,Root); var layout=body.gameObject.AddComponent<VerticalLayoutGroup>(); layout.spacing=f.Style.Spacing; layout.childControlWidth=true; layout.childForceExpandWidth=true; layout.childControlHeight=false; layout.childForceExpandHeight=false;
            gold=f.Label(body,""); hearts=f.Label(body,""); countdown=f.Label(body,"");
            play=f.Button(body,"",clicked:RequestPlay); checkIn=f.Button(body,"",clicked:()=>RequestOpen(ShellScreen.CheckIn));
            quests=f.Button(body,"",clicked:()=>RequestOpen(ShellScreen.Quests)); shop=f.Button(body,"",clicked:()=>RequestOpen(ShellScreen.Shop)); error=f.Label(body,"");
        }
        public void Render(MainDisplay d)
        { Snapshot=d; title.text=d.Title; gold.text=d.Gold; hearts.text=d.Hearts; countdown.text=d.Countdown;
          play.GetComponentInChildren<TMP_Text>().text=d.Play; checkIn.GetComponentInChildren<TMP_Text>().text=d.CheckIn;
          quests.GetComponentInChildren<TMP_Text>().text=d.Quests; shop.GetComponentInChildren<TMP_Text>().text=d.Shop; }
        public void RequestPlay() => PlayRequested?.Invoke();
        public void RequestOpen(ShellScreen screen) => OpenRequested?.Invoke(screen);
        public void ShowError(string text) { Error=text; error.text=text; }
    }
}
