using System;
using System.IO;
using System.Linq;
using System.Reflection;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace IndieGame.Shell.Tests
{
    public sealed class KitChunkyTests
    {
        const string Folder="Assets/Art/KitChunky";
        static readonly string[] Sliced={"btn_primary","btn_secondary","btn_locked","list_row","panel_popup","pill_plain","tab_bar_plain","tab_cell","tab_cell_selected"};
        static readonly string[] Singles={"bg_home","bg_play","logo"};
        static string[] Sources()=>Directory.GetFiles(Folder,"*.png").OrderBy(p=>p).ToArray();
        [OneTimeSetUp] public void ImportKit()
        {
            // The importer lives in the smoke project's allowed Assets/Editor directory.
            var importer=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("KitChunkyImporter")).First(t=>t!=null);
            importer.GetMethod("Ensure").Invoke(null,null);
        }
        [Test] public void EverySlicedSpritePreservesItsCapsAndRims()
        {
            foreach(var name in Sliced)
            {
                var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Folder+"/"+name+".png"); Assert.That(sprite,Is.Not.Null,name);
                var b=sprite.border; Assert.That(new[]{b.x,b.y,b.z,b.w},Is.All.GreaterThan(0),name);
                Assert.That(b.x+b.z,Is.LessThan(sprite.rect.width),name+" horizontal stretch region");
                Assert.That(b.y+b.w,Is.LessThan(sprite.rect.height),name+" vertical stretch region");
                if(name=="panel_popup") Assert.That(b.w,Is.GreaterThanOrEqualTo(386),"Ribbon and upper inner rim must stay above vertical stretch region");
            }
        }
        [Test] public void AtlasPacksEveryUiSpriteAndExcludesBackgroundsAndLogo()
        {
            Assert.That(EditorSettings.spritePackerMode,Is.EqualTo(SpritePackerMode.AlwaysOnAtlas));
            var atlas=AssetDatabase.LoadAssetAtPath<SpriteAtlas>(Folder+"/KitChunkyUi.spriteatlas"); Assert.That(atlas,Is.Not.Null);
            // Mascot is a regular single UI sprite: atlas packing preserves its aspect and alpha.
            var ui=Sources().Where(p=>!Singles.Contains(Path.GetFileNameWithoutExtension(p))).ToArray();
            SpriteAtlasUtility.PackAtlases(new[]{atlas},EditorUserBuildSettings.activeBuildTarget,false);
            Assert.That(atlas.spriteCount,Is.EqualTo(ui.Length));
            foreach(var path in Sources())
                Assert.That(atlas.CanBindTo(AssetDatabase.LoadAssetAtPath<Sprite>(path)),Is.EqualTo(ui.Contains(path)),path);
        }
        [Test] public void ChunkyStyleValidatesWithTheRequestedSlots()
        {
            // Exercise the actual PlayMode style builder, without duplicating its mappings.
            var type=Type.GetType("IndieGame.Shell.Tests.DemoStyles, IndieGame.Shell.Tests.PlayMode",true);
            var demos=Activator.CreateInstance(type,true);
            try
            {
                var style=(ShellStyle)type.GetMethod("MakeChunky").Invoke(demos,null);
                Assert.DoesNotThrow(()=>style.Validate()); Assert.That(style.name,Is.EqualTo("chunky"));
                Assert.That(style.Theme.PanelSprite.name,Is.EqualTo("panel_popup"));
                Assert.That(style.Theme.Primary.Enabled.name,Is.EqualTo("btn_primary"));
                Assert.That(style.Theme.Secondary.Enabled.name,Is.EqualTo("btn_secondary"));
                Assert.That(style.Theme.Primary.Locked.name,Is.EqualTo("btn_locked"));
                Assert.That(style.Theme.Secondary.Locked.name,Is.EqualTo("btn_locked"));
                Assert.That(style.ListRowSprite.name,Is.EqualTo("list_row"));
                Assert.That(style.CurrencyIcon.name,Is.EqualTo("icons_1")); Assert.That(style.HeartIcon.name,Is.EqualTo("icons_2"));
                Assert.That(style.BoosterIcon.name,Is.EqualTo("icons_5")); Assert.That(style.ScreenBackground.name,Is.EqualTo("bg_home"));
                Assert.That(style.Theme.CloseSprite.name,Is.EqualTo("btn_close"));
                Assert.That(style.Theme.ToggleOn.name,Is.EqualTo("toggle_1")); Assert.That(style.Theme.ToggleOff.name,Is.EqualTo("toggle_2"));
            }
            finally { type.GetMethod("Dispose").Invoke(demos,null); }
        }
        [Test] public void EveryCutoutHasTransparentCornersAndCorrectImportSettings()
        {
            foreach(var path in Sources())
            {
                var t=(TextureImporter)AssetImporter.GetAtPath(path); var settings=new TextureImporterSettings(); t.ReadTextureSettings(settings);
                Assert.That(t.textureType,Is.EqualTo(TextureImporterType.Sprite),path);
                Assert.That(settings.spriteMeshType,Is.EqualTo(SpriteMeshType.FullRect),path);
                Assert.That(t.alphaIsTransparency,Is.True,path); Assert.That(t.mipmapEnabled,Is.False,path);
                Assert.That(t.filterMode,Is.EqualTo(FilterMode.Bilinear),path);
                var android=t.GetPlatformTextureSettings("Android"); Assert.That(android.overridden,Is.True,path);
                Assert.That(android.format,Is.EqualTo(TextureImporterFormat.ASTC_6x6),path);
                if(Path.GetFileNameWithoutExtension(path)=="tab_bar_plain") Assert.That(android.maxTextureSize,Is.GreaterThanOrEqualTo(2148),"Keep full source size and border scale on Android");
                if(Path.GetFileNameWithoutExtension(path).StartsWith("bg_")) continue; // Opaque scene backgrounds are intentional.
                var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                foreach(var corner in new[]{new Vector2Int(0,0),new Vector2Int(texture.width-1,0),new Vector2Int(0,texture.height-1),new Vector2Int(texture.width-1,texture.height-1)})
                    Assert.That(texture.GetPixel(corner.x,corner.y).a,Is.Zero,path+" corner "+corner);
            }
        }
        [Test] public void SourcePngsAreKeptByteForByte()
        {
            var original=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../art/kit_v3_cut"));
            var source=Directory.GetFiles(original,"*.png"); Assert.That(source.Length,Is.EqualTo(34),"T27 supplied folder contains 34, despite brief saying 32");
            var retired=new[]{"pill_currency.png","pill_lives.png","tab_bar.png"};
            var v5=new[]{"gear_button","board_frame"};
            var v4=new[]{"tab_home","tab_checkin","tab_quests","tab_shop","tab_trophy","pill_plain","tab_cell","tab_cell_selected"};
            Assert.That(Sources().Length,Is.EqualTo(source.Length-retired.Length+v4.Length+v5.Length+1));
            foreach(var name in v4)
                Assert.That(File.ReadAllBytes(Folder+"/"+name+".png").SequenceEqual(File.ReadAllBytes(Path.Combine(original,"../kit_v4_cut",name+".png"))),Is.True,name);
            foreach(var name in v5)
                Assert.That(File.ReadAllBytes(Folder+"/"+name+".png").SequenceEqual(File.ReadAllBytes(Path.Combine(original,"../kit_v5_cut",name+".png"))),Is.True,name);
            foreach(var path in source.Where(p=>!retired.Contains(Path.GetFileName(p))))
                Assert.That(File.ReadAllBytes(Folder+"/"+Path.GetFileName(path)).SequenceEqual(File.ReadAllBytes(path)),Is.True,path);
        }
    }
}
