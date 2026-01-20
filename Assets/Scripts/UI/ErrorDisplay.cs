using UnityEngine;
using UnityEngine.UI;

namespace Minigames.UI
{
    /// <summary>
    /// Simple error display UI component.
    /// </summary>
    public class ErrorDisplay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject errorPanel;
        [SerializeField] private Text errorText;
        [SerializeField] private Button closeButton;

        private void Start()
        {
            if (errorPanel != null)
                errorPanel.SetActive(false);

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);
        }

        public void Show(string message)
        {
            if (errorPanel != null)
                errorPanel.SetActive(true);

            if (errorText != null)
                errorText.text = message;
        }

        public void Hide()
        {
            if (errorPanel != null)
                errorPanel.SetActive(false);
        }
    }
}
