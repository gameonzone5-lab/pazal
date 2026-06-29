// ----------------------------------------------------------------------------
// AnimationSequencer.cs
// Coordinates short "scene" animations: screen shake on combo, confetti on
// perfect clear, etc. Drives a small set of tweens that play in parallel.
// ----------------------------------------------------------------------------

using System.Collections;
using UnityEngine;

namespace Game.BlockPuzzle.Visuals
{
    public sealed class AnimationSequencer : MonoBehaviour, IService
    {
        [SerializeField] private float _shakeDuration = 0.25f;
        [SerializeField] private float _shakeMagnitude = 8f;
        [SerializeField] private Transform _shakeTarget;

        public void Initialize() { }
        public void Shutdown() { }

        public void ScreenShake(float magnitude = 1f)
        {
            if (_shakeTarget == null) return;
            StopAllCoroutines();
            StartCoroutine(ShakeRoutine(magnitude));
        }

        private IEnumerator ShakeRoutine(float mult)
        {
            Vector3 original = _shakeTarget.localPosition;
            float t = 0f;
            while (t < _shakeDuration)
            {
                t += Time.deltaTime;
                float k = 1f - (t / _shakeDuration);
                var offset = Random.insideUnitSphere * (_shakeMagnitude * mult * k);
                offset.z = 0f;
                _shakeTarget.localPosition = original + offset;
                yield return null;
            }
            _shakeTarget.localPosition = original;
        }

        public void PopIn(Transform t, float duration = 0.2f, float overshoot = 1.1f)
        {
            if (t == null) return;
            StartCoroutine(PopRoutine(t, duration, overshoot));
        }

        private IEnumerator PopRoutine(Transform t, float duration, float overshoot)
        {
            Vector3 start = Vector3.zero;
            Vector3 end = Vector3.one;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float k = elapsed / duration;
                float eased = k < 0.7f ? (k / 0.7f) * overshoot : overshoot - (k - 0.7f) / 0.3f * (overshoot - 1f);
                t.localScale = Vector3.LerpUnclamped(start, end, eased);
                yield return null;
            }
            t.localScale = end;
        }
    }
}
