// ----------------------------------------------------------------------------
// GlobalInstaller.cs
// Single point where services are constructed and registered into the
// ServiceLocator. Bootstrap calls Install() during application startup
// before the first scene loads.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace Game.BlockPuzzle.Core
{
    /// <summary>
    /// Creates and wires all top-level services. Designed to be called once
    /// from Bootstrap before any other system touches the ServiceLocator.
    /// </summary>
    public static class GlobalInstaller
    {
        // Service interfaces live alongside their implementations.
        // We register concrete types for performance (no reflection).
        private static readonly List<IService> _services = new();
        private static bool _installed;

        public static bool IsInstalled => _installed;

        public static void Install(GameObject root)
        {
            if (_installed) return;
            _installed = true;

            // ---------- Lifecycle on root ---------------------------------
            Object.DontDestroyOnLoad(root);

            // ---------- Core ------------------------------------------------
            Add<Save.SaveManager>(root, nameof(Save.SaveManager));
            Add<Audio.AudioManager>(root, nameof(Audio.AudioManager));
            Add<Theme.ThemeManager>(root, nameof(Theme.ThemeManager));
            Add<Profile.PlayerProfile>(root, nameof(Profile.PlayerProfile));
            Add<Profile.StatisticsTracker>(root, nameof(Profile.StatisticsTracker));
            Add<Profile.AchievementManager>(root, nameof(Profile.AchievementManager));
            Add<Economy.EconomyManager>(root, nameof(Economy.EconomyManager));
            Add<Economy.RewardSystem>(root, nameof(Economy.RewardSystem));
            Add<Board.BoardController>(root, nameof(Board.BoardController));
            Add<Blocks.BlockSpawner>(root, nameof(Blocks.BlockSpawner));
            Add<Scoring.ScoreManager>(root, nameof(Scoring.ScoreManager));
            Add<Missions.MissionManager>(root, nameof(Missions.MissionManager));
            Add<Events.DailyChallengeManager>(root, nameof(Events.DailyChallengeManager));
            Add<Events.WeeklyEventManager>(root, nameof(Events.WeeklyEventManager));
            Add<Progression.BattlePassManager>(root, nameof(Progression.BattlePassManager));
            Add<Progression.DailyLoginRewards>(root, nameof(Progression.DailyLoginRewards));
            Add<Progression.LuckySpin>(root, nameof(Progression.LuckySpin));
            Add<Visuals.ParticleService>(root, nameof(Visuals.ParticleService));
            Add<Visuals.AnimationSequencer>(root, nameof(Visuals.AnimationSequencer));
            Add<Ads.AdsManager>(root, nameof(Ads.AdsManager));
            Add<Ads.ConsentManager>(root, nameof(Ads.ConsentManager));
            Add<IAP.IAPManager>(root, nameof(IAP.IAPManager));
            Add<Cloud.CloudSaveManager>(root, nameof(Cloud.CloudSaveManager));
            Add<Cloud.PlayGamesManager>(root, nameof(Cloud.PlayGamesManager));
            Add<Analytics.AnalyticsManager>(root, nameof(Analytics.AnalyticsManager));
            Add<Security.AntiCheat>(root, nameof(Security.AntiCheat));
            Add<Utils.HapticManager>(root, nameof(Utils.HapticManager));
            Add<Localization.LocalizationManager>(root, nameof(Localization.LocalizationManager));

            // ---------- Initialize services in safe order ------------------
            foreach (var s in _services) s.Initialize();
        }

        public static void Shutdown()
        {
            for (int i = _services.Count - 1; i >= 0; i--) _services[i].Shutdown();
            _services.Clear();
            ServiceLocator.Clear();
            EventBus.Clear();
            _installed = false;
        }

        private static void Add<T>(GameObject root, string childName) where T : Component, IService
        {
            var existing = root.GetComponentInChildren<T>(true);
            T comp;
            if (existing != null) comp = existing;
            else
            {
                var go = new GameObject(childName);
                go.transform.SetParent(root.transform, false);
                comp = go.AddComponent<T>();
            }
            ServiceLocator.Register<T>(comp);
            _services.Add(comp);
        }
    }

    /// <summary>
    /// Implemented by every MonoBehaviour service so the installer can
    /// initialize and shut them down in a deterministic order.
    /// </summary>
    public interface IService
    {
        void Initialize();
        void Shutdown();
    }
}
