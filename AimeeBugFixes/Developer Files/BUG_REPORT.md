# AIMeE (RobotMining) Bug Report

**Affects**: Multiplayer (dedicated server + client), some also affect single player  
**Classes**: `Assets.Scripts.Objects.RobotMining`, `Assets.Scripts.Vehicles.WheeledBase`, `TerrainSystem.Vein`, `TerrainSystem.VoxelTerrain`  
**Game Version**: Deep Combustion Update (tested February 2026)  
**Severity**: All bugs make AIMeE non-functional or visually broken, primarily in multiplayer

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

---

## Bug 8: Carrying AIMeE and bumping terrain/objects launches the player — FIX DEPLOYED

**Observed**: When a player picks up (drags) AIMeE and walks near walls, terrain, or other objects, bumping AIMeE against anything sends the player flying — sometimes launching them high into the air. This does not happen with other draggable objects like the storage crate.

**Root cause**: `WheeledBase.PhysicsUpdate()` runs every `FixedUpdate` including while being dragged. It calls `EnableWheelColliders(HasAuthority)` which keeps all Unity `WheelCollider` components active. When the carried AIMeE contacts terrain or objects:

1. **WheelCollider suspension springs** compress on contact → generates large spring forces pushing AIMeE away from the surface  
2. **WheelCollider tire friction** generates lateral/longitudinal reaction forces on contact  
3. **Motor/brake torque** may still be applied if `WheelCollider.isGrounded` becomes true (residual `CurrentMotorPower`/`CurrentBrakePower` lerp toward targets slowly due to `ForceScale=1000`)  
4. AIMeE's mass is reduced to `_startMass × DraggedMassScale (0.1) × massRatio` by `DragInSlot()`, making accelerations ~10× larger  
5. Forces propagate through the **unbreakable joint chain** (`CharacterJoint` with `breakForce=Infinity, breakTorque=Infinity`) from AIMeE → Pivot rigidbody → `HingeJoint` → Player rigidbody  
6. The `HingeJoint.connectedMassScale = 0.001` makes the player appear 1000× lighter from the joint's perspective, further amplifying the force effect on the player  

Storage crates (`Container` extending `DraggableThing`) don't have this problem because they have **no `WheelCollider` components** — they're just a plain rigidbody with standard colliders. The only forces acting on them through the joint are gravity and collision normals, which are manageable.

The key code path:
```csharp
// WheeledBase.PhysicsUpdate() — runs every FixedUpdate, even while dragged
public override void PhysicsUpdate()
{
    base.PhysicsUpdate();                           // DynamicThing position tracking
    this.EnableWheelColliders(this.HasAuthority);   // ← Keeps WheelColliders ACTIVE while dragged
    // ...lerp motor/brake toward targets...
    for (int i = 0; i < this.Wheels.Count; i++)
    {
        wheel.Apply(CurrentMotorPower, CurrentBrakePower, CurrentSteeringAngle);  // ← Forces applied!
    }
}
```

`DragInSlot()` does NOT call `OnEnterInventory()` (which would set `WheelsT.SetActive(false)`), so the WheelColliders remain enabled throughout the drag operation. `DynamicThing.PhysicsUpdate()` has a `if (ParentSlot != null && !IsBeingDragged) return` guard, but `IsBeingDragged` is true — so execution falls through and `WheeledBase.PhysicsUpdate()` runs in full.

**Fix (Harmony)**: Prefix on `WheeledBase.PhysicsUpdate()` — when `Joint != null || IsChild`:
1. **Disable all WheelColliders** (`wc.enabled = false`) — stops suspension spring and friction forces entirely. Body colliders (BoxCollider/MeshCollider) remain active for normal wall/terrain collisions.
2. **Zero `CurrentMotorPower`, `CurrentBrakePower`, `CurrentSteeringAngle`** — clears residual torque so values don't snap back on drop.
3. **Call `DynamicThing.PhysicsUpdate()` directly** (via IL-emitted non-virtual `call` to bypass the virtual dispatch) — maintains position/rotation tracking needed for joint movement and network sync.
4. **Skip original** `WheeledBase.PhysicsUpdate()` — prevents `EnableWheelColliders(true)` from re-enabling them and wheel force application.

