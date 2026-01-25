using System;
using UnityEngine;
using Minigames.Core;
using Minigames.Data;

namespace Minigames.Managers
{
    /// <summary>
    /// Manages player authentication and token handling.
    /// Token is stored only in memory as per requirements.
    /// </summary>
    public class AuthManager : MonoBehaviour
    {
        private static AuthManager _instance;
        public static AuthManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("AuthManager");
                    _instance = go.AddComponent<AuthManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // Events
        public event Action<PlayerProfile> OnLoginSuccess;
        public event Action OnLogout;
        public event Action<string> OnAuthError;

        private PlayerProfile currentPlayer;
        private string authToken;

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
        /// Set auth token from WebView bridge (called by host mobile app)
        /// </summary>
        public void SetTokenFromWebView(string token)
        {
            authToken = token;
            ApiClient.Instance.SetAuthToken(token);
            FetchProfile(
                (profile) => OnLoginSuccess?.Invoke(profile),
                (error) => OnAuthError?.Invoke(error)
            );
        }

        /// <summary>
        /// Register a new player
        /// </summary>
        public void Register(PlayerRegisterRequest request, Action<PlayerProfile> onSuccess, Action<string> onError)
        {
            ApiClient.Instance.Post<PlayerRegisterRequest, PlayerLoginResponse>(
                "/api/App/player/register",
                request,
                (response) =>
                {
                    authToken = response.data.token;
                    currentPlayer = response.data.player;
                    ApiClient.Instance.SetAuthToken(authToken);
                    OnLoginSuccess?.Invoke(currentPlayer);
                    onSuccess?.Invoke(currentPlayer);
                },
                (error) =>
                {
                    OnAuthError?.Invoke(error);
                    onError?.Invoke(error);
                }
            );
        }

        /// <summary>
        /// Login with username and password
        /// </summary>
        public void Login(PlayerLoginRequest request, Action<PlayerProfile> onSuccess, Action<string> onError)
        {
            ApiClient.Instance.Post<PlayerLoginRequest, PlayerLoginResponse>(
                "/api/App/player/login",
                request,
                (response) =>
                {
                    authToken = response.data.token;
                    currentPlayer = response.data.player;
                    ApiClient.Instance.SetAuthToken(authToken);
                    OnLoginSuccess?.Invoke(currentPlayer);
                    onSuccess?.Invoke(currentPlayer);
                },
                (error) =>
                {
                    OnAuthError?.Invoke(error);
                    onError?.Invoke(error);
                }
            );
        }

        /// <summary>
        /// Fetch current player profile
        /// </summary>
        public void FetchProfile(Action<PlayerProfile> onSuccess = null, Action<string> onError = null)
        {
            ApiClient.Instance.Get<PlayerProfile>(
                "/api/App/player/profile",
                (response) =>
                {
                    currentPlayer = response.data;
                    onSuccess?.Invoke(currentPlayer);
                },
                (error) =>
                {
                    OnAuthError?.Invoke(error);
                    onError?.Invoke(error);
                }
            );
        }

        /// <summary>
        /// Update player profile
        /// </summary>
        public void UpdateProfile(PlayerProfileUpdateRequest request, Action<PlayerProfile> onSuccess, Action<string> onError)
        {
            ApiClient.Instance.Put<PlayerProfileUpdateRequest, PlayerProfile>(
                "/api/App/player/profile",
                request,
                (response) =>
                {
                    currentPlayer = response.data;
                    onSuccess?.Invoke(currentPlayer);
                },
                (error) =>
                {
                    OnAuthError?.Invoke(error);
                    onError?.Invoke(error);
                }
            );
        }

        /// <summary>
        /// Deactivate current account
        /// </summary>
        public void DeactivateAccount(Action onSuccess, Action<string> onError)
        {
            ApiClient.Instance.Put<object, object>(
                "/api/App/player/deactivate",
                new object(),
                (response) =>
                {
                    Logout();
                    onSuccess?.Invoke();
                },
                (error) =>
                {
                    OnAuthError?.Invoke(error);
                    onError?.Invoke(error);
                }
            );
        }

        /// <summary>
        /// Logout and clear session
        /// </summary>
        public void Logout()
        {
            authToken = null;
            currentPlayer = null;
            ApiClient.Instance.ClearAuthToken();
            OnLogout?.Invoke();
        }

        /// <summary>
        /// Check if user is authenticated
        /// </summary>
        public bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(authToken) && currentPlayer != null;
        }

        /// <summary>
        /// Get current player profile (may be null if not logged in)
        /// </summary>
        public PlayerProfile GetCurrentPlayer()
        {
            return currentPlayer;
        }
    }
}
