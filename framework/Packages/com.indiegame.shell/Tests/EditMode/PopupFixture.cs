using System;
using IndieGame.Shell.Unity.Screens;
using IndieGame.Shell.Unity.UI;
using UnityEngine;
namespace IndieGame.Shell.Tests
{
    public sealed class PopupGame : IGameSessionActions
    {
        public int Revives,Retries,Nexts,Leaves,Pauses,Resumes;
        public void Play() {} public void Retry()=>Retries++; public void Revive()=>Revives++; public void NextLevel()=>Nexts++;
        public void Leave()=>Leaves++; public void Pause()=>Pauses++; public void Resume()=>Resumes++;
    }
    public sealed class PopupFixture : IDisposable
    {
        readonly DemoStyles Styles=new DemoStyles(); public readonly GameObject Root;
        public readonly ShellUiFactory Factory; public readonly PopupHost Host; public readonly ScreenFiles Files=new ScreenFiles();
        public readonly ShellSession Session; public readonly FakeRewardedAdService Ads=new FakeRewardedAdService();
        public readonly PopupGame Game=new PopupGame(); public readonly SessionPopupPresenter Presenter;
        public PopupFixture()
        {
            Root=new GameObject("UI",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.GraphicRaycaster));
            Factory=new ShellUiFactory(Styles.Make(false)); Host=Root.AddComponent<PopupHost>();
            Session=ScreenTestData.Session(new ScreenClock(),Files); Session.LoadAll();
            Presenter=new SessionPopupPresenter(Session,Factory,Host,ScreenTestData.Text(),Game,Ads,new SessionPopupConfig(new Reward(12),20,"revive","double"));
        }
        public void Dispose() { Presenter.Dispose(); UnityEngine.Object.DestroyImmediate(Root); Styles.Dispose(); Time.timeScale=1; }
    }
}
