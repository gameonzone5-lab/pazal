// ----------------------------------------------------------------------------
// BattlePassConfig.cs
// Battle pass tier definitions. XP thresholds and rewards per tier.
// ----------------------------------------------------------------------------

using System;
using UnityEngine;

namespace Game.BlockPuzzle.Config
{
    [System.Serializable]
    public struct BattlePassReward
    {
        public int Coins;
        public int Gems;
        public int Energy;        // 0 = none
        public string CosmeticId; // e.g. board skin, block skin
    }

    [System.Serializable]
    public struct BattlePassTier
    {
        public int Tier;
        public int XpRequired;
        public BattlePassReward FreeReward;
        public BattlePassReward PremiumReward;
    }

    [CreateAssetMenu(menuName = "BlockCraft/Battle Pass Config", fileName = "BattlePassConfig")]
    public sealed class BattlePassConfig : ScriptableObject
    {
        public string SeasonId;
        public string DisplayName;
        public int TiersCount = 30;
        public int XpPerTier = 100;
        public int PremiumPriceGems;
        public BattlePassTier[] Tiers;
        public DateTimeOffset SeasonStartUtc;
        public DateTimeOffset SeasonEndUtc;

        public int TotalXpForMaxTier => TiersCount * XpPerTier;
    }
}
