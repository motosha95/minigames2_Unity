# Unity Minigames Setup Guide

This guide will help you set up the Unity project and create the necessary scenes and UI.

## Prerequisites

- Unity Hub installed
- Unity LTS version (2020.3 or later recommended)
- Basic knowledge of Unity Editor

## Step 1: Create Unity Project

1. Open Unity Hub
2. Click "New Project"
3. Select "2D" or "3D" template (doesn't matter, we'll use UI)
4. Name it "MinigamesHub" or similar
5. Choose a location and click "Create"

## Step 2: Import Project Files

1. Copy all files from this repository into your Unity project folder
2. Ensure the folder structure matches:
   ```
   Assets/
   ├── Scripts/
   ├── Scenes/
   ├── Prefabs/
   ├── Resources/
   └── Plugins/
   ```

## Step 3: Set Build Target to WebGL

1. Go to **File → Build Settings**
2. Select **WebGL** platform
3. Click **Switch Platform**
4. Wait for Unity to reimport assets

## Step 4: Create Main Scene

### 4.1 Create the Scene

1. Right-click in `Assets/Scenes` folder
2. Create → Scene
3. Name it `MainScene`
4. Double-click to open it

### 4.2 Set Up App Initializer

1. Create empty GameObject: **GameObject → Create Empty**
2. Name it `AppInitializer`
3. In Inspector, click **Add Component**
4. Search for `AppInitializer` and add it
5. Set the `Default Base Url` field to your API base URL

### 4.3 Create UI Canvas

1. **GameObject → UI → Canvas**
2. Name it `MainCanvas`
3. Set Canvas Scaler:
   - UI Scale Mode: **Scale With Screen Size**
   - Reference Resolution: **1920 x 1080**
   - Match: **0.5**

### 4.4 Create Main Menu Structure

Create the following UI hierarchy under Canvas:

```
Canvas
├── MainMenuController (Empty GameObject)
│   └── MainMenuController component
├── GamesPanel (Panel)
│   ├── GamesListContainer (Vertical Layout Group)
│   └── ScrollRect (optional)
├── ProfilePanel (Panel)
│   └── ProfileController component
├── LeaderboardsPanel (Panel)
│   └── LeaderboardController component
├── MarketplacePanel (Panel)
│   └── MarketplaceController component
├── QuestsPanel (Panel)
│   └── QuestsController component
├── LoadingPanel (Panel)
│   └── LoadingIndicator component
├── ErrorPanel (Panel) [Optional - now using PopupManager]
│   └── ErrorDisplay component
├── PopupManager (Empty GameObject)
│   └── PopupManager component
└── NavigationBar (Horizontal Layout Group)
    ├── GamesButton
    ├── ProfileButton
    ├── LeaderboardsButton
    ├── MarketplaceButton
    └── QuestsButton
```

### 4.5 Set Up MainMenuController

1. Select `MainMenuController` GameObject
2. Add Component → `MainMenuController`
3. Wire up references:
   - **Games Panel**: Drag `GamesPanel` GameObject
   - **Profile Panel**: Drag `ProfilePanel` GameObject
   - **Leaderboards Panel**: Drag `LeaderboardsPanel` GameObject
   - **Loading Panel**: Drag `LoadingPanel` GameObject
   - **Error Panel**: Drag `ErrorPanel` GameObject
   - **Games Button**: Drag `GamesButton` GameObject
   - **Profile Button**: Drag `ProfileButton` GameObject
   - **Leaderboards Button**: Drag `LeaderboardsButton` GameObject
   - **Marketplace Button**: Drag `MarketplaceButton` GameObject
   - **Quests Button**: Drag `QuestsButton` GameObject
   - **Games List Container**: Drag `GamesListContainer` GameObject
   - **Game Item Prefab**: Create this next (see 4.6)
   - **Error Text**: Drag Text component from `ErrorPanel` (optional, now using popups)

### 4.6 Create Game Item Prefab

1. **GameObject → UI → Button**
2. Name it `GameItem`
3. Add Component → `GameItemController`
4. Under Button, add child:
   - **GameObject → UI → Text** (name: `GameNameText`)
   - **GameObject → UI → Text** (name: `GameDescriptionText`)
   - **GameObject → UI → Image** (name: `ThumbnailImage`) - optional
5. In `GameItemController` component:
   - Wire up `Game Name Text`, `Game Description Text`, `Thumbnail Image`, `Play Button`
6. Drag `GameItem` from Hierarchy to `Assets/Prefabs` folder to create prefab
7. Delete `GameItem` from Hierarchy (keep prefab)

### 4.7 Set Up Profile Panel

