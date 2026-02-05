/**
 * useKeyboardShortcuts Hook Tests
 */
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { getKeyboardShortcutsHelp } from '../useKeyboardShortcuts';

describe('getKeyboardShortcutsHelp', () => {
  it('should return an array of keyboard shortcuts', () => {
    const shortcuts = getKeyboardShortcutsHelp();
    expect(Array.isArray(shortcuts)).toBe(true);
    expect(shortcuts.length).toBeGreaterThan(0);
  });

  it('should include save shortcut', () => {
    const shortcuts = getKeyboardShortcutsHelp();
    const saveShortcut = shortcuts.find(s => s.keys === 'Ctrl+S');
    expect(saveShortcut).toBeDefined();
    expect(saveShortcut?.action).toBe('Save changes');
  });

  it('should include open shortcut', () => {
    const shortcuts = getKeyboardShortcutsHelp();
    const openShortcut = shortcuts.find(s => s.keys === 'Ctrl+O');
    expect(openShortcut).toBeDefined();
    expect(openShortcut?.action).toBe('Open workspace');
  });

  it('should include save as shortcut', () => {
    const shortcuts = getKeyboardShortcutsHelp();
    const saveAsShortcut = shortcuts.find(s => s.keys === 'Ctrl+Shift+S');
    expect(saveAsShortcut).toBeDefined();
    expect(saveAsShortcut?.action).toBe('Save As');
  });

  it('should include refresh preview shortcut', () => {
    const shortcuts = getKeyboardShortcutsHelp();
    const refreshShortcut = shortcuts.find(s => s.keys === 'F5');
    expect(refreshShortcut).toBeDefined();
    expect(refreshShortcut?.action).toBe('Refresh preview');
  });

  it('should include formatting shortcuts', () => {
    const shortcuts = getKeyboardShortcutsHelp();
    const boldShortcut = shortcuts.find(s => s.keys === 'Ctrl+B');
    const italicShortcut = shortcuts.find(s => s.keys === 'Ctrl+I');
    const underlineShortcut = shortcuts.find(s => s.keys === 'Ctrl+U');

    expect(boldShortcut).toBeDefined();
    expect(italicShortcut).toBeDefined();
    expect(underlineShortcut).toBeDefined();
  });

  it('should have unique keys', () => {
    const shortcuts = getKeyboardShortcutsHelp();
    const keys = shortcuts.map(s => s.keys);
    const uniqueKeys = new Set(keys);
    expect(keys.length).toBe(uniqueKeys.size);
  });
});
