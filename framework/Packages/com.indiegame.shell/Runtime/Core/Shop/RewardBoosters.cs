using System.Collections.Generic;

namespace IndieGame.Shell
{
    public static class RewardBoosters
    {
        public static Reward One(string id, long gold = 0, int hearts = 0) =>
            new Reward(gold, hearts, new Dictionary<string, int> { { id, 1 } });
        public static Reward All(IReadOnlyList<string> ids, long gold = 0, int hearts = 0)
        {
            var boosters = new Dictionary<string, int>();
            foreach (var id in ids) boosters[id] = 1;
            return new Reward(gold, hearts, boosters);
        }
        public static string At(IReadOnlyList<string> ids, int index) => ids[index % ids.Count];
    }
}
