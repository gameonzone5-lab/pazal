// ----------------------------------------------------------------------------
// AudioConfig.cs
// All audio resources. Sound ids are referenced as strings throughout the
// codebase to keep designers in charge.
// ----------------------------------------------------------------------------

using System;
using UnityEngine;

namespace Game.BlockPuzzle.Config
{
    public enum AudioCategory { Music, SFX, UI, Voice }

    [CreateAssetMenu(menuName = "BlockCraft/Audio Clip", fileName = "SfxClip")]
    public sealed class SfxClip : ScriptableObject
    {
        public string Id;
        public AudioCategory Category;
        public AudioClip Clip;
        [Range(0f, 1f)] public float Volume = 1f;
        [Range(0.1f, 3f)] public float Pitch = 1f;
        public bool RandomizePitch;
        public Vector2 PitchRange = new Vector2(0.95f, 1.05f);
        public bool Loop;
    }

    [CreateAssetMenu(menuName = "BlockCraft/Audio Bank", fileName = "AudioBank")]
    public sealed class AudioBank : ScriptableObject
    {
        public SfxClip[] Clips;
    }

    /// <summary>
    /// Master config — references every bank and the master mixer volumes.
    /// </summary>
    [CreateAssetMenu(menuName = "BlockCraft/Audio Config", fileName = "AudioConfig")]
    public sealed class AudioConfig : ScriptableObject
    {
        [Header("Banks")]
        public AudioBank GameplayBank;
        public AudioBank UIBank;
        public AudioBank MusicBank;

        [Header("Mix (0..1)")]
        [Range(0f, 1f)] public float MasterVolume = 1f;
        [Range(0f, 1f)] public float MusicVolume = 0.7f;
        [Range(0f, 1f)] public float SfxVolume = 1f;
        [Range(0f, 1f)] public float UiVolume = 1f;

        [Header("Defaults")]
        public bool MuteOnFocusLoss = false;

        /// <summary>Lookup an SfxClip by id across all banks.</summary>
        public SfxClip FindById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (GameplayBank != null)
                for (int i = 0; i < GameplayBank.Clips.Length; i++)
                    if (GameplayBank.Clips[i].Id == id) return GameplayBank.Clips[i];
            if (UIBank != null)
                for (int i = 0; i < UIBank.Clips.Length; i++)
                    if (UIBank.Clips[i].Id == id) return UIBank.Clips[i];
            if (MusicBank != null)
                for (int i = 0; i < MusicBank.Clips.Length; i++)
                    if (MusicBank.Clips[i].Id == id) return MusicBank.Clips[i];
            return null;
        }
    }
}
