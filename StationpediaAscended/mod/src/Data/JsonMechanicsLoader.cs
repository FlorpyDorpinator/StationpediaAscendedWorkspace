using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.UI;
using UnityEngine;

namespace StationpediaAscended.Data
{
    /// <summary>
    /// Loads and manages Game Mechanics pages from descriptions.json.
    /// These pages appear under the "Game Mechanics" button in Stationpedia.
    /// </summary>
    public static class JsonMechanicsLoader
    {
        private static Dictionary<string, GuideDescription> _loadedMechanics = new Dictionary<string, GuideDescription>();
        private static List<GuideDescription> _orderedMechanics = new List<GuideDescription>(); // Preserves JSON order
        private static bool _mechanicsRegistered = false;

        /// <summary>
        /// Load mechanics from the parsed DescriptionsRoot data
        /// </summary>
        public static void LoadMechanics(DescriptionsRoot data)
        {
            _loadedMechanics.Clear();
            _orderedMechanics.Clear();

            if (data?.mechanics == null || data.mechanics.Count == 0)
            {
                ConsoleHelper.Print("[Stationpedia Ascended] No game mechanics found in descriptions.json");
                return;
            }

            foreach (var mechanic in data.mechanics)
            {
                // Headers use headerKey instead of guideKey
                string key = mechanic.isHeader ? mechanic.headerKey : mechanic.guideKey;
                
                if (string.IsNullOrEmpty(key))
                {
                    ConsoleHelper.Print("[Stationpedia Ascended] Skipping mechanic/header with no key");
                    continue;
                }

                _loadedMechanics[key] = mechanic;
                _orderedMechanics.Add(mechanic); // Preserve JSON array order
                
                if (mechanic.isHeader)
                {
                    ConsoleHelper.Print($"[Stationpedia Ascended] Loaded section header: {mechanic.headerTitle}");
                }
                else
                {
                    ConsoleHelper.Print($"[Stationpedia Ascended] Loaded mechanic: {mechanic.displayName ?? mechanic.guideKey}");
                }
            }

            ConsoleHelper.Print($"[Stationpedia Ascended] Loaded {_loadedMechanics.Count} game mechanics/headers from JSON");
        }

        /// <summary>
        /// Register all loaded mechanics as Stationpedia pages and with GameMechanicsRegistry
        /// </summary>
        public static void RegisterMechanicsPages()
        {
            if (_mechanicsRegistered) return;

            // First, register the GameMechanics index page
            RegisterGameMechanicsIndexPage();

            foreach (var kvp in _loadedMechanics)
            {
                try
                {
                    var mechanic = kvp.Value;
                    
                    // Skip headers - they don't have pages
                    if (mechanic.isHeader) continue;
                    
                    // Create a StationpediaPage for this mechanic
                    var page = new StationpediaPage
                    {
                        Key = mechanic.guideKey,
                        Title = mechanic.displayName ?? mechanic.guideKey
                    };

                    // Set introductory text from pageDescription
                    page.Text = mechanic.pageDescription ?? "";

                    // Register the page with Stationpedia
                    Stationpedia.Register(page, false);

                    // Also register with GameMechanicsRegistry so it shows up in the listing
                    GameMechanicsRegistry.RegisterPage(mechanic.guideKey);

                    ConsoleHelper.Print($"[Stationpedia Ascended] Registered mechanic page: {mechanic.guideKey}");
                }
                catch (Exception ex)
                {
                    StationpediaAscendedMod.Log?.LogError($"Error registering mechanic {kvp.Key}: {ex.Message}");
                }
            }

            _mechanicsRegistered = true;
        }

