// ----------------------------------------------------------------------------
// RelaxMode.cs
// No game-over. Player can place pieces indefinitely. Used for casual play
// and for new players learning the controls.
// ----------------------------------------------------------------------------

using Game.BlockPuzzle.Board;
using Game.BlockPuzzle.Blocks;
using Game.BlockPuzzle.Scoring;

namespace Game.BlockPuzzle.Modes
{
    public sealed class RelaxMode : IGameMode
    {
        public ModeId Id => ModeId.Relax;
        public ModeState State { get; private set; } = ModeState.Idle;
        public string DisplayName => "Relax";

        private ScoreManager _score;
        private BlockSpawner _spawner;

        public RelaxMode(ScoreManager score, BlockSpawner spawner)
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

        public bool CheckGameOver() => false; // never

        public int ComputeFinalScore() => _score.Score;

        public ModeReward ComputeReward()
        {
            int score = _score.Score;
            return new ModeReward
            {
                Coins = score / 10,
                Gems = score / 1000,
                Xp = score / 5
            };
        }
    }
}
