## Plan: Stationpedia Ascended IDE

Build a comprehensive IDE for authoring and validating the Stationeers mod "Stationpedia Ascended" with lossless JSON round-tripping, HTML-based Stationpedia rendering, and dual-window architecture.

**Phases: 7**

### Phase 1: Content AST & Lossless Serialization
- **Objective:** Build foundation with Content AST that perfectly round-trips JSON
- **Files/Functions:**
  - `src/renderer/models/ast.ts` - AST node types
  - `src/renderer/services/parser.ts` - TMP string → AST
  - `src/renderer/services/serializer.ts` - AST → TMP string  
  - `src/renderer/services/jsonCodec.ts` - JSON ↔ internal model
  - `src/renderer/models/contentModel.ts` - Full device/guide/mechanic types
- **Tests:** parser.test.ts, serializer.test.ts, roundtrip.test.ts
- **Key Requirements:**
  - Functional/semantic equivalence (not byte-for-byte)
  - Preserve unknown fields verbatim
  - Preserve array ordering

### Phase 2: Stationpedia Renderer (HTML)
- **Objective:** HTML/CSS renderer matching in-game appearance
- **Files/Functions:**
  - `src/renderer/simulator/StationpediaRenderer.tsx`
  - `src/renderer/simulator/components/` - CollapsibleSection, TOC, etc.
  - `src/renderer/simulator/styles/stationpedia.css`
- **Tests:** Structural DOM tests (screenshots later)
- **Key Requirements:**
  - Reimplement rendering RULES, not Unity
  - Use game PNG assets
  - Render ALL fields (logic, slots, images, metadata)

### Phase 3: Editor UI with Drag-and-Drop
- **Objective:** Authoring IDE with dockable panels
- **Files/Functions:**
  - `src/renderer/editor/PanelSystem.tsx` - react-mosaic
  - `src/renderer/editor/ContentTree.tsx`
  - `src/renderer/editor/RichTextEditor.tsx` - TipTap with AST
  - `src/renderer/editor/PropertyInspector.tsx`
  - `src/renderer/editor/OperationalDetailsTree.tsx` - @dnd-kit
- **Key Requirements:**
  - @dnd-kit with nested strategy
  - Drag handles move entire nodes + children
  - IC10/Notepad theming

### Phase 4: Simulator Window
- **Objective:** Separate preview window with navigation
- **Files/Functions:**
  - `src/main/simulatorWindow.ts`
  - `src/renderer/simulator/SimulatorApp.tsx`
  - `src/renderer/simulator/NavigationBar.tsx`
  - `src/renderer/services/sharedState.ts` - IPC-based sync
- **Key Requirements:**
  - Electron IPC as authoritative sync layer
  - Mode toggle (Vanilla ↔ Ascended)

### Phase 5: Tooltip Editing System
- **Objective:** Edit genericDescriptions tooltips
- **Files/Functions:**
  - `src/renderer/editor/TooltipEditor.tsx`
  - `src/renderer/simulator/TooltipPreview.tsx`

### Phase 6: Validation & Assets
- **Objective:** Comprehensive validation and asset management
- **Files/Functions:**
  - `src/renderer/services/validator.ts` - Enhanced
  - `src/renderer/editor/AssetBrowser.tsx`

### Phase 7: Polish & Persistence
- **Objective:** Final polish and documentation
- **Files/Functions:**
  - `src/renderer/services/persistence.ts`
  - User documentation

**Decisions Made:**
- Panel library: react-mosaic-component
- Rich text: TipTap with AST as canonical model
- State sync: Electron IPC authoritative, Zustand local
- Mode toggle: Vanilla hides Ascended overrides

**Open Questions:** None - all resolved.
