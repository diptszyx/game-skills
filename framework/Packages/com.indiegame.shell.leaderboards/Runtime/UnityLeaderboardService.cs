using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using IndieGame.Shell.Unity.Screens;

namespace IndieGame.Shell.Leaderboards
{
    // Call on the Unity main thread; awaits retain its synchronization context.
    public sealed class UnityLeaderboardService : ILeaderboardRequests, ILeaderboardRename
    {
        readonly ILeaderboardBackend backend;
        Task signIn;

        public UnityLeaderboardService(ILeaderboardBackend backend)
        { this.backend = backend ?? throw new ArgumentNullException(nameof(backend)); }

        async Task<ServiceError> EnsureSignedIn()
        {
            Task pending = null;
            try
            {
                pending = signIn ?? (signIn = backend.EnsureSignedIn());
                await pending;
                return ServiceError.None;
            }
            catch (Exception)
            {
                // Only clear this attempt: another caller may already have started a retry.
                if (ReferenceEquals(signIn, pending)) signIn = null;
                return ServiceError.Unavailable;
            }
        }

        static ServiceError Error(Exception error)
        {
            if (error is TimeoutException || error is OperationCanceledException ||
                error is HttpRequestException || error is WebException ||
                error is SocketException || error is UnauthorizedAccessException)
                return ServiceError.Unavailable;
            if (error is ArgumentException) return ServiceError.InvalidArgument;
            return ServiceError.Failed;
        }

        public async Task<ServiceResult<IReadOnlyList<LeaderboardEntry>>> GetTop(string board, int count)
        {
            var empty = Array.Empty<LeaderboardEntry>();
            if (string.IsNullOrWhiteSpace(board) || count < 0)
                return new ServiceResult<IReadOnlyList<LeaderboardEntry>>(empty, ServiceError.InvalidArgument);
            var auth = await EnsureSignedIn();
            if (auth != ServiceError.None) return new ServiceResult<IReadOnlyList<LeaderboardEntry>>(empty, auth);
            try { return new ServiceResult<IReadOnlyList<LeaderboardEntry>>(await backend.Top(board, count)); }
            catch (Exception e) { return new ServiceResult<IReadOnlyList<LeaderboardEntry>>(empty, Error(e)); }
        }

        public async Task<ServiceResult<LeaderboardEntry>> GetMyRank(string board)
        {
            if (string.IsNullOrWhiteSpace(board)) return new ServiceResult<LeaderboardEntry>(null, ServiceError.InvalidArgument);
            var auth = await EnsureSignedIn();
            if (auth != ServiceError.None) return new ServiceResult<LeaderboardEntry>(null, auth);
            try
            {
                var entry = await backend.Mine(board);
                return new ServiceResult<LeaderboardEntry>(entry, entry == null ? ServiceError.NotFound : ServiceError.None);
            }
            catch (Exception e) { return new ServiceResult<LeaderboardEntry>(null, Error(e)); }
        }

        public async Task<ServiceResult<string>> UpdatePlayerName(string name)
        {
            if (!PlayerNameRules.TryNormalize(name, out var clean)) return new ServiceResult<string>(null, ServiceError.InvalidArgument);
            var auth = await EnsureSignedIn();
            if (auth != ServiceError.None) return new ServiceResult<string>(null, auth);
            try { return new ServiceResult<string>(await backend.UpdatePlayerName(clean)); }
            catch (Exception e) { return new ServiceResult<string>(null, Error(e)); }
        }

        public async Task<ServiceResult<bool>> SubmitScore(string board, long score)
        {
            if (string.IsNullOrWhiteSpace(board) || score < 0) return new ServiceResult<bool>(false, ServiceError.InvalidArgument);
            var auth = await EnsureSignedIn();
            if (auth != ServiceError.None) return new ServiceResult<bool>(false, auth);
            try { await backend.Submit(board, score); return new ServiceResult<bool>(true); }
            catch (Exception e) { return new ServiceResult<bool>(false, Error(e)); }
        }
    }
}
