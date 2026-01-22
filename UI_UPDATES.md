# UI Updates Summary

## New Features Added

### 1. Expanded Main Menu Panels

The main menu now includes **5 panels** instead of 3:

1. **Games Panel** - List of available games (existing)
2. **Profile Panel** - Player profile and stats (existing)
3. **Leaderboards Panel** - Weekly and game-specific leaderboards (existing)
4. **Marketplace Panel** - NEW - Displays products from API
5. **Quests Panel** - NEW - Displays weekly quests and milestones

### 2. Popup System

A centralized popup system has been added with three types of popups:

- **Error Popups** - For displaying error messages
- **Instruction Popups** - For displaying game instructions
- **Message Popups** - For general messages/information

### 3. New Managers

- **ProductManager** - Manages marketplace products
- **QuestManager** - Manages weekly quests and milestones (placeholder for future API)

### 4. New UI Controllers

- **MarketplaceController** - Controls marketplace panel
- **ProductItemController** - Individual product item display
- **QuestsController** - Controls quests/milestones panel
- **QuestItemController** - Individual quest/milestone item display
- **PopupManager** - Centralized popup management
- **PopupController** - Individual popup instance controller

## Updated Files

### MainMenuController.cs
- Added `marketplacePanel` and `questsPanel` references
- Added `marketplaceButton` and `questsButton` navigation buttons
- Updated `ShowPanel()` to handle all 5 panels
- Updated error handling to use PopupManager
- Added game instructions popup before starting games

### AppInitializer.cs
- Added initialization for ProductManager, QuestManager, and PopupManager

## New Data Models

### QuestModels.cs
- `Quest` - Model for weekly quests
- `Milestone` - Model for milestones
- `QuestListResponse` - API response wrapper
- `MilestoneListResponse` - API response wrapper

## Setup Instructions

### 1. Create Popup Prefabs

You need to create 3 popup prefabs in Unity Editor:

#### Error Popup Prefab
1. Create GameObject → UI → Panel
2. Name it `ErrorPopup`
3. Add Component → `PopupController`
4. Add child elements:
   - **Text** for title (`TitleText`)
   - **Text** for content (`ContentText`)
   - **Button** for close (`CloseButton`)
5. Wire up references in `PopupController`
6. Save as prefab in `Assets/Prefabs`

#### Instruction Popup Prefab
1. Same structure as Error Popup
2. Name it `InstructionPopup`
3. Can have different styling (e.g., different background color)
4. Save as prefab

#### Message Popup Prefab
1. Same structure as Error Popup
2. Name it `MessagePopup`
3. Save as prefab

### 2. Update Main Scene UI

Add the new panels and buttons to your Main Scene:

#### Add Marketplace Panel
1. Create Panel under Canvas
2. Name it `MarketplacePanel`
3. Add Component → `MarketplaceController`
4. Create child elements:
   - `ProductsListContainer` (Vertical Layout Group)
   - `RefreshButton`
   - `LoadingText`
   - `EmptyStatePanel` (optional)
5. Wire up all references in `MarketplaceController`

#### Add Quests Panel
1. Create Panel under Canvas
2. Name it `QuestsPanel`
3. Add Component → `QuestsController`
4. Create child elements:
   - `QuestTypeDropdown` (Weekly Quests / Milestones)
   - `QuestsListContainer` (Vertical Layout Group)
   - `RefreshButton`
   - `LoadingText`
   - `EmptyStatePanel` (optional)
5. Wire up all references in `QuestsController`

#### Add Navigation Buttons
1. Add `MarketplaceButton` to navigation bar
2. Add `QuestsButton` to navigation bar
3. Wire up in `MainMenuController`

### 3. Create Item Prefabs

#### Product Item Prefab
1. Create GameObject → UI → Button (or Panel)
2. Name it `ProductItem`
3. Add Component → `ProductItemController`
4. Add child elements:
   - `ProductNameText`
   - `ProductDescriptionText`
   - `ProductCostText`
   - `ProductImage` (optional)
   - `SelectButton` (or use parent button)
5. Wire up references
6. Save as prefab

#### Quest Item Prefab
1. Create GameObject → UI → Panel
2. Name it `QuestItem`
3. Add Component → `QuestItemController`
4. Add child elements:
   - `TitleText`
   - `DescriptionText`
   - `ProgressText`
   - `ProgressSlider`
   - `RewardText`
   - `ClaimButton`
   - `CompletedBadge` (GameObject, optional)
   - `ClaimedBadge` (GameObject, optional)
5. Wire up references
6. Save as prefab

### 4. Setup PopupManager

1. Find or create `PopupManager` GameObject in Main Scene
2. Add Component → `PopupManager`
3. Assign popup prefabs:
   - `Error Popup Prefab`
   - `Instruction Popup Prefab`
   - `Message Popup Prefab`
4. `Popup Container` will be auto-created if not assigned

## Usage Examples

### Showing an Error Popup
```csharp
PopupManager.Instance.ShowError("Connection Error", "Failed to connect to server");
```

### Showing Game Instructions
```csharp
PopupManager.Instance.ShowInstructions(
    "How to Play",
    "Press SPACE to score points. Game lasts 30 seconds.",
    () => {
        // Callback when popup is closed
        Debug.Log("Instructions read");
    }
);
```

### Showing a Message
```csharp
PopupManager.Instance.ShowMessage("Success", "Your score has been saved!");
```

### From a Game Script
```csharp
using Minigames.Games;

// Show instructions
GameInstructionsHelper.ShowInstructions("Game Rules", "Collect all coins!");

// Show message
GameInstructionsHelper.ShowMessage("Level Complete", "Great job!");

// Show error
GameInstructionsHelper.ShowError("Game Error", "Something went wrong");
```

## API Integration Notes

### Products (Marketplace)
- Uses existing `/api/App/products` endpoint
- Fully integrated with `ProductManager`

### Quests/Milestones
- **QuestManager** is currently a placeholder
- API endpoints need to be added to backend:
  - `GET /api/App/quests/weekly` - Get weekly quests
  - `GET /api/App/milestones` - Get milestones
- Update `QuestManager.cs` when endpoints are available

## Game Instructions

Game instructions can be provided in two ways:

1. **Via GameInfo metadata** - Add `instructions` key to game metadata in API
2. **Fallback to description** - If no instructions, uses game description

Instructions are automatically shown when a game is selected, before starting the session.

## Notes

- All popups are displayed on top of other UI (high sorting order)
- Popups can be closed by clicking the close button or background
- Multiple popups can be stacked (newest on top)
- Use `PopupManager.CloseAllPopups()` to clear all popups
- Product redemption and quest claiming are handled by host app, not Unity
