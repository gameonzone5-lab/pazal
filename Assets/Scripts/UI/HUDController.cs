// ----------------------------------------------------------------------------
// HUDController.cs
// In-game heads-up display: score, combo, best score, pause button, hint.
// Subscribes to gameplay events and animates numeric transitions.
// ----------------------------------------------------------------------------

using System.Collections;
using Game.BlockPuzzle.Audio;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Scoring;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.BlockPuzzle.UI
{
    public sealed class HUDController : MonoBehaviour
    {
        [Header("Read-outs")]
        public TMP_Text ScoreLabel;
        public TMP_Text BestLabel;
        public TMP_Text ComboLabel;
        public TMP_Text ModeLabel;
        public TMP_Text TimerLabel;
        public TMP_Text LinesLabel;

        [Header("Buttons")]
        public Button PauseButton;
        public Button RotateButton;

        [Header("Combo")]
        public GameObject ComboBadge;
        public CanvasGroup ComboGroup;

        private int _displayedScore;
        private Coroutine _tween;

        private void OnEnable()
        {
            EventBus.Subscribe<ScoreChangedEvent>(OnScore);
            EventBus.Subscribe<ComboChangedEvent>(OnCombo);
            EventBus.Subscribe<GameStateChangedEvent>(OnState);
            Bind(PauseButton, () => GameManager.Instance.PauseRun());
            Bind(RotateButton, () => Debug.Log("rotate requested (handled by piece)"));
            if (ComboBadge != null) ComboBadge.SetActive(false);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ScoreChangedEvent>(OnScore);
            EventBus.Unsubscribe<ComboChangedEvent>(OnCombo);
            EventBus.Unsubscribe<GameStateChangedEvent>(OnState);
        }

        private void OnState(GameStateChangedEvent evt)
        {
            if (evt.NewState == GameState.Playing) RefreshAll();
            if (TimerLabel != null) TimerLabel.gameObject.SetActive(evt.NewState == GameState.Playing);
        }

        private void OnScore(ScoreChangedEvent evt)
        {
            if (ScoreLabel == null) return;
            if (_tween != null) StopCoroutine(_tween);
            _tween = StartCoroutine(TweenScore(_displayedScore, evt.NewScore));
        }

        private IEnumerator TweenScore(int from, int to)
        {
            const float duration = 0.4f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                int v = Mathf.RoundToInt(Mathf.Lerp(from, to, t / duration));
                _displayedScore = v;
                ScoreLabel.text = v.ToString("N0");
                yield return null;
            }
            _displayedScore = to;
            ScoreLabel.text = to.ToString("N0");
            _tween = null;
        }

        private void OnCombo(ComboChangedEvent evt)
        {
            if (ComboLabel == null) return;
            if (evt.Combo < 2)
            {
                if (ComboBadge != null) ComboBadge.SetActive(false);
                return;
            }
            if (ComboBadge != null) ComboBadge.SetActive(true);
            ComboLabel.text = $"x{evt.Multiplier}";
            if (ComboGroup != null) StartCoroutine(Pulse(ComboGroup));
            SFXPlayer.Play("combo");
        }

        private IEnumerator Pulse(CanvasGroup group)
        {
            float t = 0f;
            const float dur = 0.25f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.Sin((t / dur) * Mathf.PI);
                group.alpha = 0.6f + k * 0.4f;
                group.transform.localScale = Vector3.one * (1f + k * 0.15f);
                yield return null;
            }
            group.alpha = 1f;
            group.transform.localScale = Vector3.one;
        }

        private void RefreshAll()
        {
            var score = ServiceLocator.Resolve<ScoreManager>();
            if (ScoreLabel != null) ScoreLabel.text = score.Score.ToString("N0");
            if (BestLabel != null) BestLabel.text = $"Best {score.HighScore:N0}";
            _displayedScore = score.Score;
        }

        private void Bind(Button b, System.Action act)
        {
            if (b == null) return;
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => { SFXPlayer.Play(SFXPlayer.Click); act?.Invoke(); });
        }

        public void SetTimer(float seconds)
        {
            if (TimerLabel == null) return;
            int s = Mathf.CeilToInt(seconds);
            int m = s / 60;
            int sec = s % 60;
            TimerLabel.text = $"{m:00}:{sec:00}";
            if (s <= 10) TimerLabel.color = Color.red;
        }

        public void SetLines(int current, int target)
        {
            if (LinesLabel != null) LinesLabel.text = $"Lines {current}/{target}";
        }
    }
}
