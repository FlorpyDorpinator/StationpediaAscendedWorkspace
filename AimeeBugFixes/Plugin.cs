using System;
using System.Collections.Generic;
using System.Linq;
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
using Util.Commands;
using Assets.Scripts.Util;

namespace AimeeBugFixes
{
    [BepInPlugin("com.florpydorp.aimeebugfixes", "AIMeE Bug Fixes", "0.15.3")]
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
            Log.LogInfo("AIMeE Bug Fixes v0.15.3 loading...");
            Log.LogInfo(string.Format("  IsBatchMode={0}, RunSimulation={1}, IsDedicated={2}", GameManager.IsBatchMode, GameManager.RunSimulation, GameManager.IsBatchMode));

            _harmony = new Harmony("com.florpydorp.aimeebugfixes");

            // Register "aimeedebug" console command (force-replace if already exists from previous load)
            try
            {
                var cmdMap = AccessTools.Field(typeof(CommandLine), "_commandsMap")
                    ?.GetValue(null) as System.Collections.Generic.SortedDictionary<string, CommandBase>;
                if (cmdMap != null)
                {
                    cmdMap["aimeedebug"] = new BasicCommand(
                        AimeeDebugOverlay.HandleCommand,
                        "Toggle AIMeE debug overlay. Usage: aimeedebug [on|off]",
                        new string[] { "on/off (optional)" },
                        false);
                    Log.LogInfo("  [Debug] Registered 'aimeedebug' console command");
                }
                else
                {
                    Log.LogWarning("  [Debug] Could not access CommandLine._commandsMap");
                }
            }
            catch (Exception ex)
            {
                Log.LogWarning(string.Format("  [Debug] Could not register console command: {0}", ex.Message));
            }

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
            // Re-enabled with safer fallback: use vicinity query but cap to queue max.
            // This remains an estimate on non-authority clients, but avoids misleading hard-zero.
            TryPatch(ref success, ref fail, "Bug6-QueueSync",
                AccessTools.Method(typeof(RobotMining), "GetLogicValue", new Type[] { typeof(LogicType) }),
                postfix: new HarmonyMethod(typeof(Bug6_QueueClientSync), nameof(Bug6_QueueClientSync.GetLogicValue_Postfix)));
            Log.LogInfo("  [Bug6] ENABLED — non-authority queue estimate with cap");

            // Bug 7: MineablesInVicinity overcounts (no per-ore bounds check)
            TryPatch(ref success, ref fail, "Bug7-VicinityBounds",
                AccessTools.Method(typeof(VoxelTerrain), "GetNumberOfMinablesNearSurface"),
                prefix: new HarmonyMethod(typeof(Bug7_VicinityBoundsFix), nameof(Bug7_VicinityBoundsFix.GetNumberOfMinablesNearSurface_Prefix)));

            // Bug 8: Disable WheelColliders when carried to prevent collision launches
            TryPatch(ref success, ref fail, "Bug8-CarryCollision",
                AccessTools.Method(typeof(WheeledBase), "PhysicsUpdate"),
                prefix: new HarmonyMethod(typeof(Bug8_CarryCollisionFix), nameof(Bug8_CarryCollisionFix.PhysicsUpdate_Prefix)));

            // Bug 9: Replace Unstuck() teleport with physics-based escape
            TryPatch(ref success, ref fail, "Bug9-AntiTeleport",
                AccessTools.Method(typeof(RobotMining), "Unstuck"),
                prefix: new HarmonyMethod(typeof(Bug9_AntiTeleportFix), nameof(Bug9_AntiTeleportFix.Unstuck_Prefix)));

            // Bug 12: Ghost ore — AIMeE mines ore server-side via TryMineServer but never
            // syncs to clients. Fix: two-event SyncList injection. Event 1 (density 0)
            // triggers MineAtPositionClient on client to remove ore. Event 2 (original
            // density) restores terrain. Net result: ore gone, terrain intact.
            TryPatch(ref success, ref fail, "Bug12-GhostOre",
                AccessTools.Method(typeof(Vein), "TryMineServer"),
                postfix: new HarmonyMethod(typeof(Bug12_GhostOreFix), nameof(Bug12_GhostOreFix.TryMineServer_Postfix)));

            // Bug 13: Underground ore — AIMeE targets ore in tunnels/caves because
            // IsNearSurface only checks a single voxel maxDepth blocks above. In a
            // tunnel, that point is empty so ore passes the check even though it's
            // unreachable from the surface. Fix: replace IsNearSurface with a column
            // scan that detects cave ceilings.
            TryPatch(ref success, ref fail, "Bug13-UndergroundOre",
                AccessTools.Method(typeof(Vein), "IsNearSurface",
                    new Type[] { typeof(Vector3Int), typeof(int) }),
                prefix: new HarmonyMethod(typeof(Bug13_UndergroundOreFix), nameof(Bug13_UndergroundOreFix.IsNearSurface_Prefix)));

            // Bug 14: Stale queue — queue not cleared on pickup/power-on/target-set
            TryPatch(ref success, ref fail, "Bug14-StaleQueue-UpdateEachFrame",
                AccessTools.Method(typeof(RobotMining), "UpdateEachFrame"),
                prefix: new HarmonyMethod(typeof(Bug14_StaleQueueFix), nameof(Bug14_StaleQueueFix.UpdateEachFrame_Prefix)));
            TryPatch(ref success, ref fail, "Bug14-StaleQueue-SetLogicValue",
                AccessTools.Method(typeof(RobotMining), "SetLogicValue", new Type[] { typeof(LogicType), typeof(double) }),
                postfix: new HarmonyMethod(typeof(Bug14_StaleQueueFix), nameof(Bug14_StaleQueueFix.SetLogicValue_Postfix)));

            // Diagnostic: Log every MineablesInVicinity read from IC/game to compare with overlay
            TryPatch(ref success, ref fail, "Diag-GetLogicValue",
                AccessTools.Method(typeof(RobotMining), "GetLogicValue", new Type[] { typeof(LogicType) }),
                postfix: new HarmonyMethod(typeof(DiagGetLogicValuePostfix), nameof(DiagGetLogicValuePostfix.Postfix)));

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

        void Update()
        {
            // Update cached data at low frequency (not every frame)
            if (AimeeDebugOverlay.Enabled)
                AimeeDebugOverlay.UpdateCache();
        }

