// ----------------------------------------------------------------------------
// LockedCell.cs
// Locked cells are unbreakable by lines. They become clearable after 4 of
// their 4-neighbors are cleared (orthogonal adjacency). After unlock they
// behave like Normal cells and can be cleared normally.
//
// Used by Adventure mode levels to create "unlock the cage" puzzles.
// ----------------------------------------------------------------------------

using Game.BlockPuzzle.Board;
using UnityEngine;

namespace Game.BlockPuzzle.Blocks
{
    public static class LockedCell
    {
        public const string Id = "locked";
        public const int AdjacentClearsRequired = 4;

        public static bool IsLocked(BoardCell cell) => cell.Type == CellType.Locked;

        public static BoardCell CreateLocked()
        {
            return new BoardCell
            {
                Type = CellType.Locked,
                ColorIndex = 0,
                Hits = 0,
                AdjacentClears = 0,
                PieceId = 0
            };
        }

        /// <summary>
        /// Returns true when the locked cell has been unlocked and can now
        /// be cleared by line completion.
        /// </summary>
        public static bool TryUnlock(ref BoardCell cell)
        {
            if (!IsLocked(cell)) return false;
            if (cell.AdjacentClears < AdjacentClearsRequired) return false;
            cell.Type = CellType.Normal;
            cell.AdjacentClears = 0;
            return true;
        }
    }
}
