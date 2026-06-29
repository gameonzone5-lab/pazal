# Assets & Content Guide

This guide explains how to author the art, audio, ScriptableObject content,
and store assets for **BlockCraft: Cube Master**.

All content is **100% original**. Do not copy art or sound from any other
game — your Play Store submission will be rejected and you may face a
copyright strike.

---

## 1. Branding (do this first)

| Asset                | Notes                                                                       |
|----------------------|------------------------------------------------------------------------------|
| Game name            | **BlockCraft: Cube Master** (exactly as written)                             |
| Logo                 | Isometric stack of 3 cubes with a glowing corner. Two-color: violet + coral. |
| Primary colors       | Violet `#8B73E5`, Coral `#F25D6A`, Charcoal `#171821`, Cream `#FAF7F0`       |
| Typography           | Use a geometric sans for UI (e.g. Manrope, Outfit, Public Sans). Free weights only. |
| App icon             | Adaptive icon: 432×432 base, 108×108 safe zone, 50% padding around art.      |

Tools:
- Figma (free)
- Inkscape + GIMP (free)
- Procreate / Affinity Designer 2 (paid)

Export:
- App icon: PNG, sRGB, no transparency (Android needs solid background)
- Adaptive icon foreground: PNG, transparent, 432×432
- Adaptive icon background: solid color or full-bleed image at 432×432
- Store feature graphic: 1024×500 PNG

---

## 2. Block sprites

The runtime renders cells via **UnityEngine.UI.Image** with `BlockColorPalette`
colors. You can ship without external sprites by relying on solid colors with
a rounded-rectangle UI sprite (built into Unity: `UISprite`). For higher
visual fidelity, ship PNG sprites per color.

### Option A: Ship without external sprites (fastest path)

1. Open `BoardView` in the scene.
2. Assign the **CellView** prefab's `_bg` Image to use Unity's default UI
   sprite (`UISprite`).
3. The runtime paints the color from `BlockColorPalette`; you get flat
   colored squares. This is what most successful indie block puzzle games
   ship with.

### Option B: Custom sprites (recommended)

Author 8 sprites, one per palette color (or use a sprite atlas with named
slots). Export at 1× and 2× for sharp rendering on 1080p+ devices.

Naming: `block_color_0.png` … `block_color_7.png`, plus
`block_bomb.png`, `block_rainbow.png`, `block_frozen.png`, `block_locked.png`.

Place under `Assets/Textures/Blocks/`.

In `BoardView` prefab, swap the Image sprite to the matching sprite per cell
type. The runtime passes the color in via `Image.color` so the sprite stays
neutral and the color tints it.

Sprite import settings:
- **Texture Type:** Sprite (2D and UI)
- **Pixels Per Unit:** 100
- **Compression:** ETC2 (Android)
- **Max Size:** 256
- **Generate Mipmaps:** off
- **sRGB:** on

---

## 3. UI art

### Buttons

Use Unity's built-in `UISprite` (rounded rect) or build your own set of
9-slice sprites:
- `btn_primary_default.png`
- `btn_primary_pressed.png`
- `btn_primary_disabled.png`
- `btn_secondary_default.png` …

9-slice borders: 8 px on each side, with the central area stretchable.

### Backgrounds

Ship **two** background gradients (dark + light) or use the `Background`
color from your `ThemeConfig` asset and paint with Canvas + Image.

If you want a layered parallax:
- `bg_layer_far.png` (mountains / clouds, 1080×1920)
- `bg_layer_mid.png` (midground silhouette)
- `bg_layer_near.png` (foreground particles, animated)

Pack them in a sprite atlas to keep draw calls low.

### Coin / Gem icons

- `icon_coin.png` — gold disc with a "C" or geometric mark
- `icon_gem.png` — faceted crystal, animated rotation optional
- `icon_energy.png` — battery shape, full / half / empty states
- `icon_xp.png` — star or chevron-up

All as 256×256 PNG with transparent backgrounds.

---

## 4. Particle FX

Create four ParticleSystem prefabs (one per effect), all in `Assets/Prefabs/FX/`:

