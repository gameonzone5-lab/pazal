// ----------------------------------------------------------------------------
// IAPProductLibrary.cs
// In-app purchase catalog. Maps Google Play product ids to local definitions.
// ----------------------------------------------------------------------------

using UnityEngine;

namespace Game.BlockPuzzle.Config
{
    public enum ProductType
    {
        Consumable,         // coin packs
        NonConsumable,      // remove ads
        Subscription        // premium
    }

    [CreateAssetMenu(menuName = "BlockCraft/IAP Product", fileName = "IAPProduct")]
    public sealed class IAPProduct : ScriptableObject
    {
        public string ProductId;        // Google Play SKU
        public ProductType Type;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;

        [Header("Reward")]
        public int CoinAmount;
        public int GemAmount;
        public bool RemoveAds;
        public string SubscriptionId;
        public int SubscriptionDays;
        public float SubscriptionBonusMultiplier = 1.25f;
    }

    [CreateAssetMenu(menuName = "BlockCraft/IAP Library", fileName = "IAPLibrary")]
    public sealed class IAPProductLibrary : ScriptableObject
    {
        public IAPProduct[] CoinPacks;
        public IAPProduct[] GemPacks;
        public IAPProduct RemoveAds;
        public IAPProduct PremiumMonthly;
        public IAPProduct[] SpecialOffers;

        public IAPProduct[] All
        {
            get
            {
                var list = new System.Collections.Generic.List<IAPProduct>();
                if (CoinPacks != null) list.AddRange(CoinPacks);
                if (GemPacks != null) list.AddRange(GemPacks);
                if (RemoveAds != null) list.Add(RemoveAds);
                if (PremiumMonthly != null) list.Add(PremiumMonthly);
                if (SpecialOffers != null) list.AddRange(SpecialOffers);
                return list.ToArray();
            }
        }

        public IAPProduct Find(string productId)
        {
            var all = All;
            for (int i = 0; i < all.Length; i++)
                if (all[i] != null && all[i].ProductId == productId) return all[i];
            return null;
        }
    }
}
