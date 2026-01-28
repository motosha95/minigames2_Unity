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
        
        [Header("Auto-Login (Guest Registration)")]
        [SerializeField] private bool autoLoginOnStart = true;
        [Tooltip("If enabled, automatically registers a guest user on app start")]
        [SerializeField] private bool useGuestRegistration = true;

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
                
                // Check if tenant ID changed - clear guest info if it did
                string savedTenantId = AuthManager.Instance.GetSavedGuestTenantId();
                if (!string.IsNullOrEmpty(savedTenantId) && savedTenantId != defaultTenantId)
                {
                    Debug.Log($"AppInitializer: Tenant ID changed from {savedTenantId} to {defaultTenantId}. Clearing guest info.");
                    AuthManager.Instance.ClearGuestInfo();
                }
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
            if (autoLoginOnStart && !string.IsNullOrEmpty(defaultTenantId))
            {
                PerformAutoLogin();
            }
            else
            {
                Debug.Log("AppInitializer: Auto-login disabled or missing tenant ID");
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
            
            // Check if tenant ID changed - clear guest info if it did
            string savedTenantId = AuthManager.Instance.GetSavedGuestTenantId();
            if (!string.IsNullOrEmpty(savedTenantId) && savedTenantId != tenantId)
            {
                Debug.Log($"AppInitializer: Tenant ID changed from {savedTenantId} to {tenantId}. Clearing guest info.");
                AuthManager.Instance.ClearGuestInfo();
            }
            
            // If auto-login is enabled and we haven't logged in yet, try auto-login
            if (autoLoginOnStart && !AuthManager.Instance.IsAuthenticated())
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
        /// Perform automatic guest registration/login on app start
        /// Checks for saved guest info first, then registers new guest if needed
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

            if (!useGuestRegistration)
            {
                Debug.LogWarning("AppInitializer: Guest registration is disabled. Set useGuestRegistration to true for auto-login.");
                return;
            }

            // Check if we have saved guest info for this tenant
            if (AuthManager.Instance.HasSavedGuestInfo(currentTenantId))
            {
                string savedSocialMediaId = AuthManager.Instance.GetSavedGuestSocialMediaId();
                Debug.Log($"AppInitializer: Found saved guest info, attempting login with socialMediaId: {savedSocialMediaId}");
                
                AuthManager.Instance.LoginWithSocialMedia(
                    savedSocialMediaId,
                    (profile) =>
                    {
                        Debug.Log($"AppInitializer: Guest login successful for player: {profile.username}");
                        // Games will be fetched automatically by HandleLoginSuccess
                    },
                    (error) =>
                    {
                        Debug.LogWarning($"AppInitializer: Saved guest login failed: {error}. Registering new guest...");
                        // If login fails (e.g., guest was deleted), register new guest
                        RegisterNewGuest(currentTenantId);
                    }
                );
            }
            else
            {
                // No saved guest info, register new guest
                RegisterNewGuest(currentTenantId);
            }
        }

        /// <summary>
        /// Register a new guest user
        /// </summary>
        private void RegisterNewGuest(string tenantId)
        {
            Debug.Log($"AppInitializer: Registering new guest user, tenantId: {tenantId}");
            AuthManager.Instance.RegisterGuest(
                (profile) =>
                {
                    Debug.Log($"AppInitializer: Guest registration successful for player: {profile.username}");
                    // Games will be fetched automatically by HandleLoginSuccess
                },
                (error) =>
                {
                    Debug.LogError($"AppInitializer: Guest registration failed: {error}");
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
