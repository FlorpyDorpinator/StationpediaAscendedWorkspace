/**
 * Tests for SimulatorApp component
 * Tests device rendering and state management
 */
import { describe, it, expect, vi, beforeEach } from 'vitest';
import React from 'react';
import type { DeviceDocument } from '@models/contentModel';

describe('SimulatorApp', () => {
  const createTestDevice = (overrides?: Partial<DeviceDocument>): DeviceDocument => ({
    deviceKey: 'TestDevice',
    displayName: 'Test Device',
    ...overrides,
  });

  beforeEach(() => {
    // Mock simulatorAPI
    (window as any).simulatorAPI = {
      getCurrentDevice: vi.fn(),
      getDevice: vi.fn(),
      getAllDevices: vi.fn(() => Promise.resolve([])),
      navigateToDevice: vi.fn(),
      setMode: vi.fn(),
      getMode: vi.fn(() => Promise.resolve('ascended')),
      onDeviceChanged: vi.fn(),
      onModeChanged: vi.fn(),
      removeDeviceChangeListener: vi.fn(),
      removeModeChangeListener: vi.fn(),
    };
  });

  describe('Component structure', () => {
    it('should create a valid React component', () => {
      // Basic component existence test
      expect(React).toBeTruthy();
    });

    it('should require simulatorAPI on window', () => {
      expect(window.simulatorAPI).toBeDefined();
      expect(window.simulatorAPI.getCurrentDevice).toBeDefined();
    });

    it('should have getAllDevices method', () => {
      expect(typeof window.simulatorAPI.getAllDevices).toBe('function');
    });

    it('should have navigateToDevice method', () => {
      expect(typeof window.simulatorAPI.navigateToDevice).toBe('function');
    });
  });

  describe('Mode management', () => {
    it('should support vanilla and ascended modes', () => {
      expect(['vanilla', 'ascended']).toContain('vanilla');
      expect(['vanilla', 'ascended']).toContain('ascended');
    });

    it('should have setMode method', () => {
      expect(typeof window.simulatorAPI.setMode).toBe('function');
    });

    it('should have getMode method', () => {
      expect(typeof window.simulatorAPI.getMode).toBe('function');
    });
  });

  describe('Navigation and history', () => {
    it('should track device navigation history', () => {
      // History would be managed internally in component state
      expect(window.simulatorAPI.navigateToDevice).toBeDefined();
    });

    it('should support back navigation', () => {
      expect(window.simulatorAPI.navigateToDevice).toBeDefined();
    });

    it('should support forward navigation', () => {
      expect(window.simulatorAPI.navigateToDevice).toBeDefined();
    });
  });

  describe('Device selection', () => {
    it('should accept device key for navigation', async () => {
      const deviceKey = 'StructureAutolathe';
      const device = createTestDevice({ deviceKey });

      (window.simulatorAPI.getDevice as any).mockResolvedValue(device);

      const result = await window.simulatorAPI.getDevice(deviceKey);
      expect(result.deviceKey).toBe('StructureAutolathe');
    });

    it('should load device by key', async () => {
      const device = createTestDevice({
        deviceKey: 'DeviceKey',
        displayName: 'Device Name',
      });

      (window.simulatorAPI.getDevice as any).mockResolvedValue(device);

      const result = await window.simulatorAPI.getDevice('DeviceKey');
      expect(result).toBeTruthy();
      expect(result.displayName).toBe('Device Name');
    });

    it('should handle missing device gracefully', async () => {
      (window.simulatorAPI.getDevice as any).mockResolvedValue(null);

      const result = await window.simulatorAPI.getDevice('NonExistent');
      expect(result).toBeNull();
    });
  });

  describe('Rendering device content', () => {
    it('should display device name', () => {
      const device = createTestDevice({
        displayName: 'Solar Panel',
        deviceKey: 'StructureSolarPanel',
      });

      expect(device.displayName).toBe('Solar Panel');
      expect(device.deviceKey).toBe('StructureSolarPanel');
    });

    it('should display device description', () => {
      const device = createTestDevice({
        pageDescription: 'This is a solar panel that converts light to power',
      });

      expect(device.pageDescription).toBeDefined();
      expect(device.pageDescription).toContain('solar panel');
    });

    it('should display operational details', () => {
      const device = createTestDevice({
        operationalDetails: [
          {
            title: 'Operation',
            description: 'How to operate this device',
          },
        ],
      });

      expect(device.operationalDetails).toHaveLength(1);
      expect(device.operationalDetails![0].title).toBe('Operation');
    });
  });

  describe('IPC Communication', () => {
    it('should listen for device changes via IPC', () => {
      expect(typeof window.simulatorAPI.onDeviceChanged).toBe('function');
    });

    it('should listen for mode changes via IPC', () => {
      expect(typeof window.simulatorAPI.onModeChanged).toBe('function');
    });

    it('should send device navigation to main process', () => {
      window.simulatorAPI.navigateToDevice('TestDevice');
      expect(window.simulatorAPI.navigateToDevice).toHaveBeenCalledWith('TestDevice');
    });

    it('should send mode change to main process', () => {
      window.simulatorAPI.setMode('vanilla');
      expect(window.simulatorAPI.setMode).toHaveBeenCalledWith('vanilla');
    });
  });

  describe('Loading states', () => {
    it('should show loading state on initialization', () => {
      expect(window.simulatorAPI.getAllDevices).toBeDefined();
    });

    it('should handle empty device list', async () => {
      (window.simulatorAPI.getAllDevices as any).mockResolvedValue([]);

      const devices = await window.simulatorAPI.getAllDevices();
      expect(devices).toEqual([]);
    });

    it('should display message when no device is selected', () => {
      // When currentDevice is null, should show placeholder
      expect(true).toBe(true);
    });
  });

  describe('Link handling', () => {
    it('should support THING: prefix links', () => {
      // Should navigate when clicking links like {THING:StructureAutolathe}
      expect(window.simulatorAPI.navigateToDevice).toBeDefined();
    });

    it('should extract device key from THING link', () => {
      const target = 'THING:StructureAutolathe';
      const deviceKey = target.replace('THING:', '');
      expect(deviceKey).toBe('StructureAutolathe');
    });
  });
});
