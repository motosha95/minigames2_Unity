using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Minigames.Data;

namespace Minigames.Core
{
    /// <summary>
    /// Core API client for making HTTP requests to the backend.
    /// Handles all REST API communication with proper error handling.
    /// </summary>
    public class ApiClient : MonoBehaviour
    {
        private static ApiClient _instance;
        public static ApiClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("ApiClient");
                    _instance = go.AddComponent<ApiClient>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("API Configuration")]
        [SerializeField] private string baseUrl = "https://api.example.com"; // Set via WebView or config
        [SerializeField] private float requestTimeout = 30f;

        private string authToken = null;

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
        /// Set the base URL for API requests (called from WebView bridge)
        /// </summary>
        public void SetBaseUrl(string url)
        {
            baseUrl = url.TrimEnd('/');
        }

        /// <summary>
        /// Set authentication token (called from AuthManager)
        /// </summary>
        public void SetAuthToken(string token)
        {
            authToken = token;
        }

        /// <summary>
        /// Clear authentication token
        /// </summary>
        public void ClearAuthToken()
        {
            authToken = null;
        }

        /// <summary>
        /// Generic GET request
        /// </summary>
        public void Get<T>(string endpoint, Action<ApiResponse<T>> onSuccess, Action<string> onError = null)
        {
            StartCoroutine(GetCoroutine(endpoint, onSuccess, onError));
        }

        /// <summary>
        /// Generic POST request
        /// </summary>
        public void Post<TRequest, TResponse>(string endpoint, TRequest data, Action<ApiResponse<TResponse>> onSuccess, Action<string> onError = null)
        {
            StartCoroutine(PostCoroutine(endpoint, data, onSuccess, onError));
        }

        /// <summary>
        /// Generic PUT request
        /// </summary>
        public void Put<TRequest, TResponse>(string endpoint, TRequest data, Action<ApiResponse<TResponse>> onSuccess, Action<string> onError = null)
        {
            StartCoroutine(PutCoroutine(endpoint, data, onSuccess, onError));
        }

        private IEnumerator GetCoroutine<T>(string endpoint, Action<ApiResponse<T>> onSuccess, Action<string> onError)
        {
            string url = $"{baseUrl}{endpoint}";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                AddAuthHeader(request);
                request.timeout = (int)requestTimeout;

                yield return request.SendWebRequest();

                HandleResponse(request, onSuccess, onError);
            }
        }

        private IEnumerator PostCoroutine<TRequest, TResponse>(string endpoint, TRequest data, Action<ApiResponse<TResponse>> onSuccess, Action<string> onError)
        {
            string url = $"{baseUrl}{endpoint}";
            string jsonData = JsonUtility.ToJson(data);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                AddAuthHeader(request);
                request.timeout = (int)requestTimeout;

                yield return request.SendWebRequest();

                HandleResponse(request, onSuccess, onError);
            }
        }

        private IEnumerator PutCoroutine<TRequest, TResponse>(string endpoint, TRequest data, Action<ApiResponse<TResponse>> onSuccess, Action<string> onError)
        {
            string url = $"{baseUrl}{endpoint}";
            string jsonData = JsonUtility.ToJson(data);

            using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                AddAuthHeader(request);
                request.timeout = (int)requestTimeout;

                yield return request.SendWebRequest();

                HandleResponse(request, onSuccess, onError);
            }
        }

        private void AddAuthHeader(UnityWebRequest request)
        {
            if (!string.IsNullOrEmpty(authToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            }
        }

        private void HandleResponse<T>(UnityWebRequest request, Action<ApiResponse<T>> onSuccess, Action<string> onError)
        {
            if (request.result == UnityWebRequest.Result.ConnectionError || 
                request.result == UnityWebRequest.Result.DataProcessingError)
            {
                onError?.Invoke($"Network error: {request.error}");
                return;
            }

            if (request.responseCode == 401)
            {
                // Unauthorized - token expired or invalid
                ClearAuthToken();
                onError?.Invoke("Unauthorized: Please login again");
                return;
            }

            if (request.responseCode >= 400)
            {
                onError?.Invoke($"HTTP {request.responseCode}: {request.downloadHandler.text}");
                return;
            }

            try
            {
                string jsonResponse = request.downloadHandler.text;
                ApiResponse<T> response = JsonUtility.FromJson<ApiResponse<T>>(jsonResponse);
                
                if (response != null && response.success)
                {
                    onSuccess?.Invoke(response);
                }
                else
                {
                    onError?.Invoke(response?.message ?? "Unknown error");
                }
            }
            catch (Exception e)
            {
                onError?.Invoke($"Parse error: {e.Message}");
            }
        }
    }
}
