// ----------------------------------------------------------------------------
// PauseMenuController.cs
// Pause overlay: Resume, Restart, Settings, Quit to Menu.
// ----------------------------------------------------------------------------

using Game.BlockPuzzle.Audio;
using Game.BlockPuzzle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Game.BlockPuzzle.UI
{
    public sealed class PauseMenuController : MonoBehaviour
    {
        public Button ResumeButton;
        public Button RestartButton;
        public Button SettingsButton;
        public Button QuitButton;

        private void OnEnable()
        {
            Bind(ResumeButton, () => GameManager.Instance.ResumeRun());
            Bind(RestartButton, Restart);
            Bind(SettingsButton, () => GameManager.Instance.OpenSettings());
            Bind(QuitButton, () => GameManager.Instance.GoToMainMenu());
        }

        private void Restart()
        {
            // Re-issue the current mode's Begin.
            var mode = ServiceLocator.TryResolve<Modes.IGameMode>();
            mode?.Begin();
        }

        private static void Bind(Button b, System.Action act)
        {
            if (b == null) return;
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => { SFXPlayer.Play(SFXPlayer.Click); act?.Invoke(); });
        }
    }
}
