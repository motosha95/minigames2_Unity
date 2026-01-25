using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Minigames.Data;
using Minigames.Managers;

namespace Minigames.UI
{
    /// <summary>
    /// Controls the player profile UI panel.
    /// </summary>
    public class ProfileController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI displayNameText;
        [SerializeField] private TextMeshProUGUI totalScoreText;
        [SerializeField] private TextMeshProUGUI weeklyScoreText;
        [SerializeField] private Button refreshButton;
        [SerializeField] private InputField displayNameInput;
        [SerializeField] private Button updateDisplayNameButton;

        private void Start()
        {
            if (refreshButton != null)
                refreshButton.onClick.AddListener(RefreshProfile);

            if (updateDisplayNameButton != null)
                updateDisplayNameButton.onClick.AddListener(UpdateDisplayName);

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
            LeaderboardManager.Instance.OnMyTotalScoreLoaded += HandleTotalScoreLoaded;
            LeaderboardManager.Instance.OnMyWeeklyScoreLoaded += HandleWeeklyScoreLoaded;
        }

        private void UnsubscribeFromEvents()
        {
            if (PlayerProfileManager.Instance != null)
                PlayerProfileManager.Instance.OnProfileUpdated -= HandleProfileUpdated;

            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.OnMyTotalScoreLoaded -= HandleTotalScoreLoaded;
                LeaderboardManager.Instance.OnMyWeeklyScoreLoaded -= HandleWeeklyScoreLoaded;
            }
        }

        private void LoadProfile()
        {
            PlayerProfileManager.Instance.RefreshProfile();
            LeaderboardManager.Instance.LoadMyTotalScore();
            LeaderboardManager.Instance.LoadMyWeeklyScore();
        }

        private void RefreshProfile()
        {
            LoadProfile();
        }

        private void UpdateDisplayName()
        {
            if (displayNameInput != null && !string.IsNullOrEmpty(displayNameInput.text))
            {
                PlayerProfileManager.Instance.UpdateDisplayName(
                    displayNameInput.text,
                    (profile) => Debug.Log("Display name updated successfully"),
                    (error) => Debug.LogError($"Failed to update display name: {error}")
                );
            }
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

            if (displayNameInput != null)
                displayNameInput.text = profile.displayName ?? "";
        }

        private void HandleTotalScoreLoaded(int score)
        {
            if (totalScoreText != null)
                totalScoreText.text = $"Total Score: {score}";
        }

        private void HandleWeeklyScoreLoaded(int score)
        {
            if (weeklyScoreText != null)
                weeklyScoreText.text = $"Weekly Score: {score}";
        }

        private void ClearProfile()
        {
            if (usernameText != null) usernameText.text = "";
            if (displayNameText != null) displayNameText.text = "";
            if (totalScoreText != null) totalScoreText.text = "";
            if (weeklyScoreText != null) weeklyScoreText.text = "";
        }
    }
}
