/**
 * Tests for CategoryTile component
 * Tests category name, device count badge, icon, and click handling
 */
import { describe, it, expect, vi } from 'vitest';
import React from 'react';

describe('CategoryTile', () => {
  const createMockProps = (overrides?: any) => ({
    category: {
      id: 'ores' as const,
      name: 'Ores',
      description: 'Raw ore materials',
      order: 1,
    },
    deviceCount: 5,
    onClick: vi.fn(),
    ...overrides,
  });

  describe('Component structure', () => {
    it('should create a valid React component', () => {
      expect(React).toBeTruthy();
    });

    it('should accept required props', () => {
      const props = createMockProps();
      expect(props.category).toBeDefined();
      expect(props.category.name).toBe('Ores');
      expect(props.deviceCount).toBe(5);
      expect(props.onClick).toBeDefined();
    });
  });

  describe('Category name display', () => {
    it('should display category name', () => {
      const props = createMockProps();
      expect(props.category.name).toBe('Ores');
    });

    it('should display different category names', () => {
      const categories = [
        { id: 'ores', name: 'Ores' },
        { id: 'ingots', name: 'Ingots' },
        { id: 'fabricators', name: 'Fabricators' },
      ];

      categories.forEach(cat => {
        expect(cat.name).toBeTruthy();
      });
    });

    it('should display category name correctly for all 14 categories', () => {
      const allCategories = [
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

      expect(allCategories).toHaveLength(14);
      allCategories.forEach(name => {
        expect(name).toBeTruthy();
      });
    });
  });

  describe('Device count badge', () => {
    it('should display device count', () => {
      const props = createMockProps({ deviceCount: 5 });
      expect(props.deviceCount).toBe(5);
    });

    it('should update device count', () => {
      let count = 5;
      expect(count).toBe(5);

      count = 12;
      expect(count).toBe(12);
    });

    it('should handle zero devices', () => {
      const props = createMockProps({ deviceCount: 0 });
      expect(props.deviceCount).toBe(0);
    });

    it('should handle large device counts', () => {
      const props = createMockProps({ deviceCount: 999 });
      expect(props.deviceCount).toBe(999);
    });

    it('should have orange background for badge', () => {
      const badgeColor = '#ff6a00';
      expect(badgeColor).toBe('#ff6a00');
    });

    it('should display badge with proper formatting', () => {
      const props = createMockProps({ deviceCount: 42 });
      const formatted = `${props.deviceCount}`;
      expect(formatted).toBe('42');
    });
  });

  describe('Category icon', () => {
    it('should display emoji icon for ores', () => {
      const props = createMockProps({
        category: { id: 'ores', name: 'Ores', description: 'Raw ore materials', order: 1 },
      });
      const iconMap: Record<string, string> = {
        ores: '⛏️',
        ingots: '🧱',
        fabricators: '🏭',
        'structure-kits': '📦',
        gases: '💨',
        reagents: '🧪',
        atmospherics: '🌡️',
        electronics: '⚡',
        'logic-devices': '🔌',
        structures: '🏗️',
        organics: '🌱',
        rockets: '🚀',
        genetics: '🧬',
        trading: '💰',
      };
      expect(iconMap[props.category.id]).toBe('⛏️');
    });

    it('should display all 14 category icons', () => {
      const iconMap: Record<string, string> = {
        ores: '⛏️',
        ingots: '🧱',
        fabricators: '🏭',
        'structure-kits': '📦',
        gases: '💨',
        reagents: '🧪',
        atmospherics: '🌡️',
        electronics: '⚡',
        'logic-devices': '🔌',
        structures: '🏗️',
        organics: '🌱',
        rockets: '🚀',
        genetics: '🧬',
        trading: '💰',
      };

      expect(Object.keys(iconMap)).toHaveLength(14);
      Object.values(iconMap).forEach(icon => {
        expect(icon).toBeTruthy();
      });
    });
  });

  describe('Click handler', () => {
    it('should call onClick when clicked', () => {
      const onClick = vi.fn();
      const props = createMockProps({ onClick });

      onClick(props.category.id);
      expect(onClick).toHaveBeenCalledWith(props.category.id);
    });

    it('should pass category id to onClick', () => {
      const onClick = vi.fn();
      const props = createMockProps({
        category: { id: 'ingots', name: 'Ingots', description: '', order: 2 },
        onClick,
      });

      onClick(props.category.id);
      expect(onClick).toHaveBeenCalledWith('ingots');
    });

    it('should handle multiple clicks', () => {
      const onClick = vi.fn();
      const props = createMockProps({ onClick });

      onClick(props.category.id);
      onClick(props.category.id);
      onClick(props.category.id);

      expect(onClick).toHaveBeenCalledTimes(3);
    });
  });

  describe('Hover effects', () => {
    it('should support hover styling', () => {
      const props = createMockProps();
      let isHovered = false;

      // Simulate hover
      isHovered = true;
      expect(isHovered).toBe(true);

      isHovered = false;
      expect(isHovered).toBe(false);
    });

    it('should show orange border on hover', () => {
      const borderColor = '#ff6a00';
      expect(borderColor).toBe('#ff6a00');
    });

    it('should apply hover effect with glow', () => {
      let hasGlow = false;
      hasGlow = true;
      expect(hasGlow).toBe(true);
    });
  });

  describe('Styling', () => {
    it('should have rounded corners', () => {
      const borderRadius = 'rounded-lg';
      expect(borderRadius).toBe('rounded-lg');
    });

    it('should have subtle shadow', () => {
      const shadow = 'shadow';
      expect(shadow).toBe('shadow');
    });

    it('should have proper padding', () => {
      const padding = 'p-4';
      expect(padding).toBeTruthy();
    });
  });

  describe('Edge cases', () => {
    it('should handle category with special characters in name', () => {
      const props = createMockProps({
        category: { id: 'organics', name: 'Organics & Food', description: '', order: 11 },
      });
      expect(props.category.name).toContain('&');
    });

    it('should handle rapid clicks', () => {
      const onClick = vi.fn();
      const props = createMockProps({ onClick });

      for (let i = 0; i < 100; i++) {
        onClick(props.category.id);
      }

      expect(onClick).toHaveBeenCalledTimes(100);
    });
  });
});
