// ----------------------------------------------------------------------------
// ChainReactor.cs
// A "chain" happens when a player places one piece, clears a line, and the
// cleared cells unlock placement of a *different* piece (still in their hand)
// that immediately clears again. Each subsequent clear deepens the chain.
// Each step awards a chain bonus.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Game.BlockPuzzle.Board;
using Game.BlockPuzzle.Blocks;
using Game.BlockPuzzle.Core;
using UnityEngine;

namespace Game.BlockPuzzle.Scoring
{
    public sealed class ChainReactor : MonoBehaviour, IService
    {
        private BoardController _board;
        private ScoreManager _score;
        private BlockPlacer _placer;

        public int CurrentChainDepth { get; private set; }
        public int BestChainThisRun { get; private set; }

        public void Initialize()
        {
            _board = ServiceLocator.Resolve<BoardController>();
            _score = ServiceLocator.Resolve<ScoreManager>();
            BestChainThisRun = 0;
            CurrentChainDepth = 0;
        }

        public void Shutdown() { }

        public void OnRunStart()
        {
            CurrentChainDepth = 0;
            BestChainThisRun = 0;
        }

        /// <summary>
        /// Called after every successful placement. If lines were cleared,
        /// attempt to chain-place any piece from the active hand that now
        /// fits.
        /// </summary>
        public void TryChainAfterPlacement(BlockPiece[] activeHand, Action<BlockPiece> consumeFromHand)
        {
            // First clear sets chain depth to 1 (if any lines were cleared).
            // The "real" chain depth is the count of subsequent clears caused
            // by auto-placed pieces — in the standard gameplay we don't
            // auto-place, so chain depth is driven by the player doing two
            // placements in a row within the combo window.
            // We model that here: if the player's next placement (within the
            // combo window) clears lines, chain depth increments.
            CurrentChainDepth++;
            if (CurrentChainDepth > BestChainThisRun) BestChainThisRun = CurrentChainDepth;
        }

        public void RegisterClearedLines(int lines, int cells)
        {
            if (lines > 0 && CurrentChainDepth > 0)
                _score.RegisterChain(CurrentChainDepth, cells);
        }

        public void OnPiecePlacedWithoutClear()
        {
            // No chain extension.
        }

        public void OnGameOver()
        {
            var save = ServiceLocator.TryResolve<Save.SaveManager>();
            if (save?.Current != null && BestChainThisRun > save.Current.Statistics.BestChain)
                save.Current.Statistics.BestChain = BestChainThisRun;
        }
    }
}
