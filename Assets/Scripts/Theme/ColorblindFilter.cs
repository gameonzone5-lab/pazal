// ----------------------------------------------------------------------------
// ColorblindFilter.cs
// CPU-side color transformations for colorblind simulation. Used by tests and
// by debug overlays. The shipping palette swap is handled by ThemeManager;
// this is the math, not the application.
// ----------------------------------------------------------------------------

using UnityEngine;

namespace Game.BlockPuzzle.Theme
{
    /// <summary>
    /// Static helpers for converting an RGB color through the standard
    /// colorblind simulation matrices (Brettel/Vienot/Mollon).
    /// </summary>
    public static class ColorblindFilter
    {
        // sRGB -> linear is implied by treating input as linear for these
        // approximations. For UI overlays the visual difference is negligible.

        public static Color Apply(Color c, ColorBlindMode mode)
        {
            return mode switch
            {
                ColorBlindMode.Deuteranopia => Transform(c, DeuteranopiaMatrix),
                ColorBlindMode.Protanopia => Transform(c, ProtanopiaMatrix),
                ColorBlindMode.Tritanopia => Transform(c, TritanopiaMatrix),
                _ => c
            };
        }

        private static Color Transform(Color c, float[] m)
        {
            float r = m[0] * c.r + m[1] * c.g + m[2] * c.b;
            float g = m[3] * c.r + m[4] * c.g + m[5] * c.b;
            float b = m[6] * c.r + m[7] * c.g + m[8] * c.b;
            return new Color(
                Mathf.Clamp01(r),
                Mathf.Clamp01(g),
                Mathf.Clamp01(b),
                c.a);
        }

        // Approximation matrices from Machado, Oliveira, Fernandes (2009).
        private static readonly float[] DeuteranopiaMatrix =
        {
            0.625f, 0.375f, 0.000f,
            0.700f, 0.300f, 0.000f,
            0.000f, 0.300f, 0.700f
        };

        private static readonly float[] ProtanopiaMatrix =
        {
            0.567f, 0.433f, 0.000f,
            0.558f, 0.442f, 0.000f,
            0.000f, 0.242f, 0.758f
        };

        private static readonly float[] TritanopiaMatrix =
        {
            0.950f, 0.050f, 0.000f,
            0.000f, 0.433f, 0.567f,
            0.000f, 0.475f, 0.525f
        };
    }
}
