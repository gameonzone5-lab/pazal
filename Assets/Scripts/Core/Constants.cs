// ----------------------------------------------------------------------------
// Constants.cs
// Project-wide compile-time constants. Anything that is a tunable parameter
// that designers might want to tweak without code changes lives in
// GameConfig assets instead; constants here are truly immutable.
// ----------------------------------------------------------------------------

namespace Game.BlockPuzzle.Core
{
    /// <summary>
    /// Global, immutable constants used across the project.
    /// </summary>
    public static class Constants
    {
        // --------------------------------------------------------------------
        // Application identity
        // --------------------------------------------------------------------
        public const string GameName = "BlockCraft: Cube Master";
        public const string BundleId = "com.blockcraft.cubemaster";
        public const string SaveVersion = "1.0.0";
        public const int SaveSchemaVersion = 1;

        // --------------------------------------------------------------------
        // Gameplay
        // --------------------------------------------------------------------
        public const int DefaultBoardSize = 10;
        public const int MinBoardSize = 6;
        public const int MaxBoardSize = 12;
        public const int PieceSlots = 3;
        public const float CellClearDelay = 0.06f;   // s per cell when clearing
        public const float LineClearStagger = 0.04f;  // s between rows/columns

        // Combo thresholds (lines cleared in a single placement)
        public const int ComboSingle = 1;
        public const int ComboDouble = 2;
        public const int ComboTriple = 3;
        public const int ComboQuadruple = 4;

        // Scoring
        public const int BasePointsPerCell = 1;
        public const int LineClearBonus = 10;
        public const int ComboMultiplier2 = 2;
        public const int ComboMultiplier3 = 3;
        public const int ComboMultiplier4 = 5;
        public const int PerfectClearBonus = 50;
        public const int ChainBonusPerStep = 25;

        // --------------------------------------------------------------------
        // Modes
        // --------------------------------------------------------------------
        public const float TimedModeSeconds3Min = 180f;
        public const float TimedModeSeconds5Min = 300f;
        public const int AdventureModeLevelsPerWorld = 20;
        public const int AdventureModeStartingLives = 5;

        // --------------------------------------------------------------------
        // Economy
        // --------------------------------------------------------------------
        public const string CurrencyCoin = "coin";
        public const string CurrencyGem = "gem";
        public const int CoinPerCell = 1;
        public const int CoinPerLine = 5;
        public const int CoinPerCombo = 10;
        public const int GemPer50Coins = 1;

        public const int StartingCoins = 250;
        public const int StartingGems = 25;

        // --------------------------------------------------------------------
        // Save
        // --------------------------------------------------------------------
        public const string SaveFileName = "save.dat";
        public const string SaveBackupFileName = "save.bak";
        public const string CloudSaveCollection = "users";

        // --------------------------------------------------------------------
        // Analytics
        // --------------------------------------------------------------------
        public const string AnalyticsEventLevelStart = "level_start";
        public const string AnalyticsEventLevelEnd = "level_end";
        public const string AnalyticsEventPiecePlaced = "piece_placed";
        public const string AnalyticsEventLineCleared = "line_cleared";
        public const string AnalyticsEventSpecialTriggered = "special_triggered";
        public const string AnalyticsEventIAPPurchase = "iap_purchase";
        public const string AnalyticsEventAdWatched = "ad_watched";
        public const string AnalyticsEventRewardClaimed = "reward_claimed";

        // --------------------------------------------------------------------
        // Limits & budgets
        // --------------------------------------------------------------------
        public const int MaxConcurrentPieces = 3;
        public const int MaxCells = MaxBoardSize * MaxBoardSize;
        public const int MaxRewardedAdsPerDay = 20;
        public const float LowRamDeviceThresholdMB = 2048;
    }
}
