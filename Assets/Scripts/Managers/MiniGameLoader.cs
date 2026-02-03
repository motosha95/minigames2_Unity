using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Minigames.Managers
{
    /// <summary>
    /// Loads minigame scenes via Addressables (additive loading).
    /// Use with GameItemController flow: click game -> LoadMiniGame -> GameLauncher runs in loaded scene.
    /// </summary>
    public class MiniGameLoader : MonoBehaviour
    {
        private static MiniGameLoader _instance;
        public static MiniGameLoader Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindFirstObjectByType<MiniGameLoader>();
                return _instance;
            }
        }

        [Header("UI References")]
        public GameObject loadingPanel;
        public Slider progressBar;
        public Text progressText;

        [Header("Main Menu (hide when game loads, restore when unloads)")]
        [Tooltip("Main menu content to hide when loading a minigame and restore when returning.")]
        public GameObject mainMenuContentToRestore;

        private AsyncOperationHandle<SceneInstance> currentMiniGameHandle;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        /// <summary>
        /// True if a minigame scene was loaded via Addressables and is still loaded.
        /// </summary>
        public bool HasLoadedScene => currentMiniGameHandle.IsValid();

        /// <summary>
        /// Gets the Addressables key for a game scene. Pattern: "MiniGame_" + sceneName
        /// </summary>
        public static string GetAddressableKey(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return null;
            return "MiniGame_" + sceneName;
        }

        /// <summary>
        /// Loads a minigame scene using Addressables (additive)
        /// </summary>
        /// <param name="addressKey">The Addressables key (e.g. "MiniGame_Basketball")</param>
        /// <param name="onComplete">Called when the scene is fully loaded</param>
        public void LoadMiniGame(string addressKey, Action onComplete = null)
        {
            if (mainMenuContentToRestore != null)
                mainMenuContentToRestore.SetActive(false);

            if (loadingPanel != null)
                loadingPanel.SetActive(true);

            StartCoroutine(LoadSceneAsync(addressKey, onComplete));
        }

        private IEnumerator LoadSceneAsync(string key, Action onComplete)
        {
            currentMiniGameHandle = Addressables.LoadSceneAsync(key, LoadSceneMode.Additive, true);

            while (!currentMiniGameHandle.IsDone)
            {
                float progress = Mathf.Clamp01(currentMiniGameHandle.PercentComplete);
                if (progressBar != null)
                    progressBar.value = progress;
                if (progressText != null)
                    progressText.text = $"Loading: {(progress * 100f):0}%";

                yield return null;
            }

            if (loadingPanel != null)
                loadingPanel.SetActive(false);

            Debug.Log($"Minigame '{key}' loaded!");
            onComplete?.Invoke();
        }

        /// <summary>
        /// Unloads the currently loaded minigame scene
        /// </summary>
        public void UnloadCurrentMiniGame(Action onComplete = null)
        {
            if (currentMiniGameHandle.IsValid())
            {
                StartCoroutine(UnloadSceneAsync(onComplete));
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        private IEnumerator UnloadSceneAsync(Action onComplete)
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(true);

            AsyncOperationHandle<SceneInstance> handle = currentMiniGameHandle;
            currentMiniGameHandle = default;

            AsyncOperationHandle<SceneInstance> unloadHandle = Addressables.UnloadSceneAsync(handle, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects, true);

            while (!unloadHandle.IsDone)
            {
                float progress = Mathf.Clamp01(unloadHandle.PercentComplete);
                if (progressBar != null)
                    progressBar.value = progress;
                if (progressText != null)
                    progressText.text = $"Unloading: {(progress * 100f):0}%";

                yield return null;
            }

            Resources.UnloadUnusedAssets();

            if (loadingPanel != null)
                loadingPanel.SetActive(false);

            if (mainMenuContentToRestore != null)
                mainMenuContentToRestore.SetActive(true);

            Debug.Log("Minigame unloaded and memory cleaned.");
            onComplete?.Invoke();
        }
    }
}
