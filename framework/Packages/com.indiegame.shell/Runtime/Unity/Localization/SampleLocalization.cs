using System;
using System.Collections.Generic;
using UnityEngine;
namespace IndieGame.Shell.Unity.Localization
{
    // Sample catalog: applications register their configured product, quest, booster and tip keys too.
    public static class SampleLocalization
    {
        public static readonly IReadOnlyList<string> Keys = Array.AsReadOnly(new[] {
            "en",
            "shell.pause", "shell.back", "shell.home", "shell.close",
            "popup.action.cancel",
            "popup.action.confirm",
            "popup.action.ad",
            "popup.action.double",
            "popup.action.gold",
            "popup.action.haptics",
            "popup.action.leave",
            "popup.action.music",
            "popup.action.next",
            "popup.action.resume",
            "popup.action.retry",
            "popup.action.revive",
            "popup.action.sound",
            "popup.leave.body",
            "popup.leave.title",
            "popup.lose.body",
            "popup.lose.title",
            "popup.pause.body",
            "popup.settings.title", "popup.settings.body", "popup.action.language", "popup.setting.language",
            "shell.language.en", "shell.language.vi", "shell.settings",
            "popup.pause.title",
            "popup.revive.body",
            "popup.revive.price",
            "popup.revive.title",
            "popup.setting.haptics",
            "popup.setting.music",
            "popup.setting.sound",
            "popup.win.body",
            "popup.win.title",
            "shell.booster",
            "shell.booster.hint",
            "shell.buy",
            "shell.checkin.other",
            "shell.checkin.slot",
            "shell.checkin.day",
            "shell.tab.home",
            "shell.checkin.title",
            "shell.claim",
            "shell.error.AlreadyClaimed",
            "shell.error.Cancelled",
            "shell.error.ClockReversed",
            "shell.error.Completed",
            "shell.error.Conflict",
            "shell.error.CorruptSection",
            "shell.error.Failed",
            "shell.error.FutureVersion",
            "shell.error.GrantRejected",
            "shell.error.Incomplete",
            "shell.error.Insufficient",
            "shell.error.InsufficientGold",
            "shell.error.InvalidArgument",
            "shell.error.InvalidConfig",
            "shell.error.InvalidData",
            "shell.error.Io",
            "shell.error.Migration",
            "shell.error.None",
            "shell.error.NotFound",
            "shell.error.NotReady",
            "shell.error.Overflow",
            "shell.error.PurchaseFailed",
            "shell.error.Skipped",
            "shell.error.Unavailable",
            "shell.error.commit",
            "shell.error.title",
            "shell.gold",
            "shell.hearts",
            "shell.leaderboard.empty",
            "shell.leaderboard.loading",
            "shell.leaderboard.offline",
            "shell.leaderboard.rename",
            "shell.leaderboard.rename.hint",
            "shell.leaderboard.rename.invalid",
            "shell.leaderboard.title",
            "shell.leaderboard.you",
            "shell.lives.countdown",
            "shell.lives.full",
            "shell.loading.tip",
            "shell.loading.title",
            "shell.main.title",
            "shell.off",
            "shell.on",
            "shell.play",
            "shell.price.iap",
            "shell.product.p",
            "shell.product.hearts","shell.product.gold_s","shell.product.gold_m","shell.product.gold_l",
            "shell.quest.win1","shell.quest.win3","shell.quest.win5","shell.quest.play3","shell.quest.play5",
            "shell.quest.row",
            "shell.quests.empty",
            "shell.quests.title",
            "shell.refresh",
            "shell.retrySave",
            "shell.reward",
            "shell.reward.unavailable",
            "shell.separator",
            "shell.shop.empty",
            "shell.shop.row",
            "shell.shop.title",
        });
        public static LocalizationService Load()
        {
            var service = new LocalizationService(new LocalizationConfig("en"));
            foreach (var language in new[] { "en", "vi" })
            {
                var asset = Resources.Load<TextAsset>("ShellLocalization/" + language);
                if (asset == null) throw new InvalidOperationException("Missing sample table: " + language);
                var result = service.LoadTable(language, asset.text);
                if (!result.Success) throw new InvalidOperationException("Invalid sample table: " + language + ": " + result.Error);
            }
            return service;
        }
    }
}
