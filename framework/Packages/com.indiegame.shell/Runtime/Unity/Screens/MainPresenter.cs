using System;
namespace IndieGame.Shell.Unity.Screens
{
    public sealed class MainPresenter : IDisposable
    {
        readonly ShellSession session; readonly MainView view; readonly ScreenText text; readonly IClock clock;
        readonly Func<string> playLabel;
        readonly IGameSessionActions game; readonly Action<ShellScreen> open;
        long second=-1; string language;
        public MainPresenter(ShellSession session, MainView view, LocalizationService localization, IClock clock, IGameSessionActions game, Action<ShellScreen> open, Func<string> playLabel = null)
        {
            this.playLabel=playLabel; this.session=session; this.view=view; this.clock=clock; this.game=game; this.open=open; text=new ScreenText(session,localization);
            view.PlayRequested+=Play; view.OpenRequested+=Open; view.Lifetime.Entered+=Enter; view.Lifetime.Resumed+=Resume; view.Lifetime.Ticked+=Tick; Enter();
        }
        public void Enter() => Refresh();
        public void Resume() => Refresh();
        public void Tick() { if(clock.UtcNow.Ticks/TimeSpan.TicksPerSecond!=second || language!=session.Settings.Language) Refresh(); }
        public void Refresh()
        {
            second=clock.UtcNow.Ticks/TimeSpan.TicksPerSecond; language=session.Settings.Language;
            var result=session.Lives.Refresh(); if(result.Success) RetrySave(); else view.ShowError(text.Get("shell.error."+result.Error));
            
            view.Render(new MainDisplay { Title=text.Get("shell.main.title"), Gold=text.Get("shell.gold",session.Economy.Gold),Hearts=text.Get("shell.hearts",session.Lives.Hearts),
                Countdown=FormatCountdown(session,text,clock),
                Play=playLabel?.Invoke()??text.Get("shell.play"),CheckIn=text.Get("shell.checkin.title"),Quests=text.Get("shell.quests.title"),Shop=text.Get("shell.shop.title") });
        }
        internal static string FormatCountdown(ShellSession session,ScreenText text,IClock clock)
        {
            var full=session.Lives.FullAtUtc;
            return full.HasValue?text.Get("shell.lives.countdown",Math.Max(0,(long)Math.Ceiling((full.Value-clock.UtcNow).TotalSeconds))):text.Get("shell.lives.full");
        }
        void Play() { game.Play(); RetrySave(); Refresh(); }
        void Open(ShellScreen screen) { open(screen); Refresh(); }
        public void RetrySave() => text.Commit(view.ShowError);
        public void Dispose() { view.PlayRequested-=Play; view.OpenRequested-=Open; view.Lifetime.Entered-=Enter; view.Lifetime.Resumed-=Resume; view.Lifetime.Ticked-=Tick; }
    }
}
