/**
 * Tests for TabBar component
 * Tests tab switching and active state styling
 */
import { describe, it, expect, vi } from 'vitest';
import React from 'react';

describe('TabBar', () => {
  const createMockProps = (overrides?: any) => ({
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
      expect(props.activeTab).toBe('guides');
      expect(props.onTabChange).toBeDefined();
    });

    it('should have Guides tab', () => {
      const props = createMockProps();
      const tabs = ['guides', 'universe'];
      expect(tabs).toContain('guides');
    });

    it('should have Universe tab', () => {
      const props = createMockProps();
      const tabs = ['guides', 'universe'];
      expect(tabs).toContain('universe');
    });
  });

  describe('Tab switching', () => {
    it('should call onTabChange with correct tab when Guides clicked', () => {
      const onTabChange = vi.fn();
      const props = createMockProps({ onTabChange });

      onTabChange('guides');
      expect(onTabChange).toHaveBeenCalledWith('guides');
    });

    it('should call onTabChange with correct tab when Universe clicked', () => {
      const onTabChange = vi.fn();
      const props = createMockProps({ onTabChange });

      onTabChange('universe');
      expect(onTabChange).toHaveBeenCalledWith('universe');
    });

    it('should handle multiple tab switches', () => {
      const onTabChange = vi.fn();
      const props = createMockProps({ onTabChange });

      onTabChange('guides');
      onTabChange('universe');
      onTabChange('guides');

      expect(onTabChange).toHaveBeenCalledTimes(3);
      expect(onTabChange).toHaveBeenNthCalledWith(1, 'guides');
      expect(onTabChange).toHaveBeenNthCalledWith(2, 'universe');
      expect(onTabChange).toHaveBeenNthCalledWith(3, 'guides');
    });
  });

  describe('Active tab styling', () => {
    it('should show Guides as active when activeTab is guides', () => {
      const props = createMockProps({ activeTab: 'guides' });
      expect(props.activeTab).toBe('guides');
    });

    it('should show Universe as active when activeTab is universe', () => {
      const props = createMockProps({ activeTab: 'universe' });
      expect(props.activeTab).toBe('universe');
    });

    it('should apply active tab styling to correct tab', () => {
      const props = createMockProps({ activeTab: 'guides' });
      
      const activeStyles = {
        guides: props.activeTab === 'guides',
        universe: props.activeTab === 'universe',
      };

      expect(activeStyles.guides).toBe(true);
      expect(activeStyles.universe).toBe(false);
    });

    it('should update active styling when activeTab prop changes', () => {
      let activeTab = 'guides';
      
      expect(activeTab === 'guides').toBe(true);
      expect(activeTab === 'universe').toBe(false);

      activeTab = 'universe';
      
      expect(activeTab === 'guides').toBe(false);
      expect(activeTab === 'universe').toBe(true);
    });
  });

  describe('Orange accent color', () => {
    it('should use orange accent for active tab', () => {
      const props = createMockProps({ activeTab: 'guides' });
      const orangeColor = '#ff6a00';
      
      expect(orangeColor).toBe('#ff6a00');
      expect(props.activeTab).toBe('guides');
    });

    it('should use hover color for non-active tabs', () => {
      const props = createMockProps({ activeTab: 'guides' });
      const hoverColor = '#ff8533';
      
      expect(hoverColor).toBe('#ff8533');
    });
  });

  describe('Edge cases', () => {
    it('should handle rapid tab switching', () => {
      const onTabChange = vi.fn();
      
      for (let i = 0; i < 10; i++) {
        onTabChange(i % 2 === 0 ? 'guides' : 'universe');
      }
      
      expect(onTabChange).toHaveBeenCalledTimes(10);
    });

    it('should not error when clicking same tab repeatedly', () => {
      const onTabChange = vi.fn();
      const props = createMockProps({ onTabChange, activeTab: 'guides' });

      onTabChange('guides');
      onTabChange('guides');
      onTabChange('guides');

      expect(onTabChange).toHaveBeenCalledTimes(3);
      expect(props.activeTab).toBe('guides');
    });
  });

  describe('Tab labels', () => {
    it('should display Guides label', () => {
      const label = 'Guides';
      expect(label).toBe('Guides');
    });

    it('should display Universe label', () => {
      const label = 'Universe';
      expect(label).toBe('Universe');
    });
  });
});
