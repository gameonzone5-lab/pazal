// ----------------------------------------------------------------------------
// MissionLibrary.cs
// Mission definitions. Each mission has a kind, a target, a reward.
// ----------------------------------------------------------------------------

using System;
using UnityEngine;

namespace Game.BlockPuzzle.Config
{
    public enum MissionKind
    {
        PlacePieces,
        ClearLines,
        ClearCombos,
        ClearBombs,
        ClearRainbows,
        ScorePoints,
        ReachCombo,
        SurviveMinutes,
        ReachAdventureLevel,
        CompleteDailyChallenge
    }

    [CreateAssetMenu(menuName = "BlockCraft/Mission", fileName = "Mission")]
    public sealed class MissionDefinition : ScriptableObject
    {
        public string Id;
        public MissionKind Kind;
        public int Target;
        public int RewardCoins;
        public int RewardGems;
        public int RewardXp;
        public bool CountsAcrossRuns = true;
    }

    [CreateAssetMenu(menuName = "BlockCraft/Mission Library", fileName = "MissionLibrary")]
    public sealed class MissionLibrary : ScriptableObject
    {
        public MissionDefinition[] DailyMissions;
        public MissionDefinition[] WeeklyMissions;
        public MissionDefinition[] BeginnerMissions;

        public MissionDefinition PickDaily(int dayIndex)
        {
            if (DailyMissions == null || DailyMissions.Length == 0) return null;
            return DailyMissions[dayIndex % DailyMissions.Length];
        }
    }
}
