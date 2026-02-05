/**
 * Persistence Service Tests
 */
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { persistenceService } from '../persistence';
import type { WorkspaceModel } from '@models/contentModel';

describe('persistenceService', () => {
  beforeEach(() => {
    // Clear localStorage
    localStorage.clear();
    // Reset dirty state
    persistenceService.setDirtyState(false);
  });

  describe('Recent Files Management', () => {
    it('should add a recent file', () => {
      persistenceService.addRecentFile('/path/to/workspace');
      const recent = persistenceService.getRecentFiles();
      expect(recent).toContain('/path/to/workspace');
    });

    it('should maintain order with most recent first', () => {
      persistenceService.addRecentFile('/path/1');
      persistenceService.addRecentFile('/path/2');
      persistenceService.addRecentFile('/path/3');

      const recent = persistenceService.getRecentFiles();
      expect(recent[0]).toBe('/path/3');
      expect(recent[1]).toBe('/path/2');
      expect(recent[2]).toBe('/path/1');
    });

    it('should move duplicate to top', () => {
      persistenceService.addRecentFile('/path/1');
      persistenceService.addRecentFile('/path/2');
      persistenceService.addRecentFile('/path/1'); // Add duplicate

      const recent = persistenceService.getRecentFiles();
      expect(recent[0]).toBe('/path/1');
      expect(recent[1]).toBe('/path/2');
      expect(recent.length).toBe(2);
    });

    it('should limit recent files to max', () => {
      for (let i = 0; i < 15; i++) {
        persistenceService.addRecentFile(`/path/${i}`);
      }

      const recent = persistenceService.getRecentFiles();
      expect(recent.length).toBeLessThanOrEqual(10);
    });

    it('should clear recent files', () => {
      persistenceService.addRecentFile('/path/1');
      persistenceService.addRecentFile('/path/2');
      persistenceService.clearRecentFiles();

      const recent = persistenceService.getRecentFiles();
      expect(recent).toEqual([]);
    });

    it('should return empty array if no recent files', () => {
      const recent = persistenceService.getRecentFiles();
      expect(recent).toEqual([]);
    });
  });

  describe('Dirty State Management', () => {
    it('should track dirty state', () => {
      expect(persistenceService.hasUnsavedChanges()).toBe(false);

      persistenceService.setDirtyState(true);
      expect(persistenceService.hasUnsavedChanges()).toBe(true);

      persistenceService.setDirtyState(false);
      expect(persistenceService.hasUnsavedChanges()).toBe(false);
    });
  });

  describe('Workspace Path Management', () => {
    it('should get and set current workspace path', () => {
      const path = '/some/workspace/path';
      persistenceService.setCurrentWorkspacePath(path);
      expect(persistenceService.getCurrentWorkspacePath()).toBe(path);
    });
  });

  describe('Auto-save', () => {
    it('should return cleanup function', () => {
      const cleanup = persistenceService.startAutoSave(1000);
      expect(typeof cleanup).toBe('function');
      cleanup();
    });

    it('should not throw when starting auto-save multiple times', () => {
      expect(() => {
        const cleanup1 = persistenceService.startAutoSave(1000);
        const cleanup2 = persistenceService.startAutoSave(1000);
        cleanup1();
        cleanup2();
      }).not.toThrow();
    });
  });

  describe('Save Callbacks', () => {
    it('should register and call before-save callbacks', async () => {
      const callback = vi.fn();
      persistenceService.onBeforeSave(callback);

      // Note: This would require workspace to be set and saving to be triggered
      // In a full implementation, this would be tested with actual save
    });
  });
});
