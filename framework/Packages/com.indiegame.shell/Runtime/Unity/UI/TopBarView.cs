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
        public Button Settings { get; }
        public TopBarView(ShellUiFactory factory, Transform parent)
        {
            Root=ShellUiFactory.Node("TopBar",parent);
            Root.anchorMin=new Vector2(0,1); Root.anchorMax=Vector2.one; Root.pivot=new Vector2(.5f,1);
            Root.sizeDelta=new Vector2(0,110); Root.anchoredPosition=Vector2.zero;
            currency=factory.AppearanceImage(Root,"Currency pill","currencyPill",true).rectTransform;
            Place(currency,new Vector2(.02f,0),new Vector2(.48f,1));
            currency.offsetMin=new Vector2(0,15); currency.offsetMax=new Vector2(0,-15);
            lives=factory.AppearanceImage(Root,"Lives pill","livesPill",true).rectTransform;
            Place(lives,new Vector2(.52f,0),new Vector2(.98f,1));
            lives.offsetMin=new Vector2(0,15); lives.offsetMax=new Vector2(0,-15);
            Place(factory.AppearanceImage(currency,"Currency icon","currencyIcon").rectTransform,new Vector2(.02f,.1f),new Vector2(.25f,.9f));
            Place(factory.AppearanceImage(lives,"Heart icon","heartIcon").rectTransform,new Vector2(.02f,.1f),new Vector2(.25f,.9f));
            gold=factory.Label(currency,"",role:"chromeText"); Place(gold.rectTransform,new Vector2(.28f,.1f),new Vector2(.95f,.9f));
            hearts=factory.Label(lives,"",role:"chromeText"); Place(hearts.rectTransform,new Vector2(.28f,.18f),new Vector2(.43f,.82f));
            countdown=factory.Label(lives,"",role:"chromeText"); Place(countdown.rectTransform,new Vector2(.45f,.18f),new Vector2(.90f,.82f));
            foreach(var text in new[]{gold,hearts,countdown})
            {
                text.fontSize=28; text.fontSizeMin=24; text.fontSizeMax=28;
                text.alignment=TextAlignmentOptions.MidlineLeft; text.textWrappingMode=TextWrappingModes.NoWrap;
            }
            countdown.fontSize=26; countdown.fontSizeMax=26; countdown.fontSizeMin=18;
            var gear=factory.AppearanceImage(Root,"Shell Settings","gearButton"); gear.raycastTarget=true;
            Place(gear.rectTransform,new Vector2(.86f,0),new Vector2(.98f,1)); gear.rectTransform.offsetMin=new Vector2(0,10); gear.rectTransform.offsetMax=new Vector2(0,-10);
            Settings=gear.gameObject.AddComponent<Button>(); Settings.targetGraphic=gear; Settings.transition=Selectable.Transition.None;
            Settings.gameObject.SetActive(false); Root.gameObject.SetActive(true);
        }
        public void SetCompact(bool compact)
        {
            Place(currency,new Vector2(.02f,0),new Vector2(compact?.40f:.48f,1)); Place(lives,new Vector2(compact?.42f:.52f,0),new Vector2(compact?.80f:.98f,1));
            currency.offsetMin=lives.offsetMin=new Vector2(0,15); currency.offsetMax=lives.offsetMax=new Vector2(0,-15);
            Settings.gameObject.SetActive(compact);
        }
        internal static void Place(RectTransform r, Vector2 min, Vector2 max)
        { r.anchorMin=min; r.anchorMax=max; r.offsetMin=r.offsetMax=Vector2.zero; }
        public void Render(TopBarDisplay display)
        { gold.text=display.Gold.ToString(System.Globalization.CultureInfo.InvariantCulture); hearts.text=display.Hearts.ToString(); countdown.text=display.Countdown ?? ""; }
    }
}
