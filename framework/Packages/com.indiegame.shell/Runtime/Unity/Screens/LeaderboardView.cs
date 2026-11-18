using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using IndieGame.Shell.Unity.UI;
namespace IndieGame.Shell.Unity.Screens
{
    public enum LeaderboardState { Loading, Empty, Error, Populated }
    public sealed class LeaderboardSnapshot
    {
        public LeaderboardState State;
        public IReadOnlyList<LeaderboardEntry> Entries = Array.Empty<LeaderboardEntry>();
        public LeaderboardEntry MyRank;
        public string MessageKey;
    }
    public sealed class LeaderboardView
    {
        readonly ListSurface surface; readonly ShellUiFactory factory; readonly UnityEngine.UI.Button refresh;
        public RectTransform Root => surface.Root;
        public LeaderboardSnapshot Snapshot { get; private set; }
        public event Action RefreshRequested;
        public LeaderboardView(ShellUiFactory factory, Transform parent, PopupView modal=null)
        {
            this.factory=factory; surface=new ListSurface(factory,parent,"Leaderboard",modal);
            refresh=factory.IconButton(modal!=null?modal.Panel:Root,"refresh","Refresh",()=>RefreshRequested?.Invoke());
            var rect=(RectTransform)refresh.transform;
            if(modal!=null) { rect.anchorMin=rect.anchorMax=new Vector2(0,1); var close=ShellLayout.Standard.CloseHitPosition; rect.anchoredPosition=new Vector2(-close.x,close.y); }
            else if(ScreenWidgets.HidesTitle(factory.Style)) { rect.anchorMin=rect.anchorMax=new Vector2(1,0); rect.pivot=new Vector2(1,0); rect.anchoredPosition=new Vector2(-16,180); }
            else { rect.anchorMin=rect.anchorMax=new Vector2(1,1); rect.pivot=new Vector2(1,1); rect.anchoredPosition=ShellLayout.Standard.HeaderRightPosition; }
        }
        public void Render(LeaderboardSnapshot snapshot, ScreenText text)
        {
            Snapshot=snapshot; var rows=new List<RowDisplay>();
            foreach(var e in snapshot.Entries)
                rows.Add(new RowDisplay(e.PlayerId,text.Get("shell.leaderboard.row",e.Rank,e.PlayerId,e.Score),highlight:snapshot.MyRank!=null&&e.PlayerId==snapshot.MyRank.PlayerId));
            if(snapshot.MyRank!=null&&!snapshot.Entries.Any(e=>e.PlayerId==snapshot.MyRank.PlayerId)) rows.Add(new RowDisplay("my-rank",snapshot.MyRank.Rank<1?text.Get("shell.leaderboard.mine.unranked",snapshot.MyRank.Score):text.Get("shell.leaderboard.mine",snapshot.MyRank.Rank,snapshot.MyRank.Score),highlight:true));
            surface.Render(new ListDisplay(text.Get("shell.leaderboard.title"),rows,snapshot.MessageKey==null?"":text.Get(snapshot.MessageKey)),_=>{});
            surface.Error(snapshot.State==LeaderboardState.Error?text.Get(snapshot.MessageKey):"");
            refresh.GetComponent<ActionAccessibility>().Set("refresh",text.Get("shell.refresh"));
        }
    }
    // Completes on the Unity main thread; null/blank board or negative score -> InvalidArgument.
    public interface ILeaderboardRequests
    {
        Task<ServiceResult<bool>> SubmitScore(string board,long score);
        Task<ServiceResult<IReadOnlyList<LeaderboardEntry>>> GetTop(string board,int count);
        Task<ServiceResult<LeaderboardEntry>> GetMyRank(string board);
    }
    public sealed class LeaderboardRequests : ILeaderboardRequests
    {
        readonly ILeaderboardService service;
        public LeaderboardRequests(ILeaderboardService service) { this.service=service; }
        public Task<ServiceResult<bool>> SubmitScore(string board,long score)=>Task.FromResult(service.SubmitScore(board,score));
        public Task<ServiceResult<IReadOnlyList<LeaderboardEntry>>> GetTop(string board,int count)=>Task.FromResult(service.GetTop(board,count));
        public Task<ServiceResult<LeaderboardEntry>> GetMyRank(string board)=>Task.FromResult(service.GetMyRank(board));
    }
    public sealed class LeaderboardPresenter : IDisposable
    {
        readonly LeaderboardView view; readonly ScreenText text; readonly ILeaderboardRequests requests; readonly string board; readonly int count; readonly Func<long> localBest;
        int requestId; bool disposed;
        public Task Pending { get; private set; }
        public LeaderboardPresenter(ShellSession session,LeaderboardView view,LocalizationService localization,ILeaderboardRequests requests,string board,int count,Func<long> localBest=null)
        {
            if(string.IsNullOrWhiteSpace(board)||count<1) throw new ArgumentException("Invalid leaderboard configuration");
            this.view=view; text=new ScreenText(session,localization); this.requests=requests; this.board=board; this.count=count; this.localBest=localBest;
            view.RefreshRequested+=Refresh; view.Root.GetComponent<ScreenLifetime>().Entered+=Refresh; view.Root.GetComponent<ScreenLifetime>().Resumed+=Refresh; Refresh();
        }
        public void Refresh() { Pending=RefreshAsync(); }
        public async Task RefreshAsync()
        {
            int id=++requestId;
            view.Render(new LeaderboardSnapshot { State=LeaderboardState.Loading,MessageKey="shell.leaderboard.loading" },text);
            try
            {
                var top=await requests.GetTop(board,count); var mine=await requests.GetMyRank(board);
                if(disposed||id!=requestId) return;
                var myRank=mine.Value??LocalRow();
                if(!top.Success||(!mine.Success&&mine.Error!=ServiceError.NotFound))
                {
                    var error=!top.Success?top.Error:mine.Error;
                    view.Render(new LeaderboardSnapshot { State=LeaderboardState.Error,MyRank=myRank,MessageKey=error==ServiceError.Unavailable?"shell.leaderboard.offline":"shell.error."+error },text); return;
                }
                view.Render(new LeaderboardSnapshot { State=top.Value.Count==0?LeaderboardState.Empty:LeaderboardState.Populated,Entries=top.Value,MyRank=myRank,
                    MessageKey=top.Value.Count==0?"shell.leaderboard.empty":null },text);
            }
            catch(Exception) { if(!disposed&&id==requestId) view.Render(new LeaderboardSnapshot { State=LeaderboardState.Error,MyRank=LocalRow(),MessageKey="shell.error.Failed" },text); }
        }
        LeaderboardEntry LocalRow()=>localBest==null?null:new LeaderboardEntry("you",localBest(),0);
        public void Dispose() { disposed=true; requestId++; view.RefreshRequested-=Refresh; view.Root.GetComponent<ScreenLifetime>().Entered-=Refresh; view.Root.GetComponent<ScreenLifetime>().Resumed-=Refresh; }
    }
}
