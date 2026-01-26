# Game Start Flow & Prefab Setup Guide

## Overview

This guide explains how games are started after fetching the game list, the complete flow, and what prefabs/components you need to set up.

## Game Start Flow

### Complete Flow Diagram

```
1. User Logs In
   ↓
2. GameCatalogManager.LoadGames() fetches games from API
   ↓
3. MainMenuController.PopulateGamesList() creates game items
   ↓
4. User clicks "Play" button on a game item
   ↓
5. MainMenuController.OnGameSelected() called
   ↓
6. Show game instructions (if available)
   ↓
7. MainMenuController.StartGameSession() called
   ↓
8. GameSessionManager.StartSession() → API call to create session
   ↓
9. Session created successfully → SceneNavigationManager.LoadGameScene()
   ↓
10. Game scene loads additively (Main Scene stays loaded)
   ↓
11. GameLauncher.Start() finds IMinigame component
   ↓
12. GameLauncher initializes game with session data
   ↓
13. Game.StartGame() called → Game begins
   ↓
14. User plays game...
   ↓
15. Game.EndGame(score) called when game ends
   ↓
16. GameSessionManager.CompleteSession() → API call with encrypted score
   ↓
17. Session completed → SceneNavigationManager.UnloadGameScene()
   ↓
18. Return to Main Scene
```

## Required Prefabs

### 1. Game Item Prefab (for Games List)

**Location:** `Assets/Prefabs/GameItemPrefab.prefab`

**Purpose:** Displays each game in the games list panel

**Required Components:**
- `GameItemController` script
- UI elements:
  - `TextMeshProUGUI` for game name
  - `TextMeshProUGUI` for game description
  - `Image` for thumbnail (optional)
  - `Button` for "Play" button

**Setup Steps:**

1. **Create the Prefab:**
   ```
   GameObject → UI → Panel
   Name: "GameItemPrefab"
   ```

2. **Add GameItemController Script:**
   - Select the prefab
   - Add Component → `GameItemController`

3. **Create UI Structure:**
   ```
   GameItemPrefab (Panel)
   ├── GameNameText (TextMeshPro - Text (UI))
   ├── GameDescriptionText (TextMeshPro - Text (UI))
   ├── ThumbnailImage (Image) [Optional]
   └── PlayButton (Button)
       └── Text (TextMeshPro - Text (UI)) - "Play"
   ```

4. **Assign References in GameItemController:**
   - `Game Name Text` → GameNameText
   - `Game Description Text` → GameDescriptionText
   - `Thumbnail Image` → ThumbnailImage (optional)
   - `Play Button` → PlayButton

5. **Save as Prefab:**
   - Drag from Hierarchy to `Assets/Prefabs/` folder
   - Delete from Hierarchy

**Example Hierarchy:**
```
GameItemPrefab (RectTransform, Image, GameItemController)
├── GameNameText (TextMeshProUGUI) - "Game Name"
├── GameDescriptionText (TextMeshProUGUI) - "Description..."
├── ThumbnailImage (Image) [Optional]
└── PlayButton (Button)
    └── Text (TextMeshProUGUI) - "Play"
```

### 2. Game Scene Setup

**Location:** `Assets/Scenes/YourGameScene.unity`

**Purpose:** Individual game scene that loads additively

**Required Components:**

1. **GameLauncher Component** (Required)
   - Empty GameObject named "GameLauncher"
   - Add Component → `GameLauncher`
   - This automatically finds and initializes your game

2. **Your Game Script** (Required)
   - Must inherit from `BaseMinigame` or implement `IMinigame`
   - Contains your game logic
   - Must be on a GameObject in the scene

**Example Game Scene Structure:**

```
YourGameScene
├── GameLauncher (GameObject with GameLauncher script)
├── YourGame (GameObject with YourGame script inheriting BaseMinigame)
│   ├── Game UI Canvas (optional)
│   ├── Game Objects
│   └── ...
└── Camera (if needed)
```

## Code Structure

### MainMenuController.cs

**Key Methods:**

