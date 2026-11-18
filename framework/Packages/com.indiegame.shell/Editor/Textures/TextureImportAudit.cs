using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IndieGame.Shell.Editor.Textures
{
    public readonly struct TextureDeviation
    {
        public readonly string Path; public readonly string Problem;
        public TextureDeviation(string path, string problem) { Path = path; Problem = problem; }
        public override string ToString() => Path + ": " + Problem;
    }

    /// <summary>Read-only: never modifies an importer. Opted-out assets are not reported.</summary>
    public static class TextureImportAudit
    {
        public static List<TextureDeviation> Audit(IEnumerable<string> assetPaths, TextureImportPolicy policy)
        {
            var result = new List<TextureDeviation>();
            foreach (var path in assetPaths)
            {
                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer) || TextureImportRules.IsOptedOut(importer, policy)) continue;
                foreach (var problem in TextureImportRules.Deviations(importer, policy)) result.Add(new TextureDeviation(path, problem));
            }
            return result;
        }

        public static List<TextureDeviation> AuditFolders(string[] folders, TextureImportPolicy policy) =>
            Audit(TextureImportApplier.TextureAssetPaths(folders), policy);

        [MenuItem("IndieGame/Textures/Audit Import Policy")]
        static void AuditFromMenu() => Log(AuditFolders(new[] { "Assets" }, TextureImportPolicy.Standard()));

        /// <summary>Entry for -executeMethod; exits 1 when any deviation exists. Optional arg: -textureFolders "Assets/A;Assets/B".</summary>
        public static void RunFromCommandLine()
        {
            var deviations = AuditFolders(TextureImportApplier.FoldersFromCommandLine(Environment.GetCommandLineArgs()), TextureImportPolicy.Standard());
            Log(deviations);
            if (Application.isBatchMode) EditorApplication.Exit(deviations.Count == 0 ? 0 : 1);
        }

        static void Log(List<TextureDeviation> deviations)
        {
            Debug.Log("[TextureAudit] " + deviations.Count + " deviation(s)");
            foreach (var d in deviations) Debug.Log("[TextureAudit] " + d);
        }
    }
}
