using System.Collections.Generic;

namespace IndieGame.Shell
{
    public sealed class Reward
    {
        public long Gold { get; }
        public int Hearts { get; }
        public IReadOnlyDictionary<string, int> Boosters { get; }

        public Reward(long gold = 0, int hearts = 0, IReadOnlyDictionary<string, int>? boosters = null)
        {
            Gold = gold;
            Hearts = hearts;
            Boosters = boosters ?? new Dictionary<string, int>();
        }
    }

    public enum GrantOutcome { Granted, AlreadyClaimed, Rejected }

    public interface IRewardGrantor
    {
        GrantOutcome Grant(string claimId, Reward reward);
    }
}
