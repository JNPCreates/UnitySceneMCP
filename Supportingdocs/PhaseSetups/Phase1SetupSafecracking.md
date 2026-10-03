# Safe Cracking VR - Phase 1 Setup Guide

## Overview

This guide will walk you through setting up a fully functional Safe Cracking mini-game in VR. You'll create:

- ✅ Wall-mounted combination dial safe at chest height
- ✅ Grabbable dial with realistic 40-number combination lock mechanics
- ✅ Full 3-step combo sequence (Right 2+ turns → Left past first → Right to third)
- ✅ Haptic feedback: light clicks per number, strong pulse on correct number lock
- ✅ Door with grabbable handle that swings open on correct combo
- ✅ Stats display: crack timer, total safes cracked, current streak
- ✅ Debug mode to show/hide current combination for testing

**Philosophy:** Pure feel-based safe cracking. No visual hints — the player learns the combo through haptic feedback and audio clicks, just like a real combination lock.

**Estimated Setup Time:** 1.5–2 hours

---

## What You Need

### External Dependencies
- ✅ **Unity 6** (or Unity 2021.3+)
- ✅ **XR Interaction Toolkit** (already installed in your project)
- ✅ **TextMeshPro** (built into Unity)

### Scripts Already Created
- ✅ `SafeCrackingGameManager.cs` - Game state, scoring, audio, UI
- ✅ `SafeDial.cs` - Dial rotation, combo state machine, haptic ticks
- ✅ `SafeDoor.cs` - Handle interaction, door swing animation, lock/unlock

