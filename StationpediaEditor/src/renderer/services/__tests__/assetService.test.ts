/**
 * Tests for asset service
 */
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { AssetService } from '../assetService';
import type { AssetRegistryEntry } from '@models/validationModel';

describe('AssetService', () => {
  let assetService: AssetService;

  beforeEach(() => {
    assetService = new AssetService();
  });

  describe('Asset Registration', () => {
    it('should register an asset', () => {
      const asset: AssetRegistryEntry = {
        path: '/workspace/assets/test.png',
        filename: 'test.png',
        relativePath: 'assets/test.png',
        size: 1024,
        usageCount: 0,
      };

      assetService.register(asset);
      const registry = assetService.getRegistry();

      expect(registry.has('test.png')).toBe(true);
      expect(registry.get('test.png')).toEqual(asset);
    });

    it('should prevent duplicate asset registrations', () => {
      const asset: AssetRegistryEntry = {
        path: '/workspace/assets/test.png',
        filename: 'test.png',
        relativePath: 'assets/test.png',
        usageCount: 0,
      };

      assetService.register(asset);
      assetService.register(asset); // Should not error

      const registry = assetService.getRegistry();
      expect(registry.size).toBe(1);
    });

    it('should track asset usage', () => {
      const asset: AssetRegistryEntry = {
        path: '/workspace/assets/test.png',
        filename: 'test.png',
        relativePath: 'assets/test.png',
        usageCount: 0,
      };

      assetService.register(asset);
      assetService.recordUsage('test.png');

      const registry = assetService.getRegistry();
      expect(registry.get('test.png')?.usageCount).toBe(1);
    });

    it('should increment usage multiple times', () => {
      const asset: AssetRegistryEntry = {
        path: '/workspace/assets/test.png',
        filename: 'test.png',
        relativePath: 'assets/test.png',
        usageCount: 0,
      };

      assetService.register(asset);
      assetService.recordUsage('test.png');
      assetService.recordUsage('test.png');
      assetService.recordUsage('test.png');

      const registry = assetService.getRegistry();
      expect(registry.get('test.png')?.usageCount).toBe(3);
    });
  });

  describe('Asset Lookup', () => {
    it('should find a registered asset', () => {
      const asset: AssetRegistryEntry = {
        path: '/workspace/assets/test.png',
        filename: 'test.png',
        relativePath: 'assets/test.png',
        usageCount: 0,
      };

      assetService.register(asset);
      const found = assetService.getAsset('test.png');

      expect(found).toEqual(asset);
    });

    it('should return undefined for unregistered asset', () => {
      const found = assetService.getAsset('nonexistent.png');
      expect(found).toBeUndefined();
    });

    it('should find assets by pattern', () => {
      const assets: AssetRegistryEntry[] = [
        {
          path: '/workspace/assets/icon.png',
          filename: 'icon.png',
          relativePath: 'assets/icon.png',
          usageCount: 0,
        },
        {
          path: '/workspace/assets/icon_alt.png',
          filename: 'icon_alt.png',
          relativePath: 'assets/icon_alt.png',
          usageCount: 0,
        },
        {
          path: '/workspace/assets/banner.jpg',
          filename: 'banner.jpg',
          relativePath: 'assets/banner.jpg',
          usageCount: 0,
        },
      ];

      assets.forEach(a => assetService.register(a));

      const icons = assetService.searchByPattern('icon*');
      expect(icons).toHaveLength(2);
      expect(icons.some(a => a.filename === 'icon.png')).toBe(true);
      expect(icons.some(a => a.filename === 'icon_alt.png')).toBe(true);
    });

    it('should search by file type', () => {
      const assets: AssetRegistryEntry[] = [
        {
          path: '/workspace/assets/image1.png',
          filename: 'image1.png',
          relativePath: 'assets/image1.png',
          usageCount: 0,
        },
        {
          path: '/workspace/assets/image2.jpg',
          filename: 'image2.jpg',
          relativePath: 'assets/image2.jpg',
          usageCount: 0,
        },
        {
          path: '/workspace/assets/image3.png',
          filename: 'image3.png',
          relativePath: 'assets/image3.png',
          usageCount: 0,
        },
      ];

      assets.forEach(a => assetService.register(a));

      const pngAssets = assetService.searchByExtension('png');
      expect(pngAssets).toHaveLength(2);
      expect(pngAssets.every(a => a.filename.endsWith('.png'))).toBe(true);
    });
  });

  describe('Unused Assets', () => {
    it('should identify unused assets', () => {
      const assets: AssetRegistryEntry[] = [
        {
          path: '/workspace/assets/used.png',
          filename: 'used.png',
          relativePath: 'assets/used.png',
          usageCount: 1,
        },
        {
          path: '/workspace/assets/unused.png',
          filename: 'unused.png',
          relativePath: 'assets/unused.png',
          usageCount: 0,
        },
      ];

      assets.forEach(a => assetService.register(a));

      const unused = assetService.getUnusedAssets();
      expect(unused).toHaveLength(1);
      expect(unused[0].filename).toBe('unused.png');
    });

    it('should return empty array when all assets are used', () => {
      const assets: AssetRegistryEntry[] = [
        {
          path: '/workspace/assets/asset1.png',
          filename: 'asset1.png',
          relativePath: 'assets/asset1.png',
          usageCount: 2,
        },
        {
          path: '/workspace/assets/asset2.png',
          filename: 'asset2.png',
          relativePath: 'assets/asset2.png',
          usageCount: 1,
        },
      ];

      assets.forEach(a => assetService.register(a));

      const unused = assetService.getUnusedAssets();
      expect(unused).toHaveLength(0);
    });
  });

  describe('Asset Statistics', () => {
    it('should calculate asset statistics', () => {
      const assets: AssetRegistryEntry[] = [
        {
          path: '/workspace/assets/asset1.png',
          filename: 'asset1.png',
          relativePath: 'assets/asset1.png',
          size: 1024,
          usageCount: 2,
        },
        {
          path: '/workspace/assets/asset2.jpg',
          filename: 'asset2.jpg',
          relativePath: 'assets/asset2.jpg',
          size: 2048,
          usageCount: 1,
        },
      ];

      assets.forEach(a => assetService.register(a));

      const stats = assetService.getStatistics();

      expect(stats.totalAssets).toBe(2);
      expect(stats.totalSize).toBe(3072);
      expect(stats.usedAssets).toBe(2);
      expect(stats.unusedAssets).toBe(0);
    });
  });

  describe('Clear Registry', () => {
    it('should clear all assets', () => {
      const asset: AssetRegistryEntry = {
        path: '/workspace/assets/test.png',
        filename: 'test.png',
        relativePath: 'assets/test.png',
        usageCount: 0,
      };

      assetService.register(asset);
      expect(assetService.getRegistry().size).toBe(1);

      assetService.clear();
      expect(assetService.getRegistry().size).toBe(0);
    });

    it('should reset all usage counts', () => {
      const assets: AssetRegistryEntry[] = [
        {
          path: '/workspace/assets/asset1.png',
          filename: 'asset1.png',
          relativePath: 'assets/asset1.png',
          usageCount: 0,
        },
        {
          path: '/workspace/assets/asset2.png',
          filename: 'asset2.png',
          relativePath: 'assets/asset2.png',
          usageCount: 0,
        },
      ];

      assets.forEach(a => assetService.register(a));
      assetService.recordUsage('asset1.png');
      assetService.recordUsage('asset2.png');
      assetService.recordUsage('asset2.png');

      assetService.resetUsageCounts();

      const registry = assetService.getRegistry();
      expect(registry.get('asset1.png')?.usageCount).toBe(0);
      expect(registry.get('asset2.png')?.usageCount).toBe(0);
    });
  });
});
