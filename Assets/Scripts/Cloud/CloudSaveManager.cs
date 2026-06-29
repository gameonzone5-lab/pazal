// ----------------------------------------------------------------------------
// CloudSaveManager.cs
// Bidirectional sync of SaveData between device and Firestore. Performs an
// upload (debounced) after every save flush and a download on app start.
//
// When the player is offline, local writes accumulate and are uploaded on
// reconnect. Conflicts are resolved by timestamp (server wins on ties).
//
// Firebase wiring is left as commented stubs; the public API is stable.
// ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Save;
using UnityEngine;

namespace Game.BlockPuzzle.Cloud
{
    public sealed class CloudSaveManager : MonoBehaviour, IService
    {
        public bool IsSignedIn { get; private set; }
        public DateTimeOffset LastUploadUtc { get; private set; }
        public DateTimeOffset LastDownloadUtc { get; private set; }

        public event Action OnSignedInChanged;

        public async Task InitializeAsync()
        {
            // Stub: actually call FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync()
            // or restore the saved provider (Google / Apple / Facebook).
            // After sign-in, fetch the user id and read users/{uid}/profile/save.
            await Task.Yield();
            IsSignedIn = false;
            OnSignedInChanged?.Invoke();
            Log.Info("CloudSave", "Initialized (stub)");
        }

        public void Shutdown() { }

        public void OnQuit()
        {
            // Flush on quit is handled by SaveManager.
        }

        public void TrySignIn()
        {
            // FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(...)
        }

        public void SignOut()
        {
            IsSignedIn = false;
            OnSignedInChanged?.Invoke();
        }

        public async Task UploadAsync()
        {
            if (!IsSignedIn) return;
            var save = ServiceLocator.Resolve<SaveManager>();
            // var doc = db.Collection("users").Document(uid);
            // await doc.SetAsync(save.Current, SetOptions.MergeAll);
            LastUploadUtc = DateTimeOffset.UtcNow;
            await Task.Yield();
            Log.Info("CloudSave", "Upload complete (stub)");
        }

        public async Task DownloadAsync()
        {
            if (!IsSignedIn) return;
            // var snap = await db.Collection("users").Document(uid).GetSnapshotAsync();
            // if (snap.Exists) save.Replace(snap.ConvertTo<SaveData>());
            LastDownloadUtc = DateTimeOffset.UtcNow;
            await Task.Yield();
        }
    }
}
