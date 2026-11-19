using System;
using System.Collections.Generic;

namespace IndieGame.Shell
{
    public static class QuestEvents
    {
        public const string Win = "win";
        public const string Play = "play";
    }

    public static class QuestPack
    {
        public const int DailyPicked = 3;

        public static DailyQuestsConfig Standard(int baseGold, IReadOnlyList<string> boosterIds = null)
        {
            if (baseGold < 0) throw new ArgumentOutOfRangeException(nameof(baseGold));
            var daily = new List<QuestTemplate>
            {
                Template("win1", QuestEvents.Win, 1, baseGold),
                Template("win3", QuestEvents.Win, 3, baseGold * 2, Bonus(boosterIds, 0)),
                Template("win5", QuestEvents.Win, 5, baseGold * 3, Bonus(boosterIds, 1)),
                Template("play3", QuestEvents.Play, 3, baseGold),
                Template("play5", QuestEvents.Play, 5, baseGold * 2),
            };
            return new DailyQuestsConfig(daily, DailyPicked);
        }

        static string Bonus(IReadOnlyList<string> boosterIds, int index) =>
            boosterIds == null || boosterIds.Count == 0 ? null : RewardBoosters.At(boosterIds, index);

        static QuestTemplate Template(string id, string eventKey, int target, int gold, string boosterId = null) =>
            new QuestTemplate(id, eventKey, target, boosterId == null ? new Reward(gold) : RewardBoosters.One(boosterId, gold));
    }
}