All located in: `F:\Unity\VR Games\MiniArena\Assets\_MiniGames\SafeCracking\Scripts\`

---

## Quick Setup (6 Steps)

### Step 1: Create the Safe Body

**1.1 Create Safe Object**
1. Right-click in Hierarchy → **3D Object → Cube**
2. Rename to **"Safe"**
3. Set Transform:
   - **Position:** (0, 1.2, 1.8) - chest height, 1.8m in front of player
   - **Rotation:** (0, 0, 0)
   - **Scale:** (0.5, 0.5, 0.4) - roughly 50cm wide x 50cm tall x 40cm deep

**1.2 Create the Safe's Front Face Reference**
1. Right-click **Safe** in Hierarchy → **Create Empty**
2. Rename to **"FrontFace"**
3. Set local position: (0, 0, 0.5) - sits at the front surface of the safe
4. This is where we'll anchor the dial and door

**Visual Check:** You should have a grey box at a comfortable arm-reach height.

---

### Step 2: Create the Safe Door

**2.1 Create Door Object**
1. Right-click **Safe** in Hierarchy → **3D Object → Cube**
2. Rename to **"SafeDoor"**
3. Set Transform (local to Safe parent):
   - **Position:** (0, 0, 0.21) - sits flush with front face
   - **Rotation:** (0, 0, 0)
   - **Scale:** (0.48, 0.48, 0.02) - slightly smaller than safe face, thin panel

**2.2 Set Up the Door Pivot**
The door must rotate around its LEFT edge (like a real safe door hinging open to the left).
1. Right-click **Safe** in Hierarchy → **Create Empty**
2. Rename to **"DoorPivot"**
3. Set local position: **(-0.24, 0, 0.21)** - left edge of door, same Z as door face
4. Drag **SafeDoor** to be a **child of DoorPivot**
5. Now reset SafeDoor's local position so it's offset right from the pivot:
   - **SafeDoor local Position:** (0.24, 0, 0) - right side offset from pivot

**2.3 Create the Door Handle**
1. Right-click **SafeDoor** in Hierarchy → **3D Object → Cylinder**
2. Rename to **"DoorHandle"**
3. Set Transform (local to SafeDoor):
   - **Position:** (0.1, 0, 0.6) - right-center of door, sticking out
   - **Rotation:** (90, 0, 0) - cylinder pointing outward
   - **Scale:** (0.05, 0.06, 0.05) - small cylindrical handle

**2.4 Add XR Grab Interactable to Handle**
1. Select **DoorHandle**
2. Add Component → **XR Grab Interactable**
3. Configure:
   - **Movement Type:** Instantaneous
   - **Track Position:** ❌ (unchecked — handle stays on door)
   - **Track Rotation:** ❌ (unchecked)
   - **Throw on Detach:** ❌ (unchecked)
4. Add a **Capsule Collider** (for grab detection)
   - **Direction:** Y-Axis
   - **Radius:** 0.04, **Height:** 0.12

**2.5 Add SafeDoor Script**
1. Select **SafeDoor**
2. Add Component → **SafeDoor**
3. Inspector:
   - **Door Pivot:** Drag **DoorPivot** GameObject
   - **Handle Grab Interactable:** Drag the **XR Grab Interactable** from DoorHandle
   - **Open Angle:** 110
   - **Open Duration:** 0.8
   - **Enable Haptics:** ✅

**Hierarchy Check:**
```
Safe
├── FrontFace (empty, reference point)
├── DoorPivot (empty, at left edge of door)
│   └── SafeDoor (cube, thin panel)
│       └── DoorHandle (cylinder + XRGrabInteractable)
```

---

### Step 3: Create the Safe Dial

**3.1 Create Dial Object**
1. Right-click **Safe** in Hierarchy → **3D Object → Cylinder**
2. Rename to **"SafeDial"**
3. Set Transform (local to Safe parent):
   - **Position:** (-0.08, 0, 0.22) - center-left on door face, proud of surface
   - **Rotation:** (90, 0, 0) - flat face toward player
   - **Scale:** (0.09, 0.015, 0.09) - flat disc, ~9cm diameter

**3.2 Add a Dial Indicator (Optional but Helpful)**
1. Right-click **SafeDial** → **3D Object → Cube**
2. Rename to **"DialMarker"**
3. Set Transform (local to dial):
   - **Position:** (0, 0.016, -0.07) - small tick at the top of the dial
   - **Scale:** (0.01, 0.003, 0.015)
4. This rotates with the dial so the player can see it turning

**3.3 Add a Fixed Indicator Above the Dial**
1. Right-click **Safe** → **3D Object → Cube**
2. Rename to **"DialIndicator"**
3. Set Transform (local to Safe):
   - **Position:** (-0.08, 0.1, 0.22) - directly above dial, same Z
   - **Scale:** (0.005, 0.02, 0.005)
4. This stays fixed — the dial's number aligns to this marker

**3.4 Add Rigidbody to Dial**
1. Select **SafeDial**
2. Add Component → **Rigidbody**
3. Configure:
   - **Is Kinematic:** ✅ (checked — we control rotation manually)
   - **Use Gravity:** ❌ (unchecked)

**3.5 Add XR Grab Interactable to Dial**
1. Select **SafeDial**
2. Add Component → **XR Grab Interactable**
3. Configure:
   - **Movement Type:** Instantaneous
   - **Track Position:** ❌ (unchecked — we handle rotation in script)
   - **Track Rotation:** ❌ (unchecked)
   - **Throw on Detach:** ❌ (unchecked)
4. Add a **Sphere Collider** for comfortable grab zone:
   - **Radius:** 0.07
   - **Center:** (0, 0, 0)

**3.6 Add SafeDial Script**
1. Select **SafeDial**
2. Add Component → **SafeDial**
3. Inspector:
   - **Total Numbers:** 40
   - **Number Tolerance:** 1
   - **Required Full Turns For First:** 2
   - **Enable Haptics:** ✅
   - **Tick Haptic Intensity:** 0.1
   - **Lock Haptic Intensity:** 0.6
   - **Enable Thumbstick Input:** ✅ (fallback if rotation tracking is tricky)
   - **Debug Mode:** ✅ (check during setup, shows current number in console)

**Hierarchy Check:**
```
Safe
├── FrontFace
├── DoorPivot
│   └── SafeDoor
│       └── DoorHandle
├── SafeDial (cylinder + XRGrabInteractable)
│   └── DialMarker (small tick, rotates with dial)
└── DialIndicator (fixed reference mark above dial)
```

---

### Step 4: Create the Game Manager

**4.1 Create Manager GameObject**
1. Right-click in Hierarchy → **Create Empty**
2. Rename to **"SafeCrackingGameManager"**
3. Position: (0, 0, 0)

**4.2 Add Scripts to Manager**
1. Select **SafeCrackingGameManager**
2. Add Component → **SafeCrackingGameManager**
3. Add Component → **Audio Source**
   - **Play On Awake:** ❌

**4.3 Configure Manager Inspector**

**Object References:**
- **Safe Dial:** Drag **SafeDial** GameObject
- **Safe Door:** Drag **SafeDoor** GameObject

**Audio:**
- **Audio Source:** Should auto-fill (or drag from this same GameObject)
- Leave all AudioClip slots empty for now (Phase 2 will add sounds)

**Debug:**
- **Debug Mode:** ✅ (shows combo on UI during testing)

---

### Step 5: Create UI (World Space Canvas)

**5.1 Create Canvas**
1. Right-click in Hierarchy → **UI → Canvas**
2. Rename to **"SafeCrackingUI"**

**5.2 Configure Canvas for VR**
1. Select **SafeCrackingUI**
2. Canvas component:
   - **Render Mode:** World Space
   - **Event Camera:** Drag your **XR Camera** (usually "Main Camera")
3. Rect Transform:
   - **Position:** (0.6, 1.3, 1.8) - beside the safe at eye level
   - **Rotation:** (0, -15, 0) - angled slightly toward player
   - **Scale:** (0.002, 0.002, 0.002) - scale down for VR
   - **Width:** 400, **Height:** 300

**5.3 Create Timer Text**
1. Right-click **SafeCrackingUI** → **UI → Text - TextMeshPro**
2. Rename to **"TimerText"**
3. Rect Transform:
   - **Pos X:** 0, **Pos Y:** 100
   - **Width:** 300, **Height:** 70
4. TextMeshProUGUI component:
   - **Text:** "00:00"
   - **Font Size:** 52
   - **Alignment:** Center
   - **Color:** White

**5.4 Create Safes Cracked Text**
1. Right-click **SafeCrackingUI** → **UI → Text - TextMeshPro**
2. Rename to **"SafesCrackedText"**
3. Rect Transform:
   - **Pos X:** -80, **Pos Y:** 20
   - **Width:** 150, **Height:** 60
4. TextMeshProUGUI component:
   - **Text:** "0"
   - **Font Size:** 44
   - **Alignment:** Center
   - **Color:** White

**5.5 Create Streak Text**
1. Right-click **SafeCrackingUI** → **UI → Text - TextMeshPro**
2. Rename to **"StreakText"**
3. Rect Transform:
   - **Pos X:** 80, **Pos Y:** 20
   - **Width:** 150, **Height:** 60
4. TextMeshProUGUI component:
   - **Text:** "0"
   - **Font Size:** 44
   - **Alignment:** Center
   - **Color:** Yellow

**5.6 Create Combo Debug Text**
1. Right-click **SafeCrackingUI** → **UI → Text - TextMeshPro**
2. Rename to **"ComboDebugText"**
3. Rect Transform:
   - **Pos X:** 0, **Pos Y:** -50
   - **Width:** 300, **Height:** 60
4. TextMeshProUGUI component:
   - **Text:** "??-??-??"
   - **Font Size:** 48
   - **Alignment:** Center
   - **Color:** Cyan

**5.7 Create Reset Button**
1. Right-click **SafeCrackingUI** → **UI → Button - TextMeshPro**
2. Rename to **"ResetButton"**
3. Rect Transform:
   - **Pos X:** -80, **Pos Y:** -120
   - **Width:** 140, **Height:** 50
4. Button Text (child):
   - **Text:** "Reset"
   - **Font Size:** 24

**5.8 Create Start Timer Button**
1. Right-click **SafeCrackingUI** → **UI → Button - TextMeshPro**
2. Rename to **"StartTimerButton"**
3. Rect Transform:
   - **Pos X:** 80, **Pos Y:** -120
   - **Width:** 140, **Height:** 50
4. Button Text (child):
   - **Text:** "Start Timer"
   - **Font Size:** 22

**5.9 Assign UI References to Game Manager**
1. Select **SafeCrackingGameManager**
2. In Inspector:
   - **Timer Text:** Drag **TimerText**
   - **Safes Cracked Text:** Drag **SafesCrackedText**
   - **Streak Text:** Drag **StreakText**
   - **Combo Debug Text:** Drag **ComboDebugText**
   - **Reset Button:** Drag **ResetButton**
   - **Start Timer Button:** Drag **StartTimerButton**

**Note:** Button click events are wired up in code automatically in `SetupEventListeners()`. You only need to drag the references!

**Hierarchy Check:**
```
SafeCrackingUI (Canvas)
├── TimerText
├── SafesCrackedText
├── StreakText
├── ComboDebugText
├── ResetButton
└── StartTimerButton
```

---

### Step 6: Final Setup and Verify XR

**6.1 Ensure XR Rig Is Present**
Your scene should already have an XR Rig from other mini-games:
- **XR Origin** (or XR Rig)
  - Main Camera
  - LeftHand Controller (with XR Direct Interactor)
  - RightHand Controller (with XR Direct Interactor)

**6.2 Verify Interaction Layers**
1. Select **SafeDial** → XR Grab Interactable → **Interaction Layer Mask: Default**
2. Select **DoorHandle** → XR Grab Interactable → **Interaction Layer Mask: Default**
3. Both XR Controllers → XR Direct Interactor → **Interaction Layer Mask: Default**

**6.3 Position Player Start**
- **XR Origin:** (0, 0, 0) - player at origin, facing +Z
- **Safe:** (0, 1.2, 1.8) - 1.8m in front, chest height
- **UI Panel:** (0.6, 1.3, 1.8) - beside safe on player's right

---

## Complete Scene Hierarchy

After setup, your hierarchy should look like this:

```
Scene: SafeCrackingScene
│
├── XR Origin (or XR Rig)
│   ├── Main Camera
│   ├── LeftHand Controller
│   └── RightHand Controller
│
├── SafeCrackingGameManager (empty + scripts + AudioSource)
│
├── Safe (cube, safe body)
│   ├── FrontFace (empty, reference)
│   ├── DoorPivot (empty, hinge at left edge)
│   │   └── SafeDoor (cube, door panel)
│   │       └── DoorHandle (cylinder + XRGrabInteractable)
│   ├── SafeDial (cylinder + XRGrabInteractable + SafeDial.cs)
│   │   └── DialMarker (small tick mark)
│   └── DialIndicator (fixed reference mark)
│
├── SafeCrackingUI (World Space Canvas)
│   ├── TimerText
│   ├── SafesCrackedText
│   ├── StreakText
│   ├── ComboDebugText
│   ├── ResetButton
│   └── StartTimerButton
│
├── Directional Light
└── Ground Plane (optional)
```

---

## How It Works (Flow Diagram)

```
1. GAME START
   ↓
   SafeCrackingGameManager.Start()
   ↓
   GenerateNewCombination() → random 3 numbers (0-39)
   ↓
   SafeDial.SetCombination(combo)
   ↓
   ComboDebugText shows combo (if debugMode ON)
   ↓

