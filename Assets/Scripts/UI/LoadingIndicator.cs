using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Minigames.UI
{
    /// <summary>
    /// Simple loading indicator UI component.
    /// </summary>
    public class LoadingIndicator : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private bool rotateOnShow = true;

        private void Start()
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(false);
        }

        public void Show(string message = "Loading...")
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(true);

            if (loadingText != null)
                loadingText.text = message;
        }

        public void Hide()
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(false);
        }

        public void SetMessage(string message)
        {
            if (loadingText != null)
                loadingText.text = message;
        }
    }
}
