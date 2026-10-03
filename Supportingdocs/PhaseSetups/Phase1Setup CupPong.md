# Beer Pong VR - Phase 1 Setup Guide

## Overview

This guide will walk you through setting up a fully functional Beer Pong mini-game in VR. You'll create:

- ✅ Spawn-on-grip ping pong balls (both hands independently)
- ✅ 10-cup triangle formation with accurate collision detection
- ✅ Ball physics with realistic throwing
- ✅ Audio feedback for table and cup impacts
- ✅ Win condition and reset system

**Philosophy:** Simple, fun physics-based VR beer pong. Grab virtual ping pong balls from thin air and sink them in cups!

**Estimated Setup Time:** 1.5-2 hours

---

## What You Need

### External Dependencies
- ✅ **Unity 6** (or Unity 2021.3+)
- ✅ **XR Interaction Toolkit** (already installed in your project)
- ✅ **TextMeshPro** (built into Unity)
- ✅ **Input System Package** (for grip button detection)

### Scripts Already Created
- ✅ `BeerPongGameManager.cs` - Cup spawning, game state, win condition
- ✅ `PingPongBall.cs` - Grabbable ball with physics
- ✅ `Cup.cs` - Collision detection, removal on score
- ✅ `HandBallSpawner.cs` - Spawn balls on grip press

