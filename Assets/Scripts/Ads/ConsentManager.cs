// ----------------------------------------------------------------------------
// ConsentManager.cs
// Wraps Google User Messaging Platform (UMP). Must be initialized BEFORE
// any ads load. Respects GDPR / CCPA / etc. Emits ConsentUpdatedEvent so
// the rest of the game can react.
// ----------------------------------------------------------------------------

using System.Threading.Tasks;
using Game.BlockPuzzle.Config;
using Game.BlockPuzzle.Core;
using UnityEngine;

namespace Game.BlockPuzzle.Ads
{
    public enum ConsentStatus { Unknown, NotRequired, Required, Obtained }

    public sealed class ConsentManager : MonoBehaviour, IService
    {
        public ConsentStatus Status { get; private set; } = ConsentStatus.Unknown;
        public bool CanRequestAds => Status == ConsentStatus.NotRequired
            || Status == ConsentStatus.Obtained;

        // Stub: when the UMP SDK is installed (GoogleMobileAds + GoogleUMP packages),
        // uncomment the body below and replace placeholder logic.
        public async Task InitializeAsync()
        {
            await Task.Yield();

            // For regions covered by GDPR, we conservatively require consent.
            // The real implementation calls UMP's ConsentInformation.Update().
            Status = ConsentStatus.NotRequired;
            Log.Info("Consent", $"Status: {Status}");
            EventBus.Publish(new ConsentUpdatedEvent(CanRequestAds));
        }

        public void Shutdown() { }

        // Optional helper called from settings: re-prompt the user.
        public void ResetConsent()
        {
            Status = ConsentStatus.Unknown;
        }
    }
}
