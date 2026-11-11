using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IndieGame.Shell
{
    public sealed class SaveDocument
    {
        public int SchemaVersion { get; internal set; }
        internal JObject Sections { get; }
        public SaveDocument(int schemaVersion = 1) : this(schemaVersion, new JObject()) { }
        internal SaveDocument(int version, JObject sections)
        { SchemaVersion = version; Sections = sections; }

        internal static JsonSerializer Serializer() => JsonSerializer.Create(new JsonSerializerSettings
        { TypeNameHandling = TypeNameHandling.None, DateParseHandling = DateParseHandling.None,
          FloatParseHandling = FloatParseHandling.Decimal });

        public SaveResult<T> ReadSection<T>(string key, Func<T> defaultFactory)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key) || defaultFactory == null)
                    return new SaveResult<T>(default!, SaveError.InvalidArgument, "Section key and default factory are required.");
                var token = Sections[key];
                return new SaveResult<T>(token == null || token.Type == JTokenType.Null
                    ? defaultFactory() : token.ToObject<T>(Serializer())!);
            }
            catch (Exception ex) { return new SaveResult<T>(default!, SaveError.InvalidData, $"Section '{key}': {ex.Message}"); }
        }

        public SaveResult<bool> WriteSection<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
                return new SaveResult<bool>(false, SaveError.InvalidArgument, "Section key is required.");
            try
            {
                var token = value == null ? JValue.CreateNull() : JToken.FromObject(value, Serializer());
                Sections[key] = token;
                return new SaveResult<bool>(true);
            }
            catch (Exception ex) { return new SaveResult<bool>(false, SaveError.InvalidData, $"Section '{key}': {ex.Message}"); }
        }
    }
}