All located in: `F:\Unity\VR Games\MiniArena\Assets\_MiniGames\BeerPong\Scripts\`

---

## Quick Setup (8 Steps)

### Step 1: Create the Beer Pong Table

**1.1 Create Table Object**
1. Right-click in Hierarchy → **3D Object → Cube**
2. Rename to **"BeerPongTable"**
3. Set Transform:
   - **Position:** (0, 0.75, 3) - 3 meters away, waist height
   - **Rotation:** (0, 0, 0)
   - **Scale:** (0.6, 0.02, 2.4) - 2ft wide x 8ft long (standard beer pong)

**1.2 Add Table Tag**
1. Select **BeerPongTable**
2. Top of Inspector → **Tag → Add Tag**
3. Click **"+"** → Create new tag: **"Table"**
4. Go back to BeerPongTable → Set **Tag: Table**

**1.3 Add Table Physics Material (Medium Bounce)**
1. In Project window → Right-click → **Create → Physics Material**
2. Name it **"TableSurface"**
3. Set values:
   - **Dynamic Friction:** 0.4
   - **Static Friction:** 0.45
   - **Bounciness:** 0.3 (ping pong balls bounce a bit)
   - **Friction Combine:** Average
   - **Bounce Combine:** Average
4. Drag **TableSurface** material onto BeerPongTable's **Box Collider → Material** slot

**Visual Check:** You should now have a long rectangular table positioned in front of the player.

---

### Step 2: Create Cup Prefab

**2.1 Create Cup Object**
1. Right-click in Hierarchy → **3D Object → Cylinder**
2. Rename to **"Cup"**
3. Set Transform:
   - **Position:** (0, 1, 3) - temporary position for prefab setup
   - **Rotation:** (0, 0, 0)
   - **Scale:** (0.08, 0.1, 0.08) - ~3 inch diameter, 4 inch tall

**2.2 Add Bottom Trigger (Critical!)**
1. Right-click **Cup** in Hierarchy → **3D Object → Sphere**
2. Rename to **"BottomTrigger"**
3. Set Transform (relative to parent):
   - **Position:** (0, -0.8, 0) - at bottom of cup
   - **Rotation:** (0, 0, 0)
   - **Scale:** (0.6, 0.6, 0.6) - smaller than cup diameter
4. Remove **Mesh Renderer** (we only need collider)
   - Uncheck or delete Mesh Renderer component
5. **CRITICAL:** Sphere Collider → Check **"Is Trigger" ✅**

**2.3 Add Cup Script**
1. Select **Cup** (parent)
2. Add Component → **Cup**
3. Inspector:
   - **Ball Tag:** "Ball"
   - **Remove Delay:** 0.1
   - **Debug Mode:** ✅ (check during setup)

**2.4 Create Prefab**
1. In Project window → Navigate to **Prefabs** folder (create if needed)
2. Drag **Cup** from Hierarchy into Prefabs folder
3. Prefab created! Delete Cup from scene (GameManager will spawn them)

**Hierarchy Check (before making prefab):**
```
Cup (cylinder with solid collider for rim hits)
└── BottomTrigger (sphere trigger at bottom)
```

**How it works:** Ball can bounce off the cup rim (solid collider), but if it falls to the bottom and hits the trigger, it scores!

---

### Step 3: Create Ping Pong Ball Prefab

**3.1 Create Ball Object**
1. Right-click in Hierarchy → **3D Object → Sphere**
2. Rename to **"PingPongBall"**
3. Set Transform:
   - **Position:** (0, 1.2, 0) - temporary
   - **Rotation:** (0, 0, 0)
   - **Scale:** (0.04, 0.04, 0.04) - 40mm diameter (regulation ping pong ball)

**3.2 Add Ball Tag**
1. Select **PingPongBall**
2. Top of Inspector → **Tag → Add Tag**
3. Create new tag: **"Ball"**
4. Set PingPongBall → **Tag: Ball**

**3.3 Add Rigidbody**
1. Select **PingPongBall**
2. Add Component → **Rigidbody**
3. Configure:
   - **Mass:** 0.0027 (2.7 grams - real ping pong ball)
   - **Drag:** 0.2
   - **Angular Drag:** 0.1
   - **Use Gravity:** ✅
   - **Interpolate:** Interpolate
   - **Collision Detection:** Continuous

**3.4 Create Ball Physics Material (Low Friction, Medium Bounce)**
1. In Project window → Right-click → **Create → Physics Material**
2. Name it **"PingPongBallMaterial"**
3. Set values:
   - **Dynamic Friction:** 0.2
   - **Static Friction:** 0.2
   - **Bounciness:** 0.5 (moderate bounce)
   - **Friction Combine:** Minimum
   - **Bounce Combine:** Average
4. Drag material onto PingPongBall's **Sphere Collider → Material** slot

**3.5 Add XR Grab Interactable**
1. Select **PingPongBall**
2. Add Component → **XR Grab Interactable** (from XR Interaction Toolkit)
3. Configure:
   - **Movement Type:** Kinematic (or Instantaneous)
   - **Throw on Detach:** ✅ (critical for throwing)
   - **Throw Smoothing Duration:** 0.08
   - **Throw Velocity Scale:** 1.0 (adjust for throw strength)
   - **Attach Transform:** Leave default

**3.6 Add PingPongBall Script**
1. Select **PingPongBall**
2. Add Component → **PingPongBall**
3. Inspector:
   - **Mass:** 0.0027
   - **Drag:** 0.2
   - **Angular Drag:** 0.1
   - **Throw Velocity Threshold:** 0.5
   - **Debug Mode:** ✅ (check during setup)

**3.7 Create Prefab**
1. In Project window → Navigate to **Prefabs** folder
2. Drag **PingPongBall** from Hierarchy into Prefabs folder
3. Prefab created! Delete from scene (HandBallSpawner will spawn them)

---

### Step 4: Create Game Manager

**4.1 Create Manager GameObject**
1. Right-click in Hierarchy → **Create Empty**
2. Rename to **"BeerPongGameManager"**
3. Position: (0, 0, 0)

**4.2 Create Cup Spawn Parent (Organization)**
1. Right-click in Hierarchy → **Create Empty**
2. Rename to **"CupFormation"**
3. Position: (0, 0, 0)

**4.3 Add Scripts to Game Manager**
1. Select **BeerPongGameManager**
2. Add Component → **BeerPongGameManager**
3. Add Component → **Audio Source**

**4.4 Configure Game Manager Inspector**

**Game State:**
- **Total Cups:** 10
- **Cups Remaining:** 10
- **Game Active:** ✅

**Cup Spawning:**
- **Cup Prefab:** Drag **Cup** prefab from Prefabs folder
- **Cup Spawn Parent:** Drag **CupFormation** GameObject
- **Cup Formation Center:** (0, 0.85, 3.5) - on table, far end
- **Cup Spacing:** 0.18 (regulation ~7 inches)

**Ball Management:**
- **Ping Pong Ball Prefab:** Drag **PingPongBall** prefab from Prefabs folder
- **Ball Cleanup Delay:** 10 (balls despawn after 10 seconds)

**Audio:**
- **Audio Source:** Should auto-fill
- Leave sound clips empty for now (you can add later)

**Debug:**
- **Debug Mode:** ✅ (check during testing)

---

### Step 5: Create UI (World Space Canvas)

**5.1 Create Canvas**
1. Right-click in Hierarchy → **UI → Canvas**
2. Rename to **"BeerPongUI"**

**5.2 Configure Canvas for VR**
1. Select **BeerPongUI**
2. Canvas component:
   - **Render Mode:** World Space
   - **Event Camera:** Drag your **XR Camera** (usually "Main Camera")
3. Rect Transform:
   - **Position:** (0, 2, 3.5) - above table
   - **Rotation:** (0, 0, 0)
   - **Scale:** (0.01, 0.01, 0.01) - scale down for VR
   - **Width:** 400
   - **Height:** 200

**5.3 Create Game State Text**
1. Right-click **BeerPongUI** → **UI → Text - TextMeshPro**
2. Rename to **"GameStateText"**
3. Rect Transform:
   - **Pos X:** 0, **Pos Y:** 40
   - **Width:** 350, **Height:** 70
4. TextMeshProUGUI component:
   - **Text:** "" (empty)
   - **Font Size:** 52
   - **Alignment:** Center
   - **Color:** Yellow

**5.4 Create Reset Button**
1. Right-click **BeerPongUI** → **UI → Button - TextMeshPro**
2. Rename to **"ResetButton"**
3. Rect Transform:
   - **Pos X:** 0, **Pos Y:** -50
   - **Width:** 150, **Height:** 50
4. Button Text (child):
   - **Text:** "Reset Game"
   - **Font Size:** 24

**5.5 Assign UI References to Game Manager**
1. Select **BeerPongGameManager**
2. In Inspector:
   - **Game State Text:** Drag **GameStateText**
   - **Reset Button:** Drag **ResetButton**

**Note:** The BeerPongGameManager script automatically wires up the button click events in code (see `SetupEventListeners()` method). You just need to drag the references!

**Hierarchy Check:**
```
BeerPongUI (Canvas)
├── GameStateText
└── ResetButton
```

---

### Step 6: Setup XR Controllers for Ball Spawning

**6.1 Locate XR Controllers**
Your scene should have an XR Rig:
- **XR Origin** (or XR Rig)
  - Main Camera
  - **LeftHand Controller**
  - **RightHand Controller**

**6.2 Add HandBallSpawner to Left Hand**
1. Select **LeftHand Controller**
2. Add Component → **HandBallSpawner**
3. Configure Inspector:
   - **Game Manager:** Drag **BeerPongGameManager** (or leave empty to auto-find)
   - **Direct Interactor:** Leave empty (auto-finds from controller)
   - **Spawn Point:** Leave empty (uses controller position)
   - **Spawn On Press:** ✅
   - **Grip Threshold:** 0.5
   - **Debug Mode:** ✅ (check during setup)

**6.3 Add HandBallSpawner to Right Hand**
1. Select **RightHand Controller**
2. Add Component → **HandBallSpawner**
3. Configure Inspector:
   - **Game Manager:** Drag **BeerPongGameManager** (or leave empty to auto-find)
   - **Direct Interactor:** Leave empty (auto-finds)
   - **Spawn Point:** Leave empty
   - **Spawn On Press:** ✅
   - **Grip Threshold:** 0.5
   - **Debug Mode:** ✅

**Note:** The script automatically finds the grip/select action from your ActionBasedController component. Works with Quest 3, Quest 2, and all standard XR controllers - no manual input action assignment needed!

---

### Step 7: Verify XR Interaction Setup

**7.1 Check XR Direct Interactors**
1. Select **LeftHand Controller** and **RightHand Controller**
2. Both should have:
   - **XR Direct Interactor** (for grabbing nearby objects)
   - May also have **XR Ray Interactor** (for UI interaction)

**7.2 Verify Interaction Layers**
1. Select **PingPongBall** prefab
2. XR Grab Interactable → **Interaction Layer Mask:** Default
3. Select both XR Controllers
4. XR Direct Interactor → **Interaction Layer Mask:** Default

---

### Step 8: Final Scene Setup

**8.1 Position Player Start**
Ensure your XR Origin is positioned so player can reach table:
- **XR Origin Position:** (0, 0, 0) - player at origin
- **Table at:** (0, 0.75, 3) - 3 meters in front
- **Cups spawn at:** (0, 0.85, 3.5) - far end of table

**8.2 Add Ground Plane (Optional)**
1. Right-click → **3D Object → Plane**
2. Rename to **"Ground"**
3. Position: (0, 0, 0)
4. Scale: (2, 1, 3)

**8.3 Lighting (Optional)**
- Verify **Directional Light** exists and is positioned to illuminate table

---

## Complete Scene Hierarchy

After setup, your hierarchy should look like this:

```
Scene: BeerPongScene
│
├── XR Origin (or XR Rig)
│   ├── Main Camera
│   ├── LeftHand Controller (+ HandBallSpawner)
│   └── RightHand Controller (+ HandBallSpawner)
│
├── BeerPongGameManager (+ AudioSource)
│
├── CupFormation (empty parent for cups)
│
├── BeerPongTable (table surface)
│
├── BeerPongUI (World Space Canvas)
│   ├── GameStateText
│   └── ResetButton
│
├── Directional Light
└── Ground (optional)
```

---

## How It Works (Flow Diagram)

```
1. GAME START
   ↓
   BeerPongGameManager.Start()
   ↓
   Spawns 10 cups in triangle formation
   ↓

