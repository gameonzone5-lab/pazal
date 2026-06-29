// ----------------------------------------------------------------------------
// BattlePassManager.cs
// Tracks XP, current tier, claimed rewards. Listens to XpGainedEvent.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Game.BlockPuzzle.Config;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Economy;
using UnityEngine;

namespace Game.BlockPuzzle.Progression
{
    public sealed class BattlePassManager : MonoBehaviour, IService
    {
        public int CurrentXp { get; private set; }
        public int CurrentTier { get; private set; }
        public bool IsPremium { get; private set; }

        public event Action OnTierReached;
        public event Action<int> OnXpGainedEvt;

        private BattlePassConfig _config;

        public void Initialize()
        {
            _config = GameConfig.Instance != null ? GameConfig.Instance.BattlePass : null;
            var save = ServiceLocator.Resolve<Save.SaveManager>();
            var bp = save.Current.BattlePass;
            if (bp != null)
            {
                CurrentXp = bp.Xp;
                CurrentTier = bp.Tier;
                IsPremium = bp.Premium;
                if (_config != null && string.IsNullOrEmpty(bp.SeasonId))
                    bp.SeasonId = _config.SeasonId;
            }
            EventBus.Subscribe<XpGainedEvent>(OnXp);
        }

        public void Shutdown()
        {
            EventBus.Unsubscribe<XpGainedEvent>(OnXp);
        }

        public void Show() => GameManager.Instance.TransitionTo(GameState.MainMenu); // show via dedicated screen

        public void OnXpGained(int amount) => AddXp(amount, "passive");

        private void OnXp(XpGainedEvent evt) => AddXp(evt.Amount, evt.Source);

        public void AddXp(int amount, string source)
        {
            if (amount <= 0 || _config == null) return;
            CurrentXp += amount;
            while (CurrentTier < _config.TiersCount
                && CurrentXp >= (CurrentTier + 1) * _config.XpPerTier)
            {
                CurrentTier++;
                OnTierReached?.Invoke();
            }
            OnXpGainedEvt?.Invoke(amount);

            var save = ServiceLocator.Resolve<Save.SaveManager>();
            save.Current.BattlePass.Xp = CurrentXp;
            save.Current.BattlePass.Tier = CurrentTier;
            save.MarkDirty();
        }

        public bool ClaimFree(int tier)
        {
            if (tier > CurrentTier || _config == null) return false;
            var save = ServiceLocator.Resolve<Save.SaveManager>();
            if (save.Current.BattlePass.ClaimedFreeTiers.Contains(tier)) return false;
            var t = FindTier(tier);
            if (t == null) return false;
            ServiceLocator.Resolve<RewardSystem>().Grant(
                t.FreeReward.Coins, t.FreeReward.Gems, t.FreeReward.Energy, $"battlepass_free_{tier}");
            save.Current.BattlePass.ClaimedFreeTiers.Add(tier);
            save.MarkDirty();
            return true;
        }

        public bool ClaimPremium(int tier)
        {
            if (!IsPremium || tier > CurrentTier || _config == null) return false;
            var save = ServiceLocator.Resolve<Save.SaveManager>();
            if (save.Current.BattlePass.ClaimedPremiumTiers.Contains(tier)) return false;
            var t = FindTier(tier);
            if (t == null) return false;
            ServiceLocator.Resolve<RewardSystem>().Grant(
                t.PremiumReward.Coins, t.PremiumReward.Gems, t.PremiumReward.Energy,
                $"battlepass_premium_{tier}");
            save.Current.BattlePass.ClaimedPremiumTiers.Add(tier);
            save.MarkDirty();
            return true;
        }

        public bool BuyPremium()
        {
            if (_config == null) return false;
            var econ = ServiceLocator.Resolve<EconomyManager>();
            if (!econ.TrySpendGems(_config.PremiumPriceGems, "battlepass_premium")) return false;
            IsPremium = true;
            var save = ServiceLocator.Resolve<Save.SaveManager>();
            save.Current.BattlePass.Premium = true;
            save.MarkDirty();
            return true;
        }

        private BattlePassTier? FindTier(int tier)
        {
            if (_config.Tiers == null) return null;
            for (int i = 0; i < _config.Tiers.Length; i++)
                if (_config.Tiers[i].Tier == tier) return _config.Tiers[i];
            return null;
        }
    }
}