When the player drops AIMeE, `MoveToWorld()` destroys the joint → `Joint` becomes null → next `PhysicsUpdate` runs the original code path which calls `EnableWheelColliders(HasAuthority)`, restoring them.

**Suggested native fix**: In `WheeledBase.PhysicsUpdate()`, add an early return when `IsBeingDragged`:
```csharp
public override void PhysicsUpdate()
{
    base.PhysicsUpdate();
    if (this.IsBeingDragged)
    {
        this.EnableWheelColliders(false);  // Disable WheelColliders while dragged
        return;                            // Skip all wheel force application
    }
    // ... rest of existing code
}
```

**Status**: 🔧 Fix deployed, awaiting confirmation.

---

## Bug 9: AIMeE teleports when stuck for 60 seconds (modes 1, 2, 3, 5) — FIX DEPLOYED

**Observed**: AIMeE visibly teleports when stuck against terrain, walls, or another AIMeE for about a minute. Particularly visible in mine mode (mode 3) and when two AIMeEs are pushing against each other. Rotation also snaps to north-facing.

**Root cause**: `UpdateEachFrame()` calls `TryUnstuck()` in modes 1, 2, 3, and 5. `TryUnstuck()` checks every 60 seconds (`_isStuckCheckAmount = 60f`) whether AIMeE moved less than 0.1 units (`_isStuckMovementAmount = 0.1f`). If stuck, it calls `Unstuck()` which **directly writes `ThingTransformPosition`** to a new point and **resets rotation to `Quaternion.identity`** — a visible teleport + rotation snap.

Additionally, there is a bug in `Unstuck()` itself: `UnityEngine.Random.Range(0, 1)` uses the **int overload** (not float), which always returns 0. This means the XZ displacement vector is always `(0, 0)`:
```csharp
// Unstuck() — the teleport
Vector2 vector = new Vector2(
    (float)Math.Sin(num * _degreesToRadians),
    (float)Math.Cos(num * _degreesToRadians))
    * MathF.Sqrt((float)(UnityEngine.Random.Range(0, 1) * 2));
    //                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    //                    int overload → always returns 0
    //                    sqrt(0 * 2) = 0 → vector is always (0,0)

thingTransformPosition.y = SpawnPoint.GetSafePoint(pos).y + 0.5f;  // ← Y snap
ThingTransformPosition = thingTransformPosition;  // ← Direct position write
ThingTransform.rotation = Quaternion.identity;  // ← Rotation snap to north
```

So `Unstuck()` doesn't actually move the AIMeE horizontally — it only snaps Y to `SpawnPoint.GetSafePoint()` height + 0.5f and resets rotation to identity. This can still cause visible "teleporting" if the safe point is at a different elevation (mine pit, hillside, underground), and the rotation snap is always jarring.

Two AIMeEs pushing against each other trigger this because both are stuck (positioned within < 0.1 units of their position 60 seconds ago), so both teleport simultaneously.

The Bug 2 scenario (off/unpowered) is a subset — `TryUnstuck` has no `OnOff`/`Powered` guard either — but the stuck-teleport problem also occurs during **normal powered operation** when the AIMeE is simply wedged.

**Fix (Harmony)**: Three-part fix:
1. **Prefix on `Unstuck()`** — replaces the teleport with physics-based escape:
   - Zero current velocity to break momentum loops
   - Apply backward impulse (`-forward * 2`) via `AddForce(VelocityChange)` — purely horizontal, no vertical component to avoid immersion-breaking hops
   - Apply random yaw torque (`±3`) to change heading
   - For mode 3, set `_reversing = 8f` to trigger extended reverse in the Roam driving loop
   - Skip original `Unstuck()` entirely
2. **Velocity-based stuck detection** — catches cases where AIMeE drives forward at full power but velocity stays near 0 for 3+ seconds (e.g., two AIMeEs pushing against each other):
   - Track per-AIMeE stuck-drive-time accumulator
   - If `TargetMotorPower > 0` and `VelocityMagnitude < 0.1` for 3+ continuous seconds → trigger 5-second reverse with random steering
3. **Increased obstacle raycast** from 0.2m to 0.5m to detect other AIMeEs and larger obstacles earlier. Added random steering angle (±30°) on reverse to break infinite forward-reverse-forward loops.

