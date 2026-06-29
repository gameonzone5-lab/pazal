// ----------------------------------------------------------------------------
// AdventureMode.cs
// Level-based progression. Loads a LevelDefinition (target score, locked
// cells, frozen cells, lines to clear). Player wins when the target is met
// AND no more pieces fit; loses if pieces are exhausted before target.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using Game.BlockPuzzle.Board;
using Game.BlockPuzzle.Blocks;
using Game.BlockPuzzle.Config;
using Game.BlockPuzzle.Core;

namespace Game.BlockPuzzle.Modes
{
    public sealed class AdventureMode : IGameMode
    {
        public ModeId Id => ModeId.Adventure;
        public ModeState State { get; private set; } = ModeState.Idle;
        public string DisplayName => $"Level {_level?.LevelNumber ?? 0}";

        public LevelDefinition Level => _level;
        public int Lives { get; private set; }
        public int CurrentScore { get; private set; }
        public int LinesCleared { get; private set; }

        private LevelDefinition _level;
        private ScoreManager _score;
        private BlockSpawner _spawner;
        private BoardController _board;
        private int _piecesPlaced;

        public AdventureMode(ScoreManager score, BlockSpawner spawner, BoardController board)
        {
            _score = score;
            _spawner = spawner;
            _board = board;
        }

        public void LoadLevel(LevelDefinition level)
        {
            _level = level;
            Lives = level?.StartingLives > 0 ? level.StartingLives : Constants.AdventureModeStartingLives;
            CurrentScore = 0;
            LinesCleared = 0;
            _piecesPlaced = 0;
        }

        public void Begin()
        {
            if (_level == null)
            {
                Log.Error("AdventureMode", "Begin called with no level loaded");
                return;
            }
            State = ModeState.Running;
            _score.ResetForNewRun();
            _spawner.OnRunStart();
            ApplyLevelLayout();
            GameManager.Instance.StartRun();
        }

        private void ApplyLevelLayout()
        {
            if (_board?.Board == null || _level?.InitialSpecials == null) return;
            foreach (var s in _level.InitialSpecials)
            {
                if (s.Type == CellType.Locked) _board.Board.PlaceSpecial(
                    new BoardCoord(s.X, s.Y),
                    Blocks.LockedCell.CreateLocked());
                else if (s.Type == CellType.Frozen) _board.Board.PlaceSpecial(
                    new BoardCoord(s.X, s.Y),
                    Blocks.FrozenBlock.CreateFrozen((byte)0));
            }
        }

        public void Pause() => State = ModeState.Paused;
        public void Resume() => State = ModeState.Running;

        public void End()
        {
            State = ModeState.GameOver;
            GameManager.Instance.EndRun(false);
        }

        public void OnPiecePlaced(BlockPiece piece, PlacementResult result)
        {
            _piecesPlaced++;
            if (result.LinesCleared > 0) LinesCleared += result.LinesCleared;
        }

        public void OnPieceConsumed(BlockPiece piece) { }

        public bool CheckGameOver()
        {
            if (Lives <= 0) return true;
            return !_spawner.AnyFits();
        }

        public bool CheckVictory()
        {
            return _level != null
                && CurrentScore >= _level.TargetScore
                && LinesCleared >= (_level.LinesToClear != null && _level.LinesToClear.Length > 0
                    ? _level.LinesToClear[0]
                    : 0);
        }

        public int ComputeFinalScore() => CurrentScore;

        public ModeReward ComputeReward()
        {
            bool victory = CheckVictory();
            int coins = CurrentScore / 8 + (victory ? 50 : 0);
            int gems = victory ? 5 : 1;
            int xp = CurrentScore / 4 + (victory ? 100 : 25);
            return new ModeReward { Coins = coins, Gems = gems, Xp = xp, IsJackpot = victory };
        }
    }
}
