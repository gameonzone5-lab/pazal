// ----------------------------------------------------------------------------
// BlockShapeLibrary.cs
// ScriptableObject holding every block shape definition. Designers create
// shape assets (single 1x1, line of 5, L-shape, etc.) here, and the spawner
// picks from them at runtime.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace Game.BlockPuzzle.Config
{
    /// <summary>
    /// A single shape. Cells are (x,y) offsets from the piece origin.
    /// </summary>
    [System.Serializable]
    public struct ShapeCell
    {
        public int X;
        public int Y;
    }

    [CreateAssetMenu(menuName = "BlockCraft/Block Shape", fileName = "BlockShape")]
    public sealed class BlockShape : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        [Tooltip("Cells occupied by the piece, relative to the piece origin.")]
        public ShapeCell[] Cells = System.Array.Empty<ShapeCell>();
        [Tooltip("Higher weight = spawned more often. 0 disables.")]
        public int Weight = 1;
        public Color ColorTint = Color.white;
        public bool CanRotate = true;
        public bool CanMirror = false;

        public int Width
        {
            get
            {
                int max = 0;
                for (int i = 0; i < Cells.Length; i++)
                    if (Cells[i].X > max) max = Cells[i].X;
                return max + 1;
            }
        }

        public int Height
        {
            get
            {
                int max = 0;
                for (int i = 0; i < Cells.Length; i++)
                    if (Cells[i].Y > max) max = Cells[i].Y;
                return max + 1;
            }
        }
    }

    /// <summary>
    /// Collection of all shapes the spawner can use.
    /// </summary>
    [CreateAssetMenu(menuName = "BlockCraft/Shape Library", fileName = "ShapeLibrary")]
    public sealed class BlockShapeLibrary : ScriptableObject
    {
        public BlockShape[] Shapes = System.Array.Empty<BlockShape>();

        public BlockShape PickRandom(Security.SecureRandom rng)
        {
            int total = 0;
            for (int i = 0; i < Shapes.Length; i++) total += Mathf.Max(0, Shapes[i].Weight);
            if (total <= 0) return null;

            int r = rng.NextInt(0, total);
            for (int i = 0; i < Shapes.Length; i++)
            {
                r -= Mathf.Max(0, Shapes[i].Weight);
                if (r < 0) return Shapes[i];
            }
            return Shapes[Shapes.Length - 1];
        }

        /// <summary>Lookup by id (used by missions and analytics).</summary>
        public BlockShape FindById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < Shapes.Length; i++)
                if (Shapes[i].Id == id) return Shapes[i];
            return null;
        }
    }
}
