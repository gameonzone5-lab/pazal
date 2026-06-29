// ----------------------------------------------------------------------------
// BoardCell.cs
// Data describing one cell on the board. Cells are value types so the board
// can be a flat array (cache friendly) instead of an array of objects.
// ----------------------------------------------------------------------------

using UnityEngine;

namespace Game.BlockPuzzle.Board
{
    public enum CellType : byte
    {
        Empty      = 0,
        Normal     = 1,
        Bomb       = 2,
        Rainbow    = 3,
        Frozen     = 4,    // needs 2 clears in same row OR direct clear
        Locked     = 5,    // unbreakable; cleared by adjacent clears
        IceCracked = 6     // state after one Frozen hit
    }

    /// <summary>
    /// Single board cell. ~12 bytes; packs tightly into arrays.
    /// </summary>
    [System.Serializable]
    public struct BoardCell
    {
        public CellType Type;
        public byte ColorIndex;     // which palette slot
        public byte Hits;           // frozen hits, etc.
        public byte AdjacentClears; // for locked cells (need 4 adjacent clears)
        public int PieceId;         // for chain reaction bookkeeping

        public bool IsEmpty => Type == CellType.Empty;
        public bool IsFilled => Type != CellType.Empty;

        public static BoardCell Empty => new BoardCell { Type = CellType.Empty };

        public Color GetColor(Theme.ThemeManager themes)
        {
            return themes.GetCellColor(Type, ColorIndex);
        }

        public override string ToString() =>
            $"{Type}(c={ColorIndex},h={Hits},a={AdjacentClears})";
    }
}
