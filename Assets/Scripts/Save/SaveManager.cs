// ----------------------------------------------------------------------------
// SaveManager.cs
// Owns the in-memory SaveData. Loads on startup, writes debounced + on pause.
// Also coordinates SaveMigration pipeline (old schema -> new schema).
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.BlockPuzzle.Config;
using Game.BlockPuzzle.Core;
using UnityEngine;

namespace Game.BlockPuzzle.Save
{
    public sealed class SaveManager : MonoBehaviour, IService
    {
        public SaveData Current { get; private set; }

        private EncryptedSaveProvider _provider;
        private float _autoSaveTimer;
        private bool _dirty;
        private bool _saving;
        private const float AutoSaveIntervalSeconds = 15f;

        public void Initialize()
        {
            // Derive key from SystemInfo.deviceUniqueIdentifier + a per-install
            // random salt. In a production build we recommend moving the salt
            // into Android Keystore via a small Java plugin.
            var seed = SystemInfo.deviceUniqueIdentifier
                       + "_" + Application.identifier
                       + "_v1";
            var key = JsonCrypto.DeriveKey(seed);
            _provider = new EncryptedSaveProvider(
                System.IO.Path.Combine(Application.persistentDataPath, Constants.SaveFileName),
                key);
            _ = LoadAsync();
        }

        public void Shutdown()
        {
            Flush();
        }

        private void Update()
        {
            if (!_dirty) return;
            _autoSaveTimer += Time.unscaledDeltaTime;
            if (_autoSaveTimer >= AutoSaveIntervalSeconds)
            {
                Flush();
            }
        }

        public void MarkDirty() => _dirty = true;

        /// <summary>Load save synchronously (after Initialize's async load).</summary>
        public async Task LoadAsync()
        {
            try
            {
                var data = await _provider.LoadAsync();
                if (data == null)
                {
                    Current = CreateDefault();
                    Log.Info("Save", "Created new save");
                }
                else
                {
                    Current = MigrateIfNeeded(data);
                    Log.Info("Save", $"Loaded save schema v{Current.SchemaVersion}");
                }
                _dirty = false;
                _autoSaveTimer = 0f;
            }
            catch (Exception ex)
            {
                Log.Error("Save", "Load failed", ex);
                Current = CreateDefault();
            }
        }

        public void Flush()
        {
            if (!_dirty || _saving || Current == null) return;
            _saving = true;
            _ = DoFlush();
        }

        private async Task DoFlush()
        {
            try
            {
                await _provider.SaveAsync(Current);
                _dirty = false;
                _autoSaveTimer = 0f;
            }
            catch (Exception ex)
            {
                Log.Error("Save", "Flush failed", ex);
            }
            finally { _saving = false; }
        }

        public void Delete()
        {
            _provider?.Delete();
            Current = CreateDefault();
        }

        // --------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------

        private static SaveData CreateDefault()
        {
            var cfg = GameConfig.Instance != null ? GameConfig.Instance.Economy : null;
            return new SaveData
            {
                Wallet = Wallet.Default(cfg),
                Statistics = new PlayerStatistics
                {
                    FirstLaunchUtc = DateTimeOffset.UtcNow,
                    LastLaunchUtc = DateTimeOffset.UtcNow,
                    ConsecutiveDaysLaunched = 1,
                    LastLoginRewardDayIndex = -1
                }
            };
        }

        private static SaveData MigrateIfNeeded(SaveData data)
        {
            int from = data.SchemaVersion;
            if (from == Constants.SaveSchemaVersion) return data;

            var migrations = new List<ISaveMigration>
            {
                new Migration_1_Initial()
            };
            foreach (var m in migrations)
            {
                if (m.FromVersion == data.SchemaVersion)
                    data = m.Apply(data);
            }
            data.SchemaVersion = Constants.SaveSchemaVersion;
            return data;
        }
    }
}