```csharp
// Called when games are loaded from API
private void HandleGamesLoaded(List<GameInfo> games)
{
    availableGames = games;
    PopulateGamesList(); // Creates game items
}

// Creates UI items for each game
private void PopulateGamesList()
{
    foreach (var game in availableGames)
    {
        GameObject itemObj = Instantiate(gameItemPrefab, gamesListContainer);
        GameItemController itemController = itemObj.GetComponent<GameItemController>();
        itemController.Setup(game, OnGameSelected);
    }
}

// Called when user clicks "Play" button
private void OnGameSelected(GameInfo game)
{
    // Show instructions, then start game
    StartGameSession(game);
}

// Starts the game session and loads game scene
private void StartGameSession(GameInfo game)
{
    // 1. Create session via API
    GameSessionManager.Instance.StartSession(
        game.id,
        null, // metadata
        (session) => {
            // 2. Load game scene
            SceneNavigationManager.Instance.LoadGameScene(game.sceneName);
        },
        (error) => {
            // Handle error
        }
    );
}
```

### GameLauncher.cs

**Purpose:** Automatically initializes game when scene loads

**How it works:**
1. Waits one frame for scene to fully load
2. Gets current session from `GameSessionManager`
3. Finds `BaseMinigame` (or `IMinigame`) component in scene
4. Calls `Initialize(session)` then `StartGame()`

**You don't need to call this manually** - it runs automatically when the game scene loads.

### BaseMinigame.cs

**Purpose:** Base class for all games

**Key Methods:**

```csharp
// Called by GameLauncher when scene loads
public virtual void Initialize(GameSessionData session)
{
    currentSession = session;
    isGameActive = false;
}

// Called by GameLauncher after Initialize
public virtual void StartGame()
{
    isGameActive = true;
    OnGameStarted(); // Override this
}

// Call this when game ends
public virtual void EndGame(int score, Dictionary<string, object> metadata = null)
{
    // Automatically completes session and returns to main scene
}
```

## Creating a New Game

### Step-by-Step Guide

#### 1. Create Game Scene

1. **File → New Scene**
2. Name it: `YourGameScene` (must match `sceneName` in backend)
3. Save to `Assets/Scenes/`

#### 2. Add GameLauncher

1. **GameObject → Create Empty**
2. Name: `GameLauncher`
3. Add Component → `GameLauncher`

#### 3. Create Your Game Script

```csharp
using UnityEngine;
using Minigames.Games;
using Minigames.Data;

public class YourGame : BaseMinigame
{
    [Header("Game Configuration")]
    [SerializeField] private string gameId = "your-game-id"; // Must match backend
    
    private int score = 0;
    
    protected override void OnGameStarted()
    {
        base.OnGameStarted();
        // Your game initialization code
        score = 0;
    }
    
    private void Update()
    {
        if (!isGameActive) return;
        
        // Your game logic here
        // Example: increment score on click
        if (Input.GetMouseButtonDown(0))
        {
            score += 10;
        }
    }
    
    // Call this when game should end
    private void EndYourGame()
    {
        EndGame(score); // BaseMinigame handles session completion
    }
}
```

#### 4. Add Game Script to Scene

1. **GameObject → Create Empty**
2. Name: `YourGame`
3. Add Component → `YourGame` script
4. Set `Game Id` field to match backend game ID

#### 5. Configure Scene Name

**Important:** The scene name must match what's returned from the backend API in `GameInfo.sceneName`.

- Backend `GameDto` → `gameConfigurations` with key `"sceneName"` → becomes `GameInfo.sceneName`
- OR backend `GameDto.name` is used as fallback

**Example:**
- If backend returns `sceneName: "YourGameScene"`, your scene file must be named `YourGameScene.unity`
- If backend returns `name: "Your Game"`, scene should be named `YourGame.unity` (spaces removed)

#### 6. Add Scene to Build Settings

1. **File → Build Settings**
2. Click **Add Open Scenes**
3. Ensure `MainScene` is at index 0
4. Your game scene should be added automatically

## Prefab Setup Checklist

### Game Item Prefab

- [ ] Created prefab in `Assets/Prefabs/`
- [ ] Has `GameItemController` component
- [ ] Has `TextMeshProUGUI` for game name
- [ ] Has `TextMeshProUGUI` for description
- [ ] Has `Button` for play button
- [ ] All references assigned in `GameItemController`
- [ ] Prefab saved

### Main Scene Setup

