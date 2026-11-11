using System;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IndieGame.Shell
{
    public sealed class RemoteConfig
    {
        public long RewardGold { get; }
        public int RefreshSeconds { get; }
        public RemoteConfig(long rewardGold, int refreshSeconds) { RewardGold = rewardGold; RefreshSeconds = refreshSeconds; }
    }
    public sealed class RemoteConfigRules
    {
        public RemoteConfig Defaults { get; }
        public long MaxRewardGold { get; }
        public int MinRefreshSeconds { get; }
        public int MaxRefreshSeconds { get; }
        public RemoteConfigRules(RemoteConfig defaults, long maxRewardGold, int minRefreshSeconds, int maxRefreshSeconds)
        {
            if (defaults == null || maxRewardGold < 0 || minRefreshSeconds < 0 || maxRefreshSeconds < minRefreshSeconds ||
                defaults.RewardGold < 0 || defaults.RewardGold > maxRewardGold || defaults.RefreshSeconds < minRefreshSeconds || defaults.RefreshSeconds > maxRefreshSeconds)
                throw new ArgumentException("Invalid configuration rules.");
            Defaults = defaults; MaxRewardGold = maxRewardGold; MinRefreshSeconds = minRefreshSeconds; MaxRefreshSeconds = maxRefreshSeconds;
        }
    }
    public enum ConfigSource { Remote, Cache, Defaults }
    public sealed class ConfigResult
    {
        public RemoteConfig Value { get; }
        public ConfigSource Source { get; }
        public SaveError Error { get; }
        public bool Success => Error == SaveError.None;
        public ConfigResult(RemoteConfig value, ConfigSource source, SaveError error = SaveError.None) { Value = value; Source = source; Error = error; }
    }
    public sealed class RemoteConfigService
    {
        readonly RemoteConfigRules rules;
        RemoteConfig? cached;
        public RemoteConfig Current { get; private set; }
        public RemoteConfigService(RemoteConfigRules rules)
        { this.rules = rules ?? throw new ArgumentNullException(nameof(rules)); Current = rules.Defaults; }
        bool Valid(RemoteConfig value) => value.RewardGold >= 0 && value.RewardGold <= rules.MaxRewardGold &&
            value.RefreshSeconds >= rules.MinRefreshSeconds && value.RefreshSeconds <= rules.MaxRefreshSeconds;
        bool TryParse(string json, out RemoteConfig value)
        {
            value = rules.Defaults;
            if (string.IsNullOrWhiteSpace(json)) return false;
            try
            {
                var o = JObject.Parse(json, new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error });
                if (o["RewardGold"]?.Type != JTokenType.Integer || o["RefreshSeconds"]?.Type != JTokenType.Integer) return false;
                var gold = BigInteger.Parse(o["RewardGold"]!.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                var seconds = BigInteger.Parse(o["RefreshSeconds"]!.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                if (gold < 0 || gold > long.MaxValue || seconds < 0 || seconds > int.MaxValue) return false;
                var candidate = new RemoteConfig((long)gold, (int)seconds);
                if (!Valid(candidate)) return false;
                value = candidate; return true;
            }
            catch (Exception ex) when (ex is JsonException || ex is FormatException || ex is OverflowException) { return false; }
        }
        public ConfigResult Refresh(IConfigSource source)
        {
            if (source == null) return new ConfigResult(Current, cached == null ? ConfigSource.Defaults : ConfigSource.Cache, SaveError.InvalidArgument);
            var fetched = source.Fetch();
            return Apply(fetched.Success ? fetched.Value : "");
        }
        public ConfigResult Apply(string json)
        {
            if (TryParse(json, out var next))
            { cached = next; Current = next; return new ConfigResult(Current, ConfigSource.Remote); }
            Current = cached ?? rules.Defaults;
            return new ConfigResult(Current, cached == null ? ConfigSource.Defaults : ConfigSource.Cache, SaveError.InvalidData);
        }
        public SaveResult<bool> Save(SaveDocument document) => document == null
            ? new SaveResult<bool>(false, SaveError.InvalidArgument)
            : document.WriteSection("config", new { Version = 1, Cached = cached });
        public SaveResult<bool> Load(SaveDocument document)
        {
            if (document == null) return new SaveResult<bool>(false, SaveError.InvalidArgument);
            var read = document.ReadSection("config", () => new JObject { ["Version"] = 1 });
            var s = read.Value;
            if (!read.Success || s == null || !Equals(s["Version"], new JValue(1))) return new SaveResult<bool>(false, SaveError.InvalidData);
            RemoteConfig? next = null;
            if (s["Cached"] != null && s["Cached"]!.Type != JTokenType.Null)
            {
                if (!TryParse(s["Cached"]!.ToString(Formatting.None), out var parsed)) return new SaveResult<bool>(false, SaveError.InvalidData);
                next = parsed;
            }
            cached = next; Current = next ?? rules.Defaults;
            return new SaveResult<bool>(true);
        }
    }
}
