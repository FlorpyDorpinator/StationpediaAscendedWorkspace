using System;
using System.Collections.Generic;
using System.Threading;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Networking;
using Assets.Scripts.Networks;
using Networks;
using UnityEngine;

namespace AtmosphereDirectionFix
{
    /// <summary>
    /// BepInEx plugin that fixes atmosphere Direction not being synchronized to clients in multiplayer.
    /// 
    /// BUG ANALYSIS:
    /// In AtmosphereHelper.ReadStatic(), when receiving network updates:
    /// 1. The Direction vector IS read from the network stream (line ~665)
    /// 2. For NEW atmospheres, Direction IS set (line ~681)
    /// 3. For EXISTING atmospheres, Direction is NEVER set - it's discarded!
    /// 
    /// This causes wind direction effects, atmospheric particles, and wind sounds to not work for
    /// clients in multiplayer games (both player-hosted and dedicated servers).
    /// 
    /// FIX: Use Prefix/Postfix to capture the Direction data and apply it to existing atmospheres.
    /// 
    /// INSTALL: Place the compiled DLL in your game's BepInEx/plugins/ folder.
    /// This fix is ONLY needed on CLIENT machines, not on dedicated servers.
    /// </summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class AtmosphereDirectionFixPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.stationeers.atmospheredirectionfix";
        public const string PluginName = "Atmosphere Direction Fix";
        public const string PluginVersion = "1.0.0";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            Logger.LogInfo($"{PluginName} v{PluginVersion} - Initializing...");
            
