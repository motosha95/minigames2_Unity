using System;
using System.Collections.Generic;

namespace Minigames.Data
{
    // API Response Models
    [Serializable]
    public class ApiResponse<T>
    {
        public bool success;
        public T data;
        public string message;
        public int statusCode;
    }

    // Authentication Models
    [Serializable]
    public class RegisterTenantPlayerDto
    {
        public string username;        // required, 3-100 chars
        public string socialMediaId;   // required
        public string countryCode;     // optional, max 10 chars
    }

    [Serializable]
    public class LoginTenantPlayerDto
    {
        public string socialMediaId;   // required
    }

    [Serializable]
    public class TenantPlayerAuthResultDto
    {
        public bool isSuccess;
        public string token;
        public PlayerDto player;
        public string errorMessage;
    }

    // Player Models
    [Serializable]
    public class PlayerDto
    {
        public string id;              // Guid
        public string username;
        public string socialMediaId;
        public string countryCode;
        public string icon;
        public string frame;
        public int score;
        public int availableKeys;     // Keys balance from backend (availableKeys)
        public DateTime? lastLoginAt;
        public bool isActive;
    }

    [Serializable]
    public class UpdatePlayerProfileDto
    {
        public string username;       // optional, 3-100 chars
        public string countryCode;     // optional, max 10 chars
        public string icon;            // optional, max 64 chars
        public string frame;            // optional, max 64 chars
    }

    [Serializable]
    public class PlayerProfileUpdateRequest
    {
        public string displayName;     // Maps to username
        public string username;        // Maps to UpdatePlayerProfileDto.username
        public string countryCode;     // Maps to UpdatePlayerProfileDto.countryCode
        public string icon;            // Maps to UpdatePlayerProfileDto.icon
        public string frame;           // Maps to UpdatePlayerProfileDto.frame
        public Dictionary<string, object> metadata; // Not in backend DTO
    }

    // Legacy/Compatibility Models (for backward compatibility with existing code)
    [Serializable]
    public class PlayerProfile
    {
        public string id;
        public string username;
        public string email;           // Not in backend, kept for compatibility
        public string displayName;     // Maps to username
        public int totalScore;
        public int weeklyScore;
        public int keysBalance;        // Keys balance
        public Dictionary<string, object> metadata;
        
        // Additional fields from PlayerDto
        public string socialMediaId;
        public string countryCode;
        public string icon;
        public string frame;
        public DateTime? lastLoginAt;
        public bool isActive;
    }

    [Serializable]
    public class PlayerLoginRequest
    {
        /// <summary>Social media ID for login (matches LoginTenantPlayerDto).</summary>
        public string socialMediaId;
        
        // Legacy fields (kept for compatibility but not used by backend)
        public string username;
        public string email;
        public string password;
    }

    [Serializable]
    public class PlayerRegisterRequest
    {
        public string username;        // required, 3-100 chars
        public string socialMediaId;   // required
        public string countryCode;     // optional
        
        // Legacy fields (kept for compatibility but not used by backend)
        public string email;
        public string password;
    }

    [Serializable]
    public class PlayerLoginResponse
    {
        public string token;
        public PlayerProfile player;
    }

    // OTP Models (may not be used if backend doesn't support OTP)
    [Serializable]
    public class RequestRegistrationOtpRequest
    {
        public string email;
        public string username;
    }

    [Serializable]
    public class VerifyOtpAndRegisterRequest
    {
        public string email;
        public string username;
        public string password;
        public string otp;
    }

    [Serializable]
    public class SendOtpResponse
    {
        // Empty; backend returns { "success": true, "data": {} } or similar
    }

    // Game Models
    [Serializable]
    public class GameDto
    {
        public string id;                 // Guid as string - game ID from backend
        public string name;               // Game name (display name)
        public string description;
        public int maxScore;
        public int timeLimit;
        public bool isActive;
        public string rules;
        public List<GameConfiguration> gameConfigurations;
    }

    [Serializable]
    public class GameConfiguration
    {
        public string key;
        public string value;
    }

    [Serializable]
    public class GameInfo
    {
        public string id;
        public string name;
        public string description;
        public string sceneName;       // Not in backend, used for Unity scene loading
        public string addressableKey;  // Not in backend, Addressables key (e.g. "MiniGame_Basketball"). If null, derived as "MiniGame_" + sceneName
        public string thumbnailUrl;    // Not in backend, optional UI field
        public bool isActive;
        public int maxScore;
        public int timeLimit;
        public string rules;
        public Dictionary<string, object> metadata;
    }

    [Serializable]
    public class PagedResult<T>
    {
        public List<T> items;
        public int total;
        public int page;
        public int pageSize;
    }

    [Serializable]
    public class GameListResponse
    {
        public List<GameInfo> games;
        public int total;
    }

