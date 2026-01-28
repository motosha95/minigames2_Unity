using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Minigames.Managers
{
    /// <summary>
    /// Manages scene navigation and loading.
    /// Handles transitions between Main Scene and Game Scenes using regular scene loading (non-additive).
    /// </summary>
    public class SceneNavigationManager : MonoBehaviour
    {
        private static SceneNavigationManager _instance;
        public static SceneNavigationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SceneNavigationManager");
                    _instance = go.AddComponent<SceneNavigationManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // Events
        public event Action<string> OnSceneLoading;
        public event Action<string> OnSceneLoaded;

        private const string MAIN_SCENE_NAME = "MainScene";
        private string currentScene = null;

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
        /// Load the main scene (hub). Replaces current scene.
        /// </summary>
        public void LoadMainScene(Action onComplete = null)
        {
            StartCoroutine(LoadSceneCoroutine(MAIN_SCENE_NAME, onComplete));
        }

        /// <summary>
        /// Load a game scene. Replaces current scene.
        /// </summary>
        public void LoadGameScene(string sceneName, Action onComplete = null)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("SceneNavigationManager: Cannot load game scene - scene name is empty");
                onComplete?.Invoke();
                return;
            }

            StartCoroutine(LoadSceneCoroutine(sceneName, onComplete));
        }

        /// <summary>
        /// Return to main scene (replaces current scene).
        /// </summary>
        public void UnloadGameScene(Action onComplete = null)
        {
            LoadMainScene(onComplete);
        }

        private IEnumerator LoadSceneCoroutine(string sceneName, Action onComplete)
        {
            OnSceneLoading?.Invoke(sceneName);
            
            // Load scene in single mode (replaces current scene)
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Update current scene tracking
            currentScene = sceneName;
            
            OnSceneLoaded?.Invoke(sceneName);
            onComplete?.Invoke();
        }

        /// <summary>
        /// Get current scene name
        /// </summary>
        public string GetCurrentScene()
        {
            return currentScene;
        }

        /// <summary>
        /// Check if currently in main scene
        /// </summary>
        public bool IsMainScene()
        {
            return currentScene == MAIN_SCENE_NAME;
        }

        /// <summary>
        /// Check if a game scene is currently loaded
        /// </summary>
        public bool IsGameSceneLoaded()
        {
            return !string.IsNullOrEmpty(currentScene) && currentScene != MAIN_SCENE_NAME;
        }

        /// <summary>
        /// Get current game scene name (null if in main scene)
        /// </summary>
        public string GetCurrentGameScene()
        {
            return IsGameSceneLoaded() ? currentScene : null;
        }
    }
}
