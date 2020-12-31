using UnityEngine;

namespace Steamworks.NET
{
    public class SteamLeaderboard
    {
        public string s_leaderboardName;
        public SteamLeaderboards.LeaderboardType Leaderboard;
        private ELeaderboardUploadScoreMethod s_leaderboardMethod = ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest;

        private SteamLeaderboard_t s_currentLeaderboard;
        private bool s_initialized = false;
        private CallResult<LeaderboardFindResult_t> m_findResult = new CallResult<LeaderboardFindResult_t>();
        private CallResult<LeaderboardScoreUploaded_t> m_uploadResult = new CallResult<LeaderboardScoreUploaded_t>();

        public SteamLeaderboard(SteamLeaderboards.LeaderboardType leaderboard, string leaderboardName)
        {
            Leaderboard = leaderboard;
            s_leaderboardName = leaderboardName;
        }

        public void InitLeaderboard()
        {
            SteamAPICall_t hSteamAPICall = SteamUserStats.FindLeaderboard(s_leaderboardName);
            m_findResult.Set(hSteamAPICall, OnLeaderboardFindResult);
        }

        public void UpdateScore(int score)
        {
            if (!s_initialized)
            {
                Debug.Log("Can't upload to the leaderboard because isn't loadded yet");
            }
            else
            {
                Debug.Log("uploading score(" + score + ") to steam leaderboard(" + s_leaderboardName + ")");
                SteamAPICall_t hSteamAPICall = SteamUserStats.UploadLeaderboardScore(s_currentLeaderboard, s_leaderboardMethod, score, null, 0);
                m_uploadResult.Set(hSteamAPICall, OnLeaderboardUploadResult);
            }
        }

        private void OnLeaderboardFindResult(LeaderboardFindResult_t pCallback, bool failure)
        {
            Debug.Log("STEAM LEADERBOARDS: Found - " + pCallback.m_bLeaderboardFound + " leaderboardID - " + pCallback.m_hSteamLeaderboard.m_SteamLeaderboard);
            s_currentLeaderboard = pCallback.m_hSteamLeaderboard;
            s_initialized = true;
        }

        private void OnLeaderboardUploadResult(LeaderboardScoreUploaded_t pCallback, bool failure)
        {
            Debug.Log("STEAM LEADERBOARDS: failure - " + failure + " Completed - " + pCallback.m_bSuccess + " NewScore: " + pCallback.m_nGlobalRankNew + " Score " + pCallback.m_nScore + " HasChanged - " + pCallback.m_bScoreChanged);
        }
    }
}
