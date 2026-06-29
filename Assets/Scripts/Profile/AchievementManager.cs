// ----------------------------------------------------------------------------
// AchievementManager.cs
// Listens to gameplay events and unlocks achievements. Each achievement has
// a Google Play Games id and a local "earned" flag.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Save;
using UnityEngine;

namespace Game.BlockPuzzle.Profile
{
    [Serializable]
    public sealed class Achievement
    {
        public string Id;                 // local id
        public string DisplayName;
        public string Description;
        public string GooglePlayId;       // PGS achievement id
        public int Steps;                 // for incremental achievements
        public bool Unlocked;
        public int Progress;
    }

    public sealed class AchievementManager : MonoBehaviour, IService
    {
        public IReadOnlyList<Achievement> All => _all;
        public event Action<Achievement> OnUnlocked;

        private List<Achievement> _all;

        public void Initialize()
        {
            _all = new List<Achievement>
            {
                New("first_clear",   "First Steps",     "Clear your first line.",                  "CgkI__first_clear"),
                New("ten_clears",    "Decimator",       "Clear 10 lines in a single run.",         "CgkI__ten_clears",   steps: 10),
                New("hundred_clears","Centurion",       "Clear 100 lines in total.",               "CgkI__hundred",      steps: 100),
                New("score_1000",    "Score Hunter",    "Reach 1,000 points.",                     "CgkI__score_1000"),
                New("score_10000",   "Score Master",    "Reach 10,000 points.",                    "CgkI__score_10k"),
                New("combo_x3",      "Triplet",         "Clear 3 lines at once.",                  "CgkI__combo_x3"),
                New("combo_x4",      "Quad Squad",      "Clear 4 lines at once.",                  "CgkI__combo_x4"),
                New("bomb_first",    "Demolitions",     "Use a bomb for the first time.",          "CgkI__bomb_first"),
                New("rainbow_first", "Pot of Gold",     "Use a rainbow for the first time.",       "CgkI__rainbow_first"),
                New("frozen_break",  "Defrosted",       "Break a frozen cell.",                    "CgkI__frozen_break"),
                New("unlock_locked", "Keymaster",       "Unlock a locked cell.",                   "CgkI__unlock_locked"),
                New("daily_first",   "Daily Devotee",   "Complete a daily challenge.",             "CgkI__daily_first"),
                New("adventurer_10", "Adventurer",      "Complete 10 adventure levels.",           "CgkI__adventurer_10", steps: 10),
                New("week_streak_7", "Loyal",           "Log in 7 days in a row.",                 "CgkI__week_streak",  steps: 7)
            };

            EventBus.Subscribe<Scoring.ScoreChangedEvent>(OnScore);
            EventBus.Subscribe<Scoring.ComboChangedEvent>(OnCombo);
            EventBus.Subscribe<Scoring.PlacementResult>(OnPlacement);
        }

        public void Shutdown()
        {
            EventBus.Unsubscribe<Scoring.ScoreChangedEvent>(OnScore);
            EventBus.Unsubscribe<Scoring.ComboChangedEvent>(OnCombo);
            EventBus.Unsubscribe<Scoring.PlacementResult>(OnPlacement);
        }

        private void OnScore(Scoring.ScoreChangedEvent evt)
        {
            if (evt.NewScore >= 1000) Unlock("score_1000");
            if (evt.NewScore >= 10000) Unlock("score_10000");
        }

        private void OnCombo(Scoring.ComboChangedEvent evt)
        {
            if (evt.Combo >= 3) Unlock("combo_x3");
            if (evt.Combo >= 4) Unlock("combo_x4");
        }

        private void OnPlacement(Scoring.PlacementResult result)
        {
            if (result.LinesCleared > 0)
            {
                Unlock("first_clear");
                Increment("ten_clears", 1);
                Increment("hundred_clears", result.LinesCleared);
            }
        }

        public void OnBombUsed() => Unlock("bomb_first");
        public void OnRainbowUsed() => Unlock("rainbow_first");
        public void OnFrozenBroken() => Unlock("frozen_break");
        public void OnLockedUnlocked() => Unlock("unlock_locked");
        public void OnDailyChallengeCompleted() => Unlock("daily_first");
        public void OnAdventureLevelCompleted() => Increment("adventurer_10", 1);

        // --------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------
        private static Achievement New(string id, string name, string desc, string gpg, int steps = 1)
        {
            return new Achievement
            {
                Id = id, DisplayName = name, Description = desc,
                GooglePlayId = gpg, Steps = Math.Max(1, steps)
            };
        }

        private Achievement Find(string id)
        {
            for (int i = 0; i < _all.Count; i++)
                if (_all[i].Id == id) return _all[i];
            return null;
        }

        public void Increment(string id, int amount)
        {
            var a = Find(id);
            if (a == null || a.Unlocked) return;
            a.Progress = Math.Min(a.Steps, a.Progress + amount);
            if (a.Progress >= a.Steps) Unlock(id);
        }

        public void Unlock(string id)
        {
            var a = Find(id);
            if (a == null || a.Unlocked) return;
            a.Unlocked = true;
            a.Progress = a.Steps;
            OnUnlocked?.Invoke(a);
            ServiceLocator.TryResolve<Cloud.PlayGamesManager>()?.UnlockAchievement(a.GooglePlayId);
            SFXPlayer.Play("reward");
        }
    }
}
