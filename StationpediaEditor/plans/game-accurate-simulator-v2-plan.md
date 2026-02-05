## Plan: Game-Accurate Stationpedia Simulator V2

Rebuild the simulator to exactly replicate the in-game Stationpedia UI using actual game assets, 9-slice CSS, and matching the Unity component hierarchy discovered in Assembly-CSharp.

**TL;DR:** Create an exact visual replica of the in-game Stationpedia that allows editing operational details, tooltips, guides, and game mechanics pages for the StationpediaAscended mod.

**Phases: 4**

---

### Phase 1: 9-Slice CSS Foundation & Assets

**Objective:** Build CSS foundation using game sprites with proper 9-slice borders

**Files to Create:**
- `src/renderer/simulator/styles/game-ui.css` - 9-slice CSS classes
- `src/renderer/simulator/assets/ui/` - Copy all UI sprites with known borders

**Key Assets & 9-Slice Values (from Unity .meta files):**
| Asset | Border (x,y,z,w = left,bottom,right,top) |
|-------|-------------------------------------------|
| `window-bg.png` | 7, 7, 7, 7 |
| `dialog-bg.png` | 18, 18, 18, 18 |
| `button-bg.png` | 7, 7, 7, 7 |
| `button-normal.png` | 18, 22, 22, 17 |
| `button-hover.png` | 5, 5, 5, 5 |
| `inv-window-bg.png` | 16, 16, 16, 16 |
| `list-item-hover.png` | 50, 50, 50, 50 |
| `scrollbar-handle.png` | 10, 10, 10, 10 |

**CSS border-image Pattern:**
```css
.game-button {
  border: 18px solid transparent;
  border-image: url('./assets/ui/button-normal.png') 18 22 22 17 fill / 18px 22px 22px 17px;
}
```

**Game Colors (from mod code):**
- Background: `#0F1F38` (StationeersBlueFull)
- Border: `#264D73` (StationeersBlueBorder)  
- Orange Accent: `#FF7A18`
- Link Blue: `#008AE6`
- Tooltip BG: `#0D1926`
- White Text: `#FFFFFF`

**Steps:**
1. Copy all UI sprites to `assets/ui/`
2. Create CSS classes with border-image for each sprite
3. Create CSS variables for game colors
4. Test 9-slice rendering in browser

---

### Phase 2: Accurate Home Screen

**Objective:** Replicate the exact home screen layout from screenshots

**Files to Create/Modify:**
- `src/renderer/simulator/components/GameHomeScreen.tsx`
- `src/renderer/simulator/components/GameSearchBar.tsx`
- `src/renderer/simulator/components/GameCategoryButton.tsx`
- `src/renderer/simulator/components/GameNavBar.tsx`

**Layout Structure (from screenshots):**
```
┌─────────────────────────────────────────────┐
│ [🏠] [<] [>]  🔥 Stationpedia Ascended  [icons] │  ← NavBar
├─────────────────────────────────────────────┤
│ [🔍 Search                                ] │  ← SearchBar
├─────────────────────────────────────────────┤
│ [Stationeers Survival Manual] [Game Mechanics] │  ← Orange/Blue buttons
├─────────────────────────────────────────────┤
│ [📖 Guides              ] [🌌 Universe      ] │  ← Tab buttons
├─────────────────────────────────────────────┤
│ [⛏ Ores        ] [🧱 Ingots         ]      │
│ [🏭 Fabricators ] [📦 Structure Kits ]      │
│ [💨 Gases       ] [🧪 Reagents       ]      │  ← Category list
│ [🌡 Atmospherics] [⚡ Electronics    ]      │     (2 columns)
│ [🔌 Logic Dev.  ] [🏗 Structures     ]      │
│ ... etc                                     │
└─────────────────────────────────────────────┘
```

**Category List (18 total from screenshot):**
1. Ores, Ingots
2. Fabricators, Structure Kits
3. Gases, Reagents
4. Atmospherics, Electronics
5. Logic Devices, Structures
6. Organics and Food, Rockets
7. Genetics, Trading
8. Hand Tools, Cartridges
9. Personal, Furniture

**Button Styling:**
- Orange buttons: "Stationeers Survival Manual", "Guides"
- Blue buttons: "Game Mechanics", "Universe"
- Dark gray with border: Category list items (icons on left)

**Steps:**
1. Create NavBar with game-accurate icons
2. Create SearchBar matching game style
3. Create main section buttons (orange/blue)
4. Create category list with proper icons
5. Wire up navigation

---

### Phase 3: Device Page Renderer (UniversalPage)

**Objective:** Replicate the device detail page exactly as shown in Satellite Dish screenshot

**Files to Create/Modify:**
- `src/renderer/simulator/components/GameDevicePage.tsx`
- `src/renderer/simulator/components/GameCollapsibleSection.tsx`
- `src/renderer/simulator/components/GameLogicItem.tsx`
- `src/renderer/simulator/components/GameRichText.tsx`

