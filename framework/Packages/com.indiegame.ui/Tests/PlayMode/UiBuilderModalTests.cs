using System.Collections;
using System.IO;
using GameKit.UI;
using GameKit.UI.Editor;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace GameKit.UI.Tests.PlayMode
{
    public sealed class DemoPrefabSetup : IPrebuildSetup, IPostBuildCleanup
    {
        public const string Temp = "Assets/_KitTmp";
        public const string PrefabPath = Temp + "/Demo.prefab";

        public void Setup()
        {
            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            if (font == null) font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (font == null) return;
            AssetDatabase.DeleteAsset(Temp);
            AssetDatabase.CreateFolder("Assets", "_KitTmp");
            UiTheme theme = DemoThemeFactory.Create(Temp, font);
            UiBuilder.BuildModal(theme, new ModalSpec
            {
                Name = "Demo", Title = "Demo title", Body = "Body text",
                Buttons = new[]
                {
                    new ModalButton { Label = "Yes", Kind = UiButtonKind.Primary },
                    new ModalButton { Label = "No", Kind = UiButtonKind.Secondary }
                }
            }, PrefabPath);
        }

        public void Cleanup() => AssetDatabase.DeleteAsset(Temp);
    }

    public sealed class UiBuilderModalTests
    {

        [UnityTest]
        [PrebuildSetup(typeof(DemoPrefabSetup))]
        [PostBuildCleanup(typeof(DemoPrefabSetup))]
        public IEnumerator DemoThemeModal_OpensAndCapturesBothAspects()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(DemoPrefabSetup.PrefabPath);
            if (prefab == null) Assert.Ignore("TMP Essentials missing");
            var world = new GameObject("KitDemoWorld");
            try
            {
                Camera camera = new GameObject("Camera", typeof(Camera)).GetComponent<Camera>();
                camera.transform.SetParent(world.transform);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color32(40, 90, 170, 255);
                var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler));
                canvasObject.transform.SetParent(world.transform);
                Canvas canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(720f, 1600f);
                scaler.matchWidthOrHeight = 1f;
                UiModal modal = Object.Instantiate(prefab, canvasObject.transform).GetComponent<UiModal>();
                modal.Open();
                yield return new WaitForSecondsRealtime(0.4f);
                Assert.IsTrue(modal.IsOpen);
                Assert.AreEqual(Vector3.one, modal.Panel.localScale);
                Capture(camera, "kit_demo_20x9.png", 720, 1600);
                Capture(camera, "kit_demo_16x9.png", 720, 1280);
            }
            finally
            {
                Object.Destroy(world);
            }
        }

        private static void Capture(Camera camera, string fileName, int width, int height)
        {
            var target = new RenderTexture(width, height, 24);
            RenderTexture previous = RenderTexture.active;
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
            image.Apply();
            RenderTexture.active = previous;
            camera.targetTexture = null;
            target.Release();
            Object.Destroy(target);
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/screenshots"));
            Directory.CreateDirectory(folder);
            File.WriteAllBytes(Path.Combine(folder, fileName), image.EncodeToPNG());
            Object.Destroy(image);
        }
    }
}
