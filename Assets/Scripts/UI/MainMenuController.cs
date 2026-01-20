using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Minigames.Data;
using Minigames.Managers;

namespace Minigames.UI
{
    /// <summary>
    /// Controls the main menu UI in the Main Scene.
    /// Handles navigation between different sections (games, profile, leaderboards).
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Menu Panels")]
        [SerializeField] private GameObject gamesPanel;
        [SerializeField] private GameObject profilePanel;
        [SerializeField] private GameObject leaderboardsPanel;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private GameObject errorPanel;

        [Header("Navigation Buttons")]
        [SerializeField] private Button gamesButton;
        [SerializeField] private Button profileButton;
        [SerializeField] private Button leaderboardsButton;

        [Header("Games List")]
        [SerializeField] private Transform gamesListContainer;
        [SerializeField] private GameObject gameItemPrefab;

        [Header("Error Display")]
        [SerializeField] private Text errorText;

        private List<GameInfo> availableGames = new List<GameInfo>();

        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
            LoadInitialData();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeUI()
        {
            // Set up navigation buttons
            if (gamesButton != null)
                gamesButton.onClick.AddListener(() => ShowPanel(gamesPanel));

            if (profileButton != null)
                profileButton.onClick.AddListener(() => ShowPanel(profilePanel));

            if (leaderboardsButton != null)
                leaderboardsButton.onClick.AddListener(() => ShowPanel(leaderboardsPanel));

            // Show games panel by default
            ShowPanel(gamesPanel);
        }

        private void SubscribeToEvents()
        {
            GameCatalogManager.Instance.OnGamesLoaded += HandleGamesLoaded;
            GameCatalogManager.Instance.OnGamesLoadError += HandleGamesLoadError;
        }

        private void UnsubscribeFromEvents()
        {
            if (GameCatalogManager.Instance != null)
            {
                GameCatalogManager.Instance.OnGamesLoaded -= HandleGamesLoaded;
                GameCatalogManager.Instance.OnGamesLoadError -= HandleGamesLoadError;
            }
        }

        private void LoadInitialData()
        {
            ShowLoading(true);
            GameCatalogManager.Instance.LoadGames();
        }

        private void ShowPanel(GameObject panel)
        {
            // Hide all panels
            if (gamesPanel != null) gamesPanel.SetActive(false);
            if (profilePanel != null) profilePanel.SetActive(false);
            if (leaderboardsPanel != null) leaderboardsPanel.SetActive(false);

            // Show selected panel
            if (panel != null) panel.SetActive(true);
        }

        private void ShowLoading(bool show)
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(show);
        }

        private void ShowError(string message)
        {
            if (errorPanel != null)
            {
                errorPanel.SetActive(true);
                if (errorText != null)
                    errorText.text = message;
            }
        }

        private void HandleGamesLoaded(List<GameInfo> games)
        {
            availableGames = games;
            ShowLoading(false);
            PopulateGamesList();
        }

        private void HandleGamesLoadError(string error)
        {
            ShowLoading(false);
            ShowError($"Failed to load games: {error}");
        }

        private void PopulateGamesList()
        {
            if (gamesListContainer == null || gameItemPrefab == null)
                return;

            // Clear existing items
            foreach (Transform child in gamesListContainer)
            {
                Destroy(child.gameObject);
            }

            // Create items for each game
            foreach (var game in availableGames)
            {
                if (!game.isActive) continue;

                GameObject itemObj = Instantiate(gameItemPrefab, gamesListContainer);
                GameItemController itemController = itemObj.GetComponent<GameItemController>();
                
                if (itemController != null)
                {
                    itemController.Setup(game, OnGameSelected);
                }
            }
        }

        private void OnGameSelected(GameInfo game)
        {
            Debug.Log($"MainMenuController: Selected game {game.name} (ID: {game.id})");
            
            // Start game session and load game scene
            GameSessionManager.Instance.StartSession(
                game.id,
                null,
                (session) =>
                {
                    Debug.Log($"MainMenuController: Session started: {session.id}");
                    WebViewBridge.Instance.NotifyGameStart(game.id);
                    SceneNavigationManager.Instance.LoadGameScene(game.sceneName, () =>
                    {
                        Debug.Log($"MainMenuController: Game scene loaded: {game.sceneName}");
                    });
                },
                (error) =>
                {
                    ShowError($"Failed to start game session: {error}");
                }
            );
        }
    }
}
