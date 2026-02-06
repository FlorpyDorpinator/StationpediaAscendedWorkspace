/**
 * File service for handling file operations via Electron IPC
 */
import type { DeviceDescription } from '@models/stationpedia';
import { normalizeDocumentTables } from '@models/contentModel';

export interface FileResult<T = void> {
  success: boolean;
  data?: T;
  error?: string;
}

export interface WorkspaceData {
  descriptions: DeviceDescription[];
  guides: DeviceDescription[];
  mechanics: DeviceDescription[];
  assets: string[];
  genericDescriptions?: Record<string, string>;
}

class FileService {
  private get isElectron(): boolean {
    return typeof window !== 'undefined' && !!window.electronAPI;
  }

  /**
   * Open folder picker dialog
   */
  async openFolder(): Promise<string | null> {
    if (!this.isElectron) {
      console.error('Electron API not available');
      return null;
    }
    try {
      const result = await window.electronAPI.openFolder();
      console.log('openFolder result:', result);
      return result;
    } catch (error) {
      console.error('openFolder error:', error);
      return null;
    }
  }

  /**
   * Read a file and return its content
   */
  async readFile(filePath: string): Promise<FileResult<string>> {
    if (!this.isElectron) {
      return { success: false, error: 'Electron API not available' };
    }
    return window.electronAPI.readFile(filePath);
  }

  /**
   * Write content to a file with optional backup
   */
  async writeFile(filePath: string, content: string, createBackup = true): Promise<FileResult> {
    if (!this.isElectron) {
      return { success: false, error: 'Electron API not available' };
    }
    return window.electronAPI.writeFile(filePath, content, createBackup);
  }

  /**
   * List files in a directory with optional extension filter
   */
  async listFiles(directory: string, extensions: string[] = []): Promise<FileResult<string[]>> {
    if (!this.isElectron) {
      return { success: false, error: 'Electron API not available' };
    }
    return window.electronAPI.listFiles(directory, extensions);
  }

  /**
   * Check if a path exists
   */
  async exists(filePath: string): Promise<boolean> {
    if (!this.isElectron) {
      return false;
    }
    return window.electronAPI.exists(filePath);
  }

  /**
   * Get settings (last workspace, preferences, etc.)
   */
  async getSettings(): Promise<Record<string, any>> {
    if (!this.isElectron) {
      // Try localStorage fallback for web mode
      try {
        const stored = localStorage.getItem('stationpedia-editor-settings');
        return stored ? JSON.parse(stored) : {};
      } catch {
        return {};
      }
    }
    return window.electronAPI.getSettings();
  }

  /**
   * Save settings
   */
  async setSettings(settings: Record<string, any>): Promise<FileResult> {
    if (!this.isElectron) {
      try {
        localStorage.setItem('stationpedia-editor-settings', JSON.stringify(settings));
        return { success: true };
      } catch (error: any) {
        return { success: false, error: error.message };
      }
    }
    return window.electronAPI.setSettings(settings);
  }

