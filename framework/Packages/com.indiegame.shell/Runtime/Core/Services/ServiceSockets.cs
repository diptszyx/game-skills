using System;
using System.Collections.Generic;
using System.Linq;

namespace IndieGame.Shell
{
    public enum ServiceError { None, InvalidArgument, Unavailable, Failed, NotFound, Conflict }
    public sealed class ServiceResult<T>
    {
        public T Value { get; }
        public ServiceError Error { get; }
        public bool Success => Error == ServiceError.None;
        public ServiceResult(T value, ServiceError error = ServiceError.None) { Value = value; Error = error; }
    }
    public enum AdOutcome { Completed, Skipped, Failed, NotReady }
    public sealed class AdResult
    {
        public AdOutcome Outcome { get; }
        public ServiceError Error { get; }
        public bool Success => Error == ServiceError.None && Outcome == AdOutcome.Completed;
        public AdResult(AdOutcome outcome, ServiceError error = ServiceError.None) { Outcome = outcome; Error = error; }
    }
    public interface IRewardedAdService { AdResult Show(string placementId); }
    public sealed class FakeRewardedAdService : IRewardedAdService
    {
        readonly Queue<AdOutcome> script = new Queue<AdOutcome>();
        public ServiceResult<bool> Enqueue(AdOutcome outcome)
        {
            if (!Enum.IsDefined(typeof(AdOutcome), outcome)) return new ServiceResult<bool>(false, ServiceError.InvalidArgument);
            script.Enqueue(outcome); return new ServiceResult<bool>(true);
        }
        public AdResult Show(string placementId)
        {
            if (string.IsNullOrWhiteSpace(placementId)) return new AdResult(AdOutcome.Failed, ServiceError.InvalidArgument);
            return new AdResult(script.Count == 0 ? AdOutcome.NotReady : script.Dequeue());
        }
    }
    public enum PurchaseOutcome { Success, Cancelled, Failed }
    public sealed class PurchaseResult
    {
        public PurchaseOutcome Outcome { get; }
        public string TransactionId { get; }
        public string ProductId { get; }
        public bool AlreadyProcessed { get; }
        public ServiceError Error { get; }
        public bool Success => Error == ServiceError.None && Outcome == PurchaseOutcome.Success;
        public PurchaseResult(PurchaseOutcome outcome, string transactionId = "", string productId = "", bool alreadyProcessed = false, ServiceError error = ServiceError.None)
        { Outcome = outcome; TransactionId = transactionId; ProductId = productId; AlreadyProcessed = alreadyProcessed; Error = error; }
    }
    public sealed class RestoreResult
    {
        public PurchaseOutcome Outcome { get; }
        public bool Success => Outcome == PurchaseOutcome.Success;
        public IReadOnlyList<PurchaseResult> Purchases { get; }
        public RestoreResult(PurchaseOutcome outcome, IEnumerable<PurchaseResult> purchases)
        { Outcome = outcome; Purchases = new List<PurchaseResult>(purchases).AsReadOnly(); }
    }
    public interface IPurchaseService { PurchaseResult Purchase(string productId); RestoreResult Restore(); }
    public sealed class FakePurchaseService : IPurchaseService
    {
        readonly Queue<PurchaseResult> script = new Queue<PurchaseResult>();
        readonly Queue<PurchaseOutcome> restores = new Queue<PurchaseOutcome>();
        readonly Dictionary<string, PurchaseResult> receipts = new Dictionary<string, PurchaseResult>(StringComparer.Ordinal);
        readonly List<PurchaseResult> orderedReceipts = new List<PurchaseResult>();
        public ServiceResult<bool> Enqueue(PurchaseOutcome outcome, string transactionId = "")
        {
            if (!Enum.IsDefined(typeof(PurchaseOutcome), outcome) || (outcome == PurchaseOutcome.Success && string.IsNullOrWhiteSpace(transactionId)))
                return new ServiceResult<bool>(false, ServiceError.InvalidArgument);
            script.Enqueue(new PurchaseResult(outcome, transactionId)); return new ServiceResult<bool>(true);
        }
        public ServiceResult<bool> EnqueueRestore(PurchaseOutcome outcome)
        {
            if (!Enum.IsDefined(typeof(PurchaseOutcome), outcome)) return new ServiceResult<bool>(false, ServiceError.InvalidArgument);
            restores.Enqueue(outcome); return new ServiceResult<bool>(true);
        }
        public PurchaseResult Purchase(string productId)
        {
            if (string.IsNullOrWhiteSpace(productId)) return new PurchaseResult(PurchaseOutcome.Failed, error: ServiceError.InvalidArgument);
            if (script.Count == 0) return new PurchaseResult(PurchaseOutcome.Failed, error: ServiceError.Unavailable);
            var next = script.Dequeue();
            if (next.Outcome != PurchaseOutcome.Success) return new PurchaseResult(next.Outcome, productId: productId);
            if (receipts.TryGetValue(next.TransactionId, out var previous))
                return previous.ProductId == productId
                    ? new PurchaseResult(PurchaseOutcome.Success, previous.TransactionId, productId, true)
                    : new PurchaseResult(PurchaseOutcome.Failed, next.TransactionId, productId, error: ServiceError.Conflict);
            var receipt = new PurchaseResult(PurchaseOutcome.Success, next.TransactionId, productId);
            receipts.Add(receipt.TransactionId, receipt); orderedReceipts.Add(receipt); return receipt;
        }
        public RestoreResult Restore()
        {
            var outcome = restores.Count == 0 ? PurchaseOutcome.Failed : restores.Dequeue();
            return new RestoreResult(outcome, outcome == PurchaseOutcome.Success
                ? orderedReceipts.Select(r => new PurchaseResult(PurchaseOutcome.Success, r.TransactionId, r.ProductId, true))
                : Array.Empty<PurchaseResult>());
        }
    }
    public interface IConfigSource { ServiceResult<string> Fetch(); }
    public sealed class FakeConfigSource : IConfigSource
    {
        readonly Queue<ServiceResult<string>> script = new Queue<ServiceResult<string>>();
        public ServiceResult<bool> Enqueue(string text, ServiceError error = ServiceError.None)
        {
            if (text == null || !Enum.IsDefined(typeof(ServiceError), error)) return new ServiceResult<bool>(false, ServiceError.InvalidArgument);
            script.Enqueue(new ServiceResult<string>(text, error)); return new ServiceResult<bool>(true);
        }
        public ServiceResult<string> Fetch() => script.Count == 0
            ? new ServiceResult<string>("", ServiceError.Unavailable) : script.Dequeue();
    }
    public sealed class LeaderboardEntry
    {
        public string PlayerId { get; }
        public long Score { get; }
        public int Rank { get; }
        public LeaderboardEntry(string playerId, long score, int rank) { PlayerId = playerId; Score = score; Rank = rank; }
    }
    public interface ILeaderboardService
    {
        ServiceResult<bool> SubmitScore(string boardId, long score);
        ServiceResult<IReadOnlyList<LeaderboardEntry>> GetTop(string boardId, int n);
        ServiceResult<LeaderboardEntry?> GetMyRank(string boardId);
    }
    // A shared backend allows deterministic multi-player tests without global state.
    public sealed class FakeLeaderboardState
    {
        internal sealed class Score { internal string Player = ""; internal long Value; }
        internal readonly Dictionary<string, List<Score>> Boards = new Dictionary<string, List<Score>>(StringComparer.Ordinal);
    }
    public sealed class FakeLeaderboardService : ILeaderboardService
    {
        readonly string player;
        readonly FakeLeaderboardState state;
        readonly Queue<ServiceError> script = new Queue<ServiceError>();
        public FakeLeaderboardService(string playerId, FakeLeaderboardState state)
        {
            if (string.IsNullOrWhiteSpace(playerId) || state == null) throw new ArgumentException("Player and state required.");
            player = playerId; this.state = state;
        }
        public ServiceResult<bool> Enqueue(ServiceError error)
        {
            if (!Enum.IsDefined(typeof(ServiceError), error)) return new ServiceResult<bool>(false, ServiceError.InvalidArgument);
            script.Enqueue(error); return new ServiceResult<bool>(true);
        }
        ServiceError Next() => script.Count == 0 ? ServiceError.None : script.Dequeue();
        public ServiceResult<bool> SubmitScore(string boardId, long score)
        {
            if (string.IsNullOrWhiteSpace(boardId) || score < 0) return new ServiceResult<bool>(false, ServiceError.InvalidArgument);
            var error = Next(); if (error != ServiceError.None) return new ServiceResult<bool>(false, error);
            if (!state.Boards.TryGetValue(boardId, out var scores)) { scores = new List<FakeLeaderboardState.Score>(); state.Boards.Add(boardId, scores); }
            var existing = scores.Find(s => s.Player == player);
            if (existing == null) scores.Add(new FakeLeaderboardState.Score { Player = player, Value = score });
            else if (score > existing.Value)
            {
                // Reinsert so a newly achieved tie follows scores submitted earlier.
                scores.Remove(existing); scores.Add(new FakeLeaderboardState.Score { Player = player, Value = score });
            }
            return new ServiceResult<bool>(true);
        }
        IReadOnlyList<LeaderboardEntry> Sorted(string boardId)
        {
            if (!state.Boards.TryGetValue(boardId, out var scores)) return Array.Empty<LeaderboardEntry>();
            return scores.OrderByDescending(s => s.Value).Select((s, i) => new LeaderboardEntry(s.Player, s.Value, i + 1)).ToList().AsReadOnly();
        }
        public ServiceResult<IReadOnlyList<LeaderboardEntry>> GetTop(string boardId, int n)
        {
            if (string.IsNullOrWhiteSpace(boardId) || n < 0) return new ServiceResult<IReadOnlyList<LeaderboardEntry>>(Array.Empty<LeaderboardEntry>(), ServiceError.InvalidArgument);
            var error = Next();
            return new ServiceResult<IReadOnlyList<LeaderboardEntry>>(error == ServiceError.None ? Sorted(boardId).Take(n).ToList().AsReadOnly() : Array.Empty<LeaderboardEntry>(), error);
        }
        public ServiceResult<LeaderboardEntry?> GetMyRank(string boardId)
        {
            if (string.IsNullOrWhiteSpace(boardId)) return new ServiceResult<LeaderboardEntry?>(null, ServiceError.InvalidArgument);
            var error = Next(); if (error != ServiceError.None) return new ServiceResult<LeaderboardEntry?>(null, error);
            var entry = Sorted(boardId).FirstOrDefault(e => e.PlayerId == player);
            return new ServiceResult<LeaderboardEntry?>(entry, entry == null ? ServiceError.NotFound : ServiceError.None);
        }
    }
}
