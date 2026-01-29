using System;
using System.Collections.Generic;

namespace Minigames.Data
{
    /// <summary>
    /// Helper class to convert between backend DTOs and Unity models.
    /// Provides compatibility between backend structure and Unity app structure.
    /// </summary>
    public static class DtoConverter
    {
        /// <summary>
        /// Convert PlayerDto to PlayerProfile for compatibility with existing code
        /// </summary>
        public static PlayerProfile ToPlayerProfile(PlayerDto dto)
        {
            if (dto == null) return null;

            var profile = new PlayerProfile
            {
                id = dto.id ?? dto.socialMediaId, // Use id (Guid) if available, fallback to socialMediaId
                username = dto.username,
                email = "", // Not in backend DTO
                displayName = dto.username,
                totalScore = dto.score,
                weeklyScore = 0, // Not in PlayerDto, will be fetched separately
                keysBalance = dto.availableKeys, // Map availableKeys to keysBalance
                socialMediaId = dto.socialMediaId,
                countryCode = dto.countryCode,
                icon = dto.icon,
                frame = dto.frame,
                lastLoginAt = dto.lastLoginAt,
                isActive = dto.isActive,
                metadata = new Dictionary<string, object>()
            };

            UnityEngine.Debug.Log($"DtoConverter: Converted PlayerDto - availableKeys: {dto.availableKeys}, keysBalance: {profile.keysBalance}, username: {profile.username}");
            
            return profile;
        }

        /// <summary>
        /// Convert TenantPlayerAuthResultDto to PlayerLoginResponse for compatibility
        /// </summary>
        public static PlayerLoginResponse ToPlayerLoginResponse(TenantPlayerAuthResultDto authResult)
        {
            if (authResult == null) return null;

            return new PlayerLoginResponse
            {
                token = authResult.token,
                player = ToPlayerProfile(authResult.player)
            };
        }

        /// <summary>
        /// Convert GameDto to GameInfo for Unity scene loading
        /// </summary>
        public static GameInfo ToGameInfo(GameDto dto)
        {
            if (dto == null) return null;

            // Extract gameId - prefer id field, then check gameConfigurations, fallback to name (should not happen)
            string gameId = dto.id;
            if (string.IsNullOrEmpty(gameId) && dto.gameConfigurations != null)
            {
                var idConfig = dto.gameConfigurations.Find(gc => gc.key == "gameId" || gc.key == "GameId" || gc.key == "id" || gc.key == "Id");
                if (idConfig != null)
                {
                    gameId = idConfig.value;
                }
            }
            
            // If still no gameId, use name as fallback (should log warning)
            if (string.IsNullOrEmpty(gameId))
            {
                UnityEngine.Debug.LogWarning($"DtoConverter: GameDto for '{dto.name}' has no id field or gameId in configurations. Using name as fallback.");
                gameId = dto.name;
            }

            // Extract sceneName from gameConfigurations if available
            string sceneName = dto.name; // Default to game name
            if (dto.gameConfigurations != null)
            {
                var sceneConfig = dto.gameConfigurations.Find(gc => gc.key == "sceneName" || gc.key == "SceneName");
                if (sceneConfig != null)
                {
                    sceneName = sceneConfig.value;
                }
            }

            return new GameInfo
            {
                id = gameId,              // Use actual gameId (Guid), not game name
                name = dto.name,           // Game name (display name)
                description = dto.description,
                sceneName = sceneName,
                thumbnailUrl = "", // Extract from gameConfigurations if needed
                isActive = dto.isActive,
                maxScore = dto.maxScore,
                timeLimit = dto.timeLimit,
                rules = dto.rules,
                metadata = new Dictionary<string, object>()
            };
        }

        /// <summary>
        /// Convert GameSessionDto to GameSessionData
        /// </summary>
        public static GameSessionData ToGameSessionData(GameSessionDto dto, string sessionId = null)
        {
            if (dto == null) return null;

            return new GameSessionData
            {
                id = sessionId ?? dto.id ?? (dto.playerId + "_" + dto.gameId),
                gameId = dto.gameId,
                playerId = dto.playerId,
                gameName = dto.gameName,
                status = dto.status,
                startTime = dto.startedAt,
                endTime = dto.endedAt,
                score = dto.finalScore,
                duration = dto.duration,
                gameData = dto.gameData,
                rejectionReason = dto.rejectionReason,
                metadata = new Dictionary<string, object>()
            };
        }
    }
}
