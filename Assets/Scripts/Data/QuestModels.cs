using System;
using System.Collections.Generic;

namespace Minigames.Data
{
    /// <summary>
    /// Models for quests/milestones.
    /// Note: These are placeholder models. Adjust based on your actual API structure.
    /// </summary>
    [Serializable]
    public class Quest
    {
        public string id;
        public string title;
        public string description;
        public string type; // "daily", "weekly", "milestone"
        public int targetValue;
        public int currentValue;
        public int reward;
        public string rewardType; // "score", "currency", "item"
        public bool isCompleted;
        public bool isClaimed;
        public DateTime? startDate;
        public DateTime? endDate;
        public Dictionary<string, object> metadata;
    }

    [Serializable]
    public class QuestListResponse
    {
        public List<Quest> quests;
        public int total;
    }

    [Serializable]
    public class Milestone
    {
        public string id;
        public string title;
        public string description;
        public int targetScore;
        public int currentScore;
        public int reward;
        public string rewardType;
        public bool isCompleted;
        public bool isClaimed;
        public Dictionary<string, object> metadata;
    }

    [Serializable]
    public class MilestoneListResponse
    {
        public List<Milestone> milestones;
        public int total;
    }
}
