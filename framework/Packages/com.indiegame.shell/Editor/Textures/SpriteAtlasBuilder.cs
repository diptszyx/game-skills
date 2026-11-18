using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace IndieGame.Shell.Editor.Textures
{
    public readonly struct AtlasBuildResult
    {
        public readonly string Path; public readonly bool Created; public readonly bool Changed;
        public AtlasBuildResult(string path, bool created, bool changed) { Path = path; Created = created; Changed = changed; }
    }

    /// <summary>One SpriteAtlas per screen. PNGs stay the source; atlases only pack folders. Re-running with the same mapping changes nothing.</summary>
    public static class SpriteAtlasBuilder
    {
        const int Padding = 4;

        public static List<AtlasBuildResult> Build(string outputFolder, IDictionary<string, string[]> atlasFolders, TextureImportPolicy policy)
        {
            var results = new List<AtlasBuildResult>();
            foreach (var pair in atlasFolders) results.Add(BuildOne(outputFolder, pair.Key, pair.Value, policy));
            AssetDatabase.SaveAssets();
            return results;
        }

        static AtlasBuildResult BuildOne(string outputFolder, string name, string[] folders, TextureImportPolicy policy)
        {
            var path = outputFolder.TrimEnd('/') + "/" + name + ".spriteatlas";
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
            var created = atlas == null;
            if (created)
            {
                Directory.CreateDirectory(Path.GetFullPath(outputFolder));
                atlas = new SpriteAtlas();
                AssetDatabase.CreateAsset(atlas, path);
            }
            var changed = created;
            changed |= SyncPackables(atlas, folders);
            changed |= SyncSettings(atlas, policy);
            if (changed) EditorUtility.SetDirty(atlas);
            return new AtlasBuildResult(path, created, changed);
        }

        static bool SyncPackables(SpriteAtlas atlas, string[] folders)
        {
            var wanted = new List<UnityEngine.Object>();
            foreach (var folder in folders)
            {
                var folderAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(folder.TrimEnd('/'));
                if (folderAsset == null) throw new ArgumentException("Atlas folder does not exist: " + folder);
                wanted.Add(folderAsset);
            }
            var current = atlas.GetPackables();
            if (SameSet(current, wanted)) return false;
            atlas.Remove(current);
            atlas.Add(wanted.ToArray());
            return true;
        }

        static bool SameSet(UnityEngine.Object[] a, List<UnityEngine.Object> b)
        {
            if (a.Length != b.Count) return false;
            foreach (var item in b) if (Array.IndexOf(a, item) < 0) return false;
            return true;
        }

        static bool SyncSettings(SpriteAtlas atlas, TextureImportPolicy policy)
        {
            var changed = false;
            var packing = atlas.GetPackingSettings();
            if (packing.enableRotation || packing.enableTightPacking || packing.padding != Padding)
            {
                packing.enableRotation = false; packing.enableTightPacking = false; packing.padding = Padding;
                atlas.SetPackingSettings(packing); changed = true;
            }
            var texture = atlas.GetTextureSettings();
            if (texture.readable || texture.generateMipMaps != policy.Mipmaps || !texture.sRGB)
            {
                texture.readable = false; texture.generateMipMaps = policy.Mipmaps; texture.sRGB = true;
                atlas.SetTextureSettings(texture); changed = true;
            }
            var android = atlas.GetPlatformSettings(TextureImportRules.AndroidPlatform);
            if (!android.overridden || android.format != policy.DefaultFormat || android.maxTextureSize != 2048)
            {
                android.overridden = true; android.format = policy.DefaultFormat; android.maxTextureSize = 2048;
                atlas.SetPlatformSettings(android); changed = true;
            }
            return changed;
        }
    }
}
