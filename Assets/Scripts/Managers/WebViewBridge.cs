using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Minigames.Core;
using Minigames.Managers;

namespace Minigames.Managers
{
    /// <summary>
    /// Bridge for communication between Unity WebGL app and host mobile app WebView.
    /// Handles receiving auth tokens, tenant config, and sending events to host app.
    /// </summary>
    public class WebViewBridge : MonoBehaviour
    {
        private static WebViewBridge _instance;
        public static WebViewBridge Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("WebViewBridge");
                    _instance = go.AddComponent<WebViewBridge>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // Events
        public event Action<string> OnAuthTokenReceived;
        public event Action<string> OnTenantConfigReceived;
        public event Action<string> OnBaseUrlReceived;

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
        /// Called from JavaScript when host app provides auth token
        /// </summary>
        public void ReceiveAuthToken(string token)
        {
            Debug.Log("WebViewBridge: Received auth token");
            AuthManager.Instance.SetTokenFromWebView(token);
            OnAuthTokenReceived?.Invoke(token);
        }

        /// <summary>
        /// Called from JavaScript when host app provides tenant config
        /// </summary>
        public void ReceiveTenantConfig(string configJson)
        {
            Debug.Log("WebViewBridge: Received tenant config");
            // TODO: Parse and apply tenant config (colors, logos, labels)
            OnTenantConfigReceived?.Invoke(configJson);
        }

        /// <summary>
        /// Called from JavaScript when host app provides base API URL
        /// </summary>
        public void ReceiveBaseUrl(string baseUrl)
        {
            Debug.Log($"WebViewBridge: Received base URL: {baseUrl}");
            ApiClient.Instance.SetBaseUrl(baseUrl);
            OnBaseUrlReceived?.Invoke(baseUrl);
        }

        // Methods called from Unity to notify host app

        /// <summary>
        /// Notify host app that a game is starting
        /// </summary>
        public void NotifyGameStart(string gameId)
        {
            SendMessageToHost("gameStart", gameId);
        }

        /// <summary>
        /// Notify host app that a game has ended
        /// </summary>
        public void NotifyGameEnd(string gameId, int score)
        {
            string data = $"{{\"gameId\":\"{gameId}\",\"score\":{score}}}";
            SendMessageToHost("gameEnd", data);
        }

        /// <summary>
        /// Notify host app about an error
        /// </summary>
        public void NotifyError(string errorMessage)
        {
            SendMessageToHost("error", errorMessage);
        }

        /// <summary>
        /// Notify host app about navigation events
        /// </summary>
        public void NotifyNavigation(string eventType, string data)
        {
            SendMessageToHost($"navigation_{eventType}", data);
        }

        /// <summary>
        /// Request to exit Unity app and open external URL
        /// </summary>
        public void RequestExternalNavigation(string url)
        {
            SendMessageToHost("externalNavigation", url);
        }

        private void SendMessageToHost(string eventType, string data)
        {
            try
            {
                #if UNITY_WEBGL && !UNITY_EDITOR
                SendMessageToJS(eventType, data);
                #else
                Debug.Log($"[WebViewBridge] Would send to host: {eventType} - {data}");
                #endif
            }
            catch (Exception e)
            {
                Debug.LogError($"WebViewBridge: Failed to send message to host: {e.Message}");
            }
        }

        #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void SendMessageToJS(string eventType, string data);
        #endif
    }
}
