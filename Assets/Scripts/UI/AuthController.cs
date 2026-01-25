using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Minigames.Data;
using Minigames.Managers;

namespace Minigames.UI
{
    /// <summary>
    /// Controls the authentication UI: login (email or username), registration with confirm password,
    /// and OTP verification.
    /// </summary>
    public class AuthController : MonoBehaviour
    {
        [Header("Login Form")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private TMP_InputField loginEmailOrUsernameInput;
        [SerializeField] private TMP_InputField loginPasswordInput;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button switchToRegisterButton;

        [Header("Register Form (Step 1)")]
        [SerializeField] private GameObject registerPanel;
        [SerializeField] private TMP_InputField registerUsernameInput;
        [SerializeField] private TMP_InputField registerEmailInput;
        [SerializeField] private TMP_InputField registerPasswordInput;
        [SerializeField] private TMP_InputField registerConfirmPasswordInput;
        [SerializeField] private Button sendOtpButton;
        [SerializeField] private Button switchToLoginFromRegisterButton;

        [Header("OTP Form (Step 2)")]
        [SerializeField] private GameObject otpPanel;
        [SerializeField] private TMP_InputField otpInput;
        [SerializeField] private TextMeshProUGUI otpSentToText;
        [SerializeField] private Button verifyAndRegisterButton;
        [SerializeField] private Button resendOtpButton;
        [SerializeField] private Button backFromOtpButton;

        [Header("Loading")]
        [SerializeField] private GameObject loadingOverlay;
        [SerializeField] private TextMeshProUGUI loadingText;

        private string _pendingEmail;
        private string _pendingUsername;
        private string _pendingPassword;

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

            if (sendOtpButton != null)
                sendOtpButton.onClick.AddListener(OnSendOtpClicked);
            if (switchToLoginFromRegisterButton != null)
                switchToLoginFromRegisterButton.onClick.AddListener(ShowLoginForm);

            if (verifyAndRegisterButton != null)
                verifyAndRegisterButton.onClick.AddListener(OnVerifyOtpClicked);
            if (resendOtpButton != null)
                resendOtpButton.onClick.AddListener(OnResendOtpClicked);
            if (backFromOtpButton != null)
                backFromOtpButton.onClick.AddListener(ShowRegisterForm);

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
            if (otpPanel != null) otpPanel.SetActive(false);
            if (loginPanel != null) loginPanel.SetActive(true);
            ClearLoginInputs();
        }

        private void ShowRegisterForm()
        {
            if (loginPanel != null) loginPanel.SetActive(false);
            if (otpPanel != null) otpPanel.SetActive(false);
            if (registerPanel != null) registerPanel.SetActive(true);
            ClearRegisterInputs();
            ClearOtpInput();
        }

        private void ShowOtpForm()
        {
            if (loginPanel != null) loginPanel.SetActive(false);
            if (registerPanel != null) registerPanel.SetActive(false);
            if (otpPanel != null) otpPanel.SetActive(true);
            ClearOtpInput();
            if (otpSentToText != null)
                otpSentToText.text = $"Enter the code sent to {_pendingEmail}";
        }

        private void ClearLoginInputs()
        {
            if (loginEmailOrUsernameInput != null) loginEmailOrUsernameInput.text = "";
            if (loginPasswordInput != null) loginPasswordInput.text = "";
        }

        private void ClearRegisterInputs()
        {
            if (registerUsernameInput != null) registerUsernameInput.text = "";
            if (registerEmailInput != null) registerEmailInput.text = "";
            if (registerPasswordInput != null) registerPasswordInput.text = "";
            if (registerConfirmPasswordInput != null) registerConfirmPasswordInput.text = "";
        }

        private void ClearOtpInput()
        {
            if (otpInput != null) otpInput.text = "";
        }

        private void SetLoading(bool loading, string message = "Loading...")
        {
            if (loadingOverlay != null) loadingOverlay.SetActive(loading);
            if (loadingText != null) loadingText.text = message;

            bool interact = !loading;
            if (loginButton != null) loginButton.interactable = interact;
            if (sendOtpButton != null) sendOtpButton.interactable = interact;
            if (verifyAndRegisterButton != null) verifyAndRegisterButton.interactable = interact;
            if (resendOtpButton != null) resendOtpButton.interactable = interact;
            if (switchToRegisterButton != null) switchToRegisterButton.interactable = interact;
            if (switchToLoginFromRegisterButton != null) switchToLoginFromRegisterButton.interactable = interact;
            if (backFromOtpButton != null) backFromOtpButton.interactable = interact;
        }

