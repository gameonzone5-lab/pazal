// ----------------------------------------------------------------------------
// ThemeManager.cs
// Owns the active ThemeConfig + ColorBlindMode. Listens to settings changes
// and re-applies the theme. UI / gameplay query it for colors.
// ----------------------------------------------------------------------------

using System;
using Game.BlockPuzzle.Board;
using Game.BlockPuzzle.Config;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Save;
using UnityEngine;

namespace Game.BlockPuzzle.Theme
{
    public enum ThemeMode { Dark, Light, Custom }

    public sealed class ThemeManager : MonoBehaviour, IService
    {
        public static ThemeManager Instance { get; private set; }
        public ThemeConfig Theme { get; private set; }
        public BlockColorPalette Palette { get; private set; }
        public ColorBlindMode ColorBlind { get; private set; }
        public ThemeMode Mode { get; private set; }

        public event Action OnThemeChanged;

        public void Initialize()
        {
            Instance = this;
            Palette = GameConfig.Instance != null ? GameConfig.Instance.Palette : null;
            var save = ServiceLocator.TryResolve<SaveManager>();
            var mode = save?.Current?.Settings.Theme ?? ThemeMode.Dark;
            var cb = save?.Current?.Settings.ColorBlind ?? ColorBlindMode.None;
            ColorBlind = cb;
            ApplyTheme(mode);
        }

        public void Shutdown()
        {
            if (Instance == this) Instance = null;
        }

        public void ApplyTheme(ThemeMode mode)
        {
            Mode = mode;
            var set = GameConfig.Instance != null ? GameConfig.Instance.Themes : null;
            switch (mode)
            {
                case ThemeMode.Light:
                    Theme = set != null && set.Light != null ? set.Light : MakeFallbackLight();
                    break;
                case ThemeMode.Dark:
                case ThemeMode.Custom:
                default:
                    Theme = set != null && set.Dark != null ? set.Dark : MakeFallbackDark();
                    break;
            }
            ApplyColorBlindOverride();
            OnThemeChanged?.Invoke();
        }

        public void SetColorBlindMode(ColorBlindMode mode)
        {
            ColorBlind = mode;
            ApplyColorBlindOverride();
            OnThemeChanged?.Invoke();
        }

        private void ApplyColorBlindOverride()
        {
            var set = GameConfig.Instance != null ? GameConfig.Instance.Themes : null;
            if (set == null) return;
            Theme = ColorBlind switch
            {
                ColorBlindMode.Deuteranopia => set.Deuteranopia ?? Theme,
                ColorBlindMode.Protanopia => set.Protanopia ?? Theme,
                ColorBlindMode.Tritanopia => set.Tritanopia ?? Theme,
                ColorBlindMode.HighContrast => set.HighContrast ?? Theme,
                _ => Theme
            };
        }

        public Color GetCellColor(CellType type, byte colorIndex)
        {
            return type switch
            {
                CellType.Bomb => Theme.CellBomb,
                CellType.Rainbow => Theme.CellRainbow,
                CellType.Frozen => Theme.CellFrozen,
                CellType.IceCracked => Theme.CellFrozen,
                CellType.Locked => Theme.CellLocked,
                _ => Palette != null ? Palette.GetAt(colorIndex, ColorBlind) : Theme.CellFilled
            };
        }

        // --------------------------------------------------------------------
        // Fallback themes (so the game still has colors if the asset is missing)
        // --------------------------------------------------------------------
        private static ThemeConfig MakeFallbackDark()
        {
            var t = ScriptableObject.CreateInstance<ThemeConfig>();
            t.Mode = ThemeMode.Dark;
            return t;
        }
        private static ThemeConfig MakeFallbackLight()
        {
            var t = ScriptableObject.CreateInstance<ThemeConfig>();
            t.Mode = ThemeMode.Light;
            t.Background = new Color(0.96f, 0.96f, 0.98f, 1f);
            t.Surface = new Color(1f, 1f, 1f, 1f);
            t.SurfaceElevated = new Color(0.94f, 0.94f, 0.96f, 1f);
            t.Primary = new Color(0.35f, 0.25f, 0.85f, 1f);
            t.Accent = new Color(0.90f, 0.30f, 0.55f, 1f);
            t.TextPrimary = new Color(0.10f, 0.10f, 0.15f, 1f);
            t.TextSecondary = new Color(0.35f, 0.35f, 0.45f, 1f);
            t.TextDisabled = new Color(0.60f, 0.60f, 0.70f, 1f);
            t.BoardBackground = new Color(0.92f, 0.92f, 0.94f, 1f);
            t.BoardGrid = new Color(0.85f, 0.85f, 0.88f, 1f);
            t.CellEmpty = new Color(0.98f, 0.98f, 1f, 1f);
            return t;
        }
    }
}
