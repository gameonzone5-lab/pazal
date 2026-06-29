// ----------------------------------------------------------------------------
// AudioManager.cs
// Top-level audio orchestrator. Plays music + SFX via the mixer; routes UI /
// gameplay / ambient categories to their own AudioSource pools; respects
// focus-loss muting.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using Game.BlockPuzzle.Config;
using Game.BlockPuzzle.Core;
using UnityEngine;

namespace Game.BlockPuzzle.Audio
{
    public sealed class AudioManager : MonoBehaviour, IService
    {
        [SerializeField] private int _sfxPoolSize = 12;

        private AudioConfig _config;
        private AudioSource _musicSource;
        private AudioSource _musicSourceSecondary; // for crossfade
        private readonly List<AudioSource> _sfxPool = new();
        private bool _focusLost;
        private bool _paused;

        public void Initialize()
        {
            _config = GameConfig.Instance != null ? GameConfig.Instance.Audio : null;
            EnsureMusicSources();
            EnsureSfxPool();
            ApplyVolumes();
        }

        public void InitializeBanks() { /* loaded via Resources — placeholder */ }

        public void Shutdown() { }

        private void EnsureMusicSources()
        {
            if (_musicSource == null)
            {
                var go = new GameObject("Music_A");
                go.transform.SetParent(transform, false);
                _musicSource = go.AddComponent<AudioSource>();
                _musicSource.playOnAwake = false;
                _musicSource.loop = true;
                _musicSource.spatialBlend = 0f;
            }
            if (_musicSourceSecondary == null)
            {
                var go = new GameObject("Music_B");
                go.transform.SetParent(transform, false);
                _musicSourceSecondary = go.AddComponent<AudioSource>();
                _musicSourceSecondary.playOnAwake = false;
                _musicSourceSecondary.loop = true;
                _musicSourceSecondary.spatialBlend = 0f;
            }
        }

        private void EnsureSfxPool()
        {
            while (_sfxPool.Count < _sfxPoolSize)
            {
                var go = new GameObject($"Sfx_{_sfxPool.Count}");
                go.transform.SetParent(transform, false);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0f;
                _sfxPool.Add(src);
            }
        }

        public void ApplyVolumes()
        {
            if (_config == null) return;
            var master = _config.MasterVolume;
            _musicSource.volume = master * _config.MusicVolume;
            _musicSourceSecondary.volume = 0f;
            foreach (var src in _sfxPool)
                src.volume = master * _config.SfxVolume;
        }

        // --------------------------------------------------------------------
        // Public API
        // --------------------------------------------------------------------

        public void Play(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            var clip = _config?.FindById(id);
            if (clip == null || clip.Clip == null) return;
            var src = RentSfx();
            if (src == null) return;
            src.clip = clip.Clip;
            src.volume = (_config?.MasterVolume ?? 1f) * (_config?.SfxVolume ?? 1f) * clip.Volume;
            src.pitch = clip.RandomizePitch
                ? Random.Range(clip.PitchRange.x, clip.PitchRange.y)
                : clip.Pitch;
            src.loop = false;
            src.Play();
        }

        public void PlayMusic(string id, float fadeSeconds = 1.5f)
        {
            if (_config == null) return;
            var clip = _config.FindById(id);
            if (clip == null || clip.Clip == null) return;
            _ = CrossfadeTo(clip.Clip, fadeSeconds);
        }

        public void StopMusic(float fadeSeconds = 0.5f)
        {
            _ = FadeOut(_musicSource, fadeSeconds);
            _ = FadeOut(_musicSourceSecondary, fadeSeconds);
        }

        public void OnAppPaused()
        {
            _paused = true;
            AudioListener.pause = true;
        }

        public void OnAppResumed()
        {
            _paused = false;
            if (!_focusLost) AudioListener.pause = false;
            ApplyVolumes();
        }

        public void MuteForFocusLoss(bool muted)
        {
            _focusLost = muted;
            if (_paused) return;
            AudioListener.pause = muted;
        }

        // --------------------------------------------------------------------
        // Internals
        // --------------------------------------------------------------------

        private AudioSource RentSfx()
        {
            for (int i = 0; i < _sfxPool.Count; i++)
                if (!_sfxPool[i].isPlaying) return _sfxPool[i];
            return _sfxPool.Count > 0 ? _sfxPool[0] : null;
        }

        private async Awaitable CrossfadeTo(AudioClip clip, float seconds)
        {
            var outgoing = _musicSource.isPlaying ? _musicSource : _musicSourceSecondary;
            var incoming = _musicSource.isPlaying ? _musicSourceSecondary : _musicSource;

            incoming.clip = clip;
            incoming.volume = 0f;
            incoming.Play();
            float t = 0f;
            float startOut = outgoing.volume;
            float targetIn = (_config?.MasterVolume ?? 1f) * (_config?.MusicVolume ?? 0.7f);
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                float k = t / seconds;
                outgoing.volume = Mathf.Lerp(startOut, 0f, k);
                incoming.volume = Mathf.Lerp(0f, targetIn, k);
                await Awaitable.NextFrameAsync();
            }
            outgoing.Stop();
            incoming.volume = targetIn;
            // Swap roles so subsequent crossfade targets the other source.
            (_musicSource, _musicSourceSecondary) = (_musicSourceSecondary, _musicSource);
        }

        private async Awaitable FadeOut(AudioSource src, float seconds)
        {
            if (src == null || !src.isPlaying) return;
            float start = src.volume;
            float t = 0f;
            while (t < seconds && src.isPlaying)
            {
                t += Time.unscaledDeltaTime;
                src.volume = Mathf.Lerp(start, 0f, t / seconds);
                await Awaitable.NextFrameAsync();
            }
            src.Stop();
            src.volume = 0f;
        }
    }
}
