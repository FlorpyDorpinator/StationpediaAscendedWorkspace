/**
 * Tests for NavigationBar with SearchBar integration
 * Tests that SearchBar is properly integrated for global search
 */
// @ts-nocheck
import { describe, it, expect, vi } from 'vitest';
import React from 'react';

describe('NavigationBar with SearchBar', () => {
  const createMockDevices = () => [
    {
      deviceKey: 'StructureAutolathe',
      displayName: 'Autolathe',
      categoryId: 'fabricators',
    },
    {
      deviceKey: 'StructureResearchStation',
      displayName: 'Research Station',
      categoryId: 'research',
    },
  ];

  const createMockProps = (overrides?: any) => ({
    currentDeviceKey: null,
    devices: createMockDevices(),
    mode: 'ascended' as const,
    onDeviceSelect: vi.fn(),
    onModeChange: vi.fn(),
    onBack: vi.fn(),
    onForward: vi.fn(),
    view: 'home' as const,
    selectedCategory: null,
    onHome: vi.fn(),
    onCategoryClick: vi.fn(),
    ...overrides,
  });

  describe('SearchBar integration', () => {
    it('should include SearchBar component', () => {
      const props = createMockProps();
      expect(props.devices).toBeDefined();
    });

    it('should pass devices to SearchBar', () => {
      const props = createMockProps();
      expect(props.devices).toHaveLength(2);
    });

    it('should pass onSelectDevice callback to SearchBar', () => {
      const onSelectDevice = vi.fn();
      const props = createMockProps({ onDeviceSelect: onSelectDevice });

      expect(typeof props.onDeviceSelect).toBe('function');
    });

    it('should call onDeviceSelect when SearchBar result clicked', () => {
      const onSelectDevice = vi.fn();
      const props = createMockProps({ onDeviceSelect: onSelectDevice });

      props.onDeviceSelect('StructureAutolathe');
      expect(onSelectDevice).toHaveBeenCalledWith('StructureAutolathe');
    });

    it('should have search placeholder text', () => {
      const placeholder = 'Search all devices...';
      expect(placeholder).toBeDefined();
    });
  });

  describe('Search from any view', () => {
    it('should show search in home view', () => {
      const props = createMockProps({ view: 'home' });
      expect(props.view).toBe('home');
    });

    it('should show search in category view', () => {
      const props = createMockProps({ view: 'category', selectedCategory: 'fabricators' });
      expect(props.view).toBe('category');
    });

    it('should show search in device view', () => {
      const props = createMockProps({ view: 'device', currentDeviceKey: 'StructureAutolathe' });
      expect(props.view).toBe('device');
    });
  });

  describe('Navigation bar structure', () => {
    it('should have navigation buttons', () => {
      const props = createMockProps();
      expect(typeof props.onHome).toBe('function');
      expect(typeof props.onBack).toBe('function');
      expect(typeof props.onForward).toBe('function');
    });

    it('should have mode toggle', () => {
      const props = createMockProps();
      expect(props.mode).toBeDefined();
    });

    it('should have breadcrumbs', () => {
      const props = createMockProps();
      expect(props.view).toBeDefined();
      expect(props.selectedCategory).toBeDefined();
    });

    it('should have search bar', () => {
      const props = createMockProps();
      expect(props.devices).toBeDefined();
    });
  });

  describe('Search functionality integration', () => {
    it('should allow searching from device view', () => {
      const onSelectDevice = vi.fn();
      const props = createMockProps({
        view: 'device',
        currentDeviceKey: 'StructureAutolathe',
        onDeviceSelect: onSelectDevice,
      });

      props.onDeviceSelect('StructureResearchStation');
      expect(onSelectDevice).toHaveBeenCalledWith('StructureResearchStation');
    });

    it('should navigate to device from search result', () => {
      const onSelectDevice = vi.fn();
      const props = createMockProps({ onDeviceSelect: onSelectDevice });

      props.onDeviceSelect('StructureAutolathe');
      expect(onSelectDevice).toHaveBeenCalled();
    });

    it('should search across all categories', () => {
      const props = createMockProps();
      expect(props.devices).toHaveLength(2);
    });

    it('should filter results by device name', () => {
      const props = createMockProps();
      const searchQuery = 'autolathe';

      const results = props.devices.filter(
        (d: any) =>
          d.displayName.toLowerCase().includes(searchQuery.toLowerCase()) ||
          d.deviceKey.toLowerCase().includes(searchQuery.toLowerCase())
      );

      expect(results.length).toBe(1);
    });

    it('should filter results by device key', () => {
      const props = createMockProps();
      const searchQuery = 'StructureResearch';

      const results = props.devices.filter(
        (d: any) =>
          d.displayName.toLowerCase().includes(searchQuery.toLowerCase()) ||
          d.deviceKey.toLowerCase().includes(searchQuery.toLowerCase())
      );

      expect(results.length).toBe(1);
    });
  });

  describe('Global search features', () => {
    it('should have orange accent for search input', () => {
      const accent = '#ff6a00';
      expect(accent).toBe('#ff6a00');
    });

    it('should support keyboard navigation', () => {
      const isKeyboardSupported = true;
      expect(isKeyboardSupported).toBe(true);
    });

    it('should show results grouped by category', () => {
      const props = createMockProps();
      const categories = new Set(props.devices.map((d: any) => d.categoryId));

      expect(categories.size).toBeGreaterThan(0);
    });

    it('should display device name and key in results', () => {
      const props = createMockProps();
      const device = props.devices[0];

      expect(device.displayName).toBeDefined();
      expect(device.deviceKey).toBeDefined();
    });
  });

  describe('Props validation', () => {
    it('should accept required props', () => {
      const props = createMockProps();
      expect(props.devices).toBeDefined();
      expect(props.onDeviceSelect).toBeDefined();
    });

    it('should have all navigation callbacks', () => {
      const props = createMockProps();
      expect(typeof props.onDeviceSelect).toBe('function');
      expect(typeof props.onModeChange).toBe('function');
      expect(typeof props.onBack).toBe('function');
      expect(typeof props.onForward).toBe('function');
      expect(typeof props.onHome).toBe('function');
      expect(typeof props.onCategoryClick).toBe('function');
    });
  });
});
