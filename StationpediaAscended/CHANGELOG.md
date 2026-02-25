# Changelog

All notable changes to Stationpedia Ascended will be documented in this file.

## [0.8.6] - 2026-02-24

### 🐛 Bug Fixes
- **Search: Cable Coil and other items missing** — `ShouldHideFromSearch` was checking the game's `HideInStationpedia` and `HiddenInPedia` flags, which incorrectly excluded legitimate items (Cable Coil, kit items, etc.) from the page index and search results. Vanilla search shows these items regardless of the flag; our filter now only hides ruptured/burnt/wreckage items
- **Search: "Starts With" not showing all matches** — Items like "Cable Coil" when searching "cable" were being excluded because `FindPageByTitle` couldn't match them (they were filtered from the index). Now all items appear correctly under "Starts With"
- **Search: "Corn" returning too many partial matches** — Partial word matches (e.g. "corner" matching "corn") are properly demoted to the bottom category instead of appearing alongside exact/whole-word matches
- **Header title not showing on first load** — "Stationpedia Ascended" header text now reliably appears on first Stationpedia open via ongoing monitor re-check
- **Logo icon overlapping text** — Phoenix logo enlarged to 36x36 (from 32x32), repositioned with +14px offset (from +8px), and scaled to 1.35x (from 1.2x)
- **Dual init path divergence** — ScriptEngine (F6) and SLP/Workshop paths now use identical initialization: all 10 Harmony patches, full monitor with header customization, Station Planner, search system, home page, and guide registration

### ⚡ Performance
- **Search: Single-pass bucketing** — Replaced 4 LINQ `Where().ToList()` calls + `Concat().GroupBy().OrderBy()` with single-pass bucketing into pre-allocated lists and `SortedDictionary` for category grouping
- **Search: ShouldHideFromSearch caching** — Results cached per page key in `_hideFromSearchCache` to avoid repeated `Regex.Replace` and string operations
- **Search: Pre-allocated result lists** — `ScoreResults` pre-allocates list capacity; category groups use `List.Sort()` instead of LINQ `OrderBy()`

### 🔧 Developer Tools
- **Centralized debug logging** — New `DebugLog` utility class with `SPDA_DEBUG` const toggle and `[Conditional("DEBUG")]` attribute; all debug output stripped from Release builds automatically
- **Search pipeline logging** — Debug builds log item hiding decisions, scoring results (priority/category per item), bucket sizes, and timing via `Stopwatch`
- **CoroutineHost property** — Path-agnostic coroutine management returns `Instance ?? _scriptEngineHost`

## [0.8.5] - 2026-02-05

### 🐛 Bug Fixes
- **Notepad hotkey changed from F2 to F4** — F2 conflicted with the game's built-in Helper Hints toggle; all references updated
- **Search results disappearing** — Results would vanish because `ReorganizeSearchResults` hid items when `FindPageByTitle()` couldn't match a page index; unmatched items now stay visible
- **Notepad showing raw JSON** — Saved files use formatted JSON with newlines but the deserializer only detected compact `{"` format; now checks for `{` AND `"lines"` anywhere
- **Guide button colors incorrect** — Custom guide buttons used `SetNormal()` (green/dark blue) instead of `SetSpecial()` which matches the vanilla light blue SpecialButton sprite
- **"Vanilla Guides" font mismatch** — Header text now copies font, fontSharedMaterial, fontSize, and fontStyle from `stationpedia.LoreGuideTitle`
- **Tooltips frozen when game paused** — `WaitForSeconds(HOVER_DELAY)` uses scaled time so tooltips never appeared at `Time.timeScale = 0`; switched to `WaitForSecondsRealtime()`
- **Survival Manual appearing on Guides page** — The manual has its own home page button but was also listed in guides; added skip for `guideKey == "SurvivalManual"`
- **Station Notepad input locking** — Keyboard input now properly locks while typing in the notepad, preventing game actions from firing
- **Station Notepad Enter key** — Enter key no longer opens the chat window while editing notes
- **Station Notepad hotload display** — JSON documents display correctly after hot-reloading the mod
- **JSON deserialization error** — Fixed `Cannot deserialize the current JSON object into type List<TableRow>` at `guides[5].OperationalDetails[6].children[0].table.headers`
- **Corrected gas terminology** — Removed incorrect X3/X1 gas naming throughout all guides; now uses proper names (Volatiles, Pollutant, Hydrogen)
- **Guides/mechanics display order** — Guides and game mechanics sections now display in correct JSON order instead of arbitrary order

