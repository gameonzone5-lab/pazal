// ----------------------------------------------------------------------------
// StatisticsTracker.cs
// Aggregates live gameplay stats and writes them to PlayerStatistics on
// every game-end. Used by profile UI, leaderboards, and personalization.
// ----------------------------------------------------------------------------

using Game.BlockPuzzle.Board;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Modes;
using Game.BlockPuzzle.Save;
using Game.BlockPuzzle.Scoring;
using UnityEngine;

namespace Game.BlockPuzzle.Profile
{
    public sealed class StatisticsTracker : MonoBehaviour, IService
    {
        // Live counters for the current run.
        public int CurrentRunPiecesPlaced { get; private set; }
        public int CurrentRunLinesCleared { get; private set; }
        public int CurrentRunBombsUsed { get; private set; }
        public int CurrentRunRainbowsUsed { get; private set; }
        public int CurrentRunCellsCleared { get; private set; }
        public int CurrentRunCombos { get; private set; }

        public void Initialize()
        {
            EventBus.Subscribe<ScoreChangedEvent>(OnScore);
            EventBus.Subscribe<ComboChangedEvent>(OnCombo);
            EventBus.Subscribe<PlacementResult>(OnPlacement);
        }

        public void Shutdown()
        {
            EventBus.Unsubscribe<ScoreChangedEvent>(OnScore);
            EventBus.Unsubscribe<ComboChangedEvent>(OnCombo);
            EventBus.Unsubscribe<PlacementResult>(OnPlacement);
        }

        public void OnRunStart()
        {
            CurrentRunPiecesPlaced = 0;
            CurrentRunLinesCleared = 0;
            CurrentRunBombsUsed = 0;
            CurrentRunRainbowsUsed = 0;
            CurrentRunCellsCleared = 0;
            CurrentRunCombos = 0;
        }

        public void OnRunEnd(ModeId modeId, int finalScore, bool victory)
        {
            var save = ServiceLocator.Resolve<SaveManager>();
            var s = save.Current.Statistics;
            s.TotalRuns++;
            s.TotalScore += finalScore;
            s.TotalCoinsEarned += ServiceLocator.Resolve<Economy.EconomyManager>().Wallet.Coins; // approximation
            s.TotalLinesCleared += CurrentRunLinesCleared;
            s.TotalCellsCleared += CurrentRunCellsCleared;
            s.TotalCombos += CurrentRunCombos;
            s.TotalBombsUsed += CurrentRunBombsUsed;
            s.TotalRainbowsUsed += CurrentRunRainbowsUsed;
            if (finalScore > s.BestScoreEndless && modeId == ModeId.Endless) s.BestScoreEndless = finalScore;
            if (finalScore > s.BestScoreTimed3 && modeId == ModeId.Timed3Min) s.BestScoreTimed3 = finalScore;
            if (finalScore > s.BestScoreTimed5 && modeId == ModeId.Timed5Min) s.BestScoreTimed5 = finalScore;
            if (finalScore > s.BestScoreDaily && modeId == ModeId.DailyChallenge) s.BestScoreDaily = finalScore;
            if (victory) s.LevelsCompleted++;
            save.MarkDirty();
        }

        // --------------------------------------------------------------------
        // Event handlers
        // --------------------------------------------------------------------
        private void OnScore(ScoreChangedEvent evt) { /* no per-event stat */ }

        private void OnCombo(ComboChangedEvent evt)
        {
            if (evt.Combo >= 2) CurrentRunCombos++;
        }

        private void OnPlacement(PlacementResult result)
        {
            CurrentRunPiecesPlaced++;
            if (result.LinesCleared > 0) CurrentRunLinesCleared += result.LinesCleared;
            if (result.ClearedCells != null) CurrentRunCellsCleared += result.ClearedCells.Count;
        }

        public void OnBombUsed() => CurrentRunBombsUsed++;
        public void OnRainbowUsed() => CurrentRunRainbowsUsed++;
    }
}
