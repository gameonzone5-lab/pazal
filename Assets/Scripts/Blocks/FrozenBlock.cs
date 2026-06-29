// ----------------------------------------------------------------------------
// FrozenBlock.cs
// Frozen cells are 2-hit cells: the first line-clear that overlaps a Frozen
// cell turns it into IceCracked; the second line-clear removes it.
//
// Frozen cells do NOT block placements — they are placed by the level
// designer (or via spawner) and act as obstacles the player must chip away.
// ----------------------------------------------------------------------------

using Game.BlockPuzzle.Board;
using UnityEngine;

namespace Game.BlockPuzzle.Blocks
{
    public static class FrozenBlock
    {
        public const string Id = "frozen";
        public const int HitsRequired = 2;

        public static bool IsFrozen(BoardCell cell)
        {
            return cell.Type == CellType.Frozen || cell.Type == CellType.IceCracked;
        }

        public static BoardCell CreateFrozen(byte colorIndex)
        {
            return new BoardCell
            {
                Type = CellType.Frozen,
                ColorIndex = colorIndex,
                Hits = 0,
                AdjacentClears = 0,
                PieceId = 0
            };
        }

        /// <summary>Register one clear hit. Returns true if the cell should be removed.</summary>
        public static bool RegisterHit(ref BoardCell cell)
        {
            if (!IsFrozen(cell)) return false;
            cell.Hits++;
            if (cell.Hits >= HitsRequired) return true;
            cell.Type = CellType.IceCracked;
            return false;
        }
    }
}
