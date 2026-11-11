using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IndieGame.Shell
{
    public sealed class LocalizationConfig
    {
        public string DefaultLanguage { get; }
        public LocalizationConfig(string defaultLanguage)
        {
            if (string.IsNullOrWhiteSpace(defaultLanguage)) throw new ArgumentException("Default language required.");
            DefaultLanguage = defaultLanguage;
        }
    }
    public sealed class LocalizationMiss
    {
        public string Language { get; }
        public string Key { get; }
        public LocalizationMiss(string language, string key) { Language = language; Key = key; }
    }
    public sealed class LocalizationService
    {
        readonly string defaultLanguage;
        readonly Dictionary<string, Dictionary<string, string>> tables = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
        readonly List<LocalizationMiss> misses = new List<LocalizationMiss>();
        public IReadOnlyList<LocalizationMiss> MissingKeys => misses.AsReadOnly();
        public LocalizationService(LocalizationConfig config)
        { defaultLanguage = (config ?? throw new ArgumentNullException(nameof(config))).DefaultLanguage; }
        // Replace one complete table atomically; a malformed table never replaces a valid one.
        public SaveResult<bool> LoadTable(string language, string json)
        {
            if (string.IsNullOrWhiteSpace(language) || string.IsNullOrWhiteSpace(json)) return new SaveResult<bool>(false, SaveError.InvalidArgument);
            try
            {
                using var reader = new JsonTextReader(new StringReader(json)) { DateParseHandling = DateParseHandling.None };
                var parsed = JObject.Load(reader, new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error });
                if (reader.Read()) return new SaveResult<bool>(false, SaveError.InvalidData);
                var next = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var p in parsed.Properties())
                {
                    if (string.IsNullOrWhiteSpace(p.Name) || p.Value.Type != JTokenType.String) return new SaveResult<bool>(false, SaveError.InvalidData);
                    next.Add(p.Name, (string)p.Value!);
                }
                tables[language] = next;
                return new SaveResult<bool>(true);
            }
            catch (JsonException) { return new SaveResult<bool>(false, SaveError.InvalidData); }
        }
        public SaveResult<string> Lookup(string language, string key)
        {
            if (string.IsNullOrWhiteSpace(language) || string.IsNullOrWhiteSpace(key)) return new SaveResult<string>("", SaveError.InvalidArgument);
            if (tables.TryGetValue(language, out var table) && table.TryGetValue(key, out var text)) return new SaveResult<string>(text);
            misses.Add(new LocalizationMiss(language, key));
            if (tables.TryGetValue(defaultLanguage, out var fallback) && fallback.TryGetValue(key, out var translated)) return new SaveResult<string>(translated);
            return new SaveResult<string>("[" + key + "]");
        }
        public SaveResult<string> Format(string language, string key, params object[] args)
        {
            if (args == null) return new SaveResult<string>("", SaveError.InvalidArgument);
            var lookup = Lookup(language, key);
            return lookup.Success ? FormatSafe(lookup.Value, args) : lookup;
        }
        public static SaveResult<string> FormatSafe(string text, params object[] args)
        {
            if (text == null || args == null) return new SaveResult<string>(text ?? "", SaveError.InvalidArgument);
            try { return new SaveResult<string>(string.Format(CultureInfo.InvariantCulture, text, args)); }
            catch (Exception) { return new SaveResult<string>(text, SaveError.InvalidData); }
        }
        public SaveResult<bool> ClearMissingKeys() { misses.Clear(); return new SaveResult<bool>(true); }
    }
}
