// ----------------------------------------------------------------------------
// ComboTracker.cs
// Tracks the current combo and exposes timing windows. A combo is "alive"
// for up to N seconds after the previous clear; if the player doesn't place
// another piece in that window, the combo resets.
//
// Also feeds the chain reactor when a placement causes more lines than the
// first placement did.
// ----------------------------------------------------------------------------

using System;
using Game.BlockPuzzle.Core;
using UnityEngine;

namespace Game.BlockPuzzle.Scoring
{
    public sealed class ComboTracker : MonoBehaviour, IService
    {
        public int CurrentCombo { get; private set; }
        public int BestComboThisRun { get; private set; }
        public float ComboWindowSeconds = 4f;
        public bool IsAlive => Time.time - _lastClearTime <= ComboWindowSeconds;

        private float _lastClearTime = -999f;

        public void Initialize()
        {
            CurrentCombo = 0;
            BestComboThisRun = 0;
            _lastClearTime = -999f;
        }

        public void Shutdown() { }

        public void RegisterClear(int linesCleared)
        {
            if (!IsAlive) CurrentCombo = 0;
            CurrentCombo = Math.Max(CurrentCombo, 0) + linesCleared;
            if (CurrentCombo > BestComboThisRun) BestComboThisRun = CurrentCombo;
            _lastClearTime = Time.time;
        }

        public void OnPiecePlacedWithoutClear()
        {
            // Soft decay only — keep current combo if still within window.
            if (!IsAlive) CurrentCombo = 0;
        }

        public void ResetRun()
        {
            CurrentCombo = 0;
            BestComboThisRun = 0;
            _lastClearTime = -999f;
        }
    }
}
