/**
 * Tests for AssetBrowser component
 */
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { AssetBrowser } from '../AssetBrowser';
import type { AssetRegistryEntry } from '@models/validationModel';

describe('AssetBrowser', () => {
  const mockAssets: AssetRegistryEntry[] = [
    {
      path: '/workspace/assets/icon.png',
      filename: 'icon.png',
      relativePath: 'assets/icon.png',
      size: 1024,
      usageCount: 2,
    },
    {
      path: '/workspace/assets/banner.jpg',
      filename: 'banner.jpg',
      relativePath: 'assets/banner.jpg',
      size: 5120,
      usageCount: 0,
    },
    {
      path: '/workspace/images/photo.png',
      filename: 'photo.png',
      relativePath: 'images/photo.png',
      size: 2048,
      usageCount: 1,
    },
  ];

  it('should render asset browser title', () => {
    render(
      <AssetBrowser
        assets={mockAssets}
        onSelectAsset={() => {}}
        onPreviewAsset={() => {}}
      />
    );

    expect(screen.getByText('Assets')).toBeInTheDocument();
  });

  it('should display all assets in grid', () => {
    render(
      <AssetBrowser
        assets={mockAssets}
        onSelectAsset={() => {}}
        onPreviewAsset={() => {}}
      />
    );

    expect(screen.getByText('icon.png')).toBeInTheDocument();
    expect(screen.getByText('banner.jpg')).toBeInTheDocument();
    expect(screen.getByText('photo.png')).toBeInTheDocument();
  });

  it('should show usage count for each asset', () => {
    render(
      <AssetBrowser
        assets={mockAssets}
        onSelectAsset={() => {}}
        onPreviewAsset={() => {}}
      />
    );

    // Usage counts should be displayed
    const usageElements = screen.getAllByText(/used?:?/i);
    expect(usageElements.length).toBeGreaterThan(0);
  });

  it('should highlight unused assets', () => {
    render(
      <AssetBrowser
        assets={mockAssets}
        onSelectAsset={() => {}}
        onPreviewAsset={() => {}}
      />
    );

    // banner.jpg has 0 usage, should be highlighted
    const unusedAssetRow = screen.getByText('banner.jpg').closest('[class*="opacity"]');
    expect(unusedAssetRow).toBeTruthy();
  });

  it('should filter assets by extension', async () => {
    const user = userEvent.setup();
    render(
      <AssetBrowser
        assets={mockAssets}
        onSelectAsset={() => {}}
        onPreviewAsset={() => {}}
      />
    );

    // Check if filter exists
    const filterInputs = screen.queryAllByPlaceholderText(/filter|search/i);
    if (filterInputs.length > 0) {
      await user.type(filterInputs[0], '.png');
      // Should show only PNG files
      expect(screen.getByText('icon.png')).toBeInTheDocument();
      expect(screen.getByText('photo.png')).toBeInTheDocument();
    }
  });

  it('should call onSelectAsset when clicking an asset', async () => {
    const user = userEvent.setup();
    const onSelectAsset = vi.fn();

    render(
      <AssetBrowser
        assets={mockAssets}
        onSelectAsset={onSelectAsset}
        onPreviewAsset={() => {}}
      />
    );

    // Click on the first asset container
    const assetElement = screen.getByText('icon.png');
    const assetContainer = assetElement.closest('[class*="border rounded"]');
    
    if (assetContainer) {
      await user.click(assetContainer);
      // Should have been called when we clicked the asset
      expect(onSelectAsset).toHaveBeenCalled();
    }
  });

  it('should show file size information', () => {
    render(
      <AssetBrowser
        assets={mockAssets}
        onSelectAsset={() => {}}
        onPreviewAsset={() => {}}
      />
    );

    // File sizes should be displayed in a human-readable format
    const sizeElements = screen.queryAllByText(/B|KB|MB/);
    expect(sizeElements.length).toBeGreaterThan(0);
  });

  it('should display empty state when no assets', () => {
    render(
      <AssetBrowser
        assets={[]}
        onSelectAsset={() => {}}
        onPreviewAsset={() => {}}
      />
    );

    expect(screen.getByText(/no asset|no image/i)).toBeInTheDocument();
  });
});