    [Serializable]
    public class GameDtoListResponse
    {
        public List<GameDto> items;  // Backend returns PagedResult<GameDto>
        public int total;
        public int page;
        public int pageSize;
    }

    // Game Session Models
    [Serializable]
    public class StartGameDto
    {
        public string gameId;          // Guid as string
        public string gameData;        // optional
    }

    [Serializable]
    public class GameSessionStartRequest
    {
        public string gameId;
        public string gameData;        // Changed from Dictionary to string
        public Dictionary<string, object> metadata; // Legacy, kept for compatibility
    }

    [Serializable]
    public class CompleteGameDto
    {
        public string gameSessionId;   // Guid as string
        public string encryptedScore;   // required, max 500 chars (encrypted score)
        public string gameData;        // optional
    }

    [Serializable]
    public class GameSessionCompleteRequest
    {
        public string gameSessionId;
        public string encryptedScore;   // Changed from int score to string encryptedScore
        public string gameData;         // Changed from Dictionary to string
        public Dictionary<string, object> metadata; // Legacy, kept for compatibility
        
        // Legacy field (kept for compatibility)
        public int score;
    }

    [Serializable]
    public class GameSessionDto
    {
        public string id;               // Session ID (Guid)
        public string playerId;         // Guid
        public string gameId;           // Guid
        public string gameName;
        public string status;           // GameStatus enum as string
        public DateTime startedAt;
        public DateTime? endedAt;
        public int finalScore;
        public TimeSpan? duration;
        public string gameData;
        public string rejectionReason;
    }

    [Serializable]
    public class GameSessionData
    {
        public string id;
        public string gameId;
        public string playerId;
        public string gameName;
        public string status;           // "active", "completed", "abandoned"
        public DateTime startTime;      // Maps to startedAt
        public DateTime? endTime;       // Maps to endedAt
        public int? score;             // Maps to finalScore
        public TimeSpan? duration;
        public string gameData;
        public string rejectionReason;
        public Dictionary<string, object> metadata; // Legacy
    }

    [Serializable]
    public class GameSessionListResponse
    {
        public List<GameSessionData> sessions;
        public int total;
        public int page;
        public int pageSize;
    }

    // Score Models
    [Serializable]
    public class PlayerScoreDto
    {
        public string playerId;         // Guid
        public string playerName;
        public int totalScore;
        public int gamesPlayed;
        public DateTime lastGamePlayed;
    }

    [Serializable]
    public class WeeklyScoreDto
    {
        public string playerId;         // Guid
        public string playerName;
        public int weeklyScore;
        public int gamesPlayedThisWeek;
        public DateTime weekStartDate;
        public DateTime weekEndDate;
    }

    [Serializable]
    public class GameScoreDto
    {
        public string playerId;         // Guid
        public string playerName;
        public string gameId;           // Guid
        public string gameName;
        public int totalScore;
        public int gamesPlayed;
        public int bestScore;
    }

    [Serializable]
    public class ScoreResponse
    {
        public int score;
        public string playerId;
        public string gameId;
    }

    [Serializable]
    public class LeaderboardEntry
    {
        public string playerId;
        public string playerName;
        public int score;
        public int rank;
    }

    [Serializable]
    public class LeaderboardResponse
    {
        public List<LeaderboardEntry> entries;
        public int total;
        public int page;
        public int pageSize;
    }

    // Product Models
    [Serializable]
    public class ProductDto
    {
        // Structure not fully specified in DTOs.txt, keeping existing structure
        public string id;
        public string name;
        public string description;
        public int cost;
        public string currencyType;
        public string imageUrl;
        public Dictionary<string, object> metadata;
    }

    [Serializable]
    public class Product
    {
        public string id;
        public string name;
        public string description;
        public int cost;
        public string currencyType;
        public string imageUrl;
        public Dictionary<string, object> metadata;
    }

    [Serializable]
    public class ProductListResponse
    {
        public List<Product> products;
        public int total;
        public int page;
        public int pageSize;
    }

    // Filter Parameters
    [Serializable]
    public class FilterParameters
    {
        public int page = 1;
        public int pageSize = 20;
    }

    [Serializable]
    public class GameFilterParameters : FilterParameters
    {
        // Inherits pagination from FilterParameters
    }

    [Serializable]
    public class GameSessionFilterParameters : FilterParameters
    {
        public int? minScore;
        public int? maxScore;
        public DateTime? startedFrom;
        public DateTime? startedTo;
        public DateTime? endedFrom;
        public DateTime? endedTo;
        public int? minDuration;
        public int? maxDuration;
        public string gameId;
        public string playerId;
        public string status;
    }

    [Serializable]
    public class ProductFilterParameters : FilterParameters
    {
        // Inherits pagination from FilterParameters
    }
}
