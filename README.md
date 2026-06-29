# BlockCraft: Cube Master

A **100% original** Android block puzzle game built with **Unity 6 LTS** + **C#**.
Drag and drop block pieces onto a grid; clear lines, build combos, unlock rewards,
and master over 50 game mechanics across 5 game modes.

> **Branding.** "BlockCraft: Cube Master" is an original name, original brand,
> original visual identity, and original code. Nothing in this project is derived
> from any copyrighted third-party game.

---

## Highlights

| Area           | Features                                                                                                  |
|----------------|-----------------------------------------------------------------------------------------------------------|
| Gameplay       | Drag-drop, random pieces, combo system, chain reactions, score multipliers, 4 special blocks (Bomb, Rainbow, Frozen, Locked) |
| Modes          | Endless, Timed (3 + 5 min), Relax, Adventure (50+ levels), Daily Challenge, Weekly events                |
| Visuals        | Modern premium UI, dark / light / colorblind themes, particle FX, screen shake, smooth tween animations |
| Audio          | Music crossfade, SFX pool with pitch randomization, focus-loss muting                                      |
| Player         | Offline play, encrypted cloud save, Google Play Games login, achievements, leaderboards, statistics       |
| Economy        | Coins, gems, energy, daily login, lucky spin, treasure chests, missions, battle pass                      |
| Monetization   | AdMob banner / interstitial / rewarded / app-open / native; IAP coin / gem / subscription / remove-ads; GDPR / UMP consent |
| Backend        | Firebase Auth, Firestore, Analytics, Crashlytics, Remote Config, Cloud Messaging                          |
| Security       | Encrypted local save (AES-GCM w/ CBC fallback), HMAC tamper check, anti-cheat, secure RNG               |
| Performance    | 60 FPS target on 2 GB RAM devices; object pools, no per-frame allocations, adaptive quality               |

---

## Quick start (TL;DR)

1. Install **Unity 6 LTS** (6000.0.x or newer).
2. Open the project folder in Unity Hub → Open.
3. Install required Unity packages (see `BUILD_INSTRUCTIONS.md`):
   - `com.unity.mobile.ad-notifications-support`
   - `com.google.unity.mobile-ads` (AdMob)
   - `com.google.play.billing` (Play Billing)
   - `com.google.play.games` (Play Games)
   - `com.google.firebase.*`
4. Drop `google-services.json` into `Assets/Plugins/Android/`.
5. Create the ScriptableObject assets under `Assets/Resources/GameConfig/`
   (the project ships with placeholder values; see `docs/ASSETS_GUIDE.md`).
6. Build & Run (File → Build Profiles → Android → Build).

Full step-by-step instructions live in `BUILD_INSTRUCTIONS.md`.

---

## Folder layout

```
Assets/
├── Scripts/                (C# source, organized by concern)
│   ├── Bootstrap/          (entry point, DI wiring)
│   ├── Core/               (lifetime, save, events, state)
│   ├── Config/             (ScriptableObject data)
│   ├── Board/              (grid, cells, line clearing, special logic)
│   ├── Blocks/             (piece spawning, drag, placement)
│   ├── Scoring/            (score, combo, chain)
│   ├── Modes/              (endless / timed / relax / adventure / daily)
│   ├── Economy/            (wallet, rewards, transactions)
│   ├── Audio/              (music + SFX)
│   ├── UI/                 (screen controllers)
│   ├── Theme/              (palettes + colorblind)
│   ├── Visuals/            (particles, animation sequencer, screen FX)
│   ├── Missions/           (mission tracker)
│   ├── Events/             (daily challenge + weekly event)
│   ├── Progression/        (battle pass, daily login, lucky spin)
│   ├── Profile/            (player profile, stats, achievements)
│   ├── Ads/                (AdMob wrapper + consent)
│   ├── IAP/                (Google Play Billing v7 wrapper)
│   ├── Cloud/              (Firebase + Play Games wrappers)
│   ├── Analytics/          (event taxonomy)
│   ├── Security/           (anti-cheat, secure RNG)
│   ├── Utils/              (haptics, pools, async helpers)
│   └── Localization/       (key→string lookups)
├── Plugins/Android/        (AndroidManifest.xml, gradle template, proguard)
├── Resources/              (GameConfig, Theme, Localization, Audio assets)
├── Prefabs/                (UI, block piece, FX prefabs)
├── Scenes/                 (MainMenu, Gameplay, Shop, Settings, ...)
├── Materials/              (UI materials, sprite shaders)
├── Shaders/                (custom UI / FX shaders)
├── Textures/               (sprite atlas, icons)
├── Animations/             (AnimationClip assets)
└── Settings/               (URP / Render pipeline assets)
```

---

## Architecture

See `ARCHITECTURE.md` for the full module map, event flow, state machine,
threading model, and persistence strategy.

Key principles:

- **Event-driven.** Gameplay emits typed events on `EventBus`; UI / audio /
  analytics subscribe. Zero tight coupling between modules.
- **Data over behaviour.** Almost everything that designers tune lives in
  ScriptableObject assets under `Resources/GameConfig/`.
- **No `FindObjectOfType` in hot paths.** Services register themselves in a
  `ServiceLocator` at boot.
- **Allocation-free in steady state.** All cell views, particles, and SFX
  sources are pooled.

---

## Documentation

- `ARCHITECTURE.md` — design overview
- `BUILD_INSTRUCTIONS.md` — step-by-step build + publish guide
- `PRIVACY_POLICY.md` — privacy policy template
- `TERMS_OF_SERVICE.md` — terms of service template
- `docs/ASSETS_GUIDE.md` — how to author art, audio, and ScriptableObjects

---

## License

The source code in this project is released under the **MIT License** (see
`LICENSE`). You are free to use, modify, and ship a derivative work, including
commercially.

**You are responsible for** the art assets, sounds, branding, and the legal
metadata you submit to Google Play. The placeholders in this repo are not
copyright-clean for public release — replace them.
