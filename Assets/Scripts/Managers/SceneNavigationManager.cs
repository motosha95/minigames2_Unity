using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Minigames.Managers
{
    /// <summary>
    /// Manages scene navigation and loading.
    /// Handles transitions between Main Scene and Game Scenes.
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
        public event Action<string> OnSceneUnloading;

        private const string MAIN_SCENE_NAME = "MainScene";
        private string currentGameScene = null;

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
        /// Load the main scene (hub)
        /// </summary>
        public void LoadMainScene(Action onComplete = null)
        {
            if (currentGameScene != null)
            {
                StartCoroutine(UnloadGameSceneThenLoadMain(onComplete));
            }
            else
            {
                StartCoroutine(LoadSceneCoroutine(MAIN_SCENE_NAME, onComplete));
            }
        }

        /// <summary>
        /// Load a game scene additively (keeps Main Scene loaded)
        /// </summary>
        public void LoadGameScene(string sceneName, Action onComplete = null)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("SceneNavigationManager: Cannot load game scene - scene name is empty");
                onComplete?.Invoke();
                return;
            }

            StartCoroutine(LoadGameSceneCoroutine(sceneName, onComplete));
        }

        /// <summary>
        /// Unload current game scene and return to main scene
        /// </summary>
        public void UnloadGameScene(Action onComplete = null)
        {
            if (string.IsNullOrEmpty(currentGameScene))
            {
                onComplete?.Invoke();
                return;
            }

            StartCoroutine(UnloadGameSceneCoroutine(currentGameScene, onComplete));
        }

        private IEnumerator LoadSceneCoroutine(string sceneName, Action onComplete)
        {
            OnSceneLoading?.Invoke(sceneName);
            
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            OnSceneLoaded?.Invoke(sceneName);
            onComplete?.Invoke();
        }

        private IEnumerator LoadGameSceneCoroutine(string sceneName, Action onComplete)
        {
            OnSceneLoading?.Invoke(sceneName);

            // Ensure Main Scene is loaded first
            if (!SceneManager.GetSceneByName(MAIN_SCENE_NAME).isLoaded)
            {
                yield return StartCoroutine(LoadSceneCoroutine(MAIN_SCENE_NAME, null));
            }

            // Load game scene additively
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            currentGameScene = sceneName;
            OnSceneLoaded?.Invoke(sceneName);
            onComplete?.Invoke();
        }

        private IEnumerator UnloadGameSceneCoroutine(string sceneName, Action onComplete)
        {
            OnSceneUnloading?.Invoke(sceneName);

            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);
            
            while (!asyncUnload.isDone)
            {
                yield return null;
            }

            currentGameScene = null;
            onComplete?.Invoke();
        }

        private IEnumerator UnloadGameSceneThenLoadMain(Action onComplete)
        {
            yield return StartCoroutine(UnloadGameSceneCoroutine(currentGameScene, null));
            yield return StartCoroutine(LoadSceneCoroutine(MAIN_SCENE_NAME, null));
            onComplete?.Invoke();
        }

        /// <summary>
        /// Get current game scene name (null if in main scene)
        /// </summary>
        public string GetCurrentGameScene()
        {
            return currentGameScene;
        }

        /// <summary>
        /// Check if a game scene is currently loaded
        /// </summary>
        public bool IsGameSceneLoaded()
        {
            return !string.IsNullOrEmpty(currentGameScene);
        }
    }
}