        private static bool IsEmail(string s)
        {
            return !string.IsNullOrEmpty(s) && s.Trim().Contains("@");
        }

        private void OnLoginClicked()
        {
            string emailOrUsername = loginEmailOrUsernameInput != null ? loginEmailOrUsernameInput.text.Trim() : "";
            string password = loginPasswordInput != null ? loginPasswordInput.text : "";

            if (string.IsNullOrEmpty(emailOrUsername))
            {
                PopupManager.Instance.ShowError("Login", "Please enter your email or username.");
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                PopupManager.Instance.ShowError("Login", "Please enter your password.");
                return;
            }

            var request = new PlayerLoginRequest { password = password };
            if (IsEmail(emailOrUsername))
                request.email = emailOrUsername;
            else
                request.username = emailOrUsername;

            SetLoading(true, "Signing in...");
            AuthManager.Instance.Login(
                request,
                _ => { },
                error =>
                {
                    SetLoading(false);
                    PopupManager.Instance.ShowError("Login Failed", error);
                }
            );
        }

        private void OnSendOtpClicked()
        {
            string username = registerUsernameInput != null ? registerUsernameInput.text.Trim() : "";
            string email = registerEmailInput != null ? registerEmailInput.text.Trim() : "";
            string password = registerPasswordInput != null ? registerPasswordInput.text : "";
            string confirm = registerConfirmPasswordInput != null ? registerConfirmPasswordInput.text : "";

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
            if (!IsEmail(email))
            {
                PopupManager.Instance.ShowError("Register", "Please enter a valid email address.");
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
            if (password != confirm)
            {
                PopupManager.Instance.ShowError("Register", "Password and Confirm Password do not match.");
                return;
            }

            _pendingEmail = email;
            _pendingUsername = username;
            _pendingPassword = password;

            SetLoading(true, "Sending verification code...");
            AuthManager.Instance.RequestRegistrationOtp(
                email,
                username,
                () =>
                {
                    SetLoading(false);
                    ShowOtpForm();
                },
                error =>
                {
                    SetLoading(false);
                    PopupManager.Instance.ShowError("Send OTP Failed", error);
                }
            );
        }

        private void OnVerifyOtpClicked()
        {
            string otp = otpInput != null ? otpInput.text.Trim() : "";
            if (string.IsNullOrEmpty(otp))
            {
                PopupManager.Instance.ShowError("Verify OTP", "Please enter the verification code.");
                return;
            }

            var request = new VerifyOtpAndRegisterRequest
            {
                email = _pendingEmail,
                username = _pendingUsername,
                password = _pendingPassword,
                otp = otp
            };

            SetLoading(true, "Creating account...");
            AuthManager.Instance.VerifyOtpAndRegister(
                request,
                _ => { },
                error =>
                {
                    SetLoading(false);
                    PopupManager.Instance.ShowError("Verification Failed", error);
                }
            );
        }

        private void OnResendOtpClicked()
        {
            if (string.IsNullOrEmpty(_pendingEmail) || string.IsNullOrEmpty(_pendingUsername))
            {
                ShowRegisterForm();
                return;
            }

            SetLoading(true, "Resending code...");
            AuthManager.Instance.RequestRegistrationOtp(
                _pendingEmail,
                _pendingUsername,
                () =>
                {
                    SetLoading(false);
                    PopupManager.Instance.ShowMessage("OTP Sent", "A new verification code has been sent to your email.");
                },
                error =>
                {
                    SetLoading(false);
                    PopupManager.Instance.ShowError("Resend Failed", error);
                }
            );
        }

        private void HandleLoginSuccess(PlayerProfile profile)
        {
            SetLoading(false);
            ClearLoginInputs();
            ClearRegisterInputs();
            ClearOtpInput();
            _pendingEmail = _pendingUsername = _pendingPassword = null;
            gameObject.SetActive(false);
        }

        private void HandleAuthError(string error)
        {
            SetLoading(false);
        }
    }
}
