using System;
using System.Collections.Generic;
using UnityEngine;
using Minigames.Core;
using Minigames.Data;

namespace Minigames.Managers
{
    /// <summary>
    /// Manages leaderboard data retrieval and caching.
    /// Handles weekly leaderboards and game-specific leaderboards.
    /// </summary>
    public class LeaderboardManager : MonoBehaviour
    {
        private static LeaderboardManager _instance;
        public static LeaderboardManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("LeaderboardManager");
                    _instance = go.AddComponent<LeaderboardManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // Events
        public event Action<LeaderboardResponse> OnWeeklyLeaderboardLoaded;
        public event Action<LeaderboardResponse> OnGameLeaderboardLoaded;
        public event Action<int> OnMyTotalScoreLoaded;
        public event Action<int> OnMyWeeklyScoreLoaded;
        public event Action<string> OnLeaderboardError;

        private LeaderboardResponse weeklyLeaderboard;
        private Dictionary<string, LeaderboardResponse> gameLeaderboards = new Dictionary<string, LeaderboardResponse>();
        private int myTotalScore;
        private int myWeeklyScore;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Load weekly leaderboard for all players
        /// </summary>
        public void LoadWeeklyLeaderboard(Action<LeaderboardResponse> onSuccess = null, Action<string> onError = null)
        {
            ApiClient.Instance.Get<LeaderboardResponse>(
                "/api/App/score/weekly",
                (response) =>
                {
                    weeklyLeaderboard = response.data;
                    OnWeeklyLeaderboardLoaded?.Invoke(weeklyLeaderboard);
                    onSuccess?.Invoke(weeklyLeaderboard);
                },
                (error) =>
                {
                    OnLeaderboardError?.Invoke(error);
                    onError?.Invoke(error);
                }
            );
        }

        /// <summary>
        /// Load leaderboard for a specific game
        /// </summary>
        public void LoadGameLeaderboard(string gameId, Action<LeaderboardResponse> onSuccess = null, Action<string> onError = null)
        {
            ApiClient.Instance.Get<LeaderboardResponse>(
                $"/api/App/score/leaderboard/{gameId}",
                (response) =>
                {
                    gameLeaderboards[gameId] = response.data;
                    OnGameLeaderboardLoaded?.Invoke(response.data);
                    onSuccess?.Invoke(response.data);
                },
                (error) =>
                {
                    OnLeaderboardError?.Invoke(error);
                    onError?.Invoke(error);
                }
            );
        }

        /// <summary>
        /// Load my total score
        /// </summary>
        public void LoadMyTotalScore(Action<int> onSuccess = null, Action<string> onError = null)
        {
            ApiClient.Instance.Get<ScoreResponse>(
                "/api/App/score/my-total",
                (response) =>
                {
                    myTotalScore = response.data.score;
                    OnMyTotalScoreLoaded?.Invoke(myTotalScore);
                    onSuccess?.Invoke(myTotalScore);
                },
                (error) =>
                {
                    OnLeaderboardError?.Invoke(error);
                    onError?.Invoke(error);
                }
            );
        }

        /// <summary>
        /// Load my weekly score
        /// </summary>
        public void LoadMyWeeklyScore(Action<int> onSuccess = null, Action<string> onError = null)
        {
            ApiClient.Instance.Get<ScoreResponse>(
                "/api/App/score/my-weekly",
                (response) =>
                {
                    myWeeklyScore = response.data.score;
                    OnMyWeeklyScoreLoaded?.Invoke(myWeeklyScore);
                    onSuccess?.Invoke(myWeeklyScore);
                },
                (error) =>
                {
                    OnLeaderboardError?.Invoke(error);
                    onError?.Invoke(error);
                }
            );
        }

        /// <summary>
        /// Get cached weekly leaderboard
        /// </summary>
        public LeaderboardResponse GetWeeklyLeaderboard()
        {
            return weeklyLeaderboard;
        }

        /// <summary>
        /// Get cached game leaderboard
        /// </summary>
        public LeaderboardResponse GetGameLeaderboard(string gameId)
        {
            return gameLeaderboards.ContainsKey(gameId) ? gameLeaderboards[gameId] : null;
        }

        /// <summary>
        /// Get cached my total score
        /// </summary>
        public int GetMyTotalScore()
        {
            return myTotalScore;
        }

        /// <summary>
        /// Get cached my weekly score
        /// </summary>
        public int GetMyWeeklyScore()
        {
            return myWeeklyScore;
        }
    }
}
