// ----------------------------------------------------------------------------
// AdsManager.cs
// Wraps Google Mobile Ads SDK (AdMob). Handles banner, interstitial,
// rewarded, app-open, and native ads. Enforces cooldown rules and
// "remove ads" ownership.
//
// IMPORTANT: this implementation is the game-side wrapper. When the
// `com.google.unity.mobile-ads` package is added to the project, uncomment
// the marked sections to wire up real banners / interstitials / rewarded.
// ----------------------------------------------------------------------------

using System;
using Game.BlockPuzzle.Analytics;
using Game.BlockPuzzle.Config;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Save;
using UnityEngine;

namespace Game.BlockPuzzle.Ads
{
    public enum AdResult { NotReady, Skipped, Watched, Failed }

    public sealed class AdsManager : MonoBehaviour, IService
    {
        private AdUnitConfig _config;
        private float _lastInterstitialTime = -999f;
        private int _placementsSinceLastInterstitial;
        private int _rewardedAdsToday;
        private DateTime _today;

        public bool AdsRemoved => ServiceLocator.Resolve<SaveManager>().Current.RemoveAdsOwned;
        public bool HasPersonalizedConsent
        {
            get
            {
                var s = ServiceLocator.Resolve<SaveManager>().Current.Settings;
                return s.PersonalizedAds;
            }
        }

        public void Initialize()
        {
            _config = GameConfig.Instance != null ? GameConfig.Instance.Ads : null;
            _today = DateTime.UtcNow.Date;
            EventBus.Subscribe<ConsentUpdatedEvent>(_ => OnConsentUpdated());

            // Wire ad-mob real init here when the package is added:
            //
            // MobileAds.Initialize(initStatus => { });
            //
            // RequestConfiguration config = new RequestConfiguration
            // {
            //     TagForChildDirectedTreatment = TagForChildDirectedTreatment.True,
            // };
            // MobileAds.SetRequestConfiguration(config);
        }

        public void Shutdown() { }

        public void OnConsentUpdated()
        {
            if (!ServiceLocator.Resolve<ConsentManager>().CanRequestAds)
            {
                Log.Info("Ads", "Consent denied — pausing ad requests");
                return;
            }
            // Refresh ad unit requests here.
        }

        // --------------------------------------------------------------------
        // Banner
        // --------------------------------------------------------------------
        public void ShowBanner()
        {
            if (AdsRemoved) return;
            // Implement with AdMob:
            //   _bannerView = new BannerView(_config.BannerAndroid, AdSize.Banner, AdPosition.Bottom);
            //   _bannerView.LoadAd(new AdRequest());
        }

        public void HideBanner()
        {
            // _bannerView?.Destroy();
        }

        // --------------------------------------------------------------------
        // Interstitial
        // --------------------------------------------------------------------
        public bool CanShowInterstitial()
        {
            if (AdsRemoved) return false;
            if (!ServiceLocator.Resolve<ConsentManager>().CanRequestAds) return false;
            if (Time.unscaledTime - _lastInterstitialTime < _config.InterstitialCooldownSeconds) return false;
            if (_placementsSinceLastInterstitial < _config.InterstitialMinPlacements) return false;
            return true;
        }

        public void TryShowInterstitial(string placement, int score = 0)
        {
            if (AdsRemoved) return;
            if (score < _config.InterstitialMinScoreForGameOver) return;
            if (!CanShowInterstitial()) return;
            // Show the ad:
            //   if (_interstitialAd != null && _interstitialAd.CanShowAd())
            //   {
            //       _interstitialAd.Show();
            //       _lastInterstitialTime = Time.unscaledTime;
            //       _placementsSinceLastInterstitial = 0;
            //       ServiceLocator.Resolve<AnalyticsManager>().LogEvent(Constants.AnalyticsEventAdWatched,
            //           new { placement, format = "interstitial" });
            //   }
            _lastInterstitialTime = Time.unscaledTime;
            _placementsSinceLastInterstitial = 0;
            ServiceLocator.Resolve<AnalyticsManager>().LogEvent(Constants.AnalyticsEventAdWatched,
                ("placement", placement), ("format", "interstitial"));
        }

        public void NotifyPiecePlaced()
        {
            _placementsSinceLastInterstitial++;
        }

        // --------------------------------------------------------------------
        // Rewarded
        // --------------------------------------------------------------------
        public void ShowRewarded(string placement, Action<AdResult> onComplete)
        {
            if (AdsRemoved) { onComplete?.Invoke(AdResult.Skipped); return; }
            EnsureDailyCounter();
            if (_rewardedAdsToday >= Constants.MaxRewardedAdsPerDay)
            {
                onComplete?.Invoke(AdResult.Skipped);
                return;
            }
            // Real call:
            //   if (_rewardedAd != null && _rewardedAd.CanShowAd())
            //   {
            //       _rewardedAd.Show(userEarnedReward =>
            //       {
            //           if (userEarnedReward != null)
            //           {
            //               _rewardedAdsToday++;
            //               onComplete?.Invoke(AdResult.Watched);
            //           }
            //           else onComplete?.Invoke(AdResult.Skipped);
            //       });
            //   }
            //   else onComplete?.Invoke(AdResult.NotReady);
            _rewardedAdsToday++;
            ServiceLocator.Resolve<AnalyticsManager>().LogEvent(Constants.AnalyticsEventAdWatched,
                ("placement", placement), ("format", "rewarded"));
            onComplete?.Invoke(AdResult.Watched);
        }

        private void EnsureDailyCounter()
        {
            var today = DateTime.UtcNow.Date;
            if (today != _today)
            {
                _today = today;
                _rewardedAdsToday = 0;
            }
        }

        // --------------------------------------------------------------------
        // App-open
        // --------------------------------------------------------------------
        public void ShowAppOpenIfReady()
        {
            if (!_config.AppOpenOnColdStart) return;
            if (AdsRemoved) return;
            // _appOpenAd?.Show();
        }

        // --------------------------------------------------------------------
        // Native
        // --------------------------------------------------------------------
        // Native ads are loaded on demand by the UI layer. Hook in here.
    }
}
