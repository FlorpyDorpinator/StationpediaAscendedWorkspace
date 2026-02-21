# AIMeE (RobotMining) Bug Report — Orbital Update

**Affects**: Multiplayer (dedicated server + client)  
**Class**: `Assets.Scripts.Objects.RobotMining` / `Assets.Scripts.Vehicles.WheeledBase`  
**Severity**: All bugs make AIMeE non-functional or visually broken in multiplayer

---

## Bug 1: AIMeE stops mining after first ore in Roam mode (Mode 3) — CONFIRMED FIXED

**Observed**: AIMeE enters Roam (Mode 3), mines one ore, then permanently refuses to mine again. Re-sending mode 3 via IC10 has no effect. 

**Root cause**: `_mineCancellation.Initialized` is never reset after `Mine()` completes.

`MovingToMineable()` calls `_mineCancellation.CancelAndInitialize()` before launching async `Mine()`. This sets `Initialized = true`. After `Mine()` finishes, `Cancel()` is **never called** — the only `Cancel()` call is in `OnDestroy()`.

Every subsequent call to `MovingToMineable()` hits this guard and returns immediately:
```csharp
// MovingToMineable(), line ~1079
if (this._mineCancellation.Initialized) return true;  // ← Stuck here forever
```

**Secondary**: `OnChildEnterInventory()` (chip swap) resets `TargetMinable` but does **not** cancel `_mineCancellation` or call `ClearMinableQueue()`. Stale `_minableDataQueue` entries and `Vein.AllAimeeQueuedMinables` hashes persist across chip swaps.

**Regression from**: Old synchronous `Mine()` used animation callbacks (`OnCustomImportFinished`/`OnCustomExportFinished`) that self-reset state. The new async `UniTaskVoid Mine()` has no completion handler.

**Fix (Harmony)**: Call `_mineCancellation.Cancel()` in `OnMinedOre()` postfix. Also cancel + `ClearMinableQueue()` on chip swap in `OnChildEnterInventory()` postfix. Safety timeout in `MovingToMineable()` prefix resets after 30s.

**Status**: ✅ Confirmed working on dedicated server — AIMeE mines continuously.

---

## Bug 2: AIMeE hops/teleports when off but receiving logic — CONFIRMED FIXED

**Observed**: AIMeE is turned off but an IC10 or logic transmitter continues setting Mode to 1–5. The robot periodically hops into the air and sometimes teleports to a random nearby position.

**Root cause**: `UpdateEachFrame()` has **no `OnOff`/`Powered` guard** before the mode switch.

While `MoveTowards()` checks `(!OnOff || !Powered || Error > 0)` and returns false, two methods called from the mode switch do not:

1. **`TryUnstuck()`** (called in modes 1, 2, 3, 5): Checks if robot moved < 0.1 units in 60 seconds. A powered-off robot always fails this check → calls `Unstuck()` → **writes `ThingTransformPosition` directly** to a random point ~2 units away = **teleport**.

2. **`PathToTarget()`** (mode 5): Applies `RigidBody.AddForce(Vector3.up * Random.Range(0.9f, 5f), ForceMode.VelocityChange)` when stationary = **hop**.

**Fix (Harmony)**: Prefix on `TryUnstuck()` and `PathToTarget()` returning false when `!OnOff || !Powered`. Postfix on `UpdateEachFrame()` enforcing brakes when off.

**Status**: ✅ Confirmed working on dedicated server — AIMeE stays still when powered off.

---

## Bug 3: Tread animations frozen on multiplayer clients — FIX DEPLOYED

**Observed**: When AIMeE moves in multiplayer, clients see the robot moving but treads are completely frozen. The treads animate correctly in single player and on the server.

**Root cause (revised after server testing)**:

The original hypothesis (server `IsOccluded` blocking `AnimateAuthority`) was partially correct for an edge case, but the **primary issue** is on the client:

1. `WheeledBase.PhysicsUpdate()` calls `EnableWheelColliders(HasAuthority)` every frame
2. On the client (`HasAuthority=false`), this **disables all WheelColliders**
3. `WheelAudio()` calls `WheelCollider.GetGroundHit()` → returns `false` on disabled collider → **early returns without updating `WheelRpm`**
4. `Animate()` (client path) reads `WheelRpm` for UV texture scrolling: `num = WheelRpm / 60f * UvOffsetScale`
5. `WheelRpm = 0` → `num = 0` → `_lastOffset` never changes → **frozen tread UV**
6. `RoverUpdate` (network sync struct) only syncs `WheelTransform.localEulerAngles` — it does NOT sync `WheelRpm` or UV offset data

