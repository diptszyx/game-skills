using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace IndieGame.Shell
{
    public sealed class QuestTemplate
    {
        public string Id { get; }
        public string EventKey { get; }
        public int Target { get; }
        public Reward Reward { get; }
        public QuestTemplate(string id, string eventKey, int target, Reward reward)
        { Id = id; EventKey = eventKey; Target = target; Reward = reward; }
    }
    public sealed class DailyQuestsConfig
    {
        public IReadOnlyList<QuestTemplate> Daily { get; }
        public IReadOnlyList<QuestTemplate> Weekly { get; }
        public int DailyCount { get; }
        public int WeeklyCount { get; }
        public DailyQuestsConfig(IReadOnlyList<QuestTemplate> daily, int dailyCount,
            IReadOnlyList<QuestTemplate>? weekly = null, int weeklyCount = 0)
        { Daily = daily; DailyCount = dailyCount; Weekly = weekly ?? Array.Empty<QuestTemplate>(); WeeklyCount = weeklyCount; }
    }
    public sealed class QuestStatus
    {
        public QuestTemplate Template { get; }
        public int Progress { get; }
        public bool Claimed { get; }
        internal QuestStatus(QuestTemplate template, int progress, bool claimed) { Template = template; Progress = progress; Claimed = claimed; }
    }
    public sealed class DailyQuestsService
    {
        readonly IClock clock;
        readonly IRewardGrantor grantor;
        readonly List<QuestTemplate> daily = new List<QuestTemplate>(), weekly = new List<QuestTemplate>();
        readonly int dailyCount, weeklyCount;
        readonly bool valid;
        int day = -1;
        Dictionary<string, Entry> entries = new Dictionary<string, Entry>(StringComparer.Ordinal);
        // Monday starts the week; date ordinal zero is Monday (0001-01-01).
        static int Week(int date) => date - date % 7;
        public DailyQuestsService(DailyQuestsConfig config, IClock clock, IRewardGrantor grantor)
        {
            this.clock = clock; this.grantor = grantor;
            if (config == null || clock == null || grantor == null || config.Daily == null || config.Weekly == null ||
                config.DailyCount < 0 || config.DailyCount > config.Daily.Count || config.WeeklyCount < 0 || config.WeeklyCount > config.Weekly.Count) return;
            dailyCount = config.DailyCount; weeklyCount = config.WeeklyCount;
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var source in new[] { config.Daily, config.Weekly })
            {
                var dest = ReferenceEquals(source, config.Daily) ? daily : weekly;
                foreach (var t in source)
                {
                    if (t == null || string.IsNullOrWhiteSpace(t.Id) || string.IsNullOrWhiteSpace(t.EventKey) || t.Target <= 0 || !ids.Add(t.Id)) return;
                    var reward = DailyRules.Copy(t.Reward); if (reward == null) return;
                    dest.Add(new QuestTemplate(t.Id, t.EventKey, t.Target, reward));
                }
            }
            valid = true;
        }
        Dictionary<string, Entry> Select(int date)
        {
            var result = new Dictionary<string, Entry>(StringComparer.Ordinal);
            AddSelection(result, daily, dailyCount, date);
            AddSelection(result, weekly, weeklyCount, Week(date) / 7);
            return result;
        }
        static void AddSelection(Dictionary<string, Entry> result, List<QuestTemplate> source, int count, int seed)
        {
            for (int i = 0; i < count; i++)
            { var t = source[(int)(((long)seed + i) % source.Count)]; result.Add(t.Id, new Entry { Id = t.Id }); }
        }
        QuestTemplate Template(string id) => daily.Find(t => t.Id == id) ?? weekly.Find(t => t.Id == id)!;
        bool IsWeekly(string id) => weekly.Exists(t => t.Id == id);
        public IReadOnlyList<QuestStatus> Quests
        {
            get
            {
                var result = new List<QuestStatus>();
                foreach (var e in entries.Values) result.Add(new QuestStatus(Template(e.Id), e.Progress, e.Claimed));
                return new ReadOnlyCollection<QuestStatus>(result);
            }
        }
        public DailyResult Refresh()
        {
            if (!valid) return new DailyResult(DailyError.InvalidConfig);
            int today = DailyRules.Day(clock);
            if (today < day) return new DailyResult(DailyError.ClockReversed);
            if (today == day) return new DailyResult();
            var next = Select(today);
            if (day >= 0 && Week(day) == Week(today))
                foreach (var id in new List<string>(next.Keys)) if (IsWeekly(id) && entries.TryGetValue(id, out var e)) next[id] = e;
            entries = next; day = today;
            return new DailyResult();
        }
        public DailyResult RecordEvent(string eventKey, long amount)
        {
            if (string.IsNullOrWhiteSpace(eventKey) || amount < 0) return new DailyResult(DailyError.InvalidArgument);
            if (amount > int.MaxValue) return new DailyResult(DailyError.Overflow);
            if (!valid) return new DailyResult(DailyError.InvalidConfig);
            int today = DailyRules.Day(clock);
            if (today < day) return new DailyResult(DailyError.ClockReversed);
            // Preflight before Refresh: bad input must not even reset an expired daily set.
            var candidate = today == day ? entries : Select(today);
            foreach (var e in candidate.Values)
            {
                var t = Template(e.Id);
                int progress = today != day && day >= 0 && Week(today) == Week(day) && IsWeekly(e.Id) && entries.TryGetValue(e.Id, out var old) ? old.Progress : e.Progress;
                if (t.EventKey == eventKey && amount > int.MaxValue - progress) return new DailyResult(DailyError.Overflow);
            }
            var refresh = Refresh(); if (!refresh.Success) return refresh;
            foreach (var e in entries.Values)
            { var t = Template(e.Id); if (t.EventKey == eventKey) e.Progress = (int)Math.Min(t.Target, (long)e.Progress + amount); }
            return new DailyResult();
        }
        public DailyResult Claim(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId)) return new DailyResult(DailyError.InvalidArgument);
            if (!valid) return new DailyResult(DailyError.InvalidConfig);
            int today = DailyRules.Day(clock);
            if (today < day) return new DailyResult(DailyError.ClockReversed);
            var candidate = today == day ? entries : Select(today);
            if (!candidate.ContainsKey(questId)) return new DailyResult(DailyError.NotFound);
            var refresh = Refresh(); if (!refresh.Success) return refresh;
            var e = entries[questId]; var t = Template(questId);
            if (e.Claimed) return new DailyResult(DailyError.AlreadyClaimed);
            if (e.Progress < t.Target) return new DailyResult(DailyError.Incomplete);
            var outcome = grantor.Grant("quest:" + DailyRules.DateId(IsWeekly(questId) ? Week(day) : day) + ":" + questId, t.Reward);
            if (outcome == GrantOutcome.Rejected) return new DailyResult(DailyError.GrantRejected);
            e.Claimed = true;
            return new DailyResult(outcome == GrantOutcome.AlreadyClaimed ? DailyError.AlreadyClaimed : DailyError.None);
        }
        public sealed class Entry
        {
            [JsonProperty(Required = Required.Always)] public string Id { get; set; } = "";
            [JsonProperty(Required = Required.Always)] public int Progress { get; set; }
            [JsonProperty(Required = Required.Always)] public bool Claimed { get; set; }
        }
        public sealed class State
        {
            [JsonProperty(Required = Required.Always)] public int Version { get; set; }
            [JsonProperty(Required = Required.Always)] public int Day { get; set; }
            [JsonProperty(Required = Required.Always)] public List<Entry> Entries { get; set; } = null!;
        }
        public DailyResult Save(SaveDocument document)
        {
            if (document == null) return new DailyResult(DailyError.InvalidArgument);
            if (!valid) return new DailyResult(DailyError.InvalidConfig);
            return new DailyResult(document.WriteSection("dailyQuests", new State { Version = 1, Day = day, Entries = new List<Entry>(entries.Values) }).Success ? DailyError.None : DailyError.CorruptSection);
        }
        public DailyResult Load(SaveDocument document)
        {
            if (document == null) return new DailyResult(DailyError.InvalidArgument);
            if (!valid) return new DailyResult(DailyError.InvalidConfig);
            var read = document.ReadSection("dailyQuests", () => new State { Version = 1, Day = -1, Entries = new List<Entry>() });
            var s = read.Value;
            bool ok = read.Success && s != null && s.Version == 1 && (s.Day == -1 || DailyRules.ValidDay(s.Day)) && s.Entries != null;
            var next = ok && s!.Day >= 0 ? Select(s.Day) : new Dictionary<string, Entry>(StringComparer.Ordinal);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            if (ok)
            {
                if (s!.Entries!.Count != next.Count) ok = false;
                else foreach (var e in s.Entries)
                {
                    if (e == null || e.Id == null || !next.ContainsKey(e.Id) || !seen.Add(e.Id) || e.Progress < 0 ||
                        e.Progress > Template(e.Id).Target || (e.Claimed && e.Progress != Template(e.Id).Target)) { ok = false; break; }
                    next[e.Id] = new Entry { Id = e.Id, Progress = e.Progress, Claimed = e.Claimed };
                }
            }
            day = ok ? s!.Day : -1; entries = ok ? next : new Dictionary<string, Entry>(StringComparer.Ordinal);
            return new DailyResult(ok ? DailyError.None : DailyError.CorruptSection);
        }
    }
}