### 📝 Content Additions
- **Species Survival Guide: Lung Damage & Suffocation** — New section with mechanics extracted from game code covering lung damage thresholds and suffocation behavior
- **Species Survival Guide: Mood & Hygiene** — New section covering mood thresholds, hygiene effects, and their impact on character stats

### 🖥️ Stationpedia Ascended IDE (New Desktop Editor)
A complete Electron + React desktop application for authoring and validating Stationpedia Ascended content, replacing the old WPF/C# editor:

- **Lossless JSON Round-Tripping** — AST-based TMP rich text parsing preserves original formatting through load/edit/save cycles
- **Game-Accurate Simulator** — HTML renderer matching in-game Stationpedia appearance with clickable TOC, collapsible sections, and mode toggle
- **Dual-Window Architecture** — Separate editor and simulator windows synced via Electron IPC
- **Dockable Panel System** — Resizable panels using react-mosaic for content tree, editor, properties, and preview
- **Drag-and-Drop Editing** — Reorder operational details sections with @dnd-kit; drag content between categories
- **Rich Text Editor** — TipTap-based editor with formatting toolbar for bold, italic, colors, headers, and lists
- **Content Tree Browser** — Organized tree view with Devices, Guides, and Mechanics tabs
- **Import JSON** — Import guide JSON files directly into the editor
- **Inline Section Headers** — Create non-collapsible header + paragraph blocks between nested sections
- **Global Tooltip Editor** — Browse and edit all tooltip categories (logic, slots, memory, modes, connections) with inline editing
- **Comprehensive Validation** — 8+ validation rules with real-time error reporting
- **Asset Browser** — Track and manage images/videos with usage tracking
- **Auto-Save with Backup** — Automatic saves with `.backups/` directory
- **Keyboard Shortcuts** — Ctrl+S save, Ctrl+Z undo, and more
- **Recent Files Menu** — Quick access to previously opened files
- **422 Tests Passing** — Full test suite covering parser, serializer, codec, and validation

### 🏗️ New Mod Architecture
- **ConsoleHelper** — Reflection-based `ConsoleWindow.Print` wrapper that auto-detects 4-param (stable) vs 5-param (beta/orbital) method signatures for cross-version compatibility
- **UIAssetInspector** — Debug tool showing detailed UI asset info under mouse cursor; toggle via `assetdisplay` console command
- **HomePageLayoutManager** — Creates "Stationeers Survival Manual" and "Game Mechanics" buttons on the home page with proper layout cloned from vanilla buttons
- **TocLinkHandler** — Table of Contents link click handling with smooth animated scrolling and parent chain expansion for nested sections
- **CategoryHeaderHandler** — Makes category header rows fully clickable to toggle visibility with hover effects
- **VanillaModeManager** — Toggle between Ascended mode (orange/blue) and Vanilla mode (native white); defaults to Vanilla
- **IconAnimator** — Animated icon transitions for expand/collapse with scale "pop" effect using `Time.unscaledDeltaTime`
- **SearchPatches** — Search result reorganization with visual category headers and smart grouping

### 🔧 Technical Changes
- **Removed "Flat Structure" concept** — "Operational Details" is now a normal renameable category
- **Section title defaults** — New sections use white text by default; orange handled by Ascended mode via `VanillaModeManager`
- **Embedded resources in Release** — `descriptions.json` and phoenix icon embedded in DLL for release builds
- **Cross-version console compatibility** — Mod works across both stable and beta/orbital Stationeers versions

---

## [0.8.0] - 2026-01-18