### Line clear
- Emission: burst of 12 particles when `Play()` is called
- Shape: cone, narrow
- Color: `Theme.LineClearFx` over `Theme.Primary`
- Size: 0.05 → 0.15 over 0.5s
- Lifetime: 0.4s
- Gravity: 0
- Renderer: Sprites/Default

### Bomb burst
- Emission: 30 particles, instant
- Shape: sphere, radius 0.3
- Color: black → orange → transparent
- Size: 0.1 → 0.3 over 0.7s
- Lifetime: 0.7s
- Renderer: soft additive

### Rainbow burst
- 60 particles, 0.6s lifetime
- Color over lifetime: rainbow gradient
- Velocity: outward radial, 1.5 m/s
- Renderer: stretched billboard

### Coin burst
- 20 coin sprites flying up with gravity
- 0.6s lifetime
- Tweens size from 1.0 → 0.0 in last 0.2s

Assign each to the matching field on `ParticleService`:
```
LineClearPrefab → LineClear
BombPrefab → BombBurst
RainbowPrefab → RainbowBurst
CoinPrefab → CoinBurst
```

---

## 5. Audio

### Music (1–3 tracks)

We recommend three looping stems:

| Track id     | Mood                          | Suggested length  |
|--------------|-------------------------------|-------------------|
| `music_menu` | Calm, hopeful                 | 2–3 min loop      |
| `music_play` | Up-tempo, focused             | 2–3 min loop      |
| `music_shop` | Bright, "casino" energy       | 1–2 min loop      |

Format: OGG Vorbis, 96 kbps, mono. Loop must be perfectly seamless — use
Audacity's loop preview tool to verify.

### SFX

Ship **at least** these SFX ids (referenced by `SFXPlayer.cs`):

| Id                | When it's played                              | Length |
|-------------------|-----------------------------------------------|--------|
| `click`           | Every button tap                              | < 0.1s |
| `pop`             | Toggle switch                                 | < 0.1s |
| `place`           | Piece dropped on board (no clear)             | < 0.2s |
| `place_with_clear`| Piece dropped that cleared lines              | < 0.3s |
| `combo`           | Combo badge appears                           | < 0.3s |
| `bomb_explode`    | Bomb triggered                                | < 0.5s |
| `rainbow_clear`   | Rainbow triggered                             | < 0.5s |
| `game_over`       | Run ends                                      | 1–2 s  |
| `reward`          | Any reward granted                            | < 0.5s |
| `coin_tick`       | Coin balance changes in shop                  | < 0.1s |
| `purchase`        | IAP completed                                 | < 0.5s |

Add a `UiBank` and `GameplayBank` `AudioBank` asset each and assign the
SfxClips. The `AudioConfig.FindById` lookup uses the SfxClip's `Id` field.

Format: OGG Vorbis, 96 kbps, mono, 44.1 kHz.

### Sources for original audio

- **Free with attribution:** freesound.org, OpenGameArt (CC0 / CC-BY).
- **Free no-attribution:** sonniss GDC packs, Indie Game Music Pack.
- **Paid:** A Sound Effect, AudioJungle (perpetual license, ~$5–30 per clip).
- **AI-generated:** Suno, Udio, ElevenLabs SFX — verify commercial-use rights.

---

## 6. ScriptableObject content

### Block shapes (BlockShapeLibrary)

A "balanced" library has **10–14 shapes**, weighted by frequency:

| Id           | Shape             | Weight | Notes                          |
|--------------|-------------------|--------|---------------------------------|
| `s_1x1`      | 1×1 dot           | 5      | Always available filler         |
| `s_1x2`      | 1×2 domino        | 10     |                                 |
| `s_1x3`      | 1×3 line          | 8      |                                 |
| `s_1x4`      | 1×4 line          | 5      |                                 |
| `s_1x5`      | 1×5 line          | 3      |                                 |
| `s_2x2`      | 2×2 square        | 6      |                                 |
| `s_L_small`  | 3×3 L (corner)    | 6      |                                 |
| `s_L_big`    | 4×4 L (corner)    | 3      |                                 |
| `s_T`        | 3×3 T             | 4      |                                 |
| `s_S`        | 3×3 S             | 4      |                                 |
| `s_Z`        | 3×3 Z             | 4      |                                 |
| `s_5dot`     | Plus (5 dots)     | 1      |                                 |
| `s_5x1`      | 5×1 vertical      | 1      |                                 |
| `s_U`        | 3×3 U             | 2      |                                 |

