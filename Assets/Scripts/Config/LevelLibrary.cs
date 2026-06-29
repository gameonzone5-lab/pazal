// ----------------------------------------------------------------------------
// LevelLibrary.cs
// Adventure mode level definitions. Each level packs the board with a layout
// of locked cells, frozen cells, and a score / clear target.
// ----------------------------------------------------------------------------

using UnityEngine;

namespace Game.BlockPuzzle.Config
{
    [System.Serializable]
    public struct SpecialCellData
    {
        public int X;
        public int Y;
        public Board.CellType Type;
    }

    [CreateAssetMenu(menuName = "BlockCraft/Adventure Level", fileName = "Level")]
    public sealed class LevelDefinition : ScriptableObject
    {
        public int LevelNumber;
        public string DisplayName;
        public int WorldIndex;
        public int TargetScore;
        public int[] LinesToClear;        // e.g. {1,2,3} = clear 1 line, then 2, then 3 to pass
        public int MaxPieces;             // hard cap on placements; -1 = use default
        public int StartingLives;         // -1 = use default
        public bool AllowBomb;
        public bool AllowRainbow;
        public SpecialCellData[] InitialSpecials;
        [TextArea] public string Hint;
    }

    [CreateAssetMenu(menuName = "BlockCraft/Level Library", fileName = "LevelLibrary")]
    public sealed class LevelLibrary : ScriptableObject
    {
        public LevelDefinition[] Levels;

        public LevelDefinition Find(int levelNumber)
        {
            if (Levels == null) return null;
            for (int i = 0; i < Levels.Length; i++)
                if (Levels[i].LevelNumber == levelNumber) return Levels[i];
            return null;
        }
    }
}
