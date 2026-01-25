using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Minigames.Data;
using Minigames.Managers;

namespace Minigames.UI
{
    /// <summary>
    /// Controls the authentication UI: login and registration forms.
    /// Shows login/register panels and wires them to AuthManager.
    /// </summary>
    public class AuthController : MonoBehaviour
    {
        [Header("Login Form")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private TMP_InputField loginUsernameInput;
        [SerializeField] private TMP_InputField loginPasswordInput;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button switchToRegisterButton;

        [Header("Register Form")]
        [SerializeField] private GameObject registerPanel;
        [SerializeField] private TMP_InputField registerUsernameInput;
        [SerializeField] private TMP_InputField registerEmailInput;
        [SerializeField] private TMP_InputField registerPasswordInput;
        [SerializeField] private Button registerButton;
        [SerializeField] private Button switchToLoginButton;

        [Header("Loading")]
        [SerializeField] private GameObject loadingOverlay;
        [SerializeField] private TextMeshProUGUI loadingText;

        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
            ShowLoginForm();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeUI()
        {
            if (loginButton != null)
                loginButton.onClick.AddListener(OnLoginClicked);
            if (switchToRegisterButton != null)
                switchToRegisterButton.onClick.AddListener(ShowRegisterForm);

            if (registerButton != null)
                registerButton.onClick.AddListener(OnRegisterClicked);
            if (switchToLoginButton != null)
                switchToLoginButton.onClick.AddListener(ShowLoginForm);

            SetLoading(false);
        }

        private void SubscribeToEvents()
        {
            AuthManager.Instance.OnLoginSuccess += HandleLoginSuccess;
            AuthManager.Instance.OnAuthError += HandleAuthError;
        }

        private void UnsubscribeFromEvents()
        {
            if (AuthManager.Instance != null)
            {
                AuthManager.Instance.OnLoginSuccess -= HandleLoginSuccess;
                AuthManager.Instance.OnAuthError -= HandleAuthError;
            }
        }

        private void ShowLoginForm()
        {
            if (registerPanel != null) registerPanel.SetActive(false);
            if (loginPanel != null) loginPanel.SetActive(true);
            ClearLoginInputs();
        }

        private void ShowRegisterForm()
        {
            if (loginPanel != null) loginPanel.SetActive(false);
            if (registerPanel != null) registerPanel.SetActive(true);
            ClearRegisterInputs();
        }

        private void ClearLoginInputs()
        {
            if (loginUsernameInput != null) loginUsernameInput.text = "";
            if (loginPasswordInput != null) loginPasswordInput.text = "";
        }

        private void ClearRegisterInputs()
        {
            if (registerUsernameInput != null) registerUsernameInput.text = "";
            if (registerEmailInput != null) registerEmailInput.text = "";
            if (registerPasswordInput != null) registerPasswordInput.text = "";
        }

        private void SetLoading(bool loading, string message = "Loading...")
        {
            if (loadingOverlay != null) loadingOverlay.SetActive(loading);
            if (loadingText != null) loadingText.text = message;

            bool interact = !loading;
            if (loginButton != null) loginButton.interactable = interact;
            if (registerButton != null) registerButton.interactable = interact;
            if (switchToLoginButton != null) switchToLoginButton.interactable = interact;
            if (switchToRegisterButton != null) switchToRegisterButton.interactable = interact;
        }

        private void OnLoginClicked()
        {
            string username = loginUsernameInput != null ? loginUsernameInput.text.Trim() : "";
            string password = loginPasswordInput != null ? loginPasswordInput.text : "";

            if (string.IsNullOrEmpty(username))
            {
                PopupManager.Instance.ShowError("Login", "Please enter your username.");
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                PopupManager.Instance.ShowError("Login", "Please enter your password.");
                return;
            }

            SetLoading(true, "Signing in...");
            var request = new PlayerLoginRequest { username = username, password = password };
            AuthManager.Instance.Login(
                request,
                _ => { /* HandleLoginSuccess will hide auth UI */ },
                error =>
                {
                    SetLoading(false);
                    PopupManager.Instance.ShowError("Login Failed", error);
                }
            );
        }

        private void OnRegisterClicked()
        {
            string username = registerUsernameInput != null ? registerUsernameInput.text.Trim() : "";
            string email = registerEmailInput != null ? registerEmailInput.text.Trim() : "";
            string password = registerPasswordInput != null ? registerPasswordInput.text : "";

            if (string.IsNullOrEmpty(username))
            {
                PopupManager.Instance.ShowError("Register", "Please enter a username.");
                return;
            }
            if (string.IsNullOrEmpty(email))
            {
                PopupManager.Instance.ShowError("Register", "Please enter your email.");
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                PopupManager.Instance.ShowError("Register", "Please enter a password.");
                return;
            }
            if (password.Length < 6)
            {
                PopupManager.Instance.ShowError("Register", "Password must be at least 6 characters.");
                return;
            }

            SetLoading(true, "Creating account...");
            var request = new PlayerRegisterRequest
            {
                username = username,
                email = email,
                password = password
            };
            AuthManager.Instance.Register(
                request,
                _ => { /* HandleLoginSuccess */ },
                error =>
                {
                    SetLoading(false);
                    PopupManager.Instance.ShowError("Registration Failed", error);
                }
            );
        }

        private void HandleLoginSuccess(PlayerProfile profile)
        {
            SetLoading(false);
            ClearLoginInputs();
            ClearRegisterInputs();
            gameObject.SetActive(false);
        }

        private void HandleAuthError(string error)
        {
            SetLoading(false);
        }
    }
}
