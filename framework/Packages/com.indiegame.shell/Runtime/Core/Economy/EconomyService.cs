using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace IndieGame.Shell
{
    public sealed class EconomyConfig
    {
        public long InitialGold { get; }
        public IReadOnlyDictionary<string, int> InitialBoosters { get; }
        public EconomyConfig(long initialGold, IReadOnlyDictionary<string, int>? initialBoosters = null)
        { InitialGold = initialGold; InitialBoosters = initialBoosters ?? new Dictionary<string, int>(); }
    }
    public sealed class ClaimLedger
    {
        readonly HashSet<string> ids;
        internal ClaimLedger(IEnumerable<string> values) { ids = new HashSet<string>(values, StringComparer.Ordinal); }
        public bool Contains(string id) => ids.Contains(id);
        internal void Record(string id) => ids.Add(id);
        internal List<string> Snapshot() => new List<string>(ids);
    }
    public sealed class EconomyService : IRewardGrantor
    {
        readonly long initialGold;
        readonly Dictionary<string, int> initialBoosters;
        readonly LivesService lives;
        Dictionary<string, int> boosters;
        ClaimLedger ledger = new ClaimLedger(Array.Empty<string>());
        public long Gold { get; private set; }
        public int Hearts => lives.Hearts;
        public IReadOnlyDictionary<string, int> Boosters => new ReadOnlyDictionary<string, int>(boosters);
        public ClaimLedger Claims => ledger;
        static bool Valid(IReadOnlyDictionary<string, int> values)
        { foreach (var pair in values) if (string.IsNullOrWhiteSpace(pair.Key) || pair.Value < 0) return false; return true; }
        public EconomyService(EconomyConfig config, LivesService lives)
        {
            if (config == null || lives == null || config.InitialGold < 0 || !Valid(config.InitialBoosters))
                throw new ArgumentException("Invalid economy configuration.");
            initialGold = config.InitialGold; initialBoosters = new Dictionary<string, int>(config.InitialBoosters);
            Gold = initialGold; boosters = new Dictionary<string, int>(initialBoosters); this.lives = lives;
        }
        public ModuleResult GrantReward(string claimId, Reward reward)
        {
            if (string.IsNullOrWhiteSpace(claimId) || reward == null) return new ModuleResult(ModuleError.InvalidArgument);
            if (ledger.Contains(claimId)) return new ModuleResult(ModuleError.AlreadyClaimed);
            if (reward.Gold < 0 || reward.Hearts < 0 || !Valid(reward.Boosters)) return new ModuleResult(ModuleError.InvalidArgument);
            if (reward.Gold > long.MaxValue - Gold) return new ModuleResult(ModuleError.Overflow);
            var next = new Dictionary<string, int>(boosters);
            foreach (var item in reward.Boosters)
            {
                next.TryGetValue(item.Key, out var current);
                if (item.Value > int.MaxValue - current) return new ModuleResult(ModuleError.Overflow);
                next[item.Key] = current + item.Value;
            }
            if (reward.Hearts > 0)
            {
                var check = lives.AddHearts(reward.Hearts);
                if (!check.Success) return check;
            }
            Gold += reward.Gold; boosters = next; ledger.Record(claimId); return new ModuleResult();
        }
        GrantOutcome IRewardGrantor.Grant(string claimId, Reward reward)
        {
            var result = GrantReward(claimId, reward);
            return result.Success ? GrantOutcome.Granted : result.Error == ModuleError.AlreadyClaimed ? GrantOutcome.AlreadyClaimed : GrantOutcome.Rejected;
        }
        public ModuleResult SpendGold(long amount) => Spend(amount, new Dictionary<string, int>());
        public ModuleResult SpendBooster(string id, int amount)
        {
            if (string.IsNullOrWhiteSpace(id)) return new ModuleResult(ModuleError.InvalidArgument);
            return Spend(0, new Dictionary<string, int> { [id] = amount });
        }
        public ModuleResult Spend(long gold, IReadOnlyDictionary<string, int> items)
        {
            if (gold < 0 || items == null || (gold == 0 && items.Count == 0) || !Valid(items)) return new ModuleResult(ModuleError.InvalidArgument);
            foreach (var item in items) if (item.Value == 0) return new ModuleResult(ModuleError.InvalidArgument);
            if (gold > Gold) return new ModuleResult(ModuleError.Insufficient);
            foreach (var item in items)
                if (!boosters.TryGetValue(item.Key, out var count) || count < item.Value) return new ModuleResult(ModuleError.Insufficient);
            Gold -= gold;
            foreach (var item in items) boosters[item.Key] -= item.Value;
            return new ModuleResult();
        }
        public sealed class State
        {
            [JsonProperty(Required = Required.Always)] public int Version { get; set; }
            [JsonProperty(Required = Required.Always)] public long Gold { get; set; }
            [JsonProperty(Required = Required.Always)] public Dictionary<string, int> Boosters { get; set; } = null!;
            [JsonProperty(Required = Required.Always)] public List<string> Claims { get; set; } = null!;
        }
        public ModuleResult Save(SaveDocument document)
        {
            if (document == null) return new ModuleResult(ModuleError.InvalidArgument);
            return new ModuleResult(document.WriteSection("economy", new State { Version = 1, Gold = Gold,
                Boosters = boosters, Claims = ledger.Snapshot() }).Success ? ModuleError.None : ModuleError.CorruptSection);
        }
        public ModuleResult Load(SaveDocument document)
        {
            if (document == null) return new ModuleResult(ModuleError.InvalidArgument);
            var read = document.ReadSection("economy", () => new State { Version = 1, Gold = initialGold,
                Boosters = new Dictionary<string, int>(initialBoosters), Claims = new List<string>() });
            var s = read.Value;
            bool valid = read.Success && s != null && s.Version == 1 && s.Gold >= 0 && s.Boosters != null && Valid(s.Boosters) && s.Claims != null;
            if (valid)
            {
                var unique = new HashSet<string>(StringComparer.Ordinal);
                foreach (var id in s!.Claims!) if (string.IsNullOrWhiteSpace(id) || !unique.Add(id)) { valid = false; break; }
            }
            Gold = valid ? s!.Gold : initialGold;
            boosters = new Dictionary<string, int>(valid ? s!.Boosters : initialBoosters);
            ledger = new ClaimLedger(valid ? s!.Claims! : new List<string>());
            return new ModuleResult(valid ? ModuleError.None : ModuleError.CorruptSection);
        }
    }
}
