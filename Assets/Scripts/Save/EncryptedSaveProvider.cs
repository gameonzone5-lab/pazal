// ----------------------------------------------------------------------------
// EncryptedSaveProvider.cs
// Reads / writes a SaveData as JSON to disk, encrypted with AES-GCM via
// JsonCrypto. Maintains a single rolling backup file in case the primary
// file is corrupted (e.g. crash mid-write).
// ----------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.BlockPuzzle.Save
{
    [Serializable]
    internal sealed class SaveEnvelope
    {
        public int v;
        public string iv;
        public string ct;
        public string tag;
        public int schema;
    }

    public sealed class EncryptedSaveProvider
    {
        private readonly string _filePath;
        private readonly string _backupPath;
        private readonly byte[] _key;

        public EncryptedSaveProvider(string filePath, byte[] key)
        {
            _filePath = filePath;
            _backupPath = filePath + ".bak";
            _key = key;
        }

        public async Task<SaveData> LoadAsync()
        {
            string json;
            try
            {
                if (File.Exists(_filePath)) json = await ReadAllTextAsync(_filePath);
                else if (File.Exists(_backupPath)) json = await ReadAllTextAsync(_backupPath);
                else return null;
            }
            catch (Exception ex)
            {
                Log.Error("Save", "Failed to read save file", ex);
                return null;
            }

            try
            {
                var env = JsonUtility.FromJson<SaveEnvelope>(json);
                if (env == null) return null;
                var iv = JsonCrypto.Base64Decode(env.iv);
                var ct = JsonCrypto.Base64Decode(env.ct);
                var tag = JsonCrypto.Base64Decode(env.tag);
                var plain = JsonCrypto.Decrypt(ct, _key, iv, tag);
                var jsonText = System.Text.Encoding.UTF8.GetString(plain);
                return JsonUtility.FromJson<SaveData>(jsonText);
            }
            catch (Exception ex)
            {
                Log.Error("Save", "Save decrypt failed; trying backup", ex);
                try
                {
                    var bjson = await ReadAllTextAsync(_backupPath);
                    var env = JsonUtility.FromJson<SaveEnvelope>(bjson);
                    if (env == null) return null;
                    var iv = JsonCrypto.Base64Decode(env.iv);
                    var ct = JsonCrypto.Base64Decode(env.ct);
                    var tag = JsonCrypto.Base64Decode(env.tag);
                    var plain = JsonCrypto.Decrypt(ct, _key, iv, tag);
                    var jsonText = System.Text.Encoding.UTF8.GetString(plain);
                    return JsonUtility.FromJson<SaveData>(jsonText);
                }
                catch (Exception ex2)
                {
                    Log.Error("Save", "Backup also failed", ex2);
                    return null;
                }
            }
        }

        public async Task SaveAsync(SaveData data)
        {
            if (data == null) return;
            data.UpdatedAtIso = DateTimeOffset.UtcNow.ToString("o");
            var plainText = JsonUtility.ToJson(data);
            var plain = System.Text.Encoding.UTF8.GetBytes(plainText);

            var ct = JsonCrypto.Encrypt(plain, _key, out var iv, out var tag);
            var env = new SaveEnvelope
            {
                v = 1,
                schema = data.SchemaVersion,
                iv = JsonCrypto.Base64Encode(iv),
                ct = JsonCrypto.Base64Encode(ct),
                tag = JsonCrypto.Base64Encode(tag)
            };
            var envJson = JsonUtility.ToJson(env);

            // Atomic write: copy existing -> backup, then write fresh to file.
            try
            {
                if (File.Exists(_filePath))
                    File.Copy(_filePath, _backupPath, overwrite: true);
                await WriteAllTextAsync(_filePath, envJson);
            }
            catch (Exception ex)
            {
                Log.Error("Save", "Save write failed", ex);
            }
        }

        public void Delete()
        {
            try { if (File.Exists(_filePath)) File.Delete(_filePath); } catch { }
            try { if (File.Exists(_backupPath)) File.Delete(_backupPath); } catch { }
        }

        private static async Task<string> ReadAllTextAsync(string path)
        {
            using var sr = new StreamReader(path);
            return await sr.ReadToEndAsync();
        }

        private static async Task WriteAllTextAsync(string path, string contents)
        {
            using var sw = new StreamWriter(path, false);
            await sw.WriteAsync(contents);
        }
    }
}
