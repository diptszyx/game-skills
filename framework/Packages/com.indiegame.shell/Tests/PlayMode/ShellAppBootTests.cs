using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IndieGame.Shell.Unity;
using IndieGame.Shell.Unity.Screens;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace IndieGame.Shell.Tests
{
    public sealed class ShellAppBootTests
    {
        sealed class Game : IGameSessionActions
        {
            public int Plays,Pauses,Resumes,Leaves;
            public void Play()=>Plays++; public void Pause()=>Pauses++; public void Resume()=>Resumes++; public void Leave()=>Leaves++;
            public void Retry() {} public void Revive() {} public void NextLevel() {}
        }
        DemoStyles demos; ShellApp app; Game game; ShellAppConfig config; Mouse mouse; Keyboard keyboard; string path; float priorScale; GameObject external;
        int canvases,systems,objects;
#if UNITY_EDITOR
        UnityEditor.EditorWindow gameView; int previousSize;
#endif
        [SetUp] public void Setup()
        {
            SetPortrait(); demos=new DemoStyles(); game=new Game(); priorScale=Time.timeScale; Time.timeScale=.4f;
            canvases=Count<Canvas>(); systems=Count<EventSystem>(); objects=Count<RectTransform>();
            mouse=InputSystem.AddDevice<Mouse>(); keyboard=InputSystem.AddDevice<Keyboard>(); path=Path.Combine(Application.temporaryCachePath,"T29a-"+Guid.NewGuid().ToString("N")+".json");
            app=new GameObject("Shell Test").AddComponent<ShellApp>();
            config=new ShellAppConfig { Style=demos.Make(false),SavePath=path,
                Lives=new LivesConfig(5,2,TimeSpan.FromSeconds(60)), Economy=new EconomyConfig(100),
                CheckIn=new DailyCheckInConfig(new[]{Enumerable.Repeat(new Reward(1),5).ToArray(),Enumerable.Repeat(new Reward(1),5).ToArray()}), Quests=new DailyQuestsConfig(Array.Empty<QuestTemplate>(),0),
                Settings=new SettingsConfig(true,true,true,"en",new[]{"en","vi"}), RemoteConfig=new RemoteConfigRules(new RemoteConfig(1,60),1000,1,3600),
                SessionPopups=new SessionPopupConfig(new Reward(1),1,"revive","double"),Actions=game,
                LanguageTables=new Dictionary<string,TextAsset>{{"en",Resources.Load<TextAsset>("ShellLocalization/en")},{"vi",Resources.Load<TextAsset>("ShellLocalization/vi")}},
                GameScreenFactory=c=> { Assert.That(app.Ready,Is.False); Assert.That(c.Flow,Is.Not.Null); Assert.That(c.Session,Is.SameAs(app.Session)); return c.Factory.ScreenRoot(c.Parent,"Fake Game"); } };
        }
        static int Count<T>() where T:UnityEngine.Object =>UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length;
        [TearDown] public void Cleanup()
        {
            if(app!=null) UnityEngine.Object.DestroyImmediate(app.gameObject);
            if(external!=null) UnityEngine.Object.DestroyImmediate(external);
            InputSystem.RemoveDevice(mouse); InputSystem.RemoveDevice(keyboard);
#if UNITY_EDITOR
            if(gameView!=null) gameView.GetType().GetProperty("selectedSizeIndex").SetValue(gameView,previousSize);
#endif
            demos.Dispose(); Time.timeScale=priorScale;
            foreach(var suffix in new[]{"",".bak",".tmp"}) if(File.Exists(path+suffix)) File.Delete(path+suffix);
        }
        IEnumerator Boot()
        {
            var result=app.Boot(config); Assert.That(result.Success,Is.True,result.Error); Assert.That(app.Ready,Is.False);
            float until=Time.realtimeSinceStartup+5; while(!app.Ready && Time.realtimeSinceStartup<until) yield return null;
            Assert.That(app.Ready,Is.True); Assert.That(app.Popups.HandlesOwnBackKey,Is.False);
        }
        Button MainButton(string key)=>app.Main.Root.GetComponentsInChildren<Button>().Single(b=>b.GetComponentInChildren<TMP_Text>().text==app.Text.Format("en",key).Value);
        IEnumerator Tap(Button button)
        {
            yield return null; Canvas.ForceUpdateCanvases();
            var position=RectTransformUtility.WorldToScreenPoint(null,button.transform.TransformPoint(((RectTransform)button.transform).rect.center));
            var hits=new List<RaycastResult>(); EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=position},hits);
            Assert.That(hits.Count,Is.GreaterThan(0),button.name); Assert.That(hits[0].gameObject.GetComponentInParent<Button>(),Is.SameAs(button),button.name);
            InputSystem.QueueStateEvent(mouse,new MouseState {position=position}); yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState {position=position}.WithButton(MouseButton.Left)); yield return null; yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState {position=position}); yield return null; yield return new WaitForSecondsRealtime(.15f);
        }
        [UnityTest] public IEnumerator GameTableOverridesAndShellDefaultsFillMissingKeys()
        {
            config.LanguageTables=new Dictionary<string,TextAsset>{{"en",new TextAsset("{\"shell.play\":\"Go\"}")},{"vi",new TextAsset("{\"shell.play\":\"Di\"}")}};
            yield return Boot();
            Assert.That(app.Text.Lookup("en","shell.play").Value,Is.EqualTo("Go"));
            Assert.That(app.Text.Lookup("en","shell.tab.home").Value,Is.EqualTo("Home"));
            Assert.That(app.Text.Lookup("vi","shell.back").Value,Is.Not.Contain("["));
        }
        [UnityTest] public IEnumerator PlayAndPauseLeaveUseRealPointerInput()
        {
            yield return Boot(); yield return Tap(MainButton("shell.play"));
            Assert.That(game.Plays,Is.EqualTo(1)); Assert.That(app.Screens.Current,Is.SameAs(app.GameRoot));
            app.Back(); Assert.That(Time.timeScale,Is.Zero); Assert.That(game.Pauses,Is.EqualTo(1)); yield return new WaitForSecondsRealtime(.1f);
            yield return Tap(app.Flow.Popups[SessionPopupKind.Pause].Buttons["leave"]);
            yield return Tap(app.Flow.Popups[SessionPopupKind.Leave].Buttons["cancel"]);
            Assert.That(app.Popups.Top,Is.SameAs(app.Flow.Popups[SessionPopupKind.Pause].View)); Assert.That(Time.timeScale,Is.Zero); Assert.That(game.Resumes,Is.Zero);
            yield return Tap(app.Flow.Popups[SessionPopupKind.Pause].Buttons["leave"]);
            yield return Tap(app.Flow.Popups[SessionPopupKind.Leave].Buttons["confirm"]);
            Assert.That(app.Screens.Current,Is.SameAs(app.Main.Root)); Assert.That(game.Leaves,Is.EqualTo(1)); Assert.That(Time.timeScale,Is.EqualTo(.4f));
        }
        [UnityTest] public IEnumerator BackAgainRestoresCapturedPauseScale()
        {
            yield return Boot(); yield return Tap(MainButton("shell.play")); app.Back(); app.Back(); app.Back();
            yield return new WaitForSecondsRealtime(.15f); Assert.That(Time.timeScale,Is.EqualTo(.4f)); Assert.That(game.Resumes,Is.EqualTo(1)); Assert.That(app.Screens.Current,Is.SameAs(app.GameRoot));
        }
        [UnityTest] public IEnumerator CheckInTapBackAndCachedNavigation()
        {
            yield return Boot();
            Assert.That(Assert.Throws<InvalidOperationException>(()=>app.Navigate(ShellScreen.Shop)).Message,Does.Contain("Shop"));
            Assert.That(Assert.Throws<InvalidOperationException>(()=>app.Navigate(ShellScreen.Leaderboard)).Message,Does.Contain("Leaderboard"));
            yield return Tap(MainButton("shell.checkin.title")); var check=app.Screens.Current;
            Assert.That(check.name,Is.EqualTo("CheckIn")); yield return Tap(check.Find("Shell Back").GetComponent<Button>());
            Assert.That(app.Screens.Current,Is.SameAs(app.Main.Root)); app.Navigate(ShellScreen.CheckIn); Assert.That(app.Screens.Current,Is.SameAs(check));
            app.Back(); Assert.That(app.Screens.Current,Is.SameAs(app.Main.Root));
        }
        [UnityTest] public IEnumerator MandatoryPopupConsumesBackOnServiceScreen()
        {
            yield return Boot(); app.Navigate(ShellScreen.CheckIn); var screen=app.Screens.Current; app.Flow.ShowLose(); app.Back();
            yield return new WaitForSecondsRealtime(.1f); Assert.That(app.Screens.Current,Is.SameAs(screen)); Assert.That(app.Popups.Top,Is.SameAs(app.Flow.Popups[SessionPopupKind.Lose].View));
        }
        [UnityTest] public IEnumerator CorruptedSaveFailsClosedAndPreservesBytes()
        {
            File.WriteAllText(path,"broken bytes"); var bytes=File.ReadAllBytes(path); var result=app.Boot(config);
            Assert.That(result.Success,Is.False); Assert.That(app.Ready,Is.False); Assert.That(app.MainPresenter,Is.Null); Assert.That(app.Main,Is.Null); Assert.That(app.ErrorLabel.text,Is.Not.Empty);
            Assert.That(app.ErrorLabel.gameObject.activeInHierarchy,Is.True); Assert.That(app.Commit().Success,Is.False);
            yield return new WaitForSecondsRealtime(.8f); Assert.That(File.ReadAllBytes(path),Is.EqualTo(bytes)); Assert.That(File.Exists(path+".tmp"),Is.False);
        }
        [UnityTest] public IEnumerator RepeatedBootThrowsAndExistingEventSystemSurvives()
        {
            if(EventSystem.current==null) external=new GameObject("External EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
            var existing=EventSystem.current; yield return Boot(); Assert.That(EventSystem.current,Is.SameAs(existing));
            Assert.Throws<InvalidOperationException>(()=>app.Boot(config)); UnityEngine.Object.Destroy(app.gameObject); yield return null; yield return null;
            Assert.That(existing!=null,Is.True); Assert.That(Count<Canvas>(),Is.EqualTo(canvases)); Assert.That(Count<EventSystem>(),Is.EqualTo(systems+(external!=null?1:0)));
        }
        [UnityTest] public IEnumerator DestroyMidPauseRestoresScaleAndObjectBaseline()
        {
            yield return Boot(); yield return Tap(MainButton("shell.play")); app.Back(); Assert.That(Time.timeScale,Is.Zero);
            UnityEngine.Object.Destroy(app.gameObject); yield return null; yield return null;
            Assert.That(Time.timeScale,Is.EqualTo(.4f)); Assert.That(Count<Canvas>(),Is.EqualTo(canvases)); Assert.That(Count<EventSystem>(),Is.EqualTo(systems)); Assert.That(Count<RectTransform>(),Is.EqualTo(objects));
        }
        [UnityTest] public IEnumerator DestroyDuringLoadingAndFactoryFailureCleanUp()
        {
            Assert.That(app.Boot(config).Success,Is.True); UnityEngine.Object.Destroy(app.gameObject); yield return null; yield return null;
            Assert.That(Count<Canvas>(),Is.EqualTo(canvases)); Assert.That(Count<EventSystem>(),Is.EqualTo(systems));
            app=new GameObject("Factory Failure").AddComponent<ShellApp>(); config.GameScreenFactory=c=>throw new InvalidOperationException("factory failed");
            var result=app.Boot(config); Assert.That(result.Success,Is.False); Assert.That(result.Error,Does.Contain("factory failed"));
            yield return null; yield return null; Assert.That(Count<Canvas>(),Is.EqualTo(canvases)); Assert.That(Count<EventSystem>(),Is.EqualTo(systems)); Assert.That(Count<RectTransform>(),Is.EqualTo(objects));
        }
        [UnityTest] public IEnumerator InjectedClockControlsLoadingAndBackBarrier()
        {
            var clock=new ScreenClock(); config.Clock=clock; Assert.That(app.Boot(config).Success,Is.True);
            app.Back(); yield return null; Assert.That(app.Ready,Is.False); Assert.That(app.Popups.Top,Is.Null);
            clock.UtcNow=clock.UtcNow.AddSeconds(2); yield return null; yield return null; Assert.That(app.Ready,Is.True);
            app.Back(); Assert.That(app.Screens.Current,Is.SameAs(app.Main.Root));
        }
        [UnityTest] public IEnumerator OptionalScreensAndCommitRetryDoNotReplayPlay()
        {
            var files=new ScreenFiles(); config.SaveStore=new SaveStore(fileSystem:files);
            config.ShopCatalog=ScreenTestData.Catalog(); config.Purchases=new FakePurchaseService(); config.Leaderboard=new FakeLeaderboardService("me",new FakeLeaderboardState()); config.BoardId="board";
            yield return Boot(); foreach(var screen in new[]{ShellScreen.Quests,ShellScreen.Shop,ShellScreen.Leaderboard}) { app.Navigate(screen); Assert.That(app.Screens.Current.name,Is.EqualTo(screen.ToString())); app.Back(); }
            files.Fail=true; yield return Tap(MainButton("shell.play")); Assert.That(game.Plays,Is.EqualTo(1)); Assert.That(app.Commit().Success,Is.False); Assert.That(app.ErrorLabel.text,Is.Not.Empty);
            files.Fail=false; Assert.That(app.Commit().Success,Is.True); Assert.That(game.Plays,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator KeyboardBackClosesOnlyOnePopupLayer()
        {
            yield return Boot(); yield return Tap(MainButton("shell.play")); app.Back();
            yield return new WaitForSecondsRealtime(.1f);
            yield return Tap(app.Flow.Popups[SessionPopupKind.Pause].Buttons["leave"]);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape)); yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState()); yield return new WaitForSecondsRealtime(.15f);
            Assert.That(app.Popups.Top,Is.SameAs(app.Flow.Popups[SessionPopupKind.Pause].View));
            Assert.That(Time.timeScale,Is.Zero); Assert.That(game.Resumes,Is.Zero);
        }
        [UnityTest] public IEnumerator HomeTabsPlayAndClaimUseRealPointerInput()
        {
            config.Style=demos.MakeChunky(); yield return Boot();
            Assert.That(app.GetComponentInChildren<GraphicRaycaster>(),Is.Not.Null);
            var chrome=app.Screens.ChromeRoot;
            Button Tab(string key)=>chrome.Find("TabBar/Tabs/Tab "+key).GetComponent<Button>();
            yield return Tap(Tab("quests")); Assert.That(app.Screens.Current.name,Is.EqualTo("Quests")); Assert.That(chrome.gameObject.activeInHierarchy,Is.True);
            app.Navigate(ShellScreen.CheckIn);
            Assert.That(chrome.Find("TabBar/Tabs/Tab checkin/Cell").GetComponent<ShellAppearance>().Selected,Is.True);
            yield return Tap(Tab("home")); Assert.That(app.Screens.Current,Is.SameAs(app.Main.Root));
            yield return Tap(Tab("checkin")); var claim=app.Screens.Current.GetComponentsInChildren<Button>().Single(b=>b.name!="Shell Back");
            var gold=app.Session.Economy.Gold; yield return Tap(claim); yield return Tap(claim);
            Assert.That(app.Session.Economy.Gold,Is.EqualTo(gold+1));
            Assert.That(chrome.Find("TabBar/Tabs/Tab checkin/Icon/Badge").gameObject.activeSelf,Is.False);
            app.Back(); yield return Tap(MainButton("shell.play")); Assert.That(game.Plays,Is.EqualTo(1));
            Assert.That(chrome.Find("TabBar").gameObject.activeSelf,Is.False); Assert.That(chrome.Find("TopBar").gameObject.activeSelf,Is.True);
            Assert.That(app.GameHud,Is.Not.Null); Assert.That(app.Audio,Is.Not.Null); Assert.That(app.Haptics,Is.Not.Null); yield return Tap(chrome.Find("TopBar/Shell Settings").GetComponent<Button>()); Assert.That(app.Popups.Top,Is.Not.Null); app.Back(); yield return new WaitForSecondsRealtime(.15f);
            app.ShowMain(); Assert.That(chrome.Find("TabBar").gameObject.activeSelf,Is.True);
        }
        [UnityTest] public IEnumerator GameViewportMapsScreenPointsToWorldAndFollowsGameScreen()
        {
            config.Style=demos.MakeChunky(); yield return Boot();
            var viewport=app.GameHud.Viewport; Assert.That(viewport.Camera.enabled,Is.False);
            app.ShowGame(); yield return null; Canvas.ForceUpdateCanvases();
            Assert.That(viewport.Camera.enabled,Is.True);
            var area=(RectTransform)app.GameHud.Frame.Find("Game viewport");
            Vector2 Screen(Vector2 normalized)=>RectTransformUtility.WorldToScreenPoint(null,area.TransformPoint(area.rect.min+Vector2.Scale(area.rect.size,normalized)));
            Assert.That(viewport.TryScreenToWorld(Screen(new Vector2(.5f,.5f)),out var centre),Is.True); var origin=viewport.Content.position; Assert.That(centre.x,Is.EqualTo(origin.x).Within(.05f)); Assert.That(centre.y,Is.EqualTo(origin.y).Within(.05f));
            Assert.That(viewport.TryScreenToWorld(Screen(new Vector2(1,1)),out var corner),Is.True);
            Assert.That(corner.y-origin.y,Is.EqualTo(viewport.Camera.orthographicSize).Within(.05f)); Assert.That(corner.x-origin.x,Is.EqualTo(viewport.Camera.orthographicSize*viewport.Camera.aspect).Within(.05f));
            Assert.That(viewport.TryScreenToWorld(Screen(new Vector2(1.5f,.5f)),out _),Is.False);
            app.ShowMain(); yield return null; Assert.That(viewport.Camera.enabled,Is.False);
        }
        void SetPortrait()
        {
#if UNITY_EDITOR
            var assembly=typeof(UnityEditor.Editor).Assembly; var sizesType=assembly.GetType("UnityEditor.GameViewSizes");
            var singleton=typeof(UnityEditor.ScriptableSingleton<>).MakeGenericType(sizesType); var sizes=singleton.GetProperty("instance").GetValue(null);
            var group=sizesType.GetProperty("currentGroup").GetValue(sizes); var sizeType=assembly.GetType("UnityEditor.GameViewSize"); var kind=Enum.Parse(assembly.GetType("UnityEditor.GameViewSizeType"),"FixedResolution");
            int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null),index=-1;
            for(int i=0;i<count;i++) { var size=group.GetType().GetMethod("GetGameViewSize").Invoke(group,new object[]{i}); if((int)sizeType.GetProperty("width").GetValue(size)==720 && (int)sizeType.GetProperty("height").GetValue(size)==1600) { index=i; break; } }
            if(index<0) { var size=Activator.CreateInstance(sizeType,new object[]{kind,720,1600,"Shell tests portrait"}); group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size}); index=count; }
            gameView=UnityEditor.EditorWindow.GetWindow(assembly.GetType("UnityEditor.GameView")); previousSize=(int)gameView.GetType().GetProperty("selectedSizeIndex").GetValue(gameView); gameView.GetType().GetProperty("selectedSizeIndex").SetValue(gameView,index); gameView.Repaint();
#endif
        }
    }
}
