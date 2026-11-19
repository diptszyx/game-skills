using System.Linq;
using System;
using System.Collections.Generic;
using IndieGame.Shell.Unity.UI;
namespace IndieGame.Shell.Unity.Screens
{
    public sealed class QuestsPresenter : IDisposable
    {
        readonly ShellSession session; readonly QuestsView view; readonly ScreenText text; readonly IClock clock; readonly RewardFlyLayer fly;
        DateTime day; bool busy; string language;
        public QuestsPresenter(ShellSession session, QuestsView view, LocalizationService localization, IClock clock, RewardFlyLayer fly=null)
        {
            this.session=session; this.view=view; this.clock=clock; this.fly=fly; text=new ScreenText(session,localization);
            view.ClaimRequested+=Claim; view.Lifetime.Entered+=Enter; view.Lifetime.Resumed+=Resume; view.Lifetime.Ticked+=Tick; Enter();
        }
        public void Enter() => Refresh();
        public void Resume() => Refresh();
        public void Tick() { if(clock.UtcNow.Date!=day || language!=session.Settings.Language) Refresh(); }
        public void Refresh()
        {
            day=clock.UtcNow.Date; language=session.Settings.Language; var result=session.DailyQuests.Refresh();
            if(result.Success) RetrySave(); else view.ShowError(text.Get("shell.error."+result.Error));
            var rows=new List<RowDisplay>();
            foreach(var q in session.DailyQuests.Quests.OrderBy(x=>x.Template.Target))
                rows.Add(new RowDisplay(q.Template.Id,text.Get("shell.quest."+q.Template.Id),
                    text.Get("shell.claim"),CanClaim(q),(float)q.Progress/q.Template.Target,detail:q.Progress+"/"+q.Template.Target,rewards:text.RewardParts(q.Template.Reward)));
            view.Render(new ListDisplay(text.Get("shell.quests.title"),rows,text.Get("shell.quests.empty")));
        }
        public static bool CanClaim(QuestStatus quest) => !quest.Claimed && quest.Progress>=quest.Template.Target;
        void Claim(string id)
        {
            if(busy) return; busy=true;
            try
            {
                var quest=session.DailyQuests.Quests.FirstOrDefault(q=>q.Template.Id==id); var origin=view.RowRect(id);
                var result=session.DailyQuests.Claim(id);
                if(result.Success && quest!=null && fly!=null) fly.Fly(origin,text.RewardParts(quest.Template.Reward));
                if(result.Success || result.Error==DailyError.AlreadyClaimed) RetrySave(); else view.ShowError(text.Get("shell.error."+result.Error)); Refresh();
            }
            finally { busy=false; }
        }
        public void RetrySave() => text.Commit(view.ShowError);
        public void Dispose() { view.ClaimRequested-=Claim; view.Lifetime.Entered-=Enter; view.Lifetime.Resumed-=Resume; view.Lifetime.Ticked-=Tick; }
    }
}
