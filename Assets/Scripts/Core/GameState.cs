// ----------------------------------------------------------------------------
// GameState.cs
// The finite set of high-level application states.
// ----------------------------------------------------------------------------

namespace Game.BlockPuzzle.Core
{
    /// <summary>Top-level game state.</summary>
    public enum GameState
    {
        Boot,
        Loading,
        MainMenu,
        ModeSelect,
        Playing,
        Paused,
        GameOver,
        Result,
        Shop,
        Settings,
        Quit
    }
}
