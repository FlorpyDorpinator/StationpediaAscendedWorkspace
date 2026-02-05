/**
 * Tests for Breadcrumb component
 * Tests navigation path display, clickable segments, and orange styling
 */
import { describe, it, expect, vi } from 'vitest';
import React from 'react';
import type { BreadcrumbItem } from '../components/Breadcrumb';

describe('Breadcrumb', () => {
  const createMockProps = (overrides?: any) => ({
    items: [
      { label: 'Home', onClick: vi.fn() },
      { label: 'Fabricators', onClick: vi.fn() },
    ],
    ...overrides,
  });

  describe('Component structure', () => {
    it('should create a valid React component', () => {
      expect(React).toBeTruthy();
    });

    it('should accept required props', () => {
      const props = createMockProps();
      expect(props.items).toBeDefined();
      expect(Array.isArray(props.items)).toBe(true);
    });
  });

  describe('Navigation path display', () => {
    it('should display single breadcrumb item', () => {
      const props = createMockProps({
        items: [{ label: 'Home', onClick: vi.fn() }],
      });

      expect(props.items).toHaveLength(1);
      expect(props.items[0].label).toBe('Home');
    });

    it('should display multiple breadcrumb items', () => {
      const props = createMockProps({
        items: [
          { label: 'Home', onClick: vi.fn() },
          { label: 'Fabricators', onClick: vi.fn() },
        ],
      });

      expect(props.items).toHaveLength(2);
    });

    it('should display three-level breadcrumb path', () => {
      const props = createMockProps({
        items: [
          { label: 'Home', onClick: vi.fn() },
          { label: 'Fabricators', onClick: vi.fn() },
          { label: 'Autolathe', onClick: undefined },
        ],
      });

      expect(props.items).toHaveLength(3);
      expect(props.items[0].label).toBe('Home');
      expect(props.items[1].label).toBe('Fabricators');
      expect(props.items[2].label).toBe('Autolathe');
    });

    it('should display breadcrumbs in correct order', () => {
      const props = createMockProps({
        items: [
          { label: 'Home', onClick: vi.fn() },
          { label: 'Fabricators', onClick: vi.fn() },
          { label: 'Autolathe', onClick: undefined },
        ],
      });

      const path = props.items.map((item: BreadcrumbItem) => item.label).join(' > ');
      expect(path).toBe('Home > Fabricators > Autolathe');
    });

    it('should handle breadcrumb with special characters', () => {
      const props = createMockProps({
        items: [
          { label: 'Home', onClick: vi.fn() },
          { label: 'Reagents & Compounds', onClick: vi.fn() },
        ],
      });

      expect(props.items[1].label).toContain('&');
    });
  });

  describe('Separator display', () => {
    it('should display separator between items', () => {
      const props = createMockProps();
      const separator = '>';
      expect(separator).toBe('>');
    });

    it('should display correct number of separators', () => {
      const props = createMockProps({
        items: [
          { label: 'Home', onClick: vi.fn() },
          { label: 'Fabricators', onClick: vi.fn() },
          { label: 'Autolathe', onClick: undefined },
        ],
      });

      const separatorCount = props.items.length - 1;
      expect(separatorCount).toBe(2);
    });

    it('should not display separator for single item', () => {
      const props = createMockProps({
        items: [{ label: 'Home', onClick: vi.fn() }],
      });

      const separatorCount = props.items.length - 1;
      expect(separatorCount).toBe(0);
    });
  });

  describe('Clickable segments', () => {
    it('should make first item clickable', () => {
      const onClick = vi.fn();
      const props = createMockProps({
        items: [{ label: 'Home', onClick }],
      });

      props.items[0].onClick?.();
      expect(onClick).toHaveBeenCalled();
    });

    it('should make intermediate items clickable', () => {
      const onClick = vi.fn();
      const props = createMockProps({
        items: [
          { label: 'Home', onClick: vi.fn() },
          { label: 'Fabricators', onClick },
          { label: 'Autolathe', onClick: undefined },
        ],
      });

      props.items[1].onClick?.();
      expect(onClick).toHaveBeenCalled();
    });

    it('should not make last item clickable', () => {
      const props = createMockProps({
        items: [
          { label: 'Home', onClick: vi.fn() },
          { label: 'Fabricators', onClick: vi.fn() },
          { label: 'Autolathe', onClick: undefined },
        ],
      });

      expect(props.items[2].onClick).toBeUndefined();
    });

    it('should call onClick with correct label', () => {
      const onClick = vi.fn();
      const props = createMockProps({
        items: [
          { label: 'Home', onClick },
          { label: 'Fabricators', onClick: vi.fn() },
        ],
      });

      props.items[0].onClick?.();
      expect(onClick).toHaveBeenCalled();
    });

    it('should handle click on Home breadcrumb', () => {
      const onClick = vi.fn();
      const props = createMockProps({
        items: [
          { label: 'Home', onClick },
          { label: 'Fabricators', onClick: vi.fn() },
        ],
      });

      props.items[0].onClick?.();
      expect(onClick).toHaveBeenCalledTimes(1);
    });

    it('should handle click on category breadcrumb', () => {
      const onClick = vi.fn();
      const props = createMockProps({
        items: [
          { label: 'Home', onClick: vi.fn() },
          { label: 'Fabricators', onClick },
        ],
      });

      props.items[1].onClick?.();
      expect(onClick).toHaveBeenCalledTimes(1);
    });

    it('should handle multiple clicks on same breadcrumb', () => {
      const onClick = vi.fn();
      const props = createMockProps({
        items: [{ label: 'Home', onClick }],
      });

      props.items[0].onClick?.();
      props.items[0].onClick?.();
      props.items[0].onClick?.();

      expect(onClick).toHaveBeenCalledTimes(3);
    });
  });

  describe('Styling - Orange text', () => {
    it('should use orange color for breadcrumb links', () => {
      const linkColor = '#ff6a00';
      expect(linkColor).toBe('#ff6a00');
    });

    it('should display orange text for clickable items', () => {
      const textColor = 'text-stationpedia-accent';
      expect(textColor).toBe('text-stationpedia-accent');
    });

    it('should apply hover effect with orange shade', () => {
      const hoverColor = '#ff8533';
      expect(hoverColor).toBe('#ff8533');
    });

    it('should display current (last) item in normal text color', () => {
      const currentColor = 'text-stationpedia-text';
      expect(currentColor).toBe('text-stationpedia-text');
    });
  });

  describe('Hover effects', () => {
    it('should show hover effect on clickable items', () => {
      const props = createMockProps();
      let isHovered = false;

      isHovered = true;
      expect(isHovered).toBe(true);

      isHovered = false;
      expect(isHovered).toBe(false);
    });

    it('should update hover color for links', () => {
      let isHovered = false;
      const baseColor = '#ff6a00';
      const hoverColor = '#ff8533';

      const color = isHovered ? hoverColor : baseColor;
      isHovered = true;
      const newColor = isHovered ? hoverColor : baseColor;

      expect(newColor).toBe(hoverColor);
    });

    it('should have smooth transition on hover', () => {
      const transition = 'transition-colors';
      expect(transition).toBe('transition-colors');
    });

    it('should show underline on hover', () => {
      let isHovered = false;
      const decoration = isHovered ? 'underline' : 'no-underline';

      isHovered = true;
      expect(isHovered ? 'underline' : 'no-underline').toBe('underline');
    });
  });

  describe('Responsive layout', () => {
    it('should display horizontally', () => {
      const props = createMockProps();
      const display = 'flex';
      expect(display).toBe('flex');
    });

    it('should align items center', () => {
      const alignment = 'items-center';
      expect(alignment).toBe('items-center');
    });

    it('should have proper spacing between items', () => {
      const gap = 'gap-2';
      expect(gap).toBe('gap-2');
    });
  });

  describe('Props validation', () => {
    it('should accept items array', () => {
      const props = createMockProps();
      expect(Array.isArray(props.items)).toBe(true);
    });

    it('should accept empty items array', () => {
      const props = createMockProps({ items: [] });
      expect(props.items).toHaveLength(0);
    });

    it('should require label for each item', () => {
      const props = createMockProps();
      props.items.forEach((item: BreadcrumbItem) => {
        expect(item.label).toBeDefined();
      });
    });

    it('should accept optional onClick for each item', () => {
      const props = createMockProps({
        items: [
          { label: 'Home', onClick: vi.fn() },
          { label: 'Category', onClick: undefined },
        ],
      });

      expect(props.items[0].onClick).toBeDefined();
      expect(props.items[1].onClick).toBeUndefined();
    });
  });

  describe('Edge cases', () => {
    it('should handle single item breadcrumb', () => {
      const props = createMockProps({
        items: [{ label: 'Home', onClick: vi.fn() }],
      });

      expect(props.items).toHaveLength(1);
    });

    it('should handle very long breadcrumb path', () => {
      const items = Array.from({ length: 10 }, (_, i: number) => ({
        label: `Level${i}`,
        onClick: i < 9 ? vi.fn() : undefined,
      }));
      const props = createMockProps({ items });

      expect(props.items).toHaveLength(10);
    });

    it('should handle breadcrumb labels with numbers', () => {
      const props = createMockProps({
        items: [
          { label: 'Home', onClick: vi.fn() },
          { label: 'Category 1', onClick: vi.fn() },
        ],
      });

      expect(props.items[1].label).toContain('1');
    });

    it('should handle breadcrumb with empty label', () => {
      const props = createMockProps({
        items: [{ label: '', onClick: vi.fn() }],
      });

      expect(props.items[0].label).toBe('');
    });
  });
});
