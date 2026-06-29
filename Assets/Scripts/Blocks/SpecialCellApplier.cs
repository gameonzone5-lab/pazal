// ----------------------------------------------------------------------------
// SpecialCellApplier.cs
// Centralizes the rules for triggering bomb / rainbow / frozen effects.
// The board controller delegates here so all special-block logic stays in
// one place.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using Game.BlockPuzzle.Board;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Scoring;
using Game.BlockPuzzle.Theme;
using Game.BlockPuzzle.Visuals;

namespace Game.BlockPuzzle.Blocks
{
    public sealed class SpecialCellApplier
    {
        private readonly BoardController _board;
        private readonly ScoreManager _score;
        private readonly ParticleService _particles;

        public SpecialCellApplier(BoardController board, ScoreManager score, ParticleService particles)
        {
            _board = board;
            _score = score;
            _particles = particles;
        }

        // --------------------------------------------------------------------
        // Bomb: clears 3x3 around the placed cell.
        // --------------------------------------------------------------------
        public void ApplyBomb(BoardCoord center, int pieceId, byte colorIndex)
        {
            _board.ApplyBomb(center, pieceId, colorIndex);
            _score.RegisterBombTriggered(center);
            _particles?.SpawnBombBurst(center);
            ServiceLocator.TryResolve<Audio.AudioManager>()?.Play("bomb_explode");
            ServiceLocator.TryResolve<Utils.HapticManager>()?.Impact();
        }

        // --------------------------------------------------------------------
        // Rainbow: clears every cell of one color on the board.
        // --------------------------------------------------------------------
        public void ApplyRainbow(BoardCoord center, int pieceId, byte colorIndex)
        {
            _board.ApplyRainbow(center, pieceId, colorIndex);
            _score.RegisterRainbowTriggered(center);
            _particles?.SpawnRainbowBurst(center);
            ServiceLocator.TryResolve<Audio.AudioManager>()?.Play("rainbow_clear");
            ServiceLocator.TryResolve<Utils.HapticManager>()?.Success();
        }

        // --------------------------------------------------------------------
        // Frozen: not auto-triggered on place; cleared by lines.
        // --------------------------------------------------------------------
        public void ApplyFrozen(BoardCoord coord, byte colorIndex)
        {
            _board.ApplyFrozen(coord, colorIndex);
        }

        // --------------------------------------------------------------------
        // Locked: cleared after 4 adjacent clears.
        // --------------------------------------------------------------------
        public void ApplyLocked(BoardCoord coord)
        {
            _board.ApplyLocked(coord);
        }
    }
}