**Page Structure (from UniversalPage.cs + screenshot):**
```
┌─────────────────────────────────────────────┐
│ [🔍 dish                                  ] │  ← Search with query
├─────────────────────────────────────────────┤
│           [Device Image]                    │
│                                             │
│         Medium Satellite Dish               │  ← Title centered
│                                             │
│ This medium communications unit can be      │
│ used to communicate with nearby trade       │  ← Description with
│ vessels.                                    │     rich text links
│                                             │
│ When connected to a Computer (Modern)       │  ← Links in cyan #008AE6
│ containing a Communications Motherboard...  │
│                                             │
│ Prefab Hash: 439026183  Paintable: Yes     │  ← Metadata row
│ Prefab Name: StructureSatelliteDish        │
│                                             │
│ Base Power Usage: 50 W                      │
├─────────────────────────────────────────────┤
│ Operational Details [>]                     │  ← Collapsible (mod adds)
├─────────────────────────────────────────────┤
│ Constructed From [>]                        │  ← Collapsible
├─────────────────────────────────────────────┤
│ Build States [>]                            │  ← Collapsible
├─────────────────────────────────────────────┤
│ Logic [>]                                   │  ← Collapsible
│   Temperature        R/W                    │  ← SPDALogic items
│   Setting            R/W  0,1,2,3           │
├─────────────────────────────────────────────┤
│ Internal Memory [>]                         │
│ Size: 256 B    Access: Read Write          │
├─────────────────────────────────────────────┤
│ Connections [>]                             │
└─────────────────────────────────────────────┘
```

**Collapsible Section (StationpediaCategory):**
- Title text on left
- Expand/collapse arrow icon on right (> or v)
- Dark background
- Contents hidden when collapsed

**Logic Item (SPDALogic):**
- Two columns: Name | Access/Values
- Hover shows tooltip (from genericDescriptions)

**Rich Text Parsing (TMP tags):**
- `<link="THING:ThingName">` → Cyan clickable link
- `<color=#HEX>` → Colored text
- `<b>`, `<i>`, `<u>` → Bold, italic, underline
- `<size=X>` → Font size

**Steps:**
1. Create collapsible section component matching game
2. Create logic/mode/slot item renderer
3. Create rich text parser for TMP tags
4. Build full device page matching screenshot
5. Add tooltip hover support

---

### Phase 4: Editing Capabilities

**Objective:** Allow full editing of mod content (operational details, tooltips, guides, game mechanics)

**Files to Create/Modify:**
- `src/renderer/simulator/components/EditableSection.tsx`
- `src/renderer/simulator/components/EditableLogicItem.tsx`
- `src/renderer/simulator/components/GuideEditor.tsx`
- `src/renderer/simulator/components/GameMechanicsEditor.tsx`
- `src/renderer/simulator/hooks/useSimulatorEditing.ts`

**Editing Features:**

1. **Operational Details:**
   - Click to edit title/description inline
   - Drag handle to reorder sections
   - Add/remove child sections
   - Add images, videos, YouTube embeds
   - Set background colors
   - Toggle collapsible

2. **Tooltips (genericDescriptions):**
   - Hover any logic/mode/slot item
   - Click "Edit" button in tooltip
   - Modal editor for description
   - Applies to ALL devices using that item

3. **Custom Guides:**
   - Create new guide pages
   - Add to "Stationeers Survival Manual" section
   - Edit guide content with operational details format
   - Delete guides

4. **Game Mechanics Pages:**
   - Create new mechanics pages
   - Edit content with nested sections
   - Delete pages

**Visual Editing Mode:**
- Toggle "Edit Mode" button in nav
- Shows drag handles on sections
- Shows add/remove buttons
- Inline text editing on click
- Orange border indicates editable areas

**Data Sync:**
- Changes update descriptions.json in real-time
- Preview reflects edits immediately
- Save button writes to disk

**Steps:**
1. Create editable wrapper components
2. Add drag-and-drop to sections
3. Build tooltip editor modal
4. Create guide/mechanics page editor
5. Wire up data sync with main editor

---

### Open Questions

1. **Device thumbnails:** Should we load from game assets or use placeholders? → Placeholders for now
2. **All 18 categories:** Need to update category mapping to include Hand Tools, Cartridges, Personal, Furniture
3. **Font:** Use system font or try to match Exo2/Roboto? → System font close enough

---

### Success Criteria

✅ Home screen visually matches game screenshots  
✅ Device pages match Satellite Dish screenshot exactly  
✅ 9-slice borders render correctly on all UI elements  
✅ Collapsible sections expand/collapse like game  
✅ Rich text links are clickable and cyan colored  
✅ Tooltips appear on hover for logic/mode/slot items  
✅ Can edit operational details with drag-and-drop  
✅ Can edit tooltips that apply globally  
✅ Can create/delete guide pages  
✅ Can create/delete game mechanics pages  
✅ Changes sync to descriptions.json
