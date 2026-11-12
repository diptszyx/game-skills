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
    public sealed class ShellAppConfig
    {
        public ShellStyle Style;
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
        public ILeaderboardService Leaderboard;
        public string BoardId;
        public int LeaderboardRows = 10;
        public Func<ShellGameContext,RectTransform> GameScreenFactory;
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
