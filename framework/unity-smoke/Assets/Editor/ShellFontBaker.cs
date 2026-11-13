using System;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public static class ShellFontBaker
{
    const string Source = "Packages/com.indiegame.shell/Runtime/Unity/Fonts/Baloo2-ExtraBold.ttf";
    const string Output = "Packages/com.indiegame.shell/Runtime/Unity/Resources/ShellFont.asset";
    const string Punctuation = " .,!?:;·…—–-+x%/()'\"‘’“”&@#*=<>_[]{}|\\~^$€£©®°×÷";

    public static string VietnameseCharacters()
    {
        var tones = new[] { '̀', '̉', '̃', '́', '̣' };
        var builder = new StringBuilder();
        foreach (var vowel in "aăâeêioôơuưyAĂÂEÊIOÔƠUƯY")
        {
            builder.Append(vowel);
            foreach (var tone in tones) builder.Append((vowel.ToString() + tone).Normalize(NormalizationForm.FormC));
        }
        builder.Append("đĐ");
        for (char c = 'a'; c <= 'z'; c++) { builder.Append(c); builder.Append(char.ToUpperInvariant(c)); }
        for (char c = '0'; c <= '9'; c++) builder.Append(c);
        builder.Append(Punctuation);
        return new string(builder.ToString().Distinct().ToArray());
    }

    public static void Bake()
    {
        var source = AssetDatabase.LoadAssetAtPath<Font>(Source);
        if (source == null) throw new InvalidOperationException("Missing font source: " + Source);
        var font = TMP_FontAsset.CreateFontAsset(source, 64, 6, GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic, true);
        if (!font.TryAddCharacters(VietnameseCharacters(), out string missing)) throw new InvalidOperationException("Font missing glyphs: " + missing);
        font.name = "ShellFont";
        font.material.shader = Shader.Find("TextMeshPro/Mobile/Distance Field");
        font.material.EnableKeyword("OUTLINE_ON");
        AssetDatabase.CreateAsset(font, Output);
        AssetDatabase.AddObjectToAsset(font.material, font);
        foreach (var atlas in font.atlasTextures) AssetDatabase.AddObjectToAsset(atlas, font);
        font.atlasPopulationMode = AtlasPopulationMode.Static;
        EditorUtility.SetDirty(font); AssetDatabase.SaveAssets();
        Debug.Log("SHELL_FONT_BAKED glyphs=" + font.characterTable.Count);
    }
}
