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
            var request = new GameSessionStartRequest
            {
                gameId = gameId,
                metadata = metadata ?? new Dictionary<string, object>()
            };

            ApiClient.Instance.Post<GameSessionStartRequest, GameSessionData>(
                "/api/App/gameSession/start",
                request,
                (response) =>
                {
                    currentSession = response.data;
                    activeSessions[currentSession.id] = currentSession;
                    OnSessionStarted?.Invoke(currentSession);
                    onSuccess?.Invoke(currentSession);
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

            var request = new GameSessionCompleteRequest
            {
                gameSessionId = currentSession.id,
                score = score,
                metadata = metadata ?? new Dictionary<string, object>()
            };

            ApiClient.Instance.Post<GameSessionCompleteRequest, GameSessionData>(
                "/api/App/gameSession/complete",
                request,
                (response) =>
                {
                    currentSession = response.data;
                    if (activeSessions.ContainsKey(currentSession.id))
                    {
                        activeSessions.Remove(currentSession.id);
                    }
                    OnSessionCompleted?.Invoke(currentSession);
                    onSuccess?.Invoke(currentSession);
                    
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
            ApiClient.Instance.Get<GameSessionData>(
                $"/api/App/gameSession/{sessionId}",
                (response) => onSuccess?.Invoke(response.data),
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
