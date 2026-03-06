# AIMeE — Recommended Native Code Changes

**Target**: Stationeers (Deep Combustion Update)  
**Scope**: 7 method-level changes across 4 classes that resolve 14 interrelated bugs  
**Each section is self-contained and copy-pasteable.**

---

## 1. `RobotMining.UpdateEachFrame` — Add power/drag guard

The mode switch runs with no `OnOff`/`Powered` check, so a powered-off AIMeE still calls `TryUnstuck()` (teleport), `PathToTarget()` (hop), and driving methods. Dragged AIMeEs also apply motor forces through the joint, launching the player.

**Replace the existing method:**

```csharp
public override void UpdateEachFrame()
{
    base.UpdateEachFrame();
    if (WorldManager.IsGamePaused || !GameManager.RunSimulation)
        return;

    // Guard: zero outputs when off, unpowered, or being dragged
    bool isDragged = this.Joint != null || this.IsChild;
    if (!this.OnOff || !this.Powered || isDragged)
    {
        this.TargetMinable = null;
        base.TargetMotorPower = 0f;
        base.TargetBrakePower = isDragged ? 0f : this.Power;  // brake when off, free-wheel when dragged
        base.TargetSteeringAngle = 0f;
        return;
    }

    switch (this.Mode)
    {
    case 1:
        this.TargetMinable = null;
        this.TryUnstuck();
        this.Follow();
        return;
    case 2:
        this.TargetMinable = null;
        this.TryUnstuck();
        this.MoveToTarget();
        return;
    case 3:
        this.TryUnstuck();
        this.Roam();
        return;
    case 4:
        this.TargetMinable = null;
        if (this._unloadTimeout > 0f)
        {
            this._unloadTimeout -= Time.deltaTime;
            return;
        }
        this.Unload();
        return;
    case 5:
        this.TargetMinable = null;
        this.TryUnstuck();
        this.PathToTarget();
        return;
    }
    this.TargetMinable = null;
    base.TargetBrakePower = this.Power;
    this.Target = null;
}
```

> *Fixes: off-state teleport/hop (Bug 2), player launch when dragging (Bug 5)*

---

## 2. `RobotMining.Unstuck` + `TryUnstuck` — Physics-based escape

`Unstuck()` directly writes `ThingTransformPosition` (visible teleport) and resets rotation to `Quaternion.identity`. Also has a `Random.Range(0, 1)` int-overload bug that always returns 0, so horizontal displacement is always zero. `TryUnstuck()` uses a 60-second position delta that's too slow to detect two AIMeEs pushing against each other.

**Replace both methods:**

```csharp
private float _stuckDriveTime;
private float _reverseCooldown;

private void TryUnstuck()
{
    // Fast check: driving at full power but not moving for 5+ seconds
    if (base.TargetMotorPower > 0f && base.VelocityMagnitude < 0.1f
        && this._reverseCooldown <= 0f)
    {
        this._stuckDriveTime += Time.deltaTime;
        if (this._stuckDriveTime > 5f)
        {
            this.Unstuck();
            this._stuckDriveTime = 0f;
        }
    }
    else
    {
        this._stuckDriveTime = 0f;
    }

    if (this._reverseCooldown > 0f)
        this._reverseCooldown -= Time.deltaTime;

    // Original slow check (position delta over 60s) as fallback
    if (Time.time - this._lastIsStuckCheckTime > _isStuckCheckAmount)
    {
        if ((base.ThingTransformPosition - this._lastCheckPos).magnitude < _isStuckMovementAmount)
            this.Unstuck();
        this._lastCheckPos = base.ThingTransformPosition;
        this._lastIsStuckCheckTime = Time.time;
    }
}

private void Unstuck()
{
    // Physics-based: backward impulse + random yaw
    this.RigidBody.linearVelocity = Vector3.zero;
    this.RigidBody.AddForce(-this.ThingTransform.forward * 2f, ForceMode.VelocityChange);
    this.RigidBody.AddTorque(Vector3.up * UnityEngine.Random.Range(-3f, 3f), ForceMode.VelocityChange);

    if (this.Mode == 3) // Roam
        this._reversing = 8f;

    this._reverseCooldown = 3f;
}
```

> *Fixes: teleport when stuck (Bug 9), stuck-reverse-stuck oscillation (Bug 11)*

---

## 3. `RobotMining.Roam` — Continue driving when queue empty + distance sort + queue cap

Three issues: (1) early `return` when queue is empty skips all driving logic, (2) no distance sorting so AIMeE mines far ore before near ore, (3) no per-bot queue cap so first AIMeE monopolizes all ore. Also: `MinableSearchArea` was halved from 32 to 16.

