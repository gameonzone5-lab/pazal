// ----------------------------------------------------------------------------
// BoardController.cs
// MonoBehaviour wrapper for the Board. Hosts the view, exposes the board
// model to other systems, and emits events when the board changes.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Game.BlockPuzzle.Config;
using Game.BlockPuzzle.Core;
using UnityEngine;

namespace Game.BlockPuzzle.Board
{
    /// <summary>
    /// Hosts a single live Board for the current run.
    /// </summary>
    public sealed class BoardController : MonoBehaviour, IService
    {
        public Board.Board Board { get; private set; }

        [SerializeField] private int _width = Constants.DefaultBoardSize;
        [SerializeField] private int _height = Constants.DefaultBoardSize;

        public event Action<PlacementResult> OnPiecePlaced;
        public event Action<List<BoardCoord>> OnCellsCleared;
        public event Action<List<BoardCoord>, List<BoardCoord>> OnSpecialApplied;

        public void Initialize()
        {
            RecreateBoard(_width, _height);
        }

        public void Shutdown()
        {
            Board = null;
        }

        public void RecreateBoard(int width, int height)
        {
            Board = new Board.Board(width, height);
            _width = width;
            _height = height;
        }

        // --------------------------------------------------------------------
        // API used by gameplay
        // --------------------------------------------------------------------

        public PlacementResult TryPlace(BlockShape shape, int x, int y, int pieceId, byte colorIndex)
        {
            if (Board == null) return PlacementResult.Fail();
            var result = Board.Place(shape, x, y, pieceId, colorIndex);
            if (result.Success)
            {
                OnPiecePlaced?.Invoke(result);
                if (result.ClearedCells != null && result.ClearedCells.Count > 0)
                    OnCellsCleared?.Invoke(result.ClearedCells);
            }
            return result;
        }

        public void ApplyBomb(BoardCoord center, int pieceId, byte colorIndex)
        {
            if (Board == null) return;
            var area = new List<BoardCoord>(9);
            Board.GetBombArea(center.X, center.Y, area);
            var cleared = new List<BoardCoord>();
            Board.ForceClear(area, cleared);
            OnSpecialApplied?.Invoke(area, cleared);
            OnCellsCleared?.Invoke(cleared);
        }

        public void ApplyRainbow(BoardCoord center, int pieceId, byte colorIndex)
        {
            if (Board == null) return;
            var colored = new List<BoardCoord>(Board.Board.CellCount);
            Board.GetCellsOfColor(colorIndex, colored);
            var cleared = new List<BoardCoord>();
            Board.ForceClear(colored, cleared);
            OnSpecialApplied?.Invoke(colored, cleared);
            OnCellsCleared?.Invoke(cleared);
        }

        public void ApplyLocked(BoardCoord coord)
        {
            if (Board == null) return;
            Board.PlaceSpecial(coord, new BoardCell
            {
                Type = CellType.Locked,
                ColorIndex = 0,
                Hits = 0,
                AdjacentClears = 0,
                PieceId = 0
            });
        }

        public void ApplyFrozen(BoardCoord coord, byte colorIndex)
        {
            if (Board == null) return;
            Board.PlaceSpecial(coord, new BoardCell
            {
                Type = CellType.Frozen,
                ColorIndex = colorIndex,
                Hits = 0,
                AdjacentClears = 0,
                PieceId = 0
            });
        }

        public bool IsFilledCountAt(int target) => Board != null && Board.FilledCount >= target;
    }
}
