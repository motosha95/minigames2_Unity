using System.Collections.Generic;
using Minigames.Data;

namespace Minigames.Games
{
    /// <summary>
    /// Interface that all minigames must implement.
    /// Games communicate only through this interface - no direct API access.
    /// </summary>
    public interface IMinigame
    {
        /// <summary>
        /// Initialize the game with session data. Called before StartGame().
        /// </summary>
        void Initialize(GameSessionData session);

        /// <summary>
        /// Start the game. Called after Initialize().
        /// </summary>
        void StartGame();

        /// <summary>
        /// End the game with final score and metadata.
        /// This should be called by the game when it ends.
        /// The GameSessionManager will handle score submission.
        /// </summary>
        void EndGame(int score, Dictionary<string, object> metadata = null);

        /// <summary>
        /// Get the game ID this minigame represents
        /// </summary>
        string GetGameId();
    }
}
