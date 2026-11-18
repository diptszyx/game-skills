using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IndieGame.Shell
{
    public sealed class SessionResult
    {
        public SaveError Error { get; }
        public string Message { get; }
        public bool Recovered { get; }
        public bool Reset { get; }
        public IReadOnlyDictionary<string, string> ModuleErrors { get; }
        public bool Success => Error == SaveError.None && ModuleErrors.Count == 0;
        internal SessionResult(SaveError error, string message, bool recovered, bool reset, Dictionary<string, string> errors)
        {
            Error = error; Message = message; Recovered = recovered; Reset = reset;
            ModuleErrors = new ReadOnlyDictionary<string, string>(errors);
        }
    }

    // Serialized, single-writer use. Mutate services, then explicitly Commit the whole session.
    public sealed class ShellSession
    {
        readonly SaveStore store;
        readonly string path;
        SaveDocument document;
        public LivesService Lives { get; }
        public EconomyService Economy { get; }
        public DailyCheckInService DailyCheckIn { get; }
        public DailyQuestsService DailyQuests { get; }
        public SettingsService Settings { get; }
        public RemoteConfigService Config { get; }

        public ShellSession(SaveStore store, string path, IClock clock, LivesConfig lives,
            EconomyConfig economy, DailyCheckInConfig checkIn, DailyQuestsConfig quests,
            SettingsConfig settings, RemoteConfigRules config)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Save path required.", nameof(path));
            this.path = path;
            document = new SaveDocument(store.CurrentVersion);
            Lives = new LivesService(lives, clock);
            Economy = new EconomyService(economy, Lives);
            DailyCheckIn = new DailyCheckInService(checkIn, clock, Economy);
            DailyQuests = new DailyQuestsService(quests, clock, Economy);
            Settings = new SettingsService(settings);
            Config = new RemoteConfigService(config);
        }

        // Game-owned sections ride in the same document; Commit persists them with the modules.
        public SaveResult<T> ReadGameSection<T>(string key, Func<T> defaultFactory) => document.ReadSection(key, defaultFactory);
        public SaveResult<bool> WriteGameSection<T>(string key, T value) => document.WriteSection(key, value);

        public SessionResult LoadAll()
        {
            var loaded = store.Load(path);
            var errors = new Dictionary<string, string>(StringComparer.Ordinal);
            if (loaded.Value != null)
            {
                document = loaded.Value;
                Record(errors, "lives", Lives.Load(document).Error, ModuleError.None);
                Record(errors, "economy", Economy.Load(document).Error, ModuleError.None);
                Record(errors, "dailyCheckIn", DailyCheckIn.Load(document).Error, DailyError.None);
                Record(errors, "dailyQuests", DailyQuests.Load(document).Error, DailyError.None);
                var settings = Settings.Load(document);
                Record(errors, "settings", settings.Error, SaveError.None);
                if (!settings.Success) Settings.Load(new SaveDocument(store.CurrentVersion));
                var config = Config.Load(document);
                Record(errors, "config", config.Error, SaveError.None);
                if (!config.Success) Config.Load(new SaveDocument(store.CurrentVersion));
            }
            return new SessionResult(loaded.Error, loaded.Message, loaded.Recovered, loaded.Reset, errors);
        }

        // Failure keeps ALL pending in-memory mutations. Caller must inspect the result and
        // retry Commit, or explicitly LoadAll to discard them; never report a failed save as durable.
        public SessionResult Commit()
        {
            var errors = new Dictionary<string, string>(StringComparer.Ordinal);
            Record(errors, "lives", Lives.Save(document).Error, ModuleError.None);
            Record(errors, "economy", Economy.Save(document).Error, ModuleError.None);
            Record(errors, "dailyCheckIn", DailyCheckIn.Save(document).Error, DailyError.None);
            Record(errors, "dailyQuests", DailyQuests.Save(document).Error, DailyError.None);
            Record(errors, "settings", Settings.Save(document).Error, SaveError.None);
            Record(errors, "config", Config.Save(document).Error, SaveError.None);
            if (errors.Count != 0)
                return new SessionResult(SaveError.InvalidData, "Module serialization failed; no file written.", false, false, errors);
            var saved = store.Save(path, document);
            return new SessionResult(saved.Error, saved.Message, saved.Recovered, saved.Reset, errors);
        }

        static void Record<T>(Dictionary<string, string> errors, string section, T error, T none) where T : struct
        { if (!EqualityComparer<T>.Default.Equals(error, none)) errors.Add(section, error.ToString() ?? "Unknown"); }
    }
}
