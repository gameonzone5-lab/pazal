// ----------------------------------------------------------------------------
// MissionManager.cs
// Tracks mission progress and grants rewards on completion. Persists via
// SaveManager (each mission is a MissionSaveEntry).
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using Game.BlockPuzzle.Config;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Economy;
using Game.BlockPuzzle.Scoring;
using UnityEngine;

namespace Game.BlockPuzzle.Missions
{
    public sealed class MissionManager : MonoBehaviour, IService
    {
        public IReadOnlyList<MissionDefinition> ActiveDaily { get; private set; }
        public IReadOnlyList<MissionDefinition> ActiveWeekly { get; private set; }

        public void Initialize()
        {
            EnsureDaily();
            EnsureWeekly();
        }

        public void Shutdown() { }

        // --------------------------------------------------------------------
        // Daily / Weekly refresh
        // --------------------------------------------------------------------
        private void EnsureDaily()
        {
            var lib = GameConfig.Instance?.Missions;
            if (lib == null) return;
            var day = System.DateTime.UtcNow.DayOfYear;
            int n = 3;
            var list = new List<MissionDefinition>(n);
            for (int i = 0; i < n; i++)
            {
                var m = lib.PickDaily(day + i);
                if (m != null) list.Add(m);
            }
            ActiveDaily = list;
        }

        private void EnsureWeekly()
        {
            var lib = GameConfig.Instance?.Missions;
            if (lib == null) { ActiveWeekly = System.Array.Empty<MissionDefinition>(); return; }
            var wk = System.DateTime.UtcNow.DayOfYear / 7;
            int n = Mathf.Min(5, lib.WeeklyMissions != null ? lib.WeeklyMissions.Length : 0);
            var list = new List<MissionDefinition>(n);
            for (int i = 0; i < n; i++)
            {
                int idx = (wk + i) % n;
                if (lib.WeeklyMissions != null && idx >= 0 && idx < lib.WeeklyMissions.Length)
                    list.Add(lib.WeeklyMissions[idx]);
            }
            ActiveWeekly = list;
        }

        // --------------------------------------------------------------------
        // Hooks called by gameplay
        // --------------------------------------------------------------------
        public void OnPiecePlaced(int cells, PlacementResult result)
        {
            BumpKind(MissionKind.PlacePieces, 1);
            if (result.LinesCleared > 0) BumpKind(MissionKind.ClearLines, result.LinesCleared);
            if (result.IsPerfectClear) BumpKind(MissionKind.ReachCombo, 1);
        }

        public void OnLinesCleared(int lines) => BumpKind(MissionKind.ClearLines, lines);
        public void OnCombo(int combo)
        {
            if (combo >= 3) BumpKind(MissionKind.ReachCombo, 1);
            if (combo >= 2) BumpKind(MissionKind.ClearCombos, 1);
        }
        public void OnBombUsed() => BumpKind(MissionKind.ClearBombs, 1);
        public void OnRainbowUsed() => BumpKind(MissionKind.ClearRainbows, 1);
        public void OnAdventureLevelCleared() => BumpKind(MissionKind.ReachAdventureLevel, 1);
        public void OnDailyChallengeCompleted() => BumpKind(MissionKind.CompleteDailyChallenge, 1);

        public void OnScore(int delta)
        {
            BumpKind(MissionKind.ScorePoints, delta);
        }

        public void OnRewardGranted(Reward reward, string source) { /* no-op */ }

        // --------------------------------------------------------------------
        // Internals
        // --------------------------------------------------------------------
        private void BumpKind(MissionKind kind, int amount)
        {
            Bump(ActiveDaily, kind, amount);
            Bump(ActiveWeekly, kind, amount);
        }

        private void Bump(IReadOnlyList<MissionDefinition> defs, MissionKind kind, int amount)
        {
            if (defs == null) return;
            for (int i = 0; i < defs.Count; i++)
            {
                var m = defs[i];
                if (m == null || m.Kind != kind) continue;
                var entry = GetOrCreate(m);
                if (entry.Claimed) continue;
                entry.Progress = Mathf.Min(m.Target, entry.Progress + amount);
                if (entry.Progress >= m.Target) GrantReward(m);
            }
            ServiceLocator.TryResolve<Save.SaveManager>()?.MarkDirty();
        }

        private Save.MissionSaveEntry GetOrCreate(MissionDefinition m)
        {
            var save = ServiceLocator.Resolve<Save.SaveManager>();
            foreach (var e in save.Current.MissionState)
                if (e.MissionId == m.Id) return e;
            var ne = new Save.MissionSaveEntry
            {
                MissionId = m.Id,
                Progress = 0,
                Claimed = false,
                AssignedUtc = System.DateTimeOffset.UtcNow
            };
            save.Current.MissionState.Add(ne);
            return ne;
        }

        private void GrantReward(MissionDefinition m)
        {
            var rs = ServiceLocator.Resolve<RewardSystem>();
            rs.Grant(m.RewardCoins, m.RewardGems, m.RewardXp, $"mission:{m.Id}");
            var entry = GetOrCreate(m);
            entry.Claimed = true;
            SFXPlayer.Play("reward");
        }
    }
}
