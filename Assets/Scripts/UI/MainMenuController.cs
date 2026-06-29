// ----------------------------------------------------------------------------
// MainMenuController.cs
// Wires up the main menu buttons (Play, Continue, Settings, Shop, Profile,
// Leaderboard). Pulls live data (coins, gems, level) from PlayerProfile.
// ----------------------------------------------------------------------------

using Game.BlockPuzzle.Audio;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.BlockPuzzle.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        public Button PlayButton;
        public Button ContinueButton;
        public Button ShopButton;
        public Button SettingsButton;
        public Button ProfileButton;
        public Button LeaderboardButton;
        public Button DailyLoginButton;
        public Button LuckySpinButton;
        public Button BattlePassButton;

        [Header("Labels")]
        public TMP_Text CoinsLabel;
        public TMP_Text GemsLabel;
        public TMP_Text LevelLabel;
        public TMP_Text NicknameLabel;

        private void OnEnable()
        {
            Refresh();
            HookButtons(true);
            EventBus.Subscribe<CurrencyChangedEvent>(OnCurrency);
        }

        private void OnDisable()
        {
            HookButtons(false);
            EventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrency);
        }

        private void Refresh()
        {
            var profile = ServiceLocator.Resolve<Profile.PlayerProfile>();
            var econ = ServiceLocator.Resolve<Economy.EconomyManager>();
            if (CoinsLabel != null) CoinsLabel.text = FormatNumber(econ.Wallet.Coins);
            if (GemsLabel != null) GemsLabel.text = FormatNumber(econ.Wallet.Gems);
            if (LevelLabel != null) LevelLabel.text = $"Lv {profile?.Level ?? 1}";
            if (NicknameLabel != null) NicknameLabel.text = profile?.DisplayName ?? "Player";

            var save = ServiceLocator.Resolve<Save.SaveManager>();
            if (ContinueButton != null)
                ContinueButton.gameObject.SetActive(save.Current.Progress.AdventureLevel > 1);
        }

        private void OnCurrency(CurrencyChangedEvent evt) => Refresh();

        private void HookButtons(bool on)
        {
            Bind(PlayButton, on, OnPlay);
            Bind(ContinueButton, on, OnContinue);
            Bind(ShopButton, on, () => GameManager.Instance.OpenShop());
            Bind(SettingsButton, on, () => GameManager.Instance.OpenSettings());
            Bind(ProfileButton, on, () => GameManager.Instance.TransitionTo(GameState.Settings));
            Bind(LeaderboardButton, on, () => ServiceLocator.Resolve<Cloud.PlayGamesManager>()?.ShowLeaderboards());
            Bind(DailyLoginButton, on, () => ServiceLocator.Resolve<DailyLoginRewards>()?.Show());
            Bind(LuckySpinButton, on, () => ServiceLocator.Resolve<LuckySpin>()?.Show());
            Bind(BattlePassButton, on, () => ServiceLocator.Resolve<BattlePassManager>()?.Show());
        }

        private static void Bind(Button b, bool on, System.Action act)
        {
            if (b == null) return;
            b.onClick.RemoveAllListeners();
            if (on) b.onClick.AddListener(() => { SFXPlayer.Play(SFXPlayer.Click); act?.Invoke(); });
        }

        private void OnPlay()
        {
            GameManager.Instance.GoToModeSelect();
        }

        private void OnContinue()
        {
            var save = ServiceLocator.Resolve<Save.SaveManager>();
            int lvl = save.Current.Progress.AdventureLevel;
            var lib = Config.GameConfig.Instance?.Levels;
            var def = lib?.Find(lvl);
            var adv = new Modes.AdventureMode(
                ServiceLocator.Resolve<Scoring.ScoreManager>(),
                ServiceLocator.Resolve<Blocks.BlockSpawner>(),
                ServiceLocator.Resolve<Board.BoardController>());
            if (def != null) adv.LoadLevel(def);
            ServiceLocator.Register<Modes.IGameMode>(adv);
            adv.Begin();
        }

        private static string FormatNumber(long n)
        {
            if (n >= 1_000_000) return (n / 1_000_000f).ToString("0.0") + "M";
            if (n >= 10_000) return (n / 1_000f).ToString("0.#") + "K";
            return n.ToString();
        }
    }
}
