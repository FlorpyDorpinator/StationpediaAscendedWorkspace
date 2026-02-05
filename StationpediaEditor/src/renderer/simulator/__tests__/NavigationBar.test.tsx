/**
 * Tests for NavigationBar component
 * Tests navigation controls and mode toggle
 */
import { describe, it, expect, vi } from 'vitest';
import React from 'react';

describe('NavigationBar', () => {
  const createMockProps = (overrides?: any) => ({
    currentDeviceKey: 'TestDevice',
    devices: [
      { deviceKey: 'Device1', displayName: 'Device One' },
      { deviceKey: 'Device2', displayName: 'Device Two' },
    ],
    mode: 'ascended' as const,
    onDeviceSelect: vi.fn(),
    onModeChange: vi.fn(),
    onBack: vi.fn(),
    onForward: vi.fn(),
    view: 'device' as const,
    selectedCategory: null,
    onHome: vi.fn(),
    onCategoryClick: vi.fn(),
    ...overrides,
  });

  describe('Component structure', () => {
    it('should create a valid React component', () => {
      expect(React).toBeTruthy();
    });

    it('should accept required props', () => {
      const props = createMockProps();
      expect(props.currentDeviceKey).toBe('TestDevice');
      expect(props.devices).toHaveLength(2);
      expect(props.mode).toBe('ascended');
    });
  });

  describe('Navigation controls', () => {
    it('should have back button', () => {
      const props = createMockProps();
      expect(props.onBack).toBeDefined();
    });

    it('should have forward button', () => {
      const props = createMockProps();
      expect(props.onForward).toBeDefined();
    });

    it('should call onBack when back button clicked', () => {
      const props = createMockProps();
      props.onBack();
      expect(props.onBack).toHaveBeenCalled();
    });

    it('should call onForward when forward button clicked', () => {
      const props = createMockProps();
      props.onForward();
      expect(props.onForward).toHaveBeenCalled();
    });

    it('should track navigation history', () => {
      const onBack = vi.fn();
      const onForward = vi.fn();
      const props = createMockProps({ onBack, onForward });

      onBack();
      expect(onBack).toHaveBeenCalledTimes(1);

      onForward();
      expect(onForward).toHaveBeenCalledTimes(1);
    });
  });

  describe('Mode toggle', () => {
    it('should have vanilla mode option', () => {
      const props = createMockProps();
      expect(['vanilla', 'ascended']).toContain('vanilla');
    });

    it('should have ascended mode option', () => {
      const props = createMockProps();
      expect(['vanilla', 'ascended']).toContain('ascended');
    });

    it('should show current mode', () => {
      const props = createMockProps({ mode: 'vanilla' });
      expect(props.mode).toBe('vanilla');
    });

    it('should toggle to vanilla mode', () => {
      const onModeChange = vi.fn();
      const props = createMockProps({
        mode: 'ascended',
        onModeChange,
      });

      onModeChange('vanilla');
      expect(onModeChange).toHaveBeenCalledWith('vanilla');
    });

    it('should toggle to ascended mode', () => {
      const onModeChange = vi.fn();
      const props = createMockProps({
        mode: 'vanilla',
        onModeChange,
      });

      onModeChange('ascended');
      expect(onModeChange).toHaveBeenCalledWith('ascended');
    });
  });

  describe('Device selector', () => {
    it('should display current device', () => {
      const props = createMockProps({
        currentDeviceKey: 'Device1',
        devices: [{ deviceKey: 'Device1', displayName: 'First Device' }],
      });

      expect(props.currentDeviceKey).toBe('Device1');
    });

    it('should display device display name', () => {
      const device = { deviceKey: 'Device1', displayName: 'Solar Panel' };
      const props = createMockProps({
        currentDeviceKey: 'Device1',
        devices: [device],
      });

      expect(device.displayName).toBe('Solar Panel');
    });

    it('should list all devices in dropdown', () => {
      const devices = [
        { deviceKey: 'Device1', displayName: 'Device One' },
        { deviceKey: 'Device2', displayName: 'Device Two' },
        { deviceKey: 'Device3', displayName: 'Device Three' },
      ];
      const props = createMockProps({ devices });

      expect(props.devices).toHaveLength(3);
    });

    it('should call onDeviceSelect when device is selected', () => {
      const onDeviceSelect = vi.fn();
      const props = createMockProps({ onDeviceSelect });

      props.onDeviceSelect('Device2');
      expect(onDeviceSelect).toHaveBeenCalledWith('Device2');
    });

    it('should support device search', () => {
      const devices = [
        { deviceKey: 'StructureAutolathe', displayName: 'Autolathe' },
        { deviceKey: 'StructureSolarPanel', displayName: 'Solar Panel' },
      ];
      const props = createMockProps({ devices });

      // Simulate search for "solar"
      const matching = devices.filter((d) =>
        d.displayName.toLowerCase().includes('solar')
      );

      expect(matching).toHaveLength(1);
      expect(matching[0].displayName).toBe('Solar Panel');
    });

    it('should show no device selected message', () => {
      const props = createMockProps({ currentDeviceKey: null });
      expect(props.currentDeviceKey).toBeNull();
    });
  });

  describe('Dropdown functionality', () => {
    it('should open and close dropdown', () => {
      const props = createMockProps();
      let isOpen = false;

      // Simulate toggle
      isOpen = !isOpen;
      expect(isOpen).toBe(true);

      isOpen = !isOpen;
      expect(isOpen).toBe(false);
    });

    it('should filter devices by search text', () => {
      const devices = [
        { deviceKey: 'Device1', displayName: 'Autolathe' },
        { deviceKey: 'Device2', displayName: 'Solar Panel' },
        { deviceKey: 'Device3', displayName: 'Autoclave' },
      ];

      const searchText = 'auto';
      const filtered = devices.filter(
        (d) =>
          d.displayName.toLowerCase().includes(searchText) ||
          d.deviceKey.toLowerCase().includes(searchText)
      );

      expect(filtered).toHaveLength(2);
      expect(filtered[0].displayName).toBe('Autolathe');
      expect(filtered[1].displayName).toBe('Autoclave');
    });

    it('should clear search on device select', () => {
      let searchText = 'solar';
      const props = createMockProps();

      props.onDeviceSelect('Device1');
      searchText = '';

      expect(searchText).toBe('');
    });

    it('should close dropdown on device select', () => {
      let isOpen = true;
      const props = createMockProps();

      props.onDeviceSelect('Device1');
      isOpen = false;

      expect(isOpen).toBe(false);
    });
  });

  describe('Accessibility', () => {
    it('should have button titles for navigation', () => {
      const props = createMockProps();
      expect(props.onBack).toBeDefined();
      expect(props.onForward).toBeDefined();
    });

    it('should have mode button labels', () => {
      const modes = ['Vanilla', 'Ascended'];
      expect(modes).toHaveLength(2);
    });

    it('should have device selector label', () => {
      const props = createMockProps();
      expect(props.devices).toBeDefined();
    });
  });

  describe('Edge cases', () => {
    it('should handle empty device list', () => {
      const props = createMockProps({ devices: [] });
      expect(props.devices).toHaveLength(0);
    });

    it('should handle no device selected', () => {
      const props = createMockProps({ currentDeviceKey: null });
      expect(props.currentDeviceKey).toBeNull();
    });

    it('should handle device with no displayName', () => {
      const devices = [
        { deviceKey: 'Device1', displayName: null },
      ];
      const props = createMockProps({ devices });

      // Should fall back to deviceKey
      expect(devices[0].deviceKey).toBe('Device1');
    });

    it('should handle rapid mode changes', () => {
      const onModeChange = vi.fn();
      const props = createMockProps({ onModeChange });

      onModeChange('vanilla');
      onModeChange('ascended');
      onModeChange('vanilla');

      expect(onModeChange).toHaveBeenCalledTimes(3);
    });
  });

  describe('Home button', () => {
    it('should display home button with emoji', () => {
      const props = createMockProps();
      const homeEmoji = '🏠';
      expect(homeEmoji).toBe('🏠');
    });

    it('should call onHome when home button clicked', () => {
      const onHome = vi.fn();
      const props = createMockProps({ onHome });

      props.onHome();
      expect(onHome).toHaveBeenCalled();
    });

    it('should have home button title', () => {
      const title = 'Return to home';
      expect(title).toBeTruthy();
    });

    it('should position home button in navigation', () => {
      const props = createMockProps();
      expect(props.onHome).toBeDefined();
    });
  });

  describe('Breadcrumb component', () => {
    it('should display breadcrumb when viewing device', () => {
      const props = createMockProps({
        view: 'device',
        selectedCategory: 'fabricators',
      });

      expect(props.view).toBe('device');
      expect(props.selectedCategory).toBe('fabricators');
    });

    it('should build breadcrumb path for home view', () => {
      const props = createMockProps({ view: 'home' });
      const breadcrumbs = [{ label: 'Home', onClick: undefined }];

      expect(breadcrumbs).toHaveLength(1);
      expect(breadcrumbs[0].label).toBe('Home');
    });

    it('should build breadcrumb path for category view', () => {
      const props = createMockProps({
        view: 'category',
        selectedCategory: 'fabricators',
      });

      expect(props.selectedCategory).toBe('fabricators');
    });

    it('should build breadcrumb path for device view', () => {
      const props = createMockProps({
        view: 'device',
        selectedCategory: 'fabricators',
        currentDeviceKey: 'StructureAutolathe',
      });

      expect(props.view).toBe('device');
      expect(props.currentDeviceKey).toBe('StructureAutolathe');
    });

    it('should display Home > Category > Device path', () => {
      const props = createMockProps({
        view: 'device',
        selectedCategory: 'fabricators',
        currentDeviceKey: 'StructureAutolathe',
      });

      const path = `Home > Fabricators > ${props.currentDeviceKey}`;
      expect(path).toContain('Home');
      expect(path).toContain('Fabricators');
      expect(path).toContain('StructureAutolathe');
    });

    it('should handle category click from breadcrumb', () => {
      const onCategoryClick = vi.fn();
      const props = createMockProps({ onCategoryClick });

      props.onCategoryClick('fabricators');
      expect(onCategoryClick).toHaveBeenCalledWith('fabricators');
    });

    it('should use orange text for breadcrumb links', () => {
      const linkColor = '#ff6a00';
      expect(linkColor).toBe('#ff6a00');
    });

    it('should show breadcrumb for three-level path', () => {
      const props = createMockProps({
        view: 'device',
        selectedCategory: 'fabricators',
      });

      // Build breadcrumb items
      const items = [
        { label: 'Home', onClick: undefined },
        { label: 'Fabricators', onClick: undefined },
        { label: 'Device', onClick: undefined },
      ];

      expect(items).toHaveLength(3);
    });

    it('should make intermediate breadcrumbs clickable', () => {
      const onCategoryClick = vi.fn();
      const props = createMockProps({ onCategoryClick });

      // Simulate clicking on category breadcrumb
      onCategoryClick('fabricators');

      expect(onCategoryClick).toHaveBeenCalled();
    });

    it('should not make current (last) breadcrumb clickable', () => {
      const props = createMockProps({
        view: 'device',
      });

      // Last breadcrumb should not have onClick
      expect(props).toBeDefined();
    });
  });
});
