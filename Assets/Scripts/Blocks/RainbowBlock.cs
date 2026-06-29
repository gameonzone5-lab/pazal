// ----------------------------------------------------------------------------
// RainbowBlock.cs
// Logical block that, when placed, clears every cell of one color on the
// board. The "color" of the Rainbow piece is set per-piece when spawned.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using Game.BlockPuzzle.Board;

namespace Game.BlockPuzzle.Blocks
{
    public static class RainbowBlock
    {
        public const string Id = "rainbow";

        public static bool IsRainbow(BlockPiece piece)
        {
            return piece != null && piece.Kind == BlockPiece.PieceKind.Rainbow;
        }

        /// <summary>Get cells that the rainbow will clear, given its color.</summary>
        public static void GetAffectedCells(Board.Board board, byte colorIndex, List<BoardCoord> output)
        {
            board.GetCellsOfColor(colorIndex, output);
        }
    }
}
