using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IndieGame.Shell.Editor.Textures
{
    /// <summary>Single source of truth for what a compliant texture importer looks like; the applier writes it and the audit reads it.</summary>
    static class TextureImportRules
    {
        public const string AndroidPlatform = "Android";

        public static bool IsOptedOut(TextureImporter importer, TextureImportPolicy policy) =>
            HasMarker(importer.userData, policy.OptOutMarker) || InFolders(importer.assetPath, policy.OptOutFolders);

        public static TextureImporterFormat FormatFor(TextureImporter importer, TextureImportPolicy policy) =>
            HasMarker(importer.userData, policy.DetailMarker) || InFolders(importer.assetPath, policy.DetailFolders)
                ? policy.DetailFormat : policy.DefaultFormat;

        public static List<string> Deviations(TextureImporter importer, TextureImportPolicy policy)
        {
            var found = new List<string>();
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            if (importer.textureType != TextureImporterType.Sprite) found.Add("textureType is " + importer.textureType + ", expected Sprite");
            if (settings.spriteMeshType != SpriteMeshType.FullRect) found.Add("spriteMeshType is " + settings.spriteMeshType + ", expected FullRect");
            if (!importer.alphaIsTransparency) found.Add("alphaIsTransparency is off");
            if (!importer.sRGBTexture) found.Add("sRGB is off");
            if (importer.isReadable) found.Add("Read/Write is on");
            if (importer.mipmapEnabled != policy.Mipmaps) found.Add("mipmaps are " + (importer.mipmapEnabled ? "on" : "off") + ", expected " + (policy.Mipmaps ? "on" : "off"));
            var android = importer.GetPlatformTextureSettings(AndroidPlatform);
            var format = FormatFor(importer, policy);
            if (!android.overridden) found.Add("Android override is off");
            if (android.format != format) found.Add("Android format is " + android.format + ", expected " + format);
            if (android.maxTextureSize > policy.MaxSize) found.Add("Android max size " + android.maxTextureSize + " exceeds " + policy.MaxSize);
            return found;
        }

        public static void Apply(TextureImporter importer, TextureImportPolicy policy)
        {
            importer.textureType = TextureImporterType.Sprite;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.alphaIsTransparency = true;
            importer.sRGBTexture = true;
            importer.isReadable = false;
            importer.mipmapEnabled = policy.Mipmaps;
            var android = importer.GetPlatformTextureSettings(AndroidPlatform);
            android.overridden = true;
            android.format = FormatFor(importer, policy);
            android.maxTextureSize = Math.Min(android.maxTextureSize, policy.MaxSize);
            importer.SetPlatformTextureSettings(android);
        }

        static bool HasMarker(string userData, string marker) =>
            !string.IsNullOrEmpty(marker) && !string.IsNullOrEmpty(userData) && userData.Contains(marker);

        static bool InFolders(string assetPath, string[] folders)
        {
            if (folders == null) return false;
            foreach (var folder in folders)
                if (!string.IsNullOrEmpty(folder) && assetPath.StartsWith(folder.TrimEnd('/') + "/", StringComparison.Ordinal)) return true;
            return false;
        }
    }
}
