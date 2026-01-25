# TextMeshPro RTL Setup Guide

## Overview

All text components in the project have been updated to use **TextMeshPro** instead of Unity's standard `Text` component. This provides better rendering quality and built-in RTL (Right-to-Left) support for languages like Arabic, Hebrew, and Persian.

## Prerequisites

### Import TextMeshPro Package

1. In Unity Editor, go to **Window → Package Manager**
2. In the dropdown (top-left), select **Unity Registry**
3. Search for **"TextMeshPro"**
4. Click **Install** (or **Update** if already installed)
5. When prompted, click **Import TMP Essentials** and **Import TMP Examples & Extras** (optional but recommended)

## What Changed

All `Text` components have been replaced with `TextMeshProUGUI`:

### Updated Files:
- ✅ `MainMenuController.cs`
- ✅ `PopupController.cs`
- ✅ `GameItemController.cs`
- ✅ `ProfileController.cs`
- ✅ `LeaderboardController.cs`
- ✅ `LeaderboardEntryController.cs`
- ✅ `LoadingIndicator.cs`
- ✅ `ErrorDisplay.cs`
- ✅ `MarketplaceController.cs`
- ✅ `ProductItemController.cs`
- ✅ `QuestsController.cs`
- ✅ `QuestItemController.cs`
- ✅ `SampleDummyGame.cs`

## Unity Editor Setup

### Converting Existing Text Components

If you already have UI set up with standard `Text` components:

1. Select the GameObject with the `Text` component
2. In the Inspector, click the **Text** component
3. Click the **3-dot menu** (⋮) in the top-right of the component
4. Select **Convert to TextMeshPro**
5. Unity will automatically:
   - Replace the `Text` component with `TextMeshProUGUI`
   - Create a TMP font asset if needed
   - Update the component

**OR** manually:

1. Remove the `Text` component
2. Add Component → **TextMeshPro - Text (UI)**
3. Configure the text settings

### Creating New Text Elements

1. **GameObject → UI → Text - TextMeshPro**
2. This creates a `TextMeshProUGUI` component automatically

### RTL Support

TextMeshPro automatically detects RTL languages (Arabic, Hebrew, etc.) and renders them correctly. However, you can use the helper utility for additional control:

```csharp
using Minigames.UI;
using TMPro;

// Automatic RTL detection and configuration
RTLTextHelper.SetRTLTextAuto(textComponent, "مرحبا"); // Arabic text

// Manual RTL configuration
RTLTextHelper.ConfigureForRTL(textComponent, true); // Force RTL alignment
```

## RTL Helper Utility

A helper class `RTLTextHelper` has been created with utilities for RTL text:

### Methods:

- `SetRTLText(TextMeshProUGUI, string)` - Set text with RTL support
- `IsRTLText(string)` - Check if text contains RTL characters
- `SetTextWithRTL(TextMeshProUGUI, string, bool)` - Set text with optional RTL forcing
- `ConfigureForRTL(TextMeshProUGUI, bool)` - Configure alignment for RTL
- `SetRTLTextAuto(TextMeshProUGUI, string)` - Automatic RTL detection and configuration

## Font Setup for RTL

### Arabic/Farsi Fonts

1. Import your Arabic font (TTF/OTF)
2. Select the font asset
3. In Inspector, set **Font Asset Creator**
4. Configure:
   - **Source Font File**: Your font file
   - **Atlas Resolution**: 1024 or 2048 (higher for better quality)
   - **Character Set**: Unicode (or specific ranges)
   - **Character Sequence**: Include Arabic ranges (U+0600-U+06FF)
5. Click **Generate Font Atlas**
6. Save the font asset

### Using the Font

1. Select your `TextMeshProUGUI` component
2. Assign the generated font asset to **Font Asset** field
3. Set **Font Size** and other properties as needed

## Text Alignment for RTL

For RTL languages, typically use:
- **Right alignment** for RTL text
- **Left alignment** for LTR text

TextMeshPro can auto-detect, but you can manually set:
```csharp
textComponent.alignment = TextAlignmentOptions.MidlineRight; // RTL
textComponent.alignment = TextAlignmentOptions.MidlineLeft;  // LTR
```

## Common Issues

### Text Not Displaying
- Ensure TextMeshPro package is imported
- Check that Font Asset is assigned
- Verify the font includes the character set you're using

### RTL Text Not Rendering Correctly
- Ensure your font supports RTL characters
- Check font asset includes Arabic/Hebrew character ranges
- Verify text alignment is set correctly

### Missing Characters
- Regenerate font atlas with correct character ranges
- Increase atlas resolution if needed
- Use a font that supports your language

## Migration Checklist

- [ ] Import TextMeshPro package
- [ ] Convert all existing `Text` components to `TextMeshProUGUI`
- [ ] Update prefabs with TextMeshPro components
- [ ] Import/configure RTL fonts if needed
- [ ] Test with RTL text (Arabic, Hebrew, etc.)
- [ ] Verify text alignment for RTL languages

## Notes

- TextMeshPro provides better rendering quality than standard Text
- RTL support is built-in and automatic
- Font assets need to be created for custom fonts
- The helper utility (`RTLTextHelper`) is optional but recommended for explicit RTL control