2. PLAYER PRESSES GRIP BUTTON
   ↓
   HandBallSpawner.MonitorGripButton() detects press
   ↓
   Checks if hand is already holding object
   ↓
   [If hand empty]
   ↓
   BeerPongGameManager.SpawnBall(handPosition)
   ↓
   HandBallSpawner.AutoGrabBall() - forces XR grab
   ↓
   Ball now in hand, ready to throw
   ↓

3. PLAYER RELEASES GRIP (THROWS BALL)
   ↓
   XRGrabInteractable.OnReleased()
   ↓
   PingPongBall.OnReleased() - ball marked as thrown
   ↓
   Ball flies with release velocity
   ↓
   Rigidbody physics take over
   ↓

4A. BALL HITS TABLE
   ↓
   PingPongBall.OnCollisionEnter(Table)
   ↓
   Fires OnTableImpact event
   ↓
   BeerPongGameManager.HandleBallTableImpact()
   ↓
   Plays table impact sound
   ↓

4B. BALL HITS CUP RIM
   ↓
   Cup.OnCollisionEnter(Ball)
   ↓
   Fires OnCupHit event
   ↓
   BeerPongGameManager.HandleCupHit()
   ↓
   Plays cup impact sound
   ↓
   Ball bounces off rim (or into another cup)
   ↓

