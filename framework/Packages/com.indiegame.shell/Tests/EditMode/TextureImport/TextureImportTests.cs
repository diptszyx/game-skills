using System.Collections.Generic;
using System.IO;
using IndieGame.Shell.Editor.Textures;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace IndieGame.Shell.Tests.EditMode
{
    public sealed class TextureImportTests
    {
        const string Root = "Assets/__TextureImportTmp";
        const string Ui = Root + "/ui";
        const string Detail = Root + "/detail";
        TextureImportPolicy policy;

        [SetUp] public void SetUp()
        {
            AssetDatabase.DeleteAsset(Root);
            policy = TextureImportPolicy.Standard(); policy.DetailFolders = new[] { Detail };
            Directory.CreateDirectory(Ui); Directory.CreateDirectory(Detail);
            AssetDatabase.Refresh();
        }
        [TearDown] public void TearDown() { AssetDatabase.DeleteAsset(Root); AssetDatabase.Refresh(); }

        static string MakePng(string folder, string name)
        {
            var path = folder + "/" + name + ".png";
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            File.WriteAllBytes(path, tex.EncodeToPNG()); Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            return path;
        }
        static TextureImporter Importer(string path) => (TextureImporter)AssetImporter.GetAtPath(path);

        [Test] public void ApplierSetsAstc6x6OnAndroidWithMipmapsOffAndNoReadWrite()
        {
            var path = MakePng(Ui, "a");
            var report = TextureImportApplier.Apply(new[] { path }, policy);
            var importer = Importer(path);
            Assert.That(report[0].Outcome, Is.EqualTo(TextureImportOutcome.Changed));
            Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));
            Assert.That(importer.GetPlatformTextureSettings("Android").overridden, Is.True);
            Assert.That(importer.GetPlatformTextureSettings("Android").format, Is.EqualTo(TextureImporterFormat.ASTC_6x6));
            Assert.That(importer.mipmapEnabled, Is.False); Assert.That(importer.isReadable, Is.False);
            Assert.That(importer.alphaIsTransparency, Is.True); Assert.That(importer.sRGBTexture, Is.True);
            Assert.That(TextureImportApplier.Apply(new[] { path }, policy)[0].Reason, Is.EqualTo("already compliant"));
        }

        [Test] public void MipmapIsAPolicyFieldNotHardcodedOff()
        {
            var path = MakePng(Ui, "m");
            policy.Mipmaps = true;
            TextureImportApplier.Apply(new[] { path }, policy);
            Assert.That(Importer(path).mipmapEnabled, Is.True);
        }

        [Test] public void OptOutMarkerAssetIsSkippedAndUntouched()
        {
            var path = MakePng(Ui, "keep");
            var importer = Importer(path); importer.userData = TextureImportPolicy.DefaultOptOutMarker; importer.SaveAndReimport();
            var report = TextureImportApplier.Apply(new[] { path }, policy);
            Assert.That(report[0].Outcome, Is.EqualTo(TextureImportOutcome.Skipped)); Assert.That(report[0].Reason, Is.EqualTo("opt-out marker"));
            Assert.That(Importer(path).textureType, Is.Not.EqualTo(TextureImporterType.Sprite));
            Assert.That(TextureImportAudit.Audit(new[] { path }, policy), Is.Empty);
        }

        [Test] public void DetailFolderGetsAstc4x4()
        {
            var path = MakePng(Detail, "eye");
            TextureImportApplier.Apply(new[] { path }, policy);
            Assert.That(Importer(path).GetPlatformTextureSettings("Android").format, Is.EqualTo(TextureImporterFormat.ASTC_4x4));
        }

        [Test] public void AuditFlagsDeviationAndDoesNotModify()
        {
            var path = MakePng(Ui, "bad");
            var deviations = TextureImportAudit.Audit(new[] { path }, policy);
            Assert.That(deviations.Exists(d => d.Problem.Contains("Android override")), Is.True);
            Assert.That(Importer(path).GetPlatformTextureSettings("Android").overridden, Is.False);
            TextureImportApplier.Apply(new[] { path }, policy);
            Assert.That(TextureImportAudit.Audit(new[] { path }, policy), Is.Empty);
        }

        [Test] public void AtlasBuilderIsIdempotentAndUsesAstcOnAndroid()
        {
            MakePng(Ui, "x");
            var map = new Dictionary<string, string[]> { { "UI_Home", new[] { Ui } } };
            var first = SpriteAtlasBuilder.Build(Root, map, policy)[0];
            var second = SpriteAtlasBuilder.Build(Root, map, policy)[0];
            Assert.That(first.Created, Is.True); Assert.That(second.Created, Is.False); Assert.That(second.Changed, Is.False);
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(first.Path);
            Assert.That(atlas.GetPackables().Length, Is.EqualTo(1));
            Assert.That(atlas.GetPlatformSettings("Android").format, Is.EqualTo(TextureImporterFormat.ASTC_6x6));
            Assert.That(atlas.GetPlatformSettings("Android").overridden, Is.True);
        }
    }
}
