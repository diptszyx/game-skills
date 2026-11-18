using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IndieGame.Shell.Editor.Textures
{
    public enum TextureImportOutcome { Changed, Skipped }

    public readonly struct TextureImportEntry
    {
        public readonly string Path; public readonly TextureImportOutcome Outcome; public readonly string Reason;
        public TextureImportEntry(string path, TextureImportOutcome outcome, string reason) { Path = path; Outcome = outcome; Reason = reason; }
        public override string ToString() => Outcome + " " + Path + " (" + Reason + ")";
    }

    public static class TextureImportApplier
    {
        const string FoldersArg = "-textureFolders";

        [MenuItem("IndieGame/Textures/Apply Import Policy to Assets")]
        static void ApplyFromMenu() => LogReport(ApplyToFolders(new[] { "Assets" }, TextureImportPolicy.Standard()));

        /// <summary>Entry for -executeMethod. Optional arg: -textureFolders "Assets/A;Assets/B" (default Assets).</summary>
        public static void RunFromCommandLine() =>
            LogReport(ApplyToFolders(FoldersFromCommandLine(Environment.GetCommandLineArgs()), TextureImportPolicy.Standard()));

        public static List<TextureImportEntry> ApplyToFolders(string[] folders, TextureImportPolicy policy) =>
            Apply(TextureAssetPaths(folders), policy);

        public static List<TextureImportEntry> Apply(IEnumerable<string> assetPaths, TextureImportPolicy policy)
        {
            var report = new List<TextureImportEntry>();
            var changed = new List<TextureImporter>();
            foreach (var path in assetPaths)
            {
                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) { report.Add(new TextureImportEntry(path, TextureImportOutcome.Skipped, "not a texture")); continue; }
                if (TextureImportRules.IsOptedOut(importer, policy)) { report.Add(new TextureImportEntry(path, TextureImportOutcome.Skipped, "opt-out marker")); continue; }
                var deviations = TextureImportRules.Deviations(importer, policy);
                if (deviations.Count == 0) { report.Add(new TextureImportEntry(path, TextureImportOutcome.Skipped, "already compliant")); continue; }
                TextureImportRules.Apply(importer, policy);
                changed.Add(importer);
                report.Add(new TextureImportEntry(path, TextureImportOutcome.Changed, string.Join("; ", deviations)));
            }
            foreach (var importer in changed) importer.SaveAndReimport();
            return report;
        }

        internal static string[] TextureAssetPaths(string[] folders)
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", folders);
            var paths = new string[guids.Length];
            for (var i = 0; i < guids.Length; i++) paths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
            return paths;
        }

        internal static string[] FoldersFromCommandLine(string[] args)
        {
            for (var i = 0; i < args.Length - 1; i++)
                if (args[i] == FoldersArg) return args[i + 1].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            return new[] { "Assets" };
        }

        static void LogReport(List<TextureImportEntry> report)
        {
            var changed = report.FindAll(e => e.Outcome == TextureImportOutcome.Changed).Count;
            Debug.Log("[TextureImport] changed " + changed + ", skipped " + (report.Count - changed) + " of " + report.Count);
            foreach (var entry in report) if (entry.Outcome == TextureImportOutcome.Changed || entry.Reason == "opt-out marker") Debug.Log("[TextureImport] " + entry);
        }
    }
}