  /**
   * Load workspace data from a mod folder
   */
  async loadWorkspace(folderPath: string): Promise<FileResult<WorkspaceData>> {
    const data: WorkspaceData = {
      descriptions: [],
      guides: [],
      mechanics: [],
      assets: [],
    };

    try {
      // Look for descriptions.json
      const descPath = `${folderPath}/descriptions.json`;
      if (await this.exists(descPath)) {
        const result = await this.readFile(descPath);
        if (result.success && result.data) {
          try {
            const parsed = JSON.parse(result.data);
            // Handle the Stationpedia Ascended format
            if (parsed.devices && Array.isArray(parsed.devices)) {
              // New format: { genericDescriptions: {...}, guides: [...], devices: [...] }
              data.descriptions = parsed.devices.map((d: any) => {
                const normalized = {
                  ...d,
                  // Normalize OperationalDetails to operationalDetails
                  operationalDetails: d.OperationalDetails || d.operationalDetails || [],
                };
                // Remove the capital-O variant so only operationalDetails exists.
                // This prevents stale data surviving through edit+save cycles.
                delete normalized.OperationalDetails;
                // Normalize table formats from {headers, rows} to [TableRow]
                return normalizeDocumentTables(normalized);
              });
              // Extract genericDescriptions if present
              if (parsed.genericDescriptions) {
                data.genericDescriptions = parsed.genericDescriptions;
              }
              // Extract guides from descriptions.json (new format)
              if (parsed.guides && Array.isArray(parsed.guides)) {
                data.guides = parsed.guides.map((g: any) => {
                  const normalized = {
                    ...g,
                    // Normalize OperationalDetails to operationalDetails
                    operationalDetails: g.OperationalDetails || g.operationalDetails || [],
                  };
                  // Remove the capital-O variant so only operationalDetails exists.
                  delete normalized.OperationalDetails;
                  // Normalize table formats from {headers, rows} to [TableRow]
                  return normalizeDocumentTables(normalized);
                });
              }
              // Extract mechanics from descriptions.json (new format)
              if (parsed.mechanics && Array.isArray(parsed.mechanics)) {
                data.mechanics = parsed.mechanics.map((m: any) => {
                  const normalized = {
                    ...m,
                    // Normalize OperationalDetails to operationalDetails
                    operationalDetails: m.OperationalDetails || m.operationalDetails || [],
                  };
                  // Remove the capital-O variant so only operationalDetails exists.
                  delete normalized.OperationalDetails;
                  // Normalize table formats from {headers, rows} to [TableRow]
                  return normalizeDocumentTables(normalized);
                });
              }
            } else if (Array.isArray(parsed)) {
              // Old format: direct array
              data.descriptions = parsed;
            } else if (parsed.descriptions) {
              // Alternative format
              data.descriptions = parsed.descriptions;
            }
          } catch (e) {
            console.error('Failed to parse descriptions.json:', e);
          }
        }
      }

      // NOTE: Markdown guides folder is no longer used for editing
      // The guides array in descriptions.json is the source of truth
      // Markdown files are kept for reference only

      // Look for game-mechanics folder
      const mechanicsPath = `${folderPath}/game-mechanics`;
      if (await this.exists(mechanicsPath)) {
        const mechanicsResult = await this.listFiles(mechanicsPath, ['.json', '.md']);
        if (mechanicsResult.success && mechanicsResult.data) {
          for (const file of mechanicsResult.data) {
            const fileResult = await this.readFile(file);
            if (fileResult.success && fileResult.data) {
              const ext = file.toLowerCase().split('.').pop();
              const fileName = file.split(/[/\\]/).pop() || '';
              const baseName = fileName.replace(/\.(json|md)$/i, '');
              
              if (ext === 'json') {
                try {
                  const mechanic = JSON.parse(fileResult.data);
                  if (mechanic.deviceKey) {
                    data.mechanics.push(mechanic);
                  }
                } catch (e) {
                  console.error(`Failed to parse mechanic ${file}:`, e);
                }
              } else if (ext === 'md') {
                data.mechanics.push({
                  deviceKey: `Mechanic_${baseName.replace(/[^a-zA-Z0-9]/g, '_')}`,
                  displayName: baseName.split('-').map(w => 
                    w.charAt(0).toUpperCase() + w.slice(1)
                  ).join(' '),
                  pageDescription: fileResult.data,
                  _sourcePath: file,
                  _isMarkdown: true,
                });
              }
            }
          }
        }
      }

      // Find assets
      const assetsPath = `${folderPath}/assets`;
      const imagesPath = `${folderPath}/images`; // Alt path used by the mod
      const actualAssetsPath = await this.exists(imagesPath) ? imagesPath :
                               await this.exists(assetsPath) ? assetsPath : null;
      
      if (actualAssetsPath) {
        const assetsResult = await this.listFiles(actualAssetsPath, ['.png', '.jpg', '.jpeg', '.gif']);
        if (assetsResult.success && assetsResult.data) {
          data.assets = assetsResult.data;
        }
      }

      return { success: true, data };
    } catch (error: any) {
      return { success: false, error: error.message };
    }
  }

