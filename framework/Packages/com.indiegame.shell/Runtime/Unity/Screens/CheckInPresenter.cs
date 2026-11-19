using System;
using System.Collections.Generic;
using IndieGame.Shell.Unity.UI;
namespace IndieGame.Shell.Unity.Screens
{
    public sealed class CheckInPresenter : IDisposable
    {
        readonly ShellSession session; readonly CheckInView view; readonly ScreenText text; readonly IClock clock; readonly RewardFlyLayer fly;
        DateTime day; bool busy; string language;
        public CheckInPresenter(ShellSession session, CheckInView view, LocalizationService localization, IClock clock, RewardFlyLayer fly=null)
        {
            this.session=session; this.view=view; this.clock=clock; this.fly=fly; text=new ScreenText(session,localization);
            view.ClaimRequested+=Claim; view.Lifetime.Entered+=Enter; view.Lifetime.Resumed+=Resume; view.Lifetime.Ticked+=Tick; Enter();
        }
        public void Enter() => Refresh();
        public void Resume() => Refresh();
        public void Tick() { if(clock.UtcNow.Date!=day || language!=session.Settings.Language) Refresh(); }
        public void Refresh()
        {
            day=clock.UtcNow.Date; language=session.Settings.Language; var p=session.DailyCheckIn.Preview(); var rows=new List<RowDisplay>();
            for(int i=0;i<CheckInPack.Days;i++) rows.Add(new RowDisplay(i.ToString(),text.Get("shell.checkin.slot",i+1,
                i==p.NextSlot?text.Reward(p.Reward):text.Get("shell.checkin.other")),i==p.NextSlot?text.Get("shell.claim"):"",i==p.NextSlot&&p.CanClaimNow));
            view.Render(new ListDisplay(text.Get("shell.checkin.title"),rows));
            if(view.UsesDayCells)
            {
            var labels=new string[CheckInPack.Days]; var rewards=new string[CheckInPack.Days]; var states=new DayState[CheckInPack.Days]; var goldOnly=new bool[CheckInPack.Days];
            // NextSlot advances immediately after a claim and wraps modulo the cycle length. At wrap,
            // retain the completed cycle until the next UTC day becomes claimable.
            int collected=p.NextSlot==0 && p.Error==DailyError.AlreadyClaimed?CheckInPack.Days:p.NextSlot;
            for(int i=0;i<CheckInPack.Days;i++)
            {
                labels[i]=text.Get("shell.checkin.day",i+1);
                var slotReward=session.DailyCheckIn.RewardAtSlot(i);
                goldOnly[i]=slotReward!=null && slotReward.Gold>0;
                rewards[i]=slotReward==null?"":goldOnly[i]?slotReward.Gold.ToString(System.Globalization.CultureInfo.InvariantCulture)+(HasExtras(slotReward)?"+":""):text.Reward(slotReward);
                states[i]=i<collected?DayState.Collected:i==p.NextSlot&&p.CanClaimNow?DayState.Today:DayState.Locked;
            }
            view.RenderDays(labels,rewards,states,goldOnly,text.Get("shell.claim"));
            }
            if(!p.Success) view.ShowError(text.Get("shell.error."+p.Error));
        }
        void Claim()
        {
            if(busy || !session.DailyCheckIn.Preview().CanClaimNow) return;
            busy=true;
            try
            {
                var preview=session.DailyCheckIn.Preview(); var origin=view.SlotRect(preview.NextSlot);
                var result=session.DailyCheckIn.Claim();
                if(result.Success && fly!=null) fly.Fly(origin,text.RewardParts(preview.Reward));
                if(result.Success || result.Error==DailyError.AlreadyClaimed) RetrySave(); else view.ShowError(text.Get("shell.error."+result.Error)); Refresh();
            }
            finally { busy=false; }
        }
        static bool HasExtras(Reward reward) => reward.Hearts>0 || reward.Boosters.Count>0;
        public void RetrySave() => text.Commit(view.ShowError);
        public void Dispose() { view.ClaimRequested-=Claim; view.Lifetime.Entered-=Enter; view.Lifetime.Resumed-=Resume; view.Lifetime.Ticked-=Tick; }
    }
}
