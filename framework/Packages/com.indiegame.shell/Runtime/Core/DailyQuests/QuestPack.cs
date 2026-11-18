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

        public static DailyQuestsConfig Standard(int baseGold)
        {
            if (baseGold < 0) throw new ArgumentOutOfRangeException(nameof(baseGold));
            var daily = new List<QuestTemplate>
            {
                Template("win1", QuestEvents.Win, 1, baseGold),
                Template("win3", QuestEvents.Win, 3, baseGold * 2),
                Template("win5", QuestEvents.Win, 5, baseGold * 3),
                Template("play3", QuestEvents.Play, 3, baseGold),
                Template("play5", QuestEvents.Play, 5, baseGold * 2),
            };
            return new DailyQuestsConfig(daily, DailyPicked);
        }

        static QuestTemplate Template(string id, string eventKey, int target, int gold) =>
            new QuestTemplate(id, eventKey, target, new Reward(gold));
    }
}
