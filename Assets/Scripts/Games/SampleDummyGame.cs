using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Minigames.Data;

namespace Minigames.Games
{
    /// <summary>
    /// Sample dummy minigame for testing the game session lifecycle.
    /// This demonstrates how a game should implement IMinigame interface.
    /// </summary>
    public class SampleDummyGame : BaseMinigame
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Button endGameButton;
        [SerializeField] private Button returnButton;

        private int currentScore = 0;
        private float gameTimer = 0f;
        private const float GAME_DURATION = 30f; // 30 seconds

        private void Start()
        {
            if (endGameButton != null)
            {
                endGameButton.onClick.AddListener(OnEndGameClicked);
            }

            if (returnButton != null)
            {
                returnButton.onClick.AddListener(ReturnToMainScene);
            }

            // Update UI
            UpdateScoreDisplay();
        }

        protected override void OnGameStarted()
        {
            base.OnGameStarted();
            currentScore = 0;
            gameTimer = 0f;
            StartCoroutine(GameTimerCoroutine());
            Debug.Log("SampleDummyGame: Game started!");
        }

        private IEnumerator GameTimerCoroutine()
        {
            while (isGameActive && gameTimer < GAME_DURATION)
            {
                gameTimer += Time.deltaTime;
                UpdateTimerDisplay();
                yield return null;
            }

            // Auto-end game when timer expires
            if (isGameActive)
            {
                EndGame(currentScore);
            }
        }

        private void Update()
        {
            if (!isGameActive) return;

            // Simple score increment on spacebar
            if (Input.GetKeyDown(KeyCode.Space))
            {
                currentScore += 10;
                UpdateScoreDisplay();
            }
        }

        private void UpdateScoreDisplay()
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {currentScore}";
            }
        }

        private void UpdateTimerDisplay()
        {
            if (timerText != null)
            {
                float remaining = GAME_DURATION - gameTimer;
                timerText.text = $"Time: {remaining:F1}s";
            }
        }

        private void OnEndGameClicked()
        {
            if (isGameActive)
            {
                EndGame(currentScore);
            }
        }

        protected override void OnGameEnded(int score, Dictionary<string, object> metadata)
        {
            base.OnGameEnded(score, metadata);
            Debug.Log($"SampleDummyGame: Game ended with score {score}");
        }
    }
}
