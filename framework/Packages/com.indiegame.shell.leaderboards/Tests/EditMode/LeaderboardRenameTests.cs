using System;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace IndieGame.Shell.Leaderboards.Tests
{
    public sealed class LeaderboardRenameTests
    {
        FakeLeaderboardBackend backend;
        UnityLeaderboardService service;
        [SetUp] public void SetUp()
        { backend = new FakeLeaderboardBackend(); service = new UnityLeaderboardService(backend); }

        [Test] public async Task RenameTrimsNameAndReturnsBackendName()
        {
            var result = await service.UpdatePlayerName("  Neo ");
            Assert.That(result.Success, Is.True);
            Assert.That(backend.RenamedTo, Is.EqualTo("Neo"));
            Assert.That(result.Value, Is.EqualTo("Neo#1234"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ",TestName="InvalidNameIsRejectedBeforeSignInOrBackend_OnlySpaces")]
        [TestCase("two words",TestName="InvalidNameIsRejectedBeforeSignInOrBackend_InnerSpace")]
        public async Task InvalidNameIsRejectedBeforeSignInOrBackend(string raw)
        {
            var result = await service.UpdatePlayerName(raw);
            Assert.That(result.Error, Is.EqualTo(ServiceError.InvalidArgument));
            Assert.That(backend.Renames, Is.EqualTo(0));
            Assert.That(backend.SignIns, Is.EqualTo(0));
        }

        [Test] public async Task ServerRejectionMapsToInvalidArgument()
        {
            backend.RequestError = new ArgumentException("rejected");
            Assert.That((await service.UpdatePlayerName("Neo")).Error, Is.EqualTo(ServiceError.InvalidArgument));
        }

        [Test] public async Task TransportFailureMapsToUnavailable()
        {
            backend.RequestError = new HttpRequestException("offline");
            Assert.That((await service.UpdatePlayerName("Neo")).Error, Is.EqualTo(ServiceError.Unavailable));
        }

        [Test] public async Task SignInFailureMapsToUnavailable()
        {
            backend.SignInError = new HttpRequestException("offline");
            Assert.That((await service.UpdatePlayerName("Neo")).Error, Is.EqualTo(ServiceError.Unavailable));
            Assert.That(backend.Renames, Is.EqualTo(0));
        }
    }
}