        void OnGUI()
        {
            AimeeDebugOverlay.DrawOverlayFromCache();
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
                    if (AimeeDebugOverlay.Enabled && Plugin.ShouldLog("Bug1-Mined", 5f))
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
                    if (AimeeDebugOverlay.Enabled)
                        Plugin.Log.LogInfo("[Bug1] Full mining state reset on chip change");
                }
            }
            catch (Exception ex)
            {
                if (AimeeDebugOverlay.Enabled)
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
                        if (AimeeDebugOverlay.Enabled)
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
                if (AimeeDebugOverlay.Enabled)
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
                if (AimeeDebugOverlay.Enabled && Plugin.ShouldLog(string.Format("Bug5-Unstuck-{0}", __instance.GetInstanceID()), 10f))
                    Plugin.Log.LogInfo("[Bug5] TryUnstuck: BLOCKED (being dragged)");
                return false;
            }
            if (!__instance.OnOff || !__instance.Powered)
            {
                if (AimeeDebugOverlay.Enabled && Plugin.ShouldLog(string.Format("Bug2-Unstuck-{0}", __instance.GetInstanceID()), 10f))
                    Plugin.Log.LogInfo("[Bug2] TryUnstuck: BLOCKED (off/unpowered)");
                return false;
            }
            return true;
        }

        public static bool PathToTarget_Prefix(RobotMining __instance)
        {
            if (__instance.Joint != null || __instance.IsChild)
            {
                if (AimeeDebugOverlay.Enabled && Plugin.ShouldLog(string.Format("Bug5-Path-{0}", __instance.GetInstanceID()), 10f))
                    Plugin.Log.LogInfo("[Bug5] PathToTarget: BLOCKED (being dragged)");
                return false;
            }
            if (!__instance.OnOff || !__instance.Powered)
            {
                if (AimeeDebugOverlay.Enabled && Plugin.ShouldLog(string.Format("Bug2-Path-{0}", __instance.GetInstanceID()), 10f))
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
                if (AimeeDebugOverlay.Enabled && Plugin.ShouldLog("Bug3-UV-err", 30f))
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
                catch (Exception ex) { if (AimeeDebugOverlay.Enabled) Plugin.Log.LogError(string.Format("[Bug3] AnimateAuthority[{0}] error: {1}", i, ex.Message)); }
            }
        }
    }

    // =========================================================================
    // BUG 8: AIMeE WheelColliders cause collision launches when carried
    //
    // Problem: When player carries AIMeE (DragInSlot), WheelColliders stay
    // enabled. On collision with terrain/objects, suspension spring forces
    // and friction propagate through the unbreakable joint chain to the
    // player's rigidbody, launching them. Storage crates don't have this
    // issue because they have no WheelColliders.
    //
    // Fix: Prefix on WheeledBase.PhysicsUpdate that disables WheelColliders
    // and skips all wheel force application when being carried. Calls
    // base DynamicThing.PhysicsUpdate via reflection for position tracking.
    // =========================================================================
    public static class Bug8_CarryCollisionFix
    {
        // Delegate to call DynamicThing.PhysicsUpdate directly (not the virtual dispatch)
        private delegate void BasePhysicsUpdateDelegate(DynamicThing instance);
        private static readonly BasePhysicsUpdateDelegate BasePhysicsUpdate;

        static Bug8_CarryCollisionFix()
        {
            // Get the non-virtual function pointer for DynamicThing.PhysicsUpdate
            var method = typeof(DynamicThing).GetMethod("PhysicsUpdate",
                BindingFlags.Public | BindingFlags.Instance);
            if (method != null)
            {
                var dm = new System.Reflection.Emit.DynamicMethod(
                    "CallBaseDynamicThingPhysicsUpdate",
                    typeof(void),
                    new[] { typeof(DynamicThing) },
                    typeof(Bug8_CarryCollisionFix), true);
                var il = dm.GetILGenerator();
                il.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
                il.Emit(System.Reflection.Emit.OpCodes.Call, method); // Call (not Callvirt)
                il.Emit(System.Reflection.Emit.OpCodes.Ret);
                BasePhysicsUpdate = (BasePhysicsUpdateDelegate)dm.CreateDelegate(
                    typeof(BasePhysicsUpdateDelegate));
            }
        }

        public static bool PhysicsUpdate_Prefix(WheeledBase __instance)
        {
            // Only intercept when being carried/dragged
            if (__instance.Joint == null && !__instance.IsChild)
                return true;

            // Disable all WheelColliders — stops suspension spring forces
            // and friction from generating. Body colliders (BoxCollider/
            // MeshCollider) remain active for normal wall/terrain collision.
            for (int i = 0; i < __instance.Wheels.Count; i++)
            {
                var wc = __instance.Wheels[i].WheelCollider;
                if (wc != null && wc.enabled)
                    wc.enabled = false;
            }

            // Zero any residual torque targets so they don't snap back on drop
            __instance.CurrentMotorPower = 0f;
            __instance.CurrentBrakePower = 0f;
            __instance.CurrentSteeringAngle = 0f;

            // Still run base DynamicThing.PhysicsUpdate for position/rotation
            // tracking (needed for joint movement and network sync)
            try
            {
                if (BasePhysicsUpdate != null)
                    BasePhysicsUpdate(__instance);
            }
            catch (Exception ex)
            {
                if (AimeeDebugOverlay.Enabled)
                    Plugin.Log.LogError(string.Format("[Bug8] base.PhysicsUpdate error: {0}", ex.Message));
            }

            return false; // skip WheeledBase.PhysicsUpdate entirely
        }
    }

    // =========================================================================
    // BUG 9: Stuck teleporting — Unstuck() directly writes position
    //
    // Problem: TryUnstuck() fires every 60s when AIMeE moves < 0.1 units.
    // Unstuck() then directly writes ThingTransformPosition to a new point
    // and resets rotation to Quaternion.identity — a visible teleport.
    // Additionally, Random.Range(0, 1) with int params always gives 0,
    // so the XZ offset is always zero — it only snaps Y to safe-point
    // height and resets rotation, but this is still jarring.
    // Happens with two AIMeEs pushing against each other (both stuck for
    // 60s) and when wedged against terrain/walls.
    //
    // Fix: Prefix on Unstuck() that replaces the teleport with a physics-
    // based escape — backward impulse + random torque + set _reversing for
    // mode 3 compatibility. No vertical component to avoid hop appearance.
    // =========================================================================
    public static class Bug9_AntiTeleportFix
    {
        private static readonly FieldInfo ReversingField =
            AccessTools.Field(typeof(RobotMining), "_reversing");
        private static readonly FieldInfo DeltaField =
            AccessTools.Field(typeof(RobotMining), "Delta");

        public static bool Unstuck_Prefix(RobotMining __instance)
        {
            try
            {
                var rb = __instance.RigidBody;
                if (rb != null)
                {
                    // Zero current velocity to break any stuck-loop momentum
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;

                    // Push backward to escape obstacle (reduced from 2f to prevent
                    // wheelies — no vertical component for immersion)
                    Vector3 backDir = -__instance.ThingTransform.forward;
                    rb.AddForce(backDir * 1f, ForceMode.VelocityChange);

                    // Random torque to change heading (reduced from ±3 to prevent spin)
                    float randomTorque = UnityEngine.Random.Range(-2f, 2f);
                    rb.AddTorque(Vector3.up * randomTorque, ForceMode.VelocityChange);
                }

                // For mode 3, trigger extended reverse via our Roam_Prefix
                // Reduced from 8f to 5f to prevent excessive backward driving
                if (__instance.Mode == 3 && ReversingField != null)
                {
                    ReversingField.SetValue(__instance, 5f);
                }

                // For other modes, randomize steering to help escape
                if (DeltaField != null)
                {
                    float delta = UnityEngine.Random.Range(-30f, 30f);
                    DeltaField.SetValue(__instance, delta);
                    __instance.TargetSteeringAngle = delta;
                }

                if (AimeeDebugOverlay.Enabled)
                    Plugin.Log.LogInfo(string.Format(
                        "[Bug9] Unstuck: physics escape for AIMeE#{0} mode={1}",
                        __instance.GetInstanceID(), __instance.Mode));
            }
            catch (Exception ex)
            {
                if (AimeeDebugOverlay.Enabled)
                    Plugin.Log.LogError(string.Format("[Bug9] Error: {0}", ex.Message));
            }

            return false; // Skip original teleport
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
    //  6. Sorts queue by distance — closest ore mined first (Bug 10)
    // =========================================================================
    public static class Bug4_RoamFix
    {
        // Expose fields for debug overlay access
        internal static readonly FieldInfo QueueField =
            AccessTools.Field(typeof(RobotMining), "_minableDataQueue");
        internal static readonly FieldInfo ReversingField =
            AccessTools.Field(typeof(RobotMining), "_reversing");
        internal static readonly FieldInfo RoamTimeoutField =
            AccessTools.Field(typeof(RobotMining), "_roamTimeout");
        internal static readonly FieldInfo DeltaField =
            AccessTools.Field(typeof(RobotMining), "Delta");
        internal static readonly FieldInfo ScanTimeoutField =
            AccessTools.Field(typeof(RobotMining), "_minableScanTimeout");
        internal static readonly FieldInfo TargetMinableField =
            AccessTools.Field(typeof(RobotMining), "TargetMinable");
        internal static readonly FieldInfo SearchAreaField =
            AccessTools.Field(typeof(RobotMining), "MinableSearchArea");
        internal static readonly FieldInfo MaxDepthField =
            AccessTools.Field(typeof(RobotMining), "maxMiningDepth");
        internal static readonly FieldInfo MineCancellationField =
            AccessTools.Field(typeof(RobotMining), "_mineCancellation");
        private static readonly MethodInfo MovingToMineableMethod =
            AccessTools.Method(typeof(RobotMining), "MovingToMineable");

        // Cap per-AIMeE queue to prevent one bot monopolizing all ore
        // Reduced from 10 to 5 so multiple AIMeEs share nearby ore fairly
        private const int MaxQueuePerAimee = 5;

        // Bug 9 improvement: velocity-based stuck detection
        // Tracks how long each AIMeE has been driving forward at near-zero velocity
        private static readonly Dictionary<int, float> _stuckDriveTime =
            new Dictionary<int, float>();
        // Also track where AIMeE was when stuck timer started, so we can
        // distinguish "slow on a cliff" (position changes) from "truly stuck"
        // (position doesn't change). Without this, cliff climbing triggers
        // false-positive stuck detection because velocity is low.
        private static readonly Dictionary<int, Vector3> _stuckStartPos =
            new Dictionary<int, Vector3>();
        private const float StuckVelocityThreshold = 0.1f;
        private const float StuckTimeBeforeReverse = 5f;
        private const float StuckPositionThreshold = 0.5f; // must move < 0.5m to be truly stuck

        // Bug 11: Post-reverse cooldown prevents stuck detection from immediately
        // re-triggering after a reverse ends (which causes tight circling loops)
        private static readonly Dictionary<int, float> _reverseEndCooldown =
            new Dictionary<int, float>();
        private const float PostReverseCooldownTime = 3f;

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
                    int hashBefore = Vein.AllAimeeQueuedMinables.Count;
                    __instance.ClearMinableQueue();
                    int hashAfterClear = Vein.AllAimeeQueuedMinables.Count;

                    VoxelTerrain.GetAimeeMinableQueue(
                        __instance.ThingTransformPosition,
                        (float)searchArea,
                        queue,
                        maxDepth);

                    int rawQueueCount = queue.Count;
                    int hashAfterFill = Vein.AllAimeeQueuedMinables.Count;

                    // Bug 10: Sort queue by distance — farthest at front (index 0),
                    // closest at end. Queue pops from end, so closest ore gets mined first.
                    // Sort BEFORE cap so cap removes farthest ore.
                    if (queue.Count > 1)
                    {
                        Vector3 aimeePos = __instance.ThingTransformPosition;
                        queue.Sort((a, b) =>
                        {
                            float distA = (a.Vein.GetMinableWorldPosition(a.MinableIndex) - aimeePos).sqrMagnitude;
                            float distB = (b.Vein.GetMinableWorldPosition(b.MinableIndex) - aimeePos).sqrMagnitude;
                            return distB.CompareTo(distA); // Descending: farthest first
                        });
                    }

                    // Cap queue: remove excess from front (= farthest ore after sort)
                    while (queue.Count > MaxQueuePerAimee)
                    {
                        var excess = queue[0];
                        excess.Vein.RemoveAimeeMinableHash(excess.MinableIndex);
                        queue.RemoveAt(0);
                    }

                    // Detailed diagnostics on every queue fill attempt
                    bool debug = AimeeDebugOverlay.Enabled;
                    if (debug)
                    {
                        Plugin.Log.LogInfo(string.Format(
                            "[Bug4] QUEUE FILL id={0}: hashBefore={1}, hashAfterClear={2}, rawQueue={3}, hashAfterFill={4}, cappedQueue={5}, pos=({6:F0},{7:F0},{8:F0}), searchArea={9}, maxDepth={10}",
                            __instance.GetInstanceID(), hashBefore, hashAfterClear,
                            rawQueueCount, hashAfterFill, queue.Count,
                            __instance.ThingTransformPosition.x,
                            __instance.ThingTransformPosition.y,
                            __instance.ThingTransformPosition.z,
                            searchArea, maxDepth));

                        // If queue is still 0, diagnose WHY
                        if (rawQueueCount == 0 && hashAfterClear > 0)
                        {
                            Plugin.Log.LogWarning(string.Format(
                                "[Bug4] LEAK DETECTED! AllAimeeQueuedMinables has {0} stale entries blocking queue fill! Clearing as emergency fix.",
                                hashAfterClear));
                            Vein.AllAimeeQueuedMinables.Clear();

                            // Retry fill after clearing
                            VoxelTerrain.GetAimeeMinableQueue(
                                __instance.ThingTransformPosition,
                                (float)searchArea,
                                queue,
                                maxDepth);
                            // Bug 10: Sort retry by distance too
                            if (queue.Count > 1)
                            {
                                Vector3 retryPos = __instance.ThingTransformPosition;
                                queue.Sort((a, b) =>
                                {
                                    float dA = (a.Vein.GetMinableWorldPosition(a.MinableIndex) - retryPos).sqrMagnitude;
                                    float dB = (b.Vein.GetMinableWorldPosition(b.MinableIndex) - retryPos).sqrMagnitude;
                                    return dB.CompareTo(dA);
                                });
                            }
                            while (queue.Count > MaxQueuePerAimee)
                            {
                                var excess = queue[0];
                                excess.Vein.RemoveAimeeMinableHash(excess.MinableIndex);
                                queue.RemoveAt(0);
                            }
                            Plugin.Log.LogInfo(string.Format(
                                "[Bug4] After emergency clear: queue={0}, globalHash={1}",
                                queue.Count, Vein.AllAimeeQueuedMinables.Count));
                        }
                    }

                    // Update debug overlay data
                    if (debug)
                    {
                        AimeeDebugOverlay.UpdateQueueFillData(
                            __instance.GetInstanceID(), hashBefore, hashAfterClear,
                            rawQueueCount, hashAfterFill, queue.Count);
                    }
                }

                bool hasTarget = queue.Count > 0;

                // Set target from queue tail (closest ore after sort).
                // Bug 10: When picking a NEW target (previous was mined/dead),
                // re-sort remaining queue by CURRENT position so AIMeE always
                // drives to the closest ore from where she is NOW — not where
                // she was when the queue was originally filled.
                if (hasTarget)
                {
                    var currentTarget = (TargetMinableData?)TargetMinableField.GetValue(__instance);
                    if (currentTarget == null && queue.Count > 1)
                    {
                        Vector3 curPos = __instance.ThingTransformPosition;
                        queue.Sort((a, b) =>
                        {
                            float dA = (a.Vein.GetMinableWorldPosition(a.MinableIndex) - curPos).sqrMagnitude;
                            float dB = (b.Vein.GetMinableWorldPosition(b.MinableIndex) - curPos).sqrMagnitude;
                            return dB.CompareTo(dA);
                        });
                    }
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
                int id = __instance.GetInstanceID();

                float reversing = (float)ReversingField.GetValue(__instance);

                // Bug 11: Post-reverse cooldown prevents stuck→reverse→stuck loops
                float cooldown = 0f;
                _reverseEndCooldown.TryGetValue(id, out cooldown);
                if (cooldown > 0f)
                {
                    cooldown -= Time.deltaTime;
                    _reverseEndCooldown[id] = cooldown;
                }

                if (reversing <= 0f)
                {
                    RaycastHit hit;
                    int mask = ~0;
                    try { mask = CursorManager.Instance.TerrainHitMask; } catch { }

                    bool hitObstacle = Physics.Raycast(worldCoM,
                        __instance.ThingTransform.forward, out hit, 0.5f, mask);

                    // Velocity-based stuck detection (disabled during post-reverse cooldown)
                    // Also checks position delta to avoid false positives on cliffs:
                    // climbing a steep slope = low velocity but position IS changing.
                    bool velocityStuck = false;
                    if (cooldown <= 0f)
                    {
                        if (!hitObstacle && velocity < StuckVelocityThreshold &&
                            __instance.TargetMotorPower > 0f)
                        {
                            if (!_stuckDriveTime.TryGetValue(id, out float stuckTime))
                            {
                                stuckTime = 0f;
                                _stuckStartPos[id] = __instance.ThingTransformPosition;
                            }
                            stuckTime += Time.deltaTime;
                            _stuckDriveTime[id] = stuckTime;

                            if (stuckTime >= StuckTimeBeforeReverse)
                            {
                                // Only trigger if position hasn't changed much
                                // (rules out slow climbing on slopes/cliffs)
                                Vector3 startPos;
                                if (!_stuckStartPos.TryGetValue(id, out startPos))
                                    startPos = __instance.ThingTransformPosition;
                                float posDelta = (startPos - __instance.ThingTransformPosition).magnitude;
                                if (posDelta < StuckPositionThreshold)
                                {
                                    velocityStuck = true;
                                }
                                else if (AimeeDebugOverlay.Enabled)
                                {
                                    Plugin.Log.LogInfo(string.Format(
                                        "[Bug9] Velocity-stuck SKIPPED for AIMeE#{0}: moved {1:F2}m (climbing?)",
                                        id, posDelta));
                                }
                                _stuckDriveTime[id] = 0f;
                                _stuckStartPos[id] = __instance.ThingTransformPosition;
                            }
                        }
                        else
                        {
                            _stuckDriveTime[id] = 0f;
                        }
                    }
                    else
                    {
                        _stuckDriveTime[id] = 0f;
                    }

                    if (hitObstacle || velocityStuck)
                    {
                        reversing = 3f;
                        float steerDelta = UnityEngine.Random.Range(-30f, 30f);
                        DeltaField.SetValue(__instance, steerDelta);
                        __instance.TargetSteeringAngle = steerDelta;

                        if (AimeeDebugOverlay.Enabled && velocityStuck)
                            Plugin.Log.LogInfo(string.Format(
                                "[Bug9] Velocity-stuck reverse for AIMeE#{0}, vel={1:F3}",
                                __instance.GetInstanceID(), velocity));
                    }
                }

                if (reversing > 0f)
                {
                    __instance.TargetMotorPower = -RocketMath.MapToScale(
                        0f, __instance.MaxSpeed, power, 0f, velocity);
                    reversing -= Time.deltaTime;

                    // Bug 11: When reverse just ended, reset steering to a mild
                    // angle and set cooldown to prevent immediate re-triggering
                    if (reversing <= 0f)
                    {
                        float mildSteer = UnityEngine.Random.Range(-5f, 5f);
                        DeltaField.SetValue(__instance, mildSteer);
                        __instance.TargetSteeringAngle = mildSteer;
                        _reverseEndCooldown[id] = PostReverseCooldownTime;
                        RoamTimeoutField.SetValue(__instance, 0.5f);
                    }

                    ReversingField.SetValue(__instance, reversing);
                    return false;
                }
                ReversingField.SetValue(__instance, reversing);

                float roamTimeout = (float)RoamTimeoutField.GetValue(__instance);
                if (roamTimeout <= 0f)
                {
                    roamTimeout = UnityEngine.Random.Range(8f, 12f);
                    float delta = UnityEngine.Random.Range(-30f, 30f);
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

                // Mode 3 periodic logging (every 0.5s per AIMeE)
                if (AimeeDebugOverlay.Enabled && Plugin.ShouldLog(string.Format("Mode3-{0}", __instance.GetInstanceID()), 0.5f))
                {
                    int vicinity = VoxelTerrain.GetNumberOfMinablesNearSurface(
                        __instance.ThingTransformPosition, (float)searchArea, maxDepth);
                    Plugin.Log.LogInfo(string.Format(
                        "[Mode3] AIMeE#{0}: vicinity={1}, queue={2}, globalHash={3}, vel={4:F2}, motor={5:F4}, pos=({6:F0},{7:F0},{8:F0})",
                        __instance.GetInstanceID(), vicinity, queue.Count,
                        Vein.AllAimeeQueuedMinables.Count,
                        velocity, __instance.TargetMotorPower,
                        __instance.ThingTransformPosition.x, __instance.ThingTransformPosition.y,
                        __instance.ThingTransformPosition.z));
                }

                return false; // skip original
            }
            catch (Exception ex)
            {
                if (AimeeDebugOverlay.Enabled && Plugin.ShouldLog("Bug4-err", 30f))
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
    // instead of reading the empty local queue, estimate queue using
    // GetNumberOfMinablesNearSurface and cap to the roam queue max (10).
    // This is still an estimate (not replicated server queue), but avoids
    // hard-zero and tracks usable queued targets better than raw vicinity.
    // =========================================================================
    public static class Bug6_QueueClientSync
    {
        private static readonly FieldInfo SearchAreaField =
            AccessTools.Field(typeof(RobotMining), "MinableSearchArea");
        private static readonly FieldInfo MaxDepthField =
            AccessTools.Field(typeof(RobotMining), "maxMiningDepth");
        private const int MaxQueuePerAimee = 5;

        public static void GetLogicValue_Postfix(RobotMining __instance, LogicType logicType, ref double __result)
        {
            // Only act on clients (non-authority) for MineablesInQueue
            if (logicType != LogicType.MineablesInQueue) return;

            if (__instance.HasAuthority)
            {
                // On server: log the real queue value for diagnosing "always 0" reports
                if (AimeeDebugOverlay.Enabled && Plugin.ShouldLog("Bug6-srv", 5f))
                    Plugin.Log.LogInfo(string.Format("[Bug6] Server queue read: {0} (real _minableDataQueue.Count)",
                        (int)__result));
                return;
            }

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
                __result = (double)Math.Min(count, MaxQueuePerAimee);

                if (AimeeDebugOverlay.Enabled && Plugin.ShouldLog("Bug6-est", 5f))
                    Plugin.Log.LogInfo(string.Format("[Bug6] Client estimate: count={0}, capped={1}, pos=({2:F0},{3:F0},{4:F0})",
                        count, (int)__result,
                        __instance.ThingTransformPosition.x,
                        __instance.ThingTransformPosition.y,
                        __instance.ThingTransformPosition.z));
            }
            catch (Exception ex)
            {
                if (AimeeDebugOverlay.Enabled && Plugin.ShouldLog("Bug6-err", 10f))
                    Plugin.Log.LogError(string.Format("[Bug6] Client estimate ERROR: {0}", ex.Message));
            }
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

        // Track call counts for diagnosing whether Bug7 is actually intercepting
        internal static int TotalCalls = 0;
        internal static int SuccessCalls = 0;
        internal static int ErrorCalls = 0;
        internal static int LastResult = -1;
        internal static string LastCaller = "unknown";

        public static bool GetNumberOfMinablesNearSurface_Prefix(
            Vector3 position, float boundsSize, int maxDepthFromSurface, ref int __result)
        {
            TotalCalls++;
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

                        // Near-surface check — use Bug 13's improved column scan
                        // instead of the original single-voxel check
                        bool nearSurface = false;
                        Bug13_UndergroundOreFix.IsNearSurface_Prefix(
                            worldPos, maxDepthFromSurface, ref nearSurface);
                        if (nearSurface)
                        {
                            count++;
                        }
                    }
                }

                if (AimeeDebugOverlay.Enabled && Plugin.ShouldLog("Bug7-Diag", 5f))
                {
                    Plugin.Log.LogInfo(string.Format(
                        "[Bug7] Vicinity: veins={0}, total={1}, active={2}, inBounds={3}, nearSurface={4}, pos=({5:F0},{6:F0},{7:F0}), bounds={8}, depth={9}, calls={10}/{11}, errors={12}",
                        veins.Count, totalMinables, activeCount, inBoundsCount, count,
                        position.x, position.y, position.z,
                        boundsSize, maxDepthFromSurface,
                        SuccessCalls, TotalCalls, ErrorCalls));
                }

                __result = count;
                LastResult = count;
                SuccessCalls++;
            }
            catch (Exception ex)
            {
                ErrorCalls++;
                if (AimeeDebugOverlay.Enabled)
                    Plugin.Log.LogError(string.Format("[Bug7] ERROR #{0}: {1}\n{2}", ErrorCalls, ex.Message, ex.StackTrace));
                return true; // fall through to original method
            }
            return false; // skip original
        }
    }

    // =========================================================================
    // BUG 12: Ghost Ore — mined ore stays visible on multiplayer clients
    //
    // Root cause: AIMeE's mining paths call Vein.TryMineServer() which marks
    // ore as mined server-side but doesn't sync to clients. The game's client
    // sync mechanism uses VoxelChangeEvent (created by SetDensityWorldSpace),
    // which calls MineAtPositionClient on clients when density < 127. Every
    // other mining caller (player, quarry, explosion) uses this — AIMeE skips
    // it entirely.
    //
    // Fix: Two-event approach. After TryMineServer succeeds:
    //   1. Inject VoxelChangeEvent with density 0 → client Execute() carves
    //      terrain AND calls MineAtPositionClient (ore disappears).
    //   2. Inject a SECOND event with the ORIGINAL density → client Execute()
    //      restores terrain to its original density (hole filled back in).
    //
    // The ore stays mined because MineAtPositionClient sets IsActive=false,
    // and the second event's ShouldReleaseMinables(originalDensity >= 127)
    // returns false — so MineAtPositionClient is NOT called again.
    //
    // Net result on client: ore removed, terrain intact. Server: unmodified.
    // =========================================================================
    public static class Bug12_GhostOreFix
    {
        public static void TryMineServer_Postfix(bool __result, Vector3Int minedPosition)
        {
            if (!__result)
                return;

            if (!NetworkServer.HasClients())
                return;

            try
            {
                // Read the current terrain density BEFORE injecting events.
                float currentDensity = VoxelTerrain.GetDensityWorldSpace(minedPosition);
                byte originalDensityByte = VoxelTerrain.DensityToByte(currentDensity);

                Vector3 pos = new Vector3(minedPosition.x, minedPosition.y, minedPosition.z);

                // Event 1: density 0 → ShouldReleaseMinables(0)=true
                //   → MineAtPositionClient fires → ore model removed
                //   → terrain carved to empty (temporary)
                VoxelTerrain.VoxelChangeEvents.Add(
                    VoxelTerrain.VoxelChangeEvent.Create(pos, 0));

                // Event 2: original density → restores terrain voxel
                //   → ShouldReleaseMinables(>=127)=false → no double-mine
                //   → terrain density restored to original value
                VoxelTerrain.VoxelChangeEvents.Add(
                    VoxelTerrain.VoxelChangeEvent.Create(pos, originalDensityByte));

                if (AimeeDebugOverlay.Enabled)
                    Plugin.Log.LogInfo(string.Format(
                        "[Bug12] Two-event sync at ({0},{1},{2}): mine(0) + restore({3})",
                        minedPosition.x, minedPosition.y, minedPosition.z,
                        originalDensityByte));
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(string.Format("[Bug12] SyncList injection error: {0}", ex.Message));
            }
        }
    }

    // =========================================================================
    // BUG 13: Underground Ore — AIMeE mines ore in tunnels/caves
    //
    // Root cause: Vein.IsNearSurface(position, maxDepth) only checks a single
    // voxel at position + Vector3Int.up * maxDepth (3 blocks up). If that
    // voxel has density < 0.5, the ore is considered "near surface." In a
    // tunnel or cave, the space 3 blocks above ore can easily be empty even
    // though there is solid rock between the cave and the actual surface.
    //
    // Fix: Replace IsNearSurface with a column scan. From the ore position,
    // scan upward block-by-block. Track transitions between solid (density
    // >= 0.5) and empty (< 0.5). If we find a cave ceiling pattern (empty
    // then solid again), the ore is underground. The ore is only reachable
    // if there is continuous empty space from within maxDepth blocks above
    // the ore all the way to the surface with no intervening solid layer.
    // =========================================================================
    public static class Bug13_UndergroundOreFix
    {
        // Maximum number of blocks to scan upward from ore position.
        // Must be large enough to reach the cave ceiling in tall caves.
        // 20 was too low — caves 17+ blocks tall would fool the scan
        // because it never found the ceiling. 64 covers all realistic
        // Stationeers terrain depths on any planet.
        private const int MaxScanHeight = 64;
        private const float SolidThreshold = 0.49803922f; // same as game's ~127/255

        // Diagnostic counters
        internal static int TotalChecked = 0;
        internal static int RejectedCount = 0;

        public static bool IsNearSurface_Prefix(Vector3Int position, int maxDepth, ref bool __result)
        {
            TotalChecked++;
            try
            {
                // Scan upward from one block above the ore position.
                // We're looking for a continuous column of empty voxels from
                // near the ore to the surface. If we encounter solid terrain
                // AFTER already seeing empty space, there's a ceiling → cave.
                bool foundEmpty = false;
                int emptyStart = -1;

                for (int dy = 1; dy <= MaxScanHeight; dy++)
                {
                    Vector3Int checkPos = new Vector3Int(position.x, position.y + dy, position.z);
                    float density = VoxelTerrain.GetDensityWorldSpace(checkPos);

                    if (density < SolidThreshold)
                    {
                        // Empty voxel
                        if (!foundEmpty)
                        {
                            foundEmpty = true;
                            emptyStart = dy;
                        }
                    }
                    else
                    {
                        // Solid voxel
                        if (foundEmpty)
                        {
                            // We found empty space then hit solid again = cave ceiling
                            __result = false;
                            RejectedCount++;
                            return false;
                        }
                    }
                }

                // If we found empty space starting within maxDepth blocks and
                // never hit a ceiling, the ore is surface-reachable
                __result = foundEmpty && emptyStart <= maxDepth;
                if (!__result) RejectedCount++;
            }
            catch (Exception ex)
            {
                if (AimeeDebugOverlay.Enabled)
                    Plugin.Log.LogError(string.Format("[Bug13] IsNearSurface error: {0}", ex.Message));
                // Fallback to original check
                __result = VoxelTerrain.GetDensityWorldSpace(
                    position + Vector3Int.up * maxDepth) < SolidThreshold;
            }
            return false; // skip original
        }
    }

    // =========================================================================
    // BUG 14: Stale Queue — queue not cleared on pickup/power-on/target-set
    //
    // Root cause: AIMeE’s _minableDataQueue is only refilled when it empties
    // naturally (all entries consumed). It is NEVER cleared when:
    //   - AIMeE is picked up (moved to a new location)
    //   - AIMeE is turned on (may have been repositioned while off)
    //   - TargetXYZ is set via IC (AIMeE will drive to new location)
    // This means stale queue entries point to ore at old locations, wasting
    // time driving back. Worse, AllAimeeQueuedMinables hash entries linger,
    // blocking other AIMeEs from claiming that ore.
    //
    // Fix: Track IsCursor and OnOff state per instance. Clear queue on:
    //   1. IsCursor transitions to true (picked up)
    //   2. OnOff transitions false→true (turned on)
    //   3. TargetX/Y/Z is written via SetLogicValue (new destination)
    // =========================================================================
    public static class Bug14_StaleQueueFix
    {
        private static readonly Dictionary<int, bool> _wasCursor = new Dictionary<int, bool>();
        private static readonly Dictionary<int, bool> _wasOn = new Dictionary<int, bool>();

        public static void UpdateEachFrame_Prefix(RobotMining __instance)
        {
            if (!__instance.HasAuthority) return;

            int id = __instance.GetInstanceID();
            bool isCursor = __instance.IsCursor;
            bool isOn = __instance.OnOff;

            _wasCursor.TryGetValue(id, out bool wasCursor);
            _wasOn.TryGetValue(id, out bool wasOn);

            bool shouldClear = false;
            string reason = null;

            if (isCursor && !wasCursor)
            {
                shouldClear = true;
                reason = "picked up";
            }
            else if (isOn && !wasOn)
            {
                shouldClear = true;
                reason = "turned on";
            }

            if (shouldClear)
            {
                __instance.ClearMinableQueue();
                if (AimeeDebugOverlay.Enabled)
                    Plugin.Log.LogInfo(string.Format(
                        "[Bug14] Queue cleared ({0}): id={1}", reason, id));
            }

            _wasCursor[id] = isCursor;
            _wasOn[id] = isOn;
        }

        public static void SetLogicValue_Postfix(RobotMining __instance, LogicType logicType)
        {
            if (logicType != LogicType.TargetX &&
                logicType != LogicType.TargetY &&
                logicType != LogicType.TargetZ)
                return;

            __instance.ClearMinableQueue();
            if (AimeeDebugOverlay.Enabled)
                Plugin.Log.LogInfo(string.Format(
                    "[Bug14] Queue cleared ({0} set): id={1}", logicType, __instance.GetInstanceID()));
        }
    }

    // =========================================================================
    // DIAGNOSTIC: Log every GetLogicValue(MineablesInVicinity) read
    // This shows what the IC chip / game actually sees vs our overlay
    // =========================================================================
    public static class DiagGetLogicValuePostfix
    {
        public static void Postfix(RobotMining __instance, LogicType logicType, double __result)
        {
            if (!AimeeDebugOverlay.Enabled) return;
            if (logicType == LogicType.MineablesInVicinity)
            {
                if (Plugin.ShouldLog(string.Format("Diag-Vicinity-{0}", __instance.GetInstanceID()), 1f))
                {
                    Plugin.Log.LogInfo(string.Format(
                        "[Diag] GetLogicValue(MineablesInVicinity) AIMeE#{0}: returned={1}, Bug7.LastResult={2}, Bug7.calls={3}/{4}, errors={5}",
                        __instance.GetInstanceID(), __result,
                        Bug7_VicinityBoundsFix.LastResult,
                        Bug7_VicinityBoundsFix.SuccessCalls,
                        Bug7_VicinityBoundsFix.TotalCalls,
                        Bug7_VicinityBoundsFix.ErrorCalls));
                }
            }
        }
    }

    // =========================================================================
    // DEBUG OVERLAY: "aimeedebug" console command + floating stats
    //
    // Type "aimeedebug" in console to toggle. Shows:
    //  - Floating text above each AIMeE with stats
    //  - Detailed state change logging to BepInEx console
    // =========================================================================
    public static class AimeeDebugOverlay
    {
        public static bool Enabled = false;
        private static GUIStyle _style;
        private static GUIStyle _shadowStyle;
        private static GUIStyle _panelStyle;
        private static GUIStyle _bgStyle;

        // Cached data updated at low frequency from Update()
        private static float _lastCacheTime;
        private const float CacheInterval = 0.5f;
        private static RobotMining[] _cachedAimees = new RobotMining[0];
        private static readonly Dictionary<int, CachedAimeeData> _cachedData =
            new Dictionary<int, CachedAimeeData>();
        private static string _globalPanelText = "";

        // Per-AIMeE diagnostic data from last queue fill
        private static readonly Dictionary<int, QueueFillDiag> _lastQueueFill =
            new Dictionary<int, QueueFillDiag>();

        // Per-AIMeE last known state for change detection
        private static readonly Dictionary<int, AimeeState> _lastState =
            new Dictionary<int, AimeeState>();

        internal struct QueueFillDiag
        {
            public int HashBefore;
            public int HashAfterClear;
            public int RawQueueCount;
            public int HashAfterFill;
            public int CappedQueueCount;
            public float Timestamp;
        }

        internal struct AimeeState
        {
            public int Mode;
            public int QueueCount;
            public int Vicinity;
            public bool IsStorageFull;
            public bool IsMining;
            public bool HasTarget;
        }

        internal struct CachedAimeeData
        {
            public string OverlayText;
            public Vector3 WorldPos;
        }

        public static string HandleCommand(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0].ToLower() == "on") Enabled = true;
                else if (args[0].ToLower() == "off") Enabled = false;
                else return "Usage: aimeedebug [on|off]";
            }
            else
            {
                Enabled = !Enabled;
            }
            string state = Enabled ? "ENABLED" : "DISABLED";
            Plugin.Log.LogInfo(string.Format("[Debug] AIMeE debug overlay {0}", state));
            return string.Format("AIMeE debug overlay: {0}", state);
        }

        public static void UpdateQueueFillData(int instanceId, int hashBefore,
            int hashAfterClear, int rawQueue, int hashAfterFill, int cappedQueue)
        {
            _lastQueueFill[instanceId] = new QueueFillDiag
            {
                HashBefore = hashBefore,
                HashAfterClear = hashAfterClear,
                RawQueueCount = rawQueue,
                HashAfterFill = hashAfterFill,
                CappedQueueCount = cappedQueue,
                Timestamp = Time.time
            };
        }

        /// <summary>
        /// Called from Plugin.Update() — does all expensive work at low frequency.
        /// </summary>
        public static void UpdateCache()
        {
            float now = Time.time;
            if (now - _lastCacheTime < CacheInterval) return;
            _lastCacheTime = now;

            _cachedAimees = UnityEngine.Object.FindObjectsOfType<RobotMining>();
            _cachedData.Clear();

            int searchArea = 32;
            int maxDepth = 3;
            try
            {
                if (Bug4_RoamFix.SearchAreaField != null) searchArea = (int)Bug4_RoamFix.SearchAreaField.GetValue(null);
                if (Bug4_RoamFix.MaxDepthField != null) maxDepth = (int)Bug4_RoamFix.MaxDepthField.GetValue(null);
            }
            catch { }

            foreach (var aimee in _cachedAimees)
            {
                try
                {
                    int id = aimee.GetInstanceID();
                    bool isAuthority = aimee.HasAuthority;

                    // Values available on both client and server
                    int vicinityViaLogic = (int)aimee.GetLogicValue(LogicType.MineablesInVicinity);
                    int vicinityDirect = VoxelTerrain.GetNumberOfMinablesNearSurface(
                        aimee.ThingTransformPosition, (float)searchArea, maxDepth);

                    string vicinityStr;
                    if (vicinityViaLogic != vicinityDirect)
                    {
                        vicinityStr = string.Format("{0} (IC={1} Direct={2} MISMATCH!)",
                            vicinityViaLogic, vicinityViaLogic, vicinityDirect);
                        if (Plugin.ShouldLog(string.Format("Vicinity-Mismatch-{0}", id), 2f))
                        {
                            Plugin.Log.LogWarning(string.Format(
                                "[Diag] VICINITY MISMATCH AIMeE#{0}: IC_GetLogicValue={1}, DirectCall={2}",
                                id, vicinityViaLogic, vicinityDirect));
                        }
                    }
                    else
                    {
                        vicinityStr = vicinityViaLogic.ToString();
                    }

                    string text;
                    if (isAuthority)
                    {
                        // SERVER/HOST view: full data available
                        var queue = (List<TargetMinableData>)Bug4_RoamFix.QueueField.GetValue(aimee);
                        var targetRaw = Bug4_RoamFix.TargetMinableField.GetValue(aimee);
                        var cancelWrapper = (CancellationTokenWrapper)Bug4_RoamFix.MineCancellationField.GetValue(aimee);

                        bool hasTarget = targetRaw != null && ((TargetMinableData?)targetRaw) != null;
                        bool isMining = cancelWrapper != null && cancelWrapper.Initialized;
                        int queueCount = queue != null ? queue.Count : -1;

                        string targetInfo = "none";
                        if (hasTarget)
                        {
                            var target = ((TargetMinableData?)targetRaw).Value;
                            Vector3 tPos = target.Vein.GetMinableWorldPosition(target.MinableIndex);
                            float tDist = Vector3.Distance(aimee.ThingTransformPosition, tPos);
                            bool tActive = target.Vein.GetActive(target.MinableIndex);
                            targetInfo = string.Format("d={0:F1} {1}", tDist, tActive ? "ACTIVE" : "DEAD");
                        }

                        string fillInfo = "no data";
                        if (_lastQueueFill.TryGetValue(id, out QueueFillDiag diag))
                        {
                            float age = now - diag.Timestamp;
                            fillInfo = string.Format("raw={0} hPre={1} hClr={2} hPost={3} ({4:F0}s ago)",
                                diag.RawQueueCount, diag.HashBefore, diag.HashAfterClear,
                                diag.HashAfterFill, age);
                        }

                        float reversingVal = 0f;
                        try
                        {
                            if (Bug4_RoamFix.ReversingField != null)
                                reversingVal = (float)Bug4_RoamFix.ReversingField.GetValue(aimee);
                        }
                        catch { }

                        text = string.Format(
                            "=== AIMeE #{0} === [SERVER]\n" +
                            "Mode: {1}  On: {2}  Power: {3}\n" +
                            "Queue: {4}  Vicinity: {5}\n" +
                            "GlobalHash: {6}\n" +
                            "Target: {7}\n" +
                            "Mining: {8}  StorageFull: {9}\n" +
                            "Vel: {10:F1}  Motor: {11:F3}  Rev: {12:F1}\n" +
                            "LastFill: {13}",
                            id, aimee.Mode, aimee.OnOff, aimee.Powered,
                            queueCount, vicinityStr,
                            Vein.AllAimeeQueuedMinables.Count,
                            targetInfo,
                            isMining, aimee.IsStorageFull,
                            aimee.VelocityMagnitude, aimee.TargetMotorPower,
                            reversingVal,
                            fillInfo);

                        // Detect state changes (only meaningful on authority)
                        DetectStateChanges(aimee, id, queueCount, vicinityViaLogic, hasTarget, isMining);
                    }
                    else
                    {
                        // CLIENT view: queue and target fields are server-only,
                        // but we can estimate queue via GetLogicValue (Bug 6 postfix)
                        // and show vicinity-based info for the player.
                        int queueEstimate = (int)aimee.GetLogicValue(LogicType.MineablesInQueue);

                        // Try reading the target field — it's null on client but worth checking
                        string targetInfo = "server-only";
                        try
                        {
                            var targetRaw = Bug4_RoamFix.TargetMinableField.GetValue(aimee);
                            if (targetRaw != null && ((TargetMinableData?)targetRaw) != null)
                            {
                                var target = ((TargetMinableData?)targetRaw).Value;
                                Vector3 tPos = target.Vein.GetMinableWorldPosition(target.MinableIndex);
                                float tDist = Vector3.Distance(aimee.ThingTransformPosition, tPos);
                                bool tActive = target.Vein.GetActive(target.MinableIndex);
                                targetInfo = string.Format("d={0:F1} {1}", tDist, tActive ? "ACTIVE" : "DEAD");
                            }
                        }
                        catch { }

                        text = string.Format(
                            "=== AIMeE #{0} === [REMOTE]\n" +
                            "Mode: {1}  On: {2}  Power: {3}\n" +
                            "Queue(est): {4}  Vicinity: {5}\n" +
                            "Target: {6}\n" +
                            "Bug7: {7}/{8}/{9}  Bug13: {10}/{11}\n" +
                            "StorageFull: {12}  Vel: {13:F1}",
                            id, aimee.Mode, aimee.OnOff, aimee.Powered,
                            queueEstimate, vicinityStr,
                            targetInfo,
                            Bug7_VicinityBoundsFix.SuccessCalls,
                            Bug7_VicinityBoundsFix.TotalCalls,
                            Bug7_VicinityBoundsFix.ErrorCalls,
                            Bug13_UndergroundOreFix.RejectedCount,
                            Bug13_UndergroundOreFix.TotalChecked,
                            aimee.IsStorageFull,
                            aimee.VelocityMagnitude);
                    }

                    _cachedData[id] = new CachedAimeeData
                    {
                        OverlayText = text,
                        WorldPos = aimee.ThingTransformPosition + Vector3.up * 1.2f
                    };
                }
                catch (Exception ex)
                {
                    if (Plugin.ShouldLog("Debug-Cache-err", 10f))
                        Plugin.Log.LogError(string.Format("[Debug] Cache update error: {0}", ex.Message));
                }
            }

            _globalPanelText = string.Format(
                "=== AIMeE Debug ===\n" +
                "AIMeEs found: {0}\n" +
                "AllAimeeQueuedMinables: {1}\n" +
                "Time: {2:F1}",
                _cachedAimees.Length,
                Vein.AllAimeeQueuedMinables.Count,
                now);
        }

        /// <summary>
        /// Called from OnGUI — only renders cached strings, no expensive lookups.
        /// </summary>
        public static void DrawOverlayFromCache()
        {
            if (!Enabled) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label);
                _style.fontSize = 14;
                _style.fontStyle = FontStyle.Bold;
                _style.normal.textColor = new Color(1f, 0.48f, 0.09f);
                _style.alignment = TextAnchor.UpperCenter;
                _style.wordWrap = false;

                _shadowStyle = new GUIStyle(_style);
                _shadowStyle.normal.textColor = Color.black;

                _panelStyle = new GUIStyle(_style);
                _panelStyle.fontSize = 13;
                _panelStyle.alignment = TextAnchor.UpperLeft;
                _panelStyle.normal.textColor = Color.white;

                _bgStyle = new GUIStyle(GUI.skin.box);
                _bgStyle.normal.background = MakeTexture(1, 1, new Color(0f, 0f, 0f, 0.7f));
            }

            // Render cached per-AIMeE overlays
            foreach (var kvp in _cachedData)
            {
                Vector3 screenPos = cam.WorldToScreenPoint(kvp.Value.WorldPos);
                if (screenPos.z <= 0f || screenPos.z > 100f) continue;

                float x = screenPos.x;
                float y = Screen.height - screenPos.y;
                float scale = Mathf.Clamp(1f - (screenPos.z - 5f) / 60f, 0.5f, 1.5f);
                int fontSize = Mathf.RoundToInt(14 * scale);
                _style.fontSize = fontSize;
                _shadowStyle.fontSize = fontSize;

                float width = 350f * scale;
                float height = 200f * scale;
                Rect rect = new Rect(x - width * 0.5f, y - height, width, height);
                Rect shadowRect = new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height);

                GUI.Label(shadowRect, kvp.Value.OverlayText, _shadowStyle);
                GUI.Label(rect, kvp.Value.OverlayText, _style);
            }

            // Global panel
            Rect bgRect = new Rect(10, 10, 260, 75);
            GUI.Box(bgRect, "", _bgStyle);
            GUI.Label(new Rect(15, 12, 250, 70), _globalPanelText, _panelStyle);
        }

        private static void DetectStateChanges(RobotMining aimee, int id,
            int queueCount, int vicinity, bool hasTarget, bool isMining)
        {
            AimeeState current = new AimeeState
            {
                Mode = aimee.Mode,
                QueueCount = queueCount,
                Vicinity = vicinity,
                IsStorageFull = aimee.IsStorageFull,
                IsMining = isMining,
                HasTarget = hasTarget
            };

            if (_lastState.TryGetValue(id, out AimeeState prev))
            {
                if (prev.Mode != current.Mode)
                    Plugin.Log.LogInfo(string.Format("[Debug] AIMeE#{0} Mode: {1} -> {2} (vicinity={3}, queue={4})", id, prev.Mode, current.Mode, current.Vicinity, current.QueueCount));
                if (prev.QueueCount != current.QueueCount)
                    Plugin.Log.LogInfo(string.Format("[Debug] AIMeE#{0} Queue: {1} -> {2} (globalHash={3})", id, prev.QueueCount, current.QueueCount, Vein.AllAimeeQueuedMinables.Count));
                if (prev.Vicinity != current.Vicinity && Plugin.ShouldLog(string.Format("Vicinity-Change-{0}", id), 1f))
                    Plugin.Log.LogInfo(string.Format("[Debug] AIMeE#{0} Vicinity: {1} -> {2}", id, prev.Vicinity, current.Vicinity));
                if (prev.IsMining != current.IsMining)
                    Plugin.Log.LogInfo(string.Format("[Debug] AIMeE#{0} Mining: {1} -> {2}", id, prev.IsMining, current.IsMining));
                if (prev.HasTarget != current.HasTarget)
                    Plugin.Log.LogInfo(string.Format("[Debug] AIMeE#{0} HasTarget: {1} -> {2}", id, prev.HasTarget, current.HasTarget));
                if (prev.IsStorageFull != current.IsStorageFull)
                    Plugin.Log.LogInfo(string.Format("[Debug] AIMeE#{0} StorageFull: {1} -> {2}", id, prev.IsStorageFull, current.IsStorageFull));
            }

            _lastState[id] = current;
        }

        private static Texture2D _bgTex;
        private static Texture2D MakeTexture(int w, int h, Color color)
        {
            if (_bgTex != null) return _bgTex;
            _bgTex = new Texture2D(w, h);
            Color[] pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            _bgTex.SetPixels(pixels);
            _bgTex.Apply();
            return _bgTex;
        }
    }
}
