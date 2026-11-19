using UnityEngine;
namespace IndieGame.Shell.Unity.UI
{
    // Game-supplied extra for the win or lose popup (e.g. a cheering or sad mascot). The shell calls Play once the popup has finished opening and Stop when it leaves.
    public interface IResultCelebration
    {
        void Play(RectTransform stage);
        void Stop();
    }
    public readonly struct ResultCelebrationLayout
    {
        public readonly float StageHeight, ConfettiOriginHeightFraction;
        public ResultCelebrationLayout(float stageHeight,float confettiOriginHeightFraction)
        { StageHeight=stageHeight; ConfettiOriginHeightFraction=confettiOriginHeightFraction; }
        public static readonly ResultCelebrationLayout Standard=new ResultCelebrationLayout(360,.35f);
    }
    // Lives on a result popup view: starts the game's celebration (and confetti, win only) when opening finishes, stops both on close.
    public sealed class ResultCelebrationDriver : MonoBehaviour
    {
        IResultCelebration game; ConfettiView confetti; PopupView view;
        public RectTransform Stage { get; private set; }
        public ConfettiView Confetti=>confetti;
        public bool Played { get; private set; }
        public bool Stopped { get; private set; }
        public static bool ShouldPlay(float progress,float openSeconds,bool closing)=>!closing&&progress>=openSeconds;
        public static ResultCelebrationDriver Attach(PopupView view,IResultCelebration game,bool withConfetti)
        {
            var layout=ResultCelebrationLayout.Standard;
            var stage=new GameObject("Result celebration stage",typeof(RectTransform)).GetComponent<RectTransform>();
            stage.SetParent(view.Panel,false);
            stage.anchorMin=new Vector2(0,1); stage.anchorMax=Vector2.one; stage.pivot=new Vector2(.5f,0);
            stage.sizeDelta=new Vector2(0,layout.StageHeight); stage.anchoredPosition=Vector2.zero;
            var driver=view.gameObject.AddComponent<ResultCelebrationDriver>();
            driver.view=view; driver.game=game; driver.Stage=stage;
            if(withConfetti) driver.confetti=ConfettiView.Create((RectTransform)view.transform,ConfettiConfig.Default,layout.ConfettiOriginHeightFraction,System.Environment.TickCount);
            return driver;
        }
        void Update()=>Evaluate(view.Progress,view.Closing);
        public void Evaluate(float progress,bool closing)
        {
            if(Stopped) return;
            if(closing) { Stop(); return; }
            if(Played||!ShouldPlay(progress,view.Motion.OpenSeconds,closing)) return;
            Played=true; if(confetti!=null) confetti.Play();
            if(game!=null) game.Play(Stage);
        }
        public void Stop()
        {
            if(Stopped) return;
            Stopped=true; if(confetti!=null) confetti.Stop();
            if(Played&&game!=null) game.Stop();
        }
        void OnDisable()=>Stop();
    }
}
