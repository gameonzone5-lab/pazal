// ----------------------------------------------------------------------------
// Wallet.cs
// Plain-old data store for coins and gems. Backed by SaveData on disk.
// ----------------------------------------------------------------------------

using System;
using Game.BlockPuzzle.Config;
using Game.BlockPuzzle.Core;

namespace Game.BlockPuzzle.Economy
{
    /// <summary>
    /// Player's wallet. Immutable from outside; use EconomyManager methods to
    /// mutate.
    /// </summary>
    [Serializable]
    public sealed class Wallet
    {
        public long Coins;
        public long Gems;
        public int Energy;
        public DateTimeOffset EnergyRechargeAtUtc;

        public static Wallet Default(EconomyConfig cfg)
        {
            return new Wallet
            {
                Coins = cfg != null ? cfg.StartingCoins : Constants.StartingCoins,
                Gems = cfg != null ? cfg.StartingGems : Constants.StartingGems,
                Energy = 5,
                EnergyRechargeAtUtc = DateTimeOffset.UtcNow
            };
        }
    }

    /// <summary>
    /// Emitted for every transaction, even zero-sum ones (for analytics).
    /// </summary>
    public readonly struct TransactionRecord : IGameEvent
    {
        public readonly string Currency;
        public readonly long Delta;
        public readonly long After;
        public readonly string Reason;
        public readonly DateTimeOffset At;
        public TransactionRecord(string currency, long delta, long after, string reason)
        {
            Currency = currency;
            Delta = delta;
            After = after;
            Reason = reason;
            At = DateTimeOffset.UtcNow;
        }
    }
}
