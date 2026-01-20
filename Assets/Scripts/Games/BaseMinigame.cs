using System.Collections.Generic;
using UnityEngine;
using Minigames.Data;
using Minigames.Managers;

namespace Minigames.Games
{
    /// <summary>
    /// Base class for minigames that provides common functionality.
    /// All minigames should inherit from this class.
    /// </summary>
    public abstract class BaseMinigame : MonoBehaviour, IMinigame
    {
        [Header("Game Configuration")]
        [SerializeField] protected string gameId;
        
        protected GameSessionData currentSession;
        protected bool isGameActive = false;

        /// <summary>
        /// Initialize the game with session data
        /// </summary>
        public virtual void Initialize(GameSessionData session)
        {
            currentSession = session;
            isGameActive = false;
        }

        /// <summary>
        /// Start the game
        /// </summary>
        public virtual void StartGame()
        {
            isGameActive = true;
            OnGameStarted();
        }

        /// <summary>
        /// End the game and notify GameSessionManager
        /// </summary>
        public virtual void EndGame(int score, Dictionary<string, object> metadata = null)
        {
            if (!isGameActive)
            {
                Debug.LogWarning($"BaseMinigame: Attempted to end game that wasn't active");
                return;
            }

            isGameActive = false;
            OnGameEnded(score, metadata);

            // Notify GameSessionManager to complete the session
            GameSessionManager.Instance.CompleteSession(
                score,
                metadata,
                (session) => OnSessionCompleted(session),
                (error) => OnSessionError(error)
            );
        }

        /// <summary>
        /// Get the game ID
        /// </summary>
        public virtual string GetGameId()
        {
            return gameId;
        }

        /// <summary>
        /// Called when game starts - override in derived classes
        /// </summary>
        protected virtual void OnGameStarted()
        {
            Debug.Log($"BaseMinigame: Game {gameId} started");
        }

        /// <summary>
        /// Called when game ends - override in derived classes
        /// </summary>
        protected virtual void OnGameEnded(int score, Dictionary<string, object> metadata)
        {
            Debug.Log($"BaseMinigame: Game {gameId} ended with score {score}");
        }

        /// <summary>
        /// Called when session is successfully completed
        /// </summary>
        protected virtual void OnSessionCompleted(GameSessionData session)
        {
            Debug.Log($"BaseMinigame: Session {session.id} completed successfully");
            // Return to main scene
            SceneNavigationManager.Instance.UnloadGameScene(() =>
            {
                Debug.Log("BaseMinigame: Returned to main scene");
            });
        }

        /// <summary>
        /// Called when session completion fails
        /// </summary>
        protected virtual void OnSessionError(string error)
        {
            Debug.LogError($"BaseMinigame: Session completion failed: {error}");
            // Still return to main scene even on error
            SceneNavigationManager.Instance.UnloadGameScene(() =>
            {
                Debug.Log("BaseMinigame: Returned to main scene after error");
            });
        }

        /// <summary>
        /// Return to main scene without completing session (abandon)
        /// </summary>
        protected virtual void ReturnToMainScene()
        {
            GameSessionManager.Instance.AbandonCurrentSession();
            SceneNavigationManager.Instance.UnloadGameScene();
        }
    }
}
