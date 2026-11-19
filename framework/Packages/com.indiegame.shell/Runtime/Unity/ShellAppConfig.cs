using System;
using System.Collections.Generic;
using IndieGame.Shell.Unity.Screens;
using IndieGame.Shell.Unity.UI;
using UnityEngine;
namespace IndieGame.Shell.Unity
{
    public sealed class BootResult
    {
        public bool Success { get; }
        public string Error { get; }
        internal BootResult(bool success, string error = null) { Success=success; Error=error; }
    }
    public sealed class ShellGameContext
    {
        public ShellUiFactory Factory { get; internal set; }
        public ShellSession Session { get; internal set; }
        public LocalizationService Text { get; internal set; }
        public PopupHost Popups { get; internal set; }
        public SessionPopupPresenter Flow { get; internal set; }
        public RectTransform Parent { get; internal set; }
        public Action<ShellScreen> Navigate { get; internal set; }
        public Action ShowMain { get; internal set; }
    }
    public sealed class HomeStageContext
    {
        public ShellUiFactory Factory { get; internal set; }
        public ShellStyle Style { get; internal set; }
        public RectTransform Parent { get; internal set; }
    }
    public sealed class ShellAppConfig
    {
        public ShellStyle Style;
        public Func<string> PlayLabel;
        public IDictionary<string,TextAsset> LanguageTables;
        public string SavePath;
        // ShellSession's existing injectable storage seam is SaveStore(ISaveFileSystem).
        public SaveStore SaveStore;
        public IClock Clock = new SystemClock();
        public LivesConfig Lives;
        public EconomyConfig Economy;
        public DailyCheckInConfig CheckIn;
        public DailyQuestsConfig Quests;
        public SettingsConfig Settings;
        public RemoteConfigRules RemoteConfig;
        public SessionPopupConfig SessionPopups;
        public IGameSessionActions Actions;
        public IRewardedAdService RewardedAds;
        public Func<ShellSession,ShopService> ShopFactory;
        public ProductCatalog ShopCatalog;
        public IPurchaseService Purchases;
        public ILeaderboardRequests Leaderboard;
        public string BoardId;
        // Optional. Own best score shown as an unranked row when the board has no entry for the player or is unreachable.
        public Func<long> LeaderboardLocalBest;
        public int LeaderboardRows = 10;
        // Optional. Localization key or plain text for the one-line caption under the leaderboard (e.g. what the score counts); null hides it.
        public string LeaderboardCaption;
        // Optional. Sprite per booster id for reward strips; ids without a sprite show plain text.
        public IReadOnlyDictionary<string,Sprite> BoosterIcons = new Dictionary<string,Sprite>();
        public IDictionary<string,AudioClip> Sounds;
        public Func<ShellGameContext,RectTransform> GameScreenFactory;
        // Optional. Game extra on the win popup (e.g. a mascot); the shell always adds confetti.
        public IResultCelebration WinCelebration;
        // Optional. Game extra on the lose popup (e.g. a sad mascot); no confetti.
        public IResultCelebration LoseCelebration;
        // Optional. Builds game decoration for Home; see HomeStageBuilder for the placement and fit contract.
        public Func<HomeStageContext,RectTransform> HomeStageFactory;
        const int StandardFakePurchases=50;
        public static ShellAppConfig Standard(ShellGameConfig game, ShellStyle style,
            IReadOnlyDictionary<string,TextAsset> languageTables, IGameSessionActions actions,
            Func<ShellGameContext,RectTransform> gameScreen, string savePath, DailyQuestsConfig quests = null, ProductCatalog shop = null, DailyCheckInConfig checkIn = null)
        {
            Require(game,nameof(game)); game.Validate();
            // Existing service constructors require these durations/prices to be positive.
            if(game.heartSeconds == 0 || game.reviveGold == 0 || game.shopPrice == 0 || (shop==null && game.shopGold == 0))
                throw new InvalidOperationException("Standard requires positive heartSeconds, reviveGold, shopPrice and (without a shop override) shopGold");
            Require(languageTables,nameof(languageTables));
            var ads = new FakeRewardedAdService();
            for(int i=0;i<20;i++) ads.Enqueue(AdOutcome.Completed);
            var purchases = new FakePurchaseService();
            for(int i=0;i<StandardFakePurchases;i++) purchases.Enqueue(PurchaseOutcome.Success,Guid.NewGuid().ToString("N"));
            var tables = new Dictionary<string,TextAsset>();
            foreach(var table in languageTables) tables.Add(table.Key,table.Value);
            var config = new ShellAppConfig {
                Style=style, SavePath=savePath, LanguageTables=tables,
                Lives=new LivesConfig(game.maxHearts,game.initialHearts,TimeSpan.FromSeconds(game.heartSeconds)),
                Economy=new EconomyConfig(game.initialGold,game.InitialBoosterCounts()),
                CheckIn=checkIn??CheckInPack.Standard(game.checkInGold,game.rewardBoosters),
                Quests=quests??QuestPack.Standard(game.questGold,game.rewardBoosters),
                Settings=new SettingsConfig(true,true,true,game.languages[0],game.languages),
                RemoteConfig=new RemoteConfigRules(new RemoteConfig(1,60),1000,1,3600),
                SessionPopups=new SessionPopupConfig(new Reward(game.winGold),game.reviveGold,
                    ShellDefaults.RevivePlacement,ShellDefaults.DoublePlacement),
                Actions=actions, RewardedAds=ads, BoardId=game.gameId,
                ShopCatalog=shop??ShopPack.Standard(game.shopGold,game.shopPrice,game.maxHearts,game.rewardBoosters,game.boosterPrice),
                Purchases=purchases,
                Leaderboard=new LeaderboardRequests(new FakeLeaderboardService("You",new FakeLeaderboardState())),
                Sounds=new Dictionary<string,AudioClip>(), GameScreenFactory=gameScreen
            };
            config.Validate();
            return config;
        }

        internal static void Require(object value,string name)
        { if(value==null) throw new InvalidOperationException("Missing ShellApp config: "+name); }
        public void Validate()
        {
            Require(Style,nameof(Style)); Style.Validate();
            Require(Lives,nameof(Lives)); Require(Economy,nameof(Economy)); Require(CheckIn,nameof(CheckIn));
            Require(Quests,nameof(Quests)); Require(Settings,nameof(Settings)); Require(RemoteConfig,nameof(RemoteConfig));
            Require(SessionPopups,nameof(SessionPopups)); Require(Actions,nameof(Actions)); Require(GameScreenFactory,nameof(GameScreenFactory));
            if(string.IsNullOrWhiteSpace(SavePath)) throw new InvalidOperationException("Missing ShellApp config: SavePath");
            Require(LanguageTables,nameof(LanguageTables));
            foreach(var language in Settings.Languages)
                if(!LanguageTables.TryGetValue(language,out var asset)||asset==null)
                    throw new InvalidOperationException("Missing language table: "+language);
            if(ShopFactory==null && (ShopCatalog!=null || Purchases!=null))
            { Require(ShopCatalog,nameof(ShopCatalog)); Require(Purchases,nameof(Purchases)); }
            if(Leaderboard!=null && (string.IsNullOrWhiteSpace(BoardId)||LeaderboardRows<1))
                throw new InvalidOperationException("Invalid Leaderboard BoardId / LeaderboardRows");
        }
    }
}