5. BALL ENTERS CUP INTERIOR
   ↓
   CupInteriorTrigger.OnTriggerEnter(Ball)
   ↓
   Cup.HandleBallEntered()
   ↓
   Checks if ball was thrown (not placed)
   ↓
   Cup.ScoreCup() - marks cup as scored
   ↓
   Fires OnCupScored event
   ↓
   BeerPongGameManager.HandleCupScored()
   ↓
   Plays cup score sound
   ↓
   Cup destroys itself (Destroy after 0.1s delay)
   ↓
   cupsRemaining decremented
   ↓

6. CHECK WIN CONDITION
   ↓
   if (cupsRemaining <= 0)
   ↓
   BeerPongGameManager.HandleGameWon()
   ↓
   gameActive = false
   ↓
   Shows "YOU WIN!" message
   ↓
   Plays win sound
   ↓

7. PLAYER CLICKS RESET BUTTON
   ↓
   BeerPongGameManager.ResetGame()
   ↓
   Clears all cups and balls
   ↓
   Respawns 10 cups in triangle
   ↓
   Game ready to play again
```

---

## Inspector Configuration Quick Reference

### BeerPongTable
```
✅ Box Collider (NOT trigger)
✅ Physics Material: TableSurface
✅ Tag: "Table"

Transform:
├── Position: (0, 0.75, 3)
├── Rotation: (0, 0, 0)
└── Scale: (0.6, 0.02, 2.4)
```

### Cup Prefab
```
Cup (parent cylinder)
├── Capsule Collider (NOT trigger) - for rim collisions/audio
└── Cup.cs script
    ├── Ball Tag: "Ball"
    ├── Remove Delay: 0.1
    └── Debug Mode: ✅

