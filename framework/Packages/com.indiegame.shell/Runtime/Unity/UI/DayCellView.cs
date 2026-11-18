using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Unity.UI
{
    public enum DayState { Collected, Today, Locked }
    public sealed class DayCellView
    {
        public RectTransform Root { get; }
        readonly ShellUiFactory factory;
        readonly Image background;
        readonly RectTransform rewardCard;
        readonly TMP_Text day,reward;
        readonly RectTransform goldIcon;
        internal const float RewardBandHeight=.30f;
        const float DayLabelBottom=.76f, GoldTextInset=34;
        public DayCellView(ShellUiFactory factory, Transform parent)
        {
            this.factory=factory;
            Root=ShellUiFactory.Node("DayCell",parent); Root.sizeDelta=new Vector2(122,160);
            background=factory.AppearanceImage(Root,"State","dayLocked",true);
            TopBarView.Place(background.rectTransform,new Vector2(0,RewardBandHeight),Vector2.one);
            day=factory.Label(Root,"",role:"tabLabel"); TopBarView.Place(day.rectTransform,new Vector2(0,DayLabelBottom),Vector2.one);
            rewardCard=ShellUiFactory.Node("Reward card",Root); TopBarView.Place(rewardCard,Vector2.zero,new Vector2(1,RewardBandHeight-.03f));
            var card=factory.AppearanceImage(rewardCard,"Surface","row",true); ShellUiFactory.Stretch(card.rectTransform);
            goldIcon=factory.AppearanceImage(rewardCard,"Gold icon","currencyIcon").rectTransform;
            TopBarView.Place(goldIcon,new Vector2(.06f,.08f),new Vector2(.36f,.92f)); goldIcon.gameObject.SetActive(false);
            reward=factory.Label(rewardCard,"",role:"rowText"); ShellUiFactory.Stretch(reward.rectTransform);
            day.fontSize=day.fontSizeMax=22; day.fontSizeMin=14; day.fontStyle=FontStyles.Bold;
            reward.fontSize=reward.fontSizeMax=22; reward.fontSizeMin=14; rewardCard.gameObject.SetActive(false); Root.gameObject.SetActive(true);
        }
        public void Render(string dayLabel,string rewardText,DayState state,bool goldOnly=false)
        {
            if(!System.Enum.IsDefined(typeof(DayState),state)) throw new System.ArgumentOutOfRangeException(nameof(state));
            day.text=dayLabel; reward.text=rewardText; rewardCard.gameObject.SetActive(!string.IsNullOrEmpty(rewardText));
            goldIcon.gameObject.SetActive(goldOnly);
            reward.rectTransform.offsetMin=new Vector2(goldOnly?GoldTextInset:0,0);
            var appearance=background.GetComponent<ShellAppearance>();
            appearance.Role=state==DayState.Collected?"dayCollected":state==DayState.Today?"dayToday":"dayLocked";
            appearance.Apply(factory.Style);
        }
    }
}
