using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Minigames.Data;
using Minigames.Managers;

namespace Minigames.UI
{
    /// <summary>
    /// Controls the weekly quests/milestones UI panel.
    /// Displays quests and milestones for the player.
    /// </summary>
    public class QuestsController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Dropdown questTypeDropdown;
        [SerializeField] private Transform questsListContainer;
        [SerializeField] private GameObject questItemPrefab;
        [SerializeField] private Button refreshButton;
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private GameObject emptyStatePanel;

        private List<Quest> weeklyQuests = new List<Quest>();
        private List<Milestone> milestones = new List<Milestone>();

        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
            LoadQuests();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeUI()
        {
            if (questTypeDropdown != null)
            {
                questTypeDropdown.options.Clear();
                questTypeDropdown.options.Add(new Dropdown.OptionData("Weekly Quests"));
                questTypeDropdown.options.Add(new Dropdown.OptionData("Milestones"));
                questTypeDropdown.value = 0;
                questTypeDropdown.onValueChanged.AddListener(OnQuestTypeChanged);
            }

            if (refreshButton != null)
            {
                refreshButton.onClick.AddListener(LoadQuests);
            }
        }

        private void SubscribeToEvents()
        {
            QuestManager.Instance.OnQuestsLoaded += HandleQuestsLoaded;
            QuestManager.Instance.OnMilestonesLoaded += HandleMilestonesLoaded;
            QuestManager.Instance.OnQuestError += HandleQuestError;
        }

        private void UnsubscribeFromEvents()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestsLoaded -= HandleQuestsLoaded;
                QuestManager.Instance.OnMilestonesLoaded -= HandleMilestonesLoaded;
                QuestManager.Instance.OnQuestError -= HandleQuestError;
            }
        }

        private void LoadQuests()
        {
            ShowLoading(true);
            
            if (questTypeDropdown == null || questTypeDropdown.value == 0)
            {
                QuestManager.Instance.LoadWeeklyQuests();
            }
            else
            {
                QuestManager.Instance.LoadMilestones();
            }
        }

        private void OnQuestTypeChanged(int index)
        {
            LoadQuests();
        }

        private void ShowLoading(bool show)
        {
            if (loadingText != null)
                loadingText.gameObject.SetActive(show);
        }

        private void HandleQuestsLoaded(List<Quest> quests)
        {
            weeklyQuests = quests;
            ShowLoading(false);
            PopulateQuestsList();
        }

        private void HandleMilestonesLoaded(List<Milestone> milestones)
        {
            this.milestones = milestones;
            ShowLoading(false);
            PopulateMilestonesList();
        }

        private void HandleQuestError(string error)
        {
            ShowLoading(false);
            PopupManager.Instance.ShowError("Quests Error", $"Failed to load quests: {error}");
        }

        private void PopulateQuestsList()
        {
            if (questsListContainer == null || questItemPrefab == null)
                return;

            // Clear existing items
            foreach (Transform child in questsListContainer)
            {
                Destroy(child.gameObject);
            }

            // Show empty state if no quests
            if (weeklyQuests.Count == 0)
            {
                if (emptyStatePanel != null)
                    emptyStatePanel.SetActive(true);
                return;
            }

            if (emptyStatePanel != null)
                emptyStatePanel.SetActive(false);

            // Create items for each quest
            foreach (var quest in weeklyQuests)
            {
                GameObject itemObj = Instantiate(questItemPrefab, questsListContainer);
                QuestItemController itemController = itemObj.GetComponent<QuestItemController>();
                
                if (itemController != null)
                {
                    itemController.SetupQuest(quest);
                }
            }
        }

        private void PopulateMilestonesList()
        {
            if (questsListContainer == null || questItemPrefab == null)
                return;

            // Clear existing items
            foreach (Transform child in questsListContainer)
            {
                Destroy(child.gameObject);
            }

            // Show empty state if no milestones
            if (milestones.Count == 0)
            {
                if (emptyStatePanel != null)
                    emptyStatePanel.SetActive(true);
                return;
            }

            if (emptyStatePanel != null)
                emptyStatePanel.SetActive(false);

            // Create items for each milestone
            foreach (var milestone in milestones)
            {
                GameObject itemObj = Instantiate(questItemPrefab, questsListContainer);
                QuestItemController itemController = itemObj.GetComponent<QuestItemController>();
                
                if (itemController != null)
                {
                    itemController.SetupMilestone(milestone);
                }
            }
        }
    }
}
