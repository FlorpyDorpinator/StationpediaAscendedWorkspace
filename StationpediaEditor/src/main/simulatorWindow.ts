/**
 * Simulator Window Manager
 * Creates and manages the simulator preview window
 */
import { BrowserWindow, ipcMain } from 'electron';
import * as path from 'path';
import { fileURLToPath } from 'url';
import {
  IPC_CHANNELS,
  simulatorState,
  updateSimulatorState,
  getCurrentDevice,
  findDeviceByKey,
  setSimulatorDevices,
} from './ipcHandlers';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

let simulatorWindow: BrowserWindow | null = null;

/**
 * Create the simulator window
 */
export function createSimulatorWindow(mainWindow: BrowserWindow): BrowserWindow {
  if (simulatorWindow) {
    simulatorWindow.focus();
    return simulatorWindow;
  }

  // Get main window position and size
  const mainBounds = mainWindow.getBounds();

  // Position simulator to the right of main window
  const simulatorX = mainBounds.x + mainBounds.width + 10;
  const simulatorY = mainBounds.y;

  simulatorWindow = new BrowserWindow({
    width: 800,
    height: mainBounds.height,
    x: simulatorX,
    y: simulatorY,
    minWidth: 600,
    minHeight: 500,
    webPreferences: {
      preload: path.join(__dirname, '../preload/simulatorPreload.js'),
      contextIsolation: true,
      nodeIntegration: false,
    },
    backgroundColor: '#1a1a1a',
    show: false,
  });

  simulatorWindow.once('ready-to-show', () => {
    simulatorWindow?.show();
  });

  // Load simulator
  if (process.env.VITE_DEV_SERVER_URL) {
    simulatorWindow.loadURL(`${process.env.VITE_DEV_SERVER_URL}simulator/simulator.html`);
    simulatorWindow.webContents.openDevTools();
  } else {
    simulatorWindow.loadFile(path.join(__dirname, '../../dist/simulator/simulator.html'));
  }

  // Handle window closed
  simulatorWindow.on('closed', () => {
    simulatorWindow = null;
  });

  return simulatorWindow;
}

/**
 * Get simulator window
 */
export function getSimulatorWindow(): BrowserWindow | null {
  return simulatorWindow;
}

/**
 * Broadcast device change to all windows
 */
export function broadcastDeviceChange(deviceKey: string) {
  updateSimulatorState({ currentDeviceKey: deviceKey });

  // Send to simulator window
  if (simulatorWindow && !simulatorWindow.isDestroyed()) {
    simulatorWindow.webContents.send(IPC_CHANNELS['simulator:device-changed'], deviceKey);
  }
}

/**
 * Broadcast mode change to all windows
 */
export function broadcastModeChange(mode: 'vanilla' | 'ascended') {
  updateSimulatorState({ mode });

  // Send to simulator window
  if (simulatorWindow && !simulatorWindow.isDestroyed()) {
    simulatorWindow.webContents.send(IPC_CHANNELS['simulator:mode-changed'], mode);
  }
}

/**
 * Setup IPC handlers for simulator communication
 */
export function setupSimulatorIPC() {
  // Get current device
  ipcMain.handle(IPC_CHANNELS['simulator:get-current-device'], async () => {
    return getCurrentDevice();
  });

  // Get specific device
  ipcMain.handle(IPC_CHANNELS['simulator:get-device'], async (_, deviceKey: string) => {
    return findDeviceByKey(deviceKey);
  });

  // Get all devices
  ipcMain.handle(IPC_CHANNELS['simulator:get-all-devices'], async () => {
    return simulatorState.devices;
  });

  // Navigate to device
  ipcMain.on(IPC_CHANNELS['simulator:navigate-to-device'], (event, deviceKey: string) => {
    broadcastDeviceChange(deviceKey);
  });

  // Set mode
  ipcMain.on(IPC_CHANNELS['simulator:set-mode'], (event, mode: 'vanilla' | 'ascended') => {
    broadcastModeChange(mode);
  });

  // Get mode
  ipcMain.handle(IPC_CHANNELS['simulator:get-mode'], async () => {
    return simulatorState.mode;
  });
}

/**
 * Close simulator window
 */
export function closeSimulatorWindow() {
  if (simulatorWindow) {
    simulatorWindow.destroy();
    simulatorWindow = null;
  }
}
