/**
 * Preload Script for Simulator Window
 * Exposes safe IPC methods to simulator renderer
 */
const { contextBridge, ipcRenderer } = require('electron');

// Expose file system API (same as main preload)
contextBridge.exposeInMainWorld('electronAPI', {
  // File system
  readFile: (filePath: string) => ipcRenderer.invoke('fs:readFile', filePath),
  writeFile: (filePath: string, content: string, createBackup: boolean = true) =>
    ipcRenderer.invoke('fs:writeFile', filePath, content, createBackup),
  listFiles: (directory: string, extensions: string[] = []) =>
    ipcRenderer.invoke('fs:listFiles', directory, extensions),
  exists: (filePath: string) => ipcRenderer.invoke('fs:exists', filePath),
});

// Simulator API
contextBridge.exposeInMainWorld('simulatorAPI', {
  // Get current device
  getCurrentDevice: () => ipcRenderer.invoke('simulator:get-current-device'),

  // Get device by key
  getDevice: (deviceKey: string) => ipcRenderer.invoke('simulator:get-device', deviceKey),

  // Get all devices
  getAllDevices: () => ipcRenderer.invoke('simulator:get-all-devices'),

  // Navigate to device
  navigateToDevice: (deviceKey: string) => ipcRenderer.send('simulator:navigate-to-device', deviceKey),

  // Set mode
  setMode: (mode: 'vanilla' | 'ascended') => ipcRenderer.send('simulator:set-mode', mode),

  // Get mode
  getMode: () => ipcRenderer.invoke('simulator:get-mode'),

  // Listen for device changes
  onDeviceChanged: (callback: (deviceKey: string) => void) => {
    ipcRenderer.on('simulator:device-changed', (event: any, deviceKey: string) => {
      callback(deviceKey);
    });
  },

  // Listen for mode changes
  onModeChanged: (callback: (mode: 'vanilla' | 'ascended') => void) => {
    ipcRenderer.on('simulator:mode-changed', (event: any, mode: 'vanilla' | 'ascended') => {
      callback(mode);
    });
  },

  // Remove listeners
  removeDeviceChangeListener: () => {
    ipcRenderer.removeAllListeners('simulator:device-changed');
  },

  removeModeChangeListener: () => {
    ipcRenderer.removeAllListeners('simulator:mode-changed');
  },
});

// Type declarations - Note: Uses subset of main ElectronAPI for simulator window
// Exported for use in simulator renderer
export interface SimulatorElectronAPI {
  readFile: (filePath: string) => Promise<{ success: boolean; data?: string; error?: string }>;
  writeFile: (filePath: string, content: string, createBackup?: boolean) => Promise<{ success: boolean; error?: string }>;
  listFiles: (directory: string, extensions?: string[]) => Promise<{ success: boolean; data?: string[]; error?: string }>;
  exists: (filePath: string) => Promise<boolean>;
}

export interface SimulatorAPI {
  getCurrentDevice: () => Promise<any>;
  getDevice: (deviceKey: string) => Promise<any>;
  getAllDevices: () => Promise<any[]>;
  navigateToDevice: (deviceKey: string) => void;
  setMode: (mode: 'vanilla' | 'ascended') => void;
  getMode: () => Promise<'vanilla' | 'ascended'>;
  onDeviceChanged: (callback: (deviceKey: string) => void) => void;
  onModeChanged: (callback: (mode: 'vanilla' | 'ascended') => void) => void;
  removeDeviceChangeListener: () => void;
  removeModeChangeListener: () => void;
}

// Note: Window types are declared in src/renderer/simulator/types/window.d.ts
