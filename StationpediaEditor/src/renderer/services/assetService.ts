/**
 * Asset management service for managing workspace images and icons
 */

import type { AssetRegistryEntry } from '@models/validationModel';

export interface AssetStatistics {
  totalAssets: number;
  totalSize: number;
  usedAssets: number;
  unusedAssets: number;
}

/**
 * Service for managing asset registry and tracking usage
 */
export class AssetService {
  private registry: Map<string, AssetRegistryEntry> = new Map();

  /**
   * Register a new asset
   */
  register(asset: AssetRegistryEntry): void {
    // Prevent duplicate entries
    if (!this.registry.has(asset.filename)) {
      this.registry.set(asset.filename, { ...asset });
    }
  }

  /**
   * Get the entire registry
   */
  getRegistry(): Map<string, AssetRegistryEntry> {
    return new Map(this.registry);
  }

  /**
   * Get a specific asset by filename
   */
  getAsset(filename: string): AssetRegistryEntry | undefined {
    return this.registry.get(filename);
  }

  /**
   * Record usage of an asset
   */
  recordUsage(filename: string): void {
    const asset = this.registry.get(filename);
    if (asset) {
      asset.usageCount++;
    }
  }

  /**
   * Search assets by wildcard pattern
   */
  searchByPattern(pattern: string): AssetRegistryEntry[] {
    // Convert wildcard pattern to regex
    const regex = new RegExp(`^${pattern.replace(/\*/g, '.*')}$`);
    return Array.from(this.registry.values()).filter(asset => regex.test(asset.filename));
  }

  /**
   * Search assets by file extension
   */
  searchByExtension(ext: string): AssetRegistryEntry[] {
    const extension = ext.startsWith('.') ? ext : `.${ext}`;
    return Array.from(this.registry.values()).filter(asset =>
      asset.filename.toLowerCase().endsWith(extension.toLowerCase())
    );
  }

  /**
   * Get all unused assets
   */
  getUnusedAssets(): AssetRegistryEntry[] {
    return Array.from(this.registry.values()).filter(asset => asset.usageCount === 0);
  }

  /**
   * Get asset statistics
   */
  getStatistics(): AssetStatistics {
    const assets = Array.from(this.registry.values());
    const totalSize = assets.reduce((sum, a) => sum + (a.size || 0), 0);
    const usedAssets = assets.filter(a => a.usageCount > 0).length;
    const unusedAssets = assets.filter(a => a.usageCount === 0).length;

    return {
      totalAssets: assets.length,
      totalSize,
      usedAssets,
      unusedAssets,
    };
  }

  /**
   * Clear all assets from registry
   */
  clear(): void {
    this.registry.clear();
  }

  /**
   * Reset all usage counts to zero
   */
  resetUsageCounts(): void {
    for (const asset of this.registry.values()) {
      asset.usageCount = 0;
    }
  }
}

export const assetService = new AssetService();
