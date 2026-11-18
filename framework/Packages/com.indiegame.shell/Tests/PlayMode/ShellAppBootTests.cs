using IndieGame.Shell.TestKit;
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
        readonly List<InputDevice> isolatedDevices=new List<InputDevice>();
        InputSettings.BackgroundBehavior priorBackground;
        InputSettings.UpdateMode priorUpdateMode;
#if UNITY_EDITOR
        UnityEditor.EditorWindow gameView; int previousSize;
#endif
        [SetUp] public void Setup()
        {
            SetPortrait(); demos=new DemoStyles(); game=new Game(); priorScale=Time.timeScale; Time.timeScale=.4f;
            canvases=Count<Canvas>(); systems=Count<EventSystem>(); objects=Count<RectTransform>();
            priorBackground=InputSystem.settings.backgroundBehavior; priorUpdateMode=InputSystem.settings.updateMode;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.updateMode=InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
            foreach(var device in InputSystem.devices.ToArray()) if(device.enabled) { isolatedDevices.Add(device); InputSystem.DisableDevice(device); }
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
        [UnityTest] public IEnumerator StandardConfigBootsShellApp()
        {
            config=ShellAppConfig.Standard(ShellGameConfig.Parse("{\"gameId\":\"standard-smoke\"}"),
                config.Style,new Dictionary<string,TextAsset>(config.LanguageTables),game,
                c=>c.Factory.ScreenRoot(c.Parent,"Empty Game"),path);
            yield return Boot();
            Assert.That(app.Screens.Current,Is.SameAs(app.Main.Root));
            app.ShowGame(); Assert.That(app.Screens.Current,Is.SameAs(app.GameRoot));
            app.ShowMain(); Assert.That(app.Screens.Current,Is.SameAs(app.Main.Root));
        }
        static int Count<T>() where T:UnityEngine.Object =>UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length;
        [TearDown] public void Cleanup()
        {
            if(app!=null) UnityEngine.Object.DestroyImmediate(app.gameObject);
            if(external!=null) UnityEngine.Object.DestroyImmediate(external);
            InputSystem.RemoveDevice(mouse); InputSystem.RemoveDevice(keyboard);
            foreach(var device in isolatedDevices) if(device.added) InputSystem.EnableDevice(device);
            isolatedDevices.Clear(); InputSystem.settings.backgroundBehavior=priorBackground; InputSystem.settings.updateMode=priorUpdateMode;
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
            yield return null; yield return null;
        }
        Button MainButton(string key)=>app.Main.Root.GetComponentsInChildren<Button>().Single(b=>(b.GetComponentInChildren<TMP_Text>()?.text??b.GetComponent<ActionAccessibility>()?.Label)==app.Text.Format("en",key).Value);
        static IEnumerator Until(Func<bool> condition,string message)
        {
            float deadline=Time.realtimeSinceStartup+10;
            while(!condition()&&Time.realtimeSinceStartup<deadline) yield return null;
            Assert.That(condition(),Is.True,message);
        }
        IEnumerator Settled()
        {
            yield return null; yield return null;
            yield return Until(()=>app.Popups.Stack.All(p=>p.Group.alpha>=.999f&&Vector3.Distance(p.Panel.localScale,Vector3.one)<.001f),"popup transitions completed");
        }
        [UnityTest] public IEnumerator RealPointerConsumesConsecutiveClicksWhilePaused()
        {
            yield return Boot(); Time.timeScale=0;
            var play=MainButton("shell.play"); int clicks=0;
            play.onClick.AddListener(()=>clicks++);
            yield return new RealPointer(mouse).Click(play);
            Assert.That(clicks,Is.EqualTo(1)); Assert.That(game.Plays,Is.EqualTo(1));
            app.ShowMain();
            yield return new RealPointer(mouse).Click(play);
            Assert.That(clicks,Is.EqualTo(2)); Assert.That(game.Plays,Is.EqualTo(2));
            Assert.That(mouse.leftButton.isPressed,Is.False);
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
            yield return Boot(); yield return new RealPointer(mouse).Click(MainButton("shell.play"));
            Assert.That(game.Plays,Is.EqualTo(1)); Assert.That(app.Screens.Current,Is.SameAs(app.GameRoot));
            app.Back(); Assert.That(Time.timeScale,Is.Zero); Assert.That(game.Pauses,Is.EqualTo(1)); yield return new WaitForSecondsRealtime(.1f);
            yield return new RealPointer(mouse).Click(app.Flow.Popups[SessionPopupKind.Pause].Buttons["leave"]);
            yield return new RealPointer(mouse).Click(app.Flow.Popups[SessionPopupKind.Leave].Buttons["cancel"]);
            Assert.That(app.Popups.Top,Is.SameAs(app.Flow.Popups[SessionPopupKind.Pause].View)); Assert.That(Time.timeScale,Is.Zero); Assert.That(game.Resumes,Is.Zero);
            yield return new RealPointer(mouse).Click(app.Flow.Popups[SessionPopupKind.Pause].Buttons["leave"]);
            yield return new RealPointer(mouse).Click(app.Flow.Popups[SessionPopupKind.Leave].Buttons["confirm"]);
            Assert.That(app.Screens.Current,Is.SameAs(app.Main.Root)); Assert.That(game.Leaves,Is.EqualTo(1)); Assert.That(Time.timeScale,Is.EqualTo(.4f));
        }
        [UnityTest] public IEnumerator BackAgainRestoresCapturedPauseScale()
        {
            yield return Boot(); yield return new RealPointer(mouse).Click(MainButton("shell.play")); app.Back(); app.Back(); app.Back();
            yield return new WaitForSecondsRealtime(.15f); Assert.That(Time.timeScale,Is.EqualTo(.4f)); Assert.That(game.Resumes,Is.EqualTo(1)); Assert.That(app.Screens.Current,Is.SameAs(app.GameRoot));
        }
        [UnityTest] public IEnumerator CheckInTapBackAndCachedNavigation()
        {
            yield return Boot();
            Assert.That(Assert.Throws<InvalidOperationException>(()=>app.Navigate(ShellScreen.Shop)).Message,Does.Contain("Shop"));
            Assert.That(Assert.Throws<InvalidOperationException>(()=>app.Navigate(ShellScreen.Leaderboard)).Message,Does.Contain("Leaderboard"));
            yield return new RealPointer(mouse).Click(MainButton("shell.checkin.title")); var check=app.Screens.Current;
            Assert.That(check.name,Is.EqualTo("CheckIn")); yield return new RealPointer(mouse).Click(check.GetComponentsInChildren<Button>().Single(b=>b.GetComponent<ActionAccessibility>()?.Key=="back"));
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
            yield return Boot(); yield return new RealPointer(mouse).Click(MainButton("shell.play")); app.Back(); Assert.That(Time.timeScale,Is.Zero);
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
            config.ShopCatalog=ScreenTestData.Catalog(); config.Purchases=new FakePurchaseService(); config.Leaderboard=new LeaderboardRequests(new FakeLeaderboardService("me",new FakeLeaderboardState())); config.BoardId="board";
            yield return Boot(); foreach(var screen in new[]{ShellScreen.Quests,ShellScreen.Shop,ShellScreen.Leaderboard}) { app.Navigate(screen); Assert.That(app.Screens.Current.name,Is.EqualTo(screen.ToString())); app.Back(); }
            files.Fail=true; yield return new RealPointer(mouse).Click(MainButton("shell.play")); Assert.That(game.Plays,Is.EqualTo(1)); Assert.That(app.Commit().Success,Is.False); Assert.That(app.ErrorLabel.text,Is.Not.Empty);
            files.Fail=false; Assert.That(app.Commit().Success,Is.True); Assert.That(game.Plays,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator KeyboardBackClosesOnlyOnePopupLayer()
        {
            yield return Boot(); yield return new RealPointer(mouse).Click(MainButton("shell.play")); app.Back();
            yield return Settled();
            yield return new RealPointer(mouse).Click(app.Flow.Popups[SessionPopupKind.Pause].Buttons["leave"]);
            var leave=app.Flow.Popups[SessionPopupKind.Leave].View;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));
            yield return Until(()=>keyboard.escapeKey.isPressed,"Escape consumed");
            yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());
            yield return Until(()=>!keyboard.escapeKey.isPressed,"Escape released");
            yield return Until(()=>!leave.gameObject.activeSelf,"Leave popup closed");
            yield return Settled();
            Assert.That(app.Popups.Top,Is.SameAs(app.Flow.Popups[SessionPopupKind.Pause].View));
            Assert.That(Time.timeScale,Is.Zero); Assert.That(game.Resumes,Is.Zero);
        }
        [UnityTest] public IEnumerator HomeTabsPlayAndClaimUseRealPointerInput()
        {
            config.Style=demos.MakeChunky(); yield return Boot();
            Assert.That(app.GetComponentInChildren<GraphicRaycaster>(),Is.Not.Null);
            var chrome=app.Screens.ChromeRoot;
            Button Tab(string key)=>chrome.GetComponentsInChildren<ActionAccessibility>(true).Single(a=>a.Key==key).GetComponent<Button>();
            yield return new RealPointer(mouse).Click(Tab("quests")); Assert.That(app.Screens.Current,Is.SameAs(app.Main.Root)); Assert.That(app.Popups.Top.name,Is.EqualTo("Tab Quests")); Assert.That(chrome.gameObject.activeInHierarchy,Is.True);
            app.Navigate(ShellScreen.CheckIn); yield return null;
            Assert.That(Tab("checkin").transform.Find("Cell").GetComponent<ShellAppearance>().Selected,Is.True);
            yield return new RealPointer(mouse).Click(Tab("home")); Assert.That(app.Screens.Current,Is.SameAs(app.Main.Root));
            yield return Until(()=>app.Popups.Top==null,"tab popup closed by Home tab");
            yield return new RealPointer(mouse).Click(Tab("checkin")); var claim=app.Popups.Top.Content.GetComponentsInChildren<Button>().Single();
            var gold=app.Session.Economy.Gold; yield return new RealPointer(mouse).Click(claim);
            Assert.That(claim.IsInteractable(),Is.False,"Claim must disable after collection");
            claim.onClick.Invoke(); // Also exercise the presenter guard against a duplicate callback.
            Assert.That(app.Session.Economy.Gold,Is.EqualTo(gold+1));
            Assert.That(Tab("checkin").transform.Find("Icon/Badge").gameObject.activeSelf,Is.False);
            app.Back(); yield return Until(()=>app.Popups.Top==null,"tab popup closed by Back");
            yield return new RealPointer(mouse).Click(MainButton("shell.play")); Assert.That(game.Plays,Is.EqualTo(1));
            Assert.That(chrome.Find("TabBar").gameObject.activeSelf,Is.False); Assert.That(chrome.Find("TopBar").gameObject.activeSelf,Is.True);
            Assert.That(app.GameHud,Is.Not.Null); Assert.That(app.Audio,Is.Not.Null); Assert.That(app.Haptics,Is.Not.Null); yield return new RealPointer(mouse).Click(chrome.GetComponentsInChildren<Button>().Single(b=>b.GetComponent<ActionAccessibility>()?.Key=="pause")); Assert.That(app.Popups.Top,Is.Not.Null); app.Back(); yield return Settled();
            app.ShowMain(); Assert.That(chrome.Find("TabBar").gameObject.activeSelf,Is.True);
        }
        Button TabButton(string key)=>app.Screens.ChromeRoot.GetComponentsInChildren<ActionAccessibility>(true).Single(a=>a.Key==key).GetComponent<Button>();
        void Swipe(TabSwipeHandler handler,float fromX,float toX)
        {
            float now=0; handler.Clock=()=>now; handler.Width=()=>720;
            var begin=new PointerEventData(EventSystem.current){position=new Vector2(fromX,800),pressPosition=new Vector2(fromX,800)}; handler.OnBeginDrag(begin);
            now=.4f; handler.OnEndDrag(new PointerEventData(EventSystem.current){position=new Vector2(toX,800),pressPosition=new Vector2(fromX,800)});
        }
        [UnityTest] public IEnumerator TabOpensAsPopupOverHomeHighlightsTabAndCloseReturnsHome()
        {
            config.Style=demos.MakeChunky(); yield return Boot();
            var gear=app.Screens.ChromeRoot.Find("TopBar").GetComponentsInChildren<ActionAccessibility>(true); Assert.That(gear.Any(a=>a.Key=="settings"),Is.True,"gear is Settings on Home"); Assert.That(gear.Any(a=>a.Key=="pause"),Is.False);
            yield return new RealPointer(mouse).Click(TabButton("quests"));
            Assert.That(app.Popups.Top.name,Is.EqualTo("Tab Quests")); Assert.That(app.Popups.Stack.Count,Is.EqualTo(1));
            Assert.That(app.Screens.Current,Is.SameAs(app.Main.Root)); Assert.That(app.Main.Root.gameObject.activeInHierarchy,Is.True,"Home stays behind the modal");
            Assert.That(app.Screens.ChromeRoot.Find("TabBar").gameObject.activeSelf,Is.True);
            Assert.That(TabButton("quests").transform.Find("Cell").GetComponent<ShellAppearance>().Selected,Is.True);
            app.Back(); yield return Until(()=>app.Popups.Top==null,"Back closes the tab popup"); yield return null;
            Assert.That(app.Screens.Current,Is.SameAs(app.Main.Root));
            Assert.That(TabButton("home").transform.Find("Cell").GetComponent<ShellAppearance>().Selected,Is.True,"closing returns the highlight to Home");
        }
        [UnityTest] public IEnumerator SwipeSwitchesTheOpenModalToTheNeighbouringTab()
        {
            config.Style=demos.MakeChunky(); yield return Boot();
            app.Navigate(ShellScreen.CheckIn); var handler=app.Popups.Top.GetComponent<TabSwipeHandler>();
            Swipe(handler,500,300);
            yield return Until(()=>app.Popups.Top!=null && app.Popups.Top.name=="Tab Quests","swipe opens the neighbouring tab");
            Assert.That(TabButton("quests").transform.Find("Cell").GetComponent<ShellAppearance>().Selected,Is.True);
            Swipe(app.Popups.Top.GetComponent<TabSwipeHandler>(),300,500); Swipe(app.Popups.Top.GetComponent<TabSwipeHandler>(),300,500);
            yield return Until(()=>app.Popups.Top==null,"swiping right past the first tab returns Home");
        }
        [UnityTest] public IEnumerator EscapeClosesOnlyTheLayerAboveATabPopup()
        {
            config.Style=demos.MakeChunky(); yield return Boot();
            app.Navigate(ShellScreen.CheckIn); var tab=app.Popups.Top; app.Flow.ShowSettings(); var settings=app.Popups.Top; Assert.That(settings,Is.Not.SameAs(tab));
            app.Back(); yield return Until(()=>app.Popups.Top==tab,"Settings closed, tab popup remains");
            Assert.That(app.Popups.Stack.Count,Is.EqualTo(1)); app.Back(); yield return Until(()=>app.Popups.Top==null,"second Back closes the tab popup");
        }
        [UnityTest] public IEnumerator TabsAsModalsFalseKeepsFlatScreensWithBack()
        {
            config.Style=demos.MakeChunky(); config.Style.TabsAsModals=false; yield return Boot();
            app.Navigate(ShellScreen.Quests); Assert.That(app.Screens.Current.name,Is.EqualTo("Quests")); Assert.That(app.Popups.Top,Is.Null);
            app.Back(); Assert.That(app.Screens.Current,Is.SameAs(app.Main.Root));
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
            gameView=UnityEditor.EditorWindow.GetWindow(assembly.GetType("UnityEditor.GameView")); previousSize=(int)gameView.GetType().GetProperty("selectedSizeIndex").GetValue(gameView); gameView.GetType().GetProperty("selectedSizeIndex").SetValue(gameView,index); gameView.Focus(); gameView.Repaint();
#endif
        }
    }
}