**Replace the constant:**

```csharp
private static int MinableSearchArea = 32;  // was 16 — restore to original value
```

**Replace the method:**

```csharp
private void Roam()
{
    if (this.IsStorageFull)
    {
        OnServer.Interact(base.InteractMode, 6, false);
        return;
    }

    // Fill queue if empty
    if (this._minableDataQueue.Count <= 0)
    {
        this.ClearMinableQueue();
        VoxelTerrain.GetAimeeMinableQueue(
            this.Transform.position,
            (float)MinableSearchArea,
            this._minableDataQueue,
            maxMiningDepth);

        if (this._minableDataQueue.Count > 0)
        {
            // Sort: farthest first (closest at end where it gets dequeued)
            Vector3 pos = base.ThingTransformPosition;
            this._minableDataQueue.Sort((a, b) =>
            {
                float dA = (a.Vein.GetMinableWorldPosition(a.MinableIndex) - pos).sqrMagnitude;
                float dB = (b.Vein.GetMinableWorldPosition(b.MinableIndex) - pos).sqrMagnitude;
                return dB.CompareTo(dA);
            });

            // Cap to 5 — remove farthest (front), free them for other AIMeEs
            const int maxQueue = 5;
            while (this._minableDataQueue.Count > maxQueue)
            {
                var excess = this._minableDataQueue[0];
                excess.Vein.RemoveAimeeMinableHash(excess.MinableIndex);
                this._minableDataQueue.RemoveAt(0);
            }
        }
    }

    // Try mining if we have a target
    if (this._minableDataQueue.Count > 0)
    {
        var q = this._minableDataQueue;
        this.TargetMinable = q[q.Count - 1];

        if (this.MovingToMineable())
            return;

        // Try to mine at target position
        if (this.TargetMinable != null)
        {
            Vector3Int minePos = this.TargetMinable.Value.Vein
                .GetMinableWorldPosition(this.TargetMinable.Value.MinableIndex).FloorToInt();

            // Sync density to clients BEFORE mining (matches all other mining callers)
            VoxelTerrain.SetDensityWorldSpace(minePos, 0f);

            Ore oreMined;
            if (this.TargetMinable.Value.Vein.TryMineServer(minePos, out oreMined, this.position))
            {
                this.OnMinedOre(oreMined);
                this.TargetMinable.Value.Vein.RemoveAimeeMinableHash(this.TargetMinable.Value.MinableIndex);
                this._minableDataQueue.RemoveAt(this._minableDataQueue.Count - 1);
                this.TargetMinable = null;
                return;
            }
        }
    }

    // === DRIVING LOGIC — always runs, even with empty queue ===
    float power = this.Power;
    float speed = base.VelocityMagnitude;
    Vector3 com = this.RigidBody.worldCenterOfMass;

    RaycastHit hit;
    if (this._reversing <= 0f &&
        Physics.Raycast(com, this.ThingTransform.forward, out hit, 0.5f,
            CursorManager.Instance.TerrainHitMask))
    {
        this._reversing = 5f;
        base.TargetSteeringAngle = UnityEngine.Random.Range(-30f, 30f);
    }

    if (this._reversing > 0f)
    {
        float mapped = RocketMath.MapToScale(0f, this.MaxSpeed, power, 0f, speed);
        base.TargetMotorPower = -mapped;
        this._reversing -= Time.deltaTime;
        return;
    }

    if (this._roamTimeout <= 0f)
    {
        this._roamTimeout = UnityEngine.Random.Range(1f, 3f);
        this.Delta = UnityEngine.Random.Range(-15f, 15f);
        base.TargetSteeringAngle = this.Delta;
    }
    this._roamTimeout -= Time.deltaTime;

    base.TargetMotorPower = RocketMath.MapToScale(0f, this.MaxSpeed, power, 0f, speed);
    base.TargetBrakePower = speed > this.MaxSpeed
        ? RocketMath.MapToScale(this.MaxSpeed, this.MaxSpeed * 1.3f, 0f, this.Power, speed)
        : 0f;
}
```

> *Fixes: roam stops when queue empty (Bug 4), mines distant ore first (Bug 10), multi-AIMeE starvation (Bug 11), ghost ore on clients (Bug 12)*

---

## 4. `RobotMining.OnMinedOre` + `OnChildEnterInventory` — Reset mine cancellation token

`OnMinedOre()` never calls `_mineCancellation.Cancel()`, so `MovingToMineable()` deadlocks on `_mineCancellation.Initialized == true` after the first mine. `OnChildEnterInventory()` (chip swap) doesn't cancel mining state either, leaving stale queue entries.

**Add one line to `OnMinedOre`** (at the very top of the method):

