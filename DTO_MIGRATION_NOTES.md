# DTO Migration Notes

## Summary

All API models have been updated to match the backend DTOs structure from `DTOs.txt`.

## Key Changes

### 1. Authentication

**Before:**
- Register: username, email, password
- Login: username/email + password OR socialMediaId

**After (matches backend):**
- Register: `RegisterTenantPlayerDto` - username, socialMediaId, countryCode (optional)
- Login: `LoginTenantPlayerDto` - socialMediaId only
- Response: `TenantPlayerAuthResultDto` - isSuccess, token, player, errorMessage

**Impact:**
- ✅ `AuthManager` updated to use new DTOs
- ✅ `DtoConverter` converts `PlayerDto` to `PlayerProfile` for compatibility
- ✅ Legacy fields kept in `PlayerLoginRequest`/`PlayerRegisterRequest` for backward compatibility
- ⚠️ OTP flow may not be supported by backend (kept for compatibility)

### 2. Player Profile

**Before:**
- `PlayerProfile`: id, username, email, displayName, totalScore, weeklyScore, metadata

**After (matches backend):**
- `PlayerDto`: username, socialMediaId, countryCode, icon, frame, score, lastLoginAt, isActive
- `UpdatePlayerProfileDto`: username, countryCode, icon, frame

**Impact:**
- ✅ `DtoConverter.ToPlayerProfile()` converts `PlayerDto` to `PlayerProfile`
- ✅ `PlayerProfile` keeps legacy fields for compatibility
- ✅ `FetchProfile()` now uses `PlayerDto` and converts
- ✅ `UpdateProfile()` now uses `UpdatePlayerProfileDto`

### 3. Game Sessions

**Before:**
- Start: gameId (string), metadata (Dictionary)
- Complete: gameSessionId, score (int), metadata (Dictionary)

**After (matches backend):**
- Start: `StartGameDto` - gameId (Guid as string), gameData (string)
- Complete: `CompleteGameDto` - gameSessionId (Guid), encryptedScore (string), gameData (string)

**Impact:**
- ✅ `GameSessionManager.StartSession()` converts metadata Dictionary to JSON string
- ✅ `GameSessionManager.CompleteSession()` encrypts score using `ScoreEncryptionHelper`
- ⚠️ **Score Encryption**: Currently uses placeholder base64 encoding - **MUST BE REPLACED** with actual encryption matching backend
- ✅ `GameSessionDto` response converted to `GameSessionData` via `DtoConverter`

### 4. Games

**Before:**
- `GameInfo`: id, name, description, sceneName, thumbnailUrl, isActive, metadata

**After (matches backend):**
- `GameDto`: name, description, maxScore, timeLimit, isActive, rules, gameConfigurations
- Response: `PagedResult<GameDto>`

**Impact:**
- ✅ `GameCatalogManager` updated to handle `PagedResult<GameDto>`
- ✅ `DtoConverter.ToGameInfo()` converts `GameDto` to `GameInfo`
- ⚠️ **Game ID**: Backend `GameDto` doesn't include ID - currently using game name/index as ID. Backend should provide ID.
- ✅ `sceneName` extracted from `gameConfigurations` if available

### 5. Scores

**Before:**
- `ScoreResponse`: score, playerId, gameId

**After (matches backend):**
- `PlayerScoreDto`: playerId, playerName, totalScore, gamesPlayed, lastGamePlayed
- `WeeklyScoreDto`: playerId, playerName, weeklyScore, gamesPlayedThisWeek, weekStartDate, weekEndDate
- `GameScoreDto`: playerId, playerName, gameId, gameName, totalScore, gamesPlayed, bestScore
- Responses: `PagedResult<T>` for leaderboards

**Impact:**
- ✅ `LeaderboardManager` updated to use correct DTOs
- ✅ Converts `PagedResult<WeeklyScoreDto>` and `PagedResult<GameScoreDto>` to `LeaderboardResponse`

### 6. Token Authentication

**Status:** ✅ Already implemented correctly

All API requests now include:
- `Authorization: Bearer {token}` header (set after login)
- `X-Tenant-Id: {tenant-id}` header (set on startup)

Example request:
```bash
GET /api/App/game/available
Headers:
  Authorization: Bearer {token}
  X-Tenant-Id: {tenant-id}
```

## Critical TODOs

### 1. Score Encryption ⚠️ **REQUIRED**

**Current:** Placeholder base64 encoding in `ScoreEncryptionHelper.EncryptScore()`

**Required:** Implement actual encryption that matches backend decryption logic.

**Location:** `Assets/Scripts/Core/ScoreEncryptionHelper.cs`

**Backend expects:** Encrypted string, max 500 chars

### 2. Game ID ⚠️ **MAY NEED FIX**

**Current:** Using game name/index as temporary ID

**Issue:** Backend `GameDto` doesn't include game ID, but API endpoints use `gameId` (Guid)

**Solution:** 
- Backend should include `id` in `GameDto`, OR
- Extract ID from API response metadata, OR
- Use a different endpoint to get game IDs

### 3. Session ID ⚠️ **VERIFY**

**Current:** Added `id` field to `GameSessionDto`

**Verify:** Backend response includes session ID. If not, we use `playerId_gameId` as fallback.

## Backward Compatibility

Legacy models kept for compatibility:
- `PlayerLoginRequest` - has socialMediaId + legacy fields
- `PlayerRegisterRequest` - has username, socialMediaId, countryCode + legacy fields
- `PlayerProfile` - converted from `PlayerDto`, keeps legacy fields
- `GameSessionStartRequest` / `GameSessionCompleteRequest` - kept for compatibility

## Testing Checklist

- [ ] Login with socialMediaId works
- [ ] Register with username + socialMediaId works
- [ ] Games list loads correctly
- [ ] Game session start works
- [ ] Game session complete with encrypted score works
- [ ] Profile fetch works
- [ ] Profile update works
- [ ] Leaderboards load correctly
- [ ] Score encryption matches backend decryption

## API Request Examples

### Login
```bash
POST /api/App/player/login
Headers:
  X-Tenant-Id: c0f9c315-4631-47fa-b00e-4e7b9d1f34fb
Body: {
  "socialMediaId": "1"
}
Response: {
  "success": true,
  "data": {
    "isSuccess": true,
    "token": "eyJhbGci...",
    "player": { PlayerDto },
    "errorMessage": null
  }
}
```

### Get Games
```bash
GET /api/App/game/available
Headers:
  Authorization: Bearer {token}
  X-Tenant-Id: {tenant-id}
Response: {
  "success": true,
  "data": {
    "items": [ GameDto, ... ],
    "total": 10,
    "page": 1,
    "pageSize": 20
  }
}
```

### Complete Session
```bash
POST /api/App/gameSession/complete
Headers:
  Authorization: Bearer {token}
  X-Tenant-Id: {tenant-id}
Body: {
  "gameSessionId": "guid-string",
  "encryptedScore": "base64-encrypted-score",
  "gameData": "optional-json-string"
}
```
