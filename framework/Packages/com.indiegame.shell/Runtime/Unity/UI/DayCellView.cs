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
        readonly TMP_Text day,reward;
        public DayCellView(ShellUiFactory factory, Transform parent)
        {
            this.factory=factory;
            Root=ShellUiFactory.Node("DayCell",parent); Root.sizeDelta=new Vector2(180,250);
            background=factory.AppearanceImage(Root,"State","dayLocked",true);
            TopBarView.Place(background.rectTransform,new Vector2(0,.18f),new Vector2(1,.82f));
            day=factory.Label(Root,""); TopBarView.Place(day.rectTransform,new Vector2(0,.82f),Vector2.one);
            reward=factory.Label(Root,""); TopBarView.Place(reward.rectTransform,Vector2.zero,new Vector2(1,.18f));
            day.fontSize=reward.fontSize=22; Root.gameObject.SetActive(true);
        }
        public void Render(string dayLabel,string rewardText,DayState state)
        {
            if(!System.Enum.IsDefined(typeof(DayState),state)) throw new System.ArgumentOutOfRangeException(nameof(state));
            day.text=dayLabel; reward.text=rewardText;
            var appearance=background.GetComponent<ShellAppearance>();
            appearance.Role=state==DayState.Collected?"dayCollected":state==DayState.Today?"dayToday":"dayLocked";
            appearance.Apply(factory.Style);
        }
    }
}
