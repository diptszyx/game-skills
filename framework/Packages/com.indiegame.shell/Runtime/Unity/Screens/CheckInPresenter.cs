using System;
using System.Collections.Generic;
namespace IndieGame.Shell.Unity.Screens
{
    public sealed class CheckInPresenter : IDisposable
    {
        readonly ShellSession session; readonly CheckInView view; readonly ScreenText text; readonly IClock clock;
        DateTime day; bool busy; string language;
        public CheckInPresenter(ShellSession session, CheckInView view, LocalizationService localization, IClock clock)
        {
            this.session=session; this.view=view; this.clock=clock; text=new ScreenText(session,localization);
            view.ClaimRequested+=Claim; view.Lifetime.Entered+=Enter; view.Lifetime.Resumed+=Resume; view.Lifetime.Ticked+=Tick; Enter();
        }
        public void Enter() => Refresh();
        public void Resume() => Refresh();
        public void Tick() { if(clock.UtcNow.Date!=day || language!=session.Settings.Language) Refresh(); }
        public void Refresh()
        {
            day=clock.UtcNow.Date; language=session.Settings.Language; var p=session.DailyCheckIn.Preview(); var rows=new List<RowDisplay>();
            for(int i=0;i<5;i++) rows.Add(new RowDisplay(i.ToString(),text.Get("shell.checkin.slot",i+1,
                i==p.NextSlot?text.Reward(p.Reward):text.Get("shell.checkin.other")),i==p.NextSlot?text.Get("shell.claim"):"",i==p.NextSlot&&p.CanClaimNow));
            view.Render(new ListDisplay(text.Get("shell.checkin.title"),rows));
            if(!p.Success) view.ShowError(text.Get("shell.error."+p.Error));
        }
        void Claim()
        {
            if(busy || !session.DailyCheckIn.Preview().CanClaimNow) return;
            busy=true;
            try { var result=session.DailyCheckIn.Claim(); if(result.Success || result.Error==DailyError.AlreadyClaimed) RetrySave(); else view.ShowError(text.Get("shell.error."+result.Error)); Refresh(); }
            finally { busy=false; }
        }
        public void RetrySave() => text.Commit(view.ShowError);
        public void Dispose() { view.ClaimRequested-=Claim; view.Lifetime.Entered-=Enter; view.Lifetime.Resumed-=Resume; view.Lifetime.Ticked-=Tick; }
    }
}
