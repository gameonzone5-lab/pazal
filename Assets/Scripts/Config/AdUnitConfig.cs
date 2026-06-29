// ----------------------------------------------------------------------------
// AdUnitConfig.cs
// AdMob unit ids + ad scheduling rules. Keep the actual production ids in
// Remote Config; this asset carries the fallbacks and developer test ids.
// ----------------------------------------------------------------------------

using UnityEngine;

namespace Game.BlockPuzzle.Config
{
    [CreateAssetMenu(menuName = "BlockCraft/Ad Unit Config", fileName = "AdUnitConfig")]
    public sealed class AdUnitConfig : ScriptableObject
    {
        [Header("Android unit ids")]
        public string BannerAndroid = "ca-app-pub-3940256099942544/6300978111"; // sample test id
        public string InterstitialAndroid = "ca-app-pub-3940256099942544/1033173712";
        public string RewardedAndroid = "ca-app-pub-3940256099942544/5224354917";
        public string AppOpenAndroid = "ca-app-pub-3940256099942544/3419835294";
        public string NativeAndroid = "ca-app-pub-3940256099942544/2247696110";

        [Header("iOS unit ids (for future)")]
        public string BannerIos;
        public string InterstitialIos;
        public string RewardedIos;
        public string AppOpenIos;
        public string NativeIos;

        [Header("Behaviour")]
        [Tooltip("Min seconds between two interstitial impressions.")]
        public float InterstitialCooldownSeconds = 60f;

        [Tooltip("Min placements (piece placements) between two interstitials.")]
        public int InterstitialMinPlacements = 8;

        [Tooltip("Show app-open ad on cold start.")]
        public bool AppOpenOnColdStart = true;

        [Tooltip("Show interstitial after game over only if score > this.")]
        public int InterstitialMinScoreForGameOver = 200;
    }
}
