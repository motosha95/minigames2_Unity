using System;
using System.Collections.Generic;
using UnityEngine;
using Minigames.Core;
using Minigames.Data;

namespace Minigames.Managers
{
    /// <summary>
    /// Manages game session lifecycle: start, complete, and track active sessions.
    /// Critical component that ensures all games follow the proper session flow.
    /// </summary>
    public class GameSessionManager : MonoBehaviour
    {
        private static GameSessionManager _instance;
        public static GameSessionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameSessionManager");
                    _instance = go.AddComponent<GameSessionManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // Events
        public event Action<GameSessionData> OnSessionStarted;
        public event Action<GameSessionData> OnSessionCompleted;
        public event Action<string> OnSessionError;

        private GameSessionData currentSession;
        private Dictionary<string, GameSessionData> activeSessions = new Dictionary<string, GameSessionData>();

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
        /// Start a new game session. Must be called before starting any game.
        /// </summary>
        public void StartSession(string gameId, Dictionary<string, object> metadata = null, 
            Action<GameSessionData> onSuccess = null, Action<string> onError = null)
        {
            // Convert metadata dictionary to JSON string for gameData
            string gameDataJson = null;
            if (metadata != null && metadata.Count > 0)
            {
                try
                {
                    gameDataJson = JsonUtility.ToJson(metadata);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"GameSessionManager: Failed to serialize metadata: {e.Message}");
                }
            }

            // Use backend DTO format
            var startDto = new StartGameDto
            {
                gameId = gameId,
                gameData = gameDataJson
            };

            ApiClient.Instance.Post<StartGameDto, GameSessionDto>(
                "/api/App/gameSession/start",
                startDto,
                (response) =>
                {
                    // Convert GameSessionDto to GameSessionData
                    currentSession = DtoConverter.ToGameSessionData(response.data);
                    if (currentSession != null)
                    {
                        activeSessions[currentSession.id] = currentSession;
                        OnSessionStarted?.Invoke(currentSession);
                        onSuccess?.Invoke(currentSession);
                    }
                    else
                    {
                        string error = "Failed to convert session data";
                        OnSessionError?.Invoke(error);
                        onError?.Invoke(error);
                    }
                },
                (error) =>
                {
                    OnSessionError?.Invoke(error);
                    onError?.Invoke(error);
                }
            );
        }

        /// <summary>
        /// Complete the current game session with score. Must be called when game ends.
        /// Score will be encrypted before sending to backend.
        /// </summary>
        public void CompleteSession(int score, Dictionary<string, object> metadata = null,
            Action<GameSessionData> onSuccess = null, Action<string> onError = null)
        {
            if (currentSession == null)
            {
                string error = "No active session to complete";
                OnSessionError?.Invoke(error);
                onError?.Invoke(error);
                return;
            }

            // Convert metadata dictionary to JSON string for gameData
            string gameDataJson = null;
            if (metadata != null && metadata.Count > 0)
            {
                try
                {
                    gameDataJson = JsonUtility.ToJson(metadata);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"GameSessionManager: Failed to serialize metadata: {e.Message}");
                }
            }

            // Encrypt score for backend
            string encryptedScore = ScoreEncryptionHelper.EncryptScore(score);

            // Use backend DTO format
            var completeDto = new CompleteGameDto
            {
                gameSessionId = currentSession.id,
                encryptedScore = encryptedScore,
                gameData = gameDataJson
            };

            ApiClient.Instance.Post<CompleteGameDto, GameSessionDto>(
                "/api/App/gameSession/complete",
                completeDto,
                (response) =>
                {
                    // Convert GameSessionDto to GameSessionData
                    var completedSession = DtoConverter.ToGameSessionData(response.data, currentSession.id);
                    if (completedSession != null)
                    {
                        currentSession = completedSession;
                        if (activeSessions.ContainsKey(currentSession.id))
                        {
                            activeSessions.Remove(currentSession.id);
                        }
                        OnSessionCompleted?.Invoke(currentSession);
                        onSuccess?.Invoke(currentSession);
                    }
                    else
                    {
                        string error = "Failed to convert session data";
                        OnSessionError?.Invoke(error);
                        onError?.Invoke(error);
                    }
                    
                    // Clear current session after completion
                    currentSession = null;
                },
                (error) =>
                {
                    OnSessionError?.Invoke(error);
                    onError?.Invoke(error);
                }
            );
        }

        /// <summary>
        /// Get session details by ID
        /// </summary>
        public void GetSession(string sessionId, Action<GameSessionData> onSuccess, Action<string> onError)
        {
            ApiClient.Instance.Get<GameSessionDto>(
                $"/api/App/gameSession/{sessionId}",
                (response) =>
                {
                    var sessionData = DtoConverter.ToGameSessionData(response.data, sessionId);
                    onSuccess?.Invoke(sessionData);
                },
                (error) => onError?.Invoke(error)
            );
        }

        /// <summary>
        /// Get all my game sessions with pagination
        /// </summary>
        public void GetMySessions(int page = 1, int pageSize = 20, Action<GameSessionListResponse> onSuccess = null, Action<string> onError = null)
        {
            string endpoint = $"/api/App/gameSession/my-sessions?page={page}&pageSize={pageSize}";
            ApiClient.Instance.Get<GameSessionListResponse>(
                endpoint,
                (response) => onSuccess?.Invoke(response.data),
                (error) => onError?.Invoke(error)
            );
        }

        /// <summary>
        /// Get active (in-progress) game sessions
        /// </summary>
        public void GetActiveSessions(int page = 1, int pageSize = 20, Action<GameSessionListResponse> onSuccess = null, Action<string> onError = null)
        {
            string endpoint = $"/api/App/gameSession/my-sessions/active?page={page}&pageSize={pageSize}";
            ApiClient.Instance.Get<GameSessionListResponse>(
                endpoint,
                (response) => onSuccess?.Invoke(response.data),
                (error) => onError?.Invoke(error)
            );
        }

        /// <summary>
        /// Get current active session (if any)
        /// </summary>
        public GameSessionData GetCurrentSession()
        {
            return currentSession;
        }

        /// <summary>
        /// Check if there's an active session
        /// </summary>
        public bool HasActiveSession()
        {
            return currentSession != null;
        }

        /// <summary>
        /// Abandon current session (cleanup without completing)
        /// </summary>
        public void AbandonCurrentSession()
        {
            if (currentSession != null)
            {
                if (activeSessions.ContainsKey(currentSession.id))
                {
                    activeSessions.Remove(currentSession.id);
                }
                currentSession = null;
            }
        }
    }
}