  /**
   * Save descriptions.json (preserves the original format structure)
   */
  async saveDescriptions(folderPath: string, descriptions: any): Promise<FileResult> {
    const filePath = `${folderPath}/descriptions.json`;
    
    // First read the existing file to preserve the structure (genericDescriptions, etc)
    const existingResult = await this.readFile(filePath);
    let fileContent: any;
    
    // Handle both old array format and new workspace format
    // Normalize tables to array format before saving
    const devices = (Array.isArray(descriptions) ? descriptions : descriptions.devices || [])
      .map((d: any) => normalizeDocumentTables(d));
    const guides = (Array.isArray(descriptions) ? [] : descriptions.guides || [])
      .map((g: any) => normalizeDocumentTables(g));
    const mechanics = (Array.isArray(descriptions) ? [] : descriptions.mechanics || [])
      .map((m: any) => normalizeDocumentTables(m));
    const genericDescriptions = Array.isArray(descriptions) ? undefined : descriptions.genericDescriptions;
    
    if (existingResult.success && existingResult.data) {
      try {
        const existing = JSON.parse(existingResult.data);
        if (existing.devices) {
          // Preserve existing structure, update devices, guides, and mechanics
          // Also convert operationalDetails back to OperationalDetails for consistency
          fileContent = {
            ...existing,
            devices: devices.map((d: any) => {
              // Remove undefined/null values to prevent JSON.stringify from turning them into null
              const cleaned = Object.fromEntries(
                Object.entries(d).filter(([_, v]) => v !== undefined && v !== null)
              );
              // Move operationalDetails to OperationalDetails for mod compatibility
              if (cleaned.operationalDetails) {
                cleaned.OperationalDetails = cleaned.operationalDetails;
                delete cleaned.operationalDetails;
              }
              return cleaned;
            }),
            guides: guides.map((g: any) => {
              // Remove undefined/null values to prevent JSON.stringify from turning them into null
              const cleaned = Object.fromEntries(
                Object.entries(g).filter(([_, v]) => v !== undefined && v !== null)
              );
              // Move operationalDetails to OperationalDetails for mod compatibility
              if (cleaned.operationalDetails) {
                cleaned.OperationalDetails = cleaned.operationalDetails;
                delete cleaned.operationalDetails;
              }
              // Guides use guideKey, not deviceKey - remove deviceKey if present
              delete cleaned.deviceKey;
              return cleaned;
            }),
            mechanics: mechanics.map((m: any) => {
              // Remove undefined/null values to prevent JSON.stringify from turning them into null
              const cleaned = Object.fromEntries(
                Object.entries(m).filter(([_, v]) => v !== undefined && v !== null)
              );
              // Move operationalDetails to OperationalDetails for mod compatibility
              if (cleaned.operationalDetails) {
                cleaned.OperationalDetails = cleaned.operationalDetails;
                delete cleaned.operationalDetails;
              }
              // Mechanics use guideKey, not deviceKey - remove deviceKey if present
              delete cleaned.deviceKey;
              return cleaned;
            }),
          };
          // Update genericDescriptions if provided
          if (genericDescriptions !== undefined) {
            fileContent.genericDescriptions = genericDescriptions;
          }
        } else {
          // Direct array format
          fileContent = devices;
        }
      } catch {
        fileContent = devices;
      }
    } else {
      fileContent = devices;
    }
    
    const content = JSON.stringify(fileContent, null, 2);
    return this.writeFile(filePath, content, true);
  }

  /**
   * Save a single guide or mechanic
   */
  async saveItem(folderPath: string, item: DeviceDescription, type: 'guide' | 'mechanic'): Promise<FileResult> {
    const subFolder = type === 'guide' ? 'guides' : 'game-mechanics';
    const filePath = `${folderPath}/${subFolder}/${item.deviceKey}.json`;
    const content = JSON.stringify(item, null, 2);
    return this.writeFile(filePath, content, true);
  }
}

export const fileService = new FileService();

