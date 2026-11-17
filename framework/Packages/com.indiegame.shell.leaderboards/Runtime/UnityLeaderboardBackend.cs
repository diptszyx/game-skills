using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Exceptions;
using SdkEntry = Unity.Services.Leaderboards.Models.LeaderboardEntry;

namespace IndieGame.Shell.Leaderboards
{
    public sealed class UnityLeaderboardBackend : ILeaderboardBackend
    {
        public async Task EnsureSignedIn()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        static LeaderboardEntry Map(SdkEntry entry) => new LeaderboardEntry(
            string.IsNullOrWhiteSpace(entry.PlayerName) ? entry.PlayerId : entry.PlayerName,
            (long)entry.Score, entry.Rank + 1);

        // Normalize SDK transport/auth failures without leaking SDK types into the service core.
        static async Task<T> Request<T>(Func<Task<T>> request)
        {
            try { return await request(); }
            catch (AuthenticationException e) { throw new UnauthorizedAccessException(e.Message, e); }
            catch (LeaderboardsException e) when (
                e.Reason == LeaderboardsExceptionReason.NoInternetConnection ||
                e.Reason == LeaderboardsExceptionReason.ServiceUnavailable ||
                e.Reason == LeaderboardsExceptionReason.Unauthorized ||
                e.Reason == LeaderboardsExceptionReason.AccessTokenMissing ||
                e.Reason == LeaderboardsExceptionReason.PlayerIdMissing)
            { throw new HttpRequestException(e.Message, e); }
            catch (RequestFailedException e) when (
                e.ErrorCode == CommonErrorCodes.TransportError || e.ErrorCode == CommonErrorCodes.Timeout ||
                e.ErrorCode == CommonErrorCodes.ServiceUnavailable)
            { throw new HttpRequestException(e.Message, e); }
        }

        public async Task<IReadOnlyList<LeaderboardEntry>> Top(string board, int n)
        {
            if (n == 0) return Array.Empty<LeaderboardEntry>();
            var page = await Request(() => LeaderboardsService.Instance.GetScoresAsync(board,
                new GetScoresOptions { Offset = 0, Limit = n }));
            var entries = new List<LeaderboardEntry>();
            foreach (var entry in page.Results) entries.Add(Map(entry));
            return entries.AsReadOnly();
        }

        public async Task<LeaderboardEntry> Mine(string board)
        {
            try { return Map(await Request(() => LeaderboardsService.Instance.GetPlayerScoreAsync(board))); }
            // SDK maps HTTP 404 / service code 27009 specifically to EntryNotFound.
            catch (LeaderboardsException e) when (e.Reason == LeaderboardsExceptionReason.EntryNotFound)
            { return null; }
        }

        public async Task Submit(string board, long score)
        { await Request(() => LeaderboardsService.Instance.AddPlayerScoreAsync(board, (double)score)); }
    }
}
