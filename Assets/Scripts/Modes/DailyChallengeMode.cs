// ----------------------------------------------------------------------------
// DailyChallengeMode.cs
// Each calendar day picks a deterministic seed; every player gets the same
// piece sequence and a target score. Score is uploaded to the leaderboard
// at the end. Includes a free-attempt + extra-attempt-for-ads flow.
// ----------------------------------------------------------------------------

using System;
using Game.BlockPuzzle.Board;
using Game.BlockPuzzle.Blocks;
using Game.BlockPuzzle.Core;

namespace Game.BlockPuzzle.Modes
{
    public sealed class DailyChallengeMode : IGameMode
    {
        public ModeId Id => ModeId.DailyChallenge;
        public ModeState State { get; private set; } = ModeState.Idle;
        public string DisplayName => "Daily";

        public string ChallengeId { get; private set; }
        public int AttemptsToday { get; private set; }

        private ScoreManager _score;
        private BlockSpawner _spawner;

        public DailyChallengeMode(ScoreManager score, BlockSpawner spawner)
        {
            _score = score;
            _spawner = spawner;
        }

        public void Begin()
        {
            ChallengeId = ComputeChallengeId(DateTime.UtcNow.Date);
            AttemptsToday++;
            State = ModeState.Running;
            _score.ResetForNewRun();
            _spawner.OnRunStart();
            GameManager.Instance.StartRun();
        }

        public void Pause() => State = ModeState.Paused;
        public void Resume() => State = ModeState.Running;
        public void End()
        {
            State = ModeState.GameOver;
            GameManager.Instance.EndRun(true);
        }

        public void OnPiecePlaced(BlockPiece piece, PlacementResult result) { }
        public void OnPieceConsumed(BlockPiece piece) { }

        public bool CheckGameOver() => !_spawner.AnyFits();
        public int ComputeFinalScore() => _score.Score;

        public ModeReward ComputeReward()
        {
            int score = _score.Score;
            return new ModeReward
            {
                Coins = score / 6 + 25,
                Gems = score / 600 + 1,
                Xp = score / 3 + 50,
                IsJackpot = score > 3000
            };
        }

        /// <summary>Stable id from the date (yyyy-mm-dd).</summary>
        public static string ComputeChallengeId(DateTime utcDate)
        {
            return $"daily-{utcDate:yyyy-MM-dd}";
        }
    }
}
