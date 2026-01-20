using System;
using UnityEngine;
using Minigames.Data;

namespace Minigames.Managers
{
    /// <summary>
    /// Manages player profile data and updates.
    /// Acts as a bridge between AuthManager and UI components.
    /// </summary>
    public class PlayerProfileManager : MonoBehaviour
    {
        private static PlayerProfileManager _instance;
        public static PlayerProfileManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("PlayerProfileManager");
                    _instance = go.AddComponent<PlayerProfileManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // Events
        public event Action<PlayerProfile> OnProfileUpdated;
        public event Action<int> OnTotalScoreUpdated;
        public event Action<int> OnWeeklyScoreUpdated;

        private PlayerProfile currentProfile;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Subscribe to auth events
            AuthManager.Instance.OnLoginSuccess += HandleLoginSuccess;
            AuthManager.Instance.OnLogout += HandleLogout;
        }

        private void OnDestroy()
        {
            if (AuthManager.Instance != null)
            {
                AuthManager.Instance.OnLoginSuccess -= HandleLoginSuccess;
                AuthManager.Instance.OnLogout -= HandleLogout;
            }
        }

        private void HandleLoginSuccess(PlayerProfile profile)
        {
            currentProfile = profile;
            OnProfileUpdated?.Invoke(profile);
        }

        private void HandleLogout()
        {
            currentProfile = null;
            OnProfileUpdated?.Invoke(null);
        }

        /// <summary>
        /// Get current player profile
        /// </summary>
        public PlayerProfile GetProfile()
        {
            return currentProfile ?? AuthManager.Instance.GetCurrentPlayer();
        }

        /// <summary>
        /// Refresh profile from server
        /// </summary>
        public void RefreshProfile(Action<PlayerProfile> onSuccess = null, Action<string> onError = null)
        {
            AuthManager.Instance.FetchProfile(
                (profile) =>
                {
                    currentProfile = profile;
                    OnProfileUpdated?.Invoke(profile);
                    onSuccess?.Invoke(profile);
                },
                onError
            );
        }

        /// <summary>
        /// Update profile display name
        /// </summary>
        public void UpdateDisplayName(string displayName, Action<PlayerProfile> onSuccess = null, Action<string> onError = null)
        {
            var request = new PlayerProfileUpdateRequest
            {
                displayName = displayName
            };

            AuthManager.Instance.UpdateProfile(
                request,
                (profile) =>
                {
                    currentProfile = profile;
                    OnProfileUpdated?.Invoke(profile);
                    onSuccess?.Invoke(profile);
                },
                onError
            );
        }
    }
}
