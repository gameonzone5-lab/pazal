// ----------------------------------------------------------------------------
// GameStateMachine.cs
// A small typed state machine used by Modes to express allowed transitions
// (e.g. Playing -> Paused but not GameOver -> Paused).
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Game.BlockPuzzle.Core
{
    /// <summary>
    /// Minimal generic state machine. Each state can declare which other
    /// states it may transition to. Out-of-bounds transitions throw.
    /// </summary>
    public sealed class GameStateMachine<TState> where TState : Enum
    {
        private readonly Dictionary<TState, HashSet<TState>> _allowed = new();
        public TState Current { get; private set; }

        public event Action<TState, TState> OnChanged;

        public GameStateMachine(TState initial)
        {
            Current = initial;
        }

        /// <summary>Allow a transition from <paramref name="from"/> to <paramref name="to"/>.</summary>
        public void Allow(TState from, TState to)
        {
            if (!_allowed.TryGetValue(from, out var set))
            {
                set = new HashSet<TState>();
                _allowed[from] = set;
            }
            set.Add(to);
        }

        /// <summary>Try to transition. Returns true on success.</summary>
        public bool TryTransition(TState to)
        {
            if (EqualityComparer<TState>.Default.Equals(Current, to)) return true;
            if (_allowed.TryGetValue(Current, out var set) && set.Contains(to))
            {
                var prev = Current;
                Current = to;
                OnChanged?.Invoke(prev, to);
                return true;
            }
            return false;
        }

        /// <summary>Force a transition (no transition rules checked).</summary>
        public void Force(TState to)
        {
            var prev = Current;
            Current = to;
            OnChanged?.Invoke(prev, to);
        }
    }
}
