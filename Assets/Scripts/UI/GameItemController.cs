using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Minigames.Data;

namespace Minigames.UI
{
    /// <summary>
    /// Controller for individual game item in the games list.
    /// </summary>
    public class GameItemController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI gameNameText;
        [SerializeField] private TextMeshProUGUI gameDescriptionText;
        [SerializeField] private Image thumbnailImage;
        [SerializeField] private Button playButton;

        private GameInfo gameInfo;
        private System.Action<GameInfo> onGameSelected;

        private void Start()
        {
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayButtonClicked);
            }
        }

        /// <summary>
        /// Setup the game item with game info
        /// </summary>
        public void Setup(GameInfo game, System.Action<GameInfo> onSelected)
        {
            gameInfo = game;
            onGameSelected = onSelected;
            Debug.Log(game.name);
            if (gameNameText != null)
                gameNameText.text = game.name ?? "Unknown Game";

            if (gameDescriptionText != null)
                gameDescriptionText.text = game.description ?? "";

            // TODO: Load thumbnail image from URL if needed
            // if (thumbnailImage != null && !string.IsNullOrEmpty(game.thumbnailUrl))
            // {
            //     StartCoroutine(LoadThumbnail(game.thumbnailUrl));
            // }
        }

        private void OnPlayButtonClicked()
        {
            if (gameInfo != null && onGameSelected != null)
            {
                onGameSelected.Invoke(gameInfo);
            }
        }
    }
}
