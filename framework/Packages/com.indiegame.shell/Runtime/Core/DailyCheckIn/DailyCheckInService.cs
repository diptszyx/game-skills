using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace IndieGame.Shell
{
    public enum DailyError { None, InvalidArgument, InvalidConfig, Overflow, AlreadyClaimed, Incomplete, NotFound, ClockReversed, GrantRejected, CorruptSection }
    public sealed class DailyResult
    {
        public DailyError Error { get; }
        public bool Success => Error == DailyError.None;
        public DailyResult(DailyError error = DailyError.None) { Error = error; }
    }
    internal static class DailyRules
    {
        internal static int Day(IClock clock) => (int)(clock.UtcNow.Date.Ticks / TimeSpan.TicksPerDay);
        internal static string DateId(int day) => new DateTime((long)day * TimeSpan.TicksPerDay, DateTimeKind.Utc).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        internal static bool ValidDay(int day) => day >= 0 && day <= DateTime.MaxValue.Ticks / TimeSpan.TicksPerDay;
        internal static Reward? Copy(Reward? reward)
        {
            if (reward == null || reward.Gold < 0 || reward.Hearts < 0) return null;
            var boosters = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var p in reward.Boosters)
            { if (string.IsNullOrWhiteSpace(p.Key) || p.Value < 0) return null; boosters.Add(p.Key, p.Value); }
            return new Reward(reward.Gold, reward.Hearts, new System.Collections.ObjectModel.ReadOnlyDictionary<string, int>(boosters));
        }
    }
    public sealed class DailyCheckInConfig
    {
        public IReadOnlyList<IReadOnlyList<Reward>> RewardTables { get; }
        public DailyCheckInConfig(IReadOnlyList<IReadOnlyList<Reward>> rewardTables) { RewardTables = rewardTables; }
    }
    public sealed class DailyCheckInPreview
    {
        public int NextSlot { get; }
        public Reward? Reward { get; }
        public bool CanClaimNow => Error == DailyError.None;
        public DateTime? NextClaimAtUtc { get; }
        public DailyError Error { get; }
        public bool Success => Error == DailyError.None || Error == DailyError.AlreadyClaimed;
        internal DailyCheckInPreview(int slot, Reward? reward, DateTime? nextClaimAtUtc, DailyError error)
        { NextSlot = slot; Reward = reward; NextClaimAtUtc = nextClaimAtUtc; Error = error; }
    }
    public sealed class DailyCheckInService
    {
        readonly IClock clock;
        readonly IRewardGrantor grantor;
        readonly List<List<Reward>> tables = new List<List<Reward>>();
        readonly bool valid;
        int day = -1, claimedDay = -1, nextSlot;
        public int NextSlot => nextSlot;
        public int ObservedDay => day;
        public DailyCheckInService(DailyCheckInConfig config, IClock clock, IRewardGrantor grantor)
        {
            this.clock = clock; this.grantor = grantor;
            if (clock == null || grantor == null || config?.RewardTables == null || config.RewardTables.Count != 2) return;
            foreach (var row in config.RewardTables)
            {
                if (row == null || row.Count != 5) return;
                var copy = new List<Reward>();
                foreach (var r in row) { var reward = DailyRules.Copy(r); if (reward == null) return; copy.Add(reward); }
                tables.Add(copy);
            }
            valid = true;
        }
        DailyError Eligibility(int today) => !valid ? DailyError.InvalidConfig :
            today < day ? DailyError.ClockReversed : claimedDay == today ? DailyError.AlreadyClaimed : DailyError.None;
        Reward RewardFor(int today) => tables[today % tables.Count][nextSlot];
        // Read-only: does not advance the observed date, slot, ledger or clock watermark.
        public DailyCheckInPreview Preview()
        {
            if (!valid) return new DailyCheckInPreview(nextSlot, null, null, DailyError.InvalidConfig);
            int today = DailyRules.Day(clock);
            int nextDay = Math.Max(today, day) + 1;
            DateTime? next = DailyRules.ValidDay(nextDay)
                ? new DateTime((long)nextDay * TimeSpan.TicksPerDay, DateTimeKind.Utc) : (DateTime?)null;
            return new DailyCheckInPreview(nextSlot, RewardFor(today), next, Eligibility(today));
        }
        public DailyResult Claim()
        {
            if (!valid) return new DailyResult(DailyError.InvalidConfig);
            int today = DailyRules.Day(clock);
            var eligibility = Eligibility(today);
            if (eligibility != DailyError.None) return new DailyResult(eligibility);
            var outcome = grantor.Grant("checkin:" + DailyRules.DateId(today), RewardFor(today));
            if (outcome == GrantOutcome.Rejected) return new DailyResult(DailyError.GrantRejected);
            day = today; claimedDay = today; nextSlot = (nextSlot + 1) % tables[0].Count;
            return new DailyResult(outcome == GrantOutcome.AlreadyClaimed ? DailyError.AlreadyClaimed : DailyError.None);
        }
        public sealed class State
        {
            [JsonProperty(Required = Required.Always)] public int Version { get; set; }
            [JsonProperty(Required = Required.Always)] public int Day { get; set; }
            [JsonProperty(Required = Required.Always)] public int ClaimedDay { get; set; }
            [JsonProperty(Required = Required.Always)] public int NextSlot { get; set; }
        }
        public DailyResult Save(SaveDocument document)
        {
            if (document == null) return new DailyResult(DailyError.InvalidArgument);
            if (!valid) return new DailyResult(DailyError.InvalidConfig);
            return new DailyResult(document.WriteSection("dailyCheckIn", new State { Version = 1, Day = day, ClaimedDay = claimedDay, NextSlot = nextSlot }).Success ? DailyError.None : DailyError.CorruptSection);
        }
        public DailyResult Load(SaveDocument document)
        {
            if (document == null) return new DailyResult(DailyError.InvalidArgument);
            if (!valid) return new DailyResult(DailyError.InvalidConfig);
            var read = document.ReadSection("dailyCheckIn", () => new State { Version = 1, Day = -1, ClaimedDay = -1 });
            var s = read.Value;
            bool ok = read.Success && s != null && s.Version == 1 && s.NextSlot >= 0 && s.NextSlot < tables[0].Count &&
                ((s.Day == -1 && s.ClaimedDay == -1 && s.NextSlot == 0) || (DailyRules.ValidDay(s.Day) && s.ClaimedDay == s.Day));
            day = ok ? s!.Day : -1; claimedDay = ok ? s!.ClaimedDay : -1; nextSlot = ok ? s!.NextSlot : 0;
            return new DailyResult(ok ? DailyError.None : DailyError.CorruptSection);
        }
    }
}
