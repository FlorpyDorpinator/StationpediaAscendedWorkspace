# Stationpedia Editor

A rich desktop editor for creating and editing Stationpedia content for the Stationeers game mod "Stationpedia Ascended".

![Electron](https://img.shields.io/badge/Electron-28.1-blue)
![React](https://img.shields.io/badge/React-18.2-blue)
![TypeScript](https://img.shields.io/badge/TypeScript-5.3-blue)

## Features

- 📝 **Rich Text Editing** - Word processor-style editing with TipTap
- 🔗 **Link Picker** - Easily insert cross-references to other Stationpedia pages
- 👁️ **Live Preview** - See how content will appear in-game
- 🔍 **Fast Search** - Fuzzy search across all content with Fuse.js
- ✅ **Validation** - Real-time error checking for broken links and syntax
- 💾 **Auto-backup** - Automatic backup creation when saving
- 📊 **Operational Details Editor** - Drag-and-drop editing of device operation info

## Installation

### Prerequisites

- Node.js 18+ 
- npm or yarn

### Setup

1. Navigate to the StationpediaEditor directory:
   ```bash
   cd StationpediaEditor
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Run in development mode:
   ```bash
   # Terminal 1: Start Vite dev server
   npm run dev
   
   # Terminal 2: Start Electron (set env variable first)
   # Windows PowerShell:
   $env:VITE_DEV_SERVER_URL = "http://localhost:5173/"
   npx electron .
   
   # Windows CMD:
   set VITE_DEV_SERVER_URL=http://localhost:5173/ && npx electron .
   
   # Linux/Mac:
   VITE_DEV_SERVER_URL=http://localhost:5173/ npx electron .
   ```

4. Build for production:
   ```bash
   npm run build
   ```

## Usage

### Opening a Mod Folder

1. Click **📁 Open Mod Folder** in the sidebar
2. Navigate to your Stationpedia Ascended mod folder
3. The editor will load all content:
   - `descriptions.json` - Device descriptions
   - `guides/` folder - Game guides
   - `game-mechanics/` folder - Mechanics pages

### Editing Content

#### Text Formatting

Use the toolbar or keyboard shortcuts:
- **Bold**: Ctrl+B
- **Italic**: Ctrl+I
- **Underline**: Ctrl+U
- **Headings**: H1, H2, H3 buttons
- **Lists**: Bullet and numbered lists
- **Code**: Inline code blocks

#### Inserting Links

1. Click the **🔗 Link** button in the toolbar
2. Search for the target page by name or key
3. Click to insert a `{THING:DeviceKey}` reference

#### Source View

Toggle between visual editing and raw HTML/TMP source:
- Click **</> Source** to view/edit raw markup
- Click **📝 Visual** to return to rich editing

### Saving

- Press **Ctrl+S** or click the **💾 Save** button
- Backups are automatically created in a `.backups` folder

### Preview

The right panel shows an approximate preview of how content will appear in-game:
- Collapsible sections
- Logic types table
- Operational details hierarchy
- Slot information

## Folder Structure

Expected mod folder structure:

```
your-mod-folder/
├── descriptions.json      # Main device descriptions
├── guides/                 # Game guide JSON files
│   ├── guide-1.json
│   └── guide-2.json
├── game-mechanics/         # Mechanics JSON files
│   ├── mechanic-1.json
│   └── mechanic-2.json
└── assets/                 # Optional image assets
    └── image.png
```

### descriptions.json Format

```json
[
  {
    "deviceKey": "ItemWrench",
    "displayName": "Wrench",
    "pageDescription": "<p>The wrench is used to...</p>",
    "operationalDetails": [
      {
        "title": "Usage",
        "description": "How to use the wrench..."
      }
    ],
    "logicDescriptions": {
      "Setting": {
        "description": "Current setting value",
        "dataType": "Integer",
        "range": "0-100"
      }
    }
  }
]
```

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+S | Save all changes |
| Ctrl+B | Bold text |
| Ctrl+I | Italic text |
| Ctrl+U | Underline text |
| Ctrl+Z | Undo |
| Ctrl+Y | Redo |

## Development

### Tech Stack

- **Electron 28** - Desktop framework
- **React 18** - UI components
- **TypeScript 5.3** - Type safety
- **Vite 5** - Build tool
- **TipTap 2** - Rich text editor
- **Zustand 4** - State management
- **Fuse.js 7** - Fuzzy search
- **TailwindCSS 3** - Styling

### Project Structure

```
src/
├── main/           # Electron main process
│   └── index.ts    # Window creation, IPC handlers
├── preload/        # Electron preload scripts
│   └── index.ts    # Safe IPC exposure
└── renderer/       # React application
    ├── components/ # UI components
    ├── services/   # File IO, indexing, validation
    ├── store/      # Zustand state management
    ├── models/     # TypeScript interfaces
    └── styles/     # TailwindCSS styles
```

### Scripts

| Command | Description |
|---------|-------------|
| `npm run dev` | Start Vite dev server |
| `npm run build` | Build for production |
| `npm run type-check` | Run TypeScript checks |
| `npm run preview` | Preview production build |

## License

MIT - Created for the Stationeers modding community.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## Acknowledgements

- [Stationeers](https://store.steampowered.com/app/544550/Stationeers/) by RocketWerkz
- [TipTap](https://tiptap.dev/) - Headless editor framework
- [Electron](https://www.electronjs.org/) - Desktop app framework
