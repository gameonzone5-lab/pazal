// ----------------------------------------------------------------------------
// EconomyConfig.cs
// Tuning for currency rewards, gem-to-coin conversion, ad multipliers.
// ----------------------------------------------------------------------------

using UnityEngine;

namespace Game.BlockPuzzle.Config
{
    [CreateAssetMenu(menuName = "BlockCraft/Economy Config", fileName = "EconomyConfig")]
    public sealed class EconomyConfig : ScriptableObject
    {
        [Header("Starting balances")]
        public long StartingCoins = Constants.StartingCoins;
        public long StartingGems = Constants.StartingGems;

        [Header("In-game rewards")]
        public int CoinsPerCell = Constants.CoinPerCell;
        public int CoinsPerLine = Constants.CoinPerLine;
        public int CoinsPerCombo = Constants.CoinPerCombo;
        public int CoinsPerSpecialCleared = 5;
        public int CoinsPerPerfectClear = 30;

        [Header("Conversion")]
        public int GemsPerFiftyCoins = 1;
        public int CoinsPerGem = 50;

        [Header("Reward ad multipliers")]
        public float RewardedAdCoinMultiplier = 2.0f;
        public float RewardedAdXpMultiplier = 2.0f;

        [Header("Caps")]
        public long MaxCoins = 999_999_999;
        public long MaxGems = 99_999;
    }
}