```csharp
public virtual void OnMinedOre(Ore oreMined)
{
    this._mineCancellation.Cancel();  // ← ADD THIS LINE

    foreach (Slot slot in this._storageSlots)
    {
        // ... existing code unchanged ...
    }
    // ... rest unchanged ...
}
```

**Add two lines to `OnChildEnterInventory`** (after `this.TargetMinable = null;`):

```csharp
public override void OnChildEnterInventory(DynamicThing newChild)
{
    base.OnChildEnterInventory(newChild);
    this.RefreshError();
    if (GameManager.GameState == GameState.Running)
    {
        if (this.ProgrammableChip != null && newChild is ProgrammableChip)
        {
            if (GameManager.RunSimulation)
                OnServer.Interact(base.InteractMode, 0, false);
            this.TargetX = 0f;
            this.TargetY = 0f;
            this.TargetZ = 0f;
            this.TargetMinable = null;
            this._mineCancellation.Cancel();  // ← ADD THIS LINE
            this.ClearMinableQueue();          // ← ADD THIS LINE
            this.ProgrammableChip.Reset();
        }
        this.ClearError();
    }
}
```

> *Fixes: mine deadlock after first ore (Bug 1)*

---

## 5. `WheeledBase.PhysicsUpdate` — Disable wheels when dragged

`PhysicsUpdate()` keeps WheelColliders active while the robot is being dragged. Suspension spring forces and motor torque transfer through the drag joint to the player, launching them on terrain contact.

**Add an early guard at the top of `PhysicsUpdate`:**

```csharp
public override void PhysicsUpdate()
{
    base.PhysicsUpdate();

    // Disable wheel physics while being carried/dragged
    if (this.Joint != null || this.IsChild)
    {
        this.EnableWheelColliders(false);
        this.CurrentMotorPower = 0f;
        this.CurrentBrakePower = 0f;
        this.CurrentSteeringAngle = 0f;
        return;
    }

    // ... rest of existing code unchanged from here ...
    this.PlayableAreaState = base.CheckPlayableArea();
    // etc.
}
```

> *Fixes: player launch when carrying AIMeE near terrain (Bug 8), combined with UpdateEachFrame guard for player launch when dragging (Bug 5)*

---

## 6. `Vein.IsNearSurface` — Column scan instead of single-voxel check

The current check looks at exactly one point (`position + up * maxDepth`). In tunnels/caves, that point can land in a cave void, falsely marking underground ore as reachable.

**Replace the method:**

```csharp
private bool IsNearSurface(Vector3Int position, int maxDepth)
{
    bool foundEmpty = false;
    for (int y = 1; y <= 64; y++)
    {
        float density = VoxelTerrain.GetDensityWorldSpace(
            position + Vector3Int.up * y);
        bool isSolid = density >= 0.498f;

        if (!foundEmpty && !isSolid)
        {
            foundEmpty = true;
            if (y > maxDepth)
                return false;  // air is too far above ore
        }
        else if (foundEmpty && isSolid)
        {
            return false;  // cave ceiling detected — ore is underground
        }
    }
    return foundEmpty;
}
```

> *Fixes: targets underground ore in tunnels (Bug 13)*

---

## 7. `Vein.GetNumberOfReachableMinables` — Add bounds check

This method counts every active near-surface ore in the entire vein with no spatial filter. Since veins can span far beyond AIMeE's search area, `MineablesInVicinity` massively overcounts.

**Replace the method:**

```csharp
public int GetNumberOfReachableMinables(int maxDepth, BoundsInt searchBounds)
{
    int count = 0;
    for (int i = 0; i < this._minables.Length; i++)
    {
        if (this._minables[i].IsActive)
        {
            Vector3Int worldPos = this._minables[i].WorldPositionInt(this.VeinWorldPosition);
            if (searchBounds.Contains(worldPos) && this.IsNearSurface(worldPos, maxDepth))
                count++;
        }
    }
    return count;
}
```

**Update the caller in `VoxelTerrain.GetNumberOfMinablesNearSurface`** to pass bounds:

```csharp
public static int GetNumberOfMinablesNearSurface(Vector3 worldPosition, float searchArea, int maxDepth)
{
    Vector3Int pos = worldPosition.FloorToInt();
    BoundsInt searchBounds = new BoundsInt(
        pos - new Vector3Int((int)searchArea, (int)searchArea, (int)searchArea),
        new Vector3Int((int)searchArea * 2, (int)searchArea * 2, (int)searchArea * 2));

    List<Vein> veins = new List<Vein>();
    Vein.GetVeinsInBounds(searchBounds, ref veins);

    int total = 0;
    for (int i = 0; i < veins.Count; i++)
        total += veins[i].GetNumberOfReachableMinables(maxDepth, searchBounds);
    return total;
}
```

