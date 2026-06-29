// ----------------------------------------------------------------------------
// BlockColorPalette.cs
// 8-color block palette + accessibility variants.
// ----------------------------------------------------------------------------

using UnityEngine;

namespace Game.BlockPuzzle.Config
{
    /// <summary>
    /// Defines the set of colors used for blocks. Includes default,
    /// high-contrast, and colorblind-safe variants.
    /// </summary>
    [CreateAssetMenu(menuName = "BlockCraft/Color Palette", fileName = "ColorPalette")]
    public sealed class BlockColorPalette : ScriptableObject
    {
        [Header("Standard")]
        public Color[] Standard = new Color[]
        {
            new Color(0.95f, 0.30f, 0.40f), // coral
            new Color(0.95f, 0.55f, 0.20f), // tangerine
            new Color(0.95f, 0.85f, 0.25f), // sun
            new Color(0.45f, 0.85f, 0.45f), // mint
            new Color(0.30f, 0.75f, 0.95f), // sky
            new Color(0.55f, 0.45f, 0.95f), // violet
            new Color(0.95f, 0.45f, 0.85f), // magenta
            new Color(0.30f, 0.85f, 0.85f)  // teal
        };

        [Header("Color-blind safe (Wong palette)")]
        public Color[] ColorBlindSafe = new Color[]
        {
            new Color(0.00f, 0.45f, 0.70f),
            new Color(0.90f, 0.62f, 0.00f),
            new Color(0.00f, 0.62f, 0.45f),
            new Color(0.80f, 0.47f, 0.65f),
            new Color(0.94f, 0.89f, 0.26f),
            new Color(0.34f, 0.71f, 0.91f),
            new Color(0.84f, 0.37f, 0.00f),
            new Color(0.50f, 0.50f, 0.50f)
        };

        public Color[] Get(Theme.ColorBlindMode mode)
        {
            return mode == Theme.ColorBlindMode.None ? Standard : ColorBlindSafe;
        }

        public Color GetAt(int index, Theme.ColorBlindMode mode)
        {
            var arr = Get(mode);
            if (arr == null || arr.Length == 0) return Color.white;
            return arr[index % arr.Length];
        }
    }
}
