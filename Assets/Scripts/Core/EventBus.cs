// ----------------------------------------------------------------------------
// EventBus.cs
// Strongly typed, allocation-free publish/subscribe bus. Avoids the brittle
// pattern of every system reaching into every other system.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.BlockPuzzle.Core
{
    /// <summary>
    /// Marker interface for all events on the bus.
    /// </summary>
    public interface IGameEvent { }

    /// <summary>
    /// Static event bus. Handlers are stored per event type. Handlers are
    /// weak-ref-free by design: subscribers are responsible for unsubscribing
    /// in OnDisable / OnDestroy to avoid leaks.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _subscribers = new();
        private static readonly object _lock = new();

        /// <summary>Subscribe a handler.</summary>
        public static void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            if (handler == null) return;
            lock (_lock)
            {
                if (!_subscribers.TryGetValue(typeof(T), out var list))
                {
                    list = new List<Delegate>(4);
                    _subscribers[typeof(T)] = list;
                }
                list.Add(handler);
            }
        }

        /// <summary>Unsubscribe a handler.</summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            if (handler == null) return;
            lock (_lock)
            {
                if (_subscribers.TryGetValue(typeof(T), out var list))
                    list.Remove(handler);
            }
        }

        /// <summary>Publish an event immediately. Synchronous on calling thread.</summary>
        public static void Publish<T>(T evt) where T : IGameEvent
        {
            if (evt == null) return;
            Delegate[] snapshot;
            lock (_lock)
            {
                if (!_subscribers.TryGetValue(typeof(T), out var list) || list.Count == 0) return;
                snapshot = list.ToArray();
            }
            for (int i = 0; i < snapshot.Length; i++)
            {
                try
                {
                    ((Action<T>)snapshot[i]).Invoke(evt);
                }
                catch (Exception ex)
                {
                    Log.Error("EventBus", $"Handler for {typeof(T).Name} threw", ex);
                }
            }
        }

        /// <summary>Clear all subscribers (tests / scene transitions).</summary>
        public static void Clear()
        {
            lock (_lock) _subscribers.Clear();
        }
    }

    // ------------------------------------------------------------------------
    // Built-in events. Game-specific events live in their owning module.
    // ------------------------------------------------------------------------

    /// <summary>Emitted when game state changes.</summary>
    public readonly struct GameStateChangedEvent : IGameEvent
    {
        public readonly GameState OldState;
        public readonly GameState NewState;
        public GameStateChangedEvent(GameState oldState, GameState newState)
        {
            OldState = oldState;
            NewState = newState;
        }
    }

    /// <summary>Emitted on every coin / gem delta.</summary>
    public readonly struct CurrencyChangedEvent : IGameEvent
    {
        public readonly string CurrencyId;
        public readonly long OldAmount;
        public readonly long NewAmount;
        public readonly string Reason;
        public CurrencyChangedEvent(string id, long oldA, long newA, string reason)
        {
            CurrencyId = id;
            OldAmount = oldA;
            NewAmount = newA;
            Reason = reason;
        }
    }

    /// <summary>Emitted when the player earns / spends XP (battle pass).</summary>
    public readonly struct XpGainedEvent : IGameEvent
    {
        public readonly int Amount;
        public readonly string Source;
        public XpGainedEvent(int amount, string source) { Amount = amount; Source = source; }
    }

    /// <summary>Emitted by remote config when values change.</summary>
    public readonly struct RemoteConfigUpdatedEvent : IGameEvent
    {
        public readonly string Key;
        public RemoteConfigUpdatedEvent(string key) { Key = key; }
    }

    /// <summary>Emitted when ad consent state changes (UMP / GDPR).</summary>
    public readonly struct ConsentUpdatedEvent : IGameEvent
    {
        public readonly bool CanRequestAds;
        public ConsentUpdatedEvent(bool can) { CanRequestAds = can; }
    }
}
