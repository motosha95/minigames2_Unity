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

    // Player Models
    [Serializable]
    public class PlayerRegisterRequest
    {
        public string username;
        public string email;
        public string password;
    }

    [Serializable]
    public class PlayerLoginRequest
    {
        public string username;
        public string password;
    }

    [Serializable]
    public class PlayerLoginResponse
    {
        public string token;
        public PlayerProfile player;
    }

    [Serializable]
    public class PlayerProfile
    {
        public string id;
        public string username;
        public string email;
        public string displayName;
        public int totalScore;
        public int weeklyScore;
        public Dictionary<string, object> metadata;
    }

    [Serializable]
    public class PlayerProfileUpdateRequest
    {
        public string displayName;
        public Dictionary<string, object> metadata;
    }

    // Game Models
    [Serializable]
    public class GameInfo
    {
        public string id;
        public string name;
        public string description;
        public string sceneName;
        public string thumbnailUrl;
        public bool isActive;
        public Dictionary<string, object> metadata;
    }

    [Serializable]
    public class GameListResponse
    {
        public List<GameInfo> games;
        public int total;
    }

    // Game Session Models
    [Serializable]
    public class GameSessionStartRequest
    {
        public string gameId;
        public Dictionary<string, object> metadata;
    }

    [Serializable]
    public class GameSessionData
    {
        public string id;
        public string gameId;
        public string playerId;
        public string status; // "active", "completed", "abandoned"
        public DateTime startTime;
        public DateTime? endTime;
        public int? score;
        public Dictionary<string, object> metadata;
    }

    [Serializable]
    public class GameSessionCompleteRequest
    {
        public string gameSessionId;
        public int score;
        public Dictionary<string, object> metadata;
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
}
