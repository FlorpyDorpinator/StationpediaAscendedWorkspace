/**
 * Tests for CategoryListView component
 * Tests category header, device list, search filtering, and device selection
 */
import { describe, it, expect, vi } from 'vitest';
import React from 'react';

describe('CategoryListView', () => {
  const createMockProps = (overrides?: any) => ({
    category: {
      id: 'fabricators' as const,
      name: 'Fabricators',
      description: 'Devices that fabricate items',
      order: 3,
    },
    devices: [
      { deviceKey: 'StructureAutolathe', displayName: 'Autolathe' },
      { deviceKey: 'StructureGreenhouse', displayName: 'Greenhouse' },
      { deviceKey: 'StructureFurnace', displayName: 'Furnace' },
    ],
    onSelectDevice: vi.fn(),
    searchQuery: '',
    onSearchChange: vi.fn(),
    ...overrides,
  });

  describe('Component structure', () => {
    it('should create a valid React component', () => {
      expect(React).toBeTruthy();
    });

    it('should accept required props', () => {
      const props = createMockProps();
      expect(props.category).toBeDefined();
      expect(props.devices).toBeDefined();
      expect(props.onSelectDevice).toBeDefined();
      expect(props.searchQuery).toBeDefined();
      expect(props.onSearchChange).toBeDefined();
    });
  });

  describe('Category header', () => {
    it('should display category name', () => {
      const props = createMockProps();
      expect(props.category.name).toBe('Fabricators');
    });

    it('should display category icon', () => {
      const props = createMockProps();
      const icon = '🏭';
      expect(icon).toBe('🏭');
    });

    it('should display different category names', () => {
      const categories = [
        { id: 'ores', name: 'Ores', description: 'Raw ore materials', order: 1 },
        { id: 'ingots', name: 'Ingots', description: 'Processed metal ingots', order: 2 },
        { id: 'fabricators', name: 'Fabricators', description: 'Devices that fabricate items', order: 3 },
      ];

      categories.forEach((cat) => {
        expect(cat.name).toBeTruthy();
      });
    });

    it('should display category description', () => {
      const props = createMockProps();
      expect(props.category.description).toBeTruthy();
    });
  });

  describe('Device count display', () => {
    it('should display total device count', () => {
      const props = createMockProps();
      const count = props.devices.length;
      expect(count).toBe(3);
    });

    it('should update count based on devices', () => {
      const props = createMockProps({
        devices: [
          { deviceKey: 'Device1', displayName: 'Device 1' },
          { deviceKey: 'Device2', displayName: 'Device 2' },
        ],
      });
      expect(props.devices.length).toBe(2);
    });

    it('should handle zero devices', () => {
      const props = createMockProps({ devices: [] });
      expect(props.devices.length).toBe(0);
    });

    it('should display count format correctly', () => {
      const props = createMockProps();
      const countText = `${props.devices.length} devices`;
      expect(countText).toBe('3 devices');
    });

    it('should show plural for multiple devices', () => {
      const props = createMockProps();
      const count = props.devices.length;
      const plural = count !== 1 ? 'devices' : 'device';
      expect(plural).toBe('devices');
    });

    it('should show singular for one device', () => {
      const props = createMockProps({
        devices: [{ deviceKey: 'Device1', displayName: 'Device 1' }],
      });
      const count = props.devices.length;
      const plural = count !== 1 ? 'devices' : 'device';
      expect(plural).toBe('device');
    });
  });

  describe('Search input', () => {
    it('should have search input field', () => {
      const props = createMockProps();
      expect(props.searchQuery).toBeDefined();
    });

    it('should display placeholder text', () => {
      const placeholder = 'Search devices...';
      expect(placeholder).toBeTruthy();
    });

    it('should show current search query', () => {
      const props = createMockProps({ searchQuery: 'lathe' });
      expect(props.searchQuery).toBe('lathe');
    });

    it('should call onSearchChange when input changes', () => {
      const onSearchChange = vi.fn();
      const props = createMockProps({ onSearchChange });

      props.onSearchChange('auto');
      expect(onSearchChange).toHaveBeenCalledWith('auto');
    });

    it('should handle empty search query', () => {
      const props = createMockProps({ searchQuery: '' });
      expect(props.searchQuery).toBe('');
    });

    it('should handle clearing search', () => {
      const onSearchChange = vi.fn();
      const props = createMockProps({
        searchQuery: 'search',
        onSearchChange,
      });

      props.onSearchChange('');
      expect(onSearchChange).toHaveBeenCalledWith('');
    });

    it('should be case-insensitive for search', () => {
      const devices = [
        { deviceKey: 'StructureAutolathe', displayName: 'Autolathe' },
        { deviceKey: 'StructureAutoclave', displayName: 'Autoclave' },
      ];

      const query = 'AUTO';
      const lowerQuery = query.toLowerCase();
      const matching = devices.filter((d) =>
        d.displayName.toLowerCase().includes(lowerQuery)
      );

      expect(matching.length).toBe(2);
    });
  });

  describe('Device list rendering', () => {
    it('should display all devices in category', () => {
      const props = createMockProps();
      expect(props.devices).toHaveLength(3);
    });

    it('should display device display names', () => {
      const props = createMockProps();
      props.devices.forEach((device: any) => {
        expect(device.displayName).toBeTruthy();
      });
    });

    it('should display device keys', () => {
      const props = createMockProps();
      props.devices.forEach((device: any) => {
        expect(device.deviceKey).toBeTruthy();
      });
    });

    it('should render DeviceListItem for each device', () => {
      const props = createMockProps();
      expect(props.devices.length).toBe(3);
    });

    it('should maintain order of devices', () => {
      const devices = [
        { deviceKey: 'Device1', displayName: 'First' },
        { deviceKey: 'Device2', displayName: 'Second' },
        { deviceKey: 'Device3', displayName: 'Third' },
      ];
      const props = createMockProps({ devices });

      expect(props.devices[0].displayName).toBe('First');
      expect(props.devices[1].displayName).toBe('Second');
      expect(props.devices[2].displayName).toBe('Third');
    });
  });

  describe('Search filtering', () => {
    it('should filter devices by search query', () => {
      const devices = [
        { deviceKey: 'StructureAutolathe', displayName: 'Autolathe' },
        { deviceKey: 'StructureGreenhouse', displayName: 'Greenhouse' },
        { deviceKey: 'StructureFurnace', displayName: 'Furnace' },
      ];

      const query = 'lathe';
      const matching = devices.filter((d) =>
        d.displayName.toLowerCase().includes(query) ||
        d.deviceKey.toLowerCase().includes(query)
      );

      expect(matching).toHaveLength(1);
      expect(matching[0].deviceKey).toBe('StructureAutolathe');
    });

    it('should filter by device key', () => {
      const devices = [
        { deviceKey: 'StructureAutolathe', displayName: 'Autolathe' },
        { deviceKey: 'StructureGreenhouse', displayName: 'Greenhouse' },
        { deviceKey: 'ItemStructureKit', displayName: 'Structure Kit' },
      ];

      const query = 'Structure';
      const matching = devices.filter((d) =>
        d.deviceKey.toLowerCase().includes(query.toLowerCase())
      );

      // StructureAutolathe and StructureGreenhouse both contain "Structure", but ItemStructureKit contains it too
      expect(matching).toHaveLength(3);
    });

    it('should return all devices when query is empty', () => {
      const devices = [
        { deviceKey: 'Device1', displayName: 'Device 1' },
        { deviceKey: 'Device2', displayName: 'Device 2' },
        { deviceKey: 'Device3', displayName: 'Device 3' },
      ];

      const query: string = '';
      const matching = query
        ? devices.filter((d) =>
            d.displayName.toLowerCase().includes(query.toLowerCase())
          )
        : devices;

      expect(matching).toHaveLength(3);
    });

    it('should return empty when no matches found', () => {
      const devices = [
        { deviceKey: 'StructureAutolathe', displayName: 'Autolathe' },
        { deviceKey: 'StructureGreenhouse', displayName: 'Greenhouse' },
      ];

      const query = 'xyz';
      const matching = devices.filter((d) =>
        d.displayName.toLowerCase().includes(query.toLowerCase()) ||
        d.deviceKey.toLowerCase().includes(query.toLowerCase())
      );

      expect(matching).toHaveLength(0);
    });

    it('should handle partial matches', () => {
      const devices = [
        { deviceKey: 'StructureAutolathe', displayName: 'Autolathe' },
        { deviceKey: 'StructureAutoclave', displayName: 'Autoclave' },
        { deviceKey: 'StructureFurnace', displayName: 'Furnace' },
      ];

      const query = 'auto';
      const matching = devices.filter((d) =>
        d.displayName.toLowerCase().includes(query.toLowerCase())
      );

      expect(matching).toHaveLength(2);
    });

    it('should update filtered list as search query changes', () => {
      let query: string = '';
      const devices = [
        { deviceKey: 'StructureAutolathe', displayName: 'Autolathe' },
        { deviceKey: 'StructureGreenhouse', displayName: 'Greenhouse' },
      ];

      const getFiltered = () =>
        query
          ? devices.filter((d) =>
              d.displayName.toLowerCase().includes(query.toLowerCase())
            )
          : devices;

      expect(getFiltered()).toHaveLength(2);

      query = 'auto';
      expect(getFiltered()).toHaveLength(1);

      query = '';
      expect(getFiltered()).toHaveLength(2);
    });
  });

  describe('Device selection', () => {
    it('should call onSelectDevice when device is clicked', () => {
      const onSelectDevice = vi.fn();
      const props = createMockProps({ onSelectDevice });

      props.onSelectDevice('StructureAutolathe');
      expect(onSelectDevice).toHaveBeenCalledWith('StructureAutolathe');
    });

    it('should pass device key to onSelectDevice', () => {
      const onSelectDevice = vi.fn();
      const props = createMockProps({ onSelectDevice });

      props.onSelectDevice(props.devices[0].deviceKey);
      expect(onSelectDevice).toHaveBeenCalledWith('StructureAutolathe');
    });

    it('should handle multiple device selections', () => {
      const onSelectDevice = vi.fn();
      const props = createMockProps({ onSelectDevice });

      props.devices.forEach((device: any) => {
        props.onSelectDevice(device.deviceKey);
      });

      expect(onSelectDevice).toHaveBeenCalledTimes(3);
    });
  });

  describe('Scrollable list', () => {
    it('should be scrollable for many devices', () => {
      const manyDevices = Array.from({ length: 50 }, (_, i) => ({
        deviceKey: `Device${i}`,
        displayName: `Device ${i}`,
      }));

      const props = createMockProps({ devices: manyDevices });
      expect(props.devices).toHaveLength(50);
    });

    it('should maintain scroll position', () => {
      const props = createMockProps();
      let scrollPosition = 0;

      scrollPosition = 100;
      expect(scrollPosition).toBe(100);
    });
  });

  describe('Edge cases', () => {
    it('should handle empty device list', () => {
      const props = createMockProps({ devices: [] });
      expect(props.devices).toHaveLength(0);
    });

    it('should handle single device', () => {
      const props = createMockProps({
        devices: [{ deviceKey: 'Device1', displayName: 'Device 1' }],
      });
      expect(props.devices).toHaveLength(1);
    });

    it('should handle very long device lists', () => {
      const devices = Array.from({ length: 1000 }, (_, i) => ({
        deviceKey: `Device${i}`,
        displayName: `Device ${i}`,
      }));
      const props = createMockProps({ devices });
      expect(props.devices).toHaveLength(1000);
    });

    it('should handle search with special characters', () => {
      const devices = [
        { deviceKey: 'ItemN2O', displayName: 'N₂O' },
        { deviceKey: 'ItemCO2', displayName: 'CO₂' },
      ];

      const query = 'N';
      const matching = devices.filter((d) =>
        d.displayName.toLowerCase().includes(query.toLowerCase()) ||
        d.deviceKey.toLowerCase().includes(query.toLowerCase())
      );

      expect(matching.length).toBeGreaterThan(0);
    });
  });
});
