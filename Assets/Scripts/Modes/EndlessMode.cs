// ----------------------------------------------------------------------------
// EndlessMode.cs
// Default mode. Player plays until no piece in their hand fits. Final score
// is whatever they accumulated.
// ----------------------------------------------------------------------------

using Game.BlockPuzzle.Board;
using Game.BlockPuzzle.Blocks;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Scoring;
using UnityEngine;

namespace Game.BlockPuzzle.Modes
{
    public sealed class EndlessMode : IGameMode
    {
        public ModeId Id => ModeId.Endless;
        public ModeState State { get; private set; } = ModeState.Idle;
        public string DisplayName => "Classic";

        private ScoreManager _score;
        private BlockSpawner _spawner;

        public EndlessMode(ScoreManager score, BlockSpawner spawner)
        {
            _score = score;
            _spawner = spawner;
        }

        public void Begin()
        {
            State = ModeState.Running;
            _score.ResetForNewRun();
            _spawner.OnRunStart();
            GameManager.Instance.StartRun();
            Log.Info("EndlessMode", "Run started");
        }

        public void Pause() => State = ModeState.Paused;
        public void Resume() => State = ModeState.Running;

        public void End()
        {
            State = ModeState.GameOver;
            GameManager.Instance.EndRun(false);
        }

        public void OnPiecePlaced(BlockPiece piece, PlacementResult result) { }
        public void OnPieceConsumed(BlockPiece piece) { }

        public bool CheckGameOver()
        {
            return !_spawner.AnyFits();
        }

        public int ComputeFinalScore() => _score.Score;

        public ModeReward ComputeReward()
        {
            int score = _score.Score;
            int coins = score / 5;
            int gems = score / 500;
            return new ModeReward { Coins = coins, Gems = gems, Xp = score / 4 };
        }
    }
}
