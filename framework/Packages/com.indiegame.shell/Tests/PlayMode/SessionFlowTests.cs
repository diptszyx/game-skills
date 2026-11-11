using System.Collections;
using IndieGame.Shell.Unity;
using IndieGame.Shell.Unity.Screens;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace IndieGame.Shell.Tests
{
    public sealed class SessionFlowTests
    {
        [UnityTest] public IEnumerator LoadingBlocksInputUntilHidden()
        {
            using(var f=new PopupFixture())
            {
                var root=f.Factory.ScreenRoot(f.Root.transform); var loading=root.gameObject.AddComponent<LoadingScreenView>();
                var content=root.gameObject.AddComponent<LoadingContentView>(); content.Initialize(f.Factory,loading,new ScreenText(f.Session,ScreenTestData.Text()),"shell.loading.tip");
                loading.Show(); loading.Hide(); Assert.That(root.GetComponent<CanvasGroup>().blocksRaycasts,Is.True);
                yield return new WaitForSecondsRealtime(.3f); Assert.That(root.GetComponent<CanvasGroup>().blocksRaycasts,Is.True);
                yield return new WaitForSecondsRealtime(.5f); Assert.That(loading.Controller.State,Is.EqualTo(LoadingScreenState.Hidden));
                Assert.That(root.GetComponent<CanvasGroup>().blocksRaycasts,Is.False); Assert.That(root.gameObject.activeSelf,Is.True);
            }
        }
        [UnityTest] public IEnumerator LoadingProgressIsRendered()
        {
            using(var f=new PopupFixture())
            {
                var root=f.Factory.ScreenRoot(f.Root.transform); var loading=root.gameObject.AddComponent<LoadingScreenView>();
                var content=root.gameObject.AddComponent<LoadingContentView>(); content.Initialize(f.Factory,loading,new ScreenText(f.Session,ScreenTestData.Text()),"shell.loading.tip");
                loading.Show(); loading.SetProgress(.65f); yield return null;
                Assert.That(content.RenderedProgress,Is.EqualTo(.65f));
                Assert.That(((RectTransform)root.Find("Loading Progress/Fill")).anchorMax.x,Is.EqualTo(.65f));
            }
        }
        [UnityTest] public IEnumerator AllFivePopupFlows()
        {
            using(var f=new PopupFixture())
            {
                f.Presenter.ShowWin("r"); Assert.That(f.Session.Economy.Gold,Is.EqualTo(112));
                f.Ads.Enqueue(AdOutcome.Completed); f.Presenter.Act(SessionPopupKind.Win,"double"); Assert.That(f.Session.Economy.Gold,Is.EqualTo(124));
                f.Presenter.Act(SessionPopupKind.Win,"next"); yield return new WaitForSecondsRealtime(.15f); Assert.That(f.Game.Nexts,Is.EqualTo(1));
                f.Presenter.ShowLose(); f.Presenter.Act(SessionPopupKind.Lose,"retry"); yield return new WaitForSecondsRealtime(.15f); Assert.That(f.Game.Retries,Is.EqualTo(1));
                f.Presenter.ShowLose(); f.Presenter.Act(SessionPopupKind.Lose,"revive"); f.Ads.Enqueue(AdOutcome.Completed); f.Presenter.Act(SessionPopupKind.Revive,"ad");
                yield return new WaitForSecondsRealtime(.15f); Assert.That(f.Game.Revives,Is.EqualTo(1));
                f.Presenter.ShowPause(); f.Presenter.Act(SessionPopupKind.Pause,"sound"); Assert.That(f.Session.Settings.Sound,Is.False);
                f.Presenter.Act(SessionPopupKind.Pause,"leave"); f.Presenter.Act(SessionPopupKind.Leave,"confirm"); yield return new WaitForSecondsRealtime(.15f);
                Assert.That(f.Game.Leaves,Is.EqualTo(1)); Assert.That(f.Game.Resumes,Is.EqualTo(1)); Assert.That(f.Host.Stack.Count,Is.Zero);
            }
        }
        [UnityTest] public IEnumerator LeaveCancelKeepsPause()
        {
            using(var f=new PopupFixture())
            {
                Time.timeScale=.4f; f.Presenter.ShowPause(); f.Presenter.ShowLeave(); f.Presenter.Act(SessionPopupKind.Leave,"cancel");
                yield return new WaitForSecondsRealtime(.15f); Assert.That(Time.timeScale,Is.Zero); Assert.That(f.Game.Resumes,Is.Zero);
                Assert.That(f.Host.Top,Is.SameAs(f.Presenter.Popups[SessionPopupKind.Pause].View));
                f.Host.Back(); yield return new WaitForSecondsRealtime(.15f); Assert.That(Time.timeScale,Is.EqualTo(.4f));
            }
        }
        [UnityTest] public IEnumerator EveryPauseExitRestoresPriorState()
        {
            foreach(var exit in new[]{"resume","back","scrim","disable","host","dispose"})
            using(var f=new PopupFixture())
            {
                Time.timeScale=.35f; f.Presenter.ShowPause(); var p=f.Presenter.Popups[SessionPopupKind.Pause].View;
                if(exit=="resume") f.Presenter.Act(SessionPopupKind.Pause,"resume");
                else if(exit=="back") f.Host.Back(); else if(exit=="scrim") p.GetComponent<Button>().onClick.Invoke();
                else if(exit=="disable") p.gameObject.SetActive(false); else if(exit=="host") f.Host.enabled=false; else f.Presenter.Dispose();
                yield return new WaitForSecondsRealtime(.15f);
                Assert.That(Time.timeScale,Is.EqualTo(.35f),exit); Assert.That(f.Game.Resumes,Is.EqualTo(1),exit);
            }
        }
        [UnityTest] public IEnumerator MandatoryPopupsRejectBack()
        {
            foreach(var kind in new[]{SessionPopupKind.Win,SessionPopupKind.Lose,SessionPopupKind.Revive}) using(var f=new PopupFixture())
            {
                if(kind==SessionPopupKind.Win) f.Presenter.ShowWin("r"); else if(kind==SessionPopupKind.Lose) f.Presenter.ShowLose(); else f.Presenter.ShowRevive();
                var p=f.Host.Top; Assert.That(f.Host.Back(),Is.False); Assert.That(p.RequestDismiss(),Is.False); p.GetComponent<Button>().onClick.Invoke();
                yield return new WaitForSecondsRealtime(.15f); Assert.That(f.Host.Top,Is.SameAs(p)); Assert.That(p.gameObject.activeSelf,Is.True);
            }
        }
    }
}
