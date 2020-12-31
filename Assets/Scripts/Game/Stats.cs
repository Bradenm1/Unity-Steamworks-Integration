using Steamworks.NET;
using UnityEngine;

namespace Game
{
    public class Stats : MonoBehaviour
    {
        // Persisted Stat details
        public static int PKillTotal;
        public static int PDepthTotal;

        // Non-persisted Stat details
        public static int NPKillTotal;
        public static bool NPHasDied;
        public static bool NPKilledBoss;
        public static int NPCurrentDepth;

        private void Awake()
        {
            Events.Events.OnGameEnd += OnGameEnd;
            Events.Events.OnEnemyKill += OnEnemyKilled;
            Events.Events.OnPlayerDeath += OnPlayerDeath;
        }

        /// <summary>
        /// Run when the game ends
        /// </summary>
        public static void OnGameEnd()
        {
            // Reset the stats
            NPKillTotal = 0;
            NPCurrentDepth = 0;
            NPHasDied = false;
            NPKilledBoss = false;
        }

        /// <summary>
        /// Run when an enemy is killed
        /// </summary>
        public static void OnEnemyKilled()
        {
            PKillTotal++;
            NPKillTotal++;

#if !DISABLESTEAMWORKS
            SteamStatsAndAchievements.Instance.UpdateStats();
            SteamLeaderboards.UpdateLeaderboardScore(SteamLeaderboards.LeaderboardType.LEA_KILLS, PKillTotal);
#endif
        }

        /// <summary>
        /// Run when the player dies
        /// </summary>
        public static void OnPlayerDeath()
        {
            NPHasDied = true;

#if !DISABLESTEAMWORKS
            SteamLeaderboards.UpdateLeaderboardScore(SteamLeaderboards.LeaderboardType.LEA_DEATHS, 1);
#endif
        }

        private void OnDestroy()
        {
            Events.Events.OnGameEnd -= OnGameEnd;
            Events.Events.OnEnemyKill -= OnEnemyKilled;
            Events.Events.OnPlayerDeath -= OnPlayerDeath;
        }
    }
}
