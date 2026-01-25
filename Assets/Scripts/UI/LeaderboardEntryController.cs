using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Minigames.Data;

namespace Minigames.UI
{
    /// <summary>
    /// Controller for individual leaderboard entry.
    /// </summary>
    public class LeaderboardEntryController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI rankText;
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI scoreText;

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
