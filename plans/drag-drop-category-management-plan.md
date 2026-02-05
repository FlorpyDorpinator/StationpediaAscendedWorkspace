## Plan: Drag-and-Drop Category Management with Headers

Add drag-and-drop functionality to move items between categories, create inline section headers, and reorder items within categories.

**Phases (4 phases)**

1. **Phase 1: Add Store Actions for Move and Reorder**
    - **Objective:** Implement store actions for category moves, reordering, and header management
    - **Files/Functions to Modify/Create:** editorStore.ts
    - **Steps:**
        1. Add `moveToCategory(itemKey, targetCategory, insertIndex?)` action
        2. Add `reorderItem(itemKey, newIndex)` action to change position within array
        3. Add `addHeader(category, title, insertIndex)` action to create inline headers
        4. Add `deleteHeader(category, headerKey)` action
        5. Transform keys (deviceKey ↔ guideKey) when moving between categories

2. **Phase 2: Implement Drag-and-Drop in Content Tree**
    - **Objective:** Make tree items draggable with drop zones for categories and reordering
    - **Files/Functions to Modify/Create:** ContentTree.tsx
    - **Steps:**
        1. Add drag handlers to list items (onDragStart, onDragEnd)
        2. Add drop zone handlers to category tabs (onDragOver, onDrop)
        3. Add drop indicator between items for reordering
        4. Visual feedback during drag (highlight valid drop targets)
        5. Call store actions on successful drop

3. **Phase 3: Add Inline Header Support**
    - **Objective:** Allow creating section headers that appear in the lists and in-game
    - **Files/Functions to Modify/Create:** ContentTree.tsx, contentModel.ts
    - **Steps:**
        1. Define header structure (special item with `isHeader: true, headerTitle: string`)
        2. Add "Add Header" button in each category panel
        3. Render headers with distinct styling in the tree
        4. Make headers draggable/reorderable with other items
        5. Ensure headers are saved to JSON and loaded correctly

4. **Phase 4: Move SpeciesSurvivalGuide2 and Test**
    - **Objective:** Test the feature by moving the species survival guide
    - **Files/Functions to Modify/Create:** Manual testing via editor
    - **Steps:**
        1. Open editor, drag SpeciesSurvivalGuide2 from Guides to Mechanics tab
        2. Verify JSON is updated correctly
        3. Build and verify in-game

**Notes:**
- Inline headers will appear in-game in the Stationpedia pages
- Headers use special structure that the mod will render as section dividers
