## Phase 4 Complete: Simulator Window

Built a separate Electron preview window (Simulator) with navigation controls and IPC-based state synchronization with the main editor window.

**Files created/changed:**
- src/main/ipcHandlers.ts
- src/main/simulatorWindow.ts
- src/main/index.ts (modified)
- src/preload/simulatorPreload.ts
- src/renderer/simulator/simulator.html
- src/renderer/simulator/simulatorMain.tsx
- src/renderer/simulator/SimulatorApp.tsx
- src/renderer/simulator/components/NavigationBar.tsx
- src/renderer/services/sharedState.ts
- vite.config.ts (modified)
- src/main/__tests__/ipcHandlers.test.ts
- src/renderer/simulator/__tests__/SimulatorApp.test.tsx
- src/renderer/simulator/__tests__/NavigationBar.test.tsx

**Functions created/changed:**
- IPC Channels: get-current-device, navigate-to-device, device-changed, mode-changed, workspace-changed, open-simulator, close-simulator, sync-state
- simulatorWindow: create, show, close, isOpen functions
- SimulatorApp: Main simulator component with IPC subscription
- NavigationBar: Back/forward navigation, device selector, mode toggle
- sharedState: State sync service with subscribe/publish pattern

**Tests created/changed:**
- ipcHandlers.test.ts: 22 tests for IPC handlers
- SimulatorApp.test.tsx: 25 tests for simulator app
- NavigationBar.test.tsx: 29 tests for navigation

**Review Status:** APPROVED

**Key Implementation Details:**
1. **Dual-window architecture** with Electron IPC as authoritative sync
2. **Navigation history** with back/forward browser-like behavior
3. **Device selector** dropdown with search filtering
4. **Mode toggle** (Vanilla ↔ Ascended) with localStorage persistence
5. **Deep linking** via {THING:} links navigating to devices
6. **Window lifecycle** management (main closing closes simulator)
7. **Multi-entry Vite build** for main and simulator HTML

**Git Commit Message:**
```
feat: Add Simulator window with IPC-based state sync

- Add simulator window management in main process
- Add IPC handlers for device navigation and mode toggle
- Add SimulatorApp with live preview using StationpediaRenderer
- Add NavigationBar with back/forward and device selector
- Add sharedState service for state synchronization
- Add preload script for simulator window security
- Update Vite config for multi-entry build
- Add 76 tests for simulator components
```