> *Fixes: MineablesInVicinity overcounts (Bug 7)*

---

## 8. `RobotMining.GetLogicValue` — Client-side queue estimate

`MineablesInQueue` returns `_minableDataQueue.Count` which is always 0 on clients because the queue is server-only. Clients have terrain/vein data synced locally and can derive an estimate.

**Replace the MineablesInQueue case:**

```csharp
if (logicType == LogicType.MineablesInQueue)
{
    if (this.HasAuthority)
        return (double)this._minableDataQueue.Count;

    // Client estimate from locally-synced terrain data
    int estimate = VoxelTerrain.GetNumberOfMinablesNearSurface(
        base.ThingTransformPosition,
        (float)MinableSearchArea,
        maxMiningDepth);
    return (double)Mathf.Min(estimate, 5);  // cap to match server queue limit
}
```

> *Fixes: MineablesInQueue always 0 on client (Bug 6)*

---

## 9. Client tread animation — Estimate `WheelRpm` from synced velocity

On clients, `WheelCollider` is disabled → `GetGroundHit()` fails → `WheelRpm` stays 0 → tread UV scrolling is frozen. `VelocityMagnitude` IS synced via `PhysicsUpdateMessage`.

**In `WheeledBase.PhysicsUpdate`, add this before the wheel loop (non-authority path):**

```csharp
if (!this.HasAuthority)
{
    float speed = base.VelocityMagnitude;
    float dir = Vector3.Dot(this.RigidBody.linearVelocity, this.ThingTransform.forward) >= 0f ? 1f : -1f;
    for (int i = 0; i < this.Wheels.Count; i++)
    {
        float radius = this.Wheels[i].WheelCollider != null
            ? this.Wheels[i].WheelCollider.radius : 0.15f;
        this.Wheels[i].WheelRpm = dir * speed * 60f / (2f * Mathf.PI * radius);
    }
}
```

> *Fixes: frozen tread animation on multiplayer clients (Bug 3)*

---

## 10. `RobotMining` — Clear queue on pickup, power-on, and TargetXYZ write

`_minableDataQueue` is only refilled when it empties naturally. It is never cleared when the AIMeE is picked up, turned on after being repositioned, or when TargetX/Y/Z is set via IC. Stale entries cause AIMeE to drive back to old locations, and lingering `AllAimeeQueuedMinables` hash entries block other AIMeEs from claiming that ore.

**Add state-tracking fields to `RobotMining`:**

```csharp
private bool _wasCursor;
private bool _wasOn;
```

**Add to the top of `UpdateEachFrame` (after the power/drag guard):**

```csharp
// Clear stale queue on pickup or power-on
bool isCursor = this.IsCursor;
bool isOn = this.OnOff;

if (isCursor && !this._wasCursor)
    this.ClearMinableQueue();   // picked up — location changed
else if (isOn && !this._wasOn)
    this.ClearMinableQueue();   // turned on — may have been repositioned while off

this._wasCursor = isCursor;
this._wasOn = isOn;
```

**Add to `SetLogicValue`, after writing TargetX/Y/Z:**

```csharp
public override void SetLogicValue(LogicType logicType, double value)
{
    base.SetLogicValue(logicType, value);
    // ... existing target assignment code ...

    // Clear stale queue when target destination changes
    if (logicType == LogicType.TargetX || logicType == LogicType.TargetY ||
        logicType == LogicType.TargetZ)
    {
        this.ClearMinableQueue();
    }
}
```

> *Fixes: stale queue after pickup/power-on/target change (Bug 14)*

---

## Summary: 7 Changes → 14 Bugs Fixed

| Change | File | Bugs Fixed |
|--------|------|------------|
| `UpdateEachFrame` power/drag guard | RobotMining.cs | 2, 5 |
| `Unstuck`/`TryUnstuck` physics rewrite | RobotMining.cs | 9, 11 |
| `Roam` drive-always + sort + cap + density sync | RobotMining.cs | 4, 10, 11, 12 |
| `OnMinedOre`/`OnChildEnterInventory` cancel token | RobotMining.cs | 1 |
| `PhysicsUpdate` drag guard | WheeledBase.cs | 8, (5) |
| `IsNearSurface` column scan | Vein.cs | 13 |
| `GetNumberOfReachableMinables` bounds check | Vein.cs + VoxelTerrain.cs | 7 |
| `GetLogicValue` client estimate | RobotMining.cs | 6 |
| Client `WheelRpm` estimation | WheeledBase.cs | 3 |
| Queue clear on pickup/power-on/TargetXYZ | RobotMining.cs | 14 |
