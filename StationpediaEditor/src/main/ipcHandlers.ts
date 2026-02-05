/**
 * IPC Channel Names and Handler Implementations
 * Defines all communication channels between main and renderer processes
 */

export const IPC_CHANNELS = {
  // Simulator state sync
  'simulator:get-current-device': 'simulator:get-current-device',
  'simulator:navigate-to-device': 'simulator:navigate-to-device',
  'simulator:device-changed': 'simulator:device-changed',
  'simulator:mode-changed': 'simulator:mode-changed',
  'simulator:get-device': 'simulator:get-device',
  'simulator:get-all-devices': 'simulator:get-all-devices',
  'simulator:set-mode': 'simulator:set-mode',
  'simulator:get-mode': 'simulator:get-mode',
} as const;

export type SimulatorMode = 'vanilla' | 'ascended';

export interface SimulatorState {
  currentDeviceKey: string | null;
  mode: SimulatorMode;
  devices: any[];
}

// Global state for simulator
export let simulatorState: SimulatorState = {
  currentDeviceKey: null,
  mode: 'ascended',
  devices: [],
};

/**
 * Update simulator state and broadcast to all windows
 */
export function updateSimulatorState(updates: Partial<SimulatorState>) {
  simulatorState = { ...simulatorState, ...updates };
  return simulatorState;
}

/**
 * Get current simulator state
 */
export function getSimulatorState(): SimulatorState {
  return simulatorState;
}

/**
 * Set devices in simulator state
 */
export function setSimulatorDevices(devices: any[]) {
  simulatorState.devices = devices;
}

/**
 * Find a device by key
 */
export function findDeviceByKey(deviceKey: string): any | null {
  return simulatorState.devices.find((d) => d.deviceKey === deviceKey) || null;
}

/**
 * Get the current device
 */
export function getCurrentDevice(): any | null {
  if (!simulatorState.currentDeviceKey) return null;
  return findDeviceByKey(simulatorState.currentDeviceKey);
}
