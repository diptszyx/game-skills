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
        internal const float RewardBandHeight=.30f;
        const float DayLabelBottom=.76f;
        public DayCellView(ShellUiFactory factory, Transform parent)
        {
            this.factory=factory;
            Root=ShellUiFactory.Node("DayCell",parent); Root.sizeDelta=new Vector2(122,160);
            background=factory.AppearanceImage(Root,"State","dayLocked",true);
            TopBarView.Place(background.rectTransform,new Vector2(0,RewardBandHeight),Vector2.one);
            day=factory.Label(Root,"",role:"tabLabel"); TopBarView.Place(day.rectTransform,new Vector2(0,DayLabelBottom),Vector2.one);
            rewardCard=ShellUiFactory.Node("Reward card",Root); TopBarView.Place(rewardCard,Vector2.zero,new Vector2(1,RewardBandHeight-.03f));
            var card=factory.AppearanceImage(rewardCard,"Surface","row",true); ShellUiFactory.Stretch(card.rectTransform);
            reward=factory.Label(rewardCard,"",role:"rowText"); ShellUiFactory.Stretch(reward.rectTransform);
            day.fontSize=day.fontSizeMax=22; day.fontSizeMin=14; day.fontStyle=FontStyles.Bold;
            reward.fontSize=reward.fontSizeMax=22; reward.fontSizeMin=14; rewardCard.gameObject.SetActive(false); Root.gameObject.SetActive(true);
        }
        public void Render(string dayLabel,string rewardText,DayState state)
        {
            if(!System.Enum.IsDefined(typeof(DayState),state)) throw new System.ArgumentOutOfRangeException(nameof(state));
            day.text=dayLabel; reward.text=rewardText; rewardCard.gameObject.SetActive(!string.IsNullOrEmpty(rewardText));
            var appearance=background.GetComponent<ShellAppearance>();
            appearance.Role=state==DayState.Collected?"dayCollected":state==DayState.Today?"dayToday":"dayLocked";
            appearance.Apply(factory.Style);
        }
    }
}
