using System;
using System.Collections;
using IndieGame.Shell.Unity.Screens;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
namespace IndieGame.Shell.Tests
{
    public sealed class DataScreenBuildTests
    {
        DemoStyles demos; GameObject root; IDisposable presenter;
        [SetUp] public void Setup() { demos=new DemoStyles(); root=new GameObject("Canvas",typeof(RectTransform),typeof(Canvas)); root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay; }
        [TearDown] public void Cleanup() { presenter?.Dispose(); presenter=null; UnityEngine.Object.DestroyImmediate(root); demos.Dispose(); }
        sealed class Game : IGameSessionActions { public void Retry() {} public void Revive() {} public void NextLevel() {} public void Leave() {} public void Pause() {} public void Resume() {} public bool Play() => true; }
        IEnumerator Build(string screen,bool bright)
        {
            var factory=new ShellUiFactory(demos.Make(bright)); var clock=new ScreenClock(); var session=ScreenTestData.Session(clock,new ScreenFiles()); session.LoadAll(); var text=ScreenTestData.Text();
            RectTransform built;
            switch(screen)
            {
                case "CheckIn": var c=new CheckInView(factory,root.transform); presenter=new CheckInPresenter(session,c,text,clock); built=c.Root; break;
                case "Quests": var q=new QuestsView(factory,root.transform); presenter=new QuestsPresenter(session,q,text,clock); built=q.Root; break;
                case "Main": var m=new MainView(factory,root.transform); presenter=new MainPresenter(session,m,text,clock,new Game(),_=>{}); built=m.Root; break;
                default: var s=new ShopView(factory,root.transform,root.AddComponent<PopupHost>()); presenter=new ShopPresenter(session,new ShopService(ScreenTestData.Catalog(),session,new FakePurchaseService()),s,text); built=s.Root; break;
            }
            yield return null; Canvas.ForceUpdateCanvases();
            Assert.That(built.gameObject.activeInHierarchy,Is.True);
            var labels=built.GetComponentsInChildren<TMP_Text>(); Assert.That(labels.Length,Is.GreaterThan(0));
            foreach(var label in labels) Assert.That(label.text,Does.Not.Contain("["));
            Assert.That(text.MissingKeys,Is.Empty); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator CheckInDark()=>Build("CheckIn",false);
        [UnityTest] public IEnumerator CheckInBright()=>Build("CheckIn",true);
        [UnityTest] public IEnumerator QuestsDark()=>Build("Quests",false);
        [UnityTest] public IEnumerator QuestsBright()=>Build("Quests",true);
        [UnityTest] public IEnumerator MainDark()=>Build("Main",false);
        [UnityTest] public IEnumerator MainBright()=>Build("Main",true);
        [UnityTest] public IEnumerator ShopDark()=>Build("Shop",false);
        [UnityTest] public IEnumerator ShopBright()=>Build("Shop",true);
    }
}
