using System.Collections.Generic;
using System.Threading.Tasks;

namespace IndieGame.Shell.Leaderboards
{
    public interface ILeaderboardBackend
    {
        Task EnsureSignedIn();
        Task<IReadOnlyList<LeaderboardEntry>> Top(string board, int n);
        Task<LeaderboardEntry> Mine(string board); // Null when the player has no score.
        Task Submit(string board, long score);
        Task<string> UpdatePlayerName(string name); // Throws ArgumentException when the service rejects the name.
    }
}