Weights are relative. Total weight sum doesn't matter; the picker uses
modular random.

Set `CanRotate = true` on shapes that look good rotated; `CanMirror = true`
on shapes with non-symmetric variants.

### Levels (LevelLibrary)

Author 20 starter levels. Each level needs:
- `LevelNumber` (1..20)
- `TargetScore` (ramps from 200 to 5000)
- `LinesToClear` (e.g. `new[] {1,2,3}` for "clear 1, then 2, then 3 to pass")
- `MaxPieces` (-1 for default)
- `StartingLives` (default 5)
- `AllowBomb` / `AllowRainbow`
- `InitialSpecials` (Locked / Frozen cells to set up the puzzle)

Example for **Level 1** (Tutorial):
```
LevelNumber = 1
DisplayName = "First Steps"
TargetScore = 200
LinesToClear = { 1 }
InitialSpecials = []
AllowBomb = false
AllowRainbow = false
Hint = "Drag a piece onto the board. Fill a row or column to clear it."
```

### Missions (MissionLibrary)

Author 10 daily missions and 10 weekly missions.

### Daily login (DailyLoginConfig)

Author 28 days of rewards. Days 7, 14, 21, 28 should be `IsMilestone = true`
with gems as the reward.

### Lucky spin (LuckySpinConfig)

Author 8 slots with weights summing to ~100. At least one `IsJackpot = true`.

### Battle pass (BattlePassConfig)

Author 30 tiers. Free reward every tier; premium reward every tier.

---

## 7. Store assets

### Required by Play Console

- **App icon** — 512×512 PNG, 32-bit
- **Feature graphic** — 1024×500 PNG
- **Phone screenshots** — at least 4, 16:9 or 9:16 (portrait), 1080×1920 minimum
- **Short description** — 80 chars max
- **Full description** — 4000 chars max
- **Privacy policy URL** — hosted publicly (GitHub Pages works)

### Recommended

- 7" tablet screenshots
- 10" tablet screenshots
- Promo video (YouTube, 30s)

### Categories

- **Application → Games → Puzzle** (or "Casual" depending on review)

### Localization

Translate the description into at least: English, Spanish, Portuguese,
French, German, Italian, Russian, Japanese, Korean, Chinese (Simplified).
Localazy + GitHub Actions can automate this.

---

## 8. Marketing creatives

### Screenshots checklist

Show, in order:
1. **Main menu** with currency balances
2. **Active gameplay** mid-combo
3. **Special block in action** (Bomb or Rainbow)
4. **Game over screen** with score and rewards
5. **Shop / Battle Pass screen**
6. **Daily challenge reward popup**

Don't:
- Use stock photo "happy players" with fake reactions
- Show misleading UI ("WIN $1000!" or similar)
- Use copyrighted music in promo videos

---

## 9. Compliance & legal

### Age rating

Use Play Console's questionnaire. For a typical puzzle game:
- No violence → none
- No user-generated content → reduces rating
- No real-money gambling → PEGI 3 / ESRB E

### Data safety form

Required for any app that collects data. Our game:
- **Account info** → optional Google sign-in
- **App activity** → anonymous gameplay telemetry
- **App info** → crash logs, diagnostics
- **Device IDs** → SSAID, Advertising ID

Mark "Data is encrypted in transit" = yes.
Mark "You can request data deletion" = yes (Settings → Delete save data).

---

## 10. Versioning & changelogs

Maintain `CHANGELOG.md` at the repo root. Format:

```markdown
## [1.0.0] - 2026-06-29

### Added
- Initial release
- Endless, Timed 3/5, Relax, Adventure modes
- Daily Challenge + 8-event Weekly Event rotation
- Battle Pass Season 1 ("Block Party")
- 30 Adventure levels + 20 beginner missions
- AdMob banner / interstitial / rewarded
- IAP: coin packs, gem packs, remove ads, premium subscription

### Fixed
- n/a

### Removed
- n/a
```

Bump the version in `Constants.cs` and the Android `versionCode` /
`versionName` on every release. Use semantic versioning (`MAJOR.MINOR.PATCH`).
