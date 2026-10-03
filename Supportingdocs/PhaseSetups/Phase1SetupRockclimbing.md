# Rock Climbing – Phase 1 Setup Guide

## Overview

This guide walks you through building the Rock Climbing game in Unity. You'll create:

- A procedurally generated 10m climbing wall with colour-coded holds
- XRI ClimbInteractable on each hold — locomotion handled entirely by XRI
- A rest platform at the top that detects when you arrive
- Height tracking, timer, and all-time best height
- "New Route" button to regenerate holds

**Philosophy:** Let XRI do the climbing. Your XR Origin already has ClimbProvider — we just generate the wall and holds, and the rest happens automatically.

**Estimated Setup Time:** 30–45 minutes

---

## What You Need

### Scripts Already Created
- ✅ `RockClimbing/Scripts/RockClimbingGameManager.cs`
- ✅ `RockClimbing/Scripts/ClimbingSectionGenerator.cs`
- ✅ `RockClimbing/Scripts/ClimbingHold.cs`
- ✅ `RockClimbing/Scripts/RestPlatform.cs`

### Packages Required
- XR Interaction Toolkit 3.2.1 (already installed)
- TextMeshPro

---

## Setup Steps

---

### Step 1: Verify Your XR Origin

Your XR Origin prefab already includes everything needed for climbing.

**1.1 — Confirm ClimbProvider exists**
1. Expand `XR Origin` in Hierarchy → find the **Locomotion** child GO
2. Confirm it has a **Climb Provider** component — if missing, Add Component → XR → Locomotion → **Climb Provider**

**1.2 — Confirm Direct Interactors on hands**
1. Expand `XR Origin → Camera Offset → Left/Right Controller`
2. Each hand must have an **XR Direct Interactor** (not Ray Interactor)
3. Direct contact with the hold is required — ClimbInteractable does not work with Ray Interactors

> ⚠️ Do NOT use Ray Interactors for climbing. The hands must physically touch the hold collider.

---

### Step 2: Create the Rock Climbing Manager

**2.1 — Create the root GameObject**
1. Right-click Hierarchy → **Create Empty**, rename **`RockClimbingManager`**
2. Set Transform:
   - **Position:** `(0, 0, 2)` — the wall generates forward from here

**2.2 — Add scripts**
1. Add Component → **Rock Climbing Game Manager**
2. Add Component → **Climbing Section Generator**
3. Add Component → **Audio Source**, uncheck **Play On Awake**

**2.3 — Create the ground level marker**
1. Right-click `RockClimbingManager` → **Create Empty**, rename **`GroundLevel`**
2. Local Position: `(0, 0, 0)` — this marks the floor height the timer uses

> The GameManager measures your height above `GroundLevel`. Position it at the exact floor level of your scene.

---

### Step 3: Create the UI Panel

**3.1 — Create the Canvas**
1. Right-click Hierarchy → **UI → Canvas**, rename **`RockClimbingUI`**
2. **Render Mode:** `World Space`
3. Rect Transform:
   - **Position:** `(0, 2, 0.8)` — visible from player start position, above eye level
   - **Scale:** `(0.002, 0.002, 0.002)`
   - **Width:** `500` | **Height:** `300`

**3.2 — Add background panel**
1. Right-click `RockClimbingUI` → **UI → Panel**
2. Image colour: `(0, 0, 0, 150)` — dark semi-transparent

**3.3 — Status text**
1. Right-click `RockClimbingUI` → **UI → Text - TextMeshPro**, rename **`StatusText`**
2. **Text:** `Grab a hold to start climbing!`
3. **Font Size:** `28` | **Alignment:** Center
4. Rect Transform: `Pos Y: 115` | `Width: 480` | `Height: 50`

**3.4 — Height text**
1. Right-click `RockClimbingUI` → **UI → Text - TextMeshPro**, rename **`HeightText`**
2. **Text:** `Height: 0.0 m`
3. **Font Size:** `40` | **Alignment:** Center
4. Rect Transform: `Pos Y: 55` | `Width: 480` | `Height: 55`

**3.5 — Timer text**
1. Right-click `RockClimbingUI` → **UI → Text - TextMeshPro**, rename **`TimerText`**
2. **Text:** `0:00.0`
3. **Font Size:** `56` | **Alignment:** Center | **Color:** Yellow
4. Rect Transform: `Pos Y: -25` | `Width: 480` | `Height: 70`

