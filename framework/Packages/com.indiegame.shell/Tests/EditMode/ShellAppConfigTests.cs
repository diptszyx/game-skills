using System;
using System.Collections.Generic;
using System.Linq;
using IndieGame.Shell.Unity;
using IndieGame.Shell.Unity.Screens;
using NUnit.Framework;
using UnityEngine;
namespace IndieGame.Shell.Tests
{
    public sealed class ShellAppConfigTests
    {
        DemoStyles demos;
        sealed class Game : IGameSessionActions { public void Play() {} public void Retry() {} public void Revive() {} public void NextLevel() {} public void Leave() {} public void Pause() {} public void Resume() {} }
        ShellAppConfig Config()=>new ShellAppConfig {
            Style=demos.Make(false), SavePath="test.json", Lives=new LivesConfig(5,2,TimeSpan.FromSeconds(60)), Economy=new EconomyConfig(100),
            CheckIn=new DailyCheckInConfig(new[]{Enumerable.Repeat(new Reward(1),5).ToArray(),Enumerable.Repeat(new Reward(1),5).ToArray()}), Quests=new DailyQuestsConfig(Array.Empty<QuestTemplate>(),0),
            Settings=new SettingsConfig(true,true,true,"en",new[]{"en","vi"}), RemoteConfig=new RemoteConfigRules(new RemoteConfig(1,60),1000,1,3600),
            SessionPopups=new SessionPopupConfig(new Reward(1),1,"revive","double"), Actions=new Game(),
            LanguageTables=new Dictionary<string,TextAsset>{{"en",Resources.Load<TextAsset>("ShellLocalization/en")},{"vi",Resources.Load<TextAsset>("ShellLocalization/vi")}},
            GameScreenFactory=c=>c.Factory.ScreenRoot(c.Parent,"Game") };
        [SetUp] public void Setup()=>demos=new DemoStyles();
        [TearDown] public void Cleanup()=>demos.Dispose();
        [Test] public void MissingRequiredConfigNamesEveryPiece()
        {
            foreach(var name in new[]{"Style","SavePath","Lives","Economy","CheckIn","Quests","Settings","RemoteConfig","SessionPopups","Actions","LanguageTables","GameScreenFactory"})
            {
                var c=Config(); typeof(ShellAppConfig).GetField(name).SetValue(c,null);
                Assert.That(Assert.Throws<InvalidOperationException>(c.Validate).Message,Does.Contain(name));
            }
        }
        [Test] public void MissingStyleRolesAreNamed()
        {
            foreach(var name in new[]{"ScreenBackground","CurrencyIcon","HeartIcon","BoosterIcon","ListRowSprite","LockedButtonSprite","Theme"})
            {
                var c=Config(); typeof(IndieGame.Shell.Unity.UI.ShellStyle).GetField(name).SetValue(c.Style,null);
                Assert.That(Assert.Throws<InvalidOperationException>(c.Validate).Message,Does.Contain(name));
            }
        }
        [Test] public void MissingAndInvalidLanguageTablesAreNamed()
        {
            var c=Config(); c.LanguageTables.Remove("vi"); Assert.That(Assert.Throws<InvalidOperationException>(c.Validate).Message,Does.Contain("vi"));
            var invalid=new TextAsset("invalid"); c.LanguageTables["vi"]=invalid;
            var root=new GameObject("App");
            try { var result=root.AddComponent<ShellApp>().Boot(c); Assert.That(result.Success,Is.False); Assert.That(result.Error,Does.Contain("vi")); Assert.That(root.GetComponentsInChildren<Canvas>(true),Is.Empty); }
            finally { UnityEngine.Object.DestroyImmediate(root); UnityEngine.Object.DestroyImmediate(invalid); }
        }
        [Test] public void OptionalServicesAndUnknownScreenThrowNamedErrors()
        {
            var root=new GameObject("App");
            try
            {
                var app=root.AddComponent<ShellApp>();
                Assert.That(Assert.Throws<InvalidOperationException>(()=>app.Navigate(ShellScreen.Shop)).Message,Does.Contain("Shop"));
                Assert.That(Assert.Throws<InvalidOperationException>(()=>app.Navigate(ShellScreen.Leaderboard)).Message,Does.Contain("Leaderboard"));
                Assert.That(Assert.Throws<InvalidOperationException>(()=>app.Navigate((ShellScreen)999)).Message,Does.Contain("999"));
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
        sealed class AsyncLeaderboard : ILeaderboardRequests
        {
            public System.Threading.Tasks.Task<ServiceResult<bool>> SubmitScore(string board,long score)=>System.Threading.Tasks.Task.FromResult(new ServiceResult<bool>(true));
            public System.Threading.Tasks.Task<ServiceResult<IReadOnlyList<LeaderboardEntry>>> GetTop(string board,int count)=>System.Threading.Tasks.Task.FromResult(new ServiceResult<IReadOnlyList<LeaderboardEntry>>(Array.Empty<LeaderboardEntry>()));
            public System.Threading.Tasks.Task<ServiceResult<LeaderboardEntry>> GetMyRank(string board)=>System.Threading.Tasks.Task.FromResult(new ServiceResult<LeaderboardEntry>(null,ServiceError.NotFound));
        }
        [Test] public void ShellAppConfigAcceptsAsyncLeaderboardRequests()
        {
            var c=Config(); var requests=new AsyncLeaderboard(); c.Leaderboard=requests; c.BoardId="board";
            Assert.DoesNotThrow(c.Validate);
            Assert.That(c.Leaderboard,Is.SameAs(requests));
        }
        [Test] public void PartialOptionalConfigIsNamed()
        {
            var c=Config(); c.ShopCatalog=ScreenTestData.Catalog(); Assert.That(Assert.Throws<InvalidOperationException>(c.Validate).Message,Does.Contain("Purchases"));
            c=Config(); c.Leaderboard=new LeaderboardRequests(new FakeLeaderboardService("me",new FakeLeaderboardState())); Assert.That(Assert.Throws<InvalidOperationException>(c.Validate).Message,Does.Contain("BoardId"));
            c=Config(); c.Clock=null; Assert.DoesNotThrow(c.Validate);
        }
    }
}
