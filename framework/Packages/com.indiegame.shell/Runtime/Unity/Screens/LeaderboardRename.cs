using System.Linq;
using System.Threading.Tasks;
namespace IndieGame.Shell.Unity.Screens
{
    // Completes on the Unity main thread; the result value is the name the player now has.
    public interface ILeaderboardRename
    {
        Task<ServiceResult<string>> UpdatePlayerName(string name);
    }
    // Mirrors Unity Authentication's client-side check; length and profanity are enforced by the server.
    public static class PlayerNameRules
    {
        public static bool TryNormalize(string raw,out string name)
        {
            name=raw==null?null:raw.Trim();
            return !string.IsNullOrEmpty(name)&&!name.Any(char.IsWhiteSpace);
        }
    }
}
