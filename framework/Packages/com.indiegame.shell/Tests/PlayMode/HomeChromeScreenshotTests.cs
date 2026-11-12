using System.Collections;
using System.IO;
using System.Linq;
using IndieGame.Shell.Unity.UI;
using IndieGame.Shell.Unity;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace IndieGame.Shell.Tests
{
    public sealed class HomeChromeScreenshotTests
    {
        static float Luminance(Color c)
        {
            System.Func<float,float> linear=v=>v<=.04045f?v/12.92f:Mathf.Pow((v+.055f)/1.055f,2.4f);
            return .2126f*linear(c.r)+.7152f*linear(c.g)+.0722f*linear(c.b);
        }
        [UnityTest] public IEnumerator TabLabelsStayInsideCellsAtReadableSize()
        {
            foreach(var width in new[]{720,1080}) foreach(var lang in new[]{"en","vi"})
            {
                int height=width==720?1600:2340;
                var demos=new DemoStyles(); GameObject root=null,camObject=null; RenderTexture target=null; Texture2D pixels=null;
                try
                {
                    camObject=new GameObject("Chrome camera",typeof(Camera)); var camera=camObject.GetComponent<Camera>();
                    camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=Color.black; camera.orthographic=true;
                    camera.transform.position=new Vector3(0,0,-10); camera.nearClipPlane=.1f; camera.farClipPlane=100;
                    target=new RenderTexture(width,height,24); target.Create(); camera.targetTexture=target;
                    root=new GameObject("Chrome canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));
                    var canvas=root.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceCamera; canvas.worldCamera=camera; canvas.planeDistance=10;
                    var scaler=root.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution=new Vector2(720,1600); scaler.matchWidthOrHeight=0;
                    var factory=new ShellUiFactory(demos.MakeChunky()); var host=root.AddComponent<ScreenHost>(); host.Initialize(factory);
                    // Deterministic notch simulation on the SAME safe root, with no nested fitter.
                    host.SafeRoot.GetComponent<SafeAreaFitter>().enabled=false;
                    var safe=SafeAreaMath.ToAnchors(new Rect(0,60,width,height-140),new Vector2(width,height));
                    host.SafeRoot.anchorMin=safe.min; host.SafeRoot.anchorMax=safe.max;
                    host.Show(factory.ScreenRoot(root.transform,"Chrome demo"));
                    var top=new TopBarView(factory,host.ChromeRoot); top.Render(new TopBarDisplay(12345,5,"04:32"));
                    var keys=new[]{"home","checkin","quests","shop","leaderboard"};
                    var labels=lang=="vi"?new[]{"Trang chủ","Điểm danh","Nhiệm vụ","Cửa hàng","Bảng xếp hạng"}:new[]{"Home","Check in","Quests","Shop","Leaderboard"};
                    var tabs=new TabBarView(factory,host.ChromeRoot,keys.Select((k,i)=>new TabDefinition(k,labels[i],k)).ToArray());
                    host.SetTabBar(tabs); host.ShowTabBar(true); tabs.SetSelected("home"); tabs.SetBadge("quests",true);
                    for(int i=0;i<3;i++)
                    {
                        var day=new DayCellView(factory,host.Current); day.Root.anchoredPosition=new Vector2((i-1)*210,80);
                        day.Render((lang=="vi"?"Ngày ":"Day ")+(i+1),(i+1)*50+" gold",(DayState)i);
                    }
                    yield return null; yield return null; Canvas.ForceUpdateCanvases();
                    foreach(var label in top.Root.GetComponentsInChildren<TMP_Text>())
                    {
                        Assert.That(label.fontSharedMaterial,Is.SameAs(factory.Style.Theme.Primary.LabelMaterial));
                        Assert.That(label.fontSharedMaterial.GetColor("_OutlineColor"),Is.EqualTo((Color)new Color32(0x30,0x0c,0x31,255)));
                        Assert.That(label.fontSharedMaterial.GetFloat("_OutlineWidth"),Is.GreaterThan(0));
                    }
                    var corners=new Vector3[4]; host.SafeRoot.GetWorldCorners(corners);
                    var labelRects=new System.Collections.Generic.List<Rect>();
                    foreach(Transform tab in tabs.Root.Find("Tabs"))
                    {
                        var label=tab.GetComponentInChildren<TMP_Text>(); label.ForceMeshUpdate();
                        Assert.That(label.fontSize,Is.GreaterThanOrEqualTo(16),lang+" "+label.text);
                        var cellTexture=UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/KitChunky/"+(tab.name=="Tab home"?"tab_cell_selected":"tab_cell")+".png");
                        float cellLuminance=Luminance(cellTexture.GetPixel(cellTexture.width/2,cellTexture.height/2)),labelLuminance=Luminance(label.color);
                        Assert.That((Mathf.Max(cellLuminance,labelLuminance)+.05f)/(Mathf.Min(cellLuminance,labelLuminance)+.05f),Is.GreaterThanOrEqualTo(4.5f),lang+" label contrast "+label.text);
                        Assert.That(label.textInfo.lineCount,Is.LessThanOrEqualTo(2),label.text);
                        var labelCorners=new Vector3[4]; label.rectTransform.GetWorldCorners(labelCorners);
                        var bounds=new Rect(labelCorners[0],labelCorners[2]-labelCorners[0]);
                        foreach(var otherLabel in labelRects) Assert.That(bounds.Overlaps(otherLabel),Is.False,label.text);
                        labelRects.Add(bounds);
                        var localMin=tab.InverseTransformPoint(labelCorners[0]); var localMax=tab.InverseTransformPoint(labelCorners[2]);
                        var own=((RectTransform)tab).rect;
                        Assert.That(localMin.x,Is.GreaterThanOrEqualTo(own.xMin)); Assert.That(localMin.y,Is.GreaterThanOrEqualTo(own.yMin));
                        Assert.That(localMax.x,Is.LessThanOrEqualTo(own.xMax)); Assert.That(localMax.y,Is.LessThanOrEqualTo(own.yMax));
                        var rect=(RectTransform)tab; Assert.That(rect.rect.width,Is.GreaterThanOrEqualTo(44)); Assert.That(rect.rect.height,Is.GreaterThanOrEqualTo(44));
                        var tabCorners=new Vector3[4]; rect.GetWorldCorners(tabCorners);
                        Assert.That(tabCorners[0].x,Is.GreaterThanOrEqualTo(corners[0].x-.01f)); Assert.That(tabCorners[2].x,Is.LessThanOrEqualTo(corners[2].x+.01f));
                        Assert.That(tabCorners[0].y,Is.GreaterThanOrEqualTo(corners[0].y-.01f)); Assert.That(tabCorners[2].y,Is.LessThanOrEqualTo(corners[2].y+.01f));
                    }
                    foreach(var text in root.GetComponentsInChildren<TMP_Text>())
                    {
                        text.ForceMeshUpdate(); Assert.That(text.isTextOverflowing,Is.False,text.text);
                        foreach(var c in text.textInfo.characterInfo.Take(text.textInfo.characterCount)) if(c.isVisible) Assert.That(c.textElement.glyph.index,Is.Not.Zero,text.text);
                    }
                    camera.Render(); var previous=RenderTexture.active; RenderTexture.active=target;
                    pixels=new Texture2D(width,height,TextureFormat.RGB24,false); pixels.ReadPixels(new Rect(0,0,width,height),0,0); pixels.Apply(); RenderTexture.active=previous;
                    var folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Logs/shots")); Directory.CreateDirectory(folder);
                    File.WriteAllBytes(Path.Combine(folder,"T28d_"+width+"x"+height+"_"+lang+".png"),CapturePng.Encode(pixels));
                }
                finally
                {
                    if(root!=null) Object.DestroyImmediate(root); if(camObject!=null) Object.DestroyImmediate(camObject);
                    if(pixels!=null) Object.DestroyImmediate(pixels); if(target!=null) { target.Release(); Object.DestroyImmediate(target); } demos.Dispose();
                }
            }
        }
    }
}
