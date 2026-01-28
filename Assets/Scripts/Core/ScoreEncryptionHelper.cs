using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Minigames.Core
{
    /// <summary>
    /// Helper for encrypting scores before sending to backend.
    /// Matches the backend EncryptionService implementation using AES-256-CBC encryption.
    /// Backend requires EncryptedScore (string, max 500 chars) instead of plain score.
    /// </summary>
    public static class ScoreEncryptionHelper
    {
        private const int KeySize = 256; // AES-256
        private const int BlockSize = 128; // AES block size
        private const int Pbkdf2Iterations = 10000;
        private const string Pbkdf2Salt = "MiniGames2_Salt_2024";
        
        // Encryption key - should be set via SetEncryptionKey() or will use default (for development)
        // In production, this should be passed from the host app via WebViewBridge
        private static string _encryptionKey = null;
        private static readonly string DefaultEncryptionKey = "YourSuperSecretKeyThatIsAtLeast32CharactersLong!@#$%^&*()_+"; // 32+ chars for AES-256

        /// <summary>
        /// Set the encryption key (should be called from WebViewBridge or AppInitializer).
        /// Key must be at least 32 characters long for AES-256 encryption.
        /// </summary>
        public static void SetEncryptionKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning("ScoreEncryptionHelper: Encryption key is null or empty. Using default key.");
                _encryptionKey = DefaultEncryptionKey;
                return;
            }

            if (key.Length < 32)
            {
                Debug.LogError($"ScoreEncryptionHelper: Encryption key must be at least 32 characters long. Provided key length: {key.Length}");
                throw new ArgumentException("Encryption key must be at least 32 characters long for AES-256 encryption");
            }

            _encryptionKey = key;
            Debug.Log("ScoreEncryptionHelper: Encryption key set successfully");
        }

        /// <summary>
        /// Get the current encryption key (uses default if not set).
        /// </summary>
        private static string GetEncryptionKey()
        {
            if (string.IsNullOrEmpty(_encryptionKey))
            {
                Debug.LogWarning("ScoreEncryptionHelper: Encryption key not set. Using default key. Set encryption key via SetEncryptionKey() for production.");
                return DefaultEncryptionKey;
            }
            return _encryptionKey;
        }

        /// <summary>
        /// Encrypt score for backend submission.
        /// Matches backend EncryptionService.EncryptScore() implementation.
        /// Format: Encrypts "{score}|{timestamp}" where timestamp is Unix seconds.
        /// </summary>
        public static string EncryptScore(int score)
        {
            try
            {
                // Add timestamp to make each encryption unique and prevent replay attacks
                // Matches backend: var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string plainText = $"{score}|{timestamp}";

                return Encrypt(plainText);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ScoreEncryptionHelper: Error encrypting score: {ex.Message}");
                throw new InvalidOperationException("Encryption failed", ex);
            }
        }

        /// <summary>
        /// Decrypt score from backend (for validation/testing purposes).
        /// Matches backend EncryptionService.DecryptScore() implementation.
        /// </summary>
        public static int DecryptScore(string encryptedScore)
        {
            try
            {
                string decrypted = Decrypt(encryptedScore);
                string[] parts = decrypted.Split('|');

                if (parts.Length != 2)
                {
                    Debug.LogWarning("ScoreEncryptionHelper: Invalid encrypted score format");
                    throw new InvalidOperationException("Invalid encrypted score format");
                }

                if (!int.TryParse(parts[0], out int score))
                {
                    Debug.LogWarning("ScoreEncryptionHelper: Unable to parse score from encrypted data");
                    throw new InvalidOperationException("Invalid score value in encrypted data");
                }

                if (!long.TryParse(parts[1], out long timestamp))
                {
                    Debug.LogWarning("ScoreEncryptionHelper: Unable to parse timestamp from encrypted data");
                    throw new InvalidOperationException("Invalid timestamp in encrypted data");
                }

                // Validate timestamp is not too old (prevent replay attacks)
                // Matches backend: Allow scores from up to 24 hours ago
                long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                long ageInSeconds = currentTimestamp - timestamp;

                // Allow scores from up to 24 hours ago or 5 minutes in future (clock skew)
                if (ageInSeconds > 86400 || ageInSeconds < -300)
                {
                    Debug.LogWarning($"ScoreEncryptionHelper: Encrypted score timestamp is invalid. Age: {ageInSeconds} seconds");
                    throw new InvalidOperationException("Encrypted score has expired or has invalid timestamp");
                }

                return score;
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                Debug.LogError($"ScoreEncryptionHelper: Error decrypting score: {ex.Message}");
                throw new InvalidOperationException("Failed to decrypt score. The score may be corrupted or tampered with.", ex);
            }
        }

        /// <summary>
        /// Encrypt plain text using AES-256-CBC encryption.
        /// Matches backend EncryptionService.Encrypt() implementation.
        /// </summary>
        private static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.BlockSize = BlockSize;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Derive key from the encryption key string using PBKDF2
                byte[] key = DeriveKey(GetEncryptionKey());
                aes.Key = key;

                // Generate a random IV for each encryption
                aes.GenerateIV();
                byte[] iv = aes.IV;

                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (System.IO.MemoryStream msEncrypt = new System.IO.MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (System.IO.StreamWriter swEncrypt = new System.IO.StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }

                    byte[] encrypted = msEncrypt.ToArray();

                    // Combine IV and encrypted data
                    byte[] result = new byte[iv.Length + encrypted.Length];
                    Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                    Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);

                    return Convert.ToBase64String(result);
                }
            }
        }

        /// <summary>
        /// Decrypt encrypted text using AES-256-CBC decryption.
        /// Matches backend EncryptionService.Decrypt() implementation.
        /// </summary>
        private static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                throw new ArgumentNullException(nameof(encryptedText));

            byte[] fullCipher = Convert.FromBase64String(encryptedText);

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.BlockSize = BlockSize;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Derive key from the encryption key string using PBKDF2
                byte[] key = DeriveKey(GetEncryptionKey());
                aes.Key = key;

                // Extract IV from the encrypted data
                byte[] iv = new byte[aes.IV.Length];
                byte[] cipher = new byte[fullCipher.Length - iv.Length];

                Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                aes.IV = iv;

                using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (System.IO.MemoryStream msDecrypt = new System.IO.MemoryStream(cipher))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (System.IO.StreamReader srDecrypt = new System.IO.StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Derives a 256-bit key from the encryption key string using PBKDF2.
        /// Matches backend EncryptionService.DeriveKey() implementation.
        /// </summary>
        private static byte[] DeriveKey(string password)
        {
            byte[] salt = Encoding.UTF8.GetBytes(Pbkdf2Salt);

            using (Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256))
            {
                return deriveBytes.GetBytes(32); // 256 bits = 32 bytes
            }
        }

        /// <summary>
        /// Validate encrypted score format (for testing/debugging).
        /// </summary>
        public static bool ValidateEncryptedScore(string encryptedScore)
        {
            try
            {
                DecryptScore(encryptedScore);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
