using System.Linq;
using System;
using System.Collections.Generic;
namespace IndieGame.Shell.Unity.Screens
{
    public sealed class QuestsPresenter : IDisposable
    {
        readonly ShellSession session; readonly QuestsView view; readonly ScreenText text; readonly IClock clock;
        DateTime day; bool busy; string language;
        public QuestsPresenter(ShellSession session, QuestsView view, LocalizationService localization, IClock clock)
        {
            this.session=session; this.view=view; this.clock=clock; text=new ScreenText(session,localization);
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
                    text.Get("shell.claim"),CanClaim(q),(float)q.Progress/q.Template.Target,detail:q.Progress+"/"+q.Template.Target+" · "+text.RewardShort(q.Template.Reward)));
            view.Render(new ListDisplay(text.Get("shell.quests.title"),rows,text.Get("shell.quests.empty")));
        }
        public static bool CanClaim(QuestStatus quest) => !quest.Claimed && quest.Progress>=quest.Template.Target;
        void Claim(string id)
        {
            if(busy) return; busy=true;
            try { var result=session.DailyQuests.Claim(id); if(result.Success || result.Error==DailyError.AlreadyClaimed) RetrySave(); else view.ShowError(text.Get("shell.error."+result.Error)); Refresh(); }
            finally { busy=false; }
        }
        public void RetrySave() => text.Commit(view.ShowError);
        public void Dispose() { view.ClaimRequested-=Claim; view.Lifetime.Entered-=Enter; view.Lifetime.Resumed-=Resume; view.Lifetime.Ticked-=Tick; }
    }
}