**3.6 — All-time stats text**
1. Right-click `RockClimbingUI` → **UI → Text - TextMeshPro**, rename **`AllTimeText`**
2. **Text:** `Best: 0.0 m  |  Attempts: 0  Tops: 0`
3. **Font Size:** `22` | **Alignment:** Center | **Color:** Light grey
4. Rect Transform: `Pos Y: -105` | `Width: 480` | `Height: 45`

**3.7 — New Route button**
1. Right-click `RockClimbingUI` → **UI → Button - TextMeshPro**, rename **`NewRouteButton`**
2. Child text label: `NEW ROUTE` | **Font Size:** `32`
3. Rect Transform: `Pos Y: -150` | `Width: 220` | `Height: 55`

> ⚠️ **Do NOT set OnClick() in the Inspector.** The script wires it in `SetupEventListeners()`. Leave OnClick empty.

**Final UI hierarchy:**
```
RockClimbingUI (Canvas, World Space)
├── Panel             (dark background)
├── StatusText        ("Grab a hold to start climbing!", size 28)
├── HeightText        ("Height: 0.0 m", size 40)
├── TimerText         ("0:00.0", size 56, yellow)
├── AllTimeText       ("Best: 0.0 m  |  ...", size 22, grey)
└── NewRouteButton    ("NEW ROUTE")  ← OnClick: LEAVE EMPTY
```

---

### Step 4: Wire the Inspector

Select **`RockClimbingManager`** and fill in both scripts.

#### Rock Climbing Game Manager

| Section | Field | Assign |
|---------|-------|--------|
| Object References | Section Generator | `RockClimbingManager` (itself — drag the GO) |
| Object References | XR Origin Transform | `XR Origin` root GO ← **NOT the Camera** |
| Ground Reference | Ground Level Marker | `RockClimbingManager/GroundLevel` |
| UI Display | Status Text | `RockClimbingUI/StatusText` |
| UI Display | Height Text | `RockClimbingUI/HeightText` |
| UI Display | Timer Text | `RockClimbingUI/TimerText` |
| UI Display | All Time Text | `RockClimbingUI/AllTimeText` |
| UI Controls | New Route Button | `RockClimbingUI/NewRouteButton` |
| Audio | Audio Source | `RockClimbingManager` (drag the GO) |
| Audio | Grab Sound | (optional AudioClip — short thud/click) |
| Audio | Top Reached Sound | (optional AudioClip — success chime) |

#### Climbing Section Generator

| Section | Field | Assign |
|---------|-------|--------|
| Wall Dimensions | Wall Height | `10` |
| Wall Dimensions | Wall Width | `3` |
| Wall Dimensions | Wall Thickness | `0.3` |
| Hold Grid | Rows | `18` |
| Hold Grid | Columns | `5` |
| Hold Grid | Row Spacing | `0.5` |
| Hold Grid | Start Height | `1.0` |
| Hold Shape | Hold Radius | `0.07` |
| Hold Shape | Min/Max Holds Per Row | `2` / `4` |
| Platform | Platform Depth | `1.5` |
| References | Climb Provider | (optional — leave empty to auto-find) |

---

## Complete Scene Hierarchy

```
Scene
│
├── XR Origin (XR Rig)
│   ├── Camera Offset
│   │   └── Main Camera
│   ├── Left Controller
│   │   └── Direct Interactor      ← required for ClimbInteractable
│   ├── Right Controller
│   │   └── Direct Interactor
│   └── Locomotion
│       └── Climb Provider         ← already present in your prefab
│
├── RockClimbingManager            (GameManager.cs + SectionGenerator.cs + AudioSource)
│   └── GroundLevel                (empty GO, marks floor height)
│
├── RockClimbingUI                 (Canvas, World Space)
│   ├── Panel
│   ├── StatusText
│   ├── HeightText
│   ├── TimerText
│   ├── AllTimeText
│   └── NewRouteButton
│
└── [Generated at runtime by ClimbingSectionGenerator]
    ├── ClimbingWall               (Cube — the flat wall surface)
    ├── RestPlatform               (Cube — ledge at the top)
    │   └── PlatformTrigger        (trigger collider + RestPlatform.cs)
    ├── Hold_0_2                   (Cylinder + Rigidbody + ClimbInteractable + ClimbingHold)
    ├── Hold_1_0
    └── ...                        (up to 18 rows × 5 columns)
```

---

## How It Works (Flow Diagram)

