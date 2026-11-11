using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IndieGame.Shell
{
    // One writer per path. A migration edits its private document and advances exactly one version.
    public sealed class SaveStore
    {
        readonly ISaveFileSystem files;
        readonly Dictionary<int, Action<SaveDocument>> migrations = new Dictionary<int, Action<SaveDocument>>();
        public int CurrentVersion { get; }
        public SaveStore(int currentVersion = 1, ISaveFileSystem? fileSystem = null)
        {
            if (currentVersion < 1) throw new ArgumentOutOfRangeException(nameof(currentVersion));
            CurrentVersion = currentVersion;
            files = fileSystem ?? new SystemSaveFileSystem();
        }
        public SaveResult<bool> RegisterMigration(int fromVersion, Action<SaveDocument> migration)
        {
            if (fromVersion < 1 || fromVersion >= CurrentVersion || migration == null || migrations.ContainsKey(fromVersion))
                return new SaveResult<bool>(false, SaveError.InvalidArgument, "Migration must be unique and advance a supported version by one.");
            migrations.Add(fromVersion, migration);
            return new SaveResult<bool>(true);
        }

        SaveResult<SaveDocument?> Read(string path)
        {
            string json;
            try { json = files.ReadAllText(path); }
            catch (Exception ex) { return new SaveResult<SaveDocument?>(null, SaveError.Io, $"Read '{path}': {ex.Message}"); }
            try
            {
                using var reader = new JsonTextReader(new StringReader(json))
                { DateParseHandling = DateParseHandling.None, FloatParseHandling = FloatParseHandling.Decimal };
                var root = JObject.Load(reader, new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error });
                if (reader.Read()) throw new JsonException("Trailing content.");
                var versionToken = root["SchemaVersion"];
                if (versionToken != null && versionToken.Type != JTokenType.Integer) throw new JsonException("SchemaVersion must be an integer.");
                var version = versionToken == null ? BigInteger.One
                    : BigInteger.Parse(versionToken.ToString(), CultureInfo.InvariantCulture);
                if (version > CurrentVersion) return new SaveResult<SaveDocument?>(null, SaveError.FutureVersion,
                    $"'{path}' schema {version} is newer than supported {CurrentVersion}; overwrite refused.");
                if (version < 1) throw new JsonException("SchemaVersion must be positive.");
                var sections = root["Sections"];
                if (sections != null && sections.Type != JTokenType.Object) throw new JsonException("Sections must be an object.");
                return new SaveResult<SaveDocument?>(new SaveDocument((int)version, (JObject?)sections ?? new JObject()));
            }
            catch (Exception ex) { return new SaveResult<SaveDocument?>(null, SaveError.InvalidData, $"Invalid save '{path}': {ex.Message}"); }
        }

        SaveResult<SaveDocument?> Upgrade(SaveResult<SaveDocument?> result)
        {
            if (result.Value == null) return result;
            var doc = result.Value;
            try
            {
                while (doc.SchemaVersion < CurrentVersion)
                {
                    if (!migrations.TryGetValue(doc.SchemaVersion, out var migrate))
                        throw new InvalidOperationException($"Missing migration {doc.SchemaVersion} → {doc.SchemaVersion + 1}.");
                    migrate(doc);
                    doc.SchemaVersion++;
                }
                return result;
            }
            catch (Exception ex) { return new SaveResult<SaveDocument?>(null, SaveError.Migration, ex.Message); }
        }

        public SaveResult<SaveDocument?> Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return new SaveResult<SaveDocument?>(null, SaveError.InvalidArgument, "Path required.");
            try
            {
                SaveResult<SaveDocument?>? primary = files.Exists(path) ? Read(path) : null;
                if (primary != null && primary.Error != SaveError.InvalidData) return Upgrade(primary);
                var backup = files.Exists(path + ".bak") ? Read(path + ".bak") : null;
                if (backup != null && backup.Success)
                {
                    var upgraded = Upgrade(backup);
                    if (!upgraded.Success) return upgraded;
                    return new SaveResult<SaveDocument?>(upgraded.Value, SaveError.InvalidData,
                        $"Recovered from backup. {primary?.Message ?? "Primary missing."}", recovered: true);
                }
                if (backup != null && backup.Error != SaveError.InvalidData) return backup;
                if (primary == null && backup == null) return new SaveResult<SaveDocument?>(new SaveDocument(CurrentVersion));
                return new SaveResult<SaveDocument?>(new SaveDocument(CurrentVersion), SaveError.InvalidData,
                    $"Reset save. {primary?.Message ?? "Primary missing."} {backup?.Message ?? "Backup missing."}", reset: true);
            }
            catch (Exception ex) { return new SaveResult<SaveDocument?>(null, SaveError.Io, $"Load '{path}': {ex.Message}"); }
        }

        public SaveResult<bool> Save(string path, SaveDocument document)
        {
            if (string.IsNullOrWhiteSpace(path) || document == null)
                return new SaveResult<bool>(false, SaveError.InvalidArgument, "Path and document required.");
            if (document.SchemaVersion != CurrentVersion)
                return new SaveResult<bool>(false, document.SchemaVersion > CurrentVersion ? SaveError.FutureVersion : SaveError.Migration,
                    "Document must match current schema; load with registered migrations first.");
            try
            {
                bool exists = files.Exists(path);
                bool validPrimary = false;
                foreach (var candidate in new[] { path, path + ".bak" })
                {
                    if (!files.Exists(candidate)) continue;
                    var check = Read(candidate);
                    if (check.Error == SaveError.FutureVersion || check.Error == SaveError.Io)
                        return new SaveResult<bool>(false, check.Error, check.Message);
                    if (candidate == path) validPrimary = check.Success;
                }
                var root = new JObject { ["SchemaVersion"] = document.SchemaVersion, ["Sections"] = document.Sections.DeepClone() };
                var temp = path + ".tmp";
                files.WriteAllText(temp, root.ToString(Formatting.Indented));
                if (exists) files.Replace(temp, path, validPrimary ? path + ".bak" : null);
                else files.Move(temp, path);
                return new SaveResult<bool>(true);
            }
            catch (Exception ex) { return new SaveResult<bool>(false, SaveError.Io, $"Save '{path}': {ex.Message}"); }
        }
    }
}