**Suggested native fix**: 
1. In `Unstuck()`, use `Random.Range(0f, 1f)` (float overload) instead of `Random.Range(0, 1)` (int overload) so the displacement actually works.
2. Replace the direct `ThingTransformPosition` write with `RigidBody.AddForce()` to keep movement physics-based.
3. Remove the `Quaternion.identity` rotation reset — it's disorienting and serves no purpose.
4. Add velocity-based stuck detection (near-zero velocity while motor is active) as a faster first-response before the 60-second position check.
5. Increase the `Roam()` obstacle raycast distance from 0.2m to at least 0.5m so AIMeEs detect each other sooner.

**Status**: 🔧 Fix deployed, awaiting confirmation.

---

## Bug 10: AIMeE mines distant ore instead of closest — FIX DEPLOYED

**Observed**: When multiple ore blocks are available, AIMeE often mines ore that is far away while closer ore exists. Particularly noticeable with multiple AIMeEs mining — some travel long distances while nearby ore goes unmined.

**Root cause**: `VoxelTerrain.GetAimeeMinableQueue()` delegates to `vein.GetAimeeMinables()` which iterates each vein's `_minables` array and adds qualifying ore (active, in bounds, near surface, not already claimed) in **array index order** — no distance sorting:

```csharp
// Vein.GetAimeeMinables() — no distance sorting
public void GetAimeeMinables(List<TargetMinableData> queue, int maxDepth, BoundsInt searchBounds)
{
    for (int i = 0; i < this._minables.Length; i++)  // ← Array order, not distance
    {
        if (this.GetActive(i))
        {
            Vector3Int position = this._minables[i].WorldPositionInt(this.VeinWorldPosition);
            if (searchBounds.Contains(position) && this.IsNearSurface(position, maxDepth))
            {
                int item = this.MakeMinableHash(i);
                if (!Vein.AllAimeeQueuedMinables.Contains(item))
                {
                    Vein.AllAimeeQueuedMinables.Add(item);
                    queue.Add(new TargetMinableData { Vein = this, MinableIndex = i });
                }
            }
        }
    }
}
```

The queue pops from the **end** (`queue[queue.Count - 1]`), so the last-added ore is mined first. Since veins are iterated by chunk lookup order and ore by array index, the closest ore may be added first and thus mined last.

With `AllAimeeQueuedMinables` preventing duplicate assignments across AIMeEs, the first AIMeE to fill its queue claims ore blocks, and subsequent AIMeEs are forced to take whatever remains — often distant ore.

**Fix (Harmony)**: After `GetAimeeMinableQueue` fills the queue, sort by `sqrMagnitude` distance from the AIMeE's position in **descending** order (farthest at index 0, closest at end). Since the queue pops from the end, closest ore is dequeued and mined first. Sorting happens before any per-AIMeE queue cap, so excess entries removed from the front are the farthest ore — keeping only the closest blocks.

```csharp
// Sort: farthest at front (index 0), closest at end (dequeued first)
queue.Sort((a, b) =>
{
    float distA = (a.Vein.GetMinableWorldPosition(a.MinableIndex) - aimeePos).sqrMagnitude;
    float distB = (b.Vein.GetMinableWorldPosition(b.MinableIndex) - aimeePos).sqrMagnitude;
    return distB.CompareTo(distA); // Descending: farthest first
});
```

