# Game Prefab Quick Reference

## Quick Setup Checklist

### ✅ Game Item Prefab (for Games List)

**Create once, reuse for all games**

```
Location: Assets/Prefabs/GameItemPrefab.prefab

Structure:
GameItemPrefab (Panel)
├── GameNameText (TextMeshProUGUI)
├── GameDescriptionText (TextMeshProUGUI)  
├── ThumbnailImage (Image) [Optional]
└── PlayButton (Button)
    └── Text (TextMeshProUGUI) - "Play"

Components:
- GameItemController (script)
  - Assign: GameNameText
  - Assign: GameDescriptionText
  - Assign: ThumbnailImage (optional)
  - Assign: PlayButton
```

### ✅ Game Scene (for each game)

**Create one scene per game**

```
Location: Assets/Scenes/YourGameScene.unity

Structure:
YourGameScene
├── GameLauncher (GameObject)
│   └── GameLauncher (Component) ← REQUIRED
│
└── YourGame (GameObject)
    └── YourGame (Component) ← Your script inheriting BaseMinigame
        - gameId: "your-game-id" ← Must match backend
```

## Code Template for New Game

```csharp
using UnityEngine;
using Minigames.Games;
using Minigames.Data;

public class YourGame : BaseMinigame
{
    [Header("Game Configuration")]
    [SerializeField] private string gameId = "your-game-id"; // Match backend
    
    private int score = 0;
    
    protected override void OnGameStarted()
    {
        base.OnGameStarted();
        // Initialize your game here
        score = 0;
    }
    
    private void Update()
    {
        if (!isGameActive) return;
        
        // Your game logic
    }
    
    // Call this when game ends
    private void OnGameOver()
    {
        EndGame(score); // Handles session completion automatically
    }
}
```

## Flow Summary

```
1. Games Loaded → PopulateGamesList()
2. User Clicks Play → OnGameSelected()
3. Start Session → GameSessionManager.StartSession()
4. Load Scene → SceneNavigationManager.LoadGameScene()
5. GameLauncher → Finds your game, calls Initialize() then StartGame()
6. Game Plays → Your game logic
7. Game Ends → EndGame(score) → Completes session → Returns to main
```

## MainMenuController Setup

**Required References:**

```csharp
[Header("Games List")]
[SerializeField] private Transform gamesListContainer; // ScrollView Content
[SerializeField] private GameObject gameItemPrefab;     // GameItemPrefab prefab
```

**Setup in Unity:**
1. Create ScrollView in Games Panel
2. Assign ScrollView's Content to `gamesListContainer`
3. Assign GameItemPrefab to `gameItemPrefab` field

## Scene Name Matching

**Critical:** Scene name must match backend

- Backend `GameDto.gameConfigurations` → `key="sceneName"` → `GameInfo.sceneName`
- OR Backend `GameDto.name` → used as fallback
- Unity scene: `{sceneName}.unity`

**Example:**
- Backend: `"sceneName": "SnakeGameScene"`
- Unity: `SnakeGameScene.unity`

## Common Issues

| Issue | Solution |
|-------|----------|
| Game doesn't start | Check GameLauncher exists in scene |
| Game item not showing | Check prefab assigned in MainMenuController |
| Session error | Check authentication and tenant ID |
| Scene not found | Check scene name matches GameInfo.sceneName |
| Game not initialized | Check game script inherits BaseMinigame |