- [ ] `MainMenuController` has `gameItemPrefab` assigned
- [ ] `MainMenuController` has `gamesListContainer` assigned (e.g., ScrollView Content)
- [ ] Games Panel has ScrollView with Content area
- [ ] Content area has Vertical Layout Group (optional, for auto-layout)

### Game Scene Setup

- [ ] Game scene created and named correctly
- [ ] `GameLauncher` GameObject added to scene
- [ ] Your game script inherits `BaseMinigame`
- [ ] Game script has `gameId` set correctly
- [ ] Scene added to Build Settings
- [ ] Scene name matches backend `sceneName` or `name`

## Example: Complete Game Scene Setup

```
YourGameScene
├── GameLauncher (GameObject)
│   └── GameLauncher (Component)
│
├── YourGame (GameObject)
│   ├── YourGame (Component) - inherits BaseMinigame
│   ├── Canvas (UI)
│   │   ├── ScoreText (TextMeshProUGUI)
│   │   ├── TimerText (TextMeshProUGUI)
│   │   └── EndButton (Button)
│   └── GameObjects (your game logic)
│
└── Main Camera
```

## Important Notes

### Scene Name Matching

The `sceneName` from `GameInfo` must match your Unity scene file name:

- Backend `GameDto.gameConfigurations` with `key="sceneName"` → `GameInfo.sceneName`
- OR Backend `GameDto.name` → used as fallback
- Unity scene file: `{sceneName}.unity`

**Example:**
- Backend returns: `{ "name": "Snake Game", "gameConfigurations": [{ "key": "sceneName", "value": "SnakeGameScene" }] }`
- Unity scene file: `SnakeGameScene.unity`

### Game ID Matching

Your game script's `gameId` field must match the backend game ID:

- Backend `GameDto` → converted to `GameInfo` with `id`
- Your `BaseMinigame.gameId` must match `GameInfo.id`

**Note:** Currently backend doesn't provide game ID in `GameDto`, so we use game name as temporary ID. This should be fixed when backend adds ID field.

### Session Lifecycle

**Critical:** Always follow this order:

1. ✅ `GameSessionManager.StartSession()` - Creates session on backend
2. ✅ `SceneNavigationManager.LoadGameScene()` - Loads game scene
3. ✅ `GameLauncher` automatically initializes game
4. ✅ Game plays...
5. ✅ `BaseMinigame.EndGame(score)` - Completes session and returns to main

**Never:**
- ❌ Load game scene before starting session
- ❌ Call `EndGame()` without an active session
- ❌ Manually unload scene (let `BaseMinigame` handle it)

## Troubleshooting

### Game doesn't start

1. **Check GameLauncher exists** in game scene
2. **Check game script** inherits `BaseMinigame` or implements `IMinigame`
3. **Check session** exists: `GameSessionManager.Instance.GetCurrentSession() != null`
4. **Check scene name** matches `GameInfo.sceneName`
5. **Check Build Settings** - scene is added

### Game item not showing

1. **Check prefab** is assigned in `MainMenuController`
2. **Check container** is assigned (`gamesListContainer`)
3. **Check games loaded** - `GameCatalogManager.Instance.AreGamesLoaded()`
4. **Check game is active** - `game.isActive == true`

### Session errors

1. **Check authentication** - `AuthManager.Instance.IsAuthenticated()`
2. **Check tenant ID** - `ApiClient.Instance.GetTenantId()`
3. **Check API response** - look for errors in console
4. **Check game ID** - matches backend game ID

## Summary

**To start a game:**

1. ✅ Games are fetched and displayed in list
2. ✅ User clicks "Play" button
3. ✅ `GameSessionManager.StartSession()` creates session
4. ✅ `SceneNavigationManager.LoadGameScene()` loads game scene
5. ✅ `GameLauncher` finds and initializes your game
6. ✅ Game plays and calls `EndGame(score)`
7. ✅ Session completes and returns to main scene

**Required Prefabs:**

1. **GameItemPrefab** - for displaying games in list
2. **Game Scene** - with `GameLauncher` and your game script

**Required Components:**

1. `GameItemController` - on GameItemPrefab
2. `GameLauncher` - in each game scene
3. Your game script - inheriting `BaseMinigame`
