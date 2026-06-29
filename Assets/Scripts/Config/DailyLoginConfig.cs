// ----------------------------------------------------------------------------
// DailyLoginConfig.cs
// 28-day rolling reward schedule for daily login bonus.
// ----------------------------------------------------------------------------

using UnityEngine;

namespace Game.BlockPuzzle.Config
{
    [System.Serializable]
    public struct DailyLoginReward
    {
        public int Day;          // 1..28
        public int Coins;
        public int Gems;
        public int Energy;
        public string CosmeticId;
        public bool IsMilestone;
    }

    [CreateAssetMenu(menuName = "BlockCraft/Daily Login Config", fileName = "DailyLoginConfig")]
    public sealed class DailyLoginConfig : ScriptableObject
    {
        public int DaysInCycle = 28;
        public DailyLoginReward[] Schedule;

        public DailyLoginReward GetRewardFor(int dayIndex)
        {
            if (Schedule == null || Schedule.Length == 0) return default;
            dayIndex = ((dayIndex % DaysInCycle) + DaysInCycle) % DaysInCycle;
            for (int i = 0; i < Schedule.Length; i++)
                if (Schedule[i].Day == dayIndex + 1) return Schedule[i];
            return Schedule[0];
        }
    }
}
