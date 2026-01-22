using UnityEngine;
using Minigames.UI;

namespace Minigames.Games
{
    /// <summary>
    /// Helper class for games to show instructions popups.
    /// </summary>
    public static class GameInstructionsHelper
    {
        /// <summary>
        /// Show game instructions popup
        /// </summary>
        public static void ShowInstructions(string title, string instructions, System.Action onClose = null)
        {
            if (PopupManager.Instance != null)
            {
                PopupManager.Instance.ShowInstructions(title, instructions, onClose);
            }
            else
            {
                Debug.LogWarning("GameInstructionsHelper: PopupManager not available");
            }
        }

        /// <summary>
        /// Show a message popup
        /// </summary>
        public static void ShowMessage(string title, string message, System.Action onClose = null)
        {
            if (PopupManager.Instance != null)
            {
                PopupManager.Instance.ShowMessage(title, message, onClose);
            }
        }

        /// <summary>
        /// Show an error popup
        /// </summary>
        public static void ShowError(string title, string message, System.Action onClose = null)
        {
            if (PopupManager.Instance != null)
            {
                PopupManager.Instance.ShowError(title, message, onClose);
            }
        }
    }
}
