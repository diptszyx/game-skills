using System;
using System.Collections.Generic;
using IndieGame.Shell.Unity.UI;
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
            if(view.UsesDayCells)
            {
            var labels=new string[5]; var rewards=new string[5]; var states=new DayState[5]; var goldOnly=new bool[5];
            // NextSlot advances immediately after a claim and wraps modulo five. At wrap,
            // retain the completed cycle until the next UTC day becomes claimable.
            int collected=p.NextSlot==0 && p.Error==DailyError.AlreadyClaimed?5:p.NextSlot;
            for(int i=0;i<5;i++)
            {
                labels[i]=text.Get("shell.checkin.day",i+1);
                goldOnly[i]=i==p.NextSlot && p.Reward!=null && p.Reward.Gold>0 && p.Reward.Hearts==0 && p.Reward.Boosters.Count==0;
                rewards[i]=i!=p.NextSlot?"":goldOnly[i]?p.Reward.Gold.ToString(System.Globalization.CultureInfo.InvariantCulture):text.Reward(p.Reward);
                states[i]=i<collected?DayState.Collected:i==p.NextSlot&&p.CanClaimNow?DayState.Today:DayState.Locked;
            }
            view.RenderDays(labels,rewards,states,goldOnly);
            }
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
