// ----------------------------------------------------------------------------
// ThemeConfig.cs
// Theme assets. Each theme defines background, primary, accent, text colors
// plus optional gradient stops and overlay textures.
// ----------------------------------------------------------------------------

using UnityEngine;

namespace Game.BlockPuzzle.Config
{
    /// <summary>
    /// A single named theme. Authored as ScriptableObject so artists can
    /// iterate without touching code.
    /// </summary>
    [CreateAssetMenu(menuName = "BlockCraft/Theme", fileName = "Theme")]
    public sealed class ThemeConfig : ScriptableObject
    {
        public string Id;
        public Theme.ThemeMode Mode;
        [Header("Surfaces")]
        public Color Background = new Color(0.07f, 0.07f, 0.10f, 1f);
        public Color Surface = new Color(0.12f, 0.12f, 0.16f, 1f);
        public Color SurfaceElevated = new Color(0.18f, 0.18f, 0.24f, 1f);
        [Header("Accents")]
        public Color Primary = new Color(0.55f, 0.45f, 0.95f, 1f);
        public Color Accent = new Color(0.95f, 0.45f, 0.85f, 1f);
        [Header("Text")]
        public Color TextPrimary = new Color(0.95f, 0.95f, 0.98f, 1f);
        public Color TextSecondary = new Color(0.65f, 0.65f, 0.75f, 1f);
        public Color TextDisabled = new Color(0.40f, 0.40f, 0.50f, 1f);
        [Header("Gameplay")]
        public Color BoardBackground = new Color(0.04f, 0.04f, 0.06f, 1f);
        public Color BoardGrid = new Color(0.20f, 0.20f, 0.28f, 1f);
        public Color CellEmpty = new Color(0.10f, 0.10f, 0.14f, 1f);
        public Color CellFilled = new Color(0.55f, 0.45f, 0.95f, 1f);
        public Color CellFrozen = new Color(0.40f, 0.70f, 0.95f, 1f);
        public Color CellLocked = new Color(0.55f, 0.40f, 0.30f, 1f);
        public Color CellBomb = new Color(0.20f, 0.20f, 0.20f, 1f);
        public Color CellRainbow = new Color(1.00f, 0.80f, 0.20f, 1f);
        [Header("FX")]
        public Color LineClearFx = new Color(1f, 0.95f, 0.40f, 1f);
        public Color ComboFx = new Color(1f, 0.55f, 0.20f, 1f);
    }

    /// <summary>
    /// Library that bundles a Light, Dark, and per-colorblind override.
    /// </summary>
    [CreateAssetMenu(menuName = "BlockCraft/Theme Set", fileName = "ThemeSet")]
    public sealed class ThemeSet : ScriptableObject
    {
        public ThemeConfig Dark;
        public ThemeConfig Light;
        public ThemeConfig Deuteranopia;
        public ThemeConfig Protanopia;
        public ThemeConfig Tritanopia;
        public ThemeConfig HighContrast;
    }
}
