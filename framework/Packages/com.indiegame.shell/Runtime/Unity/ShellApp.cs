using System;
using System.Collections;
using System.Collections.Generic;
using IndieGame.Shell.Unity.Screens;
using IndieGame.Shell.Unity.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
namespace IndieGame.Shell.Unity
{
    // Explicitly booted by the game. No static startup hook or scene singleton.
    public sealed class ShellApp : MonoBehaviour
    {
        public bool Ready { get; private set; }
        public ShellSession Session { get; private set; }
        public LocalizationService Text { get; private set; }
        public ScreenHost Screens { get; private set; }
        public PopupHost Popups { get; private set; }
        public SessionPopupPresenter Flow { get; private set; }
        public MainView Main { get; private set; }
        public RectTransform HomeStage { get; private set; }
        public RectTransform GameRoot { get; private set; }
        public GameHudView GameHud { get; private set; }
        public IAudioService Audio { get; private set; }
        public IHapticsPlayer Haptics { get; private set; }
        public TMP_Text ErrorLabel { get; private set; }
        public IBoosterInventory Boosters { get; private set; }
        public MainPresenter MainPresenter { get; private set; }
        readonly List<IDisposable> owned = new List<IDisposable>();
        readonly Dictionary<ShellScreen,RectTransform> cached = new Dictionary<ShellScreen,RectTransform>();
        TabModals tabModals;
        ShellAppConfig config; ShellUiFactory factory; ScreenText screenText;
        GameObject canvas, events; RewardFlyLayer rewardFly; bool booted, loaded, destroyed; float capturedScale;
        SessionResult loadResult;
        public BootResult Boot(ShellAppConfig configuration)
        {
            if(booted) throw new InvalidOperationException("ShellApp Boot already called");
            booted=true; capturedScale=Time.timeScale;
            try
            {
                ShellAppConfig.Require(configuration,"ShellAppConfig"); configuration.Validate(); config=configuration;
                var clock=config.Clock??new SystemClock();
                Text=new LocalizationService(new LocalizationConfig(config.Settings.DefaultLanguage));
                foreach(var table in config.LanguageTables)
                {
                    if(table.Value==null) throw new InvalidOperationException("Missing language table: "+table.Key);
                    var shellDefaults=Resources.Load<TextAsset>("ShellLocalization/"+table.Key);
                    var result=Text.LoadLayeredTable(table.Key,shellDefaults!=null?shellDefaults.text:null,table.Value.text);
                    if(!result.Success) throw new InvalidOperationException("Invalid language table: "+table.Key+": "+result.Error);
                }
                factory=new ShellUiFactory(config.Style){ BoosterIcons=config.BoosterIcons??new Dictionary<string,Sprite>() };
                canvas=new GameObject("Shell Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
                canvas.transform.SetParent(transform,false); canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
                var scaler=canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution=new Vector2(720,1600); scaler.matchWidthOrHeight=0;
                Session=new ShellSession(config.SaveStore??new SaveStore(),config.SavePath,clock,config.Lives,config.Economy,config.CheckIn,config.Quests,config.Settings,config.RemoteConfig);
                Boosters=new CommittingBoosters(Session.Economy,this); screenText=new ScreenText(Session,Text); loadResult=Session.LoadAll();
                if(FindFirstObjectByType<AudioListener>()==null) gameObject.AddComponent<AudioListener>();
                var audioPool=new UnityAudioSourcePool(transform,config.Sounds??new Dictionary<string,AudioClip>()); owned.Add(audioPool);
                Audio=new AudioService(Session.Settings,audioPool); Haptics=new HapticsGate(Session.Settings,new UnityHapticsPlayer());
                ErrorLabel=factory.Label(canvas.transform,""); ErrorLabel.name="Shell Error";
                ErrorLabel.rectTransform.anchorMin=ErrorLabel.rectTransform.anchorMax=new Vector2(.5f,0);
                ErrorLabel.rectTransform.anchoredPosition=new Vector2(0,35);
                if(!loadResult.Success)
                {
                    ErrorLabel.text=screenText.Get("shell.error."+loadResult.Error);
                    if(loadResult.ModuleErrors.Count>0) foreach(var error in loadResult.ModuleErrors) ErrorLabel.text+="\n"+screenText.Get("shell.error."+error.Value);
                    // Retain only the error surface. No presenter, input system, or commit exists.
                    return new BootResult(false,ErrorLabel.text);
                }
                loaded=true;
                if(EventSystem.current==null && FindFirstObjectByType<EventSystem>()==null)
                { events=new GameObject("Shell Input",typeof(EventSystem),typeof(InputSystemUIInputModule)); events.transform.SetParent(transform,false); }
                var backdrop=factory.AppearanceImage(canvas.transform,"Backdrop","screen"); ShellUiFactory.Stretch(backdrop.rectTransform); backdrop.preserveAspect=false; backdrop.transform.SetAsFirstSibling();
                Screens=canvas.AddComponent<ScreenHost>(); Screens.Initialize(factory); Screens.Loading.Initialize(clock);
                var popupRoot=ShellUiFactory.Node("Safe Popups",canvas.transform); ShellUiFactory.Stretch(popupRoot);
                popupRoot.gameObject.AddComponent<SafeAreaFitter>(); popupRoot.gameObject.SetActive(true);
                Popups=popupRoot.gameObject.AddComponent<PopupHost>(); Popups.HandlesOwnBackKey=false;
                if(config.Style.ModalTabs) tabModals=new TabModals(Popups,factory);
                var actions=new ActionsBridge(this,config.Actions);
                rewardFly=RewardFlyLayer.Create(factory,canvas.transform);
                Flow=new SessionPopupPresenter(Session,factory,Popups,Text,actions,config.RewardedAds,config.SessionPopups,rewardFly,config.WinCelebration,config.LoseCelebration); owned.Add(Flow);
                GameRoot=config.GameScreenFactory(new ShellGameContext { Factory=factory,Session=Session,Text=Text,Popups=Popups,Flow=Flow,Parent=Screens.SafeRoot,Navigate=Navigate,ShowMain=ShowMain });
                if(destroyed || this==null) throw new InvalidOperationException("ShellApp destroyed during Boot");
                if(GameRoot==null) throw new InvalidOperationException("GameScreenFactory returned null");
                GameRoot.SetParent(Screens.SafeRoot,false); GameRoot.gameObject.SetActive(false);
                GameHud=new GameHudView(factory,GameRoot,config.Style.EnableHomeChrome); owned.Add(GameHud);
                if(!config.Style.EnableHomeChrome) {
                    var pause=factory.IconButton(GameRoot,"pause",screenText.Get("shell.pause"),()=>Flow.ShowPause(),accessibilityKey:"pause");
                    var pr=(RectTransform)pause.transform; pr.anchorMin=pr.anchorMax=new Vector2(1,1); pr.pivot=new Vector2(1,1); pr.anchoredPosition=ShellLayout.Standard.HeaderRightPosition;
                    var lifetime=GameRoot.GetComponent<ScreenLifetime>()??GameRoot.gameObject.AddComponent<ScreenLifetime>();
                    void RefreshPauseName()=>pause.GetComponent<ActionAccessibility>().Set("pause",screenText.Get("shell.pause"));
                    lifetime.Entered+=RefreshPauseName; lifetime.Ticked+=RefreshPauseName;
                }
                Main=new MainView(factory,Screens.SafeRoot);
                if(config.HomeStageFactory!=null) HomeStage=HomeStageBuilder.Create(Main.Root,factory,config.HomeStageFactory);
                MainPresenter=new MainPresenter(Session,Main,Text,clock,actions,Navigate,config.PlayLabel); owned.Add(MainPresenter);
                if(config.Style.EnableHomeChrome)
                {
                    var chrome=new ChromePresenter(Session,Text,clock,factory,Screens,Main.Root,GameRoot,ShowMain,Navigate,()=>{ if(!Ready || Popups.Top!=null) return; if(Screens.Current==GameRoot) Flow.ShowPause(); else Flow.ShowSettings(); },error=>ErrorLabel.text=error,tabModals,rewardFly,()=>Popups.Top!=null);
                    owned.Add(chrome); rewardFly.SetTarget(RewardPart.GoldIcon,chrome.TopBar.GoldIcon); rewardFly.SetTarget(RewardPart.HeartIcon,chrome.TopBar.HeartIcon);
                }
                Screens.Show(Main.Root);
                Screens.Loading.gameObject.AddComponent<LoadingContentView>().Initialize(factory,Screens.Loading,screenText,"shell.loading.tip");
                Screens.Loading.transform.SetAsLastSibling(); Screens.Loading.Show(); Screens.Loading.SetProgress(1); Screens.Loading.Hide();
                StartCoroutine(FinishBoot());
                return new BootResult(true);
            }
            catch(Exception ex) { Cleanup(); return new BootResult(false,ex.Message); }
        }
        IEnumerator FinishBoot()
        {
            while(Screens!=null && Screens.Loading.Controller.State!=LoadingScreenState.Hidden) yield return null;
            if(!destroyed && loaded) Ready=true;
        }
        void RequireReady() { if(!Ready) throw new InvalidOperationException("ShellApp not Ready"); }
        public void ShowMain() { RequireReady(); tabModals?.CloseActive(); Screens.Show(Main.Root); }
        public void ShowGame() { RequireReady(); tabModals?.CloseActive(); Screens.Show(GameRoot); }
        public void OpenShopOverGame()
        {
            RequireReady();
            if(tabModals==null) throw new InvalidOperationException("OpenShopOverGame needs ShellStyle.ModalTabs");
            if(Screens.Current==GameRoot) Flow.ShowPause();
            OpenScreen(ShellScreen.Shop,true);
        }
        public void Navigate(ShellScreen screen)=>OpenScreen(screen,false);
        void OpenScreen(ShellScreen screen,bool keepScreen)
        {
            if(!Enum.IsDefined(typeof(ShellScreen),screen)) throw new InvalidOperationException("Unavailable ShellScreen: "+screen);
            if(screen==ShellScreen.Shop && config?.ShopFactory==null && config?.ShopCatalog==null) throw new InvalidOperationException("Unavailable ShellScreen Shop: Shop service");
            if(screen==ShellScreen.Leaderboard && config?.Leaderboard==null) throw new InvalidOperationException("Unavailable ShellScreen Leaderboard: Leaderboard service");
            RequireReady();
            bool modal=tabModals!=null;
            RectTransform root=null;
            if(modal ? !tabModals.HasFrame(screen) : !cached.TryGetValue(screen,out root))
            {
                var frame=modal?tabModals.CreateFrame(screen,screenText.Get("shell.close")):null;
                Transform parent=modal?Popups.transform:Screens.SafeRoot; var clock=config.Clock??new SystemClock();
                switch(screen)
                {
                    case ShellScreen.CheckIn:
                        var check=new CheckInView(factory,parent,frame); owned.Add(new CheckInPresenter(Session,check,Text,clock,rewardFly)); root=check.Root; break;
                    case ShellScreen.Quests:
                        var quests=new QuestsView(factory,parent,frame); owned.Add(new QuestsPresenter(Session,quests,Text,clock,rewardFly)); root=quests.Root; break;
                    case ShellScreen.Shop:
                        var shop=new ShopView(factory,parent,Popups,frame);
                        var service=config.ShopFactory!=null?config.ShopFactory(Session):new ShopService(config.ShopCatalog,Session,config.Purchases);
                        if(service==null) throw new InvalidOperationException("Unavailable ShellScreen Shop: ShopFactory returned null");
                        owned.Add(new ShopPresenter(Session,service,shop,Text)); root=shop.Root; break;
                    case ShellScreen.Leaderboard:
                        var board=new LeaderboardView(factory,parent,frame,config.LeaderboardCaption); owned.Add(new LeaderboardPresenter(Session,board,Text,config.Leaderboard,config.BoardId,config.LeaderboardRows,config.LeaderboardLocalBest,config.Leaderboard as ILeaderboardRename)); root=board.Root; break;
                    default: throw new InvalidOperationException("Unavailable ShellScreen: "+screen);
                }
                if(!modal)
                {
                    var back=factory.IconButton(root,"back",screenText.Get("shell.back"),Back);
                    var rect=(RectTransform)back.transform; rect.anchorMin=rect.anchorMax=new Vector2(0,1); rect.pivot=new Vector2(0,1); rect.anchoredPosition=ShellLayout.Standard.BackPosition;
                    cached.Add(screen,root);
                }
            }
            if(modal) { if(!keepScreen && Screens.Current!=Main.Root) Screens.Show(Main.Root); tabModals.Open(screen); return; }
            foreach(var a in root.GetComponentsInChildren<ActionAccessibility>(true)) if(a.Key=="back") a.Set("back",screenText.Get("shell.back")); Screens.Show(root);
        }
        public SessionResult Commit()
        {
            if(!loaded) { if(loadResult!=null && !loadResult.Success) return loadResult; throw new InvalidOperationException("ShellApp session not loaded"); }
            var result=Session.Commit(); ErrorLabel.transform.SetAsLastSibling(); if(!result.Success) ErrorLabel.text=screenText.CommitError(result); else ErrorLabel.text="";
            return result;
        }
        void Update()
        {
            var keyboard=Keyboard.current;
            if(keyboard!=null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.backspaceKey.wasPressedThisFrame)) Back();
        }
        public void Back()
        {
            if(!Ready || Screens.Loading.Controller.State!=LoadingScreenState.Hidden) return;
            if(Popups.Stack.Count>0) { Popups.Back(); return; }
            if(Screens.Current==GameRoot) { Flow.ShowPause(); return; }
            if(Screens.Current!=Main.Root) ShowMain();
        }
        void OfferHearts()
        {
            if(config.ShopFactory!=null||config.ShopCatalog!=null) Navigate(ShellScreen.Shop);
        }
        sealed class CommittingBoosters : IBoosterInventory
        {
            readonly IBoosterInventory inner; readonly ShellApp app;
            internal CommittingBoosters(IBoosterInventory inner,ShellApp app) { this.inner=inner; this.app=app; }
            public int Count(string id)=>inner.Count(id);
            public event Action Changed { add { inner.Changed+=value; } remove { inner.Changed-=value; } }
            public bool Use(string id)
            {
                if(!inner.Use(id)) return false;
                app.Commit(); return true;
            }
        }
        sealed class ActionsBridge : IGameSessionActions
        {
            readonly ShellApp app; readonly IGameSessionActions game;
            internal ActionsBridge(ShellApp app,IGameSessionActions game) { this.app=app; this.game=game; }
            public bool Play()
            {
                app.RequireReady();
                if(!app.Session.Attempt.CanStart()) { app.OfferHearts(); return false; }
                if(!game.Play()) return false;
                app.Session.Attempt.Begin(); app.ShowGame(); return true;
            }
            public void Retry() { if(CanStartOrOffer()) { game.Retry(); app.Session.Attempt.Begin(); } }
            public void NextLevel() { if(CanStartOrOffer()) { game.NextLevel(); app.Session.Attempt.Begin(); } }
            public void Revive()=>game.Revive();
            public void Leave()
            {
                bool spent=app.Session.Attempt.Left();
                game.Leave(); app.ShowMain();
                if(spent) app.Commit();
            }
            bool CanStartOrOffer()
            {
                if(app.Session.Attempt.CanStart()) return true;
                Leave(); app.OfferHearts(); return false;
            }
            public void Pause()=>game.Pause(); public void Resume()=>game.Resume();
        }
        void OnDestroy() { destroyed=true; Cleanup(); }
        void Cleanup()
        {
            Ready=false; if(this!=null) StopAllCoroutines();
            for(int i=owned.Count-1;i>=0;i--) { try { owned[i].Dispose(); } catch(Exception) { /* continue releasing owned objects */ } }
            owned.Clear(); cached.Clear(); tabModals=null; loaded=false;
            if(GameRoot!=null && (canvas==null || !GameRoot.IsChildOf(canvas.transform))) Release(GameRoot.gameObject);
            Release(canvas); Release(events); canvas=events=null;
            if(booted) Time.timeScale=capturedScale;
        }
        static void Release(GameObject value) { if(value==null) return; value.SetActive(false); if(Application.isPlaying) Destroy(value); else DestroyImmediate(value); }
    }
}
