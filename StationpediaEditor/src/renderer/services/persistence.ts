/**
 * Persistence Service
 * Handles file I/O, auto-save, backups, and recent files tracking
 */
import { fileService } from './fileService';
import type { WorkspaceModel } from '@models/contentModel';

export interface PersistenceService {
  openWorkspace(): Promise<WorkspaceModel | null>;
  saveWorkspace(workspace: WorkspaceModel): Promise<boolean>;
  saveWorkspaceAs(workspace: WorkspaceModel): Promise<string | null>;
  hasUnsavedChanges(): boolean;
  getRecentFiles(): string[];
  addRecentFile(path: string): void;
  clearRecentFiles(): void;
  setDirtyState(dirty: boolean): void;
  startAutoSave(interval?: number): () => void; // Returns cleanup function
}

class PersistenceServiceImpl implements PersistenceService {
  private isDirty = false;
  private currentWorkspacePath: string | null = null;
  private recentFilesKey = 'stationpedia-recent-files';
  private maxRecentFiles = 10;
  private autoSaveTimer: NodeJS.Timeout | null = null;
  private currentWorkspace: WorkspaceModel | null = null;
  private saveCallbacks: Array<(workspace: WorkspaceModel) => Promise<void>> = [];

  /**
   * Open a workspace from the file system
   */
  async openWorkspace(): Promise<WorkspaceModel | null> {
    try {
      const folderPath = await fileService.openFolder();
      if (!folderPath) return null;

      // Load workspace
      const result = await fileService.loadWorkspace(folderPath);
      if (!result.success || !result.data) {
        console.error('Failed to load workspace:', result.error);
        return null;
      }

      // Convert to workspace model
      const workspace: WorkspaceModel = {
        devices: (result.data.descriptions as any) || [],
        guides: (result.data.guides as any) || [],
        mechanics: (result.data.mechanics as any) || [],
        genericDescriptions: result.data.genericDescriptions,
      };

      this.currentWorkspacePath = folderPath;
      this.currentWorkspace = workspace;
      this.isDirty = false;
      this.addRecentFile(folderPath);

      return workspace;
    } catch (error) {
      console.error('Error opening workspace:', error);
      return null;
    }
  }

  /**
   * Save current workspace to disk
   */
  async saveWorkspace(workspace: WorkspaceModel): Promise<boolean> {
    if (!this.currentWorkspacePath) {
      console.warn('No workspace path set, cannot save');
      return false;
    }

    try {
      this.currentWorkspace = workspace;

      // Call save callbacks (e.g., for tooltips)
      for (const callback of this.saveCallbacks) {
        await callback(workspace);
      }

      // Save descriptions (pass full workspace so guides/mechanics/genericDescriptions are preserved)
      const result = await fileService.saveDescriptions(
        this.currentWorkspacePath,
        workspace
      );

      if (result.success) {
        this.isDirty = false;
        return true;
      } else {
        console.error('Save failed:', result.error);
        return false;
      }
    } catch (error) {
      console.error('Error saving workspace:', error);
      return false;
    }
  }

  /**
   * Save workspace to a new location
   */
  async saveWorkspaceAs(workspace: WorkspaceModel): Promise<string | null> {
    try {
      // In a real implementation, this would open a "Save As" dialog
      // For now, we'll just save to the current path and let the dialog be handled in the UI
      const folderPath = await fileService.openFolder();
      if (!folderPath) return null;

      this.currentWorkspacePath = folderPath;
      const success = await this.saveWorkspace(workspace);
      return success ? folderPath : null;
    } catch (error) {
      console.error('Error in saveWorkspaceAs:', error);
      return null;
    }
  }

  /**
   * Check if there are unsaved changes
   */
  hasUnsavedChanges(): boolean {
    return this.isDirty;
  }

  /**
   * Mark workspace as dirty/modified
   */
  setDirtyState(dirty: boolean): void {
    this.isDirty = dirty;
  }

  /**
   * Get list of recent workspace paths
   */
  getRecentFiles(): string[] {
    try {
      const stored = localStorage.getItem(this.recentFilesKey);
      return stored ? JSON.parse(stored) : [];
    } catch {
      return [];
    }
  }

  /**
   * Add a workspace path to recent files
   */
  addRecentFile(path: string): void {
    try {
      const recent = this.getRecentFiles();
      // Remove if already exists (to move to top)
      const filtered = recent.filter(p => p !== path);
      // Add to top
      const updated = [path, ...filtered].slice(0, this.maxRecentFiles);
      localStorage.setItem(this.recentFilesKey, JSON.stringify(updated));
    } catch (error) {
      console.warn('Failed to update recent files:', error);
    }
  }

  /**
   * Clear recent files list
   */
  clearRecentFiles(): void {
    try {
      localStorage.removeItem(this.recentFilesKey);
    } catch {
      // Ignore errors
    }
  }

  /**
   * Start auto-save with configurable interval
   * Returns a cleanup function to stop auto-save
   */
  startAutoSave(interval: number = 30000): () => void {
    // Stop any existing auto-save
    if (this.autoSaveTimer) {
      clearInterval(this.autoSaveTimer);
    }

    this.autoSaveTimer = setInterval(async () => {
      if (this.isDirty && this.currentWorkspace) {
        try {
          await this.saveWorkspace(this.currentWorkspace);
          console.log('Auto-saved workspace');
        } catch (error) {
          console.error('Auto-save failed:', error);
        }
      }
    }, interval);

    // Return cleanup function
    return () => {
      if (this.autoSaveTimer) {
        clearInterval(this.autoSaveTimer);
        this.autoSaveTimer = null;
      }
    };
  }

  /**
   * Register a callback to be called before saving
   */
  onBeforeSave(callback: (workspace: WorkspaceModel) => Promise<void>): void {
    this.saveCallbacks.push(callback);
  }

  /**
   * Get current workspace path
   */
  getCurrentWorkspacePath(): string | null {
    return this.currentWorkspacePath;
  }

  /**
   * Set workspace path (for loading existing workspace)
   */
  setCurrentWorkspacePath(path: string): void {
    this.currentWorkspacePath = path;
  }
}

export const persistenceService = new PersistenceServiceImpl();
