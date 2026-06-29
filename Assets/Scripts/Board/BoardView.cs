// ----------------------------------------------------------------------------
// BoardView.cs
// Visual layer for the Board. Listens to BoardController events and animates
// cell state changes. Uses simple Image-based cells so the game ships without
// requiring external sprite assets.
//
// Cell visuals are pooled; we never instantiate a new cell once the pool is
// filled. This is critical for low-RAM devices.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace Game.BlockPuzzle.Board
{
    /// <summary>
    /// Renders the board. Pool of Image components per cell.
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private RectTransform _boardRect;
        [SerializeField] private GridLayoutGroup _grid;
        [SerializeField] private CellView _cellPrefab;

        private readonly Dictionary<BoardCoord, CellView> _views = new();
        private readonly Stack<CellView> _pool = new();
        private BoardController _controller;
        private float _cellSize;

        private void Awake()
        {
            if (_boardRect == null) _boardRect = (RectTransform)transform;
            if (_grid == null) _grid = GetComponentInChildren<GridLayoutGroup>(true);
        }

        public void Bind(BoardController controller)
        {
            if (_controller != null)
            {
                _controller.OnPiecePlaced -= HandlePiecePlaced;
                _controller.OnCellsCleared -= HandleCellsCleared;
                _controller.OnSpecialApplied -= HandleSpecialApplied;
            }
            _controller = controller;
            _controller.OnPiecePlaced += HandlePiecePlaced;
            _controller.OnCellsCleared += HandleCellsCleared;
            _controller.OnSpecialApplied += HandleSpecialApplied;
            Rebuild();
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.OnPiecePlaced -= HandlePiecePlaced;
                _controller.OnCellsCleared -= HandleCellsCleared;
                _controller.OnSpecialApplied -= HandleSpecialApplied;
            }
        }

        public void Rebuild()
        {
            if (_controller?.Board == null) return;
            int w = _controller.Board.Width;
            int h = _controller.Board.Height;
            ConfigureGrid(w, h);
            // Clear existing views
            foreach (var kv in _views) ReturnToPool(kv.Value);
            _views.Clear();

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var view = RentCell();
                    view.transform.SetParent(_grid.transform, false);
                    view.SetCoord(new BoardCoord(x, y));
                    var cell = _controller.Board.Get(x, y);
                    view.SetState(cell, ThemeManager.Instance);
                    _views[new BoardCoord(x, y)] = view;
                }
            }
        }

        private void ConfigureGrid(int w, int h)
        {
            if (_grid == null) return;
            var parent = _grid.transform as RectTransform;
            float pw = parent.rect.width;
            float ph = parent.rect.height;
            // Use square cells that fit both dimensions.
            float size = Mathf.Floor(Mathf.Min(pw / w, ph / h));
            _cellSize = size;
            _grid.cellSize = new Vector2(size, size);
            _grid.spacing = Vector2.zero;
            _grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _grid.constraintCount = w;
        }

        private CellView RentCell()
        {
            if (_pool.Count > 0)
            {
                var c = _pool.Pop();
                c.gameObject.SetActive(true);
                return c;
            }
            var inst = Instantiate(_cellPrefab, _grid.transform);
            return inst;
        }

        private void ReturnToPool(CellView view)
        {
            view.gameObject.SetActive(false);
            view.transform.SetParent(transform, false);
            _pool.Push(view);
        }

        private void HandlePiecePlaced(PlacementResult result)
        {
            if (result.PlacedCells == null) return;
            for (int i = 0; i < result.PlacedCells.Count; i++)
            {
                var c = result.PlacedCells[i];
                if (_views.TryGetValue(c, out var view))
                    view.SetState(_controller.Board.Get(c), ThemeManager.Instance);
            }
        }

        private void HandleCellsCleared(List<BoardCoord> cells)
        {
            if (cells == null) return;
            // Stagger the cell-clear animation so lines feel like a sweep.
            for (int i = 0; i < cells.Count; i++)
            {
                var c = cells[i];
                if (_views.TryGetValue(c, out var view))
                    view.PlayClearAnimation();
            }
        }

        private void HandleSpecialApplied(List<BoardCoord> area, List<BoardCoord> cleared)
        {
            if (area == null) return;
            for (int i = 0; i < area.Count; i++)
            {
                var c = area[i];
                if (_views.TryGetValue(c, out var view))
                    view.SetState(_controller.Board.Get(c), ThemeManager.Instance);
            }
        }

        public Vector2 GetCellWorldPosition(BoardCoord c)
        {
            if (!_views.TryGetValue(c, out var view)) return transform.position;
            var rt = view.transform as RectTransform;
            return rt.position;
        }

        public BoardCoord WorldToCell(Vector2 world)
        {
            if (_controller?.Board == null) return new BoardCoord(-1, -1);
            var local = transform.InverseTransformPoint(world);
            int x = Mathf.FloorToInt(local.x / _cellSize);
            int y = Mathf.FloorToInt(-local.y / _cellSize);
            return new BoardCoord(x, y);
        }
    }
}
