// ----------------------------------------------------------------------------
// JsonCrypto.cs
// JSON + AES-GCM helpers. Produces a self-contained "envelope" file:
//   { "v": 1, "iv": "...", "ct": "...", "tag": "..." }
// ----------------------------------------------------------------------------

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Game.BlockPuzzle.Save
{
    /// <summary>
    /// JSON crypto helpers. Uses AES-GCM where available; falls back to AES-CBC
    /// + HMAC-SHA256 if GCM is not exposed (older Mono).
    /// </summary>
    public static class JsonCrypto
    {
        // 256-bit key. In production this is generated per-install via
        // SecureRandom and stored in Android Keystore via a plugin. For the
        // open-source build we derive from device-id + a static salt and
        // optionally let SaveManager rotate the key.
        private const int KeySizeBytes = 32;
        private const int IvSizeBytes = 12; // for GCM; 16 for CBC
        private const int SaltSizeBytes = 16;

        public static byte[] DeriveKey(string seed)
        {
            using var kdf = new Rfc2898DeriveBytes(seed,
                Encoding.UTF8.GetBytes("blockcraft-v1-salt"),
                100_000, HashAlgorithmName.SHA256);
            return kdf.GetBytes(KeySizeBytes);
        }

        public static byte[] Encrypt(byte[] plaintext, byte[] key, out byte[] iv, out byte[] tag)
        {
            iv = new byte[IvSizeBytes];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(iv);

#if UNITY_2021_2_OR_NEWER && !UNITY_2021_2_0
            try
            {
                using var gcm = new AesGcm(key);
                tag = new byte[16];
                var ct = new byte[plaintext.Length];
                gcm.Encrypt(iv, plaintext, ct, tag);
                return ct;
            }
            catch (PlatformNotSupportedException)
            {
                // fall through to CBC
            }
#endif
            return EncryptCbc(plaintext, key, iv, out tag);
        }

        private static byte[] EncryptCbc(byte[] plaintext, byte[] key, byte[] iv, out byte[] tag)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(plaintext, 0, plaintext.Length);
                cs.FlushFinalBlock();
            }
            using var hmac = new HMACSHA256(key);
            tag = hmac.ComputeHash(ms.ToArray());
            return ms.ToArray();
        }

        public static byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] iv, byte[] tag)
        {
#if UNITY_2021_2_OR_NEWER && !UNITY_2021_2_0
            try
            {
                using var gcm = new AesGcm(key);
                var pt = new byte[ciphertext.Length];
                gcm.Decrypt(iv, ciphertext, tag, pt);
                return pt;
            }
            catch (PlatformNotSupportedException) { }
            catch (CryptographicException) { /* try CBC */ }
#endif
            return DecryptCbc(ciphertext, key, iv, tag);
        }

        private static byte[] DecryptCbc(byte[] ciphertext, byte[] key, byte[] iv, byte[] tag)
        {
            using var hmac = new HMACSHA256(key);
            var expected = hmac.ComputeHash(ciphertext);
            if (!FixedTimeEquals(expected, tag))
                throw new CryptographicException("HMAC mismatch — save tampered");

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            using var ms = new MemoryStream(ciphertext);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var outMs = new MemoryStream();
            cs.CopyTo(outMs);
            return outMs.ToArray();
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }

        public static string Base64Encode(byte[] bytes) =>
            Convert.ToBase64String(bytes);

        public static byte[] Base64Decode(string text) =>
            Convert.FromBase64String(text);
    }
}
