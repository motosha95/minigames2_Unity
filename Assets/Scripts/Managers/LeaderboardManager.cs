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
        /// Backend returns PagedResult<WeeklyScoreDto>
        /// </summary>
        public void LoadWeeklyLeaderboard(Action<LeaderboardResponse> onSuccess = null, Action<string> onError = null)
        {
            ApiClient.Instance.Get<PagedResult<WeeklyScoreDto>>(
                "/api/App/score/weekly",
                (response) =>
                {
                    // Convert PagedResult<WeeklyScoreDto> to LeaderboardResponse
                    var leaderboard = new LeaderboardResponse
                    {
                        entries = new List<LeaderboardEntry>(),
                        total = response.data.total,
                        page = response.data.page,
                        pageSize = response.data.pageSize
                    };

                    if (response.data.items != null)
                    {
                        for (int i = 0; i < response.data.items.Count; i++)
                        {
                            var scoreDto = response.data.items[i];
                            leaderboard.entries.Add(new LeaderboardEntry
                            {
                                playerId = scoreDto.playerId,
                                playerName = scoreDto.playerName,
                                score = scoreDto.weeklyScore,
                                rank = (response.data.page - 1) * response.data.pageSize + i + 1
                            });
                        }
                    }

                    weeklyLeaderboard = leaderboard;
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
        /// Backend returns PagedResult<GameScoreDto>
        /// </summary>
        public void LoadGameLeaderboard(string gameId, Action<LeaderboardResponse> onSuccess = null, Action<string> onError = null)
        {
            ApiClient.Instance.Get<PagedResult<GameScoreDto>>(
                $"/api/App/score/leaderboard/{gameId}",
                (response) =>
                {
                    // Convert PagedResult<GameScoreDto> to LeaderboardResponse
                    var leaderboard = new LeaderboardResponse
                    {
                        entries = new List<LeaderboardEntry>(),
                        total = response.data.total,
                        page = response.data.page,
                        pageSize = response.data.pageSize
                    };

                    if (response.data.items != null)
                    {
                        for (int i = 0; i < response.data.items.Count; i++)
                        {
                            var scoreDto = response.data.items[i];
                            leaderboard.entries.Add(new LeaderboardEntry
                            {
                                playerId = scoreDto.playerId,
                                playerName = scoreDto.playerName,
                                score = scoreDto.totalScore, // or bestScore?
                                rank = (response.data.page - 1) * response.data.pageSize + i + 1
                            });
                        }
                    }

                    gameLeaderboards[gameId] = leaderboard;
                    OnGameLeaderboardLoaded?.Invoke(leaderboard);
                    onSuccess?.Invoke(leaderboard);
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
        /// Backend returns PlayerScoreDto
        /// </summary>
        public void LoadMyTotalScore(Action<int> onSuccess = null, Action<string> onError = null)
        {
            ApiClient.Instance.Get<PlayerScoreDto>(
                "/api/App/score/my-total",
                (response) =>
                {
                    myTotalScore = response.data.totalScore;
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
        /// Backend returns WeeklyScoreDto
        /// </summary>
        public void LoadMyWeeklyScore(Action<int> onSuccess = null, Action<string> onError = null)
        {
            ApiClient.Instance.Get<WeeklyScoreDto>(
                "/api/App/score/my-weekly",
                (response) =>
                {
                    myWeeklyScore = response.data.weeklyScore;
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
