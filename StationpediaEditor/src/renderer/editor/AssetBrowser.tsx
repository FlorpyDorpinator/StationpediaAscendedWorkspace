/**
 * AssetBrowser - Browse and manage workspace assets
 */

import React, { useMemo, useState } from 'react';
import type { AssetRegistryEntry } from '@models/validationModel';

interface AssetBrowserProps {
  assets: AssetRegistryEntry[];
  onSelectAsset: (asset: AssetRegistryEntry) => void;
  onPreviewAsset: (asset: AssetRegistryEntry) => void;
}

const formatFileSize = (bytes?: number): string => {
  if (!bytes) return '-';
  if (bytes < 1024) return `${bytes}B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)}KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)}MB`;
};

export const AssetBrowser: React.FC<AssetBrowserProps> = ({
  assets,
  onSelectAsset,
  onPreviewAsset,
}) => {
  const [searchFilter, setSearchFilter] = useState('');

  const filteredAssets = useMemo(() => {
    if (!searchFilter) return assets;

    const filter = searchFilter.toLowerCase();
    return assets.filter(
      asset =>
        asset.filename.toLowerCase().includes(filter) ||
        asset.relativePath.toLowerCase().includes(filter)
    );
  }, [assets, searchFilter]);

  const stats = useMemo(() => {
    const used = assets.filter(a => a.usageCount > 0).length;
    const unused = assets.filter(a => a.usageCount === 0).length;
    const totalSize = assets.reduce((sum, a) => sum + (a.size || 0), 0);

    return { used, unused, totalSize };
  }, [assets]);

  return (
    <div className="flex flex-col h-full bg-gray-900">
      {/* Header */}
      <div className="p-3 border-b border-gray-700 bg-gray-800">
        <h3 className="font-semibold text-gray-100 mb-2">Assets</h3>

        {/* Stats */}
        <div className="text-xs text-gray-400 mb-3 space-y-1">
          <div>Total: {assets.length} assets</div>
          <div>Used: {stats.used} | Unused: {stats.unused}</div>
          <div>Size: {formatFileSize(stats.totalSize)}</div>
        </div>

        {/* Search Filter */}
        <input
          type="text"
          placeholder="Filter by filename..."
          value={searchFilter}
          onChange={e => setSearchFilter(e.target.value)}
          className="w-full px-2 py-1 bg-gray-700 text-gray-100 text-sm rounded border border-gray-600 focus:outline-none focus:border-cyan-500"
        />
      </div>

      {/* Asset Grid */}
      <div className="flex-1 overflow-auto p-2">
        {filteredAssets.length === 0 ? (
          <div className="flex items-center justify-center h-full text-gray-500">
            <div className="text-center">
              <div className="text-sm">No assets found</div>
              {searchFilter && (
                <div className="text-xs mt-2">
                  Try adjusting your filter
                </div>
              )}
            </div>
          </div>
        ) : (
          <div className="grid grid-cols-1 gap-2">
            {filteredAssets.map(asset => (
              <div
                key={asset.filename}
                className={`border rounded p-2 transition-colors cursor-pointer hover:bg-gray-800 ${
                  asset.usageCount === 0
                    ? 'bg-gray-800 border-gray-700 opacity-60'
                    : 'bg-gray-800 border-gray-700'
                }`}
                onClick={() => onSelectAsset(asset)}
              >
                <div className="flex items-start justify-between gap-2">
                  <div className="flex-1 min-w-0">
                    {/* Filename */}
                    <div className="font-semibold text-sm text-gray-200 truncate">
                      {asset.filename}
                    </div>

                    {/* Path */}
                    <div className="text-xs text-gray-400 truncate">
                      {asset.relativePath}
                    </div>

                    {/* Info Row */}
                    <div className="text-xs text-gray-500 mt-1 space-y-0.5">
                      <div>
                        Size: <span className="text-gray-400">{formatFileSize(asset.size)}</span>
                      </div>
                      <div>
                        Used:{' '}
                        <span className={asset.usageCount === 0 ? 'text-red-400' : 'text-green-400'}>
                          {asset.usageCount}
                        </span>
                      </div>
                    </div>
                  </div>

                  {/* Action Button */}
                  <button
                    onClick={e => {
                      e.stopPropagation();
                      onPreviewAsset(asset);
                    }}
                    className="flex-shrink-0 px-2 py-1 bg-cyan-900 hover:bg-cyan-800 text-cyan-200 text-xs rounded border border-cyan-700 transition-colors"
                  >
                    Preview
                  </button>
                </div>

                {/* Unused Badge */}
                {asset.usageCount === 0 && (
                  <div className="mt-1 text-xs bg-red-900 text-red-200 px-2 py-0.5 rounded inline-block">
                    Unused
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};