```
Press Play
→ RockClimbingGameManager.Start()
│   └── InitializeGame()
│       ├── ClimbingSectionGenerator.GenerateSection()
│       │   ├── CreateWall()          — Cube, 10m tall, facing player
│       │   ├── CreatePlatform()      — Cube at top + PlatformTrigger child
│       │   └── SpawnHolds()          — 18 rows of 2–4 holds each
│       │       each hold gets: Rigidbody (kinematic) + ClimbInteractable + ClimbingHold
│       └── StatusText = "Grab a hold to start climbing!"

Player grabs a hold (hand touches cylinder, squeezes grip)
→ ClimbInteractable.OnSelectEntered()       ← XRI built-in
│   └── ClimbProvider.StartClimbGrab()      ← XRI moves XR Origin automatically
→ ClimbingHold fires OnGrabbed event
→ GameManager plays grab sound

Player pulls downward — body rises
→ ClimbProvider moves XR Origin counter to hand motion (built-in, no code needed)
→ GameManager.TrackHeight() sees height >= 0.4m
│   └── Timer starts, TotalAttempts++ saved to PlayerPrefs

Player releases hold — both hands free
→ ClimbProvider.FinishClimbGrab() — gravity re-enabled
→ Player falls naturally via CharacterController

Player reaches top — CharacterController enters PlatformTrigger
→ RestPlatform.OnTriggerEnter()
│   └── fires OnPlayerArrived
→ GameManager.HandlePlayerReachedPlatform()
    ├── Timer stops
    ├── TotalCompleted++ saved to PlayerPrefs
    ├── StatusText = "Top reached! 1:23.4"
    └── NewRouteButton re-enables after 3 seconds

Player clicks NEW ROUTE
→ ClearSection() — destroys wall, platform, all holds
→ GenerateSection() — fresh random layout
→ Timer + height reset
```

---

## Inspector Quick Reference

### RockClimbingManager
```
[Game Settings]
├── Climb Start Threshold:    0.4
└── Result Display Time:      3.0

[Object References]
├── Section Generator:        RockClimbingManager (self)
└── XR Origin Transform:      XR Origin  ← root GO, NOT Camera

[Ground Reference]
└── Ground Level Marker:      RockClimbingManager/GroundLevel

[UI Display - Text Elements]
├── Status Text:              RockClimbingUI/StatusText
├── Height Text:              RockClimbingUI/HeightText
├── Timer Text:               RockClimbingUI/TimerText
└── All Time Text:            RockClimbingUI/AllTimeText

[UI Controls - Buttons]
└── New Route Button:         RockClimbingUI/NewRouteButton  ← OnClick: LEAVE EMPTY

[Audio - All Sounds Centralized Here]
├── Audio Source:             RockClimbingManager (drag GO)
├── Grab Sound:               (optional)
├── Release Sound:            (optional)
├── Top Reached Sound:        (optional)
├── New Route Sound:          (optional)
└── Land Sound:               (optional)

[Debug]
└── Debug Mode:               ☐ (enable for grab/height logs in Console)
```

### Climbing Section Generator
```
[Wall Dimensions]
├── Wall Height:              10
├── Wall Width:               3
└── Wall Thickness:           0.3

[Hold Grid]
├── Rows:                     18
├── Columns:                  5
├── Row Spacing:              0.5
└── Start Height:             1.0

[Hold Shape]
├── Hold Radius:              0.07
├── Hold Thickness:           0.05
├── Min Holds Per Row:        2
└── Max Holds Per Row:        4

[Platform]
├── Platform Depth:           1.5
└── Platform Height:          0.25

[References]
└── Climb Provider:           (leave empty — auto-found)
```

---

## Testing Checklist

### Hold Generation
- [x] Press Play → wall, holds, and platform appear in the scene
- [x] Holds are colour-coded: green at bottom, orange in middle, red near top
- [x] Press **NEW ROUTE** → holds disappear and a fresh layout generates
- [x] Two layouts look visually different

### Climbing
- [x] Move hand to touch a hold → hand recognises it (hover colour changes)
- [x] Squeeze grip button → body starts moving upward as you pull down
- [x] Both hands can grab different holds simultaneously
- [x] Releasing both hands → you fall naturally under gravity
- [x] No errors in Console during climbing

### Timer & Height
- [ ] Timer does NOT start until you move 0.4m above the ground
- [ ] HeightText updates in real time as you climb *(UI visibility issue — needs wrist/arm mount)*
- [ ] AllTimeText shows "Best: X.Xm" updating as you reach new heights

### Finish (Gold Bar → Teleport)
- [x] Gold finish holds visible at the top of the wall
- [x] Grabbing a finish hold and releasing → teleports to rest platform behind wall
- [ ] "Top reached!" message appears with your time
- [x] NEW ROUTE button works

### Persistence
- [ ] Reach the top, note your best height
- [ ] Exit Play Mode, re-enter → AllTimeText shows previous best

