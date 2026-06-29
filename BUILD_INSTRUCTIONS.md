# Build & Publish Instructions

This document walks you from a fresh clone to a Play Store-ready APK / AAB.

Estimated time: **2–4 hours** for a first-time shipper (most of it is Google
Play Console / Firebase console setup).

---

## 0. Prerequisites

- **Unity 6 LTS** (6000.0.x or newer). Use the Android Build Support
  module during install.
- **Android SDK Platform 34**, Build-Tools 34, NDK 26.x or newer.
- **JDK 17** (Unity 6 requires JDK 17).
- **Gradle 8.x** (Unity bundles a working version).
- A **Google Play Console** account ($25 one-time fee).
- A **Firebase Console** account (free tier is fine).
- A code-signing keystore (generate via `keytool` or Unity's publishing settings).

---

## 1. Clone & open the project

```bash
git clone <your-repo-url> blockcraft
cd blockcraft
```

Then in Unity Hub → **Add** → select the `blockcraft` folder → **Open** with
the Unity 6 LTS version.

---

## 2. Install required Unity packages

`Window → Package Manager → + → Add by name…`

| Package                                                  | Purpose                       |
|----------------------------------------------------------|-------------------------------|
| `com.google.unity.mobile-ads` (latest)                   | AdMob                         |
| `com.google.play.billing` (latest)                       | Google Play Billing v7        |
| `com.google.play.games` (latest for Unity 6)             | Sign-in, leaderboards, achievements |
| `com.google.firebase.*` (Analytics, Auth, Firestore, Messaging, Crashlytics, RemoteConfig) | Backend |
| `com.unity.nuget.newtonsoft-json` (optional)             | Faster JSON parsing if needed |

> Note: Firebase Unity SDKs include their own .unitypackage downloaders on
> the Firebase Console. The cleanest install is via `.unitypackage` import:
> `Assets → Import Package → Custom Package…`.

If you don't want any third-party packages yet, the game compiles and runs in
**offline mode** — all of `AdsManager`, `IAPManager`, `PlayGamesManager`,
`CloudSaveManager`, `AnalyticsManager` ship with stub implementations behind
their public APIs.

---

## 3. Author the ScriptableObject assets

The runtime reads everything from `Resources/GameConfig/GameConfig.asset`.
You must create these ScriptableObject instances and assign references.

In `Project` window: right-click → `Create → BlockCraft → …`

1. **GameConfig** (`Create → BlockCraft → Game Config`)
   - `Shapes` → drag `BlockShapeLibrary` asset
   - `Palette` → drag `BlockColorPalette` asset
   - `Themes` → drag `ThemeSet` asset (optional — defaults are baked in)
   - `Audio` → drag `AudioConfig` asset
   - `Economy` → drag `EconomyConfig` asset (defaults are fine to start)
   - `Missions` → drag `MissionLibrary` asset
   - `Levels` → drag `LevelLibrary` asset (with at least 5 entries)
   - `BattlePass` → drag `BattlePassConfig` asset
   - `DailyLogin` → drag `DailyLoginConfig` asset
   - `LuckySpin` → drag `LuckySpinConfig` asset
   - `Products` → drag `IAPProductLibrary` asset
   - `Ads` → drag `AdUnitConfig` asset
   - `Localization` → drag `LocalizationConfig` asset

2. **ThemeSet** — at minimum assign a `Dark` theme. The game falls back to
   baked colors if any reference is null.

3. **BlockShapeLibrary** — at least 5 distinct shapes (see `docs/ASSETS_GUIDE.md`).

4. **LevelLibrary** — at minimum 5 levels. Empty arrays are tolerated but
   Adventure mode will not function.

5. **AudioConfig** — fill in `GameplayBank`, `UIBank`, `MusicBank`. The runtime
   uses the SfxClip ids listed in `SFXPlayer.cs`.

6. **AdUnitConfig** — paste your AdMob unit ids. Sample test ids ship by
   default; replace with your real ids before publishing.

Save the asset under `Assets/Resources/GameConfig/GameConfig.asset`.

---

## 4. Author the scenes

The project does not ship pre-built scenes (Unity scenes are binary and
template-specific). Create them in the Editor:

| Scene           | Build index | Contents                                           |
|-----------------|-------------|----------------------------------------------------|
| `Boot`          | 0           | Empty; `Bootstrap` runs from `RuntimeInitializeOnLoad` |
| `MainMenu`      | 1           | Canvas with `MainMenuController` and child buttons |
| `Gameplay`      | 2           | `BoardView`, `BlockSpawner`, `HUDController`, `DragHandler`, `BlockPlacer` setup, piece slot anchors |
| `Settings`      | 3           | `SettingsController` canvas                        |
| `Shop`          | 4           | IAP product list canvas                            |
| `Profile`       | 5           | Stats + achievements canvas                        |
| `Leaderboard`   | 6           | Play Games UI (driven by `PlayGamesManager`)       |

Add scenes to **File → Build Profiles → Android → Scene List** in the order
shown.

> The `Bootstrap` static class runs before any scene loads. You do not need
> to add it to a scene — it self-installs.

---

## 5. Wire up services in a scene

Add a single GameObject to your `Boot` (or first) scene named
`[BlockCraft.Root]` with no children; the runtime adds the rest automatically.

If you prefer to wire services manually in the Editor (recommended for
inspecting them at runtime), add these as children of any persistent
GameObject and assign them in the Inspector:

```
[BlockCraft.Root]   (DontDestroyOnLoad)
├── SaveManager
├── AudioManager
├── ThemeManager
├── PlayerProfile
├── StatisticsTracker
├── AchievementManager
├── EconomyManager
├── RewardSystem
├── BoardController
├── BlockSpawner
├── ScoreManager
├── ComboTracker
├── ChainReactor
├── MissionManager
├── DailyChallengeManager
├── WeeklyEventManager
├── BattlePassManager
├── DailyLoginRewards
├── LuckySpin
├── ParticleService
├── AnimationSequencer
├── AdsManager
├── ConsentManager
├── IAPManager
├── CloudSaveManager
├── PlayGamesManager
├── AnalyticsManager
├── AntiCheat
├── HapticManager
└── LocalizationManager
```

Each implements `IService`. `GlobalInstaller.Install()` registers them in
`ServiceLocator`. You can also let `Bootstrap` create them at runtime
(set `GameObject.Find` is used as fallback by `GlobalInstaller`).

---

## 6. Configure Player Settings

`Edit → Project Settings → Player`

- **Company Name** = your company
- **Product Name** = `BlockCraft: Cube Master`
- **Package Name** = `com.blockcraft.cubemaster` (or your own; update `Constants.BundleId` accordingly)
- **Minimum API Level** = 23 (Android 6.0)
- **Target API Level** = 34 (Android 14)
- **Scripting Backend** = IL2CPP
- **Target Architectures** = ARM64 (required by Google Play since Aug 2024); add ARMv7 if you still support very old devices
- **Internet Access** = Required
- **Force Internet Permission** = checked
- **Force SD Card Permission** = unchecked
- **Graphics APIs** = Vulkan (with GLES3 fallback)
- **Color Space** = Linear
- **Quality** = Set Default to Ultra for editor; add a "Low" tier that disables post-processing, shadows, and reduces MSAA to 1x (used on 2 GB devices via `AdaptivePerformanceTuner`)
- **Splash Screen** = Unity default with your logo override
- **Resolution Scaling** = 0.75–1.0; runtime adjusts based on `SystemInfo.systemMemorySize`

`Edit → Project Settings → Player → Publishing Settings`

- **Create a new keystore** (or import your existing one)
- Tick **Custom Main Gradle Template** (so the project uses the included `mainTemplate.gradle`)
- Tick **Custom Main Manifest** (so the included `AndroidManifest.xml` is used)
- Tick **User Proguard File** (uses `proguard-user.txt`)

---

## 7. Configure Firebase

1. Open `https://console.firebase.google.com` → **Add project**.
2. **Add app → Android** with package id `com.blockcraft.cubemaster`.
3. Download `google-services.json` and place it at
   `Assets/Plugins/Android/google-services.json`.
4. Enable **Authentication → Sign-in method → Google** (and optionally Anonymous, Apple).
5. Enable **Firestore Database** (start in production mode; set rules via the backend SDK).
6. Enable **Crashlytics**, **Analytics**, **Cloud Messaging**, **Remote Config**.
7. Copy the **Web API Key** into `AnalyticsManager` if you switch on custom
   event dispatch.

---

## 8. Configure Google Play Games Services

1. Open Play Console → your app → **Play Games Services → Setup and management**.
2. **Configuration → Credentials** → Authorize your app.
3. Note your **Application ID** (numeric) and OAuth client id.
4. Add the IDs to `PlayGamesManager.LbEndless / LbTimed3 / LbDaily` and to
   `AchievementManager` entries' `GooglePlayId`.
5. In **Linked Apps**, link the Android app and provide:
   - SHA-1 of your signing keystore
   - OAuth client IDs for Android + Web

---

## 9. Configure Google AdMob

1. `https://apps.admob.com` → **Add App** (Android).
2. Create 5 ad units: banner, interstitial, rewarded, app-open, native.
3. Paste each id into the `AdUnitConfig` asset.
4. In **Privacy & messaging → GDPR**, set the message text; the Consent
   Manager wraps `ConsentInformation.Update()`.

---

## 10. Configure IAP

1. Play Console → your app → **Monetize → Products → In-app products**.
2. Create SKUs matching the `ProductId` values in your `IAPProductLibrary`:
   - coin packs (consumable)
   - gem packs (consumable)
   - "remove_ads" (non-consumable)
   - "premium_monthly" (subscription)
3. The runtime uses the Google Play Billing v7 API; verify with a license
   tester account before publishing.

---

## 11. First build

`File → Build Profiles → Android → Build` (or `Build And Run` with a device
connected via ADB).

Smoke-test:
- App boots to Main Menu
- Tap **Play** → board renders, drag a piece, line clears, score updates
- Tap **Settings** → toggle theme / colorblind / sound
- Tap **Shop** → product list shows
- Tap **Daily** → daily challenge loads
- Force-quit and re-open → save persists

---

## 12. Pre-launch checklist

- [ ] All AdMob unit ids are **production** (not Google's sample test ids)
- [ ] `RemoveAdsOwned` flag wires correctly when IAP completes
- [ ] `Privacy Policy` URL is set and reachable from Settings
- [ ] `Terms of Service` URL is reachable
- [ ] Analytics events fire for: session start, level start/end, IAP, ad watched, reward claimed
- [ ] Crashlytics is wired (force a test crash, see it land in console)
- [ ] All Play Games leaderboards and achievements have correct ids
- [ ] All audio clips have compressed variants for mobile
- [ ] Sprite atlas is generated; no per-frame texture uploads
- [ ] Tested on a 2 GB RAM device (e.g. low-end Samsung A-series)
- [ ] Tested on landscape/portrait lockout (this game is portrait-only)
- [ ] Tested airplane mode (offline play must not crash)
- [ ] Age rating questionnaire completed in Play Console
- [ ] Data safety form completed (location = no; data shared = ad identifiers, purchase history)
- [ ] App icon and feature graphic uploaded (NOT shipped in this repo)
- [ ] Adaptive icons generated

---

## 13. Submitting to Play Store

1. `File → Build → App Bundle (.aab)` (NOT .apk — Play requires .aab)
2. Upload to **Internal Testing** track first.
3. Run a closed alpha with ~20 testers.
4. Address crashes and Core Vitals warnings.
5. Promote to **Production** when green for 7+ days.

---

## 14. Post-launch operations

- **Remote Config**: tune coin rewards, daily login schedule, ad frequencies
  without shipping a new build.
- **Firestore rules**: tighten over time as you discover abuse vectors.
- **Crashlytics**: triage daily, file follow-up issues for top crashes.
- **Play Games**: rotate leaderboard resets weekly (Cloud Functions).
- **AdMob**: review eCPM weekly; rotate underperforming ad units.

---

## Troubleshooting

### "google-services.json not found"
Make sure it lives at `Assets/Plugins/Android/google-services.json` (NOT a
subfolder). Unity recognizes only files at that exact path.

### "BUILD FAILED: Unable to find dependency"
Run `Assets → Reimport All`, then `Edit → Project Settings → Player → Publishing Settings → Check Dependencies`. Re-open the project once.

### Ads never load
Confirm `ConsentManager.CanRequestAds == true` (check via the debugger).
On first launch you may need to complete the GDPR form once.

### IAP shows but cannot purchase
You need a real license-tester account set up in Play Console. Internal
testing tracks are the only ones where test purchases work.

### Game runs at <60 FPS on 2 GB devices
- Disable post-processing
- Cap particle count via `AdaptivePerformanceTuner`
- Lower `QualitySettings.particleRaycastBudget` to 0
- Ensure `Application.targetFrameRate = 60` (Bootstrap already sets this)

---

## Support

This is an open-source release; there is no paid support. For Unity-specific
issues, see https://docs.unity3d.com. For Play Console, see
https://support.google.com/googleplay/android-developer.
