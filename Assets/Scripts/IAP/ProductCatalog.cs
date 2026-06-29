// ----------------------------------------------------------------------------
// ProductCatalog.cs
// Read-only views over the IAPProductLibrary, grouping products by category
// for UI display and discounted bundles.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using Game.BlockPuzzle.Config;
using UnityEngine;

namespace Game.BlockPuzzle.IAP
{
    public static class ProductCatalog
    {
        public static IEnumerable<IAPProduct> CoinPacks(IAPProductLibrary lib) => lib.CoinPacks ?? System.Array.Empty<IAPProduct>();
        public static IEnumerable<IAPProduct> GemPacks(IAPProductLibrary lib) => lib.GemPacks ?? System.Array.Empty<IAPProduct>();
        public static IEnumerable<IAPProduct> SpecialOffers(IAPProductLibrary lib) => lib.SpecialOffers ?? System.Array.Empty<IAPProduct>();

        public static IAPProduct RemoveAds(IAPProductLibrary lib) => lib != null ? lib.RemoveAds : null;
        public static IAPProduct Premium(IAPProductLibrary lib) => lib != null ? lib.PremiumMonthly : null;

        /// <summary>
        /// Compute the "value" of a coin pack as a multiplier vs. the cheapest
        /// non-discount pack, used by the shop to display the "best value" tag.
        /// </summary>
        public static float BestValueMultiplier(IAPProductLibrary lib)
        {
            if (lib == null || lib.CoinPacks == null || lib.CoinPacks.Length == 0) return 1f;
            float bestRatio = 0f;
            foreach (var p in lib.CoinPacks)
            {
                if (p == null || p.CoinAmount <= 0) continue;
                // The ratio is coins per "unit"; we approximate unit by DisplayName length
                // as a placeholder — real impl would use price tiers.
                float r = p.CoinAmount;
                if (r > bestRatio) bestRatio = r;
            }
            return bestRatio > 0 ? bestRatio / 1000f : 1f;
        }
    }
}
