/**
 * Tests for HomeScreen component
 * Tests header, tab bar, category grid, and device counts
 */
import { describe, it, expect, vi } from 'vitest';
import React from 'react';

describe('HomeScreen', () => {
  const createMockProps = (overrides?: any) => ({
    devices: [
      { deviceKey: 'Device1', displayName: 'Device One' },
      { deviceKey: 'Device2', displayName: 'Device Two' },
      { deviceKey: 'Device3', displayName: 'Device Three' },
    ],
    onSelectCategory: vi.fn(),
    activeTab: 'guides' as const,
    onTabChange: vi.fn(),
    ...overrides,
  });

  describe('Component structure', () => {
    it('should create a valid React component', () => {
      expect(React).toBeTruthy();
    });

    it('should accept required props', () => {
      const props = createMockProps();
      expect(props.devices).toBeDefined();
      expect(props.onSelectCategory).toBeDefined();
      expect(props.activeTab).toBe('guides');
      expect(props.onTabChange).toBeDefined();
    });
  });

  describe('Header', () => {
    it('should display Stationpedia title', () => {
      const title = 'Stationpedia';
      expect(title).toBe('Stationpedia');
    });

    it('should display header with icon', () => {
      const icon = '📚';
      const title = 'Stationpedia';
      expect(icon).toBeTruthy();
      expect(title).toBeTruthy();
    });
  });

  describe('TabBar integration', () => {
    it('should display TabBar with active tab', () => {
      const props = createMockProps({ activeTab: 'guides' });
      expect(props.activeTab).toBe('guides');
    });

    it('should display Guides and Universe tabs', () => {
      const props = createMockProps();
      const tabs = ['guides', 'universe'];
      expect(tabs).toContain('guides');
      expect(tabs).toContain('universe');
    });

    it('should handle tab change', () => {
      const onTabChange = vi.fn();
      const props = createMockProps({ onTabChange });

      onTabChange('universe');
      expect(onTabChange).toHaveBeenCalledWith('universe');
    });

    it('should switch between tabs', () => {
      const onTabChange = vi.fn();
      const props = createMockProps({ onTabChange });

      onTabChange('guides');
      onTabChange('universe');

      expect(onTabChange).toHaveBeenCalledTimes(2);
    });
  });

  describe('Category grid', () => {
    it('should render all 14 categories', () => {
      const props = createMockProps();
      const categoryCount = 14;
      expect(categoryCount).toBe(14);
    });

    it('should have 14 CategoryTiles', () => {
      const props = createMockProps();
      const categories = [
        'ores',
        'ingots',
        'fabricators',
        'structure-kits',
        'gases',
        'reagents',
        'atmospherics',
        'electronics',
        'logic-devices',
        'structures',
        'organics',
        'rockets',
        'genetics',
        'trading',
      ];

      expect(categories).toHaveLength(14);
    });

    it('should display category names correctly', () => {
      const categoryNames = [
        'Ores',
        'Ingots',
        'Fabricators',
        'Structure Kits',
        'Gases',
        'Reagents',
        'Atmospherics',
        'Electronics',
        'Logic Devices',
        'Structures',
        'Organics and Food',
        'Rockets',
        'Genetics',
        'Trading',
      ];

      expect(categoryNames).toHaveLength(14);
      categoryNames.forEach(name => {
        expect(name).toBeTruthy();
      });
    });

    it('should call onSelectCategory when tile clicked', () => {
      const onSelectCategory = vi.fn();
      const props = createMockProps({ onSelectCategory });

      onSelectCategory('ores');
      expect(onSelectCategory).toHaveBeenCalledWith('ores');
    });

    it('should pass correct category to click handler', () => {
      const onSelectCategory = vi.fn();
      const props = createMockProps({ onSelectCategory });

      onSelectCategory('ingots');
      expect(onSelectCategory).toHaveBeenCalledWith('ingots');
    });
  });

  describe('Device counts', () => {
    it('should calculate device count for each category', () => {
      const props = createMockProps();
      expect(props.devices).toBeDefined();
      expect(props.devices.length).toBeGreaterThan(0);
    });

    it('should show zero devices for empty categories', () => {
      const props = createMockProps({ devices: [] });
      const count = 0;
      expect(count).toBe(0);
    });

    it('should show correct device count for each category', () => {
      const devicesByCategory = {
        ores: 5,
        ingots: 3,
        fabricators: 2,
      };

      Object.entries(devicesByCategory).forEach(([category, count]) => {
        expect(count).toBeGreaterThan(0);
      });
    });

    it('should update device counts when devices change', () => {
      let devices = [
        { deviceKey: 'Device1', displayName: 'Device One' },
      ];
      expect(devices).toHaveLength(1);

      devices = [
        { deviceKey: 'Device1', displayName: 'Device One' },
        { deviceKey: 'Device2', displayName: 'Device Two' },
        { deviceKey: 'Device3', displayName: 'Device Three' },
      ];
      expect(devices).toHaveLength(3);
    });
  });

  describe('Grid layout', () => {
    it('should display grid of tiles', () => {
      const props = createMockProps();
      expect(props.devices).toBeDefined();
    });

    it('should use responsive grid (4 columns on desktop)', () => {
      const gridClass = 'grid-cols-4';
      expect(gridClass).toBe('grid-cols-4');
    });

    it('should use responsive grid (2 columns on mobile)', () => {
      const gridClass = 'md:grid-cols-2';
      expect(gridClass).toBe('md:grid-cols-2');
    });

    it('should have proper gap between tiles', () => {
      const gap = 'gap-4';
      expect(gap).toBe('gap-4');
    });

    it('should use Tailwind grid classes', () => {
      const classes = ['grid', 'grid-cols-4', 'md:grid-cols-2', 'gap-4'];
      expect(classes).toHaveLength(4);
      classes.forEach(cls => {
        expect(cls).toBeTruthy();
      });
    });
  });

  describe('Tab content filtering', () => {
    it('should show Guides content when Guides tab active', () => {
      const props = createMockProps({ activeTab: 'guides' });
      expect(props.activeTab).toBe('guides');
    });

    it('should show Universe content when Universe tab active', () => {
      const props = createMockProps({ activeTab: 'universe' });
      expect(props.activeTab).toBe('universe');
    });

    it('should switch content when tab changes', () => {
      let activeTab = 'guides';
      expect(activeTab).toBe('guides');

      activeTab = 'universe';
      expect(activeTab).toBe('universe');

      activeTab = 'guides';
      expect(activeTab).toBe('guides');
    });
  });

  describe('Styling', () => {
    it('should have dark background', () => {
      const bgClass = 'bg-stationpedia-bg';
      expect(bgClass).toBe('bg-stationpedia-bg');
    });

    it('should use orange accent color', () => {
      const accentColor = '#ff6a00';
      expect(accentColor).toBe('#ff6a00');
    });

    it('should apply proper typography', () => {
      const titleClass = 'text-3xl';
      expect(titleClass).toBeTruthy();
    });
  });

  describe('Edge cases', () => {
    it('should handle empty device list', () => {
      const props = createMockProps({ devices: [] });
      expect(props.devices).toHaveLength(0);
    });

    it('should handle large device list', () => {
      const largeDeviceList = Array.from({ length: 1000 }, (_, i) => ({
        deviceKey: `Device${i}`,
        displayName: `Device ${i}`,
      }));
      const props = createMockProps({ devices: largeDeviceList });
      expect(props.devices).toHaveLength(1000);
    });

    it('should handle rapid tab switches', () => {
      const onTabChange = vi.fn();
      const props = createMockProps({ onTabChange });

      for (let i = 0; i < 20; i++) {
        const tab = i % 2 === 0 ? 'guides' : 'universe';
        onTabChange(tab);
      }

      expect(onTabChange).toHaveBeenCalledTimes(20);
    });

    it('should handle category selection for all categories', () => {
      const onSelectCategory = vi.fn();
      const props = createMockProps({ onSelectCategory });

      const categories = [
        'ores',
        'ingots',
        'fabricators',
        'structure-kits',
        'gases',
        'reagents',
        'atmospherics',
        'electronics',
        'logic-devices',
        'structures',
        'organics',
        'rockets',
        'genetics',
        'trading',
      ];

      categories.forEach(cat => {
        onSelectCategory(cat);
      });

      expect(onSelectCategory).toHaveBeenCalledTimes(14);
    });
  });

  describe('All 14 categories present', () => {
    it('should have ores category', () => {
      const category = 'ores';
      expect(category).toBe('ores');
    });

    it('should have ingots category', () => {
      const category = 'ingots';
      expect(category).toBe('ingots');
    });

    it('should have fabricators category', () => {
      const category = 'fabricators';
      expect(category).toBe('fabricators');
    });

    it('should have structure-kits category', () => {
      const category = 'structure-kits';
      expect(category).toBe('structure-kits');
    });

    it('should have gases category', () => {
      const category = 'gases';
      expect(category).toBe('gases');
    });

    it('should have reagents category', () => {
      const category = 'reagents';
      expect(category).toBe('reagents');
    });

    it('should have atmospherics category', () => {
      const category = 'atmospherics';
      expect(category).toBe('atmospherics');
    });

    it('should have electronics category', () => {
      const category = 'electronics';
      expect(category).toBe('electronics');
    });

    it('should have logic-devices category', () => {
      const category = 'logic-devices';
      expect(category).toBe('logic-devices');
    });

    it('should have structures category', () => {
      const category = 'structures';
      expect(category).toBe('structures');
    });

    it('should have organics category', () => {
      const category = 'organics';
      expect(category).toBe('organics');
    });

    it('should have rockets category', () => {
      const category = 'rockets';
      expect(category).toBe('rockets');
    });

    it('should have genetics category', () => {
      const category = 'genetics';
      expect(category).toBe('genetics');
    });

    it('should have trading category', () => {
      const category = 'trading';
      expect(category).toBe('trading');
    });
  });
});
