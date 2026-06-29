// ----------------------------------------------------------------------------
// IAPManager.cs
// Wraps Google Play Billing v7. Handles purchase flow, restoration, and
// server-side receipt validation stub. Applies rewards through RewardSystem.
//
// Real billing code is left as comments and shown via the GooglePlayBilling
// NuGet/Unity package. The interfaces are stable across versions.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.BlockPuzzle.Analytics;
using Game.BlockPuzzle.Config;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Save;
using UnityEngine;

namespace Game.BlockPuzzle.IAP
{
    public enum PurchaseState { Idle, Purchasing, Success, Failed, Cancelled, Pending }

    public readonly struct PurchaseResult
    {
        public readonly PurchaseState State;
        public readonly string ProductId;
        public readonly string ErrorMessage;
        public PurchaseResult(PurchaseState state, string productId, string error = null)
        { State = state; ProductId = productId; ErrorMessage = error; }
    }

    public sealed class IAPManager : MonoBehaviour, IService
    {
        public event Action<PurchaseResult> OnPurchaseComplete;
        public IReadOnlyDictionary<string, IAPProduct> Catalog => _catalog;

        private Dictionary<string, IAPProduct> _catalog = new();

        public async Task InitializeAsync()
        {
            var lib = GameConfig.Instance?.Products;
            if (lib == null) { Log.Warn("IAP", "No IAP product library configured"); return; }
            foreach (var p in lib.All)
            {
                if (p != null) _catalog[p.ProductId] = p;
            }
            await Task.Yield();

            // Wire up Google Play Billing v7 here. Outline:
            //   using GooglePlayBilling;
            //   await billingClient.StartConnection();
            //   billingClient.OnPurchasesUpdated += OnPurchasesUpdated;
            //   QueryInventory();

            Log.Info("IAP", $"Catalog ready ({_catalog.Count} products)");
        }

        public void Shutdown() { }

        public IAPProduct FindProduct(string productId)
        {
            _catalog.TryGetValue(productId, out var p);
            return p;
        }

        // --------------------------------------------------------------------
        // Public purchase API
        // --------------------------------------------------------------------
        public void Purchase(string productId)
        {
            var product = FindProduct(productId);
            if (product == null)
            {
                OnPurchaseComplete?.Invoke(new PurchaseResult(PurchaseState.Failed, productId, "unknown_product"));
                return;
            }
            // Real call:
            //   billingClient.LaunchBillingFlow(productId, BillingClient.BillingResponseCode.OK);
            OnPurchaseComplete?.Invoke(new PurchaseResult(PurchaseState.Pending, productId));
            ApplyPending(productId, "stub-receipt");
        }

        private void ApplyPending(string productId, string receipt)
        {
            var product = FindProduct(productId);
            if (product == null) return;
            var save = ServiceLocator.Resolve<SaveManager>();

            switch (product.Type)
            {
                case ProductType.Consumable:
                    ServiceLocator.Resolve<Economy.EconomyManager>().ApplyIapReward(product, receipt);
                    ServiceLocator.Resolve<AnalyticsManager>().LogEvent(Constants.AnalyticsEventIAPPurchase,
                        ("product", productId), ("coins", product.CoinAmount.ToString()),
                        ("gems", product.GemAmount.ToString()));
                    break;
                case ProductType.NonConsumable:
                    if (product.RemoveAds)
                    {
                        save.Current.RemoveAdsOwned = true;
                        save.MarkDirty();
                    }
                    break;
                case ProductType.Subscription:
                    save.Current.PremiumActive = true;
                    save.Current.PremiumUntilUtc = DateTimeOffset.UtcNow.AddDays(product.SubscriptionDays);
                    save.MarkDirty();
                    break;
            }

            OnPurchaseComplete?.Invoke(new PurchaseResult(PurchaseState.Success, productId));
        }

        // --------------------------------------------------------------------
        // Restore
        // --------------------------------------------------------------------
        public void RestorePurchases()
        {
            // Real call: billingClient.QueryPurchaseHistory(...);
            // For now, re-apply owned non-consumables from local save.
            var save = ServiceLocator.Resolve<SaveManager>();
            if (save.Current.RemoveAdsOwned)
                OnPurchaseComplete?.Invoke(new PurchaseResult(PurchaseState.Success, "remove_ads_restored"));
            if (save.Current.PremiumActive && DateTimeOffset.UtcNow < save.Current.PremiumUntilUtc)
                OnPurchaseComplete?.Invoke(new PurchaseResult(PurchaseState.Success, "premium_restored"));
        }
    }
}