### 📝 Station Notepad - In-Game Note Taking
A complete note-taking system integrated into Stationpedia:
- **Multi-Folder Organization** - Create folders and subfolders to organize notes
- **Global & Per-Save Notes** - Notes can persist across all saves or be save-specific
- **Rich Text Formatting** - Bold, italic, headers, colors, bullet points
- **Link to Stationpedia** - Use `{LINK:DeviceKey;Display Text}` to create clickable links to any Stationpedia page
- **Block Editor Mode** - Visual block-based editing with drag-and-drop
- **Text Editor Mode** - Direct markdown-style text editing
- **Auto-Save** - Notes save automatically when switching or closing
- **Keyboard Shortcuts** - F2 to toggle, Escape to close
- **Formatting Toolbar** - Quick access to bold, italic, headers, colors, and lists

### 📚 Massive Content Expansion
- **Survival Manual** - Complete multi-part beginner's guide covering first 5 minutes to advanced systems
- **Power Systems Guide** - Comprehensive guide to electrical networks, solar, batteries, and troubleshooting
- **Airlock Guide** - Basic and advanced airlock setup with component lists and configuration
- **Air Conditioning Guide** - From portable AC safety to full climate control systems
- **Daylight Sensor Guide** - Solar tracking and automation
- **Species Survival Guide (Game Mechanic)** - Human, Zrilian, and Robot survival requirements from game code
- **Complete Smelting Guide (Game Mechanic)** - All furnace types, recipes, temperatures, and alloy creation including Ice Only Recipes and Gaseous Fuel Recipes tables

### 🎨 Table of Contents Enhancements
- **Nested TOC Support** - Multi-level table of contents with collapsible sections
- **Custom TOC Bullets** - Visual hierarchy with different bullet styles for nested items
- **Smooth Scrolling** - Click TOC items to smoothly scroll to sections
- **Auto-Generated IDs** - `tocId` property for precise section linking

### 🎬 Media Support
- **Video Embedding** - Play MP4 videos directly in Stationpedia pages using `videoFile` property
- **Image Support** - Inline images with `imageFile` property
- **YouTube Links** - Clickable YouTube links that open in browser

### 🔄 Vanilla Mode & Ascended Mode
- **Vanilla by Default** - Mod starts in vanilla Stationpedia styling
- **Easter Egg Toggle** - Click the "Stationpedia" header to toggle Ascended mode
- **Visual Distinction** - Ascended mode shows orange styling and phoenix icon
- **Mode Persistence** - Your mode preference is remembered

### 📋 New JSON Schema Features
**Device-Level Properties:**
- `flatStructure` - Render sections without nesting for simpler guides
- `pageImage` - Display image at top of page
- `buttonColor` - Custom color for guide buttons (blue, orange, green, etc.)
- `sortOrder` - Control order of guides in the category list

**OperationalDetail Properties:**
- `children` - Nested subsections for hierarchical content
- `steps` - Numbered step lists for procedures
- `items` - Bullet point lists
- `table` - Data tables with rows and cells

### 🛠️ Technical Improvements
- **JSON Mechanics Loader** - Separate loading system for game mechanics guides
- **Guide Loader System** - Modular guide loading with support for manuals, guides, and mechanics
- **Home Page Layout Manager** - Custom home page with categorized guide buttons
- **Icon Animator** - Smooth icon transitions for UI elements

### 🐛 Bug Fixes
- Fixed JSON BOM character causing parse errors
- Fixed guide key detection for mixed guideKey/deviceKey schemas
- Improved search indexing for new content types
- Fixed Pause stopping tooltips from appearing

---

## [0.3.0] - 2025-12-31

### 🤖 Comprehensive AIMeE Documentation
- **Full AIMeE Guide** - Added extensive documentation for the AIMeE (ThingRobot) companion robot
- **Mode Explanations** - Detailed descriptions for all AIMeE modes: None, Follow, MoveToTarget, Roam, Unload, PathToTarget, StorageFull
- **IC10 Programming Guide** - Complete guide for programming AIMeE with example code snippets
- **Navigation & Mining** - Documented pathfinding behavior, mining mechanics, and coordinate systems
- **Table of Contents** - Added navigable TOC with clickable links to each section

### 📑 Multi-Column Table of Contents
- **Column Layout** - TOC now displays in columns (max 8 rows per column) instead of a long vertical list
- **Dynamic Columns** - Automatically creates additional columns when entries exceed 8
- **Improved Readability** - Better use of horizontal space for guides with many sections

