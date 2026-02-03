# Addressables Game Loading

Game scenes are loaded via **MiniGameLoader** using Unity Addressables when you click a game in the list.

## Flow

1. User clicks Play on a **GameItemController** → `OnGameSelected(gameInfo)`
2. **MainMenuController** starts session → calls `MiniGameLoader.LoadMiniGame(addressKey)`
3. Scene loads **additively** (main scene stays in memory)
4. **GameLauncher** in the game scene finds `IMinigame`, initializes and starts
5. When game ends → `MiniGameLoader.UnloadCurrentMiniGame()` unloads the game scene

## Addressable Key

- **Default**: `"MiniGame_" + sceneName` (e.g. `"MiniGame_Basketball"`)
- **Override**: Set `addressableKey` in backend `gameConfigurations` for the game

## Setup Checklist

1. **Add game scene to Addressables**
   - Window → Asset Management → Addressables → Groups
   - Add the scene (e.g. `Basketball.unity`) to a group
   - Set Address to `MiniGame_Basketball` (must match `MiniGame_` + sceneName)

2. **MiniGameLoader in MainScene**
   - Attached to the MainMenu GameObject
   - `mainMenuContentToRestore`: assign the main content root (same as MainMenuController's mainContentRoot) – it is hidden when loading a game and restored when returning
   - Optional: assign `loadingPanel`, `progressBar`, `progressText` for loading UI

3. **Backend game configuration**
   - `sceneName` in gameConfigurations should match (e.g. `"Basketball"`)
   - Or set `addressableKey` to override (e.g. `"MiniGame_Basketball"`)

## Fallback

If `MiniGameLoader.Instance` is null or the address key is empty, the project falls back to `SceneNavigationManager.LoadGameScene()` (regular scene load, replaces main scene).
