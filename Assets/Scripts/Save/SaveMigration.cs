// ----------------------------------------------------------------------------
// SaveMigration.cs
// Defines the migration interface and a sample migration. Each migration
// transforms SaveData from one schema version to the next. The pipeline is
// run on load by SaveManager.
// ----------------------------------------------------------------------------

using System;

namespace Game.BlockPuzzle.Save
{
    public interface ISaveMigration
    {
        int FromVersion { get; }
        SaveData Apply(SaveData data);
    }

    /// <summary>
    /// Initial schema migration — sets defaults for any field that didn't
    /// exist in v1 and may have been added since. Future migrations will
    /// build on this.
    /// </summary>
    public sealed class Migration_1_Initial : ISaveMigration
    {
        public int FromVersion => 1;

        public SaveData Apply(SaveData data)
        {
            if (data == null) return data;
            data.Settings ??= new PlayerSettings();
            data.Wallet ??= Wallet.Default(Game.BlockPuzzle.Config.GameConfig.Instance.Economy);
            data.Statistics ??= new PlayerStatistics();
            data.Progress ??= new ProgressData();
            data.MissionState ??= new System.Collections.Generic.List<MissionSaveEntry>();
            data.DailyChallenges ??= new System.Collections.Generic.List<DailyChallengeSaveEntry>();
            data.Chests ??= new System.Collections.Generic.List<ChestSaveEntry>();
            data.BattlePass ??= new BattlePassSaveEntry();
            data.OwnedCosmetics ??= new System.Collections.Generic.List<string>();
            return data;
        }
    }
}
