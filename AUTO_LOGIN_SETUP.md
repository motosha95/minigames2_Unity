# Auto-Login Setup Guide

## Overview

The app can automatically register a guest player on startup using a fixed tenant ID. The backend will auto-generate a username (guest1, guest2, etc.) and return a token. This is useful for development/testing or when the app is embedded in a host app that manages authentication.

## Configuration

### Unity Editor Setup

1. Select the GameObject with **AppInitializer** component in your Main Scene
2. In the Inspector, configure:

   **Configuration:**
   - **Default Base Url**: Your API base URL (e.g., `https://minigames2backendtest-a9c0hnftbpg4dyhk.francecentral-01.azurewebsites.net`)
   - **Default Tenant Id**: Your tenant ID (e.g., `c0f9c315-4631-47fa-b00e-4e7b9d1f34fb`)

   **Auto-Login (Guest Registration):**
   - **Auto Login On Start**: ✅ Check this to enable auto-login
   - **Use Guest Registration**: ✅ Check this to use guest registration (recommended)

### Example Configuration

```
Configuration:
├── Default Base Url: https://minigames2backendtest-a9c0hnftbpg4dyhk.francecentral-01.azurewebsites.net
└── Default Tenant Id: c0f9c315-4631-47fa-b00e-4e7b9d1f34fb

Auto-Login (Guest Registration):
├── Auto Login On Start: ✓ (checked)
└── Use Guest Registration: ✓ (checked)
```

## Flow

When the app starts with auto-login enabled:

1. **AppInitializer.Start()** runs:
   - Sets base URL and tenant ID in **ApiClient**
   - Subscribes to WebView bridge events
   - Checks if auto-login is enabled and required values are set
   - Calls `PerformAutoLogin()`

2. **PerformAutoLogin()**:
   - Verifies tenant ID is set
   - Calls `AuthManager.RegisterGuest()`
   - Sends request: `POST /api/App/player/register/guest` with:
     - Header: `X-Tenant-Id: {tenant-id}`
     - Body: `{}` (empty - backend auto-generates username)

3. **On Login Success**:
   - **AuthManager** fires `OnLoginSuccess` event
   - **AppInitializer.HandleLoginSuccess()** → Fetches available games
   - **MainMenuController.HandleLoginSuccess()** → Shows main content and loads games (if not already loaded)

4. **Games Loaded**:
   - **GameCatalogManager** fires `OnGamesLoaded` event
   - **MainMenuController** populates the games list
   - User sees the main menu with available games

## Disabling Auto-Login

To disable auto-login:

1. Uncheck **Auto Login On Start** in **AppInitializer** component
2. Or uncheck **Use Guest Registration**
3. The app will show the login/register UI instead

## WebView Override

If the app is loaded in a WebView and the host app provides:
- **Tenant ID**: `WebViewBridge.ReceiveTenantId(tenantId)` will update it
- **Auth Token**: `WebViewBridge.ReceiveAuthToken(token)` will skip auto-login

When tenant ID is received from WebView:
- If auto-login is enabled and user is not yet authenticated, auto-login will be attempted
- This allows the host app to provide tenant ID dynamically

## API Request Example

When auto-login runs, it makes this request:

```bash
curl -X 'POST' \
  'https://minigames2backendtest-a9c0hnftbpg4dyhk.francecentral-01.azurewebsites.net/api/App/player/register/guest' \
  -H 'accept: text/plain' \
  -H 'X-Tenant-Id: c0f9c315-4631-47fa-b00e-4e7b9d1f34fb' \
  -H 'Content-Type: application/json' \
  -d '{}'
```

**Response:**
```json
{
  "success": true,
  "data": {
    "isSuccess": true,
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "player": {
      "username": "guest1",
      "socialMediaId": "...",
      "score": 0,
      "isActive": true
    }
  }
}
```

## Error Handling

If auto-login fails:
- Error is logged to console
- **AuthManager** fires `OnAuthError` event
- **AppInitializer.HandleAuthError()** logs the error
- **MainMenuController** will show the auth panel (login/register UI)

Common errors:
- **Tenant ID not set**: Check **Default Tenant Id** in AppInitializer
- **Network error**: Check base URL and network connectivity
- **401 Unauthorized**: Invalid tenant ID
- **Guest registration failed**: Check backend is running and tenant ID is valid

## Production Considerations

For production:
- **Disable auto-login** (`Auto Login On Start` = false)
- Use **WebViewBridge** to receive tenant ID and auth token from host app
- Or use the in-app login/register UI

Auto-login is intended for:
- Development/testing
- Standalone builds with fixed tenant
- Demo/preview builds

## Code Flow Diagram

```
App Start
  ↓
AppInitializer.Start()
  ↓
Set Base URL & Tenant ID
  ↓
Auto-login enabled? ──No──→ Show Login UI
  ↓ Yes
PerformAutoLogin()
  ↓
AuthManager.RegisterGuest()
  ↓
POST /api/App/player/register/guest
  ↓
Success ──→ OnLoginSuccess Event
  ↓
AppInitializer → Load Games
MainMenuController → Show Main Content
  ↓
Games Loaded → Display Games List
```

## Notes

- Tenant ID is required for all API requests
- Guest registration auto-generates username (guest1, guest2, etc.) on backend
- No user input required - fully automatic registration
- Games are fetched automatically after successful guest registration
- If games are already loaded, duplicate fetch is prevented
- Auto-login can be overridden by WebView bridge (token from host app)
- Each app start creates a new guest user (or backend may reuse existing guest)