        /// <summary>
        /// Register the GameMechanics index page that shows all available mechanics
        /// </summary>
        private static void RegisterGameMechanicsIndexPage()
        {
            try
            {
                var page = new StationpediaPage
                {
                    Key = "GameMechanics",
                    Title = "Game Mechanics"
                };

                // Build a list of available mechanics as clickable links
                var mechanicLinks = new List<string>();
                foreach (var mechanic in _orderedMechanics)
                {
                    if (mechanic.isHeader)
                    {
                        // Add header as non-clickable section title
                        mechanicLinks.Add($"\n<b><color=#FFA500>{mechanic.headerTitle}</color></b>");
                    }
                    else
                    {
                        // Add clickable link to mechanic page (link ID = page key for navigation)
                        string displayName = mechanic.displayName ?? mechanic.guideKey;
                        mechanicLinks.Add($"  • <link={mechanic.guideKey}><color=#88CCFF>{displayName}</color></link>");
                    }
                }

                page.Text = "Welcome to the Game Mechanics section of Stationpedia Ascended.\n\n" +
                           "This section contains detailed documentation about how various game systems work.\n\n" +
                           "<b>Available Topics:</b>\n" +
                           string.Join("\n", mechanicLinks);

                Stationpedia.Register(page, false);
                ConsoleHelper.Print("[Stationpedia Ascended] GameMechanics index page registered");
            }
            catch (Exception ex)
            {
                StationpediaAscendedMod.Log?.LogError($"Error registering GameMechanics index page: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all loaded mechanics in JSON array order (preserves position for headers)
        /// </summary>
        public static List<GuideDescription> GetAllMechanics()
        {
            return _orderedMechanics.ToList();
        }

        /// <summary>
        /// Get a specific mechanic by key
        /// </summary>
        public static GuideDescription GetMechanic(string mechanicKey)
        {
            if (_loadedMechanics.TryGetValue(mechanicKey, out var mechanic))
            {
                return mechanic;
            }
            return null;
        }

        /// <summary>
        /// Check if a mechanic exists
        /// </summary>
        public static bool HasMechanic(string mechanicKey)
        {
            return _loadedMechanics.ContainsKey(mechanicKey);
        }

        /// <summary>
        /// Convert a mechanic (GuideDescription) to DeviceDescriptions for rendering
        /// </summary>
        public static DeviceDescriptions ToDeviceDescriptions(GuideDescription mechanic)
        {
            if (mechanic == null) return null;

            return new DeviceDescriptions
            {
                deviceKey = mechanic.guideKey,
                displayName = mechanic.displayName,
                pageDescription = mechanic.pageDescription,
                pageDescriptionPrepend = mechanic.pageDescriptionPrepend,
                pageDescriptionAppend = mechanic.pageDescriptionAppend,
                pageImage = mechanic.pageImage,
                operationalDetails = mechanic.operationalDetails,
                operationalDetailsTitleColor = mechanic.operationalDetailsTitleColor,
                generateToc = mechanic.generateToc,
                tocTitle = mechanic.tocTitle,
                tocFlat = mechanic.tocFlat,
                operationalDetailsBackgroundColor = mechanic.operationalDetailsBackgroundColor
            };
        }

        /// <summary>
        /// Clear all loaded mechanics (for hot-reload)
        /// </summary>
        public static void Clear()
        {
            _loadedMechanics.Clear();
            _mechanicsRegistered = false;
        }

        /// <summary>
        /// Parse button color from mechanic settings
        /// </summary>
        public static Color GetButtonColor(GuideDescription mechanic)
        {
            if (mechanic == null || string.IsNullOrEmpty(mechanic.buttonColor))
            {
                // Default to blue for game mechanics
                return new Color(0f, 0.54f, 0.90f, 1f); // #008AE6
            }

            var colorStr = mechanic.buttonColor.ToLower();
            
            if (colorStr == "blue")
            {
                return new Color(0f, 0.54f, 0.90f, 1f); // #008AE6
            }
            else if (colorStr == "orange")
            {
                return new Color(1f, 0.42f, 0.09f, 1f); // #FF6A18
            }
            else if (colorStr == "green")
            {
                return new Color(0.27f, 0.68f, 0.51f, 1f); // #44AD83
            }
            else if (colorStr.StartsWith("#"))
            {
                // Try to parse hex color
                if (ColorUtility.TryParseHtmlString(colorStr, out var parsedColor))
                {
                    return parsedColor;
                }
            }

            // Default to blue
            return new Color(0f, 0.54f, 0.90f, 1f);
        }
    }
}
