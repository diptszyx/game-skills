using System;
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
        readonly Image cellBack,stateIcon;
        readonly RectTransform rewardCard;
        readonly TMP_Text day,reward; readonly float todayScale;
        readonly RectTransform goldIcon;
        readonly Button button; readonly ActionAccessibility accessibility;
        internal static readonly Vector2 LabelBand=new Vector2(.78f,.98f), IconBand=new Vector2(.32f,.76f), RewardBand=new Vector2(.04f,.28f);
        public const string AccessibilityKey="day";
        public const float LockedRewardAlpha=.75f; internal const float CardMarginX=.06f, IconMarginX=.1f;
        const float GoldTextInset=34;
        public DayCellView(ShellUiFactory factory, Transform parent, float todayScale=1, Action claimed=null)
        {
            this.factory=factory; this.todayScale=todayScale;
            Root=ShellUiFactory.Node("DayCell",parent); Root.sizeDelta=new Vector2(122,160);
            cellBack=factory.AppearanceImage(Root,"Cell","row",true); ShellUiFactory.Stretch(cellBack.rectTransform); cellBack.raycastTarget=true;
            stateIcon=factory.AppearanceImage(Root,"State","dayLocked");
            TopBarView.Place(stateIcon.rectTransform,new Vector2(IconMarginX,IconBand.x),new Vector2(1-IconMarginX,IconBand.y));
            button=Root.gameObject.AddComponent<Button>(); button.targetGraphic=cellBack; button.transition=Selectable.Transition.None; factory.WithPressFeedback(button);
            if(claimed!=null) button.onClick.AddListener(()=>claimed());
            accessibility=Root.gameObject.AddComponent<ActionAccessibility>(); accessibility.Set(AccessibilityKey,"Day");
            day=factory.Label(Root,"",role:"tabLabel"); day.name="Day label"; TopBarView.Place(day.rectTransform,new Vector2(0,LabelBand.x),new Vector2(1,LabelBand.y));
            rewardCard=ShellUiFactory.Node("Reward card",Root); TopBarView.Place(rewardCard,new Vector2(CardMarginX,RewardBand.x),new Vector2(1-CardMarginX,RewardBand.y));
            var card=factory.AppearanceImage(rewardCard,"Surface","row",true); ShellUiFactory.Stretch(card.rectTransform);
            goldIcon=factory.AppearanceImage(rewardCard,"Gold icon","currencyIcon").rectTransform;
            TopBarView.Place(goldIcon,new Vector2(.06f,.08f),new Vector2(.36f,.92f)); goldIcon.gameObject.SetActive(false);
            reward=factory.Label(rewardCard,"",role:"rowText"); reward.name="Reward text"; reward.alignment=TextAlignmentOptions.Center; ShellUiFactory.Stretch(reward.rectTransform);
            day.fontSize=day.fontSizeMax=22; day.fontSizeMin=14; day.fontStyle=FontStyles.Bold;
            reward.fontSize=reward.fontSizeMax=22; reward.fontSizeMin=14; rewardCard.gameObject.SetActive(false); Root.gameObject.SetActive(true);
        }
        public void Render(string dayLabel,string rewardText,DayState state,bool goldOnly=false,string claimLabel=null)
        {
            if(!System.Enum.IsDefined(typeof(DayState),state)) throw new System.ArgumentOutOfRangeException(nameof(state));
            button.interactable=state==DayState.Today;
            accessibility.Set(AccessibilityKey,state==DayState.Today && !string.IsNullOrWhiteSpace(claimLabel)?claimLabel:dayLabel);
            day.text=dayLabel; reward.text=rewardText; rewardCard.gameObject.SetActive(!string.IsNullOrEmpty(rewardText));
            goldIcon.gameObject.SetActive(goldOnly);
            float inset=goldOnly?GoldTextInset:0; reward.rectTransform.offsetMin=new Vector2(inset,0); reward.rectTransform.offsetMax=new Vector2(-inset,0);
            float alpha=state==DayState.Locked?LockedRewardAlpha:1; reward.alpha=alpha; goldIcon.GetComponent<Image>().canvasRenderer.SetAlpha(alpha);
            var appearance=stateIcon.GetComponent<ShellAppearance>();
            appearance.Role=state==DayState.Collected?"dayCollected":state==DayState.Today?"dayToday":"dayLocked";
            appearance.Apply(factory.Style);
            var emphasis=Vector3.one*(state==DayState.Today?todayScale:1); cellBack.rectTransform.localScale=emphasis; stateIcon.rectTransform.localScale=emphasis;
        }
    }
}
