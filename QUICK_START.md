# Quick Start - New UI Features

## What's New

✅ **5 Main Menu Panels** (was 3):
- Games
- Profile  
- Leaderboards
- **Marketplace** (NEW)
- **Quests/Milestones** (NEW)

✅ **Popup System** (NEW):
- Error popups
- Instruction popups
- Message popups

## Quick Setup Checklist

### 1. Popup Prefabs (Required)
- [ ] Create `ErrorPopup` prefab with `PopupController`
- [ ] Create `InstructionPopup` prefab with `PopupController`
- [ ] Create `MessagePopup` prefab with `PopupController`
- [ ] Assign all 3 to `PopupManager` component

### 2. Marketplace Panel
- [ ] Create `MarketplacePanel` GameObject
- [ ] Add `MarketplaceController` component
- [ ] Create `ProductItem` prefab
- [ ] Wire up all references
- [ ] Add `MarketplaceButton` to navigation

### 3. Quests Panel
- [ ] Create `QuestsPanel` GameObject
- [ ] Add `QuestsController` component
- [ ] Create `QuestItem` prefab
- [ ] Wire up all references
- [ ] Add `QuestsButton` to navigation

### 4. Update MainMenuController
- [ ] Assign `marketplacePanel` reference
- [ ] Assign `questsPanel` reference
- [ ] Assign `marketplaceButton` reference
- [ ] Assign `questsButton` reference

## Usage

### Show Popups
```csharp
// Error
PopupManager.Instance.ShowError("Title", "Error message");

// Instructions
PopupManager.Instance.ShowInstructions("Title", "Instructions text");

// Message
PopupManager.Instance.ShowMessage("Title", "Message text");
```

### Game Instructions
Game instructions are automatically shown when selecting a game (if available in game metadata).

## API Notes

- **Marketplace**: Uses `/api/App/products` ✅ (fully integrated)
- **Quests**: Placeholder - needs API endpoints:
  - `GET /api/App/quests/weekly`
  - `GET /api/App/milestones`

See `UI_UPDATES.md` for detailed setup instructions.
