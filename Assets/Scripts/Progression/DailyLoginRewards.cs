// ----------------------------------------------------------------------------
// DailyLoginRewards.cs
// Tracks consecutive day logins, claims the day's reward, resets on missed
// day. Shows a popup once per real-world day on MainMenu.
// ----------------------------------------------------------------------------

using System;
using Game.BlockPuzzle.Config;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Economy;
using UnityEngine;

namespace Game.BlockPuzzle.Progression
{
    public sealed class DailyLoginRewards : MonoBehaviour, IService
    {
        public bool CanClaim { get; private set; }
        public int NextDayIndex { get; private set; }

        public event Action OnAvailableChanged;

        public void Initialize()
        {
            Evaluate();
        }

        public void Shutdown() { }

        public void Show() => GameManager.Instance.TransitionTo(GameState.MainMenu);

        public void Evaluate()
        {
            var save = ServiceLocator.Resolve<Save.SaveManager>();
            var today = DateTimeOffset.UtcNow.Date;
            var last = save.Current.Statistics.LastLoginRewardUtc.Date;
            int lastIdx = save.Current.Statistics.LastLoginRewardDayIndex;
            int streak = save.Current.Statistics.ConsecutiveDaysLaunched;
            if (last == today) { CanClaim = false; NextDayIndex = lastIdx; OnAvailableChanged?.Invoke(); return; }

            // If more than 1 day has passed, reset streak.
            if ((today - last).TotalDays > 1)
            {
                save.Current.Statistics.ConsecutiveDaysLaunched = 1;
                NextDayIndex = 0;
            }
            else
            {
                NextDayIndex = (lastIdx + 1) % (GameConfig.Instance.DailyLogin?.DaysInCycle ?? 28);
            }
            CanClaim = true;
            OnAvailableChanged?.Invoke();
        }

        public DailyLoginReward Claim()
        {
            if (!CanClaim) return default;
            var cfg = GameConfig.Instance?.DailyLogin;
            if (cfg == null) return default;
            var reward = cfg.GetRewardFor(NextDayIndex);
            var rs = ServiceLocator.Resolve<RewardSystem>();
            rs.Grant(reward.Coins, reward.Gems, reward.Energy, $"daily_login_d{NextDayIndex + 1}");

            var save = ServiceLocator.Resolve<Save.SaveManager>();
            save.Current.Statistics.LastLoginRewardUtc = DateTimeOffset.UtcNow;
            save.Current.Statistics.LastLoginRewardDayIndex = NextDayIndex;
            save.Current.Statistics.ConsecutiveDaysLaunched++;
            save.MarkDirty();
            CanClaim = false;
            OnAvailableChanged?.Invoke();
            return reward;
        }
    }
}
