using Game;
using UnityEngine;

namespace Steamworks.NET
{
    public class SteamStatsAndAchievements : MonoBehaviour
    {
        // The achievements assigned as enum as indexes to the database
        private enum Achievement : int
        {
            ACH_START_GAME,
            ACH_KILLENEMY_5,
            ACH_DIED,
            ACH_KILLENEMY_1,
            ACH_KILLBOSS,
        };

        // The achievement data structure
        private Achievement_t[] m_Achievements = new Achievement_t[] {
            new Achievement_t(Achievement.ACH_START_GAME, "Started the game.", "You started the game. Good Job."),
            new Achievement_t(Achievement.ACH_KILLENEMY_5, "Killed five enemies.", "You killed five enemies. Good job!"),
            new Achievement_t(Achievement.ACH_KILLENEMY_1, "Killed one enemy.", "You killed one enemy."),
            new Achievement_t(Achievement.ACH_KILLBOSS, "Killed a boss.", "You killed a boss."),
            new Achievement_t(Achievement.ACH_DIED, "You Died.", "Looks like you died."),
        };

        // Our GameID
        private CGameID m_GameID;

        // Did we get the stats from Steam?
        private bool m_bRequestedStats;
        private bool m_bStatsValid;

        // Should we store stats this frame?
        private bool m_bStoreStats;

        protected Callback<UserStatsReceived_t> m_UserStatsReceived;
        protected Callback<UserStatsStored_t> m_UserStatsStored;
        protected Callback<UserAchievementStored_t> m_UserAchievementStored;

        public static SteamStatsAndAchievements Instance;

#if !DISABLESTEAMWORKS
        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            if (!SteamManager.Initialized)
                return;

            // Cache the GameID for use in the Callbacks
            m_GameID = new CGameID(SteamUtils.GetAppID());

            m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
            m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
            m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);

            Events.Events.OnGameEnd += UpdateNextLevelStats;
        }
