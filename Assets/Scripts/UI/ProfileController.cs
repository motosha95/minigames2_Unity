using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using Minigames.Data;
using Minigames.Managers;

namespace Minigames.UI
{
    /// <summary>
    /// Controls the player profile UI panel.
    /// Displays profile data from GET /api/App/player/profile:
    /// username, countryCode, icon, frame, availableKeys
    /// </summary>
    public class ProfileController : MonoBehaviour
    {
        [Header("Profile Info")]
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI displayNameText;
        [SerializeField] private TextMeshProUGUI countryCodeText;
        [SerializeField] private TextMeshProUGUI availableKeysText;

        [Header("Avatar (icon/frame from profile)")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image frameImage;

        [Header("Edit Profile")]
        [SerializeField] private TMP_InputField displayNameInput;
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private TMP_InputField countryCodeInput;
        [SerializeField] private Button updateProfileButton;
        [SerializeField] private Button updateDisplayNameButton; // Legacy: updates only display name

        [Header("Actions")]
        [SerializeField] private Button logoutButton;

        [Header("Loading")]
        [SerializeField] private GameObject loadingIndicator;

        private void Start()
        {
            if (updateProfileButton != null)
                updateProfileButton.onClick.AddListener(UpdateProfile);

            if (updateDisplayNameButton != null)
                updateDisplayNameButton.onClick.AddListener(UpdateDisplayName);

            if (logoutButton != null)
                logoutButton.onClick.AddListener(OnLogoutClicked);

            SubscribeToEvents();
            LoadProfile();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            PlayerProfileManager.Instance.OnProfileUpdated += HandleProfileUpdated;
        }

        private void UnsubscribeFromEvents()
        {
            if (PlayerProfileManager.Instance != null)
                PlayerProfileManager.Instance.OnProfileUpdated -= HandleProfileUpdated;
        }

        private void LoadProfile()
        {
            ShowLoading(true);
            PlayerProfileManager.Instance.RefreshProfile(
                _ => ShowLoading(false),
                error =>
                {
                    ShowLoading(false);
                    PopupManager.Instance.ShowError("Profile", $"Failed to load: {error}");
                }
            );
        }

        private void UpdateProfile()
        {
            string username = (usernameInput != null ? usernameInput.text : null) ?? (displayNameInput != null ? displayNameInput.text : null);
            if (string.IsNullOrWhiteSpace(username))
            {
                PopupManager.Instance.ShowError("Profile", "Username is required (3-100 characters).");
                return;
            }

            var request = new PlayerProfileUpdateRequest
            {
                username = username.Trim(),
                countryCode = countryCodeInput != null ? countryCodeInput.text : null
            };

            AuthManager.Instance.UpdateProfile(
                request,
                (profile) =>
                {
                    PopupManager.Instance.ShowMessage("Profile", "Profile updated.");
                    HandleProfileUpdated(profile);
                },
                (error) => PopupManager.Instance.ShowError("Profile", $"Failed to update: {error}")
            );
        }

        private void UpdateDisplayName()
        {
            if (displayNameInput != null && !string.IsNullOrEmpty(displayNameInput.text))
            {
                PlayerProfileManager.Instance.UpdateDisplayName(
                    displayNameInput.text,
                    (profile) => PopupManager.Instance.ShowMessage("Profile", "Display name updated."),
                    (error) => PopupManager.Instance.ShowError("Profile", $"Failed to update: {error}")
                );
            }
        }

        private void OnLogoutClicked()
        {
            AuthManager.Instance.Logout();
        }

        private void HandleProfileUpdated(PlayerProfile profile)
        {
            if (profile == null)
            {
                ClearProfile();
                return;
            }

            if (usernameText != null)
                usernameText.text = profile.username ?? "Unknown";

            if (displayNameText != null)
                displayNameText.text = profile.displayName ?? profile.username ?? "Unknown";

            if (countryCodeText != null)
                countryCodeText.text = string.IsNullOrEmpty(profile.countryCode) ? "—" : profile.countryCode;

            if (availableKeysText != null)
                availableKeysText.text = profile.keysBalance.ToString();

            if (displayNameInput != null)
                displayNameInput.text = profile.displayName ?? profile.username ?? "";

            if (usernameInput != null)
                usernameInput.text = profile.username ?? "";

            if (countryCodeInput != null)
                countryCodeInput.text = profile.countryCode ?? "";

            if (!string.IsNullOrEmpty(profile.icon))
                LoadProfileImage(profile.icon, iconImage);
            else if (iconImage != null)
                iconImage.gameObject.SetActive(false);

            if (!string.IsNullOrEmpty(profile.frame))
                LoadProfileImage(profile.frame, frameImage);
            else if (frameImage != null)
                frameImage.gameObject.SetActive(false);
        }

        private void ClearProfile()
        {
            if (usernameText != null) usernameText.text = "";
            if (displayNameText != null) displayNameText.text = "";
            if (countryCodeText != null) countryCodeText.text = "";
            if (availableKeysText != null) availableKeysText.text = "";
            if (displayNameInput != null) displayNameInput.text = "";
            if (usernameInput != null) usernameInput.text = "";
            if (countryCodeInput != null) countryCodeInput.text = "";
        }

        private void LoadProfileImage(string url, Image target)
        {
            if (target == null || string.IsNullOrEmpty(url)) return;
            if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                target.gameObject.SetActive(false);
                return;
            }
            StartCoroutine(LoadImageCoroutine(url, target));
        }

        private System.Collections.IEnumerator LoadImageCoroutine(string url, Image target)
        {
            using (var request = UnityWebRequestTexture.GetTexture(url))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success && request.downloadHandler is DownloadHandlerTexture dht)
                {
                    var tex = dht.texture;
                    var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    target.sprite = sprite;
                    target.gameObject.SetActive(true);
                }
                else
                {
                    target.gameObject.SetActive(false);
                }
            }
        }

        private void ShowLoading(bool show)
        {
            if (loadingIndicator != null)
                loadingIndicator.SetActive(show);
        }
    }
}
