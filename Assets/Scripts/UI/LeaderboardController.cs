using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Minigames.Data;
using Minigames.Managers;

namespace Minigames.UI
{
    /// <summary>
    /// Controls the leaderboard UI panel.
    /// </summary>
    public class LeaderboardController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Dropdown leaderboardTypeDropdown;
        [SerializeField] private Dropdown gameDropdown;
        [SerializeField] private Transform leaderboardListContainer;
        [SerializeField] private GameObject leaderboardEntryPrefab;
        [SerializeField] private Button refreshButton;

        private List<GameInfo> availableGames = new List<GameInfo>();
        private string currentGameId = null;

        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
            LoadGames();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeUI()
        {
            if (leaderboardTypeDropdown != null)
            {
                leaderboardTypeDropdown.options.Clear();
                leaderboardTypeDropdown.options.Add(new Dropdown.OptionData("Weekly Leaderboard"));
                leaderboardTypeDropdown.options.Add(new Dropdown.OptionData("Game Leaderboard"));
                leaderboardTypeDropdown.value = 0;
                leaderboardTypeDropdown.onValueChanged.AddListener(OnLeaderboardTypeChanged);
            }

            if (gameDropdown != null)
            {
                gameDropdown.onValueChanged.AddListener(OnGameChanged);
            }

            if (refreshButton != null)
            {
                refreshButton.onClick.AddListener(RefreshLeaderboard);
            }
        }

        private void SubscribeToEvents()
        {
            GameCatalogManager.Instance.OnGamesLoaded += HandleGamesLoaded;
            LeaderboardManager.Instance.OnWeeklyLeaderboardLoaded += HandleWeeklyLeaderboardLoaded;
            LeaderboardManager.Instance.OnGameLeaderboardLoaded += HandleGameLeaderboardLoaded;
            LeaderboardManager.Instance.OnLeaderboardError += HandleLeaderboardError;
        }

        private void UnsubscribeFromEvents()
        {
            if (GameCatalogManager.Instance != null)
                GameCatalogManager.Instance.OnGamesLoaded -= HandleGamesLoaded;

            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.OnWeeklyLeaderboardLoaded -= HandleWeeklyLeaderboardLoaded;
                LeaderboardManager.Instance.OnGameLeaderboardLoaded -= HandleGameLeaderboardLoaded;
                LeaderboardManager.Instance.OnLeaderboardError -= HandleLeaderboardError;
            }
        }

        private void LoadGames()
        {
            GameCatalogManager.Instance.LoadGames();
        }

        private void HandleGamesLoaded(List<GameInfo> games)
        {
            availableGames = games;

            if (gameDropdown != null)
            {
                gameDropdown.options.Clear();
                gameDropdown.options.Add(new Dropdown.OptionData("Select Game"));
                
                foreach (var game in games)
                {
                    if (game.isActive)
                        gameDropdown.options.Add(new Dropdown.OptionData(game.name));
                }

                gameDropdown.value = 0;
            }
        }

        private void OnLeaderboardTypeChanged(int index)
        {
            RefreshLeaderboard();
        }

        private void OnGameChanged(int index)
        {
            if (index > 0 && index <= availableGames.Count)
            {
                currentGameId = availableGames[index - 1].id;
            }
            else
            {
                currentGameId = null;
            }
            RefreshLeaderboard();
        }

        private void RefreshLeaderboard()
        {
            if (leaderboardTypeDropdown == null) return;

            bool isWeekly = leaderboardTypeDropdown.value == 0;

            if (isWeekly)
            {
                LeaderboardManager.Instance.LoadWeeklyLeaderboard();
            }
            else
            {
                if (string.IsNullOrEmpty(currentGameId))
                {
                    Debug.LogWarning("LeaderboardController: No game selected for game leaderboard");
                    return;
                }
                LeaderboardManager.Instance.LoadGameLeaderboard(currentGameId);
            }
        }

        private void HandleWeeklyLeaderboardLoaded(LeaderboardResponse response)
        {
            DisplayLeaderboard(response);
        }

        private void HandleGameLeaderboardLoaded(LeaderboardResponse response)
        {
            DisplayLeaderboard(response);
        }

        private void HandleLeaderboardError(string error)
        {
            Debug.LogError($"LeaderboardController: Error loading leaderboard: {error}");
        }

        private void DisplayLeaderboard(LeaderboardResponse response)
        {
            if (leaderboardListContainer == null || leaderboardEntryPrefab == null)
                return;

            // Clear existing entries
            foreach (Transform child in leaderboardListContainer)
            {
                Destroy(child.gameObject);
            }

            if (response == null || response.entries == null)
                return;

            // Create entries
            foreach (var entry in response.entries)
            {
                GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardListContainer);
                LeaderboardEntryController entryController = entryObj.GetComponent<LeaderboardEntryController>();
                
                if (entryController != null)
                {
                    entryController.Setup(entry);
                }
            }
        }
    }
}
