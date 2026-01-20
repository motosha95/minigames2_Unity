using System;
using System.Collections.Generic;
using UnityEngine;
using Minigames.Core;
using Minigames.Data;

namespace Minigames.Managers
{
    /// <summary>
    /// Manages the catalog of available games fetched from the API.
    /// Handles game list retrieval and caching.
    /// </summary>
    public class GameCatalogManager : MonoBehaviour
    {
        private static GameCatalogManager _instance;
        public static GameCatalogManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameCatalogManager");
                    _instance = go.AddComponent<GameCatalogManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // Events
        public event Action<List<GameInfo>> OnGamesLoaded;
        public event Action<string> OnGamesLoadError;

        private List<GameInfo> availableGames = new List<GameInfo>();
        private Dictionary<string, GameInfo> gameCache = new Dictionary<string, GameInfo>();

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
        /// Fetch available games from API
        /// </summary>
        public void LoadGames(Action<List<GameInfo>> onSuccess = null, Action<string> onError = null)
        {
            ApiClient.Instance.Get<GameListResponse>(
                "/api/App/game/available",
                (response) =>
                {
                    availableGames = response.data.games ?? new List<GameInfo>();
                    gameCache.Clear();
                    
                    foreach (var game in availableGames)
                    {
                        gameCache[game.id] = game;
                    }

                    OnGamesLoaded?.Invoke(availableGames);
                    onSuccess?.Invoke(availableGames);
                },
                (error) =>
                {
                    OnGamesLoadError?.Invoke(error);
                    onError?.Invoke(error);
                }
            );
        }

        /// <summary>
        /// Get all available games (from cache)
        /// </summary>
        public List<GameInfo> GetAvailableGames()
        {
            return new List<GameInfo>(availableGames);
        }

        /// <summary>
        /// Get game info by ID
        /// </summary>
        public GameInfo GetGameById(string gameId)
        {
            return gameCache.ContainsKey(gameId) ? gameCache[gameId] : null;
        }

        /// <summary>
        /// Get game info by scene name
        /// </summary>
        public GameInfo GetGameBySceneName(string sceneName)
        {
            return availableGames.Find(g => g.sceneName == sceneName);
        }

        /// <summary>
        /// Check if games are loaded
        /// </summary>
        public bool AreGamesLoaded()
        {
            return availableGames.Count > 0;
        }
    }
}
