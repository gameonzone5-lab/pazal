// ----------------------------------------------------------------------------
// PlayGamesManager.cs
// Wraps Google Play Games Services. Signs the player in, submits scores,
// awards achievements, shows the leaderboard UI.
//
// The real SDK lives in `com.google.play.games` (Unity plugin). Stubs are
// shown where the SDK would be called.
// ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Game.BlockPuzzle.Core;
using UnityEngine;

namespace Game.BlockPuzzle.Cloud
{
    public sealed class PlayGamesManager : MonoBehaviour, IService
    {
        public bool IsSignedIn { get; private set; }
        public string PlayerId { get; private set; }

        public event Action OnSignedInChanged;

        // Leaderboard ids — replace with the ones you create in Play Console.
        public const string LbEndless = "CgkI__endless_leaderboard";
        public const string LbTimed3 = "CgkI__timed3_leaderboard";
        public const string LbDaily = "CgkI__daily_leaderboard";

        public async Task AuthenticateAsync()
        {
            await Task.Yield();
            // PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
            IsSignedIn = false;
            PlayerId = null;
            OnSignedInChanged?.Invoke();
            Log.Info("PlayGames", "Authenticate (stub)");
        }

        public void Shutdown() { }

        public void SignOut()
        {
            IsSignedIn = false;
            PlayerId = null;
            OnSignedInChanged?.Invoke();
        }

        public void SubmitScore(string leaderboardId, int score)
        {
            if (!IsSignedIn) return;
            // PlayGamesPlatform.Instance.ReportScore(score, leaderboardId, success => {});
        }

        public void SubmitDailyScore(string challengeId, int score)
        {
            // In the real game the daily leaderboard is keyed by challenge id.
            SubmitScore(LbDaily, score);
        }

        public void UnlockAchievement(string achievementId)
        {
            if (!IsSignedIn) return;
            // PlayGamesPlatform.Instance.ReportProgress(achievementId, 100.0, success => {});
        }

        public void IncrementAchievement(string achievementId, int steps)
        {
            if (!IsSignedIn) return;
            // PlayGamesPlatform.Instance.IncrementAchievement(achievementId, steps, success => {});
        }

        public void ShowLeaderboards()
        {
            // PlayGamesPlatform.Instance.ShowLeaderboardUI();
        }

        public void ShowAchievements()
        {
            // PlayGamesPlatform.Instance.ShowAchievementsUI();
        }
    }
}
