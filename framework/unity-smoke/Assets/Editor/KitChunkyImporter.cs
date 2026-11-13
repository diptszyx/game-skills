// [UNITY-SKILL:SPRITEATLAS]
// T27 explicitly requests a repeatable importer and the V1 .spriteatlas filename.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

public sealed class KitChunkyImporter : AssetPostprocessor
{
    public const string Folder = "Assets/Art/KitChunky";
    public const string AtlasPath = Folder + "/KitChunkyUi.spriteatlas";
    // Source pixel borders: left, bottom, right, top. Caps include outline and rim.
    public static readonly Dictionary<string, Vector4> Borders = new Dictionary<string, Vector4> {
        {"tab_bar_plain", new Vector4(250,250,250,250)},
        {"tab_cell", new Vector4(130,130,130,130)},
        {"tab_cell_selected", new Vector4(140,140,140,140)},
        {"pill_plain", new Vector4(200,200,200,200)},
        {"day_cell_1", new Vector4(80,80,80,130)},
        {"day_cell_2", new Vector4(80,80,80,130)},
        {"day_cell_3", new Vector4(80,80,80,130)},
        {"btn_primary", new Vector4(310,130,310,100)},
        {"btn_secondary", new Vector4(218,90,218,80)},
        {"btn_locked", new Vector4(327,130,327,100)},
        {"list_row", new Vector4(173,40,173,40)},
        {"panel_popup", new Vector4(120,120,170,400)},
        {"board_frame", new Vector4(130,130,130,130)},

    };
    public static bool IsUi(string path) => !new[]{"bg_home","bg_play","logo"}.Contains(Path.GetFileNameWithoutExtension(path));
    static float Ppu(string name) => name.StartsWith("day_cell_") ? 300 : name == "pill_plain" ? 501.25f : name == "tab_bar_plain" ? 323.125f : name == "tab_cell" ? 349f/1.2f : name == "tab_cell_selected" ? 305 : name == "panel_popup" || name == "board_frame" ? 300 : name == "list_row" ? 300 : name == "btn_primary" ? 700 : name == "btn_secondary" ? 500 : name == "btn_locked" ? 740 : 100;
    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(Folder + "/", StringComparison.Ordinal) || !assetPath.EndsWith(".png")) return;
        Configure((TextureImporter)assetImporter);
    }
    static void Configure(TextureImporter t)
    {
        var name=Path.GetFileNameWithoutExtension(t.assetPath);
        t.textureType=TextureImporterType.Sprite; t.spriteImportMode=SpriteImportMode.Single;
        var settings=new TextureImporterSettings(); t.ReadTextureSettings(settings);
        settings.spriteMeshType=SpriteMeshType.FullRect; t.SetTextureSettings(settings);
        t.alphaSource=TextureImporterAlphaSource.FromInput; t.alphaIsTransparency=true;
        t.mipmapEnabled=false; t.filterMode=FilterMode.Bilinear; t.wrapMode=TextureWrapMode.Clamp;
        t.npotScale=TextureImporterNPOTScale.None; t.maxTextureSize=name=="tab_bar_plain"?4096:2048;
        t.textureCompression=TextureImporterCompression.Uncompressed;
        // Readback is intentional for the import-alpha regression gate.
        t.isReadable=true; t.spritePixelsPerUnit=Ppu(name);
        t.spriteBorder=Borders.TryGetValue(name,out var border)?border:Vector4.zero;
        t.SetPlatformTextureSettings(new TextureImporterPlatformSettings {
            name="Android",overridden=true,maxTextureSize=name=="tab_bar_plain"?4096:2048,format=TextureImporterFormat.ASTC_6x6
        });
    }
    public static void Ensure()
    {
        if (!Directory.Exists(Folder)) return;
        KitChunkyAtlas.EnablePacking();
        var paths=Directory.GetFiles(Folder,"*.png").OrderBy(p=>p).ToArray();
        foreach(var path in paths)
        {
            var t=(TextureImporter)AssetImporter.GetAtPath(path);
            if(t==null) { AssetDatabase.ImportAsset(path); t=(TextureImporter)AssetImporter.GetAtPath(path); }
            Configure(t); t.SaveAndReimport();
        }
        var atlas=AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AtlasPath);
        if(atlas==null) { atlas=new SpriteAtlas(); AssetDatabase.CreateAsset(atlas,AtlasPath); }
        atlas.Remove(atlas.GetPackables());
        atlas.Add(paths.Where(IsUi).Select(p=>(UnityEngine.Object)AssetDatabase.LoadAssetAtPath<Sprite>(p)).ToArray());
        atlas.SetIncludeInBuild(true);
        var packing=atlas.GetPackingSettings(); packing.padding=4; packing.enableRotation=false; packing.enableTightPacking=false; atlas.SetPackingSettings(packing);
        var texture=atlas.GetTextureSettings();
        if(texture.maxTextureSize<=0) { var defaults=new SpriteAtlas(); texture=defaults.GetTextureSettings(); UnityEngine.Object.DestroyImmediate(defaults); }
        texture.readable=false; texture.generateMipMaps=false; texture.sRGB=true; texture.filterMode=FilterMode.Bilinear; atlas.SetTextureSettings(texture);
        atlas.SetPlatformSettings(new TextureImporterPlatformSettings {name="DefaultTexturePlatform",maxTextureSize=4096,format=TextureImporterFormat.Automatic,textureCompression=TextureImporterCompression.Uncompressed});
        atlas.SetPlatformSettings(new TextureImporterPlatformSettings {name="Standalone",overridden=true,maxTextureSize=4096,format=TextureImporterFormat.RGBA32,textureCompression=TextureImporterCompression.Uncompressed});
        atlas.SetPlatformSettings(new TextureImporterPlatformSettings {name="Android",overridden=true,maxTextureSize=4096,format=TextureImporterFormat.ASTC_6x6});
        EditorUtility.SetDirty(atlas); AssetDatabase.SaveAssets();
        SpriteAtlasUtility.PackAtlases(new[]{atlas},EditorUserBuildSettings.activeBuildTarget,false);
        Debug.Log("T27 SpritePacker="+EditorSettings.spritePackerMode+" sources="+paths.Length+" packed="+atlas.spriteCount);
        foreach(var pair in Borders) Debug.Log("T27 border "+pair.Key+" L/B/R/T="+pair.Value);
    }
}

