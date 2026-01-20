# API Reference Quick Guide

All endpoints are prefixed with `/api/App`

## Authentication

All endpoints marked with `[PLAYER]` require authentication via `Authorization: Bearer {token}` header.

## Endpoints

### Player Management

#### Register Player
```
POST /api/App/player/register
Body: {
  "username": "string",
  "email": "string",
  "password": "string"
}
Response: {
  "success": true,
  "data": {
    "token": "string",
    "player": { PlayerProfile }
  }
}
```

#### Login Player
```
POST /api/App/player/login
Body: {
  "username": "string",
  "password": "string"
}
Response: {
  "success": true,
  "data": {
    "token": "string",
    "player": { PlayerProfile }
  }
}
```

#### Get Player Profile [PLAYER]
```
GET /api/App/player/profile
Headers: Authorization: Bearer {token}
Response: {
  "success": true,
  "data": { PlayerProfile }
}
```

#### Update Player Profile [PLAYER]
```
PUT /api/App/player/profile
Headers: Authorization: Bearer {token}
Body: {
  "displayName": "string",
  "metadata": {}
}
Response: {
  "success": true,
  "data": { PlayerProfile }
}
```

#### Deactivate Account [PLAYER]
```
PUT /api/App/player/deactivate
Headers: Authorization: Bearer {token}
Response: {
  "success": true
}
```

### Games

#### Get Available Games [PLAYER]
```
GET /api/App/game/available
Headers: Authorization: Bearer {token}
Query Params: (optional filtering)
Response: {
  "success": true,
  "data": {
    "games": [ GameInfo ],
    "total": number
  }
}
```

### Game Sessions

#### Start Game Session [PLAYER]
```
POST /api/App/gameSession/start
Headers: Authorization: Bearer {token}
Body: {
  "gameId": "string",
  "metadata": {}
}
Response: {
  "success": true,
  "data": { GameSessionData }
}
```

#### Complete Game Session [PLAYER]
```
POST /api/App/gameSession/complete
Headers: Authorization: Bearer {token}
Body: {
  "gameSessionId": "string",
  "score": number,
  "metadata": {}
}
Response: {
  "success": true,
  "data": { GameSessionData }
}
```

#### Get Session Details [PLAYER]
```
GET /api/App/gameSession/{gameSessionId}
Headers: Authorization: Bearer {token}
Response: {
  "success": true,
  "data": { GameSessionData }
}
```

#### Get My Sessions [PLAYER]
```
GET /api/App/gameSession/my-sessions?page=1&pageSize=20
Headers: Authorization: Bearer {token}
Query Params: page, pageSize, (optional filtering)
Response: {
  "success": true,
  "data": {
    "sessions": [ GameSessionData ],
    "total": number,
    "page": number,
    "pageSize": number
  }
}
```

#### Get Active Sessions [PLAYER]
```
GET /api/App/gameSession/my-sessions/active?page=1&pageSize=20
Headers: Authorization: Bearer {token}
Query Params: page, pageSize
Response: {
  "success": true,
  "data": {
    "sessions": [ GameSessionData ],
    "total": number,
    "page": number,
    "pageSize": number
  }
}
```

### Scores

#### Get My Total Score [PLAYER]
```
GET /api/App/score/my-total
Headers: Authorization: Bearer {token}
Response: {
  "success": true,
  "data": {
    "score": number,
    "playerId": "string",
    "gameId": null
  }
}
```

#### Get My Weekly Score [PLAYER]
```
GET /api/App/score/my-weekly
Headers: Authorization: Bearer {token}
Response: {
  "success": true,
  "data": {
    "score": number,
    "playerId": "string",
    "gameId": null
  }
}
```

#### Get Weekly Leaderboard [PLAYER]
```
GET /api/App/score/weekly
Headers: Authorization: Bearer {token}
Response: {
  "success": true,
  "data": {
    "entries": [ LeaderboardEntry ],
    "total": number,
    "page": number,
    "pageSize": number
  }
}
```

#### Get Game Leaderboard [PLAYER]
```
GET /api/App/score/leaderboard/{gameId}
Headers: Authorization: Bearer {token}
Response: {
  "success": true,
  "data": {
    "entries": [ LeaderboardEntry ],
    "total": number,
    "page": number,
    "pageSize": number
  }
}
```

### Products

#### Get Products [PLAYER]
```
GET /api/App/products?page=1&pageSize=20
Headers: Authorization: Bearer {token}
Query Params: page, pageSize, (optional filtering)
Response: {
  "success": true,
  "data": {
    "products": [ Product ],
    "total": number,
    "page": number,
    "pageSize": number
  }
}
```

## Data Models

### PlayerProfile
```csharp
{
  "id": "string",
  "username": "string",
  "email": "string",
  "displayName": "string",
  "totalScore": number,
  "weeklyScore": number,
  "metadata": {}
}
```

### GameInfo
```csharp
{
  "id": "string",
  "name": "string",
  "description": "string",
  "sceneName": "string",  // Unity scene name
  "thumbnailUrl": "string",
  "isActive": boolean,
  "metadata": {}
}
```

### GameSessionData
```csharp
{
  "id": "string",
  "gameId": "string",
  "playerId": "string",
  "status": "active" | "completed" | "abandoned",
  "startTime": "ISO8601 datetime",
  "endTime": "ISO8601 datetime" | null,
  "score": number | null,
  "metadata": {}
}
```

### LeaderboardEntry
```csharp
{
  "playerId": "string",
  "playerName": "string",
  "score": number,
  "rank": number
}
```

### Product
```csharp
{
  "id": "string",
  "name": "string",
  "description": "string",
  "cost": number,
  "currencyType": "string",
  "imageUrl": "string",
  "metadata": {}
}
```

## Error Responses

All endpoints may return errors in this format:

```json
{
  "success": false,
  "message": "Error description",
  "statusCode": 400
}
```

Common status codes:
- `200` - Success
- `400` - Bad Request
- `401` - Unauthorized (invalid/missing token)
- `404` - Not Found
- `500` - Server Error

## Implementation Notes

1. **Authentication**: Token is sent via `Authorization: Bearer {token}` header
2. **Content-Type**: All POST/PUT requests use `application/json`
3. **Pagination**: Use `page` and `pageSize` query parameters (defaults may vary)
4. **Metadata**: Flexible JSON object for additional data
5. **Session Flow**: Must start session before playing, complete after finishing
