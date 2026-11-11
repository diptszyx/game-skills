using System;
using System.Collections;
using System.IO;
using System.Linq;
using IndieGame.Shell.Unity;
using IndieGame.Shell.Unity.Localization;
using IndieGame.Shell.Unity.Screens;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace IndieGame.Shell.Tests
{
    public sealed class LocalizationScreenshotTests
    {
        static readonly string[] Names={"main","shop","checkin","quests","leaderboard","loading","win","lose","revive","pause","leave"};
        [UnityTest] public IEnumerator EveryScreenAndPopupInBothStylesAndLanguages()
        {
            var shots=Path.GetFullPath(Path.Combine(Application.dataPath,"../Logs/shots")); Directory.CreateDirectory(shots);
            for(int config=0;config<3;config++) foreach(bool bright in new[]{false,true}) foreach(var lang in new[]{"en","vi"}) foreach(var name in Names)
            {
                int width=config==0?720:1080,height=config==0?1600:1920; bool notch=config==2;
                var demos=new DemoStyles(); GameObject root=null,camObject=null; RenderTexture target=null; Texture2D pixels=null;
                IDisposable presenter=null; SessionPopupPresenter popup=null;
                try
                {
                    camObject=new GameObject("Screenshot camera",typeof(Camera)); var camera=camObject.GetComponent<Camera>();
                    camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(.12f,.05f,.18f); camera.orthographic=true;
                    camera.transform.position=new Vector3(0,0,-10); camera.nearClipPlane=.1f; camera.farClipPlane=100;
                    target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32); target.Create(); camera.targetTexture=target;
                    root=new GameObject("Screenshot canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));
                    var canvas=root.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceCamera; canvas.worldCamera=camera; canvas.planeDistance=10;
                    var scaler=root.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution=new Vector2(720,1600); scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; scaler.matchWidthOrHeight=0;
                    var safe=ShellUiFactory.Node("Simulated safe area",root.transform); ShellUiFactory.Stretch(safe);
                    var anchors=SafeAreaMath.ToAnchors(new Rect(0,notch?60:0,width,height-(notch?160:0)),new Vector2(width,height));
                    safe.anchorMin=anchors.min; safe.anchorMax=anchors.max; safe.gameObject.SetActive(true);
                    var factory=new ShellUiFactory(demos.Make(bright)); var clock=new ScreenClock(); var session=ScreenTestData.Session(clock,new ScreenFiles());
                    session.LoadAll(); session.Settings.SetLanguage(lang); var text=SampleLocalization.Load(); var game=new PopupGame();
                    var host=safe.gameObject.AddComponent<PopupHost>();
                    switch(name)
                    {
                        case "main": var main=new MainView(factory,safe); presenter=new MainPresenter(session,main,text,clock,game,_=>{}); break;
                        case "checkin": var check=new CheckInView(factory,safe); presenter=new CheckInPresenter(session,check,text,clock); break;
                        case "quests": var quests=new QuestsView(factory,safe); presenter=new QuestsPresenter(session,quests,text,clock); break;
                        case "shop": var shop=new ShopView(factory,safe,host); presenter=new ShopPresenter(session,new ShopService(ScreenTestData.Catalog(),session,new FakePurchaseService()),shop,text); break;
                        case "leaderboard":
                            var board=new LeaderboardView(factory,safe);
                            board.Render(new LeaderboardSnapshot {State=LeaderboardState.Populated,Entries=new[]{new LeaderboardEntry("River",420,1),new LeaderboardEntry("Sky",380,2)},MyRank=new LeaderboardEntry("You",120,8)},new ScreenText(session,text)); break;
                        case "loading":
                            var loadingRoot=factory.ScreenRoot(safe,"Loading"); var loading=loadingRoot.gameObject.AddComponent<LoadingScreenView>();
                            loadingRoot.gameObject.AddComponent<LoadingContentView>().Initialize(factory,loading,new ScreenText(session,text),"shell.loading.tip");
                            factory.Label(loadingRoot,text.Lookup(lang,"shell.loading.title").Value,true);
                            loading.Show(); loading.SetProgress(.65f); break;
                        default:
                            factory.ScreenRoot(safe,"Popup background");
                            popup=new SessionPopupPresenter(session,factory,host,text,game,new FakeRewardedAdService(),new SessionPopupConfig(new Reward(12),20,"revive","double"));
                            if(name=="win") popup.ShowWin("screenshot"); else if(name=="lose") popup.ShowLose(); else if(name=="revive") popup.ShowRevive(); else if(name=="pause") popup.ShowPause(); else popup.ShowLeave();
                            break;
                    }
                    yield return new WaitForSecondsRealtime(.3f); Canvas.ForceUpdateCanvases();
                    foreach(var label in root.GetComponentsInChildren<TMP_Text>()) { label.ForceMeshUpdate(); Assert.That(label.text,Does.Not.Contain("["),name+":"+lang); Assert.That(label.isTextOverflowing,Is.False,name+":"+lang+":"+label.text);
                        foreach(var character in label.textInfo.characterInfo.Take(label.textInfo.characterCount)) if(character.isVisible) Assert.That(character.textElement.glyph.index,Is.Not.EqualTo(0),"Missing glyph: "+label.text); }
                    Assert.That(text.MissingKeys,Is.Empty,name+":"+lang);
                    foreach(var view in root.GetComponentsInChildren<PopupView>())
                    {
                        var titleRect=(RectTransform)view.Panel.Find("Title"); var titleCorners=new Vector3[4]; var contentCorners=new Vector3[4];
                        titleRect.GetWorldCorners(titleCorners); view.Content.GetWorldCorners(contentCorners);
                        Assert.That(titleCorners[0].y,Is.GreaterThan(contentCorners[1].y),"Popup heading overlaps content: "+name);
                    }
                    camera.Render(); var previous=RenderTexture.active; RenderTexture.active=target;
                    pixels=new Texture2D(width,height,TextureFormat.RGB24,false); pixels.ReadPixels(new Rect(0,0,width,height),0,0); pixels.Apply(); RenderTexture.active=previous;
                    var colors=pixels.GetPixels32(); Assert.That(colors.Any(c=>!c.Equals(colors[0])),Is.True,"Uniform capture "+name);
                    var file=Path.Combine(shots,width+"x"+height+(notch?"_notch":"")+"_"+(bright?"bright-rounded":"dark-flat")+"_"+lang+"_"+name+".png");
                    File.WriteAllBytes(file,CapturePng.Encode(pixels)); Assert.That(File.Exists(file),Is.True); Assert.That(new FileInfo(file).Length,Is.GreaterThan(1000));
                }
                finally
                {
                    presenter?.Dispose(); popup?.Dispose(); Time.timeScale=1;
                    if(root!=null) UnityEngine.Object.DestroyImmediate(root); if(camObject!=null) UnityEngine.Object.DestroyImmediate(camObject);
                    if(pixels!=null) UnityEngine.Object.DestroyImmediate(pixels); if(target!=null) { target.Release(); UnityEngine.Object.DestroyImmediate(target); } demos.Dispose();
                }
            }
        }
    }
}
