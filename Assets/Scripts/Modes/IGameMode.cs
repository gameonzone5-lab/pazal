// ----------------------------------------------------------------------------
// IGameMode.cs
// Common interface for every game mode. A mode owns its own rules for
// win / loss / score limits and broadcasts events to the rest of the game.
// ----------------------------------------------------------------------------

using Game.BlockPuzzle.Board;
using Game.BlockPuzzle.Blocks;
using Game.BlockPuzzle.Core;

namespace Game.BlockPuzzle.Modes
{
    public enum ModeId
    {
        Endless,
        Timed3Min,
        Timed5Min,
        Relax,
        Adventure,
        DailyChallenge
    }

    public enum ModeState { Idle, Running, Paused, GameOver, Victory }

    public interface IGameMode
    {
        ModeId Id { get; }
        ModeState State { get; }
        string DisplayName { get; }

        void Begin();
        void Pause();
        void Resume();
        void End();

        /// <summary>Hook: a piece was placed.</summary>
        void OnPiecePlaced(BlockPiece piece, PlacementResult result);

        /// <summary>Hook: a piece was consumed (placed or refused).</summary>
        void OnPieceConsumed(BlockPiece piece);

        /// <summary>Returns true if the current state is game-over.</summary>
        bool CheckGameOver();

        /// <summary>Per-mode final score (may include time bonus etc.).</summary>
        int ComputeFinalScore();

        /// <summary>Per-mode reward calculation.</summary>
        ModeReward ComputeReward();
    }

    /// <summary>Reward granted at the end of a mode run.</summary>
    public struct ModeReward
    {
        public int Coins;
        public int Gems;
        public int Xp;
        public bool IsJackpot;
    }
}