---

## Common Issues & Solutions

### "My hands pass through holds without grabbing"
**Likely cause:** Using Ray Interactor instead of Direct Interactor, or hands aren't close enough.

**Fix:**
1. Confirm each hand has an **XR Direct Interactor** component
2. ClimbInteractable **Max Interaction Distance** defaults to 0.1m — your hand must physically touch the hold
3. Check **Interaction Layers** on the Direct Interactor and ClimbInteractable both include Default

---

### "Body doesn't move when I grab a hold"
**Likely cause:** ClimbProvider not found, or not enabled.

**Fix:**
1. Find your Locomotion GO → confirm **Climb Provider** component exists and is enabled
2. In Climbing Section Generator → **Climb Provider** field → drag the ClimbProvider component in explicitly
3. Confirm **Locomotion Mediator** (if present) lists ClimbProvider

---

### "Timer never starts"
**Likely cause:** XR Origin Transform is set to the Camera instead of the root XR Origin.

**Fix:**
1. Select RockClimbingManager → **XR Origin Transform** must be the **root `XR Origin` GO** (the one that physically moves), not `Camera Offset` or `Main Camera`
2. Confirm `GroundLevel` Y position matches the actual scene floor

---

### "Top reached never triggers / platform does nothing"
**Likely cause:** CharacterController not entering the PlatformTrigger zone.

**Fix:**
1. At runtime, find **PlatformTrigger** in Hierarchy (child of RestPlatform)
2. Enable Debug Mode on GameManager and check Console for platform logs
3. If trigger too small: increase BoxCollider size on PlatformTrigger
4. Confirm your XR Origin has a **Character Controller** component (required for trigger detection)

---

### "Hold colours all look wrong / stay grey"
**Likely cause:** Project uses Built-in Render Pipeline instead of URP.

**Fix:**
- Open `ClimbingHold.cs` → find `Shader.PropertyToID("_BaseColor")` → change `_BaseColor` to `_Color`

---

### "No ClimbProvider found" warning in Console
**Likely cause:** ClimbProvider not in scene or not on an active GO.

**Fix:**
1. Drag your **Climb Provider** component explicitly into the Generator's **Climb Provider** field
2. Avoids the auto-find and silences the warning

---

## Tuning Guide

### Hold Density
Adjust in **Climbing Section Generator:**
- **Rows / Row Spacing:** 18 rows at 0.5m = holds every 50cm up a 10m wall. Reduce rows or increase spacing for sparser routes.
- **Min/Max Holds Per Row:** `2/4` = medium. Try `3/5` for easier, `1/3` for expert.
- **Columns:** 5 columns across 3m = one hold option every 60cm horizontally. More columns = more route variety.

### Hold Size
- **Hold Radius:** `0.07` (7cm). Larger = easier to grab. Range: 0.05–0.12.
- **Hold Thickness:** `0.05` — how far holds protrude from the wall. More protrusion = easier to see and touch.

### Grab Sensitivity
In each hold's **ClimbInteractable** component (set at runtime):
- **Filter Interaction By Distance:** ON by default
- **Max Interaction Distance:** `0.1` — increase to `0.15` if holds feel hard to grab

### Climbing Feel
In **XR Origin → Locomotion → Climb Provider:**
- **Enable Gravity On Climb End:** ON = you fall when releasing. OFF = hover in place (no falling).

---

## Files Created

### Scripts (4 files)
✅ `RockClimbing/Scripts/RockClimbingGameManager.cs`
✅ `RockClimbing/Scripts/ClimbingSectionGenerator.cs`
✅ `RockClimbing/Scripts/ClimbingHold.cs`
✅ `RockClimbing/Scripts/RestPlatform.cs`

### Documentation
✅ `RockClimbing/Phase1Setup.md` — this file
✅ `RockClimbing/RockClimbingPlan.md` — full 4-phase plan
✅ `RockClimbing/PotentialFeatures.md` — feature backlog

### Prefabs (create after testing)
- [ ] `RockClimbing/Prefabs/RockClimbingManager.prefab`
- [ ] `RockClimbing/Prefabs/RockClimbingUI.prefab`

---

## Next Steps (Phase 2+)

See `PotentialFeatures.md` for the full roadmap. Top priorities:

- 🎮 **Haptic feedback** — pulse on grab via `SendHapticImpulse`
- 🔊 **Wind audio** — ambient sound volume scales with height
- ♾️ **Infinite generation** — spawn next section as player climbs, despawn below
- 📍 **Fall-to-platform** — land on last passed platform instead of ground