### 📋 New JSON Schema Features
New properties for building rich, interactive documentation:

**Device-Level Properties:**
- `generateToc` - (boolean) Enable Table of Contents panel at top of Operational Details
- `tocTitle` - (string) Custom title for TOC panel (default: "Contents")
- `operationalDetailsBackgroundColor` - (string) Hex color for section backgrounds

**OperationalDetail Properties:**
- `collapsible` - (boolean) Render as expandable/collapsible section with header bar
- `tocId` - (string) Unique ID for TOC linking; clicking scrolls to section
- `imageFile` - (string) Display inline image from mod/images/ folder
- `videoFile` - (string) Embed MP4 video player from mod/images/ folder
- `youtubeUrl` - (string) Clickable YouTube link that opens in browser
- `youtubeLabel` - (string) Custom label for YouTube link
- `backgroundColor` - (string) Custom hex color for individual section

### 🎨 Vanilla Mode Default & Easter Egg Toggle
- **Vanilla by Default** - Mod now starts in vanilla Stationpedia styling mode
- **Header Toggle** - Click the "Stationpedia" header to toggle Ascended mode (easter egg)
- **Visual Feedback** - Header changes to "Stationpedia Ascended" with orange styling when enabled
- **Icon Swap** - Custom phoenix icon only appears in Ascended mode
- **Removed Book Button** - Simplified UI by removing the mode toggle button

### 🔧 Bug Fixes
- **Fixed JSON Syntax Error** - Resolved corrupted line in descriptions.json that broke tooltip parsing
- **Fixed Brace Mismatch** - Corrected code structure issues in icon replacement logic

### 📝 New Files
- `VanillaModeManager.cs` - Static manager for vanilla/ascended mode state
- `CategoryHeaderHandler.cs` - Handler for category header interactions
- `LLM_INSTRUCTIONS.txt` - Completely rewritten documentation for JSON schema

---

## [0.2.2] - 2025-12-30

### ⚡ Search Performance Optimizations
- **O(1) Title Index Lookups** - Built cached title/word indexes for instant lookups instead of O(n) full scans
- **Reduced Search Latency** - Replaced 1-second polling loop with 2-frame delay for near-instant results
- **Category Cache** - Added category lookup cache to avoid triple-nested loops during reorganization
- **Template Caching** - Cached font, sprite, and color references to avoid repeated lookups

### 🏷️ New Property Tooltips
Hover over gas, material, and device properties to see detailed explanations:
- **Gas/Material Properties**: Flashpoint, Autoignition, Thermal Convection, Thermal Radiation, Solar Heating, Specific Heat, Freeze Temperature, Boiling Temperature, Max Liquid Temperature, Min Liquid Pressure, Latent Heat, Moles Per Litre
- **Device Properties**: Max Pressure, Volume, Base Power, Power Storage, Power Generation
- **Plant/Food Properties**: Growth Time, Nutrition, Nutrition Quality, Mood Bonus
- **Rocket Properties**: Placeable In Rocket, Rocket Mass

### 🎮 IC10 Helper Tooltips
- **Prefab Name Tooltip** - Shows full name (in case truncated), "click to copy" instruction, and IC10 usage example
- **Prefab Hash Tooltip** - Shows hash value with IC10 code example (`lb r0 <hash> Setting`)
- Explains that Prefab Name and Hash are interchangeable for IC10 programming

### 🔧 Bug Fixes & Polish
- **Prefab Name Truncation** - Fixed truncation to prevent overlap with Stack Size field
- **Removed Debug Spam** - Cleaned up console logs by removing verbose debug output
- **New Tooltip Classes** - Added `SPDAPrefabInfoTooltip` and `SPDAPropertyTooltip`

---

## [0.2.1] - 2025-12-29

### ✨ Enhanced Search Results
- **Smart Search Organization** - Search results are now intelligently grouped by relevance:
  - **Exact Matches** - Items where the title exactly matches your search (orange header)
  - **Starts With** - Items where the title begins with your search term (gold header)
  - **Category Groups** - Remaining results organized by their Stationpedia category (white headers)
  