AIMeE's treads use UV texture scrolling (`Material.SetTextureOffset`) rather than geometric wheel rotation, so even though WheelTransform rotation is synced, the visual tread effect is completely absent.

**Fix (Harmony)**:
- **Part A (Client prefix)**: Before `PhysicsUpdate`'s wheel loop, estimate `WheelRpm` from `VelocityMagnitude` (which IS synced via `PhysicsUpdateMessage.Velocity`). Formula: `rpm = velocity × 60 / (2π × radius)`. Direction from `RigidBody.velocity · ThingTransform.forward`.
- **Part B (Server postfix, safety net)**: When `HasAuthority && IsOccluded`, force `AnimateAuthority()` to ensure `WheelTransform` data stays current for network sync.

**Status**: 🔧 Fix deployed, awaiting visual confirmation on client.

---

## Bug 4: Roam regression — AIMeE stops exploring when no ore found — FIX DEPLOYED

**Observed**: AIMeE used to roam farther and farther when no ore was nearby. In the Orbital Update, she stops moving entirely when no ore is within scan range.

**Root cause**: Two changes in Roam():

1. **Early return added**: When `_minableDataQueue.Count <= 0` after scanning, the new code does `return;` — this skips all driving logic (steering, motor, obstacle avoidance). In the old code, execution fell through to the driving section, allowing AIMeE to continue roaming while periodically rescanning.

2. **`MinableSearchArea` halved**: Changed from `32` to `16`, reducing ore detection range by half.

```csharp
// NEW code - Orbital Update
if (this._minableDataQueue.Count <= 0)
{
    return;  // ← Bug: skips ALL driving logic below
}
// ... driving logic never reached when no ore nearby
```

**Fix (Harmony)**: Postfix on `Roam()` that applies the vanilla driving logic (obstacle avoidance + random steering + motor power) when the `_minableDataQueue` is empty and storage isn't full. Also restores `MinableSearchArea` to `32` via reflection in plugin init.

**Status**: 🔧 Fix deployed, awaiting behavioral confirmation.

---

## Bug 5: Picking up / dragging AIMeE launches player into the air — FIX DEPLOYED

**Observed**: When a player grabs/drags AIMeE (holding it), the player is launched upward or sideways, sometimes flying very high. Effects are worse in multiplayer.

**Root cause**: `DraggableThing.DragInSlot()` creates a `ConfigurableJoint` connecting the AIMeE and player rigidbodies, and reduces AIMeE's mass to `startMass × 0.1 × (parentMass / aimeeMass)` (~10% of original). However, `UpdateEachFrame()` continues executing AIMeE's mode logic:

1. **WheelCollider motor torque** continues being applied via `PhysicsUpdate()` — forces transfer to the player through the joint
2. **`Follow()`** mode drives toward the nearest human, who IS the dragger — creating a positive feedback loop
3. **`TryUnstuck()`** fires after 60 seconds of near-zero movement (player holding AIMeE is near-stationary) — calls `Unstuck()` which directly writes `ThingTransformPosition` to a random point, teleporting both connected bodies
4. **`PathToTarget()`** can apply `AddForce(Vector3.up * Random.Range(0.9, 5), VelocityChange)` — upward launch through joint

The combination of reduced mass + active motor torque + joint = all wheel forces directly accelerate the player's rigidbody.

```csharp
// DynamicThing.DragInSlot() — mass reduction
base.RigidBody.mass = _startMass * DraggedMassScale * (parentLink.Mass / base.RigidBody.mass);
// DraggedMassScale = 0.1f → AIMeE becomes ~10% of original mass
// Joint transfers all forces from AIMeE wheels → player body
```

**Fix (Harmony)**:
1. **Postfix on `UpdateEachFrame()`**: If `Joint != null || IsChild`, zero `TargetMotorPower`, `TargetBrakePower`, and `TargetSteeringAngle`. Setting brakes to **0** (not max) is critical — `WheelCollider` brake torque resists `ConfigurableJoint` movement, preventing the player from moving AIMeE with their sightline. A prefix was tried first but broke view-following because it skipped `base.UpdateEachFrame()` which contains `UpdatePivotJoint()` — the method that positions dragged objects on the player's sightline.
2. **Prefix on `TryUnstuck()`**: Block when dragged (`Joint != null || IsChild`), preventing teleportation while held.
3. **Prefix on `PathToTarget()`**: Block when dragged, preventing upward force application through the joint.