**Suggested native fix**: 
1. Sort the queue by distance in `GetAimeeMinableQueue()` after all `GetAimeeMinables()` calls complete (sorting at the vein level doesn't help since ore from multiple veins interleaves).
2. Sort descending so closest ore is at the end (where it gets dequeued first).
3. Consider a distance-weighted allocation when multiple AIMeEs are active — e.g., assign each ore block to the nearest AIMeE rather than letting the first bot to fill its queue claim everything within range.

**Status**: 🔧 Fix deployed, awaiting confirmation.

---

## Bug 11: Multiple AIMeEs starve each other / stuck-reverse-stuck loops — FIX DEPLOYED

**Observed**: Two related issues:
1. With 2+ AIMeEs working the same area, the first bot to scan claims most of the nearby ore via `AllAimeeQueuedMinables`, leaving nothing for the second robot. With no per-bot queue limit, one AIMeE can queue all 30+ ore blocks in range.
2. After Bug 9's physics-based reverse triggers, the robot sometimes immediately re-detects as stuck (because the reverse just started and velocity is still low) → triggers another reverse → oscillating forward/backward indefinitely.

**Root cause**:
1. **Queue starvation**: `GetAimeeMinableQueue()` has no per-AIMeE cap on the queue. Whatever `GetAimeeMinables()` returns, the bot takes. `AllAimeeQueuedMinables` (a static HashSet) prevents double-assignment, so the first bot to fill monopolizes all ore.
2. **Reverse loops**: Velocity-based stuck detection fires continuously — the robot triggers a reverse, starts moving backward, slows down near the obstacle it reversed from, and immediately satisfies the "low velocity while motor active" condition again. Without a cooldown, it oscillates.

**Fix (Harmony)**:
1. **Queue cap of 5** per AIMeE — after filling and distance-sorting (Bug 10), remove entries beyond 5 from the front (farthest ore pruned first). Excess entries are also removed from `AllAimeeQueuedMinables` so other AIMeEs can claim them.
2. **Post-reverse cooldown of 3 seconds** — after triggering a reverse, the stuck detector ignores low velocity for 3 seconds, giving the robot time to complete the reverse maneuver before re-evaluation. Tracked via a per-AIMeE `_reverseCooldown` timestamp.

**Suggested native fix**:
1. Cap `_minableDataQueue` to ~5 entries per AIMeE after `GetAimeeMinableQueue()`. Remove excess from `AllAimeeQueuedMinables`.
2. After triggering reverse in `Unstuck()`, set a cooldown flag that bypasses stuck detection for a few seconds.

**Status**: 🔧 Fix deployed, awaiting confirmation.

---

## Bug 12: Ghost ore — mined ore stays visible on multiplayer clients — FIX DEPLOYED

**Observed**: After AIMeE successfully mines an ore block, the ore disappears on the server but **remains visible on all clients**. The ghost ore block cannot be mined by players and floats in mid-air if surrounding terrain was previously mined by the player. Persists until the client re-enters the area or reconnects.

**Root cause**: AIMeE's mining path calls `Vein.TryMineServer()` which marks ore as mined server-side (`SetActive(false)`) and refreshes the server render via `DirtyAllMinables()`. However, **`TryMineServer()` does NOT modify terrain density**.

Client sync relies on `VoxelTerrain.SetDensityWorldSpace()`, which sets the terrain density to 0 at the mined position and creates a `VoxelChangeEvent` in a `SyncList`. Clients deserialize the event and call `Vein.MineAtPositionClient()` to visually remove the ore on their end.

**Every other mining caller** in the game calls `SetDensityWorldSpace` before `TryMineServer`:

| Caller | Calls SetDensityWorldSpace? |
|--------|---------------------------|
| `OnServer.MineAsteroid` (player mining) | ✅ Yes |
| `OnServer.MineIce` | ✅ Yes |
| Explosions | ✅ Yes |
| Quarry / HorizontalQuarry | ✅ Yes |
| **RobotMining** (AIMeE) | ❌ **No** |

`RobotMining` is the **only mining caller that skips the density update**. This was invisible before our other fixes because AIMeE rarely mined successfully (Bug 1 deadlocked mining after the first ore).

```csharp
// What every other caller does:
VoxelTerrain.SetDensityWorldSpace(position, 0f);  // ← Creates VoxelChangeEvent for clients
vein.TryMineServer(position, out ore, dropPos);

// What RobotMining.Roam() does:
vein.TryMineServer(position, out ore, dropPos);  // ← No density change → clients see stale ore
```

**Fix (Harmony)**: Postfix on `Vein.TryMineServer` — when mining succeeds (`__result == true`) and `NetworkServer.HasClients()`, injects **two** `VoxelChangeEvent` entries directly into `VoxelTerrain.VoxelChangeEvents` (the SyncList that propagates to clients). This **bypasses `SetDensityWorldSpace` entirely** — no server-side octree modification, no `UpdateCanAirPass`, no `DirtyLods`, no terrain side effects. Zero voxel density changes on the server.

The two-event approach:
1. **Event 1 — density byte 0**: Client receives density 0, calls `ShouldReleaseMinables(0)` which returns `true`, triggering `MineAtPositionClient` → ore model is visually removed.
2. **Event 2 — original density byte**: Client receives the original terrain density (the value before mining), restoring the terrain to its actual state. Since the ore has already been released by Event 1, no visible change occurs — the terrain simply stays intact with no hole.

This solves the earlier problem where a single event with density 0 would carve visible holes in client terrain, and a single event with the original density would not trigger `MineAtPositionClient` (because `ShouldReleaseMinables` returns `false` for density byte >= 127).

The postfix fires for ALL `TryMineServer` callers, but is harmless for non-AIMeE callers because they've already synced via `SetDensityWorldSpace`.

**Suggested native fix**: Add `VoxelTerrain.SetDensityWorldSpace(position, 0f)` before the `TryMineServer()` call in `RobotMining.Roam()`, matching all other mining callers:
```csharp
// In RobotMining.Roam(), before TryMineServer call:
VoxelTerrain.SetDensityWorldSpace(targetMinableData.Position, 0f);
this._vein.TryMineServer(targetMinableData.Position, out Item ore, base.ThingTransformPosition);
```

**Status**: ✅ Confirmed working (v0.15.3).

---

## Bug 13: AIMeE targets ore deep underground in tunnels/caves — CONFIRMED

**Observed**: AIMeE attempts to mine ore that is deep below the surface inside player-dug tunnels or natural caves. She drives to the surface position above the ore, spins in circles unable to reach it, then eventually gives up and moves on. Wastes significant time and battery on unreachable targets.

**Root cause**: `Vein.IsNearSurface(position, maxDepth)` checks **exactly one voxel** — the point `maxDepth` blocks above the ore:

```csharp
private bool IsNearSurface(Vector3Int position, int maxDepth)
{
    return VoxelTerrain.GetDensityWorldSpace(position + Vector3Int.up * maxDepth) < 0.49803922f;
    //                                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    //                                        Single point, 3 blocks up from ore
}
```

With `maxDepth = 3` (static readonly), this checks one point 3 blocks above the ore. In a tunnel or cave, that single point can be **empty air** (the tunnel/cave void), so the ore passes the surface check — even though there is a solid cave ceiling between the void and the actual surface.

Example cross-section (bottom to top):
```
Surface  ░░░░░░░░  density < 0.5 (air)
         ████████  density ≥ 0.5 (solid terrain — cave ceiling)
Cave     ░░░░░░░░  density < 0.5 (cave/tunnel air) ← IsNearSurface checks HERE
         ████████  density ≥ 0.5 (solid rock)
Ore      ◆◆◆◆◆◆◆  ore position
```

The check at `ore + up*3` lands in the cave air → reports "near surface" → AIMeE queues this underground ore.

**Fix (Harmony)**: Prefix on `Vein.IsNearSurface` replacing the single-voxel check with a column scan:
1. Scans upward from 1 to 64 blocks above the ore position
2. Tracks `foundEmpty`: first transition from solid → empty (the mining face where AIMeE would dig)
3. Tracks `solidAboveAir`: if solid terrain appears again after empty space → cave ceiling detected → ore is underground
4. Ore is surface-reachable only if empty space starts within `maxDepth` blocks AND no ceiling is found above within the scan range

```csharp
bool foundEmpty = false;
bool solidAboveAir = false;
for (int y = 1; y <= 64; y++)
{
    float density = VoxelTerrain.GetDensityWorldSpace(position + Vector3Int.up * y);
    bool isSolid = density >= 0.498f;
    if (!foundEmpty && !isSolid)
    {
        foundEmpty = true;
        if (y > maxDepth) return false;  // Empty space too far above ore
    }
    else if (foundEmpty && isSolid)
    {
        solidAboveAir = true;  // Cave ceiling detected
        break;
    }
}
return foundEmpty && !solidAboveAir;
```

Bug 7's vicinity count (prefix on `GetNumberOfMinablesNearSurface`) also uses this improved surface check so that `MineablesInVicinity` excludes underground ore.

**Suggested native fix**: Replace `IsNearSurface` with the column scan above (scan height of 64 covers all realistic terrain depths). Alternatively, expose `maxScanHeight` as a configurable parameter.

**Status**: ✅ Confirmed working (v0.15.3).

---

## Bug 14: Stale mining queue — queue not cleared on pickup, power-on, or target change — FIX DEPLOYED

**Observed**: After picking up and repositioning an AIMeE, she drives back to her old location to mine ore from the previous queue instead of scanning for nearby ore at the new location. The same happens after turning AIMeE off, moving her, and turning her back on. When TargetX/Y/Z is set by IC to redirect AIMeE, she still tries to mine from the old queue first. In multi-AIMeE setups, stale `AllAimeeQueuedMinables` hash entries from relocated bots block other AIMeEs from claiming ore near the abandoned location.

**Root cause**: `_minableDataQueue` is only refilled when it empties naturally (all entries consumed by mining). It is **never cleared** when:

1. **AIMeE is picked up** (`IsCursor` becomes true) — she moves to a new location but still has a queue full of ore positions from the old location
2. **AIMeE is turned on** (`OnOff` transitions false → true) — she may have been repositioned while off, but her queue still points to the old location
3. **TargetX/Y/Z is set via IC** (`SetLogicValue`) — the player intends AIMeE to drive to a new area, but she'll chase stale queue entries first

The `AllAimeeQueuedMinables` HashSet (global across all AIMeEs) retains entries for queued ore. When an AIMeE is moved away, those entries linger, preventing other AIMeEs from adding that ore to their own queues.

```csharp
// The queue is only refilled here (in Roam), never cleared on state change:
if (this._minableDataQueue.Count <= 0)
{
    this.ClearMinableQueue();  // clears AllAimeeQueuedMinables entries
    VoxelTerrain.GetAimeeMinableQueue(...);
}
// If count > 0, old entries persist indefinitely
```

**Fix (Harmony)**: Two patches on `RobotMining`:

1. **Prefix on `UpdateEachFrame`**: Tracks per-AIMeE `IsCursor` and `OnOff` state using instance-keyed dictionaries. Calls `ClearMinableQueue()` when:
   - `IsCursor` transitions to true (picked up)
   - `OnOff` transitions false → true (turned on)

2. **Postfix on `SetLogicValue`**: When `logicType` is `TargetX`, `TargetY`, or `TargetZ`, calls `ClearMinableQueue()` to discard stale entries so the queue is refilled at the new destination.

```csharp
// Prefix: detect state transitions
bool isCursor = __instance.IsCursor;
bool isOn = __instance.OnOff;
if (isCursor && !wasCursor)       // picked up
    __instance.ClearMinableQueue();
else if (isOn && !wasOn)           // turned on
    __instance.ClearMinableQueue();

// Postfix: detect target writes
if (logicType == LogicType.TargetX || logicType == LogicType.TargetY ||
    logicType == LogicType.TargetZ)
    __instance.ClearMinableQueue();
```

**Suggested native fix**: Call `ClearMinableQueue()` in three places:

1. At the start of `RobotMining.UpdateEachFrame()`, when `IsCursor` transitions true (track previous state with a field)
2. At the start of `RobotMining.UpdateEachFrame()`, when `OnOff` transitions false → true
3. In `RobotMining.SetLogicValue()`, after writing `TargetX`, `TargetY`, or `TargetZ`

**Status**: ✅ Confirmed working (v0.15.3).

---

## Summary

| Bug | Issue | Severity | Status |
|-----|-------|----------|--------|
| 1 | Mine deadlock after first ore | Critical | ✅ Confirmed |
| 2 | Off-state teleport/hop | High | ✅ Confirmed |
| 3 | Frozen tread animations on client | Medium | 🔧 Deployed |
| 4 | No roaming when queue empty | High | 🔧 Deployed |
| 5 | Player launch when dragging | High | 🔧 Deployed |
| 6 | MineablesInQueue always 0 on client | Medium | 🔧 Deployed |
| 7 | MineablesInVicinity overcounts | Medium | 🔧 Deployed |
| 8 | Player launch when carrying near terrain | High | 🔧 Deployed |
| 9 | Teleport when stuck | Medium | 🔧 Deployed |
| 10 | Mines distant ore first | Low | 🔧 Deployed |
| 11 | Multi-AIMeE queue starvation / reverse loops | Medium | 🔧 Deployed |
| 12 | Ghost ore on clients after mining | High | ✅ Confirmed |
| 13 | Targets underground ore in tunnels | Medium | ✅ Confirmed |
| 14 | Stale queue after pickup/power-on/target change | Medium | ✅ Confirmed |
