// ----------------------------------------------------------------------------
// BlockPlacer.cs
// Validates that a piece can be placed at a board coordinate, performs the
// placement through BoardController, and triggers special-block effects if
// the piece is a Bomb or Rainbow. Owns all the gameplay-state side effects of
// a successful placement: score, line clears, specials, audio, haptics.
// ----------------------------------------------------------------------------

using Game.BlockPuzzle.Board;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Modes;
using Game.BlockPuzzle.Scoring;
using UnityEngine;

namespace Game.BlockPuzzle.Blocks
{
    public sealed class BlockPlacer
    {
        private readonly BoardController _board;
        private readonly ScoreManager _score;
        private readonly SpecialCellApplier _specials;
        private readonly IGameMode _mode;

        public BlockPlacer(BoardController board, ScoreManager score,
            SpecialCellApplier specials, IGameMode mode)
        {
            _board = board;
            _score = score;
            _specials = specials;
            _mode = mode;
        }

        public bool TryPlace(BlockPiece piece, int x, int y)
        {
            if (piece == null || piece.Shape == null) return false;
            if (_board.Board == null) return false;

            // 1) Special-block branch
            if (piece.Kind == BlockPiece.PieceKind.Bomb)
            {
                if (!_board.Board.CanPlace(piece.Shape, x, y)) return false;
                _specials.ApplyBomb(new BoardCoord(x, y), piece.PieceId, piece.ColorIndex);
                ConsumePiece(piece);
                return true;
            }
            if (piece.Kind == BlockPiece.PieceKind.Rainbow)
            {
                if (!_board.Board.CanPlace(piece.Shape, x, y)) return false;
                _specials.ApplyRainbow(new BoardCoord(x, y), piece.PieceId, piece.ColorIndex);
                ConsumePiece(piece);
                return true;
            }

            // 2) Normal branch
            var result = _board.TryPlace(piece.Shape, x, y, piece.PieceId, piece.ColorIndex);
            if (!result.Success) return false;

            _score.RegisterPlacement(piece.Shape.Cells.Length, result);
            ConsumePiece(piece);

            // 3) Mode-specific hooks (e.g. adventure lives counter)
            _mode?.OnPiecePlaced(piece, result);

            // 4) Audio + haptics
            ServiceLocator.TryResolve<Audio.AudioManager>()?.Play(result.LinesCleared > 0 ? "place_with_clear" : "place");
            ServiceLocator.TryResolve<Utils.HapticManager>()?.Select();
            if (result.IsPerfectClear)
                ServiceLocator.TryResolve<Utils.HapticManager>()?.Success();

            return true;
        }

        private void ConsumePiece(BlockPiece piece)
        {
            piece.OnPlaced();
            // The spawner will refill this slot.
        }

        public bool CanPlace(BlockPiece piece, int x, int y)
        {
            if (piece == null || piece.Shape == null) return false;
            return _board.Board != null && _board.Board.CanPlace(piece.Shape, x, y);
        }

        public BoardCoord ScreenToCell(Vector2 world)
        {
            // Converted by BoardView; placer assumes pre-converted coords.
            // Kept here for symmetry / tests.
            return new BoardCoord(-1, -1);
        }
    }
}
