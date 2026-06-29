// ----------------------------------------------------------------------------
// RewardSystem.cs
// Coordinates reward delivery. The game talks to RewardSystem instead of
// poking EconomyManager directly so we always:
//   1) Run anti-cheat checks on the source event
//   2) Push the reward through EconomyManager (which records transactions)
//   3) Notify UI / analytics
// ----------------------------------------------------------------------------

using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Missions;
using Game.BlockPuzzle.Progression;
using UnityEngine;

namespace Game.BlockPuzzle.Economy
{
    public sealed class RewardSystem : MonoBehaviour, IService
    {
        public void Initialize() { }
        public void Shutdown() { }

        /// <summary>
        /// Grant a reward described by a Reward object. Validates against the
        /// RewardValidator so cheated clients can't mint coins.
        /// </summary>
        public bool Grant(Reward reward, string source)
        {
            if (reward == null) return false;
            if (!ServiceLocator.Resolve<Security.AntiCheat>().ValidateReward(reward, source))
            {
                Log.Warn("RewardSystem", $"Reward rejected by anti-cheat: {source}");
                return false;
            }

            var econ = ServiceLocator.Resolve<EconomyManager>();
            if (reward.Coins > 0) econ.AwardCoins(reward.Coins, source);
            if (reward.Gems > 0) econ.AwardGems(reward.Gems, source);
            if (reward.Energy > 0) econ.AwardEnergy(reward.Energy, source);
            if (reward.Xp > 0) EventBus.Publish(new XpGainedEvent(reward.Xp, source));

            // Mission / progression hooks
            ServiceLocator.TryResolve<MissionManager>()?.OnRewardGranted(reward, source);
            ServiceLocator.TryResolve<BattlePassManager>()?.OnXpGained(reward.Xp);

            EventBus.Publish(new RewardGrantedEvent(reward, source));
            return true;
        }

        public bool Grant(int coins, int gems, int xp, string source) =>
            Grant(new Reward(coins, gems, 0, xp), source);
    }

    /// <summary>Represents a single reward bundle.</summary>
    [System.Serializable]
    public struct Reward
    {
        public int Coins;
        public int Gems;
        public int Energy;
        public int Xp;
        public string CosmeticId;

        public Reward(int coins, int gems, int energy, int xp)
        {
            Coins = coins; Gems = gems; Energy = energy; Xp = xp; CosmeticId = null;
        }
    }

    /// <summary>Emitted when RewardSystem.Grant succeeds.</summary>
    public readonly struct RewardGrantedEvent : IGameEvent
    {
        public readonly Reward Reward;
        public readonly string Source;
        public RewardGrantedEvent(Reward r, string s) { Reward = r; Source = s; }
    }
}
