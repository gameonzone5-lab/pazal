// ----------------------------------------------------------------------------
// SaveData.cs
// Top-level save game data. Serialized as JSON, then encrypted.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Game.BlockPuzzle.Economy;
using UnityEngine;

namespace Game.BlockPuzzle.Save
{
    [Serializable]
    public sealed class SaveData
    {
        public int SchemaVersion = Constants.SaveSchemaVersion;
        public string CreatedAtIso;
        public string UpdatedAtIso;

        public PlayerSettings Settings = new();
        public Wallet Wallet = new();
        public PlayerStatistics Statistics = new();
        public ProgressData Progress = new();
        public List<MissionSaveEntry> MissionState = new();
        public List<DailyChallengeSaveEntry> DailyChallenges = new();
        public List<ChestSaveEntry> Chests = new();
        public BattlePassSaveEntry BattlePass = new();
        public List<string> OwnedCosmetics = new();
        public bool RemoveAdsOwned;
        public bool PremiumActive;
        public DateTimeOffset PremiumUntilUtc;
        public string LocaleCode = "en";

        // Convenience ctor sets timestamps.
        public SaveData()
        {
            var now = DateTimeOffset.UtcNow.ToString("o");
            CreatedAtIso = now;
            UpdatedAtIso = now;
        }
    }

    [Serializable]
    public sealed class PlayerSettings
    {
        public ThemeMode Theme = ThemeMode.Dark;
        public ColorBlindMode ColorBlind = ColorBlindMode.None;
        public float MasterVolume = 1f;
        public float MusicVolume = 0.7f;
        public float SfxVolume = 1f;
        public float UiVolume = 1f;
        public bool HapticsEnabled = true;
        public bool ShowGhostPiece = true;
        public bool ParticleQualityHigh = true;
        public string LanguageCode = "en";
        public bool PrivacyAnalytics = true;
        public bool PersonalizedAds = false;
    }

    [Serializable]
    public sealed class PlayerStatistics
    {
        public int TotalRuns;
        public long TotalScore;
        public long TotalCoinsEarned;
        public long TotalGemsEarned;
        public long TotalCellsCleared;
        public long TotalLinesCleared;
        public long TotalCombos;
        public long TotalBombsUsed;
        public long TotalRainbowsUsed;
        public int BestCombo;
        public int BestChain;
        public int BestScoreEndless;
        public int BestScoreTimed3;
        public int BestScoreTimed5;
        public int BestScoreDaily;
        public int LevelsCompleted;
        public DateTimeOffset FirstLaunchUtc;
        public DateTimeOffset LastLaunchUtc;
        public int ConsecutiveDaysLaunched;
        public DateTimeOffset LastLoginRewardUtc;
        public int LastLoginRewardDayIndex;
    }

    [Serializable]
    public sealed class ProgressData
    {
        public int AdventureLevel = 1;
        public List<int> CompletedLevels = new();
        public int CurrentWorldIndex;
    }

    [Serializable]
    public sealed class MissionSaveEntry
    {
        public string MissionId;
        public int Progress;
        public bool Claimed;
        public DateTimeOffset AssignedUtc;
    }

    [Serializable]
    public sealed class DailyChallengeSaveEntry
    {
        public string ChallengeId;
        public int Score;
        public DateTimeOffset PlayedUtc;
    }

    [Serializable]
    public sealed class ChestSaveEntry
    {
        public string ChestId;
        public DateTimeOffset UnlockUtc;
        public bool Claimed;
    }

    [Serializable]
    public sealed class BattlePassSaveEntry
    {
        public string SeasonId;
        public int Xp;
        public int Tier;
        public bool Premium;
        public List<int> ClaimedFreeTiers = new();
        public List<int> ClaimedPremiumTiers = new();
    }

    // Colorblind mode is referenced from ThemeConfig but lives here to
    // avoid circular namespace. We re-export the enum.
    public enum ColorBlindMode { None, Deuteranopia, Protanopia, Tritanopia, HighContrast }
}
