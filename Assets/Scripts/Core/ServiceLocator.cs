// ----------------------------------------------------------------------------
// ServiceLocator.cs
// Lightweight dependency lookup. Avoid singletons scattered everywhere —
// services register themselves at boot and are looked up by type. We
// intentionally do NOT use a full DI container to keep build size down and
// avoid reflection in IL2CPP.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Game.BlockPuzzle.Core
{
    /// <summary>
    /// Simple, fast service locator. Single-instance per process.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();
        private static readonly object _lock = new();

        /// <summary>True if a service is registered.</summary>
        public static bool IsRegistered<T>() where T : class
        {
            lock (_lock) return _services.ContainsKey(typeof(T));
        }

        /// <summary>Registers a concrete instance.</summary>
        public static void Register<T>(T instance) where T : class
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            lock (_lock)
            {
                if (_services.TryGetValue(typeof(T), out var existing) && !ReferenceEquals(existing, instance))
                {
                    Log.Warn("ServiceLocator", $"Replacing service {typeof(T).Name}");
                }
                _services[typeof(T)] = instance;
            }
        }

        /// <summary>Removes a service.</summary>
        public static void Unregister<T>() where T : class
        {
            lock (_lock) _services.Remove(typeof(T));
        }

        /// <summary>Looks up a service. Throws if missing.</summary>
        public static T Resolve<T>() where T : class
        {
            lock (_lock)
            {
                if (_services.TryGetValue(typeof(T), out var svc)) return (T)svc;
            }
            throw new InvalidOperationException(
                $"Service {typeof(T).Name} not registered. " +
                "Did you forget to add it to GlobalInstaller?");
        }

        /// <summary>Looks up a service or returns null.</summary>
        public static T TryResolve<T>() where T : class
        {
            lock (_lock)
            {
                return _services.TryGetValue(typeof(T), out var svc) ? (T)svc : null;
            }
        }

        /// <summary>Reset (used by tests and on domain reload in editor).</summary>
        public static void Clear()
        {
            lock (_lock) _services.Clear();
        }
    }
}
