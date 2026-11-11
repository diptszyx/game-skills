using System;
using System.Collections.Generic;

namespace IndieGame.Shell
{
    public interface ILivesNotificationSource
    {
        ModuleResult Refresh();
        DateTime? FullAtUtc { get; }
    }
    public interface IDailyRewardNotificationSource { DateTime? ReadyAtUtc { get; } }
    public sealed class NotificationDescriptor
    {
        public string Id { get; }
        public DateTime FireAtUtc { get; }
        public string TextKey { get; }
        public NotificationDescriptor(string id, DateTime fireAtUtc, string textKey)
        { Id = id; FireAtUtc = fireAtUtc; TextKey = textKey; }
    }
    public sealed class NotificationConfig
    {
        public string LivesId { get; }
        public string LivesTextKey { get; }
        public string DailyId { get; }
        public string DailyTextKey { get; }
        public TimeSpan QuietStartUtc { get; }
        public TimeSpan QuietEndUtc { get; }
        public NotificationConfig(string livesId, string livesTextKey, string dailyId, string dailyTextKey, TimeSpan quietStartUtc, TimeSpan quietEndUtc)
        { LivesId = livesId; LivesTextKey = livesTextKey; DailyId = dailyId; DailyTextKey = dailyTextKey; QuietStartUtc = quietStartUtc; QuietEndUtc = quietEndUtc; }
    }
    public sealed class NotificationPlanResult
    {
        public ModuleError Error { get; }
        public bool Success => Error == ModuleError.None;
        public IReadOnlyList<NotificationDescriptor> Notifications { get; }
        internal NotificationPlanResult(ModuleError error, List<NotificationDescriptor> notifications)
        { Error = error; Notifications = notifications.AsReadOnly(); }
    }
    // Plans are derived, never persisted; every call replaces the whole previous plan.
    public sealed class NotificationPlanner
    {
        readonly NotificationConfig config;
        readonly IClock clock;
        public NotificationPlanner(NotificationConfig config, IClock clock)
        {
            if (config == null || clock == null || string.IsNullOrWhiteSpace(config.LivesId) || string.IsNullOrWhiteSpace(config.DailyId) ||
                string.IsNullOrWhiteSpace(config.LivesTextKey) || string.IsNullOrWhiteSpace(config.DailyTextKey) ||
                config.QuietStartUtc < TimeSpan.Zero || config.QuietEndUtc < TimeSpan.Zero ||
                config.QuietStartUtc >= TimeSpan.FromDays(1) || config.QuietEndUtc >= TimeSpan.FromDays(1))
                throw new ArgumentException("Invalid notification configuration.");
            this.config = config; this.clock = clock;
        }
        public NotificationPlanResult Plan(ILivesNotificationSource lives, IDailyRewardNotificationSource daily)
        {
            var result = new List<NotificationDescriptor>();
            if (lives == null || daily == null) return new NotificationPlanResult(ModuleError.InvalidArgument, result);
            var refresh = lives.Refresh();
            if (!refresh.Success) return new NotificationPlanResult(refresh.Error, result);
            var now = clock.UtcNow;
            bool Add(string id, string text, DateTime? candidate)
            {
                if (!candidate.HasValue) return true;
                var time = candidate.Value;
                if (time.Kind != DateTimeKind.Utc) return false;
                if (time < now) time = now;
                var start = config.QuietStartUtc; var end = config.QuietEndUtc; var tod = time.TimeOfDay;
                bool quiet = start < end ? tod >= start && tod < end : start > end && (tod >= start || tod < end);
                if (quiet)
                {
                    var day = time.Date;
                    if (start > end && tod >= start)
                    {
                        if (day.Ticks > DateTime.MaxValue.Ticks - TimeSpan.TicksPerDay) return false;
                        day = day.AddDays(1);
                    }
                    time = day.Add(end);
                }
                result.RemoveAll(n => n.Id == id);
                result.Add(new NotificationDescriptor(id, time, text)); return true;
            }
            if (!Add(config.LivesId, config.LivesTextKey, lives.FullAtUtc) || !Add(config.DailyId, config.DailyTextKey, daily.ReadyAtUtc))
                return new NotificationPlanResult(ModuleError.InvalidArgument, new List<NotificationDescriptor>());
            result.Sort((a, b) => a.FireAtUtc.CompareTo(b.FireAtUtc));
            return new NotificationPlanResult(ModuleError.None, result);
        }
    }
}
