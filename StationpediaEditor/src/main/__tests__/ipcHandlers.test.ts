/**
 * Tests for IPC Handlers
 * Tests simulator state management and IPC channel definitions
 */
import { describe, it, expect, beforeEach, vi } from 'vitest';
import {
  IPC_CHANNELS,
  simulatorState,
  updateSimulatorState,
  getSimulatorState,
  setSimulatorDevices,
  findDeviceByKey,
  getCurrentDevice,
} from '../ipcHandlers';

describe('IPC Handlers', () => {
  beforeEach(() => {
    // Reset state
    simulatorState.currentDeviceKey = null;
    simulatorState.mode = 'ascended';
    simulatorState.devices = [];
  });

  describe('IPC Channel Names', () => {
    it('should define all simulator channels', () => {
      expect(IPC_CHANNELS['simulator:get-current-device']).toBeDefined();
      expect(IPC_CHANNELS['simulator:navigate-to-device']).toBeDefined();
      expect(IPC_CHANNELS['simulator:device-changed']).toBeDefined();
      expect(IPC_CHANNELS['simulator:mode-changed']).toBeDefined();
      expect(IPC_CHANNELS['simulator:get-device']).toBeDefined();
      expect(IPC_CHANNELS['simulator:get-all-devices']).toBeDefined();
      expect(IPC_CHANNELS['simulator:set-mode']).toBeDefined();
      expect(IPC_CHANNELS['simulator:get-mode']).toBeDefined();
    });

    it('should use consistent naming convention', () => {
      Object.values(IPC_CHANNELS).forEach((channel) => {
        expect(channel).toMatch(/^simulator:/);
      });
    });
  });

  describe('Simulator State Management', () => {
    it('should initialize with default state', () => {
      const state = getSimulatorState();
      expect(state.currentDeviceKey).toBeNull();
      expect(state.mode).toBe('ascended');
      expect(state.devices).toEqual([]);
    });

    it('should update current device', () => {
      updateSimulatorState({ currentDeviceKey: 'Device1' });
      const state = getSimulatorState();
      expect(state.currentDeviceKey).toBe('Device1');
    });

    it('should update mode', () => {
      updateSimulatorState({ mode: 'vanilla' });
      const state = getSimulatorState();
      expect(state.mode).toBe('vanilla');
    });

    it('should update devices list', () => {
      const devices = [
        { deviceKey: 'Device1', displayName: 'Device One' },
        { deviceKey: 'Device2', displayName: 'Device Two' },
      ];
      setSimulatorDevices(devices);
      const state = getSimulatorState();
      expect(state.devices).toEqual(devices);
    });

    it('should merge partial updates', () => {
      setSimulatorDevices([{ deviceKey: 'Device1', displayName: 'Device One' }]);
      updateSimulatorState({ currentDeviceKey: 'Device1' });

      const state = getSimulatorState();
      expect(state.currentDeviceKey).toBe('Device1');
      expect(state.devices).toHaveLength(1);
      expect(state.mode).toBe('ascended');
    });
  });

  describe('Device Lookup', () => {
    beforeEach(() => {
      const devices = [
        { deviceKey: 'StructureAutolathe', displayName: 'Autolathe' },
        { deviceKey: 'StructureSolarPanel', displayName: 'Solar Panel' },
        { deviceKey: 'StructureReactor', displayName: 'Nuclear Reactor' },
      ];
      setSimulatorDevices(devices);
    });

    it('should find device by key', () => {
      const device = findDeviceByKey('StructureAutolathe');
      expect(device).toBeDefined();
      expect(device?.displayName).toBe('Autolathe');
    });

    it('should return null for missing device', () => {
      const device = findDeviceByKey('NonExistentDevice');
      expect(device).toBeNull();
    });

    it('should find multiple devices', () => {
      const device1 = findDeviceByKey('StructureAutolathe');
      const device2 = findDeviceByKey('StructureSolarPanel');

      expect(device1).toBeDefined();
      expect(device2).toBeDefined();
      expect(device1?.displayName).toBe('Autolathe');
      expect(device2?.displayName).toBe('Solar Panel');
    });

    it('should get current device', () => {
      updateSimulatorState({ currentDeviceKey: 'StructureReactor' });
      const device = getCurrentDevice();

      expect(device).toBeDefined();
      expect(device?.displayName).toBe('Nuclear Reactor');
    });

    it('should return null for current device when none selected', () => {
      updateSimulatorState({ currentDeviceKey: null });
      const device = getCurrentDevice();

      expect(device).toBeNull();
    });
  });

  describe('Mode Management', () => {
    it('should support vanilla mode', () => {
      updateSimulatorState({ mode: 'vanilla' });
      const state = getSimulatorState();
      expect(state.mode).toBe('vanilla');
    });

    it('should support ascended mode', () => {
      updateSimulatorState({ mode: 'ascended' });
      const state = getSimulatorState();
      expect(state.mode).toBe('ascended');
    });

    it('should toggle between modes', () => {
      updateSimulatorState({ mode: 'vanilla' });
      let state = getSimulatorState();
      expect(state.mode).toBe('vanilla');

      updateSimulatorState({ mode: 'ascended' });
      state = getSimulatorState();
      expect(state.mode).toBe('ascended');
    });

    it('should preserve mode across state updates', () => {
      updateSimulatorState({ mode: 'vanilla' });
      updateSimulatorState({ currentDeviceKey: 'Device1' });

      const state = getSimulatorState();
      expect(state.mode).toBe('vanilla');
      expect(state.currentDeviceKey).toBe('Device1');
    });
  });

  describe('State Isolation', () => {
    it('should not modify state when getting it', () => {
      updateSimulatorState({ currentDeviceKey: 'Device1' });
      const state1 = getSimulatorState();
      const state2 = getSimulatorState();

      expect(state1.currentDeviceKey).toBe('Device1');
      expect(state2.currentDeviceKey).toBe('Device1');
      expect(state1).toEqual(state2);
    });

    it('should properly update without leaking state', () => {
      updateSimulatorState({ mode: 'vanilla' });
      updateSimulatorState({ mode: 'ascended', currentDeviceKey: 'Dev1' });

      const state = getSimulatorState();
      expect(state.mode).toBe('ascended');
      expect(state.currentDeviceKey).toBe('Dev1');
    });
  });

  describe('Edge cases', () => {
    it('should handle empty devices array', () => {
      setSimulatorDevices([]);
      const device = findDeviceByKey('AnyDevice');
      expect(device).toBeNull();
    });

    it('should handle null current device', () => {
      updateSimulatorState({ currentDeviceKey: null });
      const device = getCurrentDevice();
      expect(device).toBeNull();
    });

    it('should handle duplicate device keys gracefully', () => {
      const devices = [
        { deviceKey: 'Device1', displayName: 'First' },
        { deviceKey: 'Device1', displayName: 'Duplicate' },
      ];
      setSimulatorDevices(devices);

      // Should return first match
      const device = findDeviceByKey('Device1');
      expect(device?.displayName).toBe('First');
    });

    it('should handle rapid state updates', () => {
      updateSimulatorState({ currentDeviceKey: 'Dev1' });
      updateSimulatorState({ mode: 'vanilla' });
      updateSimulatorState({ currentDeviceKey: 'Dev2' });
      updateSimulatorState({ mode: 'ascended' });

      const state = getSimulatorState();
      expect(state.currentDeviceKey).toBe('Dev2');
      expect(state.mode).toBe('ascended');
    });
  });
});
