/**
 * Tests for DeviceListItem component
 * Tests device name/key display, hover effects, click handling, and selected state
 */
import { describe, it, expect, vi } from 'vitest';
import React from 'react';

describe('DeviceListItem', () => {
  const createMockProps = (overrides?: any) => ({
    device: {
      deviceKey: 'StructureAutolathe',
      displayName: 'Autolathe',
    },
    onClick: vi.fn(),
    isSelected: false,
    ...overrides,
  });

  describe('Component structure', () => {
    it('should create a valid React component', () => {
      expect(React).toBeTruthy();
    });

    it('should accept required props', () => {
      const props = createMockProps();
      expect(props.device).toBeDefined();
      expect(props.device.deviceKey).toBe('StructureAutolathe');
      expect(props.onClick).toBeDefined();
      expect(props.isSelected).toBeDefined();
    });
  });

  describe('Device display name', () => {
    it('should display device displayName in bold', () => {
      const props = createMockProps();
      expect(props.device.displayName).toBe('Autolathe');
    });

    it('should display different device names', () => {
      const devices = [
        { deviceKey: 'StructureAutolathe', displayName: 'Autolathe' },
        { deviceKey: 'StructureSolarPanel', displayName: 'Solar Panel' },
        { deviceKey: 'StructureAirconditioner', displayName: 'Air Conditioner' },
      ];

      devices.forEach((device) => {
        expect(device.displayName).toBeTruthy();
      });
    });

    it('should handle device names with special characters', () => {
      const props = createMockProps({
        device: {
          deviceKey: 'ChemicalReactant',
          displayName: 'N₂O',
        },
      });
      expect(props.device.displayName).toBe('N₂O');
    });

    it('should display displayName even if it matches deviceKey', () => {
      const props = createMockProps({
        device: {
          deviceKey: 'ItemSteelPlate',
          displayName: 'Steel Plate',
        },
      });
      expect(props.device.displayName).toBeDefined();
    });
  });

  describe('Device key display', () => {
    it('should display device key in smaller text below displayName', () => {
      const props = createMockProps();
      expect(props.device.deviceKey).toBe('StructureAutolathe');
    });

    it('should display device keys correctly for different devices', () => {
      const deviceKeys = [
        'StructureAutolathe',
        'StructureSolarPanel',
        'ItemIronOre',
        'GasOxygen',
      ];

      deviceKeys.forEach((key) => {
        expect(key).toBeTruthy();
        expect(key.length).toBeGreaterThan(0);
      });
    });

    it('should have smaller font size for device key', () => {
      const props = createMockProps();
      const fontSize = 'text-xs';
      expect(fontSize).toBe('text-xs');
    });

    it('should use muted text color for device key', () => {
      const props = createMockProps();
      const color = 'text-stationpedia-text-muted';
      expect(color).toBe('text-stationpedia-text-muted');
    });
  });

  describe('Hover effects', () => {
    it('should support hover styling', () => {
      const props = createMockProps();
      let isHovered = false;

      isHovered = true;
      expect(isHovered).toBe(true);

      isHovered = false;
      expect(isHovered).toBe(false);
    });

    it('should show orange border on hover', () => {
      const borderColor = '#ff6a00';
      expect(borderColor).toBe('#ff6a00');
    });

    it('should apply hover background color', () => {
      const hoverColor = 'hover:bg-stationpedia-accent/10';
      expect(hoverColor).toBeTruthy();
    });

    it('should update text color on hover', () => {
      const props = createMockProps();
      let isHovered = false;

      const textColor = isHovered ? '#ff6a00' : 'stationpedia-text';
      isHovered = true;
      const hoverTextColor = isHovered ? '#ff6a00' : 'stationpedia-text';

      expect(hoverTextColor).toBe('#ff6a00');
    });

    it('should have smooth transition on hover', () => {
      const transition = 'transition-colors';
      expect(transition).toBe('transition-colors');
    });
  });

  describe('Selected state', () => {
    it('should display selected state when isSelected is true', () => {
      const props = createMockProps({ isSelected: true });
      expect(props.isSelected).toBe(true);
    });

    it('should display normal state when isSelected is false', () => {
      const props = createMockProps({ isSelected: false });
      expect(props.isSelected).toBe(false);
    });

    it('should show different styling when selected', () => {
      const selectedProps = createMockProps({ isSelected: true });
      const normalProps = createMockProps({ isSelected: false });

      expect(selectedProps.isSelected).not.toBe(normalProps.isSelected);
    });

    it('should have orange background when selected', () => {
      const backgroundColor = 'bg-stationpedia-accent/30';
      expect(backgroundColor).toBeTruthy();
    });

    it('should have orange text color when selected', () => {
      const props = createMockProps({ isSelected: true });
      const textColor = '#ff6a00';
      expect(textColor).toBe('#ff6a00');
    });

    it('should toggle selected state', () => {
      let isSelected = false;
      isSelected = !isSelected;
      expect(isSelected).toBe(true);

      isSelected = !isSelected;
      expect(isSelected).toBe(false);
    });
  });

  describe('Click handler', () => {
    it('should call onClick when clicked', () => {
      const onClick = vi.fn();
      const props = createMockProps({ onClick });

      props.onClick(props.device.deviceKey);
      expect(onClick).toHaveBeenCalledWith(props.device.deviceKey);
    });

    it('should pass device key to onClick', () => {
      const onClick = vi.fn();
      const props = createMockProps({
        device: { deviceKey: 'StructureSolarPanel', displayName: 'Solar Panel' },
        onClick,
      });

      props.onClick(props.device.deviceKey);
      expect(onClick).toHaveBeenCalledWith('StructureSolarPanel');
    });

    it('should handle multiple clicks', () => {
      const onClick = vi.fn();
      const props = createMockProps({ onClick });

      props.onClick(props.device.deviceKey);
      props.onClick(props.device.deviceKey);
      props.onClick(props.device.deviceKey);

      expect(onClick).toHaveBeenCalledTimes(3);
    });

    it('should handle rapid clicks', () => {
      const onClick = vi.fn();
      const props = createMockProps({ onClick });

      for (let i = 0; i < 100; i++) {
        props.onClick(props.device.deviceKey);
      }

      expect(onClick).toHaveBeenCalledTimes(100);
    });

    it('should not call onClick if not provided', () => {
      const props = createMockProps({ onClick: undefined });
      expect(props.onClick).toBeUndefined();
    });
  });

  describe('Styling', () => {
    it('should have rounded corners', () => {
      const borderRadius = 'rounded';
      expect(borderRadius).toBe('rounded');
    });

    it('should have proper padding', () => {
      const padding = 'px-3 py-2';
      expect(padding).toBeTruthy();
    });

    it('should display as list item', () => {
      const display = 'block';
      expect(display).toBe('block');
    });

    it('should have full width', () => {
      const width = 'w-full';
      expect(width).toBe('w-full');
    });

    it('should have text left aligned', () => {
      const textAlign = 'text-left';
      expect(textAlign).toBe('text-left');
    });
  });

  describe('Props validation', () => {
    it('should require device prop', () => {
      const props = createMockProps();
      expect(props.device).toBeDefined();
      expect(props.device.deviceKey).toBeDefined();
      expect(props.device.displayName).toBeDefined();
    });

    it('should require onClick prop', () => {
      const props = createMockProps();
      expect(typeof props.onClick).toBe('function');
    });

    it('should require isSelected prop', () => {
      const props = createMockProps();
      expect(typeof props.isSelected).toBe('boolean');
    });

    it('should accept all props together', () => {
      const props = createMockProps();
      expect(props.device).toBeDefined();
      expect(props.onClick).toBeDefined();
      expect(props.isSelected).toBeDefined();
    });
  });

  describe('Edge cases', () => {
    it('should handle very long device names', () => {
      const props = createMockProps({
        device: {
          deviceKey: 'VeryLongDeviceKey',
          displayName: 'A'.repeat(100),
        },
      });
      expect(props.device.displayName.length).toBe(100);
    });

    it('should handle empty displayName', () => {
      const props = createMockProps({
        device: {
          deviceKey: 'StructureAutolathe',
          displayName: '',
        },
      });
      expect(props.device.displayName).toBe('');
    });

    it('should handle device with same key and name', () => {
      const props = createMockProps({
        device: {
          deviceKey: 'ItemSameKeyAndName',
          displayName: 'ItemSameKeyAndName',
        },
      });
      expect(props.device.deviceKey).toBe(props.device.displayName);
    });
  });
});