BottomTrigger (child sphere)
├── Sphere Collider (IS TRIGGER ✅)
└── No mesh renderer
└── Position: (0, -0.8, 0) - at bottom of cup
```

### PingPongBall Prefab
```
✅ Sphere Collider (NOT trigger)
✅ Physics Material: PingPongBallMaterial
✅ Tag: "Ball"
✅ Rigidbody (mass 0.0027, continuous collision)
✅ XRGrabInteractable (throw on detach ✅)
✅ PingPongBall.cs script

[PingPongBall Component]
├── Mass: 0.0027
├── Drag: 0.2
├── Throw Velocity Threshold: 0.5
└── Debug Mode: ✅
```

### BeerPongGameManager
```
[Game State]
├── Total Cups: 10
└── Game Active: ✅

[Cup Spawning]
├── Cup Prefab: Cup (from Prefabs)
├── Cup Spawn Parent: CupFormation
├── Cup Formation Center: (0, 0.85, 3.5)
└── Cup Spacing: 0.18

[Ball Management]
├── Ping Pong Ball Prefab: PingPongBall (from Prefabs)
└── Ball Cleanup Delay: 10

[UI References]
├── Game State Text: GameStateText
└── Reset Button: ResetButton

[Audio]
└── Audio Source: (auto-fill)
```

### HandBallSpawner (both controllers)
```
[References]
├── Game Manager: BeerPongGameManager (or empty to auto-find)
├── Direct Interactor: (empty, auto-finds)
└── Spawn Point: (empty, uses controller position)

[Settings]
├── Spawn On Press: ✅
└── Grip Threshold: 0.5