#endif

        // Update is called once per frame
        void Update()
        {
            if (!SteamManager.Initialized)
                return;

            if (!m_bRequestedStats)
            {
                // Is Steam Loaded? if no, can't get stats, done
                if (!SteamManager.Initialized)
                {
                    m_bRequestedStats = true;
                    return;
                }

                // If yes, request our stats
                bool bSuccess = SteamUserStats.RequestCurrentStats();

                // This function should only return false if we weren't logged in, and we already checked that.
                // But handle it being false again anyway, just ask again later.
                m_bRequestedStats = bSuccess;
            }

            if (!m_bStatsValid)
                return;

            // Get info from sources

            // Evaluate achievements
            foreach (Achievement_t achievement in m_Achievements)
            {
                if (achievement.m_bAchieved)
                    continue;

                switch (achievement.m_eAchievementID)
                {
                    case Achievement.ACH_START_GAME:
                        UnlockAchievement(achievement);
                        break;
                    case Achievement.ACH_KILLENEMY_1:
                        if (Stats.PKillTotal >= 1)
                        {
                            UnlockAchievement(achievement);
                        }
                        break;
                    case Achievement.ACH_KILLENEMY_5:
                        if (Stats.PKillTotal >= 5)
                        {
                            UnlockAchievement(achievement);
                        }
                        break;
                    case Achievement.ACH_DIED:
                        if (Stats.NPHasDied)
                        {
                            UnlockAchievement(achievement);
                        }
                        break;
                    case Achievement.ACH_KILLBOSS:
                        if (Stats.NPKilledBoss)
                        {
                            UnlockAchievement(achievement);
                        }
                        break;
                }
            }

            //Store stats in the Steam database if necessary
            if (m_bStoreStats)
            {
                // set stats
                SteamUserStats.SetStat("TotalKills", Stats.PKillTotal);
                SteamUserStats.SetStat("DepthTotal", Stats.PDepthTotal);

                bool bSuccess = SteamUserStats.StoreStats();
                // If this failed, we never sent anything to the server, try
                // again later.
                m_bStoreStats = !bSuccess;
            }
        }

        //-----------------------------------------------------------------------------
        // Purpose: Updates the stats on level change
        //-----------------------------------------------------------------------------
        public static void UpdateNextLevelStats()
        {
            Stats.PDepthTotal++;
            Stats.NPCurrentDepth++;
        }

        //-----------------------------------------------------------------------------
        // Purpose: Ability to call the update stats outside of the class
        //-----------------------------------------------------------------------------
        public void UpdateStats()
        {
            m_bStoreStats = true;
        }

        //-----------------------------------------------------------------------------
        // Purpose: We have stats data from Steam. It is authoritative, so update
        //			our data with those results now.
        //-----------------------------------------------------------------------------
        private void OnUserStatsReceived(UserStatsReceived_t pCallback)
        {
            if (!SteamManager.Initialized)
                return;

            // we may get callbacks for other games' stats arriving, ignore them
            if ((ulong)m_GameID == pCallback.m_nGameID)
            {
                if (EResult.k_EResultOK == pCallback.m_eResult)
                {
                    Debug.Log("Received stats and achievements from Steam\n");

                    m_bStatsValid = true;

                    // load achievements
                    foreach (Achievement_t ach in m_Achievements)
                    {
                        bool ret = SteamUserStats.GetAchievement(ach.m_eAchievementID.ToString(), out ach.m_bAchieved);
                        if (ret)
                        {
                            ach.m_strName = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "name");
                            ach.m_strDescription = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "desc");
                        }
                        else
                        {
                            Debug.LogWarning("SteamUserStats.GetAchievement failed for Achievement " + ach.m_eAchievementID + "\nIs it registered in the Steam Partner site?");
                        }
                    }

                    // load stats
                    SteamUserStats.GetStat("TotalKills", out Stats.PKillTotal);
                    SteamUserStats.GetStat("DepthTotal", out Stats.PDepthTotal);
                }
                else
                {
                    Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
                }
            }
        }

        //-----------------------------------------------------------------------------
        // Purpose: Unlock this achievement
        //-----------------------------------------------------------------------------
        private void UnlockAchievement(Achievement_t achievement)
        {
            achievement.m_bAchieved = true;

            // the icon may change once it's unlocked
            //achievement.m_iIconImage = 0;

            // mark it down
            SteamUserStats.SetAchievement(achievement.m_eAchievementID.ToString());

            // Store stats end of frame
            m_bStoreStats = true;
        }

        //-----------------------------------------------------------------------------
        // Purpose: Our stats data was stored!
        //-----------------------------------------------------------------------------
        private void OnUserStatsStored(UserStatsStored_t pCallback)
        {
            // we may get callbacks for other games' stats arriving, ignore them
            if ((ulong)m_GameID == pCallback.m_nGameID)
            {
                if (EResult.k_EResultOK == pCallback.m_eResult)
                {
                    Debug.Log("StoreStats - success");
                }
                else if (EResult.k_EResultInvalidParam == pCallback.m_eResult)
                {
                    // One or more stats we set broke a constraint. They've been reverted,
                    // and we should re-iterate the values now to keep in sync.
                    Debug.Log("StoreStats - some failed to validate");
                    // Fake up a callback here so that we re-load the values.
                    UserStatsReceived_t callback = new UserStatsReceived_t();
                    callback.m_eResult = EResult.k_EResultOK;
                    callback.m_nGameID = (ulong)m_GameID;
                    OnUserStatsReceived(callback);
                }
                else
                {
                    Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
                }
            }
        }

        //-----------------------------------------------------------------------------
        // Purpose: An achievement was stored
        //-----------------------------------------------------------------------------
        private void OnAchievementStored(UserAchievementStored_t pCallback)
        {
            // We may get callbacks for other games' stats arriving, ignore them
            if ((ulong)m_GameID == pCallback.m_nGameID)
            {
                if (0 == pCallback.m_nMaxProgress)
                {
                    Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
                }
                else
                {
                    Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
                }
            }
        }

        //-----------------------------------------------------------------------------
        // Purpose: Reset the stats and achievements
        //-----------------------------------------------------------------------------
        public static void ResetAchievements()
        {
            SteamUserStats.ResetAllStats(true);
            SteamUserStats.RequestCurrentStats();
        }

        //-----------------------------------------------------------------------------
        // Purpose: Display the user's stats and achievements
        //-----------------------------------------------------------------------------
        public void Render()
        {
            if (!SteamManager.Initialized)
            {
                GUILayout.Label("Steamworks not Initialized");
                return;
            }

            GUILayout.Label("m_PKillTotal: " + Stats.PKillTotal);
            GUILayout.Label("m_PTDepthTotal: " + Stats.PDepthTotal);

            GUILayout.BeginArea(new Rect(Screen.width - 300, 0, 300, 800));
            foreach (Achievement_t ach in m_Achievements)
            {
                GUILayout.Label(ach.m_eAchievementID.ToString());
                GUILayout.Label(ach.m_strName + " - " + ach.m_strDescription);
                GUILayout.Label("Achieved: " + ach.m_bAchieved);
                GUILayout.Space(20);
            }

            GUILayout.EndArea();
        }

#if !DISABLESTEAMWORKS
        private void OnDestroy()
        {
            Events.Events.OnGameEnd -= UpdateNextLevelStats;
        }
#endif

        private class Achievement_t
        {
            public Achievement m_eAchievementID;
            public string m_strName;
            public string m_strDescription;
            public bool m_bAchieved;

            /// <summary>
            /// Creates an Achievement. You must also mirror the data provided here in https://partner.steamgames.com/apps/achievements/yourappid
            /// </summary>
            /// <param name="achievement">The "API Name Progress Stat" used to uniquely identify the achievement.</param>
            /// <param name="name">The "Display Name" that will be shown to players in game and on the Steam Community.</param>
            /// <param name="desc">The "Description" that will be shown to players in game and on the Steam Community.</param>
            public Achievement_t(Achievement achievementID, string name, string desc)
            {
                m_eAchievementID = achievementID;
                m_strName = name;
                m_strDescription = desc;
                m_bAchieved = false;
            }
        }

    }
}