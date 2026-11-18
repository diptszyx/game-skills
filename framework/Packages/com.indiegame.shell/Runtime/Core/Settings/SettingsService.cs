using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace IndieGame.Shell
{
    public sealed class SettingsConfig
    {
        public bool Sound { get; }
        public bool Music { get; }
        public bool Haptics { get; }
        public string DefaultLanguage { get; }
        public IReadOnlyCollection<string> Languages { get; }
        public SettingsConfig(bool sound, bool music, bool haptics, string defaultLanguage, IEnumerable<string> languages)
        {
            if (string.IsNullOrWhiteSpace(defaultLanguage) || languages == null) throw new ArgumentException("Languages required.");
            var copy = new HashSet<string>(languages, StringComparer.Ordinal);
            foreach (var code in copy) if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Invalid language.");
            if (!copy.Contains(defaultLanguage)) throw new ArgumentException("Default language must be supported.");
            Sound = sound; Music = music; Haptics = haptics; DefaultLanguage = defaultLanguage;
            Languages = new List<string>(copy).AsReadOnly();
        }
    }
    public sealed class SettingsService
    {
        readonly SettingsConfig config;
        readonly HashSet<string> languages;
        public bool Sound { get; private set; }
        public bool Music { get; private set; }
        public bool Haptics { get; private set; }
        public string Language { get; private set; }
        public IReadOnlyCollection<string> Languages => config.Languages;
        public SettingsService(SettingsConfig config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            languages = new HashSet<string>(config.Languages, StringComparer.Ordinal);
            Sound = config.Sound; Music = config.Music; Haptics = config.Haptics; Language = config.DefaultLanguage;
        }
        public SaveResult<bool> SetSound(bool value) { Sound = value; return new SaveResult<bool>(true); }
        public SaveResult<bool> SetMusic(bool value) { Music = value; return new SaveResult<bool>(true); }
        public SaveResult<bool> SetHaptics(bool value) { Haptics = value; return new SaveResult<bool>(true); }
        public SaveResult<bool> SetLanguage(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return new SaveResult<bool>(false, SaveError.InvalidArgument);
            Language = languages.Contains(code) ? code : config.DefaultLanguage;
            return new SaveResult<bool>(true);
        }
        public SaveResult<bool> Save(SaveDocument document) => document == null
            ? new SaveResult<bool>(false, SaveError.InvalidArgument)
            : document.WriteSection("settings", new { Version = 1, Sound, Music, Haptics, Language });
        public SaveResult<bool> Load(SaveDocument document)
        {
            if (document == null) return new SaveResult<bool>(false, SaveError.InvalidArgument);
            var read = document.ReadSection("settings", () => new JObject());
            if (!read.Success || read.Value == null) return new SaveResult<bool>(false, SaveError.InvalidData);
            var s = read.Value;
            if ((s["Version"] != null && (s["Version"]!.Type != JTokenType.Integer || !s["Version"]!.Equals(new JValue(1)))) ||
                !BooleanField(s, "Sound") || !BooleanField(s, "Music") || !BooleanField(s, "Haptics") ||
                (s["Language"] != null && s["Language"]!.Type != JTokenType.String))
                return new SaveResult<bool>(false, SaveError.InvalidData);
            Sound = (bool?)s["Sound"] ?? config.Sound; Music = (bool?)s["Music"] ?? config.Music;
            Haptics = (bool?)s["Haptics"] ?? config.Haptics;
            var code = (string?)s["Language"];
            Language = code != null && languages.Contains(code) ? code : config.DefaultLanguage;
            return new SaveResult<bool>(true);
        }
        static bool BooleanField(JObject s, string key) => s[key] == null || s[key]!.Type == JTokenType.Boolean;
    }
}
