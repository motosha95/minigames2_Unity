using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Minigames.UI
{
    /// <summary>
    /// Controller for individual popup instances.
    /// Handles popup display and close functionality.
    /// </summary>
    public class PopupController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject popupPanel;

        private System.Action onCloseCallback;

        private void Start()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            // Close on background click (if panel is clicked)
            if (popupPanel != null)
            {
                Button backgroundButton = popupPanel.GetComponent<Button>();
                if (backgroundButton == null)
                {
                    backgroundButton = popupPanel.AddComponent<Button>();
                    backgroundButton.transition = Selectable.Transition.None;
                }
                backgroundButton.onClick.AddListener(OnCloseClicked);
            }
        }

        /// <summary>
        /// Setup the popup with title, content, and close callback
        /// </summary>
        public void Setup(string title, string content, System.Action onClose = null)
        {
            if (titleText != null)
                titleText.text = title ?? "";

            if (contentText != null)
                contentText.text = content ?? "";

            onCloseCallback = onClose;
        }

        private void OnCloseClicked()
        {
            onCloseCallback?.Invoke();
        }

        /// <summary>
        /// Close the popup programmatically
        /// </summary>
        public void Close()
        {
            OnCloseClicked();
        }
    }
}
