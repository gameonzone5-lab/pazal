// ----------------------------------------------------------------------------
// ScoreManager.cs
// Tracks the player's current score, combo counter, and best score. Awards
// coins / XP on score deltas. Designed to be the single point through which
// gameplay code mutates score.
// ----------------------------------------------------------------------------

using Game.BlockPuzzle.Board;
using Game.BlockPuzzle.Config;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Economy;
using UnityEngine;

namespace Game.BlockPuzzle.Scoring
{
    /// <summary>
    /// Emitted on every score delta.
    /// </summary>
    public readonly struct ScoreChangedEvent : IGameEvent
    {
        public readonly int OldScore;
        public readonly int Delta;
        public readonly int NewScore;
        public readonly string Reason;
        public ScoreChangedEvent(int oldS, int delta, int newS, string reason)
        { OldScore = oldS; Delta = delta; NewScore = newS; Reason = reason; }
    }

    /// <summary>
    /// Emitted on every combo.
    /// </summary>
    public readonly struct ComboChangedEvent : IGameEvent
    {
        public readonly int Combo;
        public readonly int Multiplier;
        public readonly bool IsNewBest;
        public ComboChangedEvent(int combo, int mult, bool newBest)
        { Combo = combo; Multiplier = mult; IsNewBest = newBest; }
    }

    public sealed class ScoreManager : MonoBehaviour, IService
    {
        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public int BestCombo { get; private set; }
        public int CurrentCombo { get; private set; }
        public int LastLinesCleared { get; private set; }

        public void Initialize()
        {
            var save = ServiceLocator.TryResolve<Save.SaveManager>();
            if (save != null && save.Current != null)
            {
                HighScore = save.Current.Statistics.BestScoreEndless;
                BestCombo = save.Current.Statistics.BestCombo;
            }
        }

        public void Shutdown() { }

        public void ResetForNewRun()
        {
            Score = 0;
            CurrentCombo = 0;
            LastLinesCleared = 0;
        }

        // --------------------------------------------------------------------
        // Placement scoring
        // --------------------------------------------------------------------
        public void RegisterPlacement(int cellsPlaced, PlacementResult result)
        {
            int points = cellsPlaced * Constants.BasePointsPerCell;
            string reason = "placement";
            int mult = 1;

            if (result.LinesCleared > 0)
            {
                points += result.LinesCleared * Constants.LineClearBonus;
                mult = ComboMultiplier(result.LinesCleared);
                points *= mult;
                reason = $"combo_x{mult}_lines_{result.LinesCleared}";
                LastLinesCleared = result.LinesCleared;
                UpdateCombo(result.LinesCleared);
            }
            else
            {
                CurrentCombo = 0;
            }

            if (result.IsPerfectClear)
            {
                points += Constants.PerfectClearBonus;
                reason += "_perfect";
            }

            AddPoints(points, reason);

            // Award coins for the placement.
            var econ = ServiceLocator.Resolve<EconomyManager>();
            int coins = cellsPlaced * GameConfig.Instance.Economy.CoinsPerCell;
            if (result.LinesCleared > 0)
                coins += result.LinesCleared * GameConfig.Instance.Economy.CoinsPerLine;
            if (result.IsPerfectClear)
                coins += GameConfig.Instance.Economy.CoinsPerPerfectClear;
            econ.AwardCoins(coins, $"run_{(result.LinesCleared > 0 ? "clear" : "place")}");

            // XP for battle pass.
            int xp = cellsPlaced + result.LinesCleared * 5;
            EventBus.Publish(new XpGainedEvent(xp, "gameplay"));
        }

        public void RegisterBombTriggered(BoardCoord center)
        {
            // Bomb counts as 1 "line" for combo purposes (visible clear).
            UpdateCombo(1);
            AddPoints(15, "bomb");
            EventBus.Publish(new ScoreChangedEvent(Score, 15, Score, "bomb"));
            ServiceLocator.Resolve<EconomyManager>()
                .AwardCoins(GameConfig.Instance.Economy.CoinsPerSpecialCleared, "bomb");
        }

        public void RegisterRainbowTriggered(BoardCoord center)
        {
            UpdateCombo(2);
            AddPoints(40, "rainbow");
            EventBus.Publish(new ScoreChangedEvent(Score, 40, Score, "rainbow"));
            ServiceLocator.Resolve<EconomyManager>()
                .AwardCoins(GameConfig.Instance.Economy.CoinsPerSpecialCleared * 2, "rainbow");
        }

        public void RegisterChain(int chainDepth, int cellsCleared)
        {
            int points = Constants.ChainBonusPerStep * chainDepth + cellsCleared;
            AddPoints(points, $"chain_d{chainDepth}");
            EventBus.Publish(new ScoreChangedEvent(Score, points, Score, $"chain_d{chainDepth}"));
            ServiceLocator.Resolve<EconomyManager>()
                .AwardCoins(cellsCleared * GameConfig.Instance.Economy.CoinsPerCell, "chain");
        }

        // --------------------------------------------------------------------
        // Internals
        // --------------------------------------------------------------------
        private void AddPoints(int points, string reason)
        {
            if (points <= 0) return;
            int old = Score;
            Score += points;
            EventBus.Publish(new ScoreChangedEvent(old, points, Score, reason));
            if (Score > HighScore)
            {
                HighScore = Score;
                var save = ServiceLocator.TryResolve<Save.SaveManager>();
                if (save?.Current != null)
                    save.Current.Statistics.BestScoreEndless = HighScore;
            }
        }

        private void UpdateCombo(int linesCleared)
        {
            CurrentCombo = linesCleared;
            if (CurrentCombo > BestCombo)
            {
                BestCombo = CurrentCombo;
                var save = ServiceLocator.TryResolve<Save.SaveManager>();
                if (save?.Current != null)
                    save.Current.Statistics.BestCombo = BestCombo;
            }
            EventBus.Publish(new ComboChangedEvent(CurrentCombo,
                ComboMultiplier(CurrentCombo), CurrentCombo == BestCombo));
        }

        public static int ComboMultiplier(int combo)
        {
            if (combo >= 4) return Constants.ComboMultiplier4;
            if (combo == 3) return Constants.ComboMultiplier3;
            if (combo == 2) return Constants.ComboMultiplier2;
            return 1;
        }

        public void SetScoreDirect(int value) => Score = value;
    }
}
