// ----------------------------------------------------------------------------
// CellView.cs
// Single-cell visual. Listens for state changes and animates.
// ----------------------------------------------------------------------------

using System.Collections;
using Game.BlockPuzzle.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace Game.BlockPuzzle.Board
{
    /// <summary>
    /// Visual representation of a single board cell.
    /// </summary>
    public sealed class CellView : MonoBehaviour
    {
        [SerializeField] private Image _bg;
        [SerializeField] private Image _accent;
        [SerializeField] private Image _frostOverlay;
        [SerializeField] private Image _lockIcon;

        public BoardCoord Coord { get; private set; }

        private Coroutine _clearAnim;

        public void SetCoord(BoardCoord c) => Coord = c;

        public void SetState(BoardCell cell, ThemeManager theme)
        {
            if (_bg != null)
                _bg.color = cell.IsFilled
                    ? theme.Palette.GetAt(cell.ColorIndex, theme.ColorBlind)
                    : theme.Theme.CellEmpty;

            // Special overlays
            if (_frostOverlay != null)
            {
                bool showFrost = cell.Type == CellType.Frozen || cell.Type == CellType.IceCracked;
                _frostOverlay.gameObject.SetActive(showFrost);
                if (showFrost) _frostOverlay.color = theme.Theme.CellFrozen;
            }
            if (_lockIcon != null)
            {
                _lockIcon.gameObject.SetActive(cell.Type == CellType.Locked);
                if (_lockIcon.gameObject.activeSelf)
                    _lockIcon.color = theme.Theme.CellLocked;
            }
            if (_accent != null)
            {
                bool isSpecial = cell.Type == CellType.Bomb
                    || cell.Type == CellType.Rainbow
                    || cell.Type == CellType.Locked;
                _accent.gameObject.SetActive(isSpecial);
                if (isSpecial)
                {
                    _accent.color = cell.Type == CellType.Bomb ? theme.Theme.CellBomb
                                  : cell.Type == CellType.Rainbow ? theme.Theme.CellRainbow
                                  : theme.Theme.CellLocked;
                }
            }
        }

        public void PlayClearAnimation()
        {
            if (_clearAnim != null) StopCoroutine(_clearAnim);
            _clearAnim = StartCoroutine(ClearRoutine());
        }

        private IEnumerator ClearRoutine()
        {
            if (_bg == null) yield break;
            var original = _bg.color;
            var end = new Color(original.r, original.g, original.b, 0f);
            float t = 0f;
            const float duration = 0.18f;
            while (t < duration)
            {
                t += Time.deltaTime;
                _bg.color = Color.Lerp(original, end, t / duration);
                transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.6f, t / duration);
                yield return null;
            }
            _bg.color = ThemeManager.Instance.Theme.CellEmpty;
            transform.localScale = Vector3.one;
            _clearAnim = null;
        }

        public void PlayBombFx()
        {
            if (_clearAnim != null) StopCoroutine(_clearAnim);
            _clearAnim = StartCoroutine(BombRoutine());
        }

        private IEnumerator BombRoutine()
        {
            if (_bg == null) yield break;
            var flash = ThemeManager.Instance.Theme.LineClearFx;
            var original = _bg.color;
            float t = 0f;
            const float duration = 0.35f;
            while (t < duration)
            {
                t += Time.deltaTime;
                _bg.color = Color.Lerp(flash, original, t / duration);
                transform.localScale = Vector3.one * (1f + Mathf.Sin(t / duration * Mathf.PI) * 0.25f);
                yield return null;
            }
            _bg.color = ThemeManager.Instance.Theme.CellEmpty;
            transform.localScale = Vector3.one;
            _clearAnim = null;
        }
    }
}
