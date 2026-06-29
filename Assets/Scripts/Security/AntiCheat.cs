// ----------------------------------------------------------------------------
// AntiCheat.cs
// Server-authoritative validator. The client calls AntiCheat.ValidateReward
// / ValidateScore / ValidateDailyChallengeScore before granting anything.
// The real validation happens on the server; this file is the local copy
// that keeps tampered clients from minting rewards offline (which the server
// would later reject anyway, but only after wasting bandwidth).
//
// Tampering on-device can never be fully prevented; the goal is to make it
// easier to cheat in a way that doesn't help (server rejects anyway).
// ----------------------------------------------------------------------------

using System;
using Game.BlockPuzzle.Config;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Economy;
using Game.BlockPuzzle.Save;
using UnityEngine;

namespace Game.BlockPuzzle.Security
{
    public sealed class AntiCheat : MonoBehaviour, IService
    {
        public int RecentPiecePlacements { get; private set; }
        public int RecentCellsCleared { get; private set; }
        public int RecentScoreDelta { get; private set; }
        public float RunStartTime { get; private set; }
        public float LastEventTime { get; private set; }

        public void Initialize()
        {
            EventBus.Subscribe<Scoring.ScoreChangedEvent>(OnScore);
            EventBus.Subscribe<Scoring.PlacementResult>(OnPlacement);
        }

        public void Shutdown()
        {
            EventBus.Unsubscribe<Scoring.ScoreChangedEvent>(OnScore);
            EventBus.Unsubscribe<Scoring.PlacementResult>(OnPlacement);
        }

        public void OnRunStart()
        {
            RecentPiecePlacements = 0;
            RecentCellsCleared = 0;
            RecentScoreDelta = 0;
            RunStartTime = Time.realtimeSinceStartup;
            LastEventTime = RunStartTime;
        }

        private void OnScore(Scoring.ScoreChangedEvent evt)
        {
            RecentScoreDelta += evt.Delta;
            LastEventTime = Time.realtimeSinceStartup;
        }

        private void OnPlacement(Scoring.PlacementResult result)
        {
            RecentPiecePlacements++;
            if (result.ClearedCells != null) RecentCellsCleared += result.ClearedCells.Count;
            LastEventTime = Time.realtimeSinceStartup;
        }

        // --------------------------------------------------------------------
        // Public validation
        // --------------------------------------------------------------------

        public bool ValidateReward(Reward reward, string source)
        {
            if (reward == null) return false;
            // Each source has a hard cap. Mints above the cap are rejected.
            int maxCoins = source switch
            {
                "placement" => 200,
                "run_place" => 200,
                "run_clear" => 100,
                "bomb" => 50,
                "rainbow" => 100,
                "chain" => 500,
                "iap:coin_small" => 10000,
                _ => 1000
            };
            int maxGems = source.StartsWith("iap:") ? 5000 : 50;
            int maxXp = source.StartsWith("iap:") ? 0 : 1000;

            return reward.Coins >= 0 && reward.Coins <= maxCoins
                && reward.Gems >= 0 && reward.Gems <= maxGems
                && reward.Xp >= 0 && reward.Xp <= maxXp;
        }

        public bool ValidateDailyChallengeScore(string challengeId, int score, out int capped)
        {
            capped = score;
            // Max score = (placements * 100) + (clears * 200) + base 500
            int max = RecentPiecePlacements * 100 + RecentCellsCleared * 200 + 500;
            if (score > max) { capped = max; return false; }
            return true;
        }

        public bool IsPlausibleTimeWindow(float maxRunSeconds = 7200f)
        {
            return (Time.realtimeSinceStartup - RunStartTime) <= maxRunSeconds;
        }
    }
}
