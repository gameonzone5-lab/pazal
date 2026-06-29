// ----------------------------------------------------------------------------
// BombBlock.cs
// Logical block that, when placed, clears a 3x3 area centered on its origin.
// Visually represented as a dark cube with a pulsing fuse icon.
//
// All gameplay effects are dispatched through SpecialCellApplier.
// ----------------------------------------------------------------------------

using Game.BlockPuzzle.Board;
using Game.BlockPuzzle.Core;

namespace Game.BlockPuzzle.Blocks
{
    public static class BombBlock
    {
        public const string Id = "bomb";

        /// <summary>True if the piece is a bomb.</summary>
        public static bool IsBomb(BlockPiece piece)
        {
            return piece != null && piece.Kind == BlockPiece.PieceKind.Bomb;
        }

        /// <summary>Get cells affected by a bomb placed at (cx, cy).</summary>
        public static void GetAffectedCells(Board.Board board, int cx, int cy, System.Collections.Generic.List<BoardCoord> output)
        {
            board.GetBombArea(cx, cy, output);
        }
    }
}