- **Visual Header Styling**
  - Beautiful blue-bordered header bars using the game's "special" UI sprite
  - Darkened background for better contrast with colored text
  - Headers are 54px tall with 19px bold text for clear visibility
  
- **Debris Filtering** - Automatically filters out burnt cables, wreckage, and ruptured variants from search results to reduce clutter

- **Priority Scoring System** - Results are scored and sorted by:
  1. Exact title match (highest priority)
  2. Title starts with search term
  3. Title contains search term
  4. Description contains search term (lowest priority)

---

## [0.1.3] - 2025-12-28

### 🔧 Build & Development Fixes
- **Fixed PDB symbol format** - Changed from `portable` to `embedded` format to resolve Mono.Cecil hot-reload exceptions (`SymbolsNotMatchingException` and `MarshalDirectiveException`)
- **Updated build task** - Simplified deployment to only copy DLL and descriptions.json (PDB now embedded in DLL)

### 🏗️ Code Organization
- **Added region markers** - Organized main StationpediaAscended.cs file with 12 collapsible regions for easier navigation:
  - Constants & Static References
  - Instance Fields
  - ScriptEngine Hot-Reload Support
  - Unity Lifecycle Methods
  - GUI Rendering
  - Resource Loading
  - Harmony Patching
  - Console Commands
  - Static ScriptEngine Methods
  - Tooltip Adding
  - Description Lookup Helpers
  - Cleanup

### 📝 Documentation
- Added XML documentation comments to major methods

---

## [0.1.2] - 2025-12-28

### 🏗️ Major Code Refactoring
Restructured the codebase from a single 2,535-line monolithic file into a clean multi-file architecture for improved maintainability and navigation.

#### New File Structure
- **`src/Data/Models.cs`** - All 9 JSON data model classes (`DescriptionsRoot`, `DeviceDescriptions`, `LogicDescription`, `ModeDescription`, `SlotDescription`, `VersionDescription`, `MemoryDescription`, `OperationalDetail`, `GenericDescriptionsData`)
- **`src/Tooltips/SPDABaseTooltip.cs`** - Abstract base class with shared tooltip functionality (hover delay, pointer events, positioning)
- **`src/Tooltips/SPDALogicTooltip.cs`** - Logic type tooltip component
- **`src/Tooltips/SPDASlotTooltip.cs`** - Slot tooltip component
- **`src/Tooltips/SPDAVersionTooltip.cs`** - Version tooltip component  
- **`src/Tooltips/SPDAMemoryTooltip.cs`** - Memory/register tooltip component
- **`src/Patches/HarmonyPatches.cs`** - All Harmony patch methods (`PopulateLogicSlotInserts_Postfix`, `ChangeDisplay_Postfix`, `OnDrag_Prefix`, `OnBeginDrag_Prefix`) plus `CreateOperationalDetailsCategory` helper
- **`src/Core/TooltipState.cs`** - Centralized tooltip visibility state management

#### Code Improvements
- Reduced main file (`StationpediaAscended.cs`) from 2,535 lines to ~1,500 lines (43% reduction)
- Eliminated code duplication in tooltip classes via shared base class
- Improved separation of concerns between data models, UI components, and patches
- Added proper namespace organization (`StationpediaAscended.Data`, `StationpediaAscended.Tooltips`, `StationpediaAscended.Patches`, `StationpediaAscended.Core`)

### 🐛 Bug Fixes
- **Fixed JSON deserialization for OperationalDetails** - Added `[JsonProperty]` attributes to handle case-sensitivity between JSON (`"OperationalDetails"`) and C# (`operationalDetails`)
- **Fixed descriptions.json path resolution** - Updated hardcoded development paths from old `StationpediaPlus` folder to `StationpediaAscended`
- **Fixed null reference in path resolution** - Added null-safety for `Path.GetDirectoryName()` which can return null
- **Fixed VS Code build task** - Updated `.vscode/tasks.json` to use correct project name and paths