**Status**: 🔧 Fix deployed, awaiting confirmation.

---

## Bug 6: MineablesInQueue always 0 on multiplayer clients — FIX DEPLOYED

**Observed**: When reading `MineablesInQueue` from AIMeE on a dedicated server client (e.g., via console display or logic reader), the value is always 0 even while AIMeE is actively mining. Works correctly in single player.

**Root cause**: `GetLogicValue(MineablesInQueue)` returns `_minableDataQueue.Count`. This queue is a **private local `List<TargetMinableData>`** populated exclusively by `Roam()`, which only executes on the **authority** (server). The queue is **not a networked property** — it has no `[ByteArraySync]` attribute and is never included in any sync message.

```csharp
// RobotMining.GetLogicValue(), line ~437
case LogicType.MineablesInQueue:
    return (double)this._minableDataQueue.Count;  // ← Always 0 on client
```

On clients (`HasAuthority=false`):
- `Roam()` never runs → `_minableDataQueue` is never populated → count is always 0
- There is no RPC or network event that communicates queue state to clients
- Terrain/vein data IS synced to clients, but the queue derivation from that data only happens server-side

**Fix (Harmony)**: Postfix on `GetLogicValue()` — when `logicType == MineablesInQueue && !HasAuthority`, call `VoxelTerrain.GetNumberOfMinablesNearSurface()` using the AIMeE's position, search area, and max mining depth. This queries locally-synced terrain data to count surface-reachable ore within range.

The returned value is not the exact server queue count (which may exclude already-queued or unreachable veins), but it is a meaningful non-zero value that correctly indicates ore availability — which is what players actually need from this logic value.

**Note**: Bug 7 (below) fixes the overcount in `GetNumberOfMinablesNearSurface`, which this fix calls on the client side. Both fixes work together for accurate client-side ore counts.

**Status**: 🔧 Fix deployed, awaiting confirmation.

---

## Bug 7: MineablesInVicinity overcounts — no per-ore bounds check — FIX DEPLOYED

**Observed**: After AIMeE has mined out a flat area, `MineablesInVicinity` (logic type) still reports a non-zero count even though there is no visible ore remaining in her vicinity. The count slowly decreases but never reaches 0 despite all reachable ore being gone.

**Root cause**: `GetLogicValue(MineablesInVicinity)` calls `VoxelTerrain.GetNumberOfMinablesNearSurface()`, which:

1. Calls `GetVeinsInBounds()` — this uses a **32-block chunk lookup**. Any 32×32×32 chunk that overlaps the search area returns ALL veins in that chunk.
2. For each returned vein, calls `GetNumberOfReachableMinables()` — which counts **every** active near-surface minable in the **entire vein** with **no position filter**.

```csharp
// Vein.GetNumberOfReachableMinables() — NO bounds check
public int GetNumberOfReachableMinables(int maxDepth)
{
    int num = 0;
    foreach (Minable minable in this._minables)
    {
        if (minable.IsActive && this.IsNearSurface(..., maxDepth))
            num++;  // ← Counts ore anywhere in the vein, even 50+ blocks away
    }
    return num;
}
```

Compare to the mining queue builder `GetAimeeMinables()`, which correctly checks `searchBounds.Contains(position)` for each individual ore block before adding it. The vicinity count was never given this same bounds check — it's a bug in the base game.

A vein can span well beyond the search area. If even one corner of a vein's 32-block chunk overlaps AIMeE's search box, every active ore block in that entire vein inflates the count — including ore far outside her operational range.

**Fix (Harmony)**: Prefix on `VoxelTerrain.GetNumberOfMinablesNearSurface()` replacing the original with a bounds-checked version. The fix:
1. Computes the same `BoundsInt` search box that `GetAimeeMinableQueue` uses
2. Gets veins via the public `Vein.GetVeinsInBounds(BoundsInt, ref List<Vein>)`
3. Iterates each vein's `_minables` array (via reflection)
4. For each minable: checks `IsActive`, then `searchBounds.Contains(worldPos)` (**the missing check**), then `IsNearSurface` (via density query)
5. Returns accurate count, skipping original method

On error, falls through to the original unpatched method as a safety net.

**Status**: 🔧 Fix deployed, awaiting confirmation.
