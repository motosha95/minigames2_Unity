using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Minigames.Core
{
    /// <summary>
    /// Helper for encrypting scores before sending to backend.
    /// Backend requires EncryptedScore (string, max 500 chars) instead of plain score.
    /// 
    /// NOTE: This is a placeholder implementation. Replace with your actual encryption method
    /// that matches the backend's decryption logic.
    /// </summary>
    public static class ScoreEncryptionHelper
    {
        /// <summary>
        /// Encrypt score for backend submission.
        /// Currently returns base64-encoded score as placeholder.
        /// Replace this with your actual encryption method.
        /// </summary>
        public static string EncryptScore(int score)
        {
            // TODO: Implement actual encryption that matches backend
            // For now, using simple base64 encoding as placeholder
            // Backend expects encrypted string, max 500 chars
            
            string scoreString = score.ToString();
            byte[] scoreBytes = Encoding.UTF8.GetBytes(scoreString);
            string encrypted = Convert.ToBase64String(scoreBytes);
            
            Debug.LogWarning($"ScoreEncryptionHelper: Using placeholder encryption. Score {score} -> {encrypted}");
            Debug.LogWarning("TODO: Replace with actual encryption method that matches backend decryption.");
            
            return encrypted;
        }

        /// <summary>
        /// Decrypt score from backend (if needed for display).
        /// Currently returns decoded score from base64.
        /// Replace this with your actual decryption method.
        /// </summary>
        public static int DecryptScore(string encryptedScore)
        {
            // TODO: Implement actual decryption that matches backend
            try
            {
                byte[] scoreBytes = Convert.FromBase64String(encryptedScore);
                string scoreString = Encoding.UTF8.GetString(scoreBytes);
                return int.Parse(scoreString);
            }
            catch
            {
                Debug.LogError($"ScoreEncryptionHelper: Failed to decrypt score: {encryptedScore}");
                return 0;
            }
        }
    }
}
