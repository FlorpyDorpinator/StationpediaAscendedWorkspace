using System;
using System.Collections.Generic;
using System.Reflection;
using Assets.Scripts;
using Assets.Scripts.Networking;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.Vehicles;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TerrainSystem;
using UnityEngine;
using Util;
using Assets.Scripts.Util;

namespace AimeeBugFixes
{
    [BepInPlugin("com.florpydorp.aimeebugfixes", "AIMeE Bug Fixes", "0.4.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        private Harmony _harmony;

        private static readonly Dictionary<string, float> _lastLogTimes = new Dictionary<string, float>();
        internal static bool ShouldLog(string key, float intervalSeconds = 10f)
        {
            float now = Time.time;
            if (!_lastLogTimes.TryGetValue(key, out float lastTime) || (now - lastTime) >= intervalSeconds)
            {
                _lastLogTimes[key] = now;
                return true;
            }
            return false;
        }

        void Awake()
        {
            Log = Logger;
            Log.LogInfo("AIMeE Bug Fixes v0.4.0 loading...");
            Log.LogInfo(string.Format("  IsBatchMode={0}, RunSimulation={1}", GameManager.IsBatchMode, GameManager.RunSimulation));

            _harmony = new Harmony("com.florpydorp.aimeebugfixes");

            int success = 0;
            int fail = 0;

            // Bug 1: Mine deadlock fix
            TryPatch(ref success, ref fail, "Bug1-OnMinedOre",
                AccessTools.Method(typeof(RobotMining), "OnMinedOre"),
                postfix: new HarmonyMethod(typeof(Bug1_MineDeadlockFix), nameof(Bug1_MineDeadlockFix.OnMinedOre_Postfix)));

            TryPatch(ref success, ref fail, "Bug1-OnChildEnterInventory",
                AccessTools.Method(typeof(RobotMining), "OnChildEnterInventory"),
                postfix: new HarmonyMethod(typeof(Bug1_MineDeadlockFix), nameof(Bug1_MineDeadlockFix.OnChildEnterInventory_Postfix)));

            TryPatch(ref success, ref fail, "Bug1-MovingToMineable",
                AccessTools.Method(typeof(RobotMining), "MovingToMineable"),
                prefix: new HarmonyMethod(typeof(Bug1_MineDeadlockFix), nameof(Bug1_MineDeadlockFix.MovingToMineable_Prefix)));

            // Bug 2: Off-state teleport/hop fix (also supports Bug 5 drag blocking)
            TryPatch(ref success, ref fail, "Bug2-TryUnstuck",
                AccessTools.Method(typeof(RobotMining), "TryUnstuck"),
                prefix: new HarmonyMethod(typeof(Bug2_OffStateTeleportFix), nameof(Bug2_OffStateTeleportFix.TryUnstuck_Prefix)));

            TryPatch(ref success, ref fail, "Bug2-PathToTarget",
                AccessTools.Method(typeof(RobotMining), "PathToTarget"),
                prefix: new HarmonyMethod(typeof(Bug2_OffStateTeleportFix), nameof(Bug2_OffStateTeleportFix.PathToTarget_Prefix)));

            TryPatch(ref success, ref fail, "Bug2+5-UpdateEachFrame",
                AccessTools.Method(typeof(RobotMining), "UpdateEachFrame"),
                postfix: new HarmonyMethod(typeof(Bug2and5_UpdateEachFramePostfix), nameof(Bug2and5_UpdateEachFramePostfix.Postfix)));

            // Bug 3: Tread/wheel animation sync on multiplayer clients
            TryPatch(ref success, ref fail, "Bug3-TreadUV",
                AccessTools.Method(typeof(WheeledBase), "PhysicsUpdate"),
                prefix: new HarmonyMethod(typeof(Bug3_WheelAnimationSyncFix), nameof(Bug3_WheelAnimationSyncFix.PhysicsUpdate_Prefix)));

            TryPatch(ref success, ref fail, "Bug3-ServerOccluded",
                AccessTools.Method(typeof(WheeledBase), "PhysicsUpdate"),
                postfix: new HarmonyMethod(typeof(Bug3_WheelAnimationSyncFix), nameof(Bug3_WheelAnimationSyncFix.PhysicsUpdate_Postfix)));

            // Bug 4: Roam overhaul — replaces Roam() entirely with hybrid old/new flow
            // Old code: search+move+mine+drive all in one flow, AIMeE always drives
            // New code: queue-based with early-return on empty queue (AIMeE stops dead)
            // Fix: keep queue system + always-drive + cap queue for multi-AIMeE fairness
            TryPatch(ref success, ref fail, "Bug4-Roam",
                AccessTools.Method(typeof(RobotMining), "Roam"),
                prefix: new HarmonyMethod(typeof(Bug4_RoamFix), nameof(Bug4_RoamFix.Roam_Prefix)));

            // Bug 6: MineablesInQueue always 0 on client (not synced)
            // REMOVED in v0.3.0 — returning terrain count as queue proxy made queue==vicinity,
            // which is wrong and confusing. IC10 on server gets real queue value. Client can't know.
            // TryPatch(ref success, ref fail, "Bug6-QueueSync",
            //     AccessTools.Method(typeof(RobotMining), "GetLogicValue", new Type[] { typeof(LogicType) }),
            //     postfix: new HarmonyMethod(typeof(Bug6_QueueClientSync), nameof(Bug6_QueueClientSync.GetLogicValue_Postfix)));
            Log.LogInfo("  [Bug6] REMOVED — fake queue proxy was harmful");

            // Bug 7: MineablesInVicinity overcounts (no per-ore bounds check)
            TryPatch(ref success, ref fail, "Bug7-VicinityBounds",
                AccessTools.Method(typeof(VoxelTerrain), "GetNumberOfMinablesNearSurface"),
                prefix: new HarmonyMethod(typeof(Bug7_VicinityBoundsFix), nameof(Bug7_VicinityBoundsFix.GetNumberOfMinablesNearSurface_Prefix)));

            // Restore original ore scan radius (was 32, halved to 16 in Orbital Update)
            try
            {
                var searchAreaField = AccessTools.Field(typeof(RobotMining), "MinableSearchArea");
                if (searchAreaField != null)
                {
                    int currentVal = (int)searchAreaField.GetValue(null);
                    Log.LogInfo(string.Format("  MinableSearchArea current={0}", currentVal));
                    if (currentVal != 32)
                    {
                        searchAreaField.SetValue(null, 32);
                        Log.LogInfo("  MinableSearchArea restored to 32");
                    }
                }
                else
                {
                    Log.LogWarning("  MinableSearchArea field not found");
                }
            }
            catch (Exception ex)
            {
                Log.LogWarning(string.Format("  Could not check MinableSearchArea: {0}", ex.Message));
            }

            Log.LogInfo(string.Format("AIMeE Bug Fixes: {0} patches applied, {1} failed", success, fail));
            if (fail > 0)
                Log.LogError("Some patches failed! Check errors above.");
        }

        private void TryPatch(ref int success, ref int fail, string name,
            MethodInfo original, HarmonyMethod prefix = null, HarmonyMethod postfix = null)
        {
            try
            {
                if (original == null)
                {
                    Log.LogError(string.Format("  [{0}] FAILED - target method not found via reflection", name));
                    fail++;
                    return;
                }
                _harmony.Patch(original, prefix: prefix, postfix: postfix);
                Log.LogInfo(string.Format("  [{0}] OK - patched {1}.{2}", name, original.DeclaringType.Name, original.Name));
                success++;
            }
            catch (Exception ex)
            {
                Log.LogError(string.Format("  [{0}] FAILED - {1}: {2}", name, ex.GetType().Name, ex.Message));
                fail++;
            }
        }

        void OnDestroy()
        {
            Log.LogInfo("AIMeE Bug Fixes: plugin component destroyed (patches remain active)");
        }
    }

    // =========================================================================
    // BUG 1: AIMeE stops mining after first ore (mine deadlock)
    // =========================================================================
    public static class Bug1_MineDeadlockFix
    {
        private static readonly FieldInfo MineCancellationField =
            AccessTools.Field(typeof(RobotMining), "_mineCancellation");

        private static readonly Dictionary<int, float> MineStartTimes =
            new Dictionary<int, float>();

        private const float MineTimeoutSeconds = 30f;

        public static void OnMinedOre_Postfix(RobotMining __instance)
        {
            try
            {
                var wrapper = (CancellationTokenWrapper)MineCancellationField.GetValue(__instance);
                if (wrapper != null && wrapper.Initialized)
                {
                    wrapper.Cancel();
                    MineStartTimes.Remove(__instance.GetInstanceID());
                    if (Plugin.ShouldLog("Bug1-Mined", 5f))
                        Plugin.Log.LogInfo("[Bug1] OnMinedOre: RESET mining token");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(string.Format("[Bug1] OnMinedOre error: {0}", ex.Message));
            }
        }

        public static void OnChildEnterInventory_Postfix(RobotMining __instance, DynamicThing newChild)
        {
            try
            {
                if (newChild is ProgrammableChip)
                {
                    var wrapper = (CancellationTokenWrapper)MineCancellationField.GetValue(__instance);
                    if (wrapper != null) wrapper.Cancel();
                    __instance.ClearMinableQueue();
                    MineStartTimes.Remove(__instance.GetInstanceID());
                    Plugin.Log.LogInfo("[Bug1] Full mining state reset on chip change");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(string.Format("[Bug1] OnChildEnterInventory error: {0}", ex.Message));
            }
        }

        public static void MovingToMineable_Prefix(RobotMining __instance)
        {
            try
            {
                var wrapper = (CancellationTokenWrapper)MineCancellationField.GetValue(__instance);
                if (wrapper == null) return;

                int id = __instance.GetInstanceID();

                if (wrapper.Initialized)
                {
                    if (!MineStartTimes.ContainsKey(id))
                    {
                        MineStartTimes[id] = Time.time;
                    }
                    else if (Time.time - MineStartTimes[id] > MineTimeoutSeconds)
                    {
                        wrapper.Cancel();
                        MineStartTimes.Remove(id);
                        Plugin.Log.LogWarning(string.Format("[Bug1] FORCE RESET after {0}s timeout", MineTimeoutSeconds));
                    }
                }
                else
                {
                    MineStartTimes.Remove(id);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(string.Format("[Bug1] MovingToMineable error: {0}", ex.Message));
            }
        }
    }

    // =========================================================================
    // BUG 2: Off-state hopping/teleporting when receiving logic
    // Also blocks TryUnstuck/PathToTarget while being dragged (Bug 5).
    // =========================================================================
    public static class Bug2_OffStateTeleportFix
    {
        public static bool TryUnstuck_Prefix(RobotMining __instance)
        {
            if (__instance.Joint != null || __instance.IsChild)
            {
                if (Plugin.ShouldLog(string.Format("Bug5-Unstuck-{0}", __instance.GetInstanceID()), 10f))
                    Plugin.Log.LogInfo("[Bug5] TryUnstuck: BLOCKED (being dragged)");
                return false;
            }
            if (!__instance.OnOff || !__instance.Powered)
            {
                if (Plugin.ShouldLog(string.Format("Bug2-Unstuck-{0}", __instance.GetInstanceID()), 10f))
                    Plugin.Log.LogInfo("[Bug2] TryUnstuck: BLOCKED (off/unpowered)");
                return false;
            }
            return true;
        }

        public static bool PathToTarget_Prefix(RobotMining __instance)
        {
            if (__instance.Joint != null || __instance.IsChild)
            {
                if (Plugin.ShouldLog(string.Format("Bug5-Path-{0}", __instance.GetInstanceID()), 10f))
                    Plugin.Log.LogInfo("[Bug5] PathToTarget: BLOCKED (being dragged)");
                return false;
            }
            if (!__instance.OnOff || !__instance.Powered)
            {
                if (Plugin.ShouldLog(string.Format("Bug2-Path-{0}", __instance.GetInstanceID()), 10f))
                    Plugin.Log.LogInfo("[Bug2] PathToTarget: BLOCKED (off/unpowered)");
                return false;
            }
            return true;
        }
    }

    // =========================================================================
    // BUG 2 + BUG 5 combined UpdateEachFrame postfix
    // =========================================================================
    public static class Bug2and5_UpdateEachFramePostfix
    {
        public static void Postfix(RobotMining __instance)
        {
            if (!GameManager.RunSimulation) return;

            // Bug 5: Dragged - zero everything so joint moves AIMeE freely
            if (__instance.Joint != null || __instance.IsChild)
            {
                __instance.TargetMotorPower = 0f;
                __instance.TargetBrakePower = 0f;
                __instance.TargetSteeringAngle = 0f;
                return;
            }

            // Bug 2: Off - enforce braking
            if (!__instance.OnOff || !__instance.Powered)
            {
                __instance.TargetBrakePower = __instance.Power;
                __instance.TargetMotorPower = 0f;
            }
        }
    }

    // =========================================================================
    // BUG 3: Tread/wheel UV animation broken on multiplayer clients
    // =========================================================================
    public static class Bug3_WheelAnimationSyncFix
    {
        private static readonly Dictionary<int, float> _smoothVelocity = new Dictionary<int, float>();
        private const float RpmSmoothTime = 0.1f;

        public static void PhysicsUpdate_Prefix(WheeledBase __instance)
        {
            if (__instance.HasAuthority || __instance.IsOccluded) return;

            try
            {
                Vector3 rbVel = __instance.RigidBody.velocity;
                float velocity = rbVel.magnitude;

                if (velocity > 0.15f)
                {
                    float forwardDot = Vector3.Dot(rbVel.normalized, __instance.ThingTransform.forward);
                    float sign = forwardDot >= 0f ? 1f : -1f;

                    for (int i = 0; i < __instance.Wheels.Count; i++)
                    {
                        var wheel = __instance.Wheels[i];
                        float radius = 0.3f;
                        if (wheel.WheelCollider != null)
                        {
                            float r = wheel.WheelCollider.radius;
                            if (r > 0.01f) radius = r;
                        }
                        float targetRpm = sign * Mathf.Clamp(velocity * 60f / (2f * Mathf.PI * radius), -100f, 100f);

                        int key = wheel.GetHashCode();
                        if (!_smoothVelocity.TryGetValue(key, out float sv)) sv = 0f;
                        wheel.WheelRpm = Mathf.SmoothDamp(wheel.WheelRpm, targetRpm, ref sv, RpmSmoothTime);
                        _smoothVelocity[key] = sv;
                    }
                }
                else
                {
                    for (int i = 0; i < __instance.Wheels.Count; i++)
                    {
                        var wheel = __instance.Wheels[i];
                        int key = wheel.GetHashCode();
                        if (!_smoothVelocity.TryGetValue(key, out float sv)) sv = 0f;
                        wheel.WheelRpm = Mathf.SmoothDamp(wheel.WheelRpm, 0f, ref sv, RpmSmoothTime);
                        _smoothVelocity[key] = sv;
                        if (Mathf.Abs(wheel.WheelRpm) < 1f)
                            wheel.WheelRpm = 0f;
                    }
                }
            }
            catch (Exception ex)
            {
                if (Plugin.ShouldLog("Bug3-UV-err", 30f))
                    Plugin.Log.LogError(string.Format("[Bug3] TreadUV prefix error: {0}", ex.Message));
            }
        }

        private static int _forceCount = 0;

        public static void PhysicsUpdate_Postfix(WheeledBase __instance)
        {
            if (!__instance.HasAuthority || !__instance.IsOccluded)
                return;

            _forceCount++;
            for (int i = 0; i < __instance.Wheels.Count; i++)
            {
                try { __instance.Wheels[i].AnimateAuthority(); }
                catch (Exception ex) { Plugin.Log.LogError(string.Format("[Bug3] AnimateAuthority[{0}] error: {1}", i, ex.Message)); }
            }
        }
    }

    // =========================================================================
    // BUG 4: Roam() overhaul — hybrid old/new flow
    //
    // Problem: Current game Roam() has a queue system that hard-returns when
    // the queue is empty, so AIMeE stops dead instead of exploring.
    // Also: AllAimeeQueuedMinables causes multi-AIMeE starvation (first to
    // fill claims ALL ore, others get nothing).
    //
    // Fix: Full Roam() replacement prefix that:
    //  1. Keeps the queue system for efficient multi-target mining
    //  2. NEVER early-returns on empty queue — always drives (like old code)
    //  3. Caps queue to 10 per AIMeE so multiple bots share ore fairly
    //  4. Uses exact game motor/brake formulas (RocketMath.MapToScale)
    //  5. Calls original MovingToMineable() for mining state machine
    // =========================================================================
    public static class Bug4_RoamFix
    {
        private static readonly FieldInfo QueueField =
            AccessTools.Field(typeof(RobotMining), "_minableDataQueue");
        private static readonly FieldInfo ReversingField =
            AccessTools.Field(typeof(RobotMining), "_reversing");
        private static readonly FieldInfo RoamTimeoutField =
            AccessTools.Field(typeof(RobotMining), "_roamTimeout");
        private static readonly FieldInfo DeltaField =
            AccessTools.Field(typeof(RobotMining), "Delta");
        private static readonly FieldInfo ScanTimeoutField =
            AccessTools.Field(typeof(RobotMining), "_minableScanTimeout");
        private static readonly FieldInfo TargetMinableField =
            AccessTools.Field(typeof(RobotMining), "TargetMinable");
        private static readonly FieldInfo SearchAreaField =
            AccessTools.Field(typeof(RobotMining), "MinableSearchArea");
        private static readonly FieldInfo MaxDepthField =
            AccessTools.Field(typeof(RobotMining), "maxMiningDepth");
        private static readonly MethodInfo MovingToMineableMethod =
            AccessTools.Method(typeof(RobotMining), "MovingToMineable");

        // Cap per-AIMeE queue to prevent one bot monopolizing all ore
        private const int MaxQueuePerAimee = 10;

        public static bool Roam_Prefix(RobotMining __instance)
        {
            // Let original handle StorageFull (needs protected InteractMode)
            if (__instance.IsStorageFull)
                return true;

            try
            {
                // --- Queue management ---
                var queue = (List<TargetMinableData>)QueueField.GetValue(__instance);
                int searchArea = (int)SearchAreaField.GetValue(null);
                int maxDepth = (int)MaxDepthField.GetValue(null);

                if (queue.Count <= 0)
                {
                    __instance.ClearMinableQueue();
                    VoxelTerrain.GetAimeeMinableQueue(
                        __instance.ThingTransformPosition,
                        (float)searchArea,
                        queue,
                        maxDepth);

                    // Cap queue: remove excess from front (keep tail = closest)
                    while (queue.Count > MaxQueuePerAimee)
                    {
                        var excess = queue[0];
                        excess.Vein.RemoveAimeeMinableHash(excess.MinableIndex);
                        queue.RemoveAt(0);
                    }
                }

                bool hasTarget = queue.Count > 0;

                // Set target from queue tail (or clear if empty)
                if (hasTarget)
                {
                    TargetMinableField.SetValue(__instance,
                        new TargetMinableData?(queue[queue.Count - 1]));
                }
                // If queue empty: DON'T return — fall through to driving code

                // --- Mining state machine (only when we have ore to mine) ---
                if (hasTarget)
                {
                    bool movingOrMining = (bool)MovingToMineableMethod.Invoke(__instance, null);
                    if (movingOrMining)
                        return false;

                    // Scan timeout countdown
                    if (ScanTimeoutField != null)
                    {
                        float scanTimeout = (float)ScanTimeoutField.GetValue(__instance);
                        if (scanTimeout > 0f)
                        {
                            scanTimeout -= Time.deltaTime;
                            ScanTimeoutField.SetValue(__instance, scanTimeout);
                        }
                    }

                    // Direct mine attempt at current position
                    var targetVal = (TargetMinableData?)TargetMinableField.GetValue(__instance);
                    if (targetVal != null)
                    {
                        Vector3 minePos = targetVal.Value.Vein.GetMinableWorldPosition(
                            targetVal.Value.MinableIndex);
                        Vector3Int minePosInt = new Vector3Int(
                            Mathf.FloorToInt(minePos.x),
                            Mathf.FloorToInt(minePos.y),
                            Mathf.FloorToInt(minePos.z));

                        Ore oreMined;
                        if (targetVal.Value.Vein.TryMineServer(
                            minePosInt, out oreMined, __instance.ThingTransformPosition))
                        {
                            __instance.OnMinedOre(oreMined);
                            targetVal.Value.Vein.RemoveAimeeMinableHash(
                                targetVal.Value.MinableIndex);
                            queue.RemoveAt(queue.Count - 1);
                            TargetMinableField.SetValue(__instance, null);
                            return false; // mined — come back next frame
                        }
                    }
                }

                // --- ALWAYS drive (exact game formulas from Roam) ---
                float power = __instance.Power;
                float velocity = __instance.VelocityMagnitude;
                Vector3 worldCoM = __instance.RigidBody.worldCenterOfMass;

                float reversing = (float)ReversingField.GetValue(__instance);
                if (reversing <= 0f)
                {
                    RaycastHit hit;
                    int mask = ~0;
                    try { mask = CursorManager.Instance.TerrainHitMask; } catch { }
                    if (Physics.Raycast(worldCoM, __instance.ThingTransform.forward,
                        out hit, 0.2f, mask))
                    {
                        reversing = 5f;
                    }
                }

                if (reversing > 0f)
                {
                    __instance.TargetMotorPower = -RocketMath.MapToScale(
                        0f, __instance.MaxSpeed, power, 0f, velocity);
                    reversing -= Time.deltaTime;
                    ReversingField.SetValue(__instance, reversing);
                    return false;
                }
                ReversingField.SetValue(__instance, reversing);

                float roamTimeout = (float)RoamTimeoutField.GetValue(__instance);
                if (roamTimeout <= 0f)
                {
                    roamTimeout = UnityEngine.Random.Range(1f, 3f);
                    float delta = UnityEngine.Random.Range(-15f, 15f);
                    DeltaField.SetValue(__instance, delta);
                    __instance.TargetSteeringAngle = delta;
                }
                roamTimeout -= Time.deltaTime;
                RoamTimeoutField.SetValue(__instance, roamTimeout);

                __instance.TargetMotorPower = RocketMath.MapToScale(
                    0f, __instance.MaxSpeed, power, 0f, velocity);
                __instance.TargetBrakePower = velocity > __instance.MaxSpeed
                    ? RocketMath.MapToScale(
                        __instance.MaxSpeed, __instance.MaxSpeed * 1.3f,
                        0f, __instance.Power, velocity)
                    : 0f;

                // Diagnostics
                if (Plugin.ShouldLog(string.Format("Bug4-Diag-{0}", __instance.GetInstanceID()), 10f))
                {
                    Plugin.Log.LogInfo(string.Format(
                        "[Bug4] Roam: queue={0}, globalHash={1}, vel={2:F2}, motor={3:F4}, steer={4:F1}, pos=({5:F0},{6:F0},{7:F0})",
                        queue.Count, Vein.AllAimeeQueuedMinables.Count,
                        velocity, __instance.TargetMotorPower, __instance.TargetSteeringAngle,
                        __instance.ThingTransformPosition.x, __instance.ThingTransformPosition.y,
                        __instance.ThingTransformPosition.z));
                }

                return false; // skip original
            }
            catch (Exception ex)
            {
                if (Plugin.ShouldLog("Bug4-err", 30f))
                    Plugin.Log.LogError(string.Format("[Bug4] Roam error: {0}", ex.Message));
                return true; // fall through to original on error
            }
        }
    }

    // =========================================================================
    // BUG 6: MineablesInQueue always 0 on multiplayer clients
    //
    // Root cause: _minableDataQueue is a private local List that is only
    // populated by Roam() on the server (authority). It is NOT a networked
    // property. On clients, it stays empty forever, so GetLogicValue returns 0.
    //
    // Fix: On non-authority clients, when MineablesInQueue is requested,
    // instead of reading the empty local queue, query the terrain directly
    // using GetNumberOfMinablesNearSurface (which uses synced voxel/vein data).
    // This gives the count of surface-reachable ore nearby — not the exact
    // server queue, but meaningful and non-zero when ore exists.
    // =========================================================================
    public static class Bug6_QueueClientSync
    {
        private static readonly FieldInfo SearchAreaField =
            AccessTools.Field(typeof(RobotMining), "MinableSearchArea");
        private static readonly FieldInfo MaxDepthField =
            AccessTools.Field(typeof(RobotMining), "maxMiningDepth");

        public static void GetLogicValue_Postfix(RobotMining __instance, LogicType logicType, ref double __result)
        {
            // Only act on clients (non-authority) for MineablesInQueue
            if (logicType != LogicType.MineablesInQueue) return;
            if (__instance.HasAuthority) return;

            try
            {
                int searchArea = 32;
                int maxDepth = 3;
                try
                {
                    if (SearchAreaField != null) searchArea = (int)SearchAreaField.GetValue(null);
                    if (MaxDepthField != null) maxDepth = (int)MaxDepthField.GetValue(null);
                }
                catch { }

                int count = VoxelTerrain.GetNumberOfMinablesNearSurface(
                    __instance.ThingTransformPosition, (float)searchArea, maxDepth);
                __result = (double)count;
            }
            catch { }
        }
    }

    // =========================================================================
    // BUG 7: MineablesInVicinity overcounts — no per-ore bounds check
    //
    // Root cause: GetNumberOfMinablesNearSurface calls GetVeinsInBounds (which
    // uses 32-block chunk lookup), then for each vein calls
    // GetNumberOfReachableMinables — which counts ALL active near-surface ore
    // in the entire vein with NO position filter. Veins can extend well beyond
    // the search area, so ore 50+ blocks away gets counted.
    //
    // Compare to GetAimeeMinableQueue which correctly checks
    // searchBounds.Contains(position) for each individual ore block.
    //
    // Fix: Replace with a bounds-checked version that mirrors GetAimeeMinables
    // logic but only counts instead of building a queue.
    // =========================================================================
    public static class Bug7_VicinityBoundsFix
    {
        private static readonly FieldInfo MinablesField =
            AccessTools.Field(typeof(Vein), "_minables");

        private const float DensityThreshold = 0.49803922f;

        public static bool GetNumberOfMinablesNearSurface_Prefix(
            Vector3 position, float boundsSize, int maxDepthFromSurface, ref int __result)
        {
            try
            {
                // Compute search bounds — same math as the private GetVeinsInBounds overload
                float half = boundsSize * 0.5f;
                BoundsInt searchBounds = new BoundsInt(
                    (int)Math.Floor(position.x - half),
                    (int)Math.Floor(position.y - half),
                    (int)Math.Floor(position.z - half),
                    (int)boundsSize + 1,
                    (int)boundsSize + 1,
                    (int)boundsSize + 1);

                // Get veins using the public API
                List<Vein> veins = new List<Vein>();
                Vein.GetVeinsInBounds(searchBounds, ref veins);

                int count = 0;
                int totalMinables = 0;
                int activeCount = 0;
                int inBoundsCount = 0;
                for (int v = 0; v < veins.Count; v++)
                {
                    Vein vein = veins[v];
                    Minable[] minables = (Minable[])MinablesField.GetValue(vein);
                    if (minables == null) continue;

                    totalMinables += minables.Length;
                    Vector3Int veinPos = vein.VeinWorldPosition;
                    for (int i = 0; i < minables.Length; i++)
                    {
                        if (!minables[i].IsActive) continue;
                        activeCount++;

                        Vector3Int worldPos = minables[i].WorldPositionInt(veinPos);

                        // Bounds check — this is what the original is missing
                        if (!searchBounds.Contains(worldPos)) continue;
                        inBoundsCount++;

                        // Near-surface check — same as IsNearSurface
                        if (VoxelTerrain.GetDensityWorldSpace(worldPos + Vector3Int.up * maxDepthFromSurface) < DensityThreshold)
                        {
                            count++;
                        }
                    }
                }

                if (Plugin.ShouldLog("Bug7-Diag", 10f))
                {
                    Plugin.Log.LogInfo(string.Format(
                        "[Bug7] Vicinity diag: veins={0}, totalMinables={1}, active={2}, inBounds={3}, nearSurface={4}, pos=({5:F0},{6:F0},{7:F0}), bounds={8}, depth={9}",
                        veins.Count, totalMinables, activeCount, inBoundsCount, count,
                        position.x, position.y, position.z,
                        boundsSize, maxDepthFromSurface));
                }

                __result = count;
            }
            catch (Exception ex)
            {
                // On failure, fall through to original method
                if (Plugin.ShouldLog("Bug7-err", 30f))
                    Plugin.Log.LogError(string.Format("[Bug7] Vicinity bounds fix error: {0}", ex.Message));
                return true;
            }
            return false; // skip original
        }
    }
}
