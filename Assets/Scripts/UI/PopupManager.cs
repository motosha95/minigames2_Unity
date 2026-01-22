using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames.UI
{
    /// <summary>
    /// Centralized popup manager for displaying various types of popups.
    /// Handles error popups, instruction popups, and general message popups.
    /// </summary>
    public class PopupManager : MonoBehaviour
    {
        private static PopupManager _instance;
        public static PopupManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("PopupManager");
                    _instance = go.AddComponent<PopupManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Popup Prefabs")]
        [SerializeField] private GameObject errorPopupPrefab;
        [SerializeField] private GameObject instructionPopupPrefab;
        [SerializeField] private GameObject messagePopupPrefab;

        [Header("Popup Container")]
        [SerializeField] private Transform popupContainer;

        private List<GameObject> activePopups = new List<GameObject>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Create popup container if not assigned
            if (popupContainer == null)
            {
                GameObject container = new GameObject("PopupContainer");
                container.transform.SetParent(transform);
                Canvas canvas = container.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 1000; // High sorting order to appear on top
                container.AddComponent<GraphicRaycaster>();
                popupContainer = container.transform;
            }
        }

        /// <summary>
        /// Show an error popup
        /// </summary>
        public void ShowError(string title, string message, Action onClose = null)
        {
            ShowPopup(PopupType.Error, title, message, onClose);
        }

        /// <summary>
        /// Show an instruction popup
        /// </summary>
        public void ShowInstructions(string title, string instructions, Action onClose = null)
        {
            ShowPopup(PopupType.Instruction, title, instructions, onClose);
        }

        /// <summary>
        /// Show a general message popup
        /// </summary>
        public void ShowMessage(string title, string message, Action onClose = null)
        {
            ShowPopup(PopupType.Message, title, message, onClose);
        }

        /// <summary>
        /// Show a popup with custom content
        /// </summary>
        public void ShowPopup(PopupType type, string title, string content, Action onClose = null)
        {
            GameObject popupPrefab = GetPopupPrefab(type);
            if (popupPrefab == null)
            {
                Debug.LogError($"PopupManager: Popup prefab for type {type} is not assigned!");
                return;
            }

            GameObject popup = Instantiate(popupPrefab, popupContainer);
            activePopups.Add(popup);

            PopupController controller = popup.GetComponent<PopupController>();
            if (controller != null)
            {
                controller.Setup(title, content, () =>
                {
                    ClosePopup(popup);
                    onClose?.Invoke();
                });
            }
            else
            {
                Debug.LogWarning("PopupManager: Popup prefab doesn't have PopupController component");
            }
        }

        /// <summary>
        /// Close a specific popup
        /// </summary>
        public void ClosePopup(GameObject popup)
        {
            if (popup != null && activePopups.Contains(popup))
            {
                activePopups.Remove(popup);
                Destroy(popup);
            }
        }

        /// <summary>
        /// Close all active popups
        /// </summary>
        public void CloseAllPopups()
        {
            foreach (var popup in activePopups)
            {
                if (popup != null)
                    Destroy(popup);
            }
            activePopups.Clear();
        }

        private GameObject GetPopupPrefab(PopupType type)
        {
            switch (type)
            {
                case PopupType.Error:
                    return errorPopupPrefab;
                case PopupType.Instruction:
                    return instructionPopupPrefab;
                case PopupType.Message:
                    return messagePopupPrefab;
                default:
                    return messagePopupPrefab;
            }
        }

        /// <summary>
        /// Set popup prefabs programmatically (useful if not set in inspector)
        /// </summary>
        public void SetPopupPrefabs(GameObject errorPrefab, GameObject instructionPrefab, GameObject messagePrefab)
        {
            errorPopupPrefab = errorPrefab;
            instructionPopupPrefab = instructionPrefab;
            messagePopupPrefab = messagePrefab;
        }
    }

    public enum PopupType
    {
        Error,
        Instruction,
        Message
    }
}
