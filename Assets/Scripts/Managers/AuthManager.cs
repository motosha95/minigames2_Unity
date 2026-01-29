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

        // PlayerPrefs keys for guest persistence
        private const string GUEST_SOCIAL_MEDIA_ID_KEY = "GuestSocialMediaId";
        private const string GUEST_TENANT_ID_KEY = "GuestTenantId";

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
        /// Register a new player using RegisterTenantPlayerDto (username, socialMediaId, countryCode)
        /// </summary>
        public void Register(PlayerRegisterRequest request, Action<PlayerProfile> onSuccess, Action<string> onError)
        {
            // Convert to backend DTO format
            var registerDto = new RegisterTenantPlayerDto
            {
                username = request.username,
                socialMediaId = request.socialMediaId,
                countryCode = request.countryCode
            };

            ApiClient.Instance.Post<RegisterTenantPlayerDto, TenantPlayerAuthResultDto>(
                "/api/App/player/register",
                registerDto,
                (response) =>
                {
                    if (response.data.isSuccess && !string.IsNullOrEmpty(response.data.token) && response.data.player != null)
                    {
                        authToken = response.data.token;
                        currentPlayer = DtoConverter.ToPlayerProfile(response.data.player);
                        ApiClient.Instance.SetAuthToken(authToken);
                        OnLoginSuccess?.Invoke(currentPlayer);
                        onSuccess?.Invoke(currentPlayer);
                    }
                    else
                    {
                        string error = response.data.errorMessage ?? "Registration failed";
                        OnAuthError?.Invoke(error);
                        onError?.Invoke(error);
                    }
                },
                (error) =>
                {
                    OnAuthError?.Invoke(error);
                    onError?.Invoke(error);
                }
            );
        }

        /// <summary>
        /// Login with social media ID (backend only supports socialMediaId).
        /// </summary>
        public void Login(PlayerLoginRequest request, Action<PlayerProfile> onSuccess, Action<string> onError)
        {
            // Convert to backend DTO format
            var loginDto = new LoginTenantPlayerDto
            {
                socialMediaId = request.socialMediaId
            };

            ApiClient.Instance.Post<LoginTenantPlayerDto, TenantPlayerAuthResultDto>(
                "/api/App/player/login",
                loginDto,
                (response) =>
                {
                    if (response.data.isSuccess && !string.IsNullOrEmpty(response.data.token) && response.data.player != null)
                    {
                        Debug.Log($"AuthManager: Login response received - player.availableKeys: {response.data.player.availableKeys}");
                        authToken = response.data.token;
                        currentPlayer = DtoConverter.ToPlayerProfile(response.data.player);
                        Debug.Log($"AuthManager: After conversion - currentPlayer.keysBalance: {currentPlayer?.keysBalance}");
                        ApiClient.Instance.SetAuthToken(authToken);
                        OnLoginSuccess?.Invoke(currentPlayer);
                        onSuccess?.Invoke(currentPlayer);
                    }
                    else
                    {
                        string error = response.data.errorMessage ?? "Login failed";
                        OnAuthError?.Invoke(error);
                        onError?.Invoke(error);
                    }
                },
                (error) =>
                {
                    OnAuthError?.Invoke(error);
                    onError?.Invoke(error);
                }
            );
        }

        /// <summary>
        /// Login with social media ID (alternative to username/email/password).
        /// Requires tenant ID to be set in ApiClient.
        /// </summary>
        public void LoginWithSocialMedia(string socialMediaId, Action<PlayerProfile> onSuccess, Action<string> onError)
        {
            var request = new PlayerLoginRequest { socialMediaId = socialMediaId };
            Login(request, onSuccess, onError);
        }

        /// <summary>
        /// Register a guest player with auto-generated username (guest1, guest2, etc.).
        /// No request body required - backend generates username automatically.
        /// Returns token and player profile on success.
        /// Saves socialMediaId locally for future auto-login.
        /// </summary>
        public void RegisterGuest(Action<PlayerProfile> onSuccess, Action<string> onError)
        {
            // Guest registration endpoint doesn't require a request body, but we send empty object for compatibility
            ApiClient.Instance.Post<object, TenantPlayerAuthResultDto>(
                "/api/App/player/register/guest",
                new object(),
                (response) =>
                {
                    if (response.data.isSuccess && !string.IsNullOrEmpty(response.data.token) && response.data.player != null)
                    {
                        authToken = response.data.token;
                        currentPlayer = DtoConverter.ToPlayerProfile(response.data.player);
                        ApiClient.Instance.SetAuthToken(authToken);
                        
                        // Save guest info for future auto-login
                        SaveGuestInfo(currentPlayer.socialMediaId);
                        
                        OnLoginSuccess?.Invoke(currentPlayer);
                        onSuccess?.Invoke(currentPlayer);
                    }
                    else
                    {
                        string error = response.data.errorMessage ?? "Guest registration failed";
                        OnAuthError?.Invoke(error);
                        onError?.Invoke(error);
                    }
                },
                (error) =>
                {
                    OnAuthError?.Invoke(error);
                    onError?.Invoke(error);
                }
            );
        }

        /// <summary>
        /// Save guest socialMediaId and tenant ID locally for future auto-login
        /// </summary>
        private void SaveGuestInfo(string socialMediaId)
        {
            if (!string.IsNullOrEmpty(socialMediaId))
            {
                PlayerPrefs.SetString(GUEST_SOCIAL_MEDIA_ID_KEY, socialMediaId);
                string tenantId = ApiClient.Instance.GetTenantId();
                if (!string.IsNullOrEmpty(tenantId))
                {
                    PlayerPrefs.SetString(GUEST_TENANT_ID_KEY, tenantId);
                }
                PlayerPrefs.Save();
                Debug.Log($"AuthManager: Saved guest info - socialMediaId: {socialMediaId}");
            }
        }

        /// <summary>
        /// Get saved guest socialMediaId if available
        /// </summary>
        public string GetSavedGuestSocialMediaId()
        {
            return PlayerPrefs.GetString(GUEST_SOCIAL_MEDIA_ID_KEY, null);
        }

        /// <summary>
        /// Get saved guest tenant ID if available
        /// </summary>
        public string GetSavedGuestTenantId()
        {
            return PlayerPrefs.GetString(GUEST_TENANT_ID_KEY, null);
        }

        /// <summary>
        /// Check if saved guest info exists and matches current tenant
        /// </summary>
        public bool HasSavedGuestInfo(string currentTenantId)
        {
            string savedSocialMediaId = GetSavedGuestSocialMediaId();
            string savedTenantId = GetSavedGuestTenantId();
            
            return !string.IsNullOrEmpty(savedSocialMediaId) && 
                   !string.IsNullOrEmpty(savedTenantId) && 
                   savedTenantId == currentTenantId;
        }

        /// <summary>
        /// Clear saved guest info (e.g., on logout or when switching tenants)
        /// </summary>
        public void ClearGuestInfo()
        {
            PlayerPrefs.DeleteKey(GUEST_SOCIAL_MEDIA_ID_KEY);
            PlayerPrefs.DeleteKey(GUEST_TENANT_ID_KEY);
            PlayerPrefs.Save();
            Debug.Log("AuthManager: Cleared saved guest info");
        }

        /// <summary>
        /// Fetch current player profile
        /// </summary>
        public void FetchProfile(Action<PlayerProfile> onSuccess = null, Action<string> onError = null)
        {
            ApiClient.Instance.Get<PlayerDto>(
                "/api/App/player/profile",
                (response) =>
                {
                    currentPlayer = DtoConverter.ToPlayerProfile(response.data);
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
        /// Update player profile using UpdatePlayerProfileDto (username, countryCode, icon, frame)
        /// </summary>
        public void UpdateProfile(PlayerProfileUpdateRequest request, Action<PlayerProfile> onSuccess, Action<string> onError)
        {
            // Convert to backend DTO format
            var updateDto = new UpdatePlayerProfileDto
            {
                username = request.displayName ?? request.username,
                countryCode = request.countryCode,
                icon = request.icon,
                frame = request.frame
            };

            ApiClient.Instance.Put<UpdatePlayerProfileDto, PlayerDto>(
                "/api/App/player/profile",
                updateDto,
                (response) =>
                {
                    currentPlayer = DtoConverter.ToPlayerProfile(response.data);
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
        /// Note: Guest info is kept for auto-login on next app start
        /// </summary>
        public void Logout()
        {
            authToken = null;
            currentPlayer = null;
            ApiClient.Instance.ClearAuthToken();
            OnLogout?.Invoke();
        }

        /// <summary>
        /// Logout and clear guest info (use when switching tenants or clearing guest data)
        /// </summary>
        public void LogoutAndClearGuest()
        {
            Logout();
            ClearGuestInfo();
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

        /// <summary>
        /// Request OTP for registration. Backend sends OTP to the given email.
        /// </summary>
        public void RequestRegistrationOtp(string email, string username, Action onSuccess, Action<string> onError)
        {
            var request = new RequestRegistrationOtpRequest { email = email, username = username };
            ApiClient.Instance.Post<RequestRegistrationOtpRequest, SendOtpResponse>(
                "/api/App/player/register/send-otp",
                request,
                _ => onSuccess?.Invoke(),
                error =>
                {
                    OnAuthError?.Invoke(error);
                    onError?.Invoke(error);
                }
            );
        }

        /// <summary>
        /// Verify OTP and complete registration. Returns token and profile on success.
        /// </summary>
        public void VerifyOtpAndRegister(VerifyOtpAndRegisterRequest request, Action<PlayerProfile> onSuccess, Action<string> onError)
        {
            ApiClient.Instance.Post<VerifyOtpAndRegisterRequest, PlayerLoginResponse>(
                "/api/App/player/register/verify-otp",
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
    }
}
