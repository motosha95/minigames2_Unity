using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Minigames.Data;

namespace Minigames.UI
{
    /// <summary>
    /// Controller for individual quest/milestone item.
    /// </summary>
    public class QuestItemController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private Button claimButton;
        [SerializeField] private GameObject completedBadge;
        [SerializeField] private GameObject claimedBadge;

        private Quest quest;
        private Milestone milestone;

        private void Start()
        {
            if (claimButton != null)
            {
                claimButton.onClick.AddListener(OnClaimClicked);
            }
        }

        /// <summary>
        /// Setup the item with quest data
        /// </summary>
        public void SetupQuest(Quest questData)
        {
            quest = questData;
            milestone = null;

            if (titleText != null)
                titleText.text = questData.title ?? "Quest";

            if (descriptionText != null)
                descriptionText.text = questData.description ?? "";

            UpdateProgress(questData.currentValue, questData.targetValue);
            UpdateReward(questData.reward, questData.rewardType);
            UpdateStatus(questData.isCompleted, questData.isClaimed);
        }

        /// <summary>
        /// Setup the item with milestone data
        /// </summary>
        public void SetupMilestone(Milestone milestoneData)
        {
            milestone = milestoneData;
            quest = null;

            if (titleText != null)
                titleText.text = milestoneData.title ?? "Milestone";

            if (descriptionText != null)
                descriptionText.text = milestoneData.description ?? "";

            UpdateProgress(milestoneData.currentScore, milestoneData.targetScore);
            UpdateReward(milestoneData.reward, milestoneData.rewardType);
            UpdateStatus(milestoneData.isCompleted, milestoneData.isClaimed);
        }

        private void UpdateProgress(int current, int target)
        {
            float progress = target > 0 ? (float)current / target : 0f;
            progress = Mathf.Clamp01(progress);

            if (progressText != null)
                progressText.text = $"{current} / {target}";

            if (progressSlider != null)
                progressSlider.value = progress;
        }

        private void UpdateReward(int reward, string rewardType)
        {
            if (rewardText != null)
                rewardText.text = $"Reward: {reward} {rewardType}";
        }

        private void UpdateStatus(bool isCompleted, bool isClaimed)
        {
            if (completedBadge != null)
                completedBadge.SetActive(isCompleted);

            if (claimedBadge != null)
                claimedBadge.SetActive(isClaimed);

            if (claimButton != null)
            {
                claimButton.interactable = isCompleted && !isClaimed;
                claimButton.gameObject.SetActive(isCompleted && !isClaimed);
            }
        }

        private void OnClaimClicked()
        {
            // Note: Claiming rewards is handled by host app, not Unity
            // Just notify or show message
            string itemName = quest != null ? quest.title : (milestone != null ? milestone.title : "Item");
            PopupManager.Instance.ShowMessage(
                "Claim Reward",
                $"Reward for {itemName} will be processed by the host app."
            );
        }
    }
}
