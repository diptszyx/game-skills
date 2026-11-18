using IndieGame.Shell.Unity.Localization;
using IndieGame.Shell.Unity.Screens;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
namespace IndieGame.Shell.Tests
{
    public sealed class MainSettingsP3Tests
    {
        static SessionPopupPresenter Presenter(PopupFixture f)=>
            new SessionPopupPresenter(f.Session,f.Factory,f.Host,SampleLocalization.Load(),f.Game,f.Ads,new SessionPopupConfig(new Reward(12),20,"revive","double"));

        [Test] public void GearIsVisibleOnHomeAndSitsRightOfBothPills()
        {
            using(var f=new PopupFixture())
            {
                var top=new TopBarView(f.Factory,f.Root.transform);
                Assert.That(top.Settings.gameObject.activeSelf,Is.True);
                var gear=(RectTransform)top.Settings.transform;
                foreach(var pill in new[]{"Currency pill","Lives pill"})
                    Assert.That(top.Root.Find(pill).GetComponent<RectTransform>().anchorMax.x,Is.LessThan(gear.anchorMin.x),pill);
                Assert.That(gear.anchorMax.x,Is.LessThanOrEqualTo(1f));
            }
        }
        [Test] public void SettingsPopupHasTogglesAndLanguageAndDoesNotPauseTheGame()
        {
            using(var f=new PopupFixture()) using(var presenter=Presenter(f))
            {
                presenter.ShowSettings();
                var p=presenter.Popups[SessionPopupKind.Settings];
                foreach(var key in new[]{"sound","music","haptics","language"}) Assert.That(p.Buttons.ContainsKey(key),Is.True,key);
                Assert.That(p.Buttons.ContainsKey("leave"),Is.False); Assert.That(f.Game.Pauses,Is.Zero);
                presenter.Act(SessionPopupKind.Settings,"sound"); Assert.That(f.Session.Settings.Sound,Is.False);
                presenter.Act(SessionPopupKind.Settings,"haptics"); Assert.That(f.Session.Settings.Haptics,Is.False);
            }
        }
        [Test] public void LanguageButtonCyclesLanguageAndRelocalizesThePopupAndPersists()
        {
            using(var f=new PopupFixture()) using(var presenter=Presenter(f))
            {
                presenter.ShowSettings();
                var p=presenter.Popups[SessionPopupKind.Settings];
                Assert.That(f.Session.Settings.Language,Is.EqualTo("en"));
                Assert.That(p.View.Panel.Find("Title").GetComponent<TMP_Text>().text,Is.EqualTo("Settings"));
                int writes=f.Files.Writes;
                presenter.Act(SessionPopupKind.Settings,"language");
                Assert.That(f.Session.Settings.Language,Is.EqualTo("vi"));
                Assert.That(p.View.Panel.Find("Title").GetComponent<TMP_Text>().text,Is.EqualTo("Cài đặt"));
                Assert.That(p.Buttons["language"].GetComponent<ActionAccessibility>().Label,Does.Contain("Tiếng Việt"));
                Assert.That(f.Files.Writes,Is.GreaterThan(writes));
                presenter.Act(SessionPopupKind.Settings,"language"); Assert.That(f.Session.Settings.Language,Is.EqualTo("en"));
            }
        }
        [Test] public void PausePopupSharesTheSameSettingsControls()
        {
            using(var f=new PopupFixture()) using(var presenter=Presenter(f))
            {
                presenter.ShowPause(); presenter.Act(SessionPopupKind.Pause,"music");
                Assert.That(f.Session.Settings.Music,Is.False);
            }
        }
        [Test] public void GameSectionRoundTripsThroughCommitAndLoad()
        {
            var clock=new ScreenClock(); var files=new ScreenFiles(); var a=ScreenTestData.Session(clock,files); a.LoadAll();
            Assert.That(a.WriteGameSection("g",new System.Collections.Generic.Dictionary<string,int>{{"best",7}}).Success,Is.True);
            Assert.That(a.Commit().Success,Is.True);
            var b=ScreenTestData.Session(clock,files); b.LoadAll();
            Assert.That(b.ReadGameSection("g",()=>new System.Collections.Generic.Dictionary<string,int>()).Value["best"],Is.EqualTo(7));
        }
    }
}
