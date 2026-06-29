// ----------------------------------------------------------------------------
// AnalyticsManager.cs
// Thin wrapper over Firebase Analytics + Crashlytics. All gameplay code
// reports through this class so we have a single point to filter / forward
// events. Auto-installs the Crashlytics hook into Log.CrashReporterHook.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Save;
using UnityEngine;

namespace Game.BlockPuzzle.Analytics
{
    public sealed class AnalyticsManager : MonoBehaviour, IService
    {
        public bool Initialized { get; private set; }
        private readonly Queue<(string name, IReadOnlyList<(string key, string value)> props)> _queue = new();
        private bool _consentGranted;

        public void Initialize()
        {
            // FirebaseAnalytics.SetConsent(new ConsentStatus(
            //   analyticsStorageConsentGranted: consent,
            //   adStorageConsentGranted: consent));
            // FirebaseAnalytics.SetUserId(...);
            // FirebaseCrashlytics.Instance.SetCustomKey("build", Application.version);

            // Wire into the logger so any Log.Fatal reaches Crashlytics.
            Log.CrashReporterHook = OnCrashReporterHook;

            // Pull privacy consent from SaveManager.
            var save = ServiceLocator.TryResolve<SaveManager>();
            _consentGranted = save?.Current.Settings.PrivacyAnalytics ?? true;

            Initialized = true;
            FlushQueue();
            Log.Info("Analytics", "Initialized (stub)");
        }

        public void Shutdown()
        {
            Log.CrashReporterHook = null;
        }

        public void SetConsent(bool analytics, bool ads)
        {
            _consentGranted = analytics;
            // FirebaseAnalytics.SetConsent(new ConsentStatus(
            //   analyticsStorageConsentGranted: analytics,
            //   adStorageConsentGranted: ads));
        }

        public void LogEvent(string name, params (string key, string value)[] props)
        {
            if (!_consentGranted) return;
            if (!Initialized)
            {
                _queue.Enqueue((name, props));
                return;
            }
            // if (props == null || props.Length == 0)
            //     FirebaseAnalytics.LogEvent(name);
            // else
            // {
            //     var parameters = new Parameter[props.Length];
            //     for (int i = 0; i < props.Length; i++)
            //         parameters[i] = new Parameter(props[i].key, props[i].value);
            //     FirebaseAnalytics.LogEvent(name, parameters);
            // }
            Log.Verbose("Analytics", $"event: {name} ({(props == null ? 0 : props.Length)} props)");
        }

        private void FlushQueue()
        {
            while (_queue.Count > 0)
            {
                var (name, props) = _queue.Dequeue();
                LogEvent(name, props.ToArray());
            }
        }

        private void OnCrashReporterHook(LogLevel level, string tag, string message, Exception ex)
        {
            // FirebaseCrashlytics.Instance.Log($"{level} [{tag}] {message}");
            // if (ex != null) FirebaseCrashlytics.Instance.RecordException(Java.Lang.Throwable.FromException(ex));
            // Also keep our local buffered log so QA can pull it via adb.
            Debug.LogError($"[CRASHLYTICS] {level} [{tag}] {message} :: {ex}");
        }
    }
}
