using UnityEngine;
using UnityEngine.UI;

namespace Minigames.UI
{
    /// <summary>
    /// Controls the settings overlay panel.
    /// Contains profile page and other settings. Opens from the settings button in the top right.
    /// </summary>
    public class SettingsController : MonoBehaviour
    {
        [Header("Settings Panel")]
        [Tooltip("The settings panel to close. If null, uses this GameObject.")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            if (settingsPanel == null)
                settingsPanel = gameObject;
        }

        private void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseSettings);

            // Ensure panel starts closed
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        /// <summary>
        /// Close the settings panel.
        /// </summary>
        public void CloseSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }
    }
}
