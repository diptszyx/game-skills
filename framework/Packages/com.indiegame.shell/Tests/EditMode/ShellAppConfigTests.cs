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
        ShellAppConfig Standard(ShellGameConfig game=null)
        {
            var existing=Config();
            return ShellAppConfig.Standard(game??ShellGameConfig.Parse("{\"gameId\":\"taporder\"}"),
                existing.Style,new Dictionary<string,TextAsset>(existing.LanguageTables),
                existing.Actions,existing.GameScreenFactory,existing.SavePath);
        }
        [Test] public void StandardConfigPassesShellAppConfigValidation()
        {
            var c=Standard(); Assert.DoesNotThrow(c.Validate);
            Assert.That(c.RewardedAds,Is.TypeOf<FakeRewardedAdService>());
            Assert.That(c.Purchases,Is.TypeOf<FakePurchaseService>());
            Assert.That(c.Leaderboard,Is.TypeOf<LeaderboardRequests>());
            Assert.That(c.Sounds,Is.Empty);
            for(int i=0;i<20;i++)
                c.RewardedAds.Show(ShellDefaults.RevivePlacement,result=>Assert.That(result.Outcome,Is.EqualTo(AdOutcome.Completed)));
            Assert.That(c.RemoteConfig.Defaults.RewardGold,Is.EqualTo(1));
            Assert.That(c.RemoteConfig.Defaults.RefreshSeconds,Is.EqualTo(60));
            Assert.That(c.RemoteConfig.MaxRewardGold,Is.EqualTo(1000));
            Assert.That(c.RemoteConfig.MinRefreshSeconds,Is.EqualTo(1));
            Assert.That(c.RemoteConfig.MaxRefreshSeconds,Is.EqualTo(3600));
            Assert.That(c.SessionPopups.RevivePlacement,Is.EqualTo(ShellDefaults.RevivePlacement));
            Assert.That(c.SessionPopups.DoublePlacement,Is.EqualTo(ShellDefaults.DoublePlacement));
            c.RewardedAds=new FakeRewardedAdService(); c.Leaderboard=new AsyncLeaderboard();
            Assert.DoesNotThrow(c.Validate);
        }
        [Test] public void StandardConfigUsesGameConfigValues()
        {
            var g=new ShellGameConfig { gameId="custom",initialGold=111,maxHearts=8,initialHearts=3,
                heartSeconds=42,checkInGold=12,questGold=13,winGold=14,reviveGold=15,
                shopGold=16,shopPrice=17,languages=new[]{"vi"} };
            var c=Standard(g); Assert.DoesNotThrow(c.Validate);
            Assert.That(c.BoardId,Is.EqualTo(g.gameId));
            Assert.That(c.Lives.MaxHearts,Is.EqualTo(8)); Assert.That(c.Lives.InitialHearts,Is.EqualTo(3));
            Assert.That(c.Lives.RegenInterval,Is.EqualTo(TimeSpan.FromSeconds(42)));
            Assert.That(c.Economy.InitialGold,Is.EqualTo(111));
            Assert.That(c.CheckIn.RewardTables.Count,Is.EqualTo(2));
            foreach(var row in c.CheckIn.RewardTables) {
                Assert.That(row.Count,Is.EqualTo(5));
                foreach(var reward in row) Assert.That(reward.Gold,Is.EqualTo(12));
            }
            Assert.That(c.Quests.DailyCount,Is.EqualTo(1)); Assert.That(c.Quests.Daily.Count,Is.EqualTo(1));
            Assert.That(c.Quests.Daily[0].Id,Is.EqualTo("q"));
            Assert.That(c.Quests.Daily[0].EventKey,Is.EqualTo("win"));
            Assert.That(c.Quests.Daily[0].Target,Is.EqualTo(1));
            Assert.That(c.Quests.Daily[0].Reward.Gold,Is.EqualTo(13));
            Assert.That(c.SessionPopups.WinReward.Gold,Is.EqualTo(14));
            Assert.That(c.SessionPopups.ReviveGold,Is.EqualTo(15));
            Assert.That(c.ShopCatalog.Products.Count,Is.EqualTo(1));
            var product=c.ShopCatalog.Products[0];
            Assert.That(product.Id,Is.EqualTo("p")); Assert.That(product.PriceKind,Is.EqualTo(PriceKind.Gold));
            Assert.That(product.Price,Is.EqualTo(17)); Assert.That(product.Reward.Gold,Is.EqualTo(16));
            Assert.That(c.Settings.DefaultLanguage,Is.EqualTo("vi"));
            Assert.That(c.Settings.Languages,Is.EquivalentTo(g.languages));
            Assert.That(c.Settings.Sound && c.Settings.Music && c.Settings.Haptics,Is.True);
        }
        [Test] public void ParseRejectsBlankGameIdAndBadHearts()
        {
            foreach(var json in new[]{"{}", "{\"gameId\":\" \"}"})
                Assert.That(Assert.Throws<InvalidOperationException>(()=>ShellGameConfig.Parse(json)).Message,Does.Contain("gameId"));
            foreach(var field in new[]{"initialGold","maxHearts","initialHearts","heartSeconds","checkInGold","questGold","winGold","reviveGold","shopGold","shopPrice"})
                Assert.That(Assert.Throws<InvalidOperationException>(()=>ShellGameConfig.Parse("{\"gameId\":\"test\",\""+field+"\":-1}")).Message,Does.Contain(field));
            Assert.That(Assert.Throws<InvalidOperationException>(()=>ShellGameConfig.Parse("{\"gameId\":\"test\",\"maxHearts\":0}")).Message,Does.Contain("maxHearts"));
            Assert.That(Assert.Throws<InvalidOperationException>(()=>ShellGameConfig.Parse("{\"gameId\":\"test\",\"initialHearts\":6}")).Message,Does.Contain("initialHearts"));
            foreach(var value in new[]{"[]","null","[\"\"]"})
                Assert.That(Assert.Throws<InvalidOperationException>(()=>ShellGameConfig.Parse("{\"gameId\":\"test\",\"languages\":"+value+"}")).Message,Does.Contain("languages"));
        }
        [Test] public void MinimalJsonUsesDefaults()
        {
            var g=ShellGameConfig.Parse("{\"gameId\":\"minimal\"}");
            Assert.That(g.gameId,Is.EqualTo("minimal"));
            Assert.That(new[]{g.initialGold,g.maxHearts,g.initialHearts,g.heartSeconds,g.checkInGold,
                g.questGold,g.winGold,g.reviveGold,g.shopGold,g.shopPrice},
                Is.EqualTo(new[]{100,5,5,1800,25,10,20,50,50,30}));
            Assert.That(g.languages,Is.EqualTo(new[]{"en","vi"}));
        }
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
