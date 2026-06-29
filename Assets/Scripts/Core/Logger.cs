// ----------------------------------------------------------------------------
// Logger.cs
// Tiny structured logger. Wraps UnityEngine.Debug and routes messages to
// Analytics / Crashlytics on errors / fatals. Use Log.Info / Log.Warn / Log.Error
// instead of Debug.Log directly so we have a single chokepoint.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.BlockPuzzle.Core
{
    /// <summary>
    /// Severity levels for the logger.
    /// </summary>
    public enum LogLevel
    {
        Verbose,
        Info,
        Warning,
        Error,
        Fatal
    }

    /// <summary>
    /// Tag-based logger. Tags look like "Board", "Ads", "IAP".
    /// </summary>
    public static class Log
    {
        // Tag is prepended; LogInfo-style allows filtering later.
        private static readonly Dictionary<string, int> _counters = new();

        public static void Verbose(string tag, string message) => Write(LogLevel.Verbose, tag, message);
        public static void Info(string tag, string message) => Write(LogLevel.Info, tag, message);
        public static void Warn(string tag, string message) => Write(LogLevel.Warning, tag, message);
        public static void Error(string tag, string message, Exception ex = null) => Write(LogLevel.Error, tag, message, ex);
        public static void Fatal(string tag, string message, Exception ex = null) => Write(LogLevel.Fatal, tag, message, ex);

        public static void IncrementCounter(string counter, int value = 1)
        {
            _counters.TryGetValue(counter, out var n);
            _counters[counter] = n + value;
        }

        public static IReadOnlyDictionary<string, int> Counters => _counters;

        private static void Write(LogLevel level, string tag, string message, Exception ex = null)
        {
            var prefix = $"[BC][{level}][{tag}]";
            var text = $"{prefix} {message}";
            switch (level)
            {
                case LogLevel.Verbose:
                case LogLevel.Info:
                    Debug.Log(text);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(text);
                    break;
                case LogLevel.Error:
                    Debug.LogError(text);
                    if (ex != null) Debug.LogException(ex);
                    break;
                case LogLevel.Fatal:
                    Debug.LogError(text);
                    if (ex != null) Debug.LogException(ex);
                    // In real builds, push to Crashlytics via a callback.
                    CrashReporterHook?.Invoke(level, tag, message, ex);
                    break;
            }
        }

        /// <summary>
        /// Crashlytics hook — set by CloudManager when Crashlytics initializes.
        /// </summary>
        public static Action<LogLevel, string, string, Exception> CrashReporterHook;
    }
}
