# User Authentication & Registration

This document describes how authentication and registration work in the Minigames app, both for **end users** and **developers**.

---

## For Users

### How to Log In

1. Open the Minigames app.
2. If you are not signed in, the **Login** screen appears first.
3. Enter:
   - **Email or Username** – your account email address or username
   - **Password**
4. Tap **“Login”**.
5. If login succeeds, you are taken to the main menu (Games, Profile, etc.).

### How to Register

1. Open the Minigames app and tap **“Create account”** (or the link that switches to registration).
2. **Step 1 – Account details**
   - **Username** – must be unique
   - **Email** – a valid email address (you’ll receive a verification code here)
   - **Password** – at least 6 characters
   - **Confirm Password** – must match the password
3. Tap **“Send verification code”** (or **“Send OTP”**).
4. **Step 2 – Verify email**
   - Check your email for the verification code.
   - Enter the **code** in the app.
   - Tap **“Verify & Create account”** (or **“Verify OTP”**).
5. If verification succeeds, you are signed in and taken to the main menu.

You can tap **“Resend code”** to receive a new verification code, or **“Back”** to change your email or other details.

### How to Log Out

1. Go to the **Profile** tab.
2. Tap **“Log out”** (or **“Logout”**).
3. You are returned to the Login screen.

### If You See Errors

- **“Please enter your email or username”** / **“Please enter your password”**  
  Make sure both login fields are filled.

- **“Please enter a username”** / **“Please enter your email”** / **“Please enter a password”**  
  Complete all registration fields.

- **“Please enter a valid email address.”**  
  Use a valid email (e.g. contains `@`).

- **“Password and Confirm Password do not match.”**  
  Confirm Password must exactly match Password.

- **“Password must be at least 6 characters.”**  
  Use a longer password when registering.

- **“Please enter the verification code.”**  
  Enter the code you received by email.

- **“Login Failed”** / **“Send OTP Failed”** / **“Verification Failed”**  
  The server could not complete the action. Check your details, then try again. If it continues, contact support.

### Token Provided by the Host App (WebView)

If you open the Minigames app from another app (e.g. a mobile game launcher) that **already logs you in**, you may not see the Login screen. The host app provides your session; you use the app as a logged-in user.

---

## For Developers

### Overview

- **AuthManager** handles login (email or username), OTP-based registration, token storage, and profile fetches.
- **AuthController** drives the Login form, Register form (with confirm password), and OTP verification step.
- **MainMenuController** shows the auth panel when the user is not authenticated, and the main content when they are.

Authentication supports:

1. **In-app login** – User signs in with **email or username** and password.
2. **In-app registration** – User enters details + confirm password → **Send OTP** → **Verify OTP** → account created and signed in.
3. **Token from host app** – Host app (e.g. WebView) sends a token; Unity uses it and fetches the profile.

### API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/App/player/login` | Log in with **email or username** and password |
| `POST` | `/api/App/player/register/send-otp` | Request OTP for registration (sends code to email) |
| `POST` | `/api/App/player/register/verify-otp` | Verify OTP and complete registration |
| `GET`  | `/api/App/player/profile` | Get current player profile (requires auth) |
| `PUT`  | `/api/App/player/profile` | Update profile (requires auth) |
| `PUT`  | `/api/App/player/deactivate` | Deactivate account (requires auth) |

The legacy **`POST /api/App/player/register`** (register without OTP) is no longer used by the default flow. The app uses the OTP flow above.

All authenticated requests use:

```
Authorization: Bearer <token>
```

### Login Request (Email or Username)

Send **either** `email` **or** `username`, plus `password`:

```json
POST /api/App/player/login
{
  "username": "string",   // use when user enters username
  "email": "string",      // use when user enters email
  "password": "string"
}
```

- If the user types an email (contains `@`), set `email` and omit `username`.
- Otherwise, set `username` and omit `email`.
- Backend must accept either `email` or `username` as the login identifier.

### Request OTP (Registration Step 1)

```json
POST /api/App/player/register/send-otp
{
  "email": "string",
  "username": "string"
}
```

Backend sends an OTP to `email` (e.g. by email). Response: `{ "success": true, "data": {} }` or similar.

### Verify OTP and Register (Registration Step 2)

```json
POST /api/App/player/register/verify-otp
{
  "email": "string",
  "username": "string",
  "password": "string",
  "otp": "string"
}
```

Backend verifies `otp` for that `email`/`username`, creates the account, and returns the same shape as login:

```json
{
  "success": true,
  "data": {
    "token": "jwt-or-session-token",
    "player": { /* PlayerProfile */ }
  }
}
```

### Token Storage

- The token is kept **only in memory** (no disk, no PlayerPrefs).
- It is cleared on logout or when the app is closed.
- Do not persist tokens or passwords in the Unity client.

### WebView Token Flow

1. Host app loads the Unity WebGL build in a WebView.
2. Host app obtains a token and sends it to Unity via the bridge.
3. Unity calls `WebViewBridge.ReceiveAuthToken(token)` (from JS).
4. **AuthManager** stores the token and calls `FetchProfile`.
5. On success, **AuthManager** fires `OnLoginSuccess`.
6. **MainMenuController** hides the auth panel and shows the main content.

