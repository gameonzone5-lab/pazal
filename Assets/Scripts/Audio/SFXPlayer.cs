// ----------------------------------------------------------------------------
// SFXPlayer.cs
// Thin facade over AudioManager for one-shot SFX playback. Add named entries
// to the AudioConfig UIBank for "click", "pop", "close", etc.
// ----------------------------------------------------------------------------

using Game.BlockPuzzle.Core;
using UnityEngine;

namespace Game.BlockPuzzle.Audio
{
    public static class SFXPlayer
    {
        public static void Play(string id)
        {
            ServiceLocator.TryResolve<AudioManager>()?.Play(id);
        }

        public static void SetVolume(float volume01)
        {
            var mgr = ServiceLocator.TryResolve<AudioManager>();
            if (mgr == null) return;
            var cfg = Config.GameConfig.Instance?.Audio;
            if (cfg != null) cfg.SfxVolume = Mathf.Clamp01(volume01);
            mgr.ApplyVolumes();
        }

        // Common one-shot ids used across the codebase. Keep these in sync
        // with the AudioConfig UIBank / GameplayBank asset.
        public const string Click = "click";
        public const string Pop = "pop";
        public const string Place = "place";
        public const string PlaceWithClear = "place_with_clear";
        public const string BombExplode = "bomb_explode";
        public const string RainbowClear = "rainbow_clear";
        public const string GameOver = "game_over";
        public const string Reward = "reward";
        public const string CoinTick = "coin_tick";
        public const string Purchase = "purchase";
    }
}
