// ----------------------------------------------------------------------------
// TimedMode.cs
// Player has N minutes (3 or 5). Score is what they earn in that window.
// When the timer hits zero, the run ends.
// ----------------------------------------------------------------------------

using Game.BlockPuzzle.Board;
using Game.BlockPuzzle.Blocks;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Scoring;
using UnityEngine;

namespace Game.BlockPuzzle.Modes
{
    public sealed class TimedMode : IGameMode
    {
        public ModeId Id { get; }
        public ModeState State { get; private set; } = ModeState.Idle;
        public string DisplayName => Id == ModeId.Timed3Min ? "Blitz 3" : "Blitz 5";

        private readonly float _durationSeconds;
        private ScoreManager _score;
        private BlockSpawner _spawner;
        private float _elapsed;

        public float RemainingSeconds => Mathf.Max(0f, _durationSeconds - _elapsed);

        public TimedMode(ModeId id, ScoreManager score, BlockSpawner spawner)
        {
            Id = id;
            _durationSeconds = id == ModeId.Timed3Min
                ? Constants.TimedModeSeconds3Min
                : Constants.TimedModeSeconds5Min;
            _score = score;
            _spawner = spawner;
        }

        public void Begin()
        {
            State = ModeState.Running;
            _elapsed = 0f;
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

        public void Tick(float dt)
        {
            if (State != ModeState.Running) return;
            _elapsed += dt;
            if (_elapsed >= _durationSeconds) End();
        }

        public void OnPiecePlaced(BlockPiece piece, PlacementResult result) { }
        public void OnPieceConsumed(BlockPiece piece) { }

        public bool CheckGameOver()
        {
            return State == ModeState.GameOver;
        }

        public int ComputeFinalScore() => _score.Score;

        public ModeReward ComputeReward()
        {
            int score = _score.Score;
            int coins = score / 4;
            int gems = score / 400;
            int xp = score / 3;
            // Time-pressure bonus if score is very high.
            bool jackpot = score > 5000;
            return new ModeReward { Coins = coins, Gems = gems, Xp = xp, IsJackpot = jackpot };
        }
    }
}