2. PLAYER GRABS DIAL
   ↓
   XRGrabInteractable.selectEntered
   ↓
   SafeDial.OnGrabbed() → state: IDLE → SEEKING_FIRST
   ↓
   Timer starts on first number tick
   ↓

3. PLAYER ROTATES DIAL (RIGHT for first number)
   ↓
   SafeDial.Update() tracks interactor angle
   ↓
   Angle delta → cumulative rotation → CW revolution count
   ↓
   Every number change → OnDialTick fires
   ↓
   Manager plays tick sound + light haptic pulse
   ↓
   After 2+ full CW turns, dial lands on first number:
   ↓
   SafeDial → state: SEEKING_FIRST → SEEKING_SECOND
   ↓
   OnNumberLocked(0, number) → strong haptic + lock sound
   ↓

4. PLAYER ROTATES DIAL (LEFT for second number)
   ↓
   CCW rotation tracked
   ↓
   [WRONG DIRECTION] → OnCombinationFailed → reset to IDLE
   ↓
   [CORRECT] After passing first number going left, land on second:
   ↓
   SafeDial → state: SEEKING_SECOND → SEEKING_THIRD
   ↓
   OnNumberLocked(1, number) → strong haptic + lock sound
   ↓

5. PLAYER ROTATES DIAL (RIGHT for third number)
   ↓
   CW rotation, no full turns required
   ↓
   [WRONG DIRECTION] → OnCombinationFailed → reset to IDLE
   ↓
   [CORRECT] Land on third number:
   ↓
   SafeDial → state: SEEKING_THIRD → COMPLETE
   ↓
   OnCombinationComplete → SafeDoor.Unlock()
   ↓

