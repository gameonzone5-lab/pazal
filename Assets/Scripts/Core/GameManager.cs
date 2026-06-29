// ----------------------------------------------------------------------------
// GameManager.cs
// Top-level orchestrator. Holds the live GameState, manages pause / resume,
// and is the public entry point for "Go to menu / start run / end run".
// ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.BlockPuzzle.Core
{
    /// <summary>
    /// Singleton orchestrator. Created at bootstrap. Survives scene loads.
    /// </summary>
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Boot;
        public float RealTimeSinceBoot => Time.realtimeSinceStartup;

        public event Action<GameState, GameState> OnStateChanged;

        // --------------------------------------------------------------------
        // Lifecycle
        // --------------------------------------------------------------------
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // --------------------------------------------------------------------
        // State transitions
        // --------------------------------------------------------------------
        public void TransitionTo(GameState next)
        {
            if (State == next) return;
            var old = State;
            State = next;
            Log.Info("GameManager", $"{old} -> {next}");
            OnStateChanged?.Invoke(old, next);
            EventBus.Publish(new GameStateChangedEvent(old, next));
        }

        public void GoToMainMenu() => TransitionTo(GameState.MainMenu);
        public void GoToModeSelect() => TransitionTo(GameState.ModeSelect);
        public void StartRun() => TransitionTo(GameState.Playing);
        public void PauseRun() => TransitionTo(GameState.Paused);
        public void ResumeRun() => TransitionTo(GameState.Playing);
        public void EndRun(bool victory) => TransitionTo(GameState.GameOver);
        public void ShowResult() => TransitionTo(GameState.Result);
        public void OpenShop() => TransitionTo(GameState.Shop);
        public void OpenSettings() => TransitionTo(GameState.Settings);
        public void QuitGame()
        {
            TransitionTo(GameState.Quit);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_ANDROID
            Application.Quit();
#endif
        }
    }
}
