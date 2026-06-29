// ----------------------------------------------------------------------------
// ScreenEffects.cs
// Top-level full-screen effects: flash, vignette pulse, dim. Cheap overlays
// driven by CanvasGroups.
// ----------------------------------------------------------------------------

using System.Collections;
using UnityEngine;

namespace Game.BlockPuzzle.Visuals
{
    public sealed class ScreenEffects : MonoBehaviour, IService
    {
        [SerializeField] private CanvasGroup _flashOverlay;
        [SerializeField] private CanvasGroup _dimOverlay;

        public void Initialize() { }
        public void Shutdown() { }

        public void Flash(Color color, float duration = 0.15f, float peakAlpha = 0.6f)
        {
            if (_flashOverlay == null) return;
            StopAllCoroutines();
            StartCoroutine(FlashRoutine(color, duration, peakAlpha));
        }

        private IEnumerator FlashRoutine(Color color, float duration, float peak)
        {
            var img = _flashOverlay.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.color = color;
            _flashOverlay.alpha = peak;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                _flashOverlay.alpha = Mathf.Lerp(peak, 0f, t / duration);
                yield return null;
            }
            _flashOverlay.alpha = 0f;
        }

        public void SetDimmed(bool dimmed)
        {
            if (_dimOverlay == null) return;
            _dimOverlay.alpha = dimmed ? 0.6f : 0f;
            _dimOverlay.blocksRaycasts = dimmed;
        }
    }
}
