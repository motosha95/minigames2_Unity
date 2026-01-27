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
        /// Backend returns PagedResult<GameDto>, we convert to GameInfo
        /// </summary>
        public void LoadGames(Action<List<GameInfo>> onSuccess = null, Action<string> onError = null)
        {
            ApiClient.Instance.Get<PagedResult<GameDto>>(
                "/api/App/game/available",
                (response) =>
                {
                    availableGames = new List<GameInfo>();
                    gameCache.Clear();
                    
                    if (response.data.items != null)
                    {
                        // Convert GameDto to GameInfo
                        // GameDto should include id field (Guid), or gameId in gameConfigurations
                        for (int i = 0; i < response.data.items.Count; i++)
                        {
                            var gameDto = response.data.items[i];
                            GameInfo gameInfo = DtoConverter.ToGameInfo(gameDto);
                            if (gameInfo != null)
                            {
                                availableGames.Add(gameInfo);
                                // Use the actual gameId (Guid) from GameInfo, not the game name
                                gameCache[gameInfo.id] = gameInfo;
                            }
                        }
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
