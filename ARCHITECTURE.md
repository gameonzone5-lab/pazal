# BlockCraft: Cube Master — Architecture

## Overview

**BlockCraft: Cube Master** is a 100% original block puzzle game for Android, built with Unity 6 LTS. The architecture is modular, event-driven, and designed to run at 60 FPS on devices with as little as 2 GB of RAM.

## Branding (original, not derived from any existing title)

- Name: **BlockCraft: Cube Master**
- Tagline: "Place. Clear. Master the Cube."
- Color identity: violet → coral gradient on near-black (light) or charcoal (dark) backgrounds.
- Icon motif: an isometric stack of 3 cubes with a glowing corner cell.

## High-Level Architecture

```
┌────────────────────────────────────────────────────────────────┐
│                       Bootstrap (RuntimeInitializeOnLoad)       │
│     Creates root GameObject, installs services, loads config.   │
└────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌────────────────────────────────────────────────────────────────┐
│                       Service Locator (DI)                       │
│  GameManager │ Board │ Audio │ Ads │ IAP │ Cloud │ Theme │ ...  │
└────────────────────────────────────────────────────────────────┘
                                │
        ┌───────────────────────┼─────────────────────────┐
        ▼                       ▼                         ▼
   Core Systems           Gameplay Layer              Presentation
   (lifetime, save,     (Board, Pieces, Modes,    (UI, Animations,
   economy, audio)        Scoring, Specials)        Particles, FX)
```

## Module Map

| Namespace                 | Responsibility                                         |
|---------------------------|--------------------------------------------------------|
| `Game.BlockPuzzle.Bootstrap` | Entry point, dependency wiring, scene loading      |
| `Game.BlockPuzzle.Core`      | Singleton controllers, lifecycle, event bus        |
| `Game.BlockPuzzle.Config`    | ScriptableObject data definitions                   |
| `Game.BlockPuzzle.Board`     | Grid, cells, line detection, special-cell logic     |
| `Game.BlockPuzzle.Blocks`    | Piece definitions, drag input, placement            |
| `Game.BlockPuzzle.Scoring`   | Score, combos, chains, multiplier                   |
| `Game.BlockPuzzle.Modes`     | Endless / Timed / Relax / Adventure / Daily          |
| `Game.BlockPuzzle.Economy`   | Wallet, transactions, rewards, currency rules       |
| `Game.BlockPuzzle.Save`      | Encrypted JSON persistence, migration, backups      |
| `Game.BlockPuzzle.Audio`     | Music, SFX pools, ducking on focus loss             |
| `Game.BlockPuzzle.UI`        | Screens, HUD, menus, settings                       |
| `Game.BlockPuzzle.Theme`     | Palettes, color-blind filters, dark/light           |
| `Game.BlockPuzzle.Visuals`   | Particle pools, animation sequencer, screen FX     |
| `Game.BlockPuzzle.Missions`  | Mission tracking, progress, claims                  |
| `Game.BlockPuzzle.Events`    | Daily / weekly / seasonal events                    |
| `Game.BlockPuzzle.Progression` | Battle pass, daily login, lucky spin              |
| `Game.BlockPuzzle.Profile`   | Player profile, statistics, achievements            |
| `Game.BlockPuzzle.Ads`       | AdMob wrapper + consent (UMP)                       |
| `Game.BlockPuzzle.IAP`       | Google Play Billing v7 + receipt validation         |
| `Game.BlockPuzzle.Cloud`     | Firebase auth, Firestore, Crashlytics, Play Games   |
| `Game.BlockPuzzle.Analytics` | Event taxonomy + dispatch                           |
| `Game.BlockPuzzle.Security`  | Encrypted storage, anti-cheat, secure RNG           |
| `Game.BlockPuzzle.Utils`     | Object pools, async helpers, JSON crypto            |
| `Game.BlockPuzzle.Localization` | Key/value lookups (English baseline)            |

## Event Bus

A single static `EventBus` exposes strongly-typed `Publish<T>` / `Subscribe<T>` methods. UI listens; gameplay emits. Avoids tight coupling and lets us swap any system without touching others.

## State Machine

`GameStateMachine` controls high-level state:

```
Boot → MainMenu → ModeSelect → Playing ↔ Paused
                            → GameOver → (Revive | Continue | Menu)
                            → Result → Menu
```

## Persistence Strategy

1. Hot state (wallet, current run) → encrypted JSON to `Application.persistentDataPath/save.dat`.
2. Statistics, achievements → Firestore when online, queued locally when offline.
3. Cross-device sync → Firestore document `users/{uid}/profile/save`.
4. Migrations registered as `ISaveMigration` with explicit version numbers.

## Threading

- All gameplay on Unity main thread.
- File I/O and crypto on `Task.Run`, marshalled back via `Awaitable.MainThreadAsync`.
- No `Thread.Sleep`, no `lock` on shared Unity objects.

## Performance Targets

| Device tier       | Target FPS | Resolution scale |
|-------------------|-----------|-------------------|
| Low (2 GB RAM)    | 60        | 0.75              |
| Mid (4 GB RAM)    | 60        | 1.0               |
| High (8+ GB RAM)  | 60        | 1.0 + AA          |

The `AdaptivePerformanceTuner` chooses sprite atlas compression, particle budgets, and shader LOD based on `SystemInfo.systemMemorySize`.

## Determinism

`SecureRandom` uses `RandomNumberGenerator` for piece generation. Player seed is mixed into game-seed so daily challenges are reproducible across players with the same challenge ID.

## Anti-Cheat

- Score deltas are sanity-checked against board state.
- Time-based modes use `Stopwatch` rather than `Time.deltaTime` accumulated floats.
- All rewards pass through a `RewardValidator` that knows the legal sources per event.

## File / Folder Layout

See `BUILD_INSTRUCTIONS.md` and the source tree under `Assets/`.
