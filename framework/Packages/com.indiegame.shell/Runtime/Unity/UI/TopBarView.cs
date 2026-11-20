using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Unity.UI
{
    public sealed class TopBarDisplay
    {
        public long Gold { get; }
        public int Hearts { get; }
        public string Countdown { get; }
        public TopBarDisplay(long gold, int hearts, string countdown) { Gold=gold; Hearts=hearts; Countdown=countdown; }
    }
    public sealed class TopBarView
    {
        public RectTransform Root { get; }
        readonly TMP_Text gold, hearts, countdown;
        readonly RectTransform currency, lives;
        readonly CanvasGroup currencyGroup, livesGroup;
        public RectTransform GoldIcon { get; }
        public RectTransform HeartIcon { get; }
        public Button Settings { get; }
        public TopBarView(ShellUiFactory factory, Transform parent)
        {
            Root=ShellUiFactory.Node("TopBar",parent);
            Root.anchorMin=new Vector2(0,1); Root.anchorMax=Vector2.one; Root.pivot=new Vector2(.5f,1);
            Root.sizeDelta=new Vector2(0,110); Root.anchoredPosition=Vector2.zero;
            currency=factory.AppearanceImage(Root,"Currency pill","currencyPill",true).rectTransform;
            bool compact=factory.Style.CompactResourcePills;
            Place(currency,new Vector2(.02f,0),new Vector2(compact?.30f:.40f,1));
            currency.offsetMin=new Vector2(0,15); currency.offsetMax=new Vector2(0,-15);
            lives=factory.AppearanceImage(Root,"Lives pill","livesPill",true).rectTransform;
            Place(lives,new Vector2(compact?.32f:.42f,0),new Vector2(compact?.70f:.80f,1));
            lives.offsetMin=new Vector2(0,15); lives.offsetMax=new Vector2(0,-15);
            currencyGroup=currency.gameObject.AddComponent<CanvasGroup>(); livesGroup=lives.gameObject.AddComponent<CanvasGroup>();
            GoldIcon=factory.AppearanceImage(currency,"Currency icon","currencyIcon").rectTransform; Place(GoldIcon,new Vector2(.02f,.1f),new Vector2(.25f,.9f));
            HeartIcon=factory.AppearanceImage(lives,"Heart icon","heartIcon").rectTransform; Place(HeartIcon,new Vector2(.02f,.1f),new Vector2(.25f,.9f));
            gold=factory.Label(currency,"",role:"chromeText"); Place(gold.rectTransform,new Vector2(.28f,.1f),new Vector2(.95f,.9f));
            hearts=factory.Label(lives,"",role:"chromeText"); Place(hearts.rectTransform,new Vector2(.27f,.18f),new Vector2(.42f,.82f));
            countdown=factory.Label(lives,"",role:"chromeText"); Place(countdown.rectTransform,new Vector2(.45f,.18f),new Vector2(.97f,.82f));
            foreach(var text in new[]{gold,hearts,countdown})
            {
                text.fontSize=28; text.fontSizeMin=24; text.fontSizeMax=28;
                text.alignment=TextAlignmentOptions.MidlineLeft; text.textWrappingMode=TextWrappingModes.NoWrap;
            }
            countdown.fontSize=26; countdown.fontSizeMax=26; countdown.fontSizeMin=18;
            var gear=factory.AppearanceImage(Root,"Shell Settings","gearButton"); gear.raycastTarget=true;
            Place(gear.rectTransform,new Vector2(.86f,0),new Vector2(.98f,1)); gear.rectTransform.offsetMin=new Vector2(0,10); gear.rectTransform.offsetMax=new Vector2(0,-10);
            Settings=gear.gameObject.AddComponent<Button>(); Settings.targetGraphic=gear; Settings.transition=Selectable.Transition.None; factory.WithPressFeedback(Settings);
            Root.gameObject.SetActive(true);
        }
        internal static void Place(RectTransform r, Vector2 min, Vector2 max)
        { r.anchorMin=min; r.anchorMax=max; r.offsetMin=r.offsetMax=Vector2.zero; }
        public void ShowPills(bool showGold, bool showLives)
        { currencyGroup.alpha=showGold?1:0; livesGroup.alpha=showLives?1:0; }
        public void Render(TopBarDisplay display)
        { gold.text=display.Gold.ToString(System.Globalization.CultureInfo.InvariantCulture); hearts.text=display.Hearts.ToString(); countdown.text=display.Countdown ?? ""; }
    }
}
