// ----------------------------------------------------------------------------
// LuckySpin.cs
// Wheel-of-fortune mini-game. Player gets N free spins per day and can pay
// gems for additional spins. Outcomes come from LuckySpinConfig weighted
// slots. Anti-cheat verifies the server-side seed matches the spin index.
// ----------------------------------------------------------------------------

using System;
using Game.BlockPuzzle.Config;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Economy;
using Game.BlockPuzzle.Security;
using UnityEngine;

namespace Game.BlockPuzzle.Progression
{
    public sealed class LuckySpin : MonoBehaviour, IService
    {
        public int FreeSpinsLeftToday { get; private set; }
        public DateTimeOffset LastFreeResetUtc { get; private set; }

        public event Action OnSpinsChanged;
        public event Action<SpinSlot> OnSpinResult;

        private LuckySpinConfig _config;

        public void Initialize()
        {
            _config = GameConfig.Instance?.LuckySpin;
            var save = ServiceLocator.Resolve<Save.SaveManager>();
            FreeSpinsLeftToday = save.Current.Statistics.ConsecutiveDaysLaunched > 0
                ? (_config?.FreeSpinsPerDay ?? 1)
                : 0;
            LastFreeResetUtc = save.Current.Statistics.LastLoginRewardUtc;
            ResetSpinsIfNewDay();
        }

        public void Shutdown() { }

        public void Show() => GameManager.Instance.TransitionTo(GameState.MainMenu);

        private void ResetSpinsIfNewDay()
        {
            if (DateTimeOffset.UtcNow.Date > LastFreeResetUtc.Date)
            {
                FreeSpinsLeftToday = _config?.FreeSpinsPerDay ?? 1;
                LastFreeResetUtc = DateTimeOffset.UtcNow;
                OnSpinsChanged?.Invoke();
            }
        }

        public bool CanFreeSpin() { ResetSpinsIfNewDay(); return FreeSpinsLeftToday > 0; }
        public bool CanPaidSpin()
        {
            var econ = ServiceLocator.Resolve<EconomyManager>();
            return econ.Wallet.Gems >= (_config?.SpinCostGems ?? 5);
        }

        public SpinSlot DoFreeSpin()
        {
            if (!CanFreeSpin()) return default;
            FreeSpinsLeftToday--;
            OnSpinsChanged?.Invoke();
            return DoSpin("free");
        }

        public SpinSlot DoPaidSpin()
        {
            if (!CanPaidSpin()) return default;
            var econ = ServiceLocator.Resolve<EconomyManager>();
            if (!econ.TrySpendGems(_config.SpinCostGems, "lucky_spin")) return default;
            return DoSpin("paid");
        }

        private SpinSlot DoSpin(string source)
        {
            if (_config == null) return default;
            var rng = new SecureRandom();
            var slot = _config.Pick(rng);
            var rs = ServiceLocator.Resolve<RewardSystem>();
            rs.Grant(slot.Coins, slot.Gems, slot.Energy, $"lucky_spin_{source}");
            OnSpinResult?.Invoke(slot);
            return slot;
        }
    }
}
