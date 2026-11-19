using System;
using System.Linq;
using IndieGame.Shell.Unity.UI;
using UnityEngine;
namespace IndieGame.Shell.Unity.Screens
{
    // Owned once by ShellApp; never belongs to a replaceable screen.
    public sealed class ChromePresenter : IDisposable
    {
        readonly ShellSession session; readonly ScreenText text; readonly IClock clock;
        readonly ScreenHost host; readonly RectTransform main,game;
        readonly Action<string> showError; readonly TabModals modals;
        readonly Action home; readonly Action<ShellScreen> navigate;
        readonly ScreenLifetime lifetime; readonly RewardFlyLayer fly;
        public TopBarView TopBar { get; }
        public TabBarView TabBar { get; }
        string language; DateTime day; long second=-1;
        static readonly string[] Keys={"home","checkin","quests","shop","leaderboard"};
        public ChromePresenter(ShellSession session,LocalizationService localization,IClock clock,ShellUiFactory factory,ScreenHost host,RectTransform main,RectTransform game,Action home,Action<ShellScreen> navigate,Action settings,Action<string> showError = null,TabModals modals = null,RewardFlyLayer fly = null)
        {
            this.fly=fly;
            this.showError=showError??(_=>{}); this.modals=modals; this.session=session; text=new ScreenText(session,localization); this.clock=clock; this.host=host; this.main=main; this.game=game; this.home=home; this.navigate=navigate;
            TopBar=new TopBarView(factory,host.ChromeRoot);
            TabBar=new TabBarView(factory,host.ChromeRoot,Keys.Select(k=>new TabDefinition(k,Label(k),k)).ToArray());
            TopBar.Settings.onClick.AddListener(()=>settings()); host.SetTabBar(TabBar); TabBar.TabSelected+=Selected; host.Shown+=Shown;
            TabSwipeHandler.Attach(factory.Style,host,TabBar);
            if(modals!=null) { modals.Opened+=SyncSelection; modals.FrameCreated+=frame=>TabSwipeHandler.AttachTo(factory.Style,frame.gameObject,TabBar,()=>TabBar.Root.gameObject.activeInHierarchy&&modals.IsTop(frame)); }
            lifetime=host.ChromeRoot.gameObject.AddComponent<ScreenLifetime>(); lifetime.Ticked+=Refresh; lifetime.Resumed+=Refresh;
            Refresh();
        }
        long Held(string icon)=>fly==null?0:fly.Held(icon);
        string Label(string key)=>text.Get("shell.tab."+key);
        public static bool CheckInBadge(ShellSession session)=>session.DailyCheckIn.Preview().CanClaimNow;
        public static bool QuestsBadge(ShellSession session)=>session.DailyQuests.Quests.Any(QuestsPresenter.CanClaim);
        public void Refresh()
        {
            if(clock.UtcNow.Date!=day) { day=clock.UtcNow.Date; var result=session.DailyQuests.Refresh(); if(!result.Success) this.showError(text.Get("shell.error."+result.Error)); }
            if(clock.UtcNow.Ticks/TimeSpan.TicksPerSecond!=second)
            { second=clock.UtcNow.Ticks/TimeSpan.TicksPerSecond; var result=session.Lives.Refresh(); if(result.Success) text.Commit(showError); else showError(text.Get("shell.error."+result.Error)); }
            if(language!=session.Settings.Language)
            { language=session.Settings.Language; foreach(var key in Keys) TabBar.SetLabel(key,Label(key)); if(modals!=null) modals.SetCloseLabel(text.Get("shell.close")); }
            RefreshGearLabel();
            TopBar.Render(new TopBarDisplay(session.Economy.Gold-Held(RewardPart.GoldIcon),session.Lives.Hearts-(int)Held(RewardPart.HeartIcon),MainPresenter.FormatCountdown(session,text,clock)));
            TabBar.SetBadge("checkin",CheckInBadge(session)); TabBar.SetBadge("quests",QuestsBadge(session));
            SyncSelection();
        }
        void RefreshGearLabel()
        {
            var accessible=TopBar.Settings.GetComponent<ActionAccessibility>()??TopBar.Settings.gameObject.AddComponent<ActionAccessibility>();
            bool inGame=host.Current==game; string key=inGame?"pause":"settings"; string label=text.Get(inGame?"shell.pause":"popup.settings.title");
            if(accessible.Key!=key||accessible.Label!=label) accessible.Set(key,label);
        }
        string SelectedKey()
        {
            if(modals!=null && modals.Active is ShellScreen active) return TabKey(active);
            return host.Current==main?"home":host.Current.name.ToLowerInvariant();
        }
        void SyncSelection()
        { if(host.Current==null||host.Current==game) return; var key=SelectedKey(); if(TabBar.Selected!=key) TabBar.SetSelected(key); }
        internal static string TabKey(ShellScreen screen)=>Keys[(int)screen+1];
        void Selected(string key) { if(key=="home") home(); else navigate((ShellScreen)(Array.IndexOf(Keys,key)-1)); }
        void Shown(RectTransform screen)
        {
            bool visible=screen!=game; TopBar.Root.gameObject.SetActive(screen==main||screen==game); host.ShowTabBar(visible);
            SyncSelection(); Refresh();
        }
        public void Dispose() { if(modals!=null) modals.Opened-=SyncSelection; TabBar.TabSelected-=Selected; host.Shown-=Shown; lifetime.Ticked-=Refresh; lifetime.Resumed-=Refresh; }
    }
}
