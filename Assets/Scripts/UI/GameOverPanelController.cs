// ----------------------------------------------------------------------------
// GameOverPanelController.cs
// Game-over overlay: final score, best score, coins / gems / XP earned,
// "Revive" (rewarded ad), "Continue" (no reward), "Home" buttons.
// ----------------------------------------------------------------------------

using Game.BlockPuzzle.Ads;
using Game.BlockPuzzle.Audio;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Economy;
using Game.BlockPuzzle.Modes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.BlockPuzzle.UI
{
    public sealed class GameOverPanelController : MonoBehaviour
    {
        [Header("Labels")]
        public TMP_Text TitleLabel;
        public TMP_Text FinalScoreLabel;
        public TMP_Text BestScoreLabel;
        public TMP_Text CoinsEarnedLabel;
        public TMP_Text GemsEarnedLabel;
        public TMP_Text XpEarnedLabel;
        public TMP_Text NewBestBadge;

        [Header("Buttons")]
        public Button HomeButton;
        public Button RestartButton;
        public Button ReviveButton;
        public Button DoubleRewardButton;

        private void OnEnable()
        {
            Populate();
            Bind(HomeButton, () => GameManager.Instance.GoToMainMenu());
            Bind(RestartButton, () => ServiceLocator.TryResolve<IGameMode>()?.Begin());
            Bind(ReviveButton, () => RequestRevive());
            Bind(DoubleRewardButton, () => RequestDoubleReward());
        }

        private void Populate()
        {
            var mode = ServiceLocator.TryResolve<IGameMode>();
            var score = ServiceLocator.Resolve<Scoring.ScoreManager>();
            if (TitleLabel != null) TitleLabel.text = "Game Over";
            if (FinalScoreLabel != null) FinalScoreLabel.text = mode?.ComputeFinalScore().ToString("N0") ?? "0";
            if (BestScoreLabel != null) BestScoreLabel.text = $"Best {score.HighScore:N0}";
            if (NewBestBadge != null) NewBestBadge.gameObject.SetActive(score.Score >= score.HighScore);
            var reward = mode?.ComputeReward() ?? new ModeReward();
            if (CoinsEarnedLabel != null) CoinsEarnedLabel.text = $"+{reward.Coins}";
            if (GemsEarnedLabel != null) GemsEarnedLabel.text = $"+{reward.Gems}";
            if (XpEarnedLabel != null) XpEarnedLabel.text = $"+{reward.Xp} XP";
        }

        private void RequestRevive()
        {
            ServiceLocator.TryResolve<AdsManager>()?.ShowRewarded("revive", result =>
            {
                if (result == AdResult.Watched)
                {
                    // Revive: clear the top row so the player gets one more placement.
                    var board = ServiceLocator.Resolve<Board.BoardController>();
                    if (board?.Board != null)
                    {
                        for (int x = 0; x < board.Board.Width; x++)
                            board.Board.Set(x, 0, BoardCell.Empty);
                    }
                    GameManager.Instance.StartRun();
                    gameObject.SetActive(false);
                }
            });
        }

        private void RequestDoubleReward()
        {
            ServiceLocator.TryResolve<AdsManager>()?.ShowRewarded("double_reward", result =>
            {
                if (result == AdResult.Watched)
                {
                    var mode = ServiceLocator.TryResolve<IGameMode>();
                    var reward = mode?.ComputeReward() ?? default;
                    var rs = ServiceLocator.Resolve<RewardSystem>();
                    rs.Grant(reward.Coins, reward.Gems, reward.Xp, "double_reward_ad");
                    Populate();
                }
            });
        }

        private static void Bind(Button b, System.Action act)
        {
            if (b == null) return;
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => { SFXPlayer.Play(SFXPlayer.Click); act?.Invoke(); });
        }
    }
}
