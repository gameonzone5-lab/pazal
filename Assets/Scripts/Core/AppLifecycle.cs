// ----------------------------------------------------------------------------
// AppLifecycle.cs
// Handles application pause / focus / quit. Persists on pause, resumes music
// on regain of focus.
// ----------------------------------------------------------------------------

using UnityEngine;

namespace Game.BlockPuzzle.Core
{
    public sealed class AppLifecycle : MonoBehaviour
    {
        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                Log.Info("Lifecycle", "Application paused -> save & mute audio");
                ServiceLocator.TryResolve<Save.SaveManager>()?.Flush();
                ServiceLocator.TryResolve<Audio.AudioManager>()?.OnAppPaused();
            }
            else
            {
                Log.Info("Lifecycle", "Application resumed -> restore audio");
                ServiceLocator.TryResolve<Audio.AudioManager>()?.OnAppResumed();
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            // Treat focus loss same as pause for audio, but lighter weight.
            if (!focus)
                ServiceLocator.TryResolve<Audio.AudioManager>()?.MuteForFocusLoss(true);
            else
                ServiceLocator.TryResolve<Audio.AudioManager>()?.MuteForFocusLoss(false);
        }

        private void OnApplicationQuit()
        {
            Log.Info("Lifecycle", "OnApplicationQuit");
            ServiceLocator.TryResolve<Save.SaveManager>()?.Flush();
            ServiceLocator.TryResolve<Cloud.CloudSaveManager>()?.OnQuit();
        }
    }
}
