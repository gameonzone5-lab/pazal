// ----------------------------------------------------------------------------
// LocalizationManager.cs
// Looks up localized strings by key. Tables are ScriptableObject assets
// under Resources/Localization. Falls back to the key if missing.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using Game.BlockPuzzle.Config;
using Game.BlockPuzzle.Core;
using TMPro;
using UnityEngine;

namespace Game.BlockPuzzle.Localization
{
    [DisallowMultipleComponent]
    public sealed class LocalizationManager : MonoBehaviour, IService
    {
        private Dictionary<string, string> _entries = new();
        private string _currentLocale = "en";

        public string CurrentLocale => _currentLocale;

        public void Initialize()
        {
            ApplyLocale(ServiceLocator.Resolve<Save.SaveManager>().Current.LocaleCode);
        }

        public void Shutdown() { }

        public void ApplyLocale(string localeCode)
        {
            _currentLocale = string.IsNullOrEmpty(localeCode) ? "en" : localeCode;
            var cfg = GameConfig.Instance != null ? GameConfig.Instance.Localization : null;
            var table = cfg != null ? cfg.Find(_currentLocale) : null;
            _entries.Clear();
            if (table != null && table.Entries != null)
            {
                foreach (var e in table.Entries)
                    if (!string.IsNullOrEmpty(e.Key)) _entries[e.Key] = e.English;
            }
        }

        public string Get(string key) => _entries.TryGetValue(key, out var s) ? s : key;

        public string Format(string key, params object[] args)
        {
            var s = Get(key);
            return args == null || args.Length == 0 ? s : string.Format(s, args);
        }

        /// <summary>Helper that reads Localization.Get and writes into a TMP label.</summary>
        public void Bind(TMP_Text label, string key, bool uppercase = false)
        {
            if (label == null) return;
            var s = Get(key);
            label.text = uppercase ? s.ToUpperInvariant() : s;
        }
    }
}
