using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace IndieGame.Shell.Leaderboards.Tests
{
    sealed class FakeLeaderboardBackend : ILeaderboardBackend
    {
        public int SignIns, Tops, Mines, Submits, Renames;
        public string RenamedTo;
        public string Board;
        public int Count;
        public long Score;
        public IReadOnlyList<LeaderboardEntry> Entries = Array.Empty<LeaderboardEntry>();
        public LeaderboardEntry Player;
        public Exception SignInError, RequestError;
        public TaskCompletionSource<bool> SignInGate;
        public Task EnsureSignedIn()
        {
            SignIns++;
            if (SignInError != null) throw SignInError;
            return SignInGate == null ? Task.CompletedTask : SignInGate.Task;
        }
        public Task<IReadOnlyList<LeaderboardEntry>> Top(string board, int n)
        {
            Tops++; Board = board; Count = n;
            if (RequestError != null) throw RequestError;
            return Task.FromResult(Entries);
        }
        public Task<LeaderboardEntry> Mine(string board)
        {
            Mines++; Board = board;
            if (RequestError != null) throw RequestError;
            return Task.FromResult(Player);
        }
        public Task<string> UpdatePlayerName(string name)
        {
            Renames++; RenamedTo = name;
            if (RequestError != null) throw RequestError;
            return Task.FromResult(name + "#1234");
        }
        public Task Submit(string board, long score)
        {
            Submits++; Board = board; Score = score;
            if (RequestError != null) throw RequestError;
            return Task.CompletedTask;
        }
    }

    public sealed class UnityLeaderboardServiceTests
    {
        FakeLeaderboardBackend backend;
        UnityLeaderboardService service;
        [SetUp] public void SetUp()
        { backend = new FakeLeaderboardBackend(); service = new UnityLeaderboardService(backend); }

        [Test] public async Task TopMapsEntriesAndRanks()
        {
            backend.Entries = new[] { new LeaderboardEntry("River", 420, 1), new LeaderboardEntry("Sky", 380, 2) };
            var result = await service.GetTop("daily", 2);
            Assert.That(result.Success, Is.True);
            Assert.That(backend.Board, Is.EqualTo("daily"));
            Assert.That(backend.Count, Is.EqualTo(2));
            Assert.That(result.Value.Count, Is.EqualTo(2));
            Assert.That(result.Value[0].PlayerId, Is.EqualTo("River"));
            Assert.That(result.Value[0].Score, Is.EqualTo(420));
            Assert.That(result.Value[0].Rank, Is.EqualTo(1));
            Assert.That(result.Value[1].Rank, Is.EqualTo(2));
            backend.Player = result.Value[1];
            Assert.That((await service.GetMyRank("daily")).Value, Is.SameAs(backend.Player));
        }

        [Test] public async Task MineWithoutScoreIsNotFound()
        {
            var result = await service.GetMyRank("daily");
            Assert.That(result.Error, Is.EqualTo(ServiceError.NotFound));
            Assert.That(result.Value, Is.Null);
            Assert.That(backend.Board, Is.EqualTo("daily"));
        }

        [Test] public async Task SubmitForwardsScore()
        {
            var result = await service.SubmitScore("daily", 9876543210L);
            Assert.That(result.Success && result.Value, Is.True);
            Assert.That(backend.Submits, Is.EqualTo(1));
            Assert.That(backend.Board, Is.EqualTo("daily"));
            Assert.That(backend.Score, Is.EqualTo(9876543210L));
            Assert.That((await service.SubmitScore("daily", 0)).Success, Is.True);
        }

        [Test] public async Task SignInHappensOnceAcrossCalls()
        {
            await service.GetTop("daily", 3); await service.GetMyRank("daily"); await service.SubmitScore("daily", 4);
            Assert.That(backend.SignIns, Is.EqualTo(1));
            Assert.That(backend.Tops + backend.Mines + backend.Submits, Is.EqualTo(3));
        }

        [Test] public async Task SignInFailureIsRetriedOnNextCall()
        {
            backend.SignInError = new UnauthorizedAccessException("auth failed");
            Assert.That((await service.GetTop("daily", 3)).Error, Is.EqualTo(ServiceError.Unavailable));
            Assert.That(backend.Tops, Is.Zero);
            backend.SignInError = null;
            backend.SignInGate = new TaskCompletionSource<bool>();
            var failed = service.GetMyRank("daily");
            backend.SignInGate.SetException(new Exception("async auth failed"));
            Assert.That((await failed).Error, Is.EqualTo(ServiceError.Unavailable));
            backend.SignInGate = null;
            Assert.That((await service.SubmitScore("daily", 4)).Success, Is.True);
            await service.GetTop("daily", 1);
            Assert.That(backend.SignIns, Is.EqualTo(3));
        }

        [Test] public async Task BackendExceptionBecomesUnavailable()
        {
            foreach (var error in new Exception[] { new TimeoutException(), new HttpRequestException(), new UnauthorizedAccessException(), new OperationCanceledException(), new InvalidOperationException() })
            {
                backend.RequestError = error;
                var expected = error is InvalidOperationException ? ServiceError.Failed : ServiceError.Unavailable;
                Assert.That((await service.GetTop("daily", 3)).Error, Is.EqualTo(expected));
                Assert.That((await service.GetMyRank("daily")).Error, Is.EqualTo(expected));
                var submit = await service.SubmitScore("daily", 4);
                Assert.That(submit.Error, Is.EqualTo(expected)); Assert.That(submit.Value, Is.False);
            }
        }

        [Test] public async Task InvalidArgumentsAreRejectedWithoutCallingBackend()
        {
            foreach (var board in new[] { null, "", " \t" })
            {
                Assert.That((await service.GetTop(board, 3)).Error, Is.EqualTo(ServiceError.InvalidArgument));
                Assert.That((await service.GetMyRank(board)).Error, Is.EqualTo(ServiceError.InvalidArgument));
                Assert.That((await service.SubmitScore(board, 1)).Error, Is.EqualTo(ServiceError.InvalidArgument));
            }
            Assert.That((await service.GetTop("daily", -1)).Error, Is.EqualTo(ServiceError.InvalidArgument));
            Assert.That((await service.SubmitScore("daily", -1)).Error, Is.EqualTo(ServiceError.InvalidArgument));
            Assert.That(backend.SignIns + backend.Tops + backend.Mines + backend.Submits, Is.Zero);
            Assert.That((await service.GetTop("daily", 0)).Success, Is.True);
        }

        [Test] public async Task ConcurrentCallsShareOneSignIn()
        {
            backend.SignInGate = new TaskCompletionSource<bool>();
            var top = service.GetTop("daily", 3);
            var mine = service.GetMyRank("daily");
            var submit = service.SubmitScore("daily", 4);
            Assert.That(backend.SignIns, Is.EqualTo(1));
            Assert.That(backend.Tops + backend.Mines + backend.Submits, Is.Zero);
            Assert.That(top.IsCompleted || mine.IsCompleted || submit.IsCompleted, Is.False);
            backend.SignInGate.SetResult(true);
            await Task.WhenAll(top, mine, submit);
            Assert.That(top.Result.Success, Is.True);
            Assert.That(mine.Result.Error, Is.EqualTo(ServiceError.NotFound));
            Assert.That(submit.Result.Success, Is.True);
            Assert.That(backend.SignIns, Is.EqualTo(1));
            Assert.That(backend.Tops + backend.Mines + backend.Submits, Is.EqualTo(3));
        }
    }
}
