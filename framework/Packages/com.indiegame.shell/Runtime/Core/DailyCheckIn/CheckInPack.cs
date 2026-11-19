using System;
using System.Collections.Generic;

namespace IndieGame.Shell
{
    public static class CheckInPack
    {
        public const int Days = 7;
        public const int FinalDayGoldMultiplier = 10;
        public const int FinalDayHearts = 1;
        const int BoosterDayOne = 2, BoosterDayTwo = 4;

        public static DailyCheckInConfig Standard(int baseGold, IReadOnlyList<string> boosterIds = null)
        {
            if (baseGold < 0) throw new ArgumentOutOfRangeException(nameof(baseGold));
            var row = new Reward[Days];
            for (int i = 0; i < Days - 1; i++) row[i] = new Reward((long)baseGold * (i + 1));
            row[Days - 1] = new Reward((long)baseGold * FinalDayGoldMultiplier, FinalDayHearts);
            if (boosterIds != null && boosterIds.Count > 0)
            {
                row[BoosterDayOne] = RewardBoosters.One(RewardBoosters.At(boosterIds, 0), (long)baseGold * (BoosterDayOne + 1));
                row[BoosterDayTwo] = RewardBoosters.One(RewardBoosters.At(boosterIds, 1), (long)baseGold * (BoosterDayTwo + 1));
                row[Days - 1] = RewardBoosters.All(boosterIds, (long)baseGold * FinalDayGoldMultiplier, FinalDayHearts);
            }
            return new DailyCheckInConfig(new[] { row, row });
        }
    }
}
