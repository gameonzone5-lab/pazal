// ----------------------------------------------------------------------------
// EconomyManager.cs
// Single source of truth for coins / gems / energy. All mutations go through
// the methods here so we have one audit point.
// ----------------------------------------------------------------------------

using System;
using Game.BlockPuzzle.Config;
using Game.BlockPuzzle.Core;
using UnityEngine;

namespace Game.BlockPuzzle.Economy
{
    public sealed class EconomyManager : MonoBehaviour, IService
    {
        public Wallet Wallet { get; private set; }

        public event Action<long> OnCoinsChanged;
        public event Action<long> OnGemsChanged;
        public event Action<int> OnEnergyChanged;

        public void Initialize()
        {
            var save = ServiceLocator.Resolve<Save.SaveManager>();
            Wallet = save?.Current?.Wallet ?? Wallet.Default(GameConfig.Instance.Economy);
            EmitAll();
        }

        public void Shutdown()
        {
            // Wallet is persisted via SaveManager.
        }

        // --------------------------------------------------------------------
        // Mutators
        // --------------------------------------------------------------------

        public bool TrySpendCoins(long amount, string reason)
        {
            if (amount <= 0) return true;
            if (Wallet.Coins < amount) return false;
            ApplyCoins(Wallet.Coins - amount, reason);
            return true;
        }

        public bool TrySpendGems(long amount, string reason)
        {
            if (amount <= 0) return true;
            if (Wallet.Gems < amount) return false;
            ApplyGems(Wallet.Gems - amount, reason);
            return true;
        }

        public void AwardCoins(long amount, string reason)
        {
            if (amount <= 0) return;
            ApplyCoins(Wallet.Coins + amount, reason);
        }

        public void AwardGems(long amount, string reason)
        {
            if (amount <= 0) return;
            ApplyGems(Wallet.Gems + amount, reason);
        }

        public bool TryConsumeEnergy(int amount, string reason)
        {
            if (amount <= 0) return true;
            RechargeIfDue();
            if (Wallet.Energy < amount) return false;
            ApplyEnergy(Wallet.Energy - amount, reason);
            return true;
        }

        public void AwardEnergy(int amount, string reason)
        {
            if (amount <= 0) return;
            ApplyEnergy(Wallet.Energy + amount, reason);
        }

        // --------------------------------------------------------------------
        // IAP hooks
        // --------------------------------------------------------------------

        public void ApplyIapReward(IAPProduct product, string receiptId)
        {
            if (product == null) return;
            if (product.CoinAmount > 0) AwardCoins(product.CoinAmount, $"iap:{product.ProductId}");
            if (product.GemAmount > 0) AwardGems(product.GemAmount, $"iap:{product.ProductId}");
            if (product.Energy > 0) AwardEnergy(product.Energy, $"iap:{product.ProductId}");
            // Subscription/remove-ads handled by SaveManager flags (not by Wallet).
        }

        // --------------------------------------------------------------------
        // Internals
        // --------------------------------------------------------------------

        private void ApplyCoins(long newValue, string reason)
        {
            long old = Wallet.Coins;
            newValue = Math.Max(0, Math.Min(GameConfig.Instance.Economy.MaxCoins, newValue));
            Wallet.Coins = newValue;
            OnCoinsChanged?.Invoke(newValue);
            EventBus.Publish(new CurrencyChangedEvent(Constants.CurrencyCoin, old, newValue, reason));
            EventBus.Publish(new TransactionRecord(Constants.CurrencyCoin, newValue - old, newValue, reason));
        }

        private void ApplyGems(long newValue, string reason)
        {
            long old = Wallet.Gems;
            newValue = Math.Max(0, Math.Min(GameConfig.Instance.Economy.MaxGems, newValue));
            Wallet.Gems = newValue;
            OnGemsChanged?.Invoke(newValue);
            EventBus.Publish(new CurrencyChangedEvent(Constants.CurrencyGem, old, newValue, reason));
            EventBus.Publish(new TransactionRecord(Constants.CurrencyGem, newValue - old, newValue, reason));
        }

        private void ApplyEnergy(int newValue, string reason)
        {
            newValue = Math.Max(0, Math.Min(99, newValue));
            Wallet.Energy = newValue;
            OnEnergyChanged?.Invoke(newValue);
        }

        private void RechargeIfDue()
        {
            if (Wallet.Energy >= 5) return;
            if (DateTimeOffset.UtcNow < Wallet.EnergyRechargeAtUtc) return;
            Wallet.Energy++;
            Wallet.EnergyRechargeAtUtc = DateTimeOffset.UtcNow.AddMinutes(5);
            OnEnergyChanged?.Invoke(Wallet.Energy);
        }

        private void EmitAll()
        {
            OnCoinsChanged?.Invoke(Wallet.Coins);
            OnGemsChanged?.Invoke(Wallet.Gems);
            OnEnergyChanged?.Invoke(Wallet.Energy);
        }
    }
}
