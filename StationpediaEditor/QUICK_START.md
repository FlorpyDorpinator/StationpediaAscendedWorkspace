# Stationpedia Editor - Quick Start

## Running the Editor

### Development Mode
```powershell
# Option 1: Use the batch file
.\start-dev.bat

# Option 2: Use npm directly
npm run dev
```

This will:
- Start Vite dev server on port 5173
- Launch Electron window automatically
- Enable hot-reload for development
- Open DevTools for debugging

### Production Build
```powershell
npm run build
```

Creates executable in `release/` folder.

## First Time Setup

1. **Open a Workspace**
   - Click "Open Workspace" button
   - Navigate to: `StationpediaAscended\mod\`
   - Select `descriptions.json`

2. **Explore the Interface**
   - **Left Panel**: Content Tree (devices, guides, tooltips)
   - **Center Panel**: Editor and Operational Details
   - **Right Panel**: Live Preview
   - **Toggle Panels**: Validation, Assets, Tooltips via toolbar

3. **Make Edits**
   - Select a device from Content Tree
   - Edit page description with rich text toolbar
   - Add/remove operational details sections
   - Drag to reorder sections
   - Auto-save happens every 30 seconds

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+S` | Save workspace |
| `Ctrl+O` | Open workspace |
| `Ctrl+Shift+S` | Save As |
| `Ctrl+N` | New workspace |
| `Ctrl+W` | Close workspace |
| `F5` | Refresh preview |

## Testing

```powershell
# Run all tests (422 tests)
npm test

# Run tests with UI
npm run test:ui

# Type checking
npm run type-check
```

## Project Structure

```
src/
├── main/               # Electron main process
├── preload/            # Electron preload scripts
└── renderer/           # React application
    ├── editor/         # Editor UI components
    ├── simulator/      # Preview renderer
    ├── components/     # Shared components
    ├── models/         # TypeScript types
    ├── services/       # Business logic
    ├── store/          # Zustand state management
    └── hooks/          # React hooks
```

## Troubleshooting

**Window doesn't open**
- Check terminal for errors
- Verify node_modules installed: `npm install`
- Try: `npm run type-check`

**JSON doesn't load**
- Check console for parsing errors
- Verify JSON is valid
- File must have `devices` array

**Preview not rendering**
- Open DevTools (Ctrl+Shift+I)
- Check Console tab for errors
- Verify TMP tags are valid

## Documentation

- Full user guide: `docs/USER_GUIDE.md`
- README: `README.md`
- Phase completion docs: `plans/`
