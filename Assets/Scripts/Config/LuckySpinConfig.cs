// ----------------------------------------------------------------------------
// LuckySpinConfig.cs
// Lucky-spin wheel of fortune rewards.
// ----------------------------------------------------------------------------

using UnityEngine;

namespace Game.BlockPuzzle.Config
{
    [System.Serializable]
    public struct SpinSlot
    {
        public string Label;
        public int Coins;
        public int Gems;
        public int Energy;
        public string CosmeticId;
        [Tooltip("0 = never, 1 = always.")]
        public float Weight = 1f;
        public bool IsJackpot;
    }

    [CreateAssetMenu(menuName = "BlockCraft/Lucky Spin Config", fileName = "LuckySpinConfig")]
    public sealed class LuckySpinConfig : ScriptableObject
    {
        public SpinSlot[] Slots;
        public int FreeSpinsPerDay = 1;
        public int SpinCostGems = 5;

        public SpinSlot Pick(Security.SecureRandom rng)
        {
            if (Slots == null || Slots.Length == 0) return default;
            float total = 0;
            for (int i = 0; i < Slots.Length; i++) total += Mathf.Max(0.0001f, Slots[i].Weight);
            float r = rng.NextFloat01() * total;
            for (int i = 0; i < Slots.Length; i++)
            {
                r -= Mathf.Max(0.0001f, Slots[i].Weight);
                if (r <= 0) return Slots[i];
            }
            return Slots[Slots.Length - 1];
        }
    }
}
