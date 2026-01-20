using UnityEngine;
using UnityEngine.UI;
using Minigames.Data;

namespace Minigames.UI
{
    /// <summary>
    /// Controller for individual leaderboard entry.
    /// </summary>
    public class LeaderboardEntryController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text rankText;
        [SerializeField] private Text playerNameText;
        [SerializeField] private Text scoreText;

        public void Setup(LeaderboardEntry entry)
        {
            if (rankText != null)
                rankText.text = $"#{entry.rank}";

            if (playerNameText != null)
                playerNameText.text = entry.playerName ?? "Unknown";

            if (scoreText != null)
                scoreText.text = entry.score.ToString();
        }
    }
}
