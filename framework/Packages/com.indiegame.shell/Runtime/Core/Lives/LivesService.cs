using System;
using Newtonsoft.Json;

namespace IndieGame.Shell
{
    public enum ModuleError { None, InvalidArgument, Overflow, Insufficient, AlreadyClaimed, CorruptSection }
    public sealed class ModuleResult
    {
        public ModuleError Error { get; }
        public bool Success => Error == ModuleError.None;
        public ModuleResult(ModuleError error = ModuleError.None) { Error = error; }
    }
    public sealed class LivesConfig
    {
        public int MaxHearts { get; }
        public int InitialHearts { get; }
        public TimeSpan RegenInterval { get; }
        public LivesConfig(int maxHearts, int initialHearts, TimeSpan regenInterval)
        { MaxHearts = maxHearts; InitialHearts = initialHearts; RegenInterval = regenInterval; }
    }
    public sealed class LivesService : ILivesNotificationSource
    {
        readonly LivesConfig config;
        readonly IClock clock;
        int hearts;
        DateTime anchor, observed, unlimitedUntil;
        public int Hearts => hearts;
        public bool IsUnlimited => EffectiveNow < unlimitedUntil;
        DateTime EffectiveNow { get { var now = clock.UtcNow; return now > observed ? now : observed; } }
        public LivesService(LivesConfig config, IClock clock)
        {
            if (config == null || clock == null || config.MaxHearts <= 0 || config.InitialHearts < 0 ||
                config.InitialHearts > config.MaxHearts || config.RegenInterval.Ticks <= 0)
                throw new ArgumentException("Invalid lives configuration.");
            this.config = config; this.clock = clock; hearts = config.InitialHearts;
            anchor = observed = clock.UtcNow;
            unlimitedUntil = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
        }
        public ModuleResult Refresh() => RefreshAt(EffectiveNow);
        ModuleResult RefreshAt(DateTime now)
        {
            if (hearts < config.MaxHearts)
            {
                var count = (now.Ticks - anchor.Ticks) / config.RegenInterval.Ticks;
                var gained = Math.Min(count, (long)config.MaxHearts - hearts);
                hearts += (int)gained;
                anchor = hearts == config.MaxHearts ? now : anchor.AddTicks(gained * config.RegenInterval.Ticks);
            }
            else anchor = now;
            observed = now;
            return new ModuleResult();
        }
        public ModuleResult SpendHeart()
        {
            // Preview first: even a rejected spend must leave timestamps unchanged.
            var now = EffectiveNow;
            var available = ProjectedHearts(now);
            if (now >= unlimitedUntil && available == 0) return new ModuleResult(ModuleError.Insufficient);
            RefreshAt(now);
            if (now < unlimitedUntil) return new ModuleResult();
            if (hearts == config.MaxHearts) anchor = now;
            hearts--;
            return new ModuleResult();
        }
        int ProjectedHearts(DateTime now) => hearts + (int)Math.Min((now.Ticks - anchor.Ticks) / config.RegenInterval.Ticks, (long)config.MaxHearts - hearts);
        public ModuleResult AddHearts(int amount)
        {
            if (amount < 0) return new ModuleResult(ModuleError.InvalidArgument);
            var now = EffectiveNow;
            if (amount > int.MaxValue - ProjectedHearts(now)) return new ModuleResult(ModuleError.Overflow);
            RefreshAt(now); hearts = Math.Min(config.MaxHearts, hearts + amount);
            if (hearts == config.MaxHearts) anchor = observed;
            return new ModuleResult();
        }
        public ModuleResult AddUnlimited(TimeSpan duration)
        {
            if (duration.Ticks <= 0) return new ModuleResult(ModuleError.InvalidArgument);
            var now = EffectiveNow;
            var start = unlimitedUntil > now ? unlimitedUntil : now;
            if (duration.Ticks > DateTime.MaxValue.Ticks - start.Ticks) return new ModuleResult(ModuleError.Overflow);
            RefreshAt(now); unlimitedUntil = start.Add(duration); return new ModuleResult();
        }
        // Pure query; notification planners explicitly Refresh first.
        public DateTime? FullAtUtc
        {
            get
            {
                if (hearts == config.MaxHearts) return null;
                var remaining = (long)config.MaxHearts - hearts;
                if (remaining > (DateTime.MaxValue.Ticks - anchor.Ticks) / config.RegenInterval.Ticks) return null;
                return anchor.AddTicks(remaining * config.RegenInterval.Ticks);
            }
        }
        public sealed class State
        {
            [JsonProperty(Required = Required.Always)] public int Version { get; set; }
            [JsonProperty(Required = Required.Always)] public int Hearts { get; set; }
            [JsonProperty(Required = Required.Always)] public DateTime Anchor { get; set; }
            [JsonProperty(Required = Required.Always)] public DateTime Observed { get; set; }
            [JsonProperty(Required = Required.Always)] public DateTime UnlimitedUntil { get; set; }
        }
        public ModuleResult Save(SaveDocument document)
        {
            if (document == null) return new ModuleResult(ModuleError.InvalidArgument);
            return new ModuleResult(document.WriteSection("lives", new State { Version = 1, Hearts = hearts,
                Anchor = anchor, Observed = observed, UnlimitedUntil = unlimitedUntil }).Success ? ModuleError.None : ModuleError.CorruptSection);
        }
        public ModuleResult Load(SaveDocument document)
        {
            if (document == null) return new ModuleResult(ModuleError.InvalidArgument);
            var read = document.ReadSection("lives", () => new State { Version = 1, Hearts = config.InitialHearts,
                Anchor = clock.UtcNow, Observed = clock.UtcNow, UnlimitedUntil = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc) });
            var s = read.Value;
            bool valid = read.Success && s != null && s.Version == 1 && s.Hearts >= 0 && s.Hearts <= config.MaxHearts &&
                s.Anchor.Kind == DateTimeKind.Utc && s.Observed.Kind == DateTimeKind.Utc && s.UnlimitedUntil.Kind == DateTimeKind.Utc && s.Anchor <= s.Observed;
            hearts = valid ? s!.Hearts : config.InitialHearts;
            anchor = valid ? s!.Anchor : clock.UtcNow; observed = valid ? s!.Observed : clock.UtcNow;
            unlimitedUntil = valid ? s!.UnlimitedUntil : DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
            return new ModuleResult(valid ? ModuleError.None : ModuleError.CorruptSection);
        }
    }
}
