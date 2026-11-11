using System;
using System.Linq;
using IndieGame.Shell.Unity;
using IndieGame.Shell.Unity.Localization;
using IndieGame.Shell.Unity.Screens;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Tests
{
    public sealed class LocalizedViewTests
    {
        [Test] public void RawViewsAreFittedWithoutExtraCall()
        {
            foreach(var size in new[]{new Vector2(720,1600),new Vector2(720,1280),new Vector2(720,1173.333f)})
            foreach(bool bright in new[]{false,true}) foreach(var lang in new[]{"en","vi"})
            foreach(var name in new[]{"main","checkin","quests","shop","leaderboard","loading","win","lose","revive","pause","leave","shop-error"})
            {
                var demos=new DemoStyles();
                var root=new GameObject("Raw canvas",typeof(RectTransform),typeof(Canvas));
                IDisposable presenter=null; SessionPopupPresenter popup=null;
                try
                {
                    var safe=ShellUiFactory.Node("Available area",root.transform); safe.sizeDelta=size; safe.gameObject.SetActive(true);
                    var factory=new ShellUiFactory(demos.Make(bright)); var clock=new ScreenClock();
                    var session=ScreenTestData.Session(clock,new ScreenFiles()); session.LoadAll(); session.Settings.SetLanguage(lang);
                    var text=SampleLocalization.Load(); var game=new PopupGame(); var host=safe.gameObject.AddComponent<PopupHost>();
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
                            // Unity does not invoke non-ExecuteAlways Awake in EditMode.
                            typeof(LoadingScreenView).GetMethod("Awake",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Instance).Invoke(loading,null);
                            loadingRoot.gameObject.AddComponent<LoadingContentView>().Initialize(factory,loading,new ScreenText(session,text),"shell.loading.tip");
                            factory.Label(loadingRoot,text.Lookup(lang,"shell.loading.title").Value,true);
                            loading.Show(); loading.SetProgress(.65f); break;
                        case "shop-error":
                            var failedShop=new ShopView(factory,safe,host);
                            failedShop.ShowError(text.Lookup(lang,"shell.error.title").Value,text.Lookup(lang,"shell.error.PurchaseFailed").Value); break;
                        default:
                            factory.ScreenRoot(safe,"Popup background");
                            popup=new SessionPopupPresenter(session,factory,host,text,game,new FakeRewardedAdService(),new SessionPopupConfig(new Reward(12),20,"revive","double"));
                            if(name=="win") popup.ShowWin("raw-view"); else if(name=="lose") popup.ShowLose(); else if(name=="revive") popup.ShowRevive(); else if(name=="pause") popup.ShowPause(); else popup.ShowLeave();
                            break;
                    }
                    // Only normal Unity layout/mesh processing, with no application fitting pass.
                    Check(root,name+":"+lang);
                    // Change the text on the existing labels, rather than reconstructing the view.
                    // The other language has different long titles and action lengths.
                    var other=lang=="en"?"vi":"en";
                    foreach(var label in root.GetComponentsInChildren<TMP_Text>())
                    {
                        if(string.IsNullOrEmpty(label.text)) continue;
                        var key=SampleLocalization.Keys.FirstOrDefault(k=>text.Lookup(lang,k).Value==label.text);
                        label.text=key==null?label.text+" ":text.Lookup(other,key).Value;
                    }
                    Check(root,name+":"+other+":updated");
                }
                finally { presenter?.Dispose(); popup?.Dispose(); Time.timeScale=1; UnityEngine.Object.DestroyImmediate(root); demos.Dispose(); }
            }
        }
        static Rect Bounds(RectTransform rect)
        {
            var corners=new Vector3[4]; rect.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners.Min(c=>c.x),corners.Min(c=>c.y),corners.Max(c=>c.x),corners.Max(c=>c.y));
        }
        static void Check(GameObject root,string context)
        {
            Canvas.ForceUpdateCanvases();
            foreach(var layout in root.GetComponentsInChildren<VerticalLayoutGroup>()) LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layout.transform);
            Canvas.ForceUpdateCanvases();
            // A held Pause remains active beneath Leave; compare within each view,
            // not between two intentionally stacked modal layers.
            var actions=root.GetComponentsInChildren<Button>().Where(b=>b.GetComponent<ShellAppearance>()?.Role=="button").ToArray();
            foreach(var label in root.GetComponentsInChildren<TMP_Text>())
            {
                label.ForceMeshUpdate();
                var rect=label.rectTransform.rect;
                // GetPreferredValues with auto sizing enabled measures at fontSizeMax
                // against infinite height, rather than at the size of the rendered mesh.
                bool automatic=label.enableAutoSizing; label.enableAutoSizing=false;
                var preferred=label.GetPreferredValues(label.text,rect.width,float.PositiveInfinity);
                label.enableAutoSizing=automatic;
                Assert.That(preferred.x,Is.LessThanOrEqualTo(rect.width+1),context+": width "+label.text);
                Assert.That(preferred.y,Is.LessThanOrEqualTo(rect.height+1),context+": height "+label.text);
                Assert.That(label.isTextOverflowing,Is.False,context+": "+label.text);
                if(string.IsNullOrEmpty(label.text)) continue;
                foreach(var action in actions)
                {
                    if(label.transform.IsChildOf(action.transform)) continue;
                    if(label.GetComponentInParent<PopupView>()!=action.GetComponentInParent<PopupView>()) continue;
                    Assert.That(Bounds(label.rectTransform).Overlaps(Bounds((RectTransform)action.transform)),Is.False,context+": label/action overlap "+label.text);
                }
            }
        }
    }
}
