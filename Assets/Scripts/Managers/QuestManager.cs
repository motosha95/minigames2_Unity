using System;
using System.Collections.Generic;
using UnityEngine;
using Minigames.Core;
using Minigames.Data;

namespace Minigames.Managers
{
    /// <summary>
    /// Manages quests and milestones data.
    /// Note: This is a placeholder implementation. Adjust API endpoints based on your backend.
    /// </summary>
    public class QuestManager : MonoBehaviour
    {
        private static QuestManager _instance;
        public static QuestManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("QuestManager");
                    _instance = go.AddComponent<QuestManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // Events
        public event Action<List<Quest>> OnQuestsLoaded;
        public event Action<List<Milestone>> OnMilestonesLoaded;
        public event Action<string> OnQuestError;

        private List<Quest> weeklyQuests = new List<Quest>();
        private List<Milestone> milestones = new List<Milestone>();

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
        /// Load weekly quests from API
        /// TODO: Replace with actual API endpoint when available
        /// </summary>
        public void LoadWeeklyQuests(Action<List<Quest>> onSuccess = null, Action<string> onError = null)
        {
            // Placeholder: If you have an API endpoint, use it here
            // For now, this is a stub that can be populated with mock data or actual API call
            
            // Example API call (uncomment when endpoint is available):
            /*
            ApiClient.Instance.Get<QuestListResponse>(
                "/api/App/quests/weekly",
                (response) =>
                {
                    weeklyQuests = response.data.quests ?? new List<Quest>();
                    OnQuestsLoaded?.Invoke(weeklyQuests);
                    onSuccess?.Invoke(weeklyQuests);
                },
                (error) =>
                {
                    OnQuestError?.Invoke(error);
                    onError?.Invoke(error);
                }
            );
            */
            
            // Temporary: Return empty list or mock data
            Debug.LogWarning("QuestManager: LoadWeeklyQuests - API endpoint not implemented yet");
            weeklyQuests = new List<Quest>();
            OnQuestsLoaded?.Invoke(weeklyQuests);
            onSuccess?.Invoke(weeklyQuests);
        }

        /// <summary>
        /// Load milestones from API
        /// TODO: Replace with actual API endpoint when available
        /// </summary>
        public void LoadMilestones(Action<List<Milestone>> onSuccess = null, Action<string> onError = null)
        {
            // Placeholder: If you have an API endpoint, use it here
            
            // Example API call (uncomment when endpoint is available):
            /*
            ApiClient.Instance.Get<MilestoneListResponse>(
                "/api/App/milestones",
                (response) =>
                {
                    milestones = response.data.milestones ?? new List<Milestone>();
                    OnMilestonesLoaded?.Invoke(milestones);
                    onSuccess?.Invoke(milestones);
                },
                (error) =>
                {
                    OnQuestError?.Invoke(error);
                    onError?.Invoke(error);
                }
            );
            */
            
            // Temporary: Return empty list or mock data
            Debug.LogWarning("QuestManager: LoadMilestones - API endpoint not implemented yet");
            milestones = new List<Milestone>();
            OnMilestonesLoaded?.Invoke(milestones);
            onSuccess?.Invoke(milestones);
        }

        /// <summary>
        /// Get cached weekly quests
        /// </summary>
        public List<Quest> GetWeeklyQuests()
        {
            return new List<Quest>(weeklyQuests);
        }

        /// <summary>
        /// Get cached milestones
        /// </summary>
        public List<Milestone> GetMilestones()
        {
            return new List<Milestone>(milestones);
        }
    }
}