1. Select `ProfilePanel` GameObject
2. Add Component → `ProfileController`
3. Create UI elements:
   - **Text** for username
   - **Text** for display name
   - **Text** for total score
   - **Text** for weekly score
   - **InputField** for display name input
   - **Button** for refresh
   - **Button** for update display name
4. Wire up all references in `ProfileController` component

### 4.8 Set Up Leaderboards Panel

1. Select `LeaderboardsPanel` GameObject
2. Add Component → `LeaderboardController`
3. Create UI elements:
   - **Dropdown** for leaderboard type (Weekly/Game)
   - **Dropdown** for game selection
   - **Vertical Layout Group** for leaderboard entries
   - **Button** for refresh
4. Create `LeaderboardEntry` prefab:
   - **GameObject → UI → Panel** (or Button)
   - Add Component → `LeaderboardEntryController`
   - Add child Text elements: `RankText`, `PlayerNameText`, `ScoreText`
   - Drag to Prefabs folder
5. Wire up references in `LeaderboardController`

### 4.9 Set Up Loading and Error Panels

1. Select `LoadingPanel`
2. Add Component → `LoadingIndicator`
3. Add child **Text** for loading message
4. Wire up references

1. Select `ErrorPanel`
2. Add Component → `ErrorDisplay`
3. Add child **Text** for error message
4. Add child **Button** for close
5. Wire up references

### 4.10 Set Up Navigation Buttons

