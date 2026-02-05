/**
 * Electron Main Process
 */
import { app, BrowserWindow, ipcMain, dialog, protocol, net } from 'electron';
import * as fs from 'fs';
import * as path from 'path';
import { fileURLToPath } from 'url';
import { createSimulatorWindow, getSimulatorWindow, setupSimulatorIPC } from './simulatorWindow';
import { setSimulatorDevices as setSimulatorWindowDevices } from './ipcHandlers';

// ESM compatibility for __dirname
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

let mainWindow: BrowserWindow | null = null;

// Register custom protocol for local assets before app is ready
protocol.registerSchemesAsPrivileged([
  { scheme: 'local-asset', privileges: { secure: true, supportFetchAPI: true, stream: true } }
]);

function createWindow() {
  mainWindow = new BrowserWindow({
    width: 1600,
    height: 1000,
    minWidth: 1200,
    minHeight: 700,
    webPreferences: {
      preload: path.join(__dirname, '../preload/index.js'),
      contextIsolation: true,
      nodeIntegration: false,
    },
    backgroundColor: '#1a1a1a',
    show: false,
  });

  mainWindow.once('ready-to-show', () => {
    mainWindow?.show();
  });

  // Load the app
  if (process.env.VITE_DEV_SERVER_URL) {
    mainWindow.loadURL(process.env.VITE_DEV_SERVER_URL);
    mainWindow.webContents.openDevTools();
  } else {
    mainWindow.loadFile(path.join(__dirname, '../../dist/index.html'));
  }

  mainWindow.on('closed', () => {
    mainWindow = null;
  });

  // Setup simulator IPC when main window is created
  setupSimulatorIPC();
}

app.whenReady().then(() => {
  // Register protocol handler for local assets (thumbnails, etc.)
  protocol.handle('local-asset', async (request) => {
    // URL format: local-asset://C/path/to/file.png (drive letter without colon due to URL spec)
    let filePath = decodeURIComponent(request.url.replace('local-asset://', ''));
    
    // On Windows, add the colon after the drive letter (C -> C:)
    if (process.platform === 'win32' && /^[a-zA-Z]\//.test(filePath)) {
      filePath = filePath[0] + ':' + filePath.slice(1);
    }
    
    try {
      // Read file directly from disk
      const data = fs.readFileSync(filePath);
      
      // Determine content type based on extension
      const ext = path.extname(filePath).toLowerCase();
      const contentTypes: Record<string, string> = {
        '.png': 'image/png',
        '.jpg': 'image/jpeg',
        '.jpeg': 'image/jpeg',
        '.gif': 'image/gif',
        '.svg': 'image/svg+xml',
        '.webp': 'image/webp',
      };
      const contentType = contentTypes[ext] || 'application/octet-stream';
      
      return new Response(data, {
        status: 200,
        headers: { 'Content-Type': contentType }
      });
    } catch (error) {
      console.error('Failed to load local asset:', filePath, error);
      return new Response('Not found', { status: 404 });
    }
  });
  
  createWindow();
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  if (mainWindow === null) {
    createWindow();
  }
});

// IPC Handlers

// Open folder dialog
ipcMain.handle('dialog:openFolder', async () => {
  const result = await dialog.showOpenDialog(mainWindow!, {
    properties: ['openDirectory'],
    title: 'Select Mod Folder',
  });
  if (result.canceled || result.filePaths.length === 0) {
    return null;
  }
  return result.filePaths[0];
});

// Read file
ipcMain.handle('fs:readFile', async (_, filePath: string) => {
  try {
    const content = fs.readFileSync(filePath, 'utf-8');
    return { success: true, data: content };
  } catch (error: any) {
    return { success: false, error: error.message };
  }
});

// Write file with backup
ipcMain.handle('fs:writeFile', async (_, filePath: string, content: string, createBackup: boolean) => {
  try {
    // Create backup if requested
    if (createBackup && fs.existsSync(filePath)) {
      const backupDir = path.join(path.dirname(filePath), '.backups');
      if (!fs.existsSync(backupDir)) {
        fs.mkdirSync(backupDir, { recursive: true });
      }
      const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
      const backupPath = path.join(backupDir, `${path.basename(filePath)}.${timestamp}.bak`);
      fs.copyFileSync(filePath, backupPath);
    }

    fs.writeFileSync(filePath, content, 'utf-8');
    return { success: true };
  } catch (error: any) {
    return { success: false, error: error.message };
  }
});

