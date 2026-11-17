namespace IndieGame.Shell.Leaderboards
{
    public static class UnityLeaderboards
    {
        // No settings: the project id comes from the Unity project's Services link.
        public static UnityLeaderboardService Create() => new UnityLeaderboardService(new UnityLeaderboardBackend());
    }
}
