using UnityEngine;
using Minigames.Managers;

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

        private void Awake()
        {
            InitializeManagers();
        }

        private void Start()
        {
            // Set default base URL (can be overridden by WebViewBridge)
            ApiClient.Instance.SetBaseUrl(defaultBaseUrl);

            // Subscribe to WebView bridge events
            WebViewBridge.Instance.OnBaseUrlReceived += HandleBaseUrlReceived;
            WebViewBridge.Instance.OnAuthTokenReceived += HandleAuthTokenReceived;
            WebViewBridge.Instance.OnTenantConfigReceived += HandleTenantConfigReceived;

            Debug.Log("AppInitializer: App initialized");
        }

        private void OnDestroy()
        {
            if (WebViewBridge.Instance != null)
            {
                WebViewBridge.Instance.OnBaseUrlReceived -= HandleBaseUrlReceived;
                WebViewBridge.Instance.OnAuthTokenReceived -= HandleAuthTokenReceived;
                WebViewBridge.Instance.OnTenantConfigReceived -= HandleTenantConfigReceived;
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
    }
}
