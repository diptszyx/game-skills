using System;

namespace IndieGame.Shell
{
    public static class CheckInPack
    {
        public const int Days = 7;
        public const int FinalDayGoldMultiplier = 10;
        public const int FinalDayHearts = 1;

        public static DailyCheckInConfig Standard(int baseGold)
        {
            if (baseGold < 0) throw new ArgumentOutOfRangeException(nameof(baseGold));
            var row = new Reward[Days];
            for (int i = 0; i < Days - 1; i++) row[i] = new Reward((long)baseGold * (i + 1));
            row[Days - 1] = new Reward((long)baseGold * FinalDayGoldMultiplier, FinalDayHearts);
            return new DailyCheckInConfig(new[] { row, row });
        }
    }
}