6. PLAYER GRABS DOOR HANDLE
   ↓
   XRGrabInteractable.selectEntered
   ↓
   SafeDoor.TryOpen()
   ↓
   [LOCKED] OnHandlePulled event → locked sound + haptic thud
   ↓
   [UNLOCKED] AnimateDoor() coroutine → door swings open 110°
   ↓
   OnSafeOpened event fires
   ↓

7. SAFE OPENED
   ↓
   Manager: safesCracked++, currentStreak++
   ↓
   isTiming = false
   ↓
   Plays doorOpenSound + safeOpenSuccessSound
   ↓
   UpdateAllUI()
   ↓

8. PLAYER CLICKS RESET
   ↓
   SafeDoor.ResetDoor() → door closes, isUnlocked = false
   ↓
   SafeDial.ResetDial() → back to IDLE, angle reset to 0
   ↓
   GenerateNewCombination() → new random combo
   ↓
   Ready to crack again
```

---

## Inspector Configuration Quick Reference

### Safe (body)
```
✅ Box Collider (solid, for reference/collision)

Transform:
├── Position: (0, 1.2, 1.8)
├── Rotation: (0, 0, 0)
└── Scale: (0.5, 0.5, 0.4)
```

### SafeDoor
```
✅ Box Collider (solid, on door panel)