1. Select each button (`GamesButton`, `ProfileButton`, `LeaderboardsButton`)
2. Add OnClick listeners (they're handled by `MainMenuController`)

### 4.11 Set Up Marketplace Panel

1. Select `MarketplacePanel` GameObject
2. Add Component → `MarketplaceController`
3. Create UI elements:
   - **Vertical Layout Group** for products list (`ProductsListContainer`)
   - **Button** for refresh (`RefreshButton`)
   - **Text** for loading state (`LoadingText`)
   - **Panel** for empty state (`EmptyStatePanel`) - optional
4. Wire up all references in `MarketplaceController`
5. Create `ProductItem` prefab (see 4.12)

### 4.12 Create Product Item Prefab

1. **GameObject → UI → Button** (or Panel)
2. Name it `ProductItem`
3. Add Component → `ProductItemController`
4. Add child elements:
   - **Text** for product name (`ProductNameText`)
   - **Text** for description (`ProductDescriptionText`)
   - **Text** for cost (`ProductCostText`)
   - **Image** for product image (`ProductImage`) - optional
5. Wire up references in `ProductItemController`
6. Drag to `Assets/Prefabs` folder

### 4.13 Set Up Quests Panel

1. Select `QuestsPanel` GameObject
2. Add Component → `QuestsController`
3. Create UI elements:
   - **Dropdown** for quest type (`QuestTypeDropdown`)
   - **Vertical Layout Group** for quests list (`QuestsListContainer`)
   - **Button** for refresh (`RefreshButton`)
   - **Text** for loading state (`LoadingText`)
   - **Panel** for empty state (`EmptyStatePanel`) - optional
4. Wire up all references in `QuestsController`
5. Create `QuestItem` prefab (see 4.14)

### 4.14 Create Quest Item Prefab

1. **GameObject → UI → Panel**
2. Name it `QuestItem`
3. Add Component → `QuestItemController`
4. Add child elements:
   - **Text** for title (`TitleText`)
   - **Text** for description (`DescriptionText`)
   - **Text** for progress (`ProgressText`)
   - **Slider** for progress bar (`ProgressSlider`)
   - **Text** for reward (`RewardText`)
   - **Button** for claim (`ClaimButton`)
   - **GameObject** for completed badge (`CompletedBadge`) - optional
   - **GameObject** for claimed badge (`ClaimedBadge`) - optional
5. Wire up references in `QuestItemController`
6. Drag to `Assets/Prefabs` folder

### 4.15 Set Up Popup System

1. Create empty GameObject: **GameObject → Create Empty**
2. Name it `PopupManager`
3. Add Component → `PopupManager`
4. Create popup prefabs (see 4.16)
5. Assign prefabs in `PopupManager` component:
   - **Error Popup Prefab**
   - **Instruction Popup Prefab**
   - **Message Popup Prefab`
6. `Popup Container` will be auto-created if not assigned

### 4.16 Create Popup Prefabs

For each popup type (Error, Instruction, Message):

1. **GameObject → UI → Panel**
2. Name it `ErrorPopup`, `InstructionPopup`, or `MessagePopup`
3. Add Component → `PopupController`
4. Add child elements:
   - **Text** for title (`TitleText`)
   - **Text** for content (`ContentText`)
   - **Button** for close (`CloseButton`)
5. Style as needed (different colors for different types)
6. Wire up references in `PopupController`
7. Save as prefab in `Assets/Prefabs`
8. Assign to `PopupManager` component

### 4.17 Save Main Scene

1. **File → Save** (Ctrl+S / Cmd+S)
2. Ensure `MainScene` is saved in `Assets/Scenes`

## Step 5: Create Sample Game Scene

### 5.1 Create the Scene

1. Right-click in `Assets/Scenes` folder
2. Create → Scene
3. Name it `SampleDummyGameScene`
4. Double-click to open it

### 5.2 Set Up Game GameObject

1. Create empty GameObject: **GameObject → Create Empty**
2. Name it `SampleDummyGame`
3. Add Component → `SampleDummyGame`
4. Set `Game Id` field (e.g., "sample-dummy-game")

### 5.3 Create UI for Game

1. **GameObject → UI → Canvas**
2. Name it `GameCanvas`
3. Under Canvas, create:
   - **Text** for score display (`ScoreText`)
   - **Text** for timer (`TimerText`)
   - **Button** for end game (`EndGameButton`)
   - **Button** for return (`ReturnButton`)

### 5.4 Wire Up SampleDummyGame

1. Select `SampleDummyGame` GameObject
2. In `SampleDummyGame` component:
   - Wire up `Score Text`
   - Wire up `Timer Text`
   - Wire up `End Game Button`
   - Wire up `Return Button`

### 5.5 Add GameLauncher

1. Create empty GameObject: **GameObject → Create Empty**
2. Name it `GameLauncher`
3. Add Component → `GameLauncher`
4. This will automatically initialize the game when scene loads

### 5.6 Save Game Scene

1. **File → Save**
2. Ensure `SampleDummyGameScene` is saved

## Step 6: Configure Build Settings

1. **File → Build Settings**
2. Ensure **WebGL** is selected
3. Click **Player Settings**
4. Under **WebGL**:
   - **Template**: Choose "Minimal" or create custom
   - **Compression Format**: Brotli (recommended) or Gzip
   - **Data Caching**: Enable if needed
5. Under **Other Settings**:
   - Set **Company Name** and **Product Name**
   - Set **Version**

## Step 7: Test in Editor

1. Open `MainScene`
2. Press **Play** button
3. Test navigation between panels
4. Test game loading (will fail without API, but should show errors properly)

## Step 8: Build for WebGL

1. **File → Build Settings**
2. Click **Add Open Scenes** to add `MainScene`
3. Click **Build**
4. Choose output folder
5. Wait for build to complete

## Step 9: WebView Integration

### For React Native:

```javascript
import { WebView } from 'react-native-webview';

<WebView
  source={{ uri: 'path/to/your/build/index.html' }}
  onMessage={(event) => {
    const message = JSON.parse(event.nativeEvent.data);
    // Handle Unity messages
  }}
  injectedJavaScript={`
    window.unityWebViewBridge = {
      receiveFromHost: function(type, data) {
        // Send to Unity
        if (window.unityInstance) {
          window.unityWebViewBridge.sendToUnity('Receive' + type, data);
        }
      }
    };
    
    // Send auth token
    window.unityWebViewBridge.receiveFromHost('authToken', 'your-token-here');
    window.unityWebViewBridge.receiveFromHost('baseUrl', 'https://api.example.com');
  `}
/>
```

### For iOS (WKWebView):

```swift
// Send message to Unity
webView.evaluateJavaScript("""
    window.unityWebViewBridge.receiveFromHost('authToken', '\(token)');
    window.unityWebViewBridge.receiveFromHost('baseUrl', '\(baseUrl)');
""")
```

### For Android (WebView):

```java
// Send message to Unity
webView.evaluateJavascript(
    "window.unityWebViewBridge.receiveFromHost('authToken', '" + token + "');",
    null
);
```

## Troubleshooting

### Scripts not showing in Inspector
- Check that scripts are in `Assets/Scripts` folder
- Ensure Unity has finished importing (check bottom-right progress bar)
- Try **Assets → Refresh** (Ctrl+R / Cmd+R)

### UI not displaying
- Check Canvas settings
- Ensure Canvas Scaler is configured
- Check that panels are active and have proper layout groups

### Game not loading
- Check that scene name matches `GameInfo.sceneName` from API
- Ensure `GameLauncher` component exists in game scene
- Check console for errors

### API calls failing
- Verify base URL is set correctly
- Check that auth token is being received from WebView
- Check browser console for CORS errors (if testing in browser)

## Next Steps

1. Connect to your backend API
2. Test authentication flow
3. Test game session lifecycle
4. Add more minigames
5. Customize UI/UX
6. Add tenant configuration support