[InitializeOnLoad]
public sealed class KitChunkyAtlas : IPreprocessBuildWithReport
{
    const string PreviousMode="T27.PreviousSpritePackerMode";
    const string Baseline=KitChunkyImporter.Folder+"/EditorPackerBaseline.txt";
    static KitChunkyAtlas()
    {
        SpriteAtlasManager.atlasRequested+=(tag,bind)=> {
            if(tag=="KitChunkyUi") { var atlas=AssetDatabase.LoadAssetAtPath<SpriteAtlas>(KitChunkyImporter.AtlasPath); if(atlas!=null) bind(atlas); }
        };
        EditorApplication.delayCall+=KitChunkyImporter.Ensure;
        // Keep excluded ProjectSettings unchanged on disk after this Editor session.
        EditorApplication.quitting+=()=> {
            if(SessionState.GetInt(PreviousMode,-1)>=0)
                EditorSettings.spritePackerMode=(SpritePackerMode)(File.Exists(Baseline)?int.Parse(File.ReadAllText(Baseline)):SessionState.GetInt(PreviousMode,0));
        };
    }
    public static void EnablePacking()
    {
        if(SessionState.GetInt(PreviousMode,-1)<0) SessionState.SetInt(PreviousMode,(int)EditorSettings.spritePackerMode);
        EditorSettings.spritePackerMode=SpritePackerMode.AlwaysOnAtlas;
        if(EditorSettings.spritePackerMode!=SpritePackerMode.AlwaysOnAtlas) throw new InvalidOperationException("KitChunky Editor packing is disabled");
    }
    public int callbackOrder=>0;
    public void OnPreprocessBuild(BuildReport report)=>KitChunkyImporter.Ensure();
}
