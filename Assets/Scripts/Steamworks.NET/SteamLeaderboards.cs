using UnityEngine;

namespace Steamworks.NET
{
    public class SteamLeaderboards : MonoBehaviour
    {
        // The leaderboards assigned as enum as indexes to the database
        public enum LeaderboardType : int
        {
            LEA_KILLS,
            LEA_DEATHS
        }

        // Leaderboard data structure
        private static SteamLeaderboard[] Leaderboards = new SteamLeaderboard[]
        {
            new SteamLeaderboard(LeaderboardType.LEA_KILLS, "Most Kills"),
            new SteamLeaderboard(LeaderboardType.LEA_DEATHS, "Most Deaths"),
        };

        public void Start()
        {
            if (!SteamManager.Initialized)
                return;

            InitAllLeaderboards();
        }

        //-----------------------------------------------------------------------------
        // Purpose: Returns a leaderboard
        //-----------------------------------------------------------------------------
        public static SteamLeaderboard GetLeaderboard(LeaderboardType type)
        {
            return Leaderboards[(int)type];
        }

        //-----------------------------------------------------------------------------
        // Purpose: Updates a given leaderboard to a given score
        //-----------------------------------------------------------------------------
        public static void UpdateLeaderboardScore(LeaderboardType type, int score)
        {
            GetLeaderboard(type).UpdateScore(score);
        }

        //-----------------------------------------------------------------------------
        // Purpose: Inits all the leaderboards
        //-----------------------------------------------------------------------------
        public void InitAllLeaderboards()
        {
            foreach (var leaderboard in Leaderboards)
            {
                leaderboard.InitLeaderboard();
            }
        }
    }
}
