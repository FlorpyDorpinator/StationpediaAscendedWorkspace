## Plan: Game-Accurate Stationpedia Simulator

Build a simulator window that replicates the in-game Stationpedia UI with category navigation, game assets, and accurate styling for previewing device documentation.

**Phases: 5**

1. **Phase 1: Game Assets & CSS Foundation**
    - **Objective:** Copy game UI assets to project and create game-accurate CSS theming
    - **Files/Functions to Modify/Create:**
      - Create `src/renderer/simulator/assets/` folder with copied PNG assets
      - Create `src/renderer/simulator/styles/stationpedia-game.css` with game-accurate styles
      - Update `vite.config.ts` to include assets in build
    - **Tests to Write:**
      - `stationpedia-game.css loads without errors`
      - `game assets are served correctly in dev mode`
    - **Steps:**
      1. Create assets folder and copy UI PNGs from AssetRipperFiles/ExportedProject/Assets/Texture2D/
      2. Write CSS that uses these assets for backgrounds, buttons, icons
      3. Run tests to verify assets load correctly
      4. Verify visual appearance matches game styling

2. **Phase 2: Category Mapping System**
    - **Objective:** Create a categorization system to group devices by type (Ores, Ingots, Fabricators, etc.)
    - **Files/Functions to Modify/Create:**
      - Create `src/renderer/simulator/categoryMapping.ts` with category definitions and device mappings
      - Create `src/renderer/simulator/utils/categorizeDevices.ts` utility functions
    - **Tests to Write:**
      - `categoryMapping.test.ts: maps known devices to correct categories`
      - `categorizeDevices.test.ts: handles unknown devices gracefully`
      - `categorizeDevices.test.ts: returns all 14 categories`
    - **Steps:**
      1. Define Category interface and CATEGORIES constant with all 14 categories
      2. Implement prefix-based auto-categorization (ThingOre*, ThingIngot*, etc.)
      3. Add manual overrides for edge cases
      4. Write getCategorizedDevices() function
      5. Run tests to verify categorization works correctly

3. **Phase 3: Home Screen with Category Grid**
    - **Objective:** Build the main home screen with category tiles matching in-game appearance
    - **Files/Functions to Modify/Create:**
      - Create `src/renderer/simulator/components/HomeScreen.tsx` with category grid
      - Create `src/renderer/simulator/components/CategoryTile.tsx` for individual tiles
      - Create `src/renderer/simulator/components/TabBar.tsx` for Guides/Universe tabs
      - Update `src/renderer/simulator/SimulatorApp.tsx` to use new navigation
    - **Tests to Write:**
      - `HomeScreen.test.tsx: renders all 14 category tiles`
      - `CategoryTile.test.tsx: displays icon and count`
      - `TabBar.test.tsx: switches between Guides and Universe`
    - **Steps:**
      1. Create TabBar component with Guides/Universe tabs
      2. Create CategoryTile component with icon, name, device count
      3. Create HomeScreen with responsive category grid
      4. Update SimulatorApp to show HomeScreen when no device selected
      5. Implement navigation from home → category → device
      6. Run tests to verify components render correctly

4. **Phase 4: Category List & Navigation**
    - **Objective:** Build the category list view showing devices within a category
    - **Files/Functions to Modify/Create:**
      - Create `src/renderer/simulator/components/CategoryListView.tsx`
      - Create `src/renderer/simulator/components/DeviceListItem.tsx`
      - Update `src/renderer/simulator/components/NavigationBar.tsx` with home button, breadcrumbs
      - Update `src/renderer/simulator/SimulatorApp.tsx` with view routing
    - **Tests to Write:**
      - `CategoryListView.test.tsx: shows devices in selected category`
      - `DeviceListItem.test.tsx: displays device name and navigates on click`
      - `NavigationBar.test.tsx: home button returns to category grid`
    - **Steps:**
      1. Create DeviceListItem with game-accurate styling
      2. Create CategoryListView showing grid/list of devices
      3. Add home button to NavigationBar
      4. Implement breadcrumb navigation (Home > Category > Device)
      5. Add view routing in SimulatorApp (home/category/device views)
      6. Run tests to verify navigation flow works

5. **Phase 5: Device Detail Polish & Search**
    - **Objective:** Polish the device detail view to match in-game and add global search
    - **Files/Functions to Modify/Create:**
      - Update `src/renderer/simulator/StationpediaRenderer.tsx` with game-accurate styling
      - Create `src/renderer/simulator/components/SearchBar.tsx`
      - Update `src/renderer/simulator/components/DeviceHeader.tsx` with game assets
      - Update collapsible sections with game-accurate expand/collapse icons
    - **Tests to Write:**
      - `SearchBar.test.tsx: filters devices across all categories`
      - `StationpediaRenderer.test.tsx: renders with game styling`
    - **Steps:**
      1. Update DeviceHeader with game icon styling
      2. Update CollapsibleSection with game expand/collapse icons
      3. Create SearchBar with search icon and dropdown results
      4. Add search to NavigationBar
      5. Polish animations and transitions
      6. Run final integration tests
      7. Verify visual match with in-game screenshots

**Open Questions:**
1. Should categories show device count badges? Yes - helps users know content size
2. Should search results show category breadcrumb? Yes - improves discoverability
3. Order of categories - alphabetical or game order? Game order preferred
