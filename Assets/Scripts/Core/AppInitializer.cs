using UnityEngine;
using Minigames.Managers;
using Minigames.UI;
using Minigames.Core;

namespace Minigames.Core
{
    /// <summary>
    /// Initializes the app on startup.
    /// Ensures all managers are created and ready.
    /// This should be attached to a GameObject in the Main Scene.
    /// </summary>
    public class AppInitializer : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private string defaultBaseUrl = "https://api.example.com";
        [SerializeField] private string defaultTenantId = "";
        [SerializeField] private string defaultEncryptionKey = ""; // Optional: Set encryption key for development/testing
        
        [Header("Auto-Login (Dev/Testing)")]
        [SerializeField] private bool autoLoginOnStart = true;
        [SerializeField] private string devSocialMediaId = "2";

        private void Awake()
        {
            InitializeManagers();
        }

        private void Start()
        {
            // Set default base URL and tenant ID (can be overridden by WebViewBridge)
            ApiClient.Instance.SetBaseUrl(defaultBaseUrl);
            if (!string.IsNullOrEmpty(defaultTenantId))
            {
                ApiClient.Instance.SetTenantId(defaultTenantId);
            }

            // Set encryption key if provided (can be overridden by WebViewBridge)
            if (!string.IsNullOrEmpty(defaultEncryptionKey))
            {
                ScoreEncryptionHelper.SetEncryptionKey(defaultEncryptionKey);
            }

            // Subscribe to WebView bridge events
            WebViewBridge.Instance.OnBaseUrlReceived += HandleBaseUrlReceived;
            WebViewBridge.Instance.OnAuthTokenReceived += HandleAuthTokenReceived;
            WebViewBridge.Instance.OnTenantConfigReceived += HandleTenantConfigReceived;
            WebViewBridge.Instance.OnTenantIdReceived += HandleTenantIdReceived;
            WebViewBridge.Instance.OnEncryptionKeyReceived += HandleEncryptionKeyReceived;
            
            // Subscribe to auth events for auto-login flow
            AuthManager.Instance.OnLoginSuccess += HandleLoginSuccess;
            AuthManager.Instance.OnAuthError += HandleAuthError;

            // Auto-login on start if enabled and tenant ID is set
            if (autoLoginOnStart && !string.IsNullOrEmpty(defaultTenantId) && !string.IsNullOrEmpty(devSocialMediaId))
            {
                PerformAutoLogin();
            }
            else
            {
                Debug.Log("AppInitializer: Auto-login disabled or missing tenant ID/socialMediaId");
            }

            Debug.Log("AppInitializer: App initialized");
        }

        private void OnDestroy()
        {
            if (WebViewBridge.Instance != null)
            {
                WebViewBridge.Instance.OnBaseUrlReceived -= HandleBaseUrlReceived;
                WebViewBridge.Instance.OnAuthTokenReceived -= HandleAuthTokenReceived;
                WebViewBridge.Instance.OnTenantConfigReceived -= HandleTenantConfigReceived;
                WebViewBridge.Instance.OnTenantIdReceived -= HandleTenantIdReceived;
                WebViewBridge.Instance.OnEncryptionKeyReceived -= HandleEncryptionKeyReceived;
            }
            
            if (AuthManager.Instance != null)
            {
                AuthManager.Instance.OnLoginSuccess -= HandleLoginSuccess;
                AuthManager.Instance.OnAuthError -= HandleAuthError;
            }
        }

        private void InitializeManagers()
        {
            // Force initialization of all singleton managers
            var apiClient = ApiClient.Instance;
            var authManager = AuthManager.Instance;
            var gameSessionManager = GameSessionManager.Instance;
            var gameCatalogManager = GameCatalogManager.Instance;
            var playerProfileManager = PlayerProfileManager.Instance;
            var leaderboardManager = LeaderboardManager.Instance;
            var sceneNavigationManager = SceneNavigationManager.Instance;
            var webViewBridge = WebViewBridge.Instance;
            var productManager = ProductManager.Instance;
            var questManager = QuestManager.Instance;
            var popupManager = PopupManager.Instance;

            Debug.Log("AppInitializer: All managers initialized");
        }

        private void HandleBaseUrlReceived(string baseUrl)
        {
            Debug.Log($"AppInitializer: Base URL updated from WebView: {baseUrl}");
        }

        private void HandleAuthTokenReceived(string token)
        {
            Debug.Log("AppInitializer: Auth token received from WebView");
            // Token is already set in AuthManager, just log
        }

        private void HandleTenantConfigReceived(string configJson)
        {
            Debug.Log($"AppInitializer: Tenant config received: {configJson}");
            // TODO: Parse and apply tenant configuration (colors, logos, labels)
        }

        private void HandleTenantIdReceived(string tenantId)
        {
            Debug.Log($"AppInitializer: Tenant ID updated from WebView: {tenantId}");
            // If auto-login is enabled and we haven't logged in yet, try auto-login
            if (autoLoginOnStart && !AuthManager.Instance.IsAuthenticated() && !string.IsNullOrEmpty(devSocialMediaId))
            {
                PerformAutoLogin();
            }
        }

        private void HandleEncryptionKeyReceived(string encryptionKey)
        {
            Debug.Log("AppInitializer: Encryption key received from WebView");
            // Encryption key is already set in ScoreEncryptionHelper, just log
        }

        /// <summary>
        /// Perform automatic login with social media ID
        /// </summary>
        private void PerformAutoLogin()
        {
            // Check if tenant ID is set (either default or from WebView)
            string currentTenantId = ApiClient.Instance.GetTenantId();
            if (string.IsNullOrEmpty(currentTenantId))
            {
                Debug.LogWarning("AppInitializer: Cannot auto-login - tenant ID is not set");
                return;
            }

            if (string.IsNullOrEmpty(devSocialMediaId))
            {
                Debug.LogWarning("AppInitializer: Cannot auto-login - social media ID is not set");
                return;
            }

            Debug.Log($"AppInitializer: Auto-logging in with socialMediaId: {devSocialMediaId}, tenantId: {currentTenantId}");
            AuthManager.Instance.LoginWithSocialMedia(
                devSocialMediaId,
                (profile) =>
                {
                    Debug.Log($"AppInitializer: Auto-login successful for player: {profile.username}");
                    // Games will be fetched automatically by HandleLoginSuccess
                },
                (error) =>
                {
                    Debug.LogError($"AppInitializer: Auto-login failed: {error}");
                }
            );
        }

        private void HandleLoginSuccess(Minigames.Data.PlayerProfile profile)
        {
            Debug.Log($"AppInitializer: Login successful - fetching games");
            // Fetch games after successful login
            GameCatalogManager.Instance.LoadGames();
        }

        private void HandleAuthError(string error)
        {
            Debug.LogError($"AppInitializer: Auth error: {error}");
        }
    }
}
