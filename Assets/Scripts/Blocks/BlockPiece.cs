// ----------------------------------------------------------------------------
// BlockPiece.cs
// Data + visual representation of a single draggable piece. Pieces are
// runtime-generated (the spawner picks a Shape from the library) and live
// in one of the piece slots at the bottom of the screen.
//
// Pieces support optional rotation (90° steps) and mirroring, if the shape
// permits. The drag controller moves them in screen space and snaps them to
// the board on release.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using Game.BlockPuzzle.Board;
using Game.BlockPuzzle.Config;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Theme;
using UnityEngine;

namespace Game.BlockPuzzle.Blocks
{
    /// <summary>
    /// Visual + data for one piece. One component per piece slot.
    /// </summary>
    public sealed class BlockPiece : MonoBehaviour
    {
        public enum PieceKind { Normal, Bomb, Rainbow }

        public PieceKind Kind = PieceKind.Normal;
        public BlockShape Shape;
        public byte ColorIndex;
        public int PieceId;

        [SerializeField] private Transform _cellRoot;
        [SerializeField] private GameObject _cellPrefab;

        private readonly List<GameObject> _spawnedCells = new();
        private bool _isDragging;

        public bool IsDragging
        {
            get => _isDragging;
            set { _isDragging = value; gameObject.SetActive(!value || true); }
        }

        public void Setup(BlockShape shape, byte colorIndex, int pieceId, PieceKind kind = PieceKind.Normal)
        {
            Shape = shape;
            ColorIndex = colorIndex;
            PieceId = pieceId;
            Kind = kind;
            RebuildVisual();
        }

        public void RebuildVisual()
        {
            if (_cellPrefab == null || _cellRoot == null) return;
            foreach (var go in _spawnedCells) Destroy(go);
            _spawnedCells.Clear();

            if (Shape == null || Shape.Cells == null) return;

            var theme = ThemeManager.Instance;
            Color color = Kind switch
            {
                PieceKind.Bomb => theme.Theme.CellBomb,
                PieceKind.Rainbow => theme.Theme.CellRainbow,
                _ => theme.Palette.GetAt(ColorIndex, theme.ColorBlind)
            };

            for (int i = 0; i < Shape.Cells.Length; i++)
            {
                var go = Instantiate(_cellPrefab, _cellRoot);
                var rt = (RectTransform)go.transform;
                rt.anchoredPosition = new Vector2(Shape.Cells[i].X * 32f, -Shape.Cells[i].Y * 32f);
                var img = go.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.color = color;
                _spawnedCells.Add(go);
            }
        }

        public void SetColorTint(byte newColorIndex)
        {
            ColorIndex = newColorIndex;
            RebuildVisual();
        }

        public IEnumerable<BoardCoord> GetCellsAt(int x, int y)
        {
            if (Shape == null) yield break;
            for (int i = 0; i < Shape.Cells.Length; i++)
                yield return new BoardCoord(x + Shape.Cells[i].X, y + Shape.Cells[i].Y);
        }

        public void OnPlaced()
        {
            // Visually mark piece as used. Spawner will refill slot.
            if (_cellRoot != null) _cellRoot.gameObject.SetActive(false);
        }
    }
}