[Auto-Detection]
└── Grip Action: Automatically found from ActionBasedController
```

---

## Testing Checklist

### ✅ Basic Functionality
- [ ] **Press Play** - 10 cups spawn in triangle formation
- [ ] **Press Grip** - Ball spawns in hand
- [ ] **Release Grip** - Ball flies in arc
- [ ] **Both hands work** - Can spawn balls with either hand
- [ ] **One ball per hand** - Can't spawn multiple balls in same hand
- [ ] **Ball bounces on table** - Physics feel realistic
- [ ] **Check Console** - Debug logs show ball spawns, throws, collisions

### ✅ Cup Scoring
- [ ] **Ball in cup** - Cup disappears when ball lands inside
- [ ] **Ball on rim** - Doesn't score if just touching rim (must enter interior)
- [ ] **Ball bounces** - Can bounce off rim into another cup
- [ ] **Only thrown balls score** - Placing ball in cup doesn't count
- [ ] **Cups remaining decreases** - Count goes from 10 → 9 → 8, etc.

### ✅ Audio (if sounds assigned)
- [ ] **Table impact sound** - Plays when ball hits table
- [ ] **Cup impact sound** - Plays when ball hits cup rim
- [ ] **Cup score sound** - Plays when ball lands in cup
- [ ] **Win sound** - Plays when all cups cleared

### ✅ UI & Game State
- [ ] **"YOU WIN!" message** - Appears when last cup cleared
- [ ] **Reset button works** - Respawns 10 cups, clears balls
- [ ] **Game stops after win** - Can't score after winning (until reset)

### ✅ VR Interaction
- [ ] **Grip button spawns ball** - Both hands
- [ ] **Ball feels lightweight** - Ping pong ball physics
- [ ] **Throwing feels natural** - Release velocity works
- [ ] **No stuck balls** - Balls release cleanly
- [ ] **UI buttons clickable** - Ray interactor can click reset

### ✅ Physics & Collisions
- [ ] **Ball doesn't fall through table** - Continuous collision working
- [ ] **Ball bounces realistically** - Not too bouncy, not dead
- [ ] **Cups don't tip over** - Stable on table
- [ ] **No double scoring** - Each cup scores only once

### ✅ Ball Cleanup
- [ ] **Old balls despawn** - After 10 seconds (or configured time)
- [ ] **No performance issues** - Game stays smooth

---

## Common Issues & Solutions

### Issue #1: "Grip button doesn't spawn ball"
**Problem:** Input Action not assigned or incorrect action path

**Solution:**
1. Select **LeftHand Controller** or **RightHand Controller**
2. HandBallSpawner → **Grip Action**
3. Click dropdown → Find **XRI [Left/Right]Hand Interaction/Grip** or **Select**
4. If not visible, check your project's Input Action Asset
5. Enable Debug Mode and check Console for errors

---

### Issue #2: "Ball spawns but doesn't auto-grab"
**Problem:** Direct Interactor not assigned or Interaction Manager missing

**Solution:**
1. Select XR controller
2. Verify **XR Direct Interactor** component exists
3. HandBallSpawner → **Direct Interactor** → Drag XR Direct Interactor
4. Check **XR Interaction Manager** exists in scene (usually on XR Origin)

---

### Issue #3: "Ball doesn't score when it lands in cup"
**Problem:** Bottom trigger not set up correctly

**Solution:**
1. Select **Cup** prefab
2. Verify **BottomTrigger** child exists at bottom of cup
3. BottomTrigger → Sphere Collider → **"Is Trigger" ✅ MUST BE CHECKED**
4. BottomTrigger → **Position:** (0, -0.8, 0) - should be at bottom, not floating
5. PingPongBall → **Tag: "Ball"** (exact spelling)
6. Cup script → **Ball Tag:** "Ball"

**Common mistakes:**
- Bottom trigger not enabled
- Trigger positioned too high (ball doesn't reach it)
- Ball tag is "ball" (lowercase) instead of "Ball"

---

### Issue #4: "Ball falls through table"
**Problem:** Collision detection mode or physics material issue

**Solution:**
1. Select **PingPongBall** prefab
2. Rigidbody → **Collision Detection: Continuous** (not Discrete)
3. Verify table has solid collider (NOT trigger)
4. Check ball isn't spawning inside table geometry

---

### Issue #5: "Can spawn multiple balls in one hand"
**Problem:** HandBallSpawner not checking if hand is holding object

**Solution:**
- This should not happen with provided scripts
- Check Console for debug messages
- Verify `IsHandHoldingObject()` is working
- Make sure only one HandBallSpawner per controller

---

### Issue #6: "Ball physics feel wrong (too heavy/floaty)"
**Problem:** Physics settings need tuning

**Solution:** Adjust in **PingPongBall** prefab:
1. Rigidbody → **Mass:** 0.0027 (real ping pong ball is very light!)
2. **Drag:** 0.2 (air resistance)
3. **Angular Drag:** 0.1
4. Physics Material → **Bounciness:** 0.4-0.6
5. XRGrabInteractable → **Throw Velocity Scale:** Adjust for desired throw strength

---

### Issue #7: "Ball bounces too much or not at all"
**Problem:** Physics material bounciness incorrect

**Solution:**
1. Find **PingPongBallMaterial** physics material
2. Adjust **Bounciness:** 0.4-0.6 (start at 0.5)
3. **Bounce Combine:** Average
4. Also check **TableSurface** material bounciness

---

### Issue #8: "Cups spawn in wrong position"
**Problem:** Cup Formation Center not set correctly

**Solution:**
1. Select **BeerPongGameManager**
2. **Cup Formation Center:** Adjust Y and Z values
   - Y should be table height + cup height/2 (~0.85)
   - Z should be far end of table (~3.5)
3. Enable Debug Mode to see gizmo preview in editor

---

### Issue #9: "Win message doesn't appear"
**Problem:** UI reference not assigned

**Solution:**
1. Select **BeerPongGameManager**
2. **Game State Text:** Drag **GameStateText** from UI Canvas
3. Verify TextMeshProUGUI component exists on GameStateText

---

### Issue #10: "Balls don't clean up / scene fills with balls"
**Problem:** Ball cleanup coroutine not running

**Solution:**
- Balls should auto-despawn after 10 seconds
- Adjust **Ball Cleanup Delay** in BeerPongGameManager
- Or use Reset button to manually clear all balls

---

## Tuning Guide (Make It Feel Good!)

### Ball Spawning Feel
Adjust in **HandBallSpawner:**
- **Grip Threshold:** 0.3-0.7 (lower = easier to spawn, higher = deliberate press)
- **Spawn On Press:** ✅ for instant spawn, ❌ for hold to spawn

### Throwing Feel
Adjust in **PingPongBall** prefab → **XR Grab Interactable:**
- **Throw Velocity Scale:** 0.8-1.5 (higher = farther throws)
- **Throw Smoothing Duration:** 0.05-0.15 (lower = more responsive)
- **Movement Type:** Try Kinematic vs Instantaneous

### Ball Physics
Adjust in **PingPongBall.cs** script:
- **Mass:** 0.002-0.004 (2-4 grams)
- **Drag:** 0.1-0.3 (air resistance)
- **Throw Velocity Threshold:** 0.3-1.0 (what counts as "thrown")

Adjust in **PingPongBallMaterial** physics material:
- **Bounciness:** 0.4-0.7 (ping pong balls are bouncy!)
- **Friction:** 0.1-0.3 (low friction)

### Table Physics
Adjust **TableSurface** physics material:
- **Bounciness:** 0.2-0.4 (some bounce on table)
- **Friction:** 0.3-0.5 (moderate slide)

### Cup Formation
Adjust in **BeerPongGameManager:**
- **Cup Spacing:** 0.15-0.20 (regulation is ~7 inches / 0.18m)
- **Cup Formation Center:** Move closer/farther for difficulty

---

## Script Reference

### BeerPongGameManager.cs
**Purpose:** Central game controller
**Location:** BeerPongGameManager GameObject
**Key Methods:**
- `SpawnCups()` - Creates 10-cup triangle formation
- `SpawnBall(position, rotation)` - PUBLIC - Called by HandBallSpawner
- `ResetGame()` - Clears cups/balls, respawns cups
- `HandleCupScored(Cup)` - Receives cup scoring events
**Key Events Subscribed:**
- Cup.OnCupScored
- Cup.OnCupHit
- PingPongBall.OnTableImpact

---

### PingPongBall.cs
**Purpose:** Grabbable ping pong ball with physics
**Location:** PingPongBall prefab
**Key Methods:**
- `ConfigurePhysics()` - Sets up realistic ping pong ball physics
**Events:**
- `OnTableImpact` - Fired when ball hits table (for audio)
**XR Events:**
- `OnGrabbed()` - Resets throw state
- `OnReleased()` - Ball can now score

---

### Cup.cs
**Purpose:** Cup collision and scoring detection
**Location:** Cup prefab
**Key Methods:**
- `OnTriggerEnter(other)` - Detects ball reaching bottom trigger
- `OnCollisionEnter(collision)` - Detects ball hitting cup rim (for audio)
- `ScoreCup()` - Marks cup as scored, fires event, destroys self
**Events:**
- `OnCupScored` - Fired when ball reaches bottom (scores)
- `OnCupHit` - Fired when ball hits cup rim (audio feedback)
**Detection:**
- Simple single-collider system: Bottom trigger detects scoring

---

### HandBallSpawner.cs
**Purpose:** Spawns balls on grip button press
**Location:** XR Controller GameObjects (LeftHand, RightHand)
**Key Methods:**
- `MonitorGripButton()` - Checks grip input each frame
- `TrySpawnBall()` - Spawns ball if hand is empty
- `AutoGrabBall(ball)` - Forces XR Direct Interactor to grab spawned ball
**Input:**
- Uses Unity Input System (InputActionProperty)
- Monitors grip/select action

---

## Next Steps (Phase 2+)

See **PotentialFeatures.md** for full roadmap. Highlights:

### High Priority (Phase 2)
- 🎨 **Visual Effects** - Particle effects when ball scores, cup disappear animation
- 🔊 **Enhanced Audio** - Celebration sounds, crowd reactions, ambience
- 📊 **Accuracy Tracking** - UI showing balls thrown vs cups made, accuracy percentage
- 🔄 **Re-Rack System** - Rearrange cups at 6, 4, 3 remaining (traditional beer pong)

### Medium Priority
- 🎮 **Game Modes** - Time trial, limited balls challenge, trick shot mode
- 💡 **Quality of Life** - Ball retrieval zone, haptic feedback, tutorial
- 🎨 **Customization** - Cup colors, ball skins, table themes

### Future Considerations
- 👥 **Multiplayer** - Two-sided table, turn-based gameplay
- 🏆 **Tournament Mode** - Brackets, leaderboards, official rules
- 🤖 **AI Opponent** - Single-player vs computer

---

## Files Created

### Scripts (4 files)
✅ `BeerPongGameManager.cs` - 400+ lines
✅ `PingPongBall.cs` - 180 lines
✅ `Cup.cs` - 200 lines
✅ `HandBallSpawner.cs` - 190 lines

### Documentation (2 files)
✅ `Phase1Setup.md` - This file!
✅ `PotentialFeatures.md` - Feature roadmap

### Prefabs (To be created by you)
- [ ] PingPongBall.prefab
- [ ] Cup.prefab

---

## Technical Notes

### Unity Physics Settings
- **Ping pong balls use very low mass** (0.0027kg) for realistic lightweight feel
- **Continuous collision detection** prevents balls passing through cups/table
- **Low friction, medium bounce** for authentic ping pong ball behavior
- **Physics materials** tune bounce and slide characteristics

### VR Input System
- **Action-Based Controllers** required for Input System integration
- **Grip Action** monitored continuously via InputActionProperty
- **Force Select** used to auto-grab spawned balls

### Event-Based Architecture
- **Cup** fires OnCupScored and OnCupHit events
- **PingPongBall** fires OnTableImpact event
- **BeerPongGameManager** subscribes to all events
- Loose coupling allows easy expansion (multiplayer, etc.)

### Performance Optimization
- **Ball cleanup** prevents scene from filling with hundreds of balls
- **Object pooling** (Phase 2) can improve performance further
- **Simple colliders** (spheres, cylinders) keep physics fast

---

## Support & Troubleshooting

**Enable Debug Logs:**
1. Check **Debug Mode** in all scripts
2. Watch Console during gameplay
3. Logs show: ball spawns, throws, cup scores, collisions

**Use Gizmos:**
- BeerPongGameManager draws cup formation preview in editor
- HandBallSpawner draws spawn point gizmos
- PingPongBall draws velocity vectors

**Verify Setup:**
- Use "Testing Checklist" above
- Check "Common Issues" section
- Review Inspector configurations

**Still stuck?**
- Re-read relevant setup step
- Verify all tags match exactly (case-sensitive)
- Check all component references are assigned
- Enable debug modes and read console logs

---

## Summary

You now have a fully functional Beer Pong VR mini-game! 🎉

**What works:**
✅ Spawn ping pong balls on grip press (both hands)
✅ Throw balls with realistic physics
✅ 10-cup triangle formation
✅ Accurate ball-in-cup detection
✅ Cups disappear when scored
✅ Win condition when all cups cleared
✅ Reset system to play again
✅ Audio event system (ready for sounds)

**What's next:**
🎨 Add visual effects and polish
🔊 Add audio clips for immersion
📊 Add accuracy tracking UI
🔄 Add re-rack system for authentic beer pong

**Enjoy your VR Beer Pong game! Time to sink some cups! 🏓🥤**
