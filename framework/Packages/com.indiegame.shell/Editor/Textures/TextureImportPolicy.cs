using UnityEditor;

namespace IndieGame.Shell.Editor.Textures
{
    /// <summary>Pure data describing how source PNGs must be imported. Borders (9-slice) are never touched: they come from the game's ShellStyle.</summary>
    public sealed class TextureImportPolicy
    {
        public const string DefaultOptOutMarker = "indiegame:texture-opt-out";
        public const string DefaultDetailMarker = "indiegame:texture-detail";

        public TextureImporterFormat DefaultFormat = TextureImporterFormat.ASTC_6x6;
        public TextureImporterFormat DetailFormat = TextureImporterFormat.ASTC_4x4;
        public bool Mipmaps = false;
        public int MaxSize = 4096;
        public string OptOutMarker = DefaultOptOutMarker;
        public string DetailMarker = DefaultDetailMarker;
        public string[] OptOutFolders = new string[0];
        public string[] DetailFolders = new string[0];

        public static TextureImportPolicy Standard() => new TextureImportPolicy();
    }
}
