# Minigames Unity WebGL Client

A Unity-based WebGL application that serves as a white-label, multi-tenant minigames platform. This app is designed to be embedded inside a mobile app WebView (iOS & Android) and communicates with a backend via REST APIs.

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── ApiClient.cs              # Core HTTP client for API requests
│   │   └── AppInitializer.cs         # App initialization on startup
│   ├── Data/
│   │   └── ApiModels.cs              # Data models for API requests/responses
│   ├── Managers/
│   │   ├── AuthManager.cs            # Authentication and token management
│   │   ├── GameCatalogManager.cs    # Game catalog management
│   │   ├── GameSessionManager.cs     # Game session lifecycle management
│   │   ├── LeaderboardManager.cs     # Leaderboard data management
│   │   ├── PlayerProfileManager.cs   # Player profile management
│   │   ├── SceneNavigationManager.cs # Scene loading/navigation
│   │   └── WebViewBridge.cs          # JS ↔ Unity communication bridge
│   ├── Games/
│   │   ├── IMinigame.cs              # Interface for all minigames
│   │   ├── BaseMinigame.cs           # Base class for minigames
│   │   └── SampleDummyGame.cs        # Sample dummy game for testing
│   └── UI/
│       ├── AuthController.cs         # Login / Register UI
│       ├── MainMenuController.cs     # Main menu UI controller
│       ├── GameItemController.cs     # Game list item controller
│       ├── ProfileController.cs      # Profile panel controller
│       ├── LeaderboardController.cs   # Leaderboard panel controller
│       └── LeaderboardEntryController.cs # Leaderboard entry controller
├── Scenes/
│   ├── MainScene.unity               # Main persistent scene (to be created in Unity Editor)
│   └── SampleDummyGameScene.unity    # Sample game scene (to be created in Unity Editor)
├── Prefabs/                          # UI prefabs (to be created in Unity Editor)
├── Resources/                        # Resources folder
│   └── WebViewBridge.html           # HTML template for WebGL build
└── Plugins/
    └── WebViewBridge.jslib          # JavaScript bridge plugin
```

## Core Features

### Architecture
- **Main Scene**: Persistent scene containing app shell, menus, and navigation
- **Game Scenes**: Individual scenes for each minigame, loaded normally (replaces current scene)
- **Manager Pattern**: Single-responsibility managers for different systems
- **Event-Driven**: Managers communicate via events/callbacks

### API Integration
- Player registration/login
- Game catalog retrieval
- Game session lifecycle (start/complete)
- Score submission and leaderboards
- Player profile management

### Game Session Lifecycle
1. Main Scene calls `GameSessionManager.StartSession()`
2. Server returns `gameSessionId`
3. Game Scene loads and initializes with session data
4. Game plays and ends
5. Game calls `GameSessionManager.CompleteSession()` with score
6. Return to Main Scene

### WebView Communication
- Receives auth token from host mobile app
- Receives tenant configuration
- Receives base API URL
- Sends game events to host app (start, end, errors, navigation)

### Authentication & Registration
- **Login** and **Register** forms (AuthController)
- Main content (Games, Profile, etc.) gated until the user is authenticated
- **Logout** from the Profile panel
- Token can be provided by the host app via WebView (skips in-app login)

See **[AUTH_INSTRUCTIONS.md](AUTH_INSTRUCTIONS.md)** for user steps, developer setup, and API details.

## Setup Instructions

### 1. Unity Project Setup
1. Open Unity Hub and create a new project (Unity LTS version recommended)
2. Set build target to **WebGL** (File → Build Settings → WebGL → Switch Platform)
3. Copy all files from this repository into your Unity project

### 2. Create Main Scene
1. Create a new scene: `Assets/Scenes/MainScene.unity`
2. Add an empty GameObject named "AppInitializer"
3. Attach `AppInitializer` component to it
4. Set up UI:
   - Create Canvas
   - Add panels for: Games List, Profile, Leaderboards
   - Add navigation buttons
   - Create prefabs for game items and leaderboard entries
5. Add `MainMenuController` to a GameObject in the scene
6. Wire up UI references in the inspector

### 3. Create Sample Game Scene
1. Create a new scene: `Assets/Scenes/SampleDummyGameScene.unity`
2. Add a GameObject with `SampleDummyGame` component
3. Set up UI (score text, timer, buttons)
4. Wire up references in the inspector
5. Set the `gameId` field in the inspector

### 4. Configure API Base URL
- Set default base URL in `AppInitializer` component
- Or receive it from WebView bridge at runtime

### 5. WebGL Build Settings
1. File → Build Settings → WebGL
2. Player Settings → WebGL:
   - Set Template to "Minimal" or custom template
   - Configure compression settings
   - Set data caching options

## Usage

### Starting a Game
1. User selects a game from the games list
2. `MainMenuController` calls `GameSessionManager.StartSession()`
3. On success, loads the game scene (replaces Main Scene)
4. Game scene initializes with session data
5. Game plays and ends
6. Game calls `EndGame()` which completes the session
7. Returns to Main Scene (replaces game scene)

### WebView Integration
The host mobile app should:
1. Load the Unity WebGL build in a WebView
2. Send auth token: `window.unityWebViewBridge.receiveFromHost('authToken', token)`
3. Send base URL: `window.unityWebViewBridge.receiveFromHost('baseUrl', url)`
4. Send tenant config: `window.unityWebViewBridge.receiveFromHost('tenantConfig', configJson)`
5. Listen for Unity messages via `window.unityWebViewBridge.sendMessage()`

## API Endpoints

All endpoints are prefixed with `/api/App`:

- `POST /player/register` - Register new player
- `POST /player/login` - Login player
- `GET /player/profile` - Get player profile
- `PUT /player/profile` - Update player profile
- `PUT /player/deactivate` - Deactivate account
- `GET /game/available` - Get available games
- `POST /gameSession/start` - Start game session
- `POST /gameSession/complete` - Complete game session
- `GET /gameSession/{id}` - Get session details
- `GET /gameSession/my-sessions` - Get my sessions
- `GET /gameSession/my-sessions/active` - Get active sessions
- `GET /score/my-total` - Get my total score
- `GET /score/my-weekly` - Get my weekly score
- `GET /score/weekly` - Get weekly leaderboard
- `GET /score/leaderboard/{gameId}` - Get game leaderboard
- `GET /products` - Get products (read-only)

## Creating a New Minigame

1. Create a new scene for your game
2. Create a script that inherits from `BaseMinigame`
3. Implement required methods:
   - `Initialize(GameSessionData session)` - Called before game starts
   - `StartGame()` - Called when game should start
   - `EndGame(int score, Dictionary<string, object> metadata)` - Called when game ends
4. Set the `gameId` field in the inspector
5. Add the scene name to your game's `GameInfo` in the backend

Example:
```csharp
public class MyGame : BaseMinigame
{
    protected override void OnGameStarted()
    {
        base.OnGameStarted();
        // Your game start logic
    }
    
    protected override void OnGameEnded(int score, Dictionary<string, object> metadata)
    {
        base.OnGameEnded(score, metadata);
        // Your game end logic
    }
}
```

## Notes

- All managers are singletons and persist across scenes
- Auth token is stored only in memory (not persisted)
- Games should not directly call APIs - use managers instead
- Scenes are loaded normally (non-additive) - each scene replaces the previous one
- Game scenes replace Main Scene when loaded, and Main Scene replaces game scene when returning

## License

[Your License Here]
