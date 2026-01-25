# User Authentication & Registration

This document describes how authentication and registration work in the Minigames app, both for **end users** and **developers**.

---

## For Users

### How to Register

1. Open the Minigames app.
2. If you are not signed in, the **Login / Register** screen appears first.
3. Tap **“Create account”** or **“Register”** (or the link that switches to the registration form).
4. Fill in:
   - **Username** – must be unique
   - **Email** – a valid email address
   - **Password** – at least 6 characters
5. Tap **“Register”**.
6. If registration succeeds, you are signed in and taken to the main menu (Games, Profile, etc.).

### How to Log In

1. Open the Minigames app.
2. On the **Login** screen, enter:
   - **Username**
   - **Password**
3. Tap **“Login”**.
4. If login succeeds, you are taken to the main menu.

### How to Log Out

1. Go to the **Profile** tab.
2. Tap **“Log out”** (or **“Logout”**).
3. You are returned to the Login / Register screen.

### If You See Errors

- **“Please enter your username”** / **“Please enter your password”**  
  Make sure both fields are filled.

- **“Please enter a username”** / **“Please enter your email”** / **“Please enter a password”**  
  Complete all registration fields.

- **“Password must be at least 6 characters”**  
  Use a longer password when registering.

- **“Login Failed”** / **“Registration Failed”**  
  The server could not sign you in or create your account. Check your username, email, and password. If the problem continues, try again later or contact support.

### Token Provided by the Host App (WebView)

If you open the Minigames app from another app (e.g. a mobile game launcher) that **already logs you in**, you may not see the Login / Register screen. In that case, the host app provides your session; you use the app as a logged-in user and do not need to log in again inside Minigames.

---

## For Developers

### Overview

- **AuthManager** handles login, registration, token storage, and profile fetches.
- **AuthController** drives the Login / Register UI (forms, validation, loading, errors).
- **MainMenuController** shows the auth panel when the user is not authenticated, and the main content (Games, Profile, etc.) when they are.

Authentication supports two flows:

1. **In-app login/register** – User signs in or signs up via the Unity UI.
2. **Token from host app** – Host app (e.g. WebView) sends a token; Unity uses it and fetches the profile.

### API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/App/player/register` | Register a new player |
| `POST` | `/api/App/player/login` | Log in with username and password |
| `GET`  | `/api/App/player/profile` | Get current player profile (requires auth) |
| `PUT`  | `/api/App/player/profile` | Update profile (requires auth) |
| `PUT`  | `/api/App/player/deactivate` | Deactivate account (requires auth) |

All authenticated requests use the header:

```
Authorization: Bearer <token>
```

### Register Request

```json
POST /api/App/player/register
{
  "username": "string",
  "email": "string",
  "password": "string"
}
```

### Login Request

```json
POST /api/App/player/login
{
  "username": "string",
  "password": "string"
}
```

### Login / Register Response

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
2. Host app obtains a token (e.g. from its own auth) and sends it to Unity via the bridge.
3. Unity calls `WebViewBridge.ReceiveAuthToken(token)` (from JS).
4. **AuthManager** stores the token and calls `FetchProfile`.
5. On successful profile fetch, **AuthManager** fires `OnLoginSuccess`.
6. **MainMenuController** hides the auth panel and shows the main content.

So when the host provides a token, the user skips the in-app Login / Register UI.

### UI Setup (Unity Editor)

#### 1. Auth Panel and AuthController

- Create a **Panel** (e.g. `AuthPanel`) that will hold the auth UI.
- Add **AuthController** to it (or to a child GameObject).
- Create two sub‑panels:
  - **Login form**: username `TMP_InputField`, password `TMP_InputField`, **Login** button, **“Create account”** / **Register** button.
  - **Register form**: username, email, password `TMP_InputField`s, **Register** button, **“Already have an account? Log in”** button.
- Optionally add a **loading overlay** (e.g. panel + “Loading…” **TextMeshPro**).
- Wire all of these in the **AuthController** inspector (see the `[SerializeField]` fields in `AuthController.cs`).

#### 2. Main Scene Structure

- **Auth panel**: `AuthPanel` (with **AuthController**).
- **Main content**: A parent object (e.g. `MainContentRoot`) containing:
  - Navigation bar (Games, Profile, Leaderboards, Marketplace, Quests).
  - All main panels (Games, Profile, etc.).
- Assign `authPanel` and `mainContentRoot` in **MainMenuController**.

#### 3. Profile and Logout

- In the **Profile** panel, add a **Log out** button.
- Assign it to **ProfileController**’s `logoutButton`.
- **ProfileController** calls `AuthManager.Logout()` when it is clicked; **MainMenuController** reacts to `OnLogout` and shows the auth panel again.

### AuthController Fields to Wire

| Field | Description |
|-------|-------------|
| `loginPanel` | GameObject for the login form |
| `loginUsernameInput` | TMP_InputField for login username |
| `loginPasswordInput` | TMP_InputField for login password |
| `loginButton` | Login submit button |
| `switchToRegisterButton` | Button to show register form |
| `registerPanel` | GameObject for the register form |
| `registerUsernameInput` | TMP_InputField for register username |
| `registerEmailInput` | TMP_InputField for email |
| `registerPasswordInput` | TMP_InputField for register password |
| `registerButton` | Register submit button |
| `switchToLoginButton` | Button to show login form |
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
- If authenticated (or after login / token from host): hide `authPanel`, show `mainContentRoot`, load games, etc.
- **AuthManager**’s `OnLoginSuccess` and `OnLogout` drive the transitions.

### Errors and Popups

- **AuthController** uses **PopupManager** for validation and API errors (e.g. “Login Failed”, “Registration Failed”).
- **ProfileController** uses **PopupManager** for profile update errors and success messages.
- Ensure **PopupManager** and its popup prefabs (e.g. error, message) are set up in the scene.

### Optional: Refresh Auth State

If the host app can change the user (e.g. switch account) without reloading the WebView, you can expose a method that:

1. Clears the current token (e.g. **AuthManager.Logout()** or a dedicated “Clear session” API).
2. Calls **MainMenuController**’s `RefreshAuthState()` (you may need to make it public or invoke it via an event) so the UI shows the auth panel again.

---

## Security Notes

- Passwords are sent only over HTTPS. Ensure your API base URL uses `https://`.
- No passwords or tokens are stored on device; token is in memory only.
- Implement password rules (length, complexity) on the **backend**; the app enforces minimal checks (e.g. non‑empty, min length) for UX only.
- Handle token expiry (e.g. 401) by clearing the token, then showing the login screen or notifying the host app.

---

## Summary

| Action | User flow | Developer hook |
|--------|-----------|----------------|
| **Register** | Register form → Submit → Main menu | `AuthManager.Register` |
| **Login** | Login form → Submit → Main menu | `AuthManager.Login` |
| **Logout** | Profile → Log out → Login screen | `AuthManager.Logout` |
| **Token from host** | No login UI; go straight to main menu | `WebViewBridge.ReceiveAuthToken` |

For more on the WebView bridge and host integration, see **README** and **WebViewBridge**-related docs.
