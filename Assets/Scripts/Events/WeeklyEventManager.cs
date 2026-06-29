// ----------------------------------------------------------------------------
// WeeklyEventManager.cs
// Runs themed weekly events with bonus rewards / missions / multipliers.
// Picks a deterministic event from a pool based on ISO week number.
// ----------------------------------------------------------------------------

using System;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Economy;
using UnityEngine;

namespace Game.BlockPuzzle.Events
{
    public sealed class WeeklyEventManager : MonoBehaviour, IService
    {
        public string CurrentEventId { get; private set; }
        public string CurrentEventName { get; private set; }
        public float CoinMultiplier { get; private set; } = 1f;
        public float XpMultiplier { get; private set; } = 1f;
        public DateTimeOffset EndsAtUtc { get; private set; }

        // 8 events cycle through 8-week loop.
        private static readonly (string id, string name, float coin, float xp)[] s_Pool =
        {
            ("score_surge",   "Score Surge", 1.5f, 1.5f),
            ("bomb_bonanza",  "Bomb Bonanza", 1.25f, 2.0f),
            ("rainbow_rush",  "Rainbow Rush", 1.25f, 2.0f),
            ("combo_carnival","Combo Carnival", 1.5f, 1.5f),
            ("purge_week",    "Purge Week", 1.25f, 2.0f),
            ("coin_cascade",  "Coin Cascade", 2.0f, 1.0f),
            ("frost_festival","Frost Festival", 1.5f, 1.5f),
            ("classic_week",  "Classic Week", 1.0f, 2.0f)
        };

        public void Initialize()
        {
            int week = (DateTime.UtcNow.DayOfYear / 7) % s_Pool.Length;
            var (id, name, coin, xp) = s_Pool[week];
            CurrentEventId = id;
            CurrentEventName = name;
            CoinMultiplier = coin;
            XpMultiplier = xp;
            EndsAtUtc = DateTimeOffset.UtcNow.AddDays(7 - (DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)DateTime.UtcNow.DayOfWeek));
            Log.Info("WeeklyEvent", $"Active event: {name} (coin x{coin}, xp x{xp})");
        }

        public void Shutdown() { }

        public Reward ApplyMultiplier(Reward r)
        {
            r.Coins = Mathf.RoundToInt(r.Coins * CoinMultiplier);
            r.Xp = Mathf.RoundToInt(r.Xp * XpMultiplier);
            return r;
        }

        public bool IsActive => DateTimeOffset.UtcNow < EndsAtUtc;
    }
}