[SafeDoor Component]
├── Door Pivot: DoorPivot (empty GO at left hinge edge)
├── Handle Grab Interactable: DoorHandle XRGrabInteractable
├── Open Angle: 110
├── Open Duration: 0.8
├── Enable Haptics: ✅
├── Locked Haptic Intensity: 0.4
└── Locked Haptic Duration: 0.2
```

### DoorHandle
```
✅ Capsule Collider (NOT trigger, for grab detection)
✅ XR Grab Interactable
   ├── Movement Type: Instantaneous
   ├── Track Position: ❌
   ├── Track Rotation: ❌
   └── Throw on Detach: ❌
```

### SafeDial
```
✅ Sphere Collider (NOT trigger, grab zone, Radius 0.07)
✅ Rigidbody (Is Kinematic: ✅, Use Gravity: ❌)
✅ XR Grab Interactable
   ├── Movement Type: Instantaneous
   ├── Track Position: ❌
   ├── Track Rotation: ❌
   └── Throw on Detach: ❌

[SafeDial Component]
├── Total Numbers: 40
├── Number Tolerance: 1
├── Required Full Turns For First: 2
├── Enable Haptics: ✅
├── Tick Haptic Intensity: 0.1
├── Tick Haptic Duration: 0.02
├── Lock Haptic Intensity: 0.6
├── Lock Haptic Duration: 0.15
└── Enable Thumbstick Input: ✅
```

### SafeCrackingGameManager
```
[Object References]
├── Safe Dial: SafeDial GameObject
└── Safe Door: SafeDoor GameObject

[UI Display]
├── Timer Text: TimerText
├── Safes Cracked Text: SafesCrackedText
├── Streak Text: StreakText
└── Combo Debug Text: ComboDebugText

[UI Controls]
├── Reset Button: ResetButton
└── Start Timer Button: StartTimerButton

[Audio]
└── Audio Source: (component on this same GameObject)

