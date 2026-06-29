// ----------------------------------------------------------------------------
// HapticManager.cs
// Thin wrapper over Android's Vibrator / VibrationEffect. iOS uses the
// AudioServicesPlaySystemSound(kSystemSoundID_Vibrate) API; in Unity that
// maps to Handheld.Vibrate(). We expose semantic helpers (Select, Impact,
// Success) so gameplay code doesn't have to know amplitudes.
// ----------------------------------------------------------------------------

using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Save;
using UnityEngine;

namespace Game.BlockPuzzle.Utils
{
    public enum HapticKind { Light, Medium, Heavy, Success, Failure }

    public sealed class HapticManager : MonoBehaviour, IService
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        private AndroidJavaClass _vibratorClass;
        private AndroidJavaObject _vibrator;
#endif

        public void Initialize()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                _vibratorClass = new AndroidJavaClass("android.os.Vibrator");
                _vibrator = _vibratorClass.CallStatic<AndroidJavaObject>("getDefault");
            }
            catch { /* device without vibrator */ }
#endif
        }

        public void Shutdown()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            _vibrator?.Dispose();
            _vibratorClass?.Dispose();
#endif
        }

        private bool IsEnabled()
        {
            var save = ServiceLocator.TryResolve<SaveManager>();
            return save?.Current.Settings.HapticsEnabled ?? true;
        }

        public void Select() => Play(HapticKind.Light);
        public void Impact() => Play(HapticKind.Medium);
        public void Heavy() => Play(HapticKind.Heavy);
        public void Success() => Play(HapticKind.Success);
        public void Failure() => Play(HapticKind.Failure);

        public void Play(HapticKind kind)
        {
            if (!IsEnabled()) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            int ms = kind switch
            {
                HapticKind.Light => 10,
                HapticKind.Medium => 25,
                HapticKind.Heavy => 50,
                HapticKind.Success => 30,
                HapticKind.Failure => 60,
                _ => 20
            };
            try
            {
                using var effectClass = new AndroidJavaClass("android.os.VibrationEffect");
                using var effect = effectClass.CallStatic<AndroidJavaObject>(
                    "createOneShot", ms,
                    kind == HapticKind.Heavy || kind == HapticKind.Failure ? 255 : 128);
                _vibrator?.Call("vibrate", effect);
            }
            catch
            {
                Handheld.Vibrate();
            }
#else
            Handheld.Vibrate();
#endif
        }
    }
}