            try
            {
                var harmony = new Harmony(PluginGuid);
                harmony.PatchAll();
                Logger.LogInfo($"{PluginName} - Successfully patched network deserialization");
                Logger.LogInfo($"{PluginName} - Wind/atmosphere direction effects should now work in multiplayer!");
            }
            catch (Exception ex)
            {
                Logger.LogError($"{PluginName} - Failed to apply patches: {ex}");
            }
        }
    }

    /// <summary>
    /// Alternative fix: Patch the Atmosphere.Read() method to accept Direction from a thread-local store.
    /// 
    /// Flow:
    /// 1. Prefix on ReadStatic() captures the Direction and flags into thread-local storage
    /// 2. Postfix on ReadStatic() applies Direction to the atmosphere after Read() completes
    /// </summary>
    [HarmonyPatch]
    public static class AtmosphereDirectionFix
    {
        // Thread-local storage for captured Direction data
        // Using ThreadLocal because multiple threads may be processing atmospheres
        private static ThreadLocal<DirectionCaptureData> _capturedData = new ThreadLocal<DirectionCaptureData>(() => new DirectionCaptureData());

        private class DirectionCaptureData
        {
            public bool HasDirection;
            public Vector3 Direction;
            public long AtmosphereRefId;
            public AtmosphereHelper.AtmosphereMode Mode;
        }

        /// <summary>
        /// We need to re-implement the Direction reading logic since we can't easily 
        /// intercept the local variables. This reads the packet again from a copy.
        /// 
        /// Actually, a cleaner approach: patch ProcessUpdate in AtmosphericsManager
        /// to apply Direction after delta state is processed.
        /// </summary>
        
        // Instead of complex IL manipulation, let's hook at a higher level:
        // Patch the specific point where atmospheres receive updates

        [HarmonyPatch(typeof(Atmosphere), "Read")]
        [HarmonyPostfix]
        public static void Atmosphere_Read_Postfix(Atmosphere __instance)
        {
            // Check if we have captured Direction data for this atmosphere
            var data = _capturedData.Value;
            if (data != null && data.HasDirection && __instance.ReferenceId == data.AtmosphereRefId)
            {
                if (data.Mode == AtmosphereHelper.AtmosphereMode.World || 
                    data.Mode == AtmosphereHelper.AtmosphereMode.Thing)
                {
                    __instance.Direction = data.Direction;
                }
                // Clear the captured data
                data.HasDirection = false;
            }
        }
    }

    /// <summary>
    /// Simpler alternative: Directly patch the deserialization to re-read Direction.
    /// 
    /// The issue is that in the network stream, Direction comes BEFORE Read() is called,
    /// and ReadStatic() already consumes those bytes. We can't read them again.
    /// 
    /// Best approach: Completely replace ReadStatic with a fixed version.
    /// </summary>
    [HarmonyPatch(typeof(AtmosphereHelper), nameof(AtmosphereHelper.ReadStatic))]
    public static class AtmosphereHelper_ReadStatic_Patch
    {
        /// <summary>
        /// Prefix that completely replaces ReadStatic() with a corrected version.
        /// Returns false to skip the original method.
        /// </summary>
        [HarmonyPrefix]
        public static bool Prefix(RocketBinaryReader reader)
        {
            try
            {
                // Read the header - same as original
                long referenceId;
                Network.ReadPackedId(reader, out referenceId);
                byte networkUpdateFlags = reader.ReadByte();
                AtmosphereHelper.AtmosphereMode mode = (AtmosphereHelper.AtmosphereMode)reader.ReadByte();
                
                // Try to find existing atmosphere
                Atmosphere atmosphere = null;
                if (referenceId > 0L)
                {
                    atmosphere = Referencable.Find<Atmosphere>(referenceId);
                }
                
                // Read parent reference if flag 1 is set
                long parentReferenceId = 0L;
                if (AtmosphereHelper.IsNetworkUpdateRequired(1, networkUpdateFlags))
                {
                    Network.ReadPackedId(reader, out parentReferenceId);
                }
                
                // Read WorldGrid and Direction for World/Thing modes
                WorldGrid worldGrid = WorldGrid.INVALID;
                Vector3 direction = Vector3.zero;
                
                if (mode == AtmosphereHelper.AtmosphereMode.World || mode == AtmosphereHelper.AtmosphereMode.Thing)
                {
                    if (AtmosphereHelper.IsNetworkUpdateRequired(2, networkUpdateFlags))
                    {
                        worldGrid = reader.ReadWorldGrid();
                    }
                    if (AtmosphereHelper.IsNetworkUpdateRequired(8, networkUpdateFlags))
                    {
                        direction = reader.ReadVector3Half();
                    }
                }
                
                // Create new atmosphere if needed - same as original
                if (atmosphere == null && referenceId > 0L)
                {
                    switch (mode)
                    {
                        case AtmosphereHelper.AtmosphereMode.World:
                        {
                            // Remove existing atmosphere at this grid position if any
                            Atmosphere existingAtmo = AtmosphericsManager.Find(worldGrid);
                            if (existingAtmo != null)
                            {
                                AtmosphericsManager.AllAtmospheres.Remove(existingAtmo);
                            }
                            atmosphere = new Atmosphere(worldGrid, referenceId)
                            {
                                Direction = direction
                            };
                            break;
                        }
                        case AtmosphereHelper.AtmosphereMode.Network:
                        {
                            AtmosphericsNetwork network = Referencable.Find<AtmosphericsNetwork>(parentReferenceId);
                            if (network == null)
                            {
                                atmosphere = new Atmosphere();
                            }
                            else
                            {
                                atmosphere = new Atmosphere(network, referenceId);
                                network.AssignAtmosphere(atmosphere);
                            }
                            break;
                        }
                        case AtmosphereHelper.AtmosphereMode.Thing:
                        {
                            Assets.Scripts.Objects.Thing thing = Referencable.Find<Assets.Scripts.Objects.Thing>(parentReferenceId);
                            if (thing == null)
                            {
                                atmosphere = new Atmosphere();
                            }
                            else
                            {
                                if (thing.InternalAtmosphere != null)
                                {
                                    thing.InternalAtmosphere.Thing = null;
                                }
                                atmosphere = new Atmosphere(thing, new VolumeLitres(1.0), referenceId);
                                thing.InternalAtmosphere = atmosphere;
                            }
                            break;
                        }
                        default:
                            atmosphere = new Atmosphere();
                            break;
                    }
                }
                
                // Fallback if still null
                if (atmosphere == null)
                {
                    atmosphere = new Atmosphere();
                }
                
                // Set basic properties
                atmosphere.ReferenceId = referenceId;
                atmosphere.Mode = mode;
                
                // *** THIS IS THE FIX ***
                // Apply Direction to EXISTING atmospheres too (not just new ones)
                if (AtmosphereHelper.IsNetworkUpdateRequired(8, networkUpdateFlags) &&
                    (mode == AtmosphereHelper.AtmosphereMode.World || mode == AtmosphereHelper.AtmosphereMode.Thing))
                {
                    atmosphere.Direction = direction;
                }
                
                // Read remaining data
                atmosphere.Read(reader, networkUpdateFlags);
                
                return false; // Skip original method
            }
            catch (Exception ex)
            {
                AtmosphereDirectionFixPlugin.Log?.LogError($"Error in ReadStatic patch: {ex}");
                return true; // Fall back to original on error
            }
        }
    }
}
