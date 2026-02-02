# Profile Panel Setup Guide

The Profile Panel displays player data from `GET /api/App/player/profile`.

## API Response Fields

| Field | Type | Description |
|-------|------|-------------|
| id | string | Player GUID |
| username | string | Display name |
| socialMediaId | string | Social login ID |
| countryCode | string | Country code (e.g. US, SA) |
| icon | string | Avatar image URL |
| frame | string | Profile frame image URL |
| score | int | Total score |
| lastLoginAt | datetime | Last login timestamp |
| isActive | bool | Account status |
| availableKeys | int | Keys balance |

## ProfileController UI References

The `ProfileController` is attached to the **ProfileTap** GameObject. Assign these in the Inspector:

### Profile Info (display only)
- **usernameText** – Username/display name
- **displayNameText** – Same as username (legacy)
- **countryCodeText** – Country code
- **scoreText** – Score from profile
- **totalScoreText** – Total score (from `/api/App/score/my-total`)
- **weeklyScoreText** – Weekly score (from `/api/App/score/my-weekly`)
- **availableKeysText** – Keys balance
- **lastLoginAtText** – Formatted last login (e.g. "2h ago")
- **isActiveText** – "Active" or "Inactive"

### Avatar
- **iconImage** – Loads from `profile.icon` URL if it starts with `http`
- **frameImage** – Loads from `profile.frame` URL if it starts with `http`

### Edit Profile
- **displayNameInput** – Input for display name (legacy)
- **usernameInput** – Input for username (3–100 chars)
- **countryCodeInput** – Input for country code
- **updateProfileButton** – Sends `PUT /api/App/player/profile`

### Actions
- **refreshButton** – Reloads profile and scores
- **logoutButton** – Logs out

### Loading
- **loadingIndicator** – Shown while profile is loading

## Minimum Setup

For a basic profile panel, create and assign at least:

1. **usernameText** – TextMeshProUGUI for username
2. **totalScoreText** – TextMeshProUGUI for total score
3. **weeklyScoreText** – TextMeshProUGUI for weekly score
4. **availableKeysText** – TextMeshProUGUI for keys
5. **refreshButton** – Button to reload
6. **logoutButton** – Button to log out

All other fields are optional. Unassigned references are safely ignored.

## Suggested Layout

```
ProfileTap (has ProfileController)
├── Header
│   ├── iconImage (avatar)
│   ├── usernameText
│   └── isActiveText
├── Stats
│   ├── totalScoreText
│   ├── weeklyScoreText
│   └── availableKeysText
├── Details
│   ├── countryCodeText
│   ├── lastLoginAtText
│   └── ...
├── Edit (optional)
│   ├── usernameInput
│   ├── countryCodeInput
│   └── updateProfileButton
└── Actions
    ├── refreshButton
    └── logoutButton
```

## Flow

1. User opens Profile tab → `ProfileController.Start()` runs
2. `LoadProfile()` calls `PlayerProfileManager.RefreshProfile()` → `GET /api/App/player/profile`
3. Also loads `GET /api/App/score/my-total` and `GET /api/App/score/my-weekly`
4. `HandleProfileUpdated()` fills all assigned UI elements
5. Refresh button repeats the load
6. Update button sends `PUT /api/App/player/profile` with username and countryCode