[Debug]
└── Debug Mode: ✅ (during testing)
```

---

## Testing Checklist

### ✅ Basic Dial Rotation
- [ ] **Press Play** - combo appears in ComboDebugText (debug mode on)
- [ ] **Grab dial** - hand attaches to dial, state changes to SeekingFirst in console
- [ ] **Rotate hand CW** - numbers tick up, console shows current number
- [ ] **Light haptic** - controller pulses slightly on each number tick
- [ ] **Release dial** - hand detaches, dial stays in position

### ✅ Combo Sequence
- [ ] **Right 2+ full turns** - revolution count reaches 2 in console
- [ ] **Land on first number** - strong haptic, "Locked digit 1" in console
- [ ] **Left past first** - state changes to SeekingSecond in console
- [ ] **Land on second number** - strong haptic, "Locked digit 2"
- [ ] **Right to third number** - strong haptic, "Locked digit 3", combo complete
- [ ] **Door handle is now active** - TryOpen succeeds

### ✅ Failure Detection
- [ ] **Wrong direction mid-sequence** - "Combination failed" in console, state → IDLE
- [ ] **Combo resets** - must start over after failure
- [ ] **Streak resets to 0** on failure

### ✅ Door Interaction
- [ ] **Grab handle before combo** - locked sound plays, haptic thud, door stays closed
- [ ] **Grab handle after combo** - door swings open smoothly over ~0.8 seconds
- [ ] **Door stays open** - does not spring back

### ✅ Stats & UI
- [ ] **Timer counts up** from first dial interaction
- [ ] **Timer stops** when safe is opened
- [ ] **Safes Cracked** increments after door opens
- [ ] **Streak** increments after door opens, resets on combo failure
- [ ] **Reset button** closes door, generates new combo, resets timer

### ✅ Debug Mode
- [ ] **debugMode ON** - combo shows as "XX-XX-XX" in ComboDebugText
- [ ] **debugMode OFF** - ComboDebugText shows "??-??-??"
- [ ] **Console logs** show state transitions and locked digits

---

## Common Issues & Solutions

### Issue #1: "Dial doesn't detect rotation / numbers don't change"
**Problem:** The `GetInteractorAngle()` method can't find the interactor transform, or the grab interactable isn't configured to ignore tracking.

**Solution:**
1. Select **SafeDial**
2. XR Grab Interactable → **Track Position: ❌** and **Track Rotation: ❌**
3. Make sure the controller has an **XR Direct Interactor** component
4. Enable Debug Mode on the dial — check Console for angle delta output

---

### Issue #2: "Dial grabs but door grabs too, objects conflict"
**Problem:** Both grab interactables activate simultaneously.

**Solution:**
1. Verify DoorHandle and SafeDial are on **separate GameObjects**
2. Confirm **Interaction Layer Masks** match on both interactables and both controllers
3. If using Ray Interactor, ensure the dial is within direct interactor range (not ray)

---

### Issue #3: "Revolution count never reaches 2"
**Problem:** `cumulativeRotation` resets when player releases and re-grabs.

**This is correct behavior!** Releasing the dial resets the grab. The player must maintain their grip through 2+ full rotations without releasing.

If this feels too hard, reduce **Required Full Turns For First** to **1** in SafeDial inspector.

---

### Issue #4: "Door doesn't open after correct combo"
**Problem:** SafeDoor is not receiving the Unlock signal, or the door pivot is set incorrectly.

**Solution:**
1. Select **SafeCrackingGameManager** — verify **Safe Door** reference is assigned
2. Select **SafeDoor** — verify **Door Pivot** is assigned (the DoorPivot empty GameObject)
3. Enable Debug Mode and check Console: "Combination complete! Door unlocked." should appear
4. Try grabbing the handle — Console should log the TryOpen call

---

### Issue #5: "Door swings in wrong direction or through the safe"
**Problem:** DoorPivot is positioned incorrectly, or the openAngle is the wrong sign.

**Solution:**
1. Verify **DoorPivot** is at the **left edge** of the door, not the center
2. In SafeDoor inspector: try setting **Open Angle** to **-110** (negative) to reverse swing direction
3. Make sure SafeDoor's local position inside DoorPivot is offset to the right: (0.24, 0, 0)

---

### Issue #6: "Handle grabs and player can pull door with no combo"
**Problem:** isUnlocked defaults to true, or SafeDoor isn't subscribed to the dial events.

**Solution:**
1. SafeDoor starts locked (isUnlocked = false) — verify no code is calling Unlock() prematurely
2. Check SafeCrackingGameManager's OnEnable — confirm event wiring
3. Enable Debug Mode — after grabbing handle while locked, console should say "Handle pulled - locked"

---

### Issue #7: "Haptics not firing"
**Problem:** Interactor is not an XRBaseInputInteractor, or haptics are disabled.

**Solution:**
1. SafeDial inspector → **Enable Haptics: ✅**
2. Verify your controllers use **XR Controller (Action-Based)** — required for SendHapticImpulse
3. Test on device (haptics don't work in editor Play mode on most setups)
4. Check Console — if controller type cast fails, you'll see no errors but also no haptics

---

### Issue #8: "UI buttons don't respond in VR"
**Problem:** Canvas not configured for XR UI interaction.

**Solution:**
1. Select **SafeCrackingUI** canvas
2. Canvas → **Event Camera:** Drag **Main Camera**
3. Verify XR controllers have **XR Ray Interactor** for UI clicks
4. Add **TrackedDeviceGraphicRaycaster** component to the canvas (required for XR UI)

---

## Tuning Guide (Make It Feel Good!)

### Dial Sensitivity
Adjust in **SafeDial** on the SafeDial GameObject:
- **Number Tolerance:** 1–2 (higher = easier, 2 means ±2 numbers counts as correct)
- **Required Full Turns For First:** 1–3 (lower = easier to start)
- **Total Numbers:** 40 for classic Master Lock feel, 20 for easier practice

### Haptic Feedback
Adjust in **SafeDial**:
- **Tick Haptic Intensity:** 0.05–0.2 (lower = subtle, higher = noticeable)
- **Lock Haptic Intensity:** 0.4–1.0 (higher = more satisfying "clunk")
- **Lock Haptic Duration:** 0.1–0.25 (longer = heavier feel)

### Door Animation
Adjust in **SafeDoor**:
- **Open Angle:** 90–120 (110 gives a wide, satisfying swing)
- **Open Duration:** 0.5–1.2 (slower = heavier vault feel, faster = snappy)

### Safe Position
Adjust the **Safe** GameObject transform:
- Move closer (Z: 1.2) for easier reach
- Move farther (Z: 2.5) for a challenge
- Raise/lower Y (1.0–1.4) to match different player heights or sitting vs. standing

---

## Script Reference

### SafeCrackingGameManager.cs
**Purpose:** Central hub — coordinates dial, door, UI, and audio
**Location:** SafeCrackingGameManager GameObject
**Key Methods:**
- `GenerateNewCombination()` - Creates random 3-number combo (0-39), passes to SafeDial
- `ResetSafe()` - Closes door, resets dial, generates new combo
- `HandleCombinationComplete()` - Calls SafeDoor.Unlock()
- `HandleSafeOpened()` - Increments safesCracked, streak, stops timer
**Events Subscribed:**
- `SafeDial.OnDialTick` - tick sound + start timing
- `SafeDial.OnNumberLocked` - lock sound + debug log
- `SafeDial.OnCombinationComplete` - unlock door
- `SafeDial.OnCombinationFailed` - reset streak, buzzer sound
- `SafeDoor.OnSafeOpened` - score + reward sounds
- `SafeDoor.OnHandlePulled` - locked door sound

---

### SafeDial.cs
**Purpose:** Dial rotation tracking and combo state machine
**Location:** SafeDial GameObject
**Key Methods:**
- `SetCombination(int[] combo)` - Called by GameManager, stores the 3-number target
- `ResetDial()` - Returns to Idle state, resets angle and revolution tracking
- `ProcessStateMachine(float angleDelta)` - Runs each number tick, checks combo progress
**Events Fired:**
- `OnDialTick(int number)` - Every time dial moves to a new number
- `OnNumberLocked(int digitIndex, int value)` - When a combo digit is successfully set
- `OnCombinationComplete()` - All 3 digits locked correctly
- `OnCombinationFailed()` - Wrong direction or overshot

**Combo State Machine:**
```
Idle → (grab) → SeekingFirst → (2+ CW turns, first number) → SeekingSecond
     → (CCW past first, second number) → SeekingThird
     → (CW to third number) → Complete
     Any wrong direction → Failed → Idle