### 🔧 Technical Changes
- Renamed `StationpediaAscended.Harmony` namespace to `StationpediaAscended.Patches` to avoid conflict with `HarmonyLib.Harmony`
- Added `using Newtonsoft.Json;` import to Models.cs for JsonProperty attributes
- Improved console logging during JSON path search for easier debugging
- Build task now correctly deploys `StationpediaAscended.dll` instead of `StationpediaPlus.dll`

---

## [0.1.0-beta] - 2025-12-28

### 🎉 Initial Release (Beta)
- **Renamed** from StationpediaPlus to **Stationpedia Ascended**
- **Custom Phoenix Icon** - Replaced the original book icon with a custom phoenix logo in the header

### ✨ New Features
- **Enhanced Tooltips System**
  - Added comprehensive tooltips for logic descriptions
  - Added slot-specific tooltips with detailed information
  - Added memory/register tooltips explaining functionality
  - Added mode descriptions for device operations
  - Added connection type explanations
  - JSON-based configuration system for easy customization

- **Operational Details Section**
  - Added dedicated "Operational Details" category that appears at the top of device pages
  - Configurable title color via JSON configuration
  - Phoenix icon displayed next to the category title
  - Defaults to collapsed state for cleaner initial view
  - Automatically positioned after the main description section

- **Page Description Customization**
  - Added ability to completely replace page descriptions
  - Added `pageDescriptionAppend` for adding content after existing descriptions
  - Added `pageDescriptionPrepend` for adding content before existing descriptions
  - Full JSON configuration support via `descriptions.json`

### 🐛 Bug Fixes
- **Fixed Critical Scrollbar Handle Bug**
  - Resolved issue where scrollbar handles would disappear on non-home Stationpedia pages
  - Implemented 5-frame delayed fix to combat handle position corruption
  - Handle `localPosition` and `anchoredPosition` now properly reset to zero
  - Bug affects base game - created detailed bug report for developers

- **Fixed Window Dragging Crash**
  - Resolved crash when dragging Stationpedia window in main menu
  - Fixed `ClampToScreen()` null reference exception due to missing `InventoryManager.ParentHuman`
  - Replaced problematic dragging logic with simple position assignment

### 🔧 Technical Improvements
- **Harmony Integration**
  - Patches `ChangeDisplay` method for enhanced functionality
  - Patches `PopulateLogicSlotInserts` for tooltip integration
  - Patches `OnDrag` and `OnBeginDrag` for improved window handling
  - Hot-reload support for development workflow

- **Performance Optimizations**
  - Coroutine-based monitoring system for Stationpedia state
  - Efficient component cleanup on mod reload
  - Delayed tooltip application to prevent UI conflicts

- **Developer Experience**
  - ScriptEngine hot-reload compatibility (F6 reload support)
  - Comprehensive debug logging system
  - Automatic file path detection for development and production environments

### 📝 Documentation
- Created comprehensive bug report document for game developers
- Added detailed JSON configuration examples
- Documented all tooltip categories and customization options

### 🎨 UI/UX Improvements
- **Custom Branding**
  - Window title changed to "Stationpedia Ascended" with orange accent color
  - Phoenix icon properly sized and positioned in header
  - Maintains original UI layout and responsiveness

- **Enhanced Information Display**
  - Tooltips appear on hover with orange border styling
  - Consistent formatting across all tooltip categories
  - Non-intrusive design that complements existing UI

### ⚙️ Configuration
- **descriptions.json Structure**
  ```json
  {
    "genericDescriptions": {
      "logic": { "key": "description" },
      "slots": { "key": "description" },
      "memory": { "key": "description" },
      "modes": { "key": "description" },
      "connections": { "key": "description" }
    },
    "devices": {
      "DevicePageKey": {
        "operationalDetails": [...],
        "operationalDetailsTitleColor": "#FF7A18",
        "pageDescription": "Complete replacement",
        "pageDescriptionAppend": "Added after existing",
        "pageDescriptionPrepend": "Added before existing"
      }
    }
  }
  ```

### 🔄 Compatibility
- Compatible with Stationeers current version
- Works with both BepInEx and ScriptEngine loading methods
- Hot-reload support for development
- No conflicts with existing Stationpedia functionality

---

*Note: This is a beta release. Please report any issues or feedback on the GitHub repository.*