// ----------------------------------------------------------------------------
// DailyChallengeManager.cs
// Generates the daily challenge seed, validates the player's score, and
// submits it to the leaderboard.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Save;
using Game.BlockPuzzle.Security;
using UnityEngine;

namespace Game.BlockPuzzle.Events
{
    public sealed class DailyChallengeManager : MonoBehaviour, IService
    {
        public string CurrentChallengeId { get; private set; }
        public int BestScoreToday { get; private set; }

        public void Initialize()
        {
            CurrentChallengeId = DailyChallengeMode.ComputeChallengeId(DateTime.UtcNow.Date);
            var save = ServiceLocator.TryResolve<SaveManager>();
            if (save?.Current != null)
            {
                foreach (var e in save.Current.DailyChallenges)
                    if (e.ChallengeId == CurrentChallengeId) BestScoreToday = e.Score;
            }
        }

        public void Shutdown() { }

        public int SubmitScore(int score)
        {
            if (score <= BestScoreToday)
            {
                BestScoreToday = Math.Max(BestScoreToday, score);
                return 0;
            }

            // Anti-cheat sanity: cap daily score to (cellsCleared * 2 + per-line bonus)
            // so a tampered client can't mint scores.
            if (!ServiceLocator.TryResolve<AntiCheat>()!.ValidateDailyChallengeScore(CurrentChallengeId, score, out var capped))
            {
                Log.Warn("DailyChallenge", $"Score {score} capped to {capped}");
                score = capped;
            }

            var save = ServiceLocator.Resolve<SaveManager>();
            var entry = new DailyChallengeSaveEntry
            {
                ChallengeId = CurrentChallengeId,
                Score = score,
                PlayedUtc = DateTimeOffset.UtcNow
            };
            save.Current.DailyChallenges.RemoveAll(e => e.ChallengeId == CurrentChallengeId);
            save.Current.DailyChallenges.Add(entry);
            save.MarkDirty();
            BestScoreToday = score;

            // Submit to leaderboards (best-effort).
            ServiceLocator.TryResolve<Cloud.PlayGamesManager>()?.SubmitDailyScore(CurrentChallengeId, score);

            // Reward (only on a personal best).
            ServiceLocator.Resolve<Economy.RewardSystem>().Grant(
                coins: score / 6 + 25,
                gems: score / 600 + 1,
                xp: score / 3 + 50,
                source: $"daily:{CurrentChallengeId}");

            return score;
        }

        public IEnumerable<DailyChallengeSaveEntry> GetHistory()
        {
            var save = ServiceLocator.Resolve<SaveManager>();
            return save.Current.DailyChallenges;
        }
    }
}
