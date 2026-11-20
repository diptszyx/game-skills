using System;
using System.Linq;
using IndieGame.Shell.Unity.Screens;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using UnityEngine.UI;
namespace IndieGame.Shell.Tests
{
    public sealed class HomeCompositionTests
    {
        [Test] public void BadgePredicatesFollowClaims()
        {
            var session=ScreenTestData.Session(new ScreenClock(),new ScreenFiles()); session.LoadAll();
            Assert.That(ChromePresenter.CheckInBadge(session),Is.True); session.DailyCheckIn.Claim(); Assert.That(ChromePresenter.CheckInBadge(session),Is.False);
            Assert.That(ChromePresenter.QuestsBadge(session),Is.False); session.DailyQuests.RecordEvent("event",2);
            Assert.That(ChromePresenter.QuestsBadge(session),Is.True); session.DailyQuests.Claim("q"); Assert.That(ChromePresenter.QuestsBadge(session),Is.False);
        }
        [Test] public void LegacyMainApiAndFourButtonsRemain()
        {
            using(var f=new PopupFixture())
            {
                var view=new MainView(f.Factory,f.Root.transform);
                Assert.That(view.Root.GetComponentsInChildren<Button>().Length,Is.EqualTo(4));
                foreach(var name in new[]{"Root","Lifetime","Snapshot","Error"}) Assert.That(typeof(MainView).GetProperty(name),Is.Not.Null,name);
                foreach(var name in new[]{"PlayRequested","OpenRequested"}) Assert.That(typeof(MainView).GetEvent(name),Is.Not.Null,name);
                Assert.That(typeof(MainView).GetMethod("Render",new[]{typeof(MainDisplay)}),Is.Not.Null);
                Assert.That(typeof(MainView).GetMethod("ShowError",new[]{typeof(string)}),Is.Not.Null);
                int plays=0; ShellScreen? opened=null; view.PlayRequested+=()=>plays++; view.OpenRequested+=s=>opened=s;
                view.RequestPlay(); view.RequestOpen(ShellScreen.Shop); view.ShowError("error");
                Assert.That(plays,Is.EqualTo(1)); Assert.That(opened,Is.EqualTo(ShellScreen.Shop)); Assert.That(view.Error,Is.EqualTo("error"));
            }
        }
        [Test] public void HomeMainContainsExactlyOneButtonAndOptionalArt()
        {
            using(var f=new PopupFixture())
            {
                // The factory is already validated; use optional home art without introducing a test-only style API.
                f.Factory.Style.EnableHomeChrome=true;
                var view=new MainView(f.Factory,f.Root.transform);
                Assert.That(view.Root.GetComponentsInChildren<Button>(true).Length,Is.EqualTo(1));
                Assert.That(view.Root.Find("Home content/Logo").gameObject.activeSelf,Is.False);
                Assert.That(view.Root.Find("Home content/Mascot").gameObject.activeSelf,Is.False);
                Assert.That(view.Root.GetComponentsInChildren<TMPro.TMP_Text>().All(t=>t.text==""),Is.True);
            }
        }
        [Test] public void PlayLabelUsesLocalizationOrGameOverride()
        {
            using(var f=new PopupFixture())
            {
                var view=new MainView(f.Factory,f.Root.transform); var clock=new ScreenClock();
                using(var presenter=new MainPresenter(f.Session,view,ScreenTestData.Text(),clock,f.Game,_=>{})) Assert.That(view.Snapshot.Play,Is.EqualTo("Play"));
                string label="Level 8";
                using(var presenter=new MainPresenter(f.Session,view,ScreenTestData.Text(),clock,f.Game,_=>{},()=>label))
                { Assert.That(view.Snapshot.Play,Is.EqualTo("Level 8")); label="Level 9"; presenter.Refresh(); Assert.That(view.Snapshot.Play,Is.EqualTo("Level 9")); }
            }
        }
        [Test] public void PlayLevelLabelIsLocalizedForBothLanguages()
        {
            var text=ScreenTestData.Text();
            Assert.That(text.Format("en","shell.play.level",8).Value,Is.EqualTo("Level 8"));
            Assert.That(text.Format("vi","shell.play.level",8).Value,Is.EqualTo("Màn 8"));
        }
        [Test] public void CheckInCellsFollowSevenDayCycleAndWrap()
        {
            using(var f=new PopupFixture())
            {
                f.Factory.Style.EnableHomeChrome=true;
                var clock=new ScreenClock(); var session=ScreenTestData.Session(clock,new ScreenFiles()); session.LoadAll();
                var view=new CheckInView(f.Factory,f.Root.transform);
                using(var presenter=new CheckInPresenter(session,view,IndieGame.Shell.Unity.Localization.SampleLocalization.Load(),clock))
                {
                    string[] Roles()=>view.Root.GetComponentsInChildren<ShellAppearance>().Where(a=>a.Role.StartsWith("day")).Select(a=>a.Role).ToArray();
                    for(int day=0;day<CheckInPack.Days;day++)
                    {
                        var roles=Roles(); Assert.That(roles[day],Is.EqualTo("dayToday"));
                        Assert.That(roles.Take(day),Is.All.EqualTo("dayCollected")); Assert.That(roles.Skip(day+1),Is.All.EqualTo("dayLocked"));
                        view.RequestClaim(); Assert.That(Roles().Take(day+1),Is.All.EqualTo("dayCollected"));
                        Assert.That(Roles().Skip(day+1),Is.All.EqualTo("dayLocked"));
                        clock.UtcNow=clock.UtcNow.AddDays(1); presenter.Refresh();
                    }
                    Assert.That(Roles()[0],Is.EqualTo("dayToday")); Assert.That(Roles().Skip(1),Is.All.EqualTo("dayLocked"));
                }
            }
        }
        [Test] public void ShellScreenNumericValuesStayCompatible()
        { Assert.That((int)ShellScreen.CheckIn,Is.Zero); Assert.That((int)ShellScreen.Quests,Is.EqualTo(1)); Assert.That((int)ShellScreen.Shop,Is.EqualTo(2)); Assert.That((int)ShellScreen.Leaderboard,Is.EqualTo(3)); }
    }
}
