/**
 * Preload Script - Exposes safe IPC methods to renderer
 */
const { contextBridge, ipcRenderer } = require('electron');

// Expose file system API
contextBridge.exposeInMainWorld('electronAPI', {
  // Dialog
  openFolder: () => ipcRenderer.invoke('dialog:openFolder'),

  // File system
  readFile: (filePath: string) => ipcRenderer.invoke('fs:readFile', filePath),
  writeFile: (filePath: string, content: string, createBackup: boolean = true) =>
    ipcRenderer.invoke('fs:writeFile', filePath, content, createBackup),
  listFiles: (directory: string, extensions: string[] = []) =>
    ipcRenderer.invoke('fs:listFiles', directory, extensions),
  exists: (filePath: string) => ipcRenderer.invoke('fs:exists', filePath),
  stat: (filePath: string) => ipcRenderer.invoke('fs:stat', filePath),
  scanImageFolder: (baseFolder?: string) => ipcRenderer.invoke('fs:scanImageFolder', baseFolder),
  copyImageToMod: (sourcePath: string, filename: string) => ipcRenderer.invoke('fs:copyImageToMod', sourcePath, filename),

  // Settings
  getSettings: () => ipcRenderer.invoke('settings:get'),
  setSettings: (settings: object) => ipcRenderer.invoke('settings:set', settings),

  // Simulator
  openSimulator: () => ipcRenderer.invoke('simulator:open'),
  sendDevicesToSimulator: (devices: any[]) => ipcRenderer.send('editor:devices-updated', devices),
});

// Type declarations for TypeScript
export interface ElectronAPI {
  openFolder: () => Promise<string | null>;
  readFile: (filePath: string) => Promise<{ success: boolean; data?: string; error?: string }>;
  writeFile: (filePath: string, content: string, createBackup?: boolean) => Promise<{ success: boolean; error?: string }>;
  listFiles: (directory: string, extensions?: string[]) => Promise<{ success: boolean; data?: string[]; error?: string }>;
  exists: (filePath: string) => Promise<boolean>;
  stat: (filePath: string) => Promise<{ success: boolean; data?: { isFile: boolean; isDirectory: boolean; size: number; modified: string }; error?: string }>;
  scanImageFolder: (baseFolder?: string) => Promise<{ success: boolean; images?: { filename: string; path: string; category?: string }[]; folder?: string; error?: string }>;
  copyImageToMod: (sourcePath: string, filename: string) => Promise<{ success: boolean; copied?: boolean; alreadyExists?: boolean; targetPath?: string; error?: string }>;
  getSettings: () => Promise<Record<string, any>>;
  setSettings: (settings: object) => Promise<{ success: boolean; error?: string }>;
  openSimulator: () => Promise<{ success: boolean }>;
  sendDevicesToSimulator: (devices: any[]) => void;
}

declare global {
  interface Window {
    electronAPI: ElectronAPI;
  }
}