// List files in directory
ipcMain.handle('fs:listFiles', async (_, directory: string, extensions: string[]) => {
  try {
    const files: string[] = [];
    
    function scanDir(dir: string) {
      const entries = fs.readdirSync(dir, { withFileTypes: true });
      for (const entry of entries) {
        const fullPath = path.join(dir, entry.name);
        if (entry.isDirectory() && !entry.name.startsWith('.')) {
          scanDir(fullPath);
        } else if (entry.isFile()) {
          const ext = path.extname(entry.name).toLowerCase();
          if (extensions.length === 0 || extensions.includes(ext)) {
            files.push(fullPath);
          }
        }
      }
    }
    
    if (fs.existsSync(directory)) {
      scanDir(directory);
    }
    
    return { success: true, data: files };
  } catch (error: any) {
    return { success: false, error: error.message };
  }
});

// Check if path exists
ipcMain.handle('fs:exists', async (_, filePath: string) => {
  return fs.existsSync(filePath);
});

// Get file info
ipcMain.handle('fs:stat', async (_, filePath: string) => {
  try {
    const stats = fs.statSync(filePath);
    return {
      success: true,
      data: {
        isFile: stats.isFile(),
        isDirectory: stats.isDirectory(),
        size: stats.size,
        modified: stats.mtime.toISOString(),
      },
    };
  } catch (error: any) {
    return { success: false, error: error.message };
  }
});

// Store and retrieve last workspace
const userDataPath = app.getPath('userData');
const settingsPath = path.join(userDataPath, 'settings.json');

ipcMain.handle('settings:get', async () => {
  try {
    if (fs.existsSync(settingsPath)) {
      const content = fs.readFileSync(settingsPath, 'utf-8');
      return JSON.parse(content);
    }
    return {};
  } catch {
    return {};
  }
});

ipcMain.handle('settings:set', async (_, settings: object) => {
  try {
    fs.writeFileSync(settingsPath, JSON.stringify(settings, null, 2), 'utf-8');
    return { success: true };
  } catch (error: any) {
    return { success: false, error: error.message };
  }
});
// Open simulator window
ipcMain.handle('simulator:open', async () => {
  if (!mainWindow) return null;
  const simulator = getSimulatorWindow() || createSimulatorWindow(mainWindow);
  return { success: true };
});

// Update simulator with devices
ipcMain.on('editor:devices-updated', (event, devices: any[]) => {
  setSimulatorWindowDevices(devices);
});

// Scan image folders for PNG files (Texture2D and thumbnails)
ipcMain.handle('fs:scanImageFolder', async (_, baseFolder?: string) => {
  try {
    // Base path to AssetRipperFiles
    const basePaths = [
      baseFolder ? path.join(baseFolder, '..', 'AssetRipperFiles', 'ExportedProject', 'Assets') : null,
      'C:\\Dev\\StationpediaAscendedWorkspace\\AssetRipperFiles\\ExportedProject\\Assets',
    ].filter(Boolean) as string[];

    let assetsPath: string | null = null;
    for (const p of basePaths) {
      if (fs.existsSync(p)) {
        assetsPath = p;
        break;
      }
    }

    if (!assetsPath) {
      return { success: false, error: 'Could not find Assets folder' };
    }

    // Folders to scan for PNGs
    const foldersToScan = [
      { path: path.join(assetsPath, 'Resources', 'ui', 'thumbnails'), category: 'Thumbnails' },
      { path: path.join(assetsPath, 'Texture2D'), category: 'UI Textures' },
    ];

    const images: { filename: string; path: string; category: string }[] = [];

    for (const folder of foldersToScan) {
      if (fs.existsSync(folder.path)) {
        const entries = fs.readdirSync(folder.path, { withFileTypes: true });
        for (const entry of entries) {
          if (entry.isFile() && entry.name.toLowerCase().endsWith('.png')) {
            images.push({
              filename: entry.name,
              path: path.join(folder.path, entry.name),
              category: folder.category,
            });
          }
        }
      }
    }

    // Sort alphabetically by filename
    images.sort((a, b) => a.filename.localeCompare(b.filename));

    return { success: true, images, folder: assetsPath };
  } catch (error: any) {
    return { success: false, error: error.message };
  }
});

// Copy image to mod images folder
ipcMain.handle('fs:copyImageToMod', async (_, sourcePath: string, filename: string) => {
  try {
    // Default mod images folder
    const modImagesFolders = [
      'C:\\Dev\\StationpediaAscendedWorkspace\\StationpediaAscended\\mod\\images',
    ];

    let targetFolder: string | null = null;
    for (const folder of modImagesFolders) {
      if (fs.existsSync(folder)) {
        targetFolder = folder;
        break;
      }
    }

    if (!targetFolder) {
      return { success: false, error: 'Could not find mod images folder' };
    }

    const targetPath = path.join(targetFolder, filename);

    // Check if file already exists
    if (fs.existsSync(targetPath)) {
      return { success: true, alreadyExists: true, targetPath };
    }

    // Copy the file
    fs.copyFileSync(sourcePath, targetPath);

    return { success: true, copied: true, targetPath };
  } catch (error: any) {
    return { success: false, error: error.message };
  }
});