using System;
namespace IndieGame.Shell.Unity.Screens
{
    public sealed class MainPresenter : IDisposable
    {
        readonly ShellSession session; readonly MainView view; readonly ScreenText text; readonly IClock clock;
        readonly IGameSessionActions game; readonly Action<ShellScreen> open;
        long second=-1; string language;
        public MainPresenter(ShellSession session, MainView view, LocalizationService localization, IClock clock, IGameSessionActions game, Action<ShellScreen> open)
        {
            this.session=session; this.view=view; this.clock=clock; this.game=game; this.open=open; text=new ScreenText(session,localization);
            view.PlayRequested+=Play; view.OpenRequested+=Open; view.Lifetime.Entered+=Enter; view.Lifetime.Resumed+=Resume; view.Lifetime.Ticked+=Tick; Enter();
        }
        public void Enter() => Refresh();
        public void Resume() => Refresh();
        public void Tick() { if(clock.UtcNow.Ticks/TimeSpan.TicksPerSecond!=second || language!=session.Settings.Language) Refresh(); }
        public void Refresh()
        {
            second=clock.UtcNow.Ticks/TimeSpan.TicksPerSecond; language=session.Settings.Language;
            var result=session.Lives.Refresh(); if(result.Success) RetrySave(); else view.ShowError(text.Get("shell.error."+result.Error));
            var full=session.Lives.FullAtUtc;
            view.Render(new MainDisplay { Title=text.Get("shell.main.title"), Gold=text.Get("shell.gold",session.Economy.Gold),Hearts=text.Get("shell.hearts",session.Lives.Hearts),
                Countdown=full.HasValue?text.Get("shell.lives.countdown",Math.Max(0,(long)Math.Ceiling((full.Value-clock.UtcNow).TotalSeconds))):text.Get("shell.lives.full"),
                Play=text.Get("shell.play"),CheckIn=text.Get("shell.checkin.title"),Quests=text.Get("shell.quests.title"),Shop=text.Get("shell.shop.title") });
        }
        void Play() { game.Play(); RetrySave(); Refresh(); }
        void Open(ShellScreen screen) { open(screen); Refresh(); }
        public void RetrySave() => text.Commit(view.ShowError);
        public void Dispose() { view.PlayRequested-=Play; view.OpenRequested-=Open; view.Lifetime.Entered-=Enter; view.Lifetime.Resumed-=Resume; view.Lifetime.Ticked-=Tick; }
    }
}
