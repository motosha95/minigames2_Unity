using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
        [Header("Auth")]
        [SerializeField] private GameObject authPanel;
        [SerializeField] private GameObject mainContentRoot;

        [Header("Menu Panels")]
        [SerializeField] private GameObject gamesPanel;
        [SerializeField] private GameObject profilePanel;
        [SerializeField] private GameObject leaderboardsPanel;
        [SerializeField] private GameObject marketplacePanel;
        [SerializeField] private GameObject questsPanel;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private GameObject errorPanel;

        [Header("Navigation Buttons")]
        [SerializeField] private Button gamesButton;
        [SerializeField] private Button profileButton;
        [SerializeField] private Button leaderboardsButton;
        [SerializeField] private Button marketplaceButton;
        [SerializeField] private Button questsButton;

        [Header("Games List")]
        [SerializeField] private Transform gamesListContainer;
        [SerializeField] private GameObject gameItemPrefab;

        [Header("Error Display")]
        [SerializeField] private TextMeshProUGUI errorText;

        private List<GameInfo> availableGames = new List<GameInfo>();

        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
            RefreshAuthState();
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

            if (marketplaceButton != null)
                marketplaceButton.onClick.AddListener(() => ShowPanel(marketplacePanel));

            if (questsButton != null)
                questsButton.onClick.AddListener(() => ShowPanel(questsPanel));
        }

        private void SubscribeToEvents()
        {
            GameCatalogManager.Instance.OnGamesLoaded += HandleGamesLoaded;
            GameCatalogManager.Instance.OnGamesLoadError += HandleGamesLoadError;
            AuthManager.Instance.OnLoginSuccess += HandleLoginSuccess;
            AuthManager.Instance.OnLogout += HandleLogout;
        }

        private void UnsubscribeFromEvents()
        {
            if (GameCatalogManager.Instance != null)
            {
                GameCatalogManager.Instance.OnGamesLoaded -= HandleGamesLoaded;
                GameCatalogManager.Instance.OnGamesLoadError -= HandleGamesLoadError;
            }
            if (AuthManager.Instance != null)
            {
                AuthManager.Instance.OnLoginSuccess -= HandleLoginSuccess;
                AuthManager.Instance.OnLogout -= HandleLogout;
            }
        }

        private void RefreshAuthState()
        {
            if (AuthManager.Instance.IsAuthenticated())
            {
                ShowMainContent();
                LoadInitialData();
            }
            else
            {
                ShowAuthPanel();
            }
        }

        private void ShowAuthPanel()
        {
            if (authPanel != null) authPanel.SetActive(true);
            if (mainContentRoot != null) mainContentRoot.SetActive(false);
        }

        private void ShowMainContent()
        {
            if (authPanel != null) authPanel.SetActive(false);
            if (mainContentRoot != null) mainContentRoot.SetActive(true);
            ShowPanel(gamesPanel);
        }

        private void HandleLoginSuccess(PlayerProfile _)
        {
            ShowMainContent();
            // Only load games if not already loaded (AppInitializer may have already triggered load)
            if (!GameCatalogManager.Instance.AreGamesLoaded())
            {
                LoadInitialData();
            }
        }

        private void HandleLogout()
        {
            ShowAuthPanel();
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
            if (marketplacePanel != null) marketplacePanel.SetActive(false);
            if (questsPanel != null) questsPanel.SetActive(false);

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
            // Use PopupManager for errors instead of inline error panel
            PopupManager.Instance.ShowError("Error", message);
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
            
            // Show game instructions if available, then start game
            string instructions = GetGameInstructions(game);
            if (!string.IsNullOrEmpty(instructions))
            {
                PopupManager.Instance.ShowInstructions(
                    game.name,
                    instructions,
                    () => StartGameSession(game)
                );
            }
            else
            {
                StartGameSession(game);
            }
        }

        private void StartGameSession(GameInfo game)
        {
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

        private string GetGameInstructions(GameInfo game)
        {
            // Try to get instructions from game metadata
            if (game.metadata != null && game.metadata.ContainsKey("instructions"))
            {
                return game.metadata["instructions"].ToString();
            }
            
            // Fallback to description if no instructions
            return game.description;
        }
    }
}
