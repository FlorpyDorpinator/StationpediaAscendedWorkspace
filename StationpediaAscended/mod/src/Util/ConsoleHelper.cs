using System;
using System.Reflection;
using Assets.Scripts;
using UnityEngine;

namespace StationpediaAscended
{
    /// <summary>
    /// Helper class to handle ConsoleWindow.Print across different game versions.
    /// The beta/orbital update added a new 'unformatted' parameter, breaking mods compiled against stable.
    /// This class auto-detects which version is available and uses the appropriate method.
    /// </summary>
    public static class ConsoleHelper
    {
        private static bool _initialized = false;
        private static MethodInfo _printMethod = null;
        private static bool _hasUnformattedParam = false;
        
        /// <summary>
        /// Print a message to the F3 console, compatible with both stable and beta game versions.
        /// Falls back to Debug.Log if neither method works.
        /// </summary>
        public static void Print(string message, ConsoleColor color = ConsoleColor.White)
        {
            if (!_initialized)
            {
                Initialize();
            }
            
            try
            {
                if (_printMethod != null)
                {
                    if (_hasUnformattedParam)
                    {
                        // New signature: (string, ConsoleColor, bool, bool, bool)
                        _printMethod.Invoke(null, new object[] { message, color, false, true, false });
                    }
                    else
                    {
                        // Old signature: (string, ConsoleColor, bool, bool)
                        _printMethod.Invoke(null, new object[] { message, color, false, true });
                    }
                }
                else
                {
                    // Fallback to Debug.Log
                    Debug.Log(message);
                }
            }
            catch (Exception)
            {
                // If invocation fails, fall back to Debug.Log
                Debug.Log(message);
            }
        }
        
        private static void Initialize()
        {
            _initialized = true;
            
            try
            {
                var consoleWindowType = typeof(ConsoleWindow);
                
                // Try to find the new 5-parameter method first (beta/orbital update)
                _printMethod = consoleWindowType.GetMethod("Print", 
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { typeof(string), typeof(ConsoleColor), typeof(bool), typeof(bool), typeof(bool) },
                    null);
                
                if (_printMethod != null)
                {
                    _hasUnformattedParam = true;
                    Debug.Log("[Stationpedia Ascended] Detected new ConsoleWindow.Print (5 params - beta/orbital)");
                    return;
                }
                
                // Fall back to old 4-parameter method (stable)
                _printMethod = consoleWindowType.GetMethod("Print",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { typeof(string), typeof(ConsoleColor), typeof(bool), typeof(bool) },
                    null);
                
                if (_printMethod != null)
                {
                    _hasUnformattedParam = false;
                    Debug.Log("[Stationpedia Ascended] Detected old ConsoleWindow.Print (4 params - stable)");
                    return;
                }
                
                Debug.LogWarning("[Stationpedia Ascended] Could not find ConsoleWindow.Print method, using Debug.Log fallback");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Stationpedia Ascended] Error detecting ConsoleWindow.Print: {ex.Message}");
                _printMethod = null;
            }
        }
    }
}
