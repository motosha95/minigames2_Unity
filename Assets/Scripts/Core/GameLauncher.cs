using UnityEngine;
using Minigames.Data;
using Minigames.Managers;
using Minigames.Games;

namespace Minigames.Core
{
    /// <summary>
    /// Helper class to launch games from game scenes.
    /// Finds the IMinigame component and initializes it with session data.
    /// </summary>
    public class GameLauncher : MonoBehaviour
    {
        private void Start()
        {
            // Wait a frame to ensure scene is fully loaded
            StartCoroutine(InitializeGameCoroutine());
        }

        private System.Collections.IEnumerator InitializeGameCoroutine()
        {
            yield return null;

            // Get current session
            GameSessionData session = GameSessionManager.Instance.GetCurrentSession();
            
            if (session == null)
            {
                Debug.LogError("GameLauncher: No active session found. Returning to main scene.");
                SceneNavigationManager.Instance.UnloadGameScene();
                yield break;
            }

            // Find IMinigame component in the scene
            // Try BaseMinigame first (most common)
            BaseMinigame baseMinigame = FindObjectOfType<BaseMinigame>();
            IMinigame minigame = baseMinigame;
            
            // If not found, search for any MonoBehaviour that implements IMinigame
            if (minigame == null)
            {
                MonoBehaviour[] allBehaviours = FindObjectsOfType<MonoBehaviour>();
                foreach (var behaviour in allBehaviours)
                {
                    if (behaviour is IMinigame)
                    {
                        minigame = behaviour as IMinigame;
                        break;
                    }
                }
            }

            if (minigame == null)
            {
                Debug.LogError("GameLauncher: No IMinigame component found in scene. Returning to main scene.");
                SceneNavigationManager.Instance.UnloadGameScene();
                yield break;
            }

            // Initialize and start the game
            minigame.Initialize(session);
            minigame.StartGame();
            
            Debug.Log($"GameLauncher: Game {minigame.GetGameId()} initialized and started");
        }
    }
}
