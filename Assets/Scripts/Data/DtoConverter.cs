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

            return new PlayerProfile
            {
                id = dto.socialMediaId, // Use socialMediaId as ID
                username = dto.username,
                email = "", // Not in backend DTO
                displayName = dto.username,
                totalScore = dto.score,
                weeklyScore = 0, // Not in PlayerDto, will be fetched separately
                socialMediaId = dto.socialMediaId,
                countryCode = dto.countryCode,
                icon = dto.icon,
                frame = dto.frame,
                lastLoginAt = dto.lastLoginAt,
                isActive = dto.isActive,
                metadata = new Dictionary<string, object>()
            };
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
        public static GameInfo ToGameInfo(GameDto dto, string gameId)
        {
            if (dto == null) return null;

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
                id = gameId,
                name = dto.name,
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
