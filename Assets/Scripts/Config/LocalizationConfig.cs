// ----------------------------------------------------------------------------
// LocalizationConfig.cs
// Localization assets. Authored as ScriptableObjects so designers can edit
// strings without touching code. Runtime uses string-table csv files.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace Game.BlockPuzzle.Config
{
    [System.Serializable]
    public struct LocalizedEntry
    {
        public string Key;
        [TextArea] public string English;
    }

    [CreateAssetMenu(menuName = "BlockCraft/Localization Table", fileName = "Localization_en")]
    public sealed class LocalizationTable : ScriptableObject
    {
        public string LocaleCode = "en";
        public LocalizedEntry[] Entries;

        public string Get(string key)
        {
            if (Entries == null) return key;
            for (int i = 0; i < Entries.Length; i++)
                if (Entries[i].Key == key) return Entries[i].English;
            return key;
        }
    }

    [CreateAssetMenu(menuName = "BlockCraft/Localization Config", fileName = "LocalizationConfig")]
    public sealed class LocalizationConfig : ScriptableObject
    {
        public LocalizationTable[] Tables;
        public string DefaultLocale = "en";

        public LocalizationTable Find(string code)
        {
            if (Tables == null) return null;
            for (int i = 0; i < Tables.Length; i++)
                if (Tables[i].LocaleCode == code) return Tables[i];
            return Tables.Length > 0 ? Tables[0] : null;
        }
    }
}
