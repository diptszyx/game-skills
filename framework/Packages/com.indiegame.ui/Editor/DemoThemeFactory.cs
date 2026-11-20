using System.IO;
using GameKit.UI;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace GameKit.UI.Editor
{
    public static class DemoThemeFactory
    {
        public static UiTheme Create(string folder, TMP_FontAsset font)
        {
            if (string.IsNullOrEmpty(folder) || !folder.StartsWith("Assets/")) throw new System.ArgumentException("Folder must be an Assets path.", nameof(folder));
            if (font == null) throw new System.ArgumentNullException(nameof(font));
            EnsureFolder(folder);
            Sprite panel = MakeSprite(folder, "panel.png", new Color32(66, 78, 110, 255), 256, 256, 52, 72, false);
            Sprite primary = MakeSprite(folder, "button_green.png", new Color32(36, 157, 94, 255), 256, 112, 40, 48, false);
            Sprite secondary = MakeSprite(folder, "button_yellow.png", new Color32(232, 184, 57, 255), 256, 112, 40, 48, false);
            Sprite neutral = MakeSprite(folder, "button_gray.png", new Color32(117, 128, 145, 255), 256, 112, 40, 48, false);
            Sprite close = MakeSprite(folder, "close.png", new Color32(221, 93, 103, 255), 96, 96, 48, 8, true);
            Material bodyMaterial = new Material(font.material) { name = "Demo Body Material" };
            bodyMaterial.color = new Color32(204, 218, 231, 255);
            Material titleMaterial = new Material(font.material) { name = "Demo Title Material" };
            titleMaterial.color = new Color32(255, 209, 114, 255);
            AssetDatabase.CreateAsset(bodyMaterial, folder + "/DemoBody.mat");
            AssetDatabase.CreateAsset(titleMaterial, folder + "/DemoTitle.mat");
            var theme = ScriptableObject.CreateInstance<UiTheme>();
            theme.name = "DemoTheme";
            theme.Font = font;
            theme.BodyMaterial = bodyMaterial;
            theme.TitleMaterial = titleMaterial;
            theme.BodyColor = new Color32(204, 218, 231, 255);
            theme.CurvedTitle = false;
            theme.TitleCurveRadius = 0f;
            theme.TitleOffsetY = -52f;
            theme.PanelSprite = panel;
            theme.PanelSize = new Vector2(560f, 460f);
            // Preserve the original demo layout when production defaults change.
            theme.TitleFontSize = 42f;
            theme.BodyFontSize = 30f;
            theme.BodyOffsetY = -45f;
            theme.ButtonRowOffsetY = -theme.PanelSize.y * .27f;
            theme.TwoButtonOffsetX = 145f;
            theme.PanelPixelsPerUnitMultiplier = panel.rect.height / theme.PanelSize.y;
            theme.TitleSize = new Vector2(theme.PanelSize.x - 80f, 100f);
            theme.BodySize = new Vector2(theme.PanelSize.x - 100f, 170f);
            theme.CloseSprite = close;
            theme.CloseSize = new Vector2(88f, 88f);
            theme.CloseOffset = new Vector2(236f, 186f);
            theme.ScrimColor = new Color(0.025f, 0.04f, 0.09f, 0.78f);
            Configure(theme.Primary, primary, bodyMaterial, Color.white, 30f, new Vector2(224f, 84f));
            Configure(theme.Secondary, secondary, bodyMaterial, new Color32(42, 42, 42, 255), 30f, new Vector2(224f, 84f));
            Configure(theme.Neutral, neutral, bodyMaterial, Color.white, 28f, new Vector2(224f, 84f));
            AssetDatabase.CreateAsset(theme, folder + "/DemoTheme.asset");
            AssetDatabase.SaveAssets();
            return theme;
        }

        private static void Configure(UiButtonStyle style, Sprite sprite, Material material, Color color, float fontSize, Vector2 size)
        {
            style.Enabled = sprite;
            style.Locked = sprite;
            style.LabelMaterial = material;
            style.LabelColor = color;
            style.FontSize = fontSize;
            style.FaceCentreFromTop = 0.5f;
            style.DefaultSize = size;
            style.LabelPadding = new Vector2(28f, 20f);
        }

        private static Sprite MakeSprite(string folder, string name, Color color, int width, int height, int radius,
            int border, bool closeGlyph)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = Path.GetFileNameWithoutExtension(name), filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float dx = Mathf.Max(radius - (x + .5f), (x + .5f) - (width - radius), 0f);
                float dy = Mathf.Max(radius - (y + .5f), (y + .5f) - (height - radius), 0f);
                float coverage = Mathf.Clamp01(radius - Mathf.Sqrt(dx * dx + dy * dy) + .5f);
                Color pixel = new Color(color.r, color.g, color.b, coverage);
                if (closeGlyph && coverage > 0f)
                {
                    float u = x - width / 2f + .5f, v = y - height / 2f + .5f, arm = width * .2f;
                    float distance = Mathf.Min(Mathf.Abs(u - v), Mathf.Abs(u + v)) * .7071f;
                    if (distance < width * .045f && Mathf.Abs(u) < arm && Mathf.Abs(v) < arm)
                        pixel = new Color(1f, 1f, 1f, coverage);
                }
                pixels[y * width + x] = pixel;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            string path = folder + "/" + name;
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Bilinear;
            importer.spriteBorder = new Vector4(border, border, border, border);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void EnsureFolder(string folder)
        {
            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++) { string next = current + "/" + parts[i]; if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]); current = next; }
        }
    }
}
