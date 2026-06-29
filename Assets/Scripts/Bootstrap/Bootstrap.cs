// ----------------------------------------------------------------------------
// Bootstrap.cs
// The first thing that runs. Runs BEFORE the first scene loads thanks to
// RuntimeInitializeOnLoadMethod. Creates a root GameObject, installs all
// services, kicks off save-load, and transitions the game to the main menu.
//
// IMPORTANT: This file must NOT have an Awake() — order of operations matters
// here and is controlled explicitly.
// ----------------------------------------------------------------------------

using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.BlockPuzzle.Bootstrap
{
    public static class Bootstrap
    {
        private const string RootObjectName = "[BlockCraft.Root]";
        private static GameObject _root;

        // Called by Unity before any scene loads (SubsystemRegistration is
        // the very first callback so we can guarantee we run first).
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            // Defensive: avoid double-init on domain reload in editor.
            if (_root != null) return;

            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Application.lowMemory += OnLowMemory;
            Application.deepLinkActivated += OnDeepLink;

            _root = new GameObject(RootObjectName);
            Object.DontDestroyOnLoad(_root);

            // Create GameManager early so the rest of the services can talk to it.
            _root.AddComponent<GameManager>();

            // Wire every service.
            GlobalInstaller.Install(_root);
            _root.AddComponent<AppLifecycle>();

            // Kick off async init (consent, cloud, IAP, audio loading).
            _ = AsyncInitialize();
        }

        private static async Task AsyncInitialize()
        {
            var gm = ServiceLocator.Resolve<GameManager>();
            gm.TransitionTo(GameState.Loading);

            // 1) Load save data first so other services can read wallet, settings.
            var save = ServiceLocator.Resolve<Save.SaveManager>();
            await save.LoadAsync();

            // 2) Apply theme / settings before showing UI.
            ServiceLocator.Resolve<Theme.ThemeManager>().ApplyTheme(
                save.Current.Settings.Theme);

            // 3) Initialize audio banks (loads Addressables / Resources).
            ServiceLocator.Resolve<Audio.AudioManager>().InitializeBanks();

            // 4) GDPR consent must come BEFORE ads try to load.
            var consent = ServiceLocator.Resolve<Ads.ConsentManager>();
            await consent.InitializeAsync();
            ServiceLocator.Resolve<Ads.AdsManager>().Initialize();

            // 5) Cloud / Play Games (best effort, fail soft).
            _ = ServiceLocator.Resolve<Cloud.PlayGamesManager>().AuthenticateAsync();
            _ = ServiceLocator.Resolve<Cloud.CloudSaveManager>().InitializeAsync();

            // 6) IAP (best effort).
            _ = ServiceLocator.Resolve<IAP.IAPManager>().InitializeAsync();

            // 7) Analytics
            ServiceLocator.Resolve<Analytics.AnalyticsManager>().Initialize();

            // 8) Show main menu.
            await LoadSceneAsync("MainMenu");
            gm.GoToMainMenu();
        }

        private static async Task LoadSceneAsync(string scene)
        {
            var op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            while (op != null && !op.isDone) await Awaitable.NextFrameAsync();
        }

        private static void OnLowMemory()
        {
            Log.Warn("Bootstrap", "Low memory event");
            // Drop caches, unload unused assets.
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        private static void OnDeepLink(string url)
        {
            Log.Info("Bootstrap", $"Deep link: {url}");
            // Route deep link to UI / missions. Stub.
        }
    }
}
