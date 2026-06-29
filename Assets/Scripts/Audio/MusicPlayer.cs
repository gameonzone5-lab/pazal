// ----------------------------------------------------------------------------
// MusicPlayer.cs
// Thin facade over AudioManager for music control. Lets UI / scenes request
// music changes without holding a reference to the AudioManager directly.
// ----------------------------------------------------------------------------

using Game.BlockPuzzle.Core;
using UnityEngine;

namespace Game.BlockPuzzle.Audio
{
    public static class MusicPlayer
    {
        public static void Play(string id, float fadeSeconds = 1.5f)
        {
            ServiceLocator.TryResolve<AudioManager>()?.PlayMusic(id, fadeSeconds);
        }

        public static void Stop(float fadeSeconds = 0.5f)
        {
            ServiceLocator.TryResolve<AudioManager>()?.StopMusic(fadeSeconds);
        }

        public static void SetVolume(float volume01)
        {
            var mgr = ServiceLocator.TryResolve<AudioManager>();
            if (mgr == null) return;
            var cfg = Config.GameConfig.Instance?.Audio;
            if (cfg != null) cfg.MusicVolume = Mathf.Clamp01(volume01);
            mgr.ApplyVolumes();
        }
    }
}
