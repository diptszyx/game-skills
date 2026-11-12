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
        readonly Action<string> showError;
        readonly Action home; readonly Action<ShellScreen> navigate;
        readonly ScreenLifetime lifetime;
        public TopBarView TopBar { get; }
        public TabBarView TabBar { get; }
        string language; DateTime day; long second=-1;
        static readonly string[] Keys={"home","checkin","quests","shop","leaderboard"};
        public ChromePresenter(ShellSession session,LocalizationService localization,IClock clock,ShellUiFactory factory,ScreenHost host,RectTransform main,RectTransform game,Action home,Action<ShellScreen> navigate,Action<string> showError = null)
        {
            this.showError=showError??(_=>{}); this.session=session; text=new ScreenText(session,localization); this.clock=clock; this.host=host; this.main=main; this.game=game; this.home=home; this.navigate=navigate;
            TopBar=new TopBarView(factory,host.ChromeRoot);
            TabBar=new TabBarView(factory,host.ChromeRoot,Keys.Select(k=>new TabDefinition(k,Label(k),k)).ToArray());
            host.SetTabBar(TabBar); TabBar.TabSelected+=Selected; host.Shown+=Shown;
            lifetime=host.ChromeRoot.gameObject.AddComponent<ScreenLifetime>(); lifetime.Ticked+=Refresh; lifetime.Resumed+=Refresh;
            Refresh();
        }
        string Label(string key)=>text.Get(key=="home"?"shell.tab.home":"shell."+key+".title");
        public static bool CheckInBadge(ShellSession session)=>session.DailyCheckIn.Preview().CanClaimNow;
        public static bool QuestsBadge(ShellSession session)=>session.DailyQuests.Quests.Any(QuestsPresenter.CanClaim);
        public void Refresh()
        {
            if(clock.UtcNow.Date!=day) { day=clock.UtcNow.Date; var result=session.DailyQuests.Refresh(); if(!result.Success) this.showError(text.Get("shell.error."+result.Error)); }
            if(clock.UtcNow.Ticks/TimeSpan.TicksPerSecond!=second)
            { second=clock.UtcNow.Ticks/TimeSpan.TicksPerSecond; var result=session.Lives.Refresh(); if(result.Success) text.Commit(showError); else showError(text.Get("shell.error."+result.Error)); }
            if(language!=session.Settings.Language)
            { language=session.Settings.Language; foreach(var key in Keys) TabBar.SetLabel(key,Label(key)); }
            TopBar.Render(new TopBarDisplay(session.Economy.Gold,session.Lives.Hearts,MainPresenter.FormatCountdown(session,text,clock)));
            TabBar.SetBadge("checkin",CheckInBadge(session)); TabBar.SetBadge("quests",QuestsBadge(session));
        }
        void Selected(string key) { if(key=="home") home(); else navigate((ShellScreen)(Array.IndexOf(Keys,key)-1)); }
        void Shown(RectTransform screen)
        {
            bool visible=screen!=game; TopBar.Root.gameObject.SetActive(visible); host.ShowTabBar(visible);
            if(visible) TabBar.SetSelected(screen==main?"home":screen.name.ToLowerInvariant());
            Refresh();
        }
        public void Dispose() { TabBar.TabSelected-=Selected; host.Shown-=Shown; lifetime.Ticked-=Refresh; lifetime.Resumed-=Refresh; }
    }
}
