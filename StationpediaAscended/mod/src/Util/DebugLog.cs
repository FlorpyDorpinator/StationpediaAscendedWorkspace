using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

#pragma warning disable CS0162 // Unreachable code detected (expected: SPDA_DEBUG const toggle)

namespace StationpediaAscended
{
    /// <summary>
    /// Centralized debug logging for Stationpedia Ascended.
    /// All debug logging should go through this class so it can be globally toggled.
    /// 
    /// To disable ALL debug logging before release, simply set SPDA_DEBUG to false.
    /// </summary>
    public static class DebugLog
    {
        /// <summary>
        /// Master switch for debug logging. Set to false before release builds.
        /// </summary>
        public const bool SPDA_DEBUG = true;

        /// <summary>
        /// Log a debug message (only when SPDA_DEBUG is true).
        /// </summary>
        [Conditional("DEBUG")]
        public static void Log(string message)
        {
            if (!SPDA_DEBUG) return;
            StationpediaAscendedMod.Log?.LogInfo($"[SPDA-DBG] {message}");
        }

        /// <summary>
        /// Log a debug message for search operations.
        /// </summary>
        [Conditional("DEBUG")]
        public static void Search(string message)
        {
            if (!SPDA_DEBUG) return;
            StationpediaAscendedMod.Log?.LogInfo($"[SPDA-Search] {message}");
        }

        /// <summary>
        /// Log a debug message for UI/header operations.
        /// </summary>
        [Conditional("DEBUG")]
        public static void UI(string message)
        {
            if (!SPDA_DEBUG) return;
            StationpediaAscendedMod.Log?.LogInfo($"[SPDA-UI] {message}");
        }

        /// <summary>
        /// Log a debug message for initialization/lifecycle operations.
        /// </summary>
        [Conditional("DEBUG")]
        public static void Init(string message)
        {
            if (!SPDA_DEBUG) return;
            StationpediaAscendedMod.Log?.LogInfo($"[SPDA-Init] {message}");
        }

        /// <summary>
        /// Start a timer for measuring an operation. Returns a Stopwatch (or null if debug disabled).
        /// Not [Conditional] because it returns a value - callers should null-check the result.
        /// </summary>
        public static Stopwatch StartTimer(string operationName)
        {
            if (!SPDA_DEBUG) return null;
            StationpediaAscendedMod.Log?.LogInfo($"[SPDA-Perf] START: {operationName}");
            return Stopwatch.StartNew();
        }

        /// <summary>
        /// Log the completion of a timed operation.
        /// </summary>
        [Conditional("DEBUG")]
        public static void StopTimer(string operationName, Stopwatch sw)
        {
            if (!SPDA_DEBUG || sw == null) return;
            sw.Stop();
            StationpediaAscendedMod.Log?.LogInfo($"[SPDA-Perf] END: {operationName} took {sw.ElapsedMilliseconds}ms");
        }
    }
}