### UI Setup (Unity Editor)

#### 1. Auth Panel and AuthController

Create a **Panel** (e.g. `AuthPanel`) with **AuthController** attached. Add three sub‑panels:

**Login form**

- **Email or Username**: one `TMP_InputField` (label e.g. “Email or Username”).
- **Password**: `TMP_InputField`.
- **Login** button.
- **“Create account”** / **Register** button (switches to register).

**Register form (Step 1)**

- **Username**: `TMP_InputField`.
- **Email**: `TMP_InputField`.
- **Password**: `TMP_InputField`.
- **Confirm Password**: `TMP_InputField`.
- **Send verification code** / **Send OTP** button.
- **“Already have an account? Log in”** button (switches to login).

**OTP form (Step 2)**

- **OTP**: `TMP_InputField` for the verification code.
- Optional **Text** (e.g. “Enter the code sent to …”) – can be updated by **AuthController** (`otpSentToText`).
- **Verify & Create account** / **Verify OTP** button.
- **Resend code** button.
- **Back** button (returns to register form).

Add a **loading overlay** (panel + “Loading…” **TextMeshPro**) and wire it in **AuthController**.

#### 2. Main Scene Structure

- **Auth panel**: `AuthPanel` (with **AuthController**).
- **Main content**: A parent (e.g. `MainContentRoot`) containing the nav bar and all main panels (Games, Profile, etc.).
- Assign `authPanel` and `mainContentRoot` in **MainMenuController**.

#### 3. Profile and Logout

- Add a **Log out** button in the Profile panel.
- Assign it to **ProfileController**’s `logoutButton`.

### AuthController Fields to Wire

| Field | Description |
|-------|-------------|
| **Login** | |
| `loginPanel` | GameObject for the login form |
| `loginEmailOrUsernameInput` | TMP_InputField for email or username |
| `loginPasswordInput` | TMP_InputField for password |
| `loginButton` | Login submit button |
| `switchToRegisterButton` | Button to show register form |
| **Register (Step 1)** | |
| `registerPanel` | GameObject for the register form |
| `registerUsernameInput` | TMP_InputField for username |
| `registerEmailInput` | TMP_InputField for email |
| `registerPasswordInput` | TMP_InputField for password |
| `registerConfirmPasswordInput` | TMP_InputField for confirm password |
| `sendOtpButton` | “Send verification code” button |
| `switchToLoginFromRegisterButton` | “Already have an account? Log in” button |
| **OTP (Step 2)** | |
| `otpPanel` | GameObject for the OTP form |
| `otpInput` | TMP_InputField for the code |
| `otpSentToText` | Optional TextMeshProUGUI (“Enter the code sent to …”) |
| `verifyAndRegisterButton` | “Verify & Create account” button |
| `resendOtpButton` | “Resend code” button |
| `backFromOtpButton` | “Back” button |
| **Loading** | |
| `loadingOverlay` | Optional loading panel |
| `loadingText` | Optional loading message (TextMeshPro) |

### MainMenuController Auth Fields

| Field | Description |
|-------|-------------|
| `authPanel` | GameObject for the auth UI (AuthController) |
| `mainContentRoot` | GameObject that contains nav + all main panels |

### Gating Behind Auth

- **MainMenuController** checks `AuthManager.IsAuthenticated()` on start.
- If **not** authenticated: show `authPanel`, hide `mainContentRoot`.
- If authenticated (or after login / OTP verify / token from host): hide `authPanel`, show `mainContentRoot`, load games, etc.
- **AuthManager**’s `OnLoginSuccess` and `OnLogout` drive the transitions.

### Errors and Popups

- **AuthController** uses **PopupManager** for validation and API errors (login, send OTP, verify OTP).
- **ProfileController** uses **PopupManager** for profile updates and logout.
- Ensure **PopupManager** and its popup prefabs (error, message) are set up in the scene.

### Optional: Refresh Auth State

If the host app can switch users without reloading the WebView:

1. Clear the token (e.g. **AuthManager.Logout()**).
2. Trigger **MainMenuController** to refresh auth state (e.g. call `RefreshAuthState()` if exposed) so the auth panel is shown again.

---

## Security Notes

- Passwords are sent only over HTTPS. Use `https://` for the API base URL.
- No passwords or tokens are stored on device; token is in memory only.
- Implement password rules (length, complexity) on the **backend**; the app enforces minimal checks (non‑empty, min length, match confirm) for UX only.
- OTP should be short‑lived and single‑use. Backend must validate OTP and rate‑limit send/verify.
- Handle token expiry (e.g. 401) by clearing the token and showing the login screen or notifying the host app.

---

## Summary

| Action | User flow | Developer hook |
|--------|-----------|----------------|
| **Login** | Email or username + password → Main menu | `AuthManager.Login` |
| **Register** | Details + confirm password → Send OTP → Enter OTP → Verify → Main menu | `AuthManager.RequestRegistrationOtp`, `AuthManager.VerifyOtpAndRegister` |
| **Logout** | Profile → Log out → Login screen | `AuthManager.Logout` |
| **Token from host** | No login UI; go straight to main menu | `WebViewBridge.ReceiveAuthToken` |

For more on the WebView bridge and host integration, see **README** and **WebViewBridge**-related docs.