```

---

### SafeDoor.cs
**Purpose:** Handle interaction and door swing animation
**Location:** SafeDoor GameObject
**Key Methods:**
- `Unlock()` - Called by GameManager, allows door to open
- `Lock()` - Closes door and re-locks
- `ResetDoor()` - Instant close, sets locked, stops animation
- `TryOpen(interactor)` - Checks isUnlocked; opens or fires locked feedback
**Events Fired:**
- `OnSafeOpened()` - Door successfully opened
- `OnHandlePulled()` - Handle grabbed while door is locked

---

## Next Steps (Phase 2+)

See **PotentialFeatures.md** for full roadmap. Highlights:

### High Priority (Phase 2)
- 🔊 **Sound Design** - Source authentic dial click SFX, vault door creak, reward chime
- ⏱️ **Timed Challenge Mode** - Countdown from 60s, beat your best crack time
- 🏆 **High Score Persistence** - PlayerPrefs to save best crack time per combo length
- 🎯 **Difficulty Settings** - Tighter tolerance, more numbers, longer combos

### Medium Priority
- 💡 **Visual Hints System** - Optional glow on dial when in correct zone (easy mode)
- 🔊 **Stethoscope Mechanic** - Hold to door, hear subtle click on correct number
- 🎰 **Multiple Safe Types** - Padlock, electronic keypad, bank vault wheel

### Future Considerations
- 🏠 **Themed Environments** - Heist room, bank vault, escape room
- 👥 **Race Mode** - Two safes side by side, first to crack wins
- 🗝️ **Lock Picking Mode** - Different mechanic with tension wrench

---

## Files Created

### Scripts (3 files)
✅ `SafeCrackingGameManager.cs` - Central hub, event-driven, all audio centralized
✅ `SafeDial.cs` - Combo state machine, rotation tracking, haptic feedback
✅ `SafeDoor.cs` - Handle interaction, door swing coroutine, lock/unlock

### Documentation (3 files)
✅ `Phase1Setup.md` - This file!
✅ `SafeCrackingPlan.md` - 4-phase development roadmap
✅ `PotentialFeatures.md` - Future ideas and game modes

### Folders Created (empty, ready for content)
- [ ] `Prefabs/` - For Safe.prefab when ready
- [ ] `Audio/` - Drop AudioClips here (Phase 2)
- [ ] `Materials/` - Safe metal material, dial chrome material

---

## Technical Notes

### Rotation-to-Number Mapping
- 360° / 40 numbers = **9 degrees per number**
- Number 0 at top (12 o'clock), numbers increase going **clockwise**
- Angle tracked via `Mathf.Atan2` on interactor position relative to dial center
- `Mathf.DeltaAngle` handles the 360°→0° wraparound cleanly

### XR Grab with Manual Rotation
- XRGrabInteractable is used only for **grab detection** (selectEntered/selectExited)
- Tracking is disabled (Track Position/Rotation off) — SafeDial.cs controls all rotation
- The dial's visual rotation is applied directly via `transform.localEulerAngles`

### Event-Based Architecture
- **SafeDial** fires events for each state change — zero coupling to GameManager
- **SafeDoor** fires events on open/locked attempts
- **SafeCrackingGameManager** subscribes in OnEnable, unsubscribes in OnDisable
- This pattern means you can test SafeDial and SafeDoor in isolation

### Tolerance System
- `IsOnNumber(target)` checks if `currentNumber` is within `numberTolerance` of the target
- Wraparound handled correctly: number 39 and number 0 are 1 apart, not 39 apart
- Default tolerance of ±1 means any of 3 adjacent numbers (out of 40) counts as correct

---

## Support & Troubleshooting

**Enable Debug Logs:**
1. Check **Debug Mode** in SafeCrackingGameManager inspector
2. Watch Console during gameplay
3. Logs show: current state transitions, number locks, combo failures, door events

**Key Console Messages to Watch For:**
- `[SafeDial] Locked digit 1: X (target: Y)` — first number accepted
- `[SafeCracking] Combination complete! Door unlocked.` — combo done, grab handle
- `[SafeCracking] Combination failed` — wrong direction, restart
- `[SafeCracking] Safe opened! Total: X, Streak: Y, Time: Z` — success

**Verify Setup:**
- Use "Testing Checklist" above
- Check "Common Issues" section
- Confirm all component references assigned (missing refs = silent failures)

**Still stuck?**
- Re-read the relevant setup step
- Check all Inspector references are dragged in (most common issue!)
- Enable Debug Mode and read Console — the state machine logs every transition
- Verify XR Grab Interactable has Track Position and Track Rotation **both unchecked**

---

## Summary

You now have a fully functional Safe Cracking VR mini-game!

**What works:**
✅ Grab and rotate combination dial
✅ Full 3-step combo sequence with direction tracking
✅ Haptic ticks on every number, strong haptic on correct locks
✅ Door handle interaction — locked thud vs. open swing
✅ Smooth door animation on correct combo
✅ Stats: crack timer, safes cracked total, current streak
✅ Debug mode to show/hide combination
✅ Reset system — new combo, door closes, dial resets

**What's next:**
🔊 Source and assign audio clips (dial clicks, lock thunks, vault swing, reward chime)
⏱️ Add timed challenge mode with countdown
🎯 Add difficulty settings (tolerance, combo length)
🔊 Add stethoscope mechanic for audio-only number detection

**Enjoy your VR Safe Cracking game! Time to crack some codes!** 🔐
