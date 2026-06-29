// ----------------------------------------------------------------------------
// PurchaseReceipt.cs
// Server-side receipt structure. The store returns a JSON blob; we capture
// it, then forward it to our backend for validation. The backend marks the
// receipt as consumed (so it can't be replayed) and returns the granted
// reward, which IAPManager.ApplyPending writes into the wallet.
// ----------------------------------------------------------------------------

using System;

namespace Game.BlockPuzzle.IAP
{
    /// <summary>
    /// Canonical purchase receipt. Constructed locally from the store's
    /// payload and signed by the backend on validation.
    /// </summary>
    [Serializable]
    public sealed class PurchaseReceipt
    {
        public string ProductId;
        public string OrderId;        // Google Play order id
        public string PurchaseToken;  // single-use token used by the backend
        public string Signature;      // base64 signature from the store
        public DateTimeOffset PurchasedAtUtc;
        public string Platform;       // "android" | "ios"
        public string Locale;
        public string AppVersion;

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(ProductId)
                && !string.IsNullOrEmpty(OrderId)
                && !string.IsNullOrEmpty(PurchaseToken);
        }

        public string CanonicalString()
        {
            // Stable string used as the request payload to the validation
            // backend. Keep field order fixed so HMAC matches on both sides.
            return $"{Platform}|{ProductId}|{OrderId}|{PurchaseToken}|{PurchasedAtUtc:O}|{AppVersion}";
        }
    }
}
