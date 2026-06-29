// ----------------------------------------------------------------------------
// Board.cs
// Pure data + logic for the puzzle board. No MonoBehaviour, no GameObjects
// (the visual layer reads this and renders). This makes the game 100%
// testable in headless mode and enables cheat-proof cloud validation.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Game.BlockPuzzle.Config;

namespace Game.BlockPuzzle.Board
{
    /// <summary>
    /// Result of attempting to place a piece.
    /// </summary>
    public readonly struct PlacementResult
    {
        public readonly bool Success;
        public readonly List<BoardCoord> PlacedCells;
        public readonly List<BoardCoord> ClearedCells;
        public readonly int LinesCleared;
        public readonly bool IsPerfectClear;

        public PlacementResult(bool success, List<BoardCoord> placed,
            List<BoardCoord> cleared, int lines, bool perfect)
        {
            Success = success;
            PlacedCells = placed;
            ClearedCells = cleared;
            LinesCleared = lines;
            IsPerfectClear = perfect;
        }

        public static PlacementResult Fail() => new PlacementResult(false, null, null, 0, false);
    }

    public readonly struct BoardCoord : IEquatable<BoardCoord>
    {
        public readonly int X;
        public readonly int Y;
        public BoardCoord(int x, int y) { X = x; Y = y; }
        public bool Equals(BoardCoord other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is BoardCoord c && Equals(c);
        public override int GetHashCode() => (X * 397) ^ Y;
        public override string ToString() => $"({X},{Y})";
    }

    /// <summary>
    /// The puzzle board. Pure data; the view layer renders it.
    /// </summary>
    public sealed class Board
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int CellCount => Width * Height;

        private readonly BoardCell[] _cells;

        // --------------------------------------------------------------------
        // Construction
        // --------------------------------------------------------------------
        public Board(int width, int height)
        {
            Width = Math.Max(Constants.MinBoardSize, Math.Min(Constants.MaxBoardSize, width));
            Height = Math.Max(Constants.MinBoardSize, Math.Min(Constants.MaxBoardSize, height));
            _cells = new BoardCell[CellCount];
            ClearAll();
        }

        public void ClearAll()
        {
            for (int i = 0; i < _cells.Length; i++) _cells[i] = BoardCell.Empty;
        }

        // --------------------------------------------------------------------
        // Cell access
        // --------------------------------------------------------------------
        public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;
        public bool InBounds(BoardCoord c) => InBounds(c.X, c.Y);

        public BoardCell Get(int x, int y) => _cells[Index(x, y)];
        public BoardCell Get(BoardCoord c) => Get(c.X, c.Y);

        public void Set(int x, int y, BoardCell cell) => _cells[Index(x, y)] = cell;
        public void Set(BoardCoord c, BoardCell cell) => Set(c.X, c.Y, cell);

        private int Index(int x, int y) => y * Width + x;

        public int FilledCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < _cells.Length; i++)
                    if (_cells[i].IsFilled) n++;
                return n;
            }
        }

        public int EmptyCount => CellCount - FilledCount;

        // --------------------------------------------------------------------
        // Placement validation
        // --------------------------------------------------------------------

        /// <summary>
        /// Check if a shape fits at top-left (x,y). Cells overlapping a Locked
        /// cell are invalid. Frozen / Bomb / Rainbow require the underlying
        /// cell to be empty.
        /// </summary>
        public bool CanPlace(BlockShape shape, int x, int y)
        {
            if (shape == null) return false;
            for (int i = 0; i < shape.Cells.Length; i++)
            {
                int cx = x + shape.Cells[i].X;
                int cy = y + shape.Cells[i].Y;
                if (!InBounds(cx, cy)) return false;
                if (Get(cx, cy).Type != CellType.Empty) return false;
            }
            return true;
        }

        /// <summary>
        /// Try to place a shape. On success, fills cells with Normal-type,
        /// color = piece color, piece id = caller-supplied.
        /// </summary>
        public PlacementResult Place(BlockShape shape, int x, int y, int pieceId, byte colorIndex)
        {
            if (shape == null) return PlacementResult.Fail();
            if (!CanPlace(shape, x, y)) return PlacementResult.Fail();

            var placed = new List<BoardCoord>(shape.Cells.Length);
            for (int i = 0; i < shape.Cells.Length; i++)
            {
                int cx = x + shape.Cells[i].X;
                int cy = y + shape.Cells[i].Y;
                _cells[Index(cx, cy)] = new BoardCell
                {
                    Type = CellType.Normal,
                    ColorIndex = colorIndex,
                    Hits = 0,
                    AdjacentClears = 0,
                    PieceId = pieceId
                };
                placed.Add(new BoardCoord(cx, cy));
            }

            // Detect lines after placement.
            var cleared = new List<BoardCoord>();
            int lines = ClearCompletedLines(cleared);

            // Increment locked adjacency counters around any cell we cleared.
            if (cleared.Count > 0)
                IncrementLockedAdjacency(cleared);

            bool perfect = EmptyCount == 0 && lines > 0;

            return new PlacementResult(true, placed, cleared, lines, perfect);
        }

        // --------------------------------------------------------------------
        // Special cell placement (Bomb, Rainbow, Frozen, Locked)
        // --------------------------------------------------------------------
        public void PlaceSpecial(BoardCoord c, BoardCell cell)
        {
            if (!InBounds(c)) return;
            _cells[Index(c.X, c.Y)] = cell;
        }

        // --------------------------------------------------------------------
        // Line clearing
        // --------------------------------------------------------------------

        /// <summary>
        /// Clears any row or column that is fully filled with non-Empty cells.
        /// Returns the number of lines cleared; cleared cells are appended to
        /// <paramref name="clearedCoords"/>.
        /// </summary>
        public int ClearCompletedLines(List<BoardCoord> clearedCoords)
        {
            if (clearedCoords == null) clearedCoords = new List<BoardCoord>();
            int lines = 0;

            // Rows
            for (int y = 0; y < Height; y++)
            {
                if (IsRowFull(y))
                {
                    for (int x = 0; x < Width; x++)
                    {
                        var c = _cells[Index(x, y)];
                        if (c.Type == CellType.Frozen)
                        {
                            c.Hits++;
                            if (c.Hits >= 2) { _cells[Index(x, y)] = BoardCell.Empty; clearedCoords.Add(new BoardCoord(x, y)); }
                            else { _cells[Index(x, y)] = c; }
                        }
                        else
                        {
                            _cells[Index(x, y)] = BoardCell.Empty;
                            clearedCoords.Add(new BoardCoord(x, y));
                        }
                    }
                    lines++;
                }
            }

            // Columns
            for (int x = 0; x < Width; x++)
            {
                if (IsColumnFull(x))
                {
                    for (int y = 0; y < Height; y++)
                    {
                        var c = _cells[Index(x, y)];
                        if (c.Type == CellType.Frozen)
                        {
                            c.Hits++;
                            if (c.Hits >= 2) { _cells[Index(x, y)] = BoardCell.Empty; clearedCoords.Add(new BoardCoord(x, y)); }
                            else { _cells[Index(x, y)] = c; }
                        }
                        else
                        {
                            _cells[Index(x, y)] = BoardCell.Empty;
                            clearedCoords.Add(new BoardCoord(x, y));
                        }
                    }
                    lines++;
                }
            }
            return lines;
        }

        public bool IsRowFull(int y)
        {
            for (int x = 0; x < Width; x++)
                if (_cells[Index(x, y)].IsEmpty) return false;
            return true;
        }

        public bool IsColumnFull(int x)
        {
            for (int y = 0; y < Height; y++)
                if (_cells[Index(x, y)].IsEmpty) return false;
            return true;
        }

        // --------------------------------------------------------------------
        // Locked cell logic
        // --------------------------------------------------------------------
        private static readonly BoardCoord[] s_Neighbors =
        {
            new BoardCoord(-1, 0), new BoardCoord(1, 0),
            new BoardCoord(0, -1), new BoardCoord(0, 1)
        };

        private void IncrementLockedAdjacency(List<BoardCoord> cleared)
        {
            // Each cleared cell contributes to the AdjacentClears counter of
            // any Locked neighbor (up to 4). When counter reaches 4, the cell
            // becomes Normal (and can be cleared normally on the next pass).
            var seen = new HashSet<BoardCoord>();
            for (int i = 0; i < cleared.Count; i++)
            {
                var c = cleared[i];
                for (int n = 0; n < 4; n++)
                {
                    var nc = new BoardCoord(c.X + s_Neighbors[n].X, c.Y + s_Neighbors[n].Y);
                    if (!InBounds(nc)) continue;
                    if (!seen.Add(nc)) continue;
                    var cell = _cells[Index(nc.X, nc.Y)];
                    if (cell.Type != CellType.Locked) continue;
                    cell.AdjacentClears++;
                    if (cell.AdjacentClears >= 4)
                    {
                        cell.Type = CellType.Normal;
                        cell.AdjacentClears = 0;
                    }
                    _cells[Index(nc.X, nc.Y)] = cell;
                }
            }
        }

        // --------------------------------------------------------------------
        // Special block activation
        // --------------------------------------------------------------------

        /// <summary>Cells affected by a Bomb at (cx,cy). 3x3 area.</summary>
        public void GetBombArea(int cx, int cy, List<BoardCoord> output)
        {
            output.Clear();
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    var c = new BoardCoord(cx + dx, cy + dy);
                    if (InBounds(c)) output.Add(c);
                }
        }

        /// <summary>Cells of a given color, used by Rainbow.</summary>
        public void GetCellsOfColor(byte colorIndex, List<BoardCoord> output)
        {
            output.Clear();
            for (int i = 0; i < _cells.Length; i++)
                if (_cells[i].ColorIndex == colorIndex && _cells[i].IsFilled)
                {
                    output.Add(new BoardCoord(i % Width, i / Width));
                }
        }

        /// <summary>Force-clear cells (no rules, except never touch Locked).</summary>
        public int ForceClear(List<BoardCoord> cells, List<BoardCoord> clearedCoords)
        {
            if (clearedCoords == null) clearedCoords = new List<BoardCoord>();
            int n = 0;
            for (int i = 0; i < cells.Count; i++)
            {
                var c = cells[i];
                if (!InBounds(c)) continue;
                var cell = _cells[Index(c.X, c.Y)];
                if (cell.Type == CellType.Locked) continue;
                _cells[Index(c.X, c.Y)] = BoardCell.Empty;
                clearedCoords.Add(c);
                n++;
            }
            if (clearedCoords.Count > 0) IncrementLockedAdjacency(clearedCoords);
            return n;
        }

        // --------------------------------------------------------------------
        // Game over detection
        // --------------------------------------------------------------------
        public bool CanAnyPieceFit(BlockShapeLibrary lib, IEnumerable<Config.ShapeCell>[] pieceShapes)
        {
            // Replaced by BlockSpawner.AnyFits which knows current pieces.
            return true;
        }

        public IEnumerable<BoardCoord> AllCoords()
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    yield return new BoardCoord(x, y);
        }

        public BoardCell[] Snapshot() => (BoardCell[])_cells.Clone();
    }
}
