/**
 * CategoryListView component
 * Displays a list of devices within a selected category with search filtering
 * Uses actual Stationpedia.json data with thumbnails from AssetRipper
 */
import React, { useMemo, useState, useEffect } from 'react';
import { getPagesByCategory, searchPages } from '../services/stationpediaService';
import { STATIONPEDIA_CATEGORIES, getThumbnailUrl, StationpediaPage } from '../types/stationpediaTypes';

export interface CategoryListViewProps {
  categoryId: string;
  onSelectDevice: (deviceKey: string) => void;
  searchQuery: string;
  onSearchChange: (query: string) => void;
  onBack?: () => void;
}

// Strip rich text tags for display
function stripRichText(text: string): string {
  if (!text) return '';
  return text
    .replace(/<link=[^>]*>/g, '')
    .replace(/<\/link>/g, '')
    .replace(/<color=[^>]*>/g, '')
    .replace(/<\/color>/g, '')
    .replace(/<N:EN:([^>]*)>/g, '$1')
    .replace(/<b>/g, '')
    .replace(/<\/b>/g, '');
}

// Device list item with actual thumbnail
const DeviceListItemWithThumbnail: React.FC<{
  page: StationpediaPage;
  onClick: (key: string) => void;
}> = ({ page, onClick }) => {
  const [isHovered, setIsHovered] = useState(false);
  const [imageError, setImageError] = useState(false);
  
  const thumbnailSrc = getThumbnailUrl(page.PrefabName);
  const displayName = stripRichText(page.Title);
  
  return (
    <button
      onClick={() => onClick(page.Key)}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
      className="flex items-center gap-3 w-full text-left transition-all duration-100 rounded"
      style={{
        backgroundColor: isHovered ? '#4A4F54' : '#3A3F44',
        border: '1px solid #5A5F64',
        padding: '8px 12px',
        minHeight: '48px',
      }}
    >
      {/* Thumbnail */}
      <div className="w-10 h-10 flex items-center justify-center flex-shrink-0 bg-[#2A2F34] rounded">
        {!imageError ? (
          <img 
            src={thumbnailSrc}
            alt={displayName} 
            className="max-w-full max-h-full object-contain"
            onError={() => setImageError(true)}
          />
        ) : (
          <span className="text-lg text-[#6E7681]">📦</span>
        )}
      </div>

      {/* Device Name */}
      <div className="flex-1 min-w-0">
        <span 
          className="text-sm font-medium block truncate"
          style={{ color: '#E6EDF3' }}
        >
          {displayName}
        </span>
        <span 
          className="text-xs block truncate"
          style={{ color: '#8B949E' }}
        >
          {page.PrefabName}
        </span>
      </div>
    </button>
  );
};

export const CategoryListView: React.FC<CategoryListViewProps> = ({
  categoryId,
  onSelectDevice,
  searchQuery,
  onSearchChange,
  onBack,
}) => {
  const [pages, setPages] = useState<StationpediaPage[]>([]);
  
  // Get the category info
  const category = useMemo(
    () => STATIONPEDIA_CATEGORIES.find(c => c.id === categoryId),
    [categoryId]
  );

  // Load pages for this category
  useEffect(() => {
    const categoryPages = getPagesByCategory(categoryId);
    setPages(categoryPages);
  }, [categoryId]);

  // Filter pages based on search query
  const filteredPages = useMemo(() => {
    if (!searchQuery.trim()) return pages;
    const lowerQuery = searchQuery.toLowerCase();
    return pages.filter(page => {
      const title = stripRichText(page.Title).toLowerCase();
      const prefab = page.PrefabName.toLowerCase();
      return title.includes(lowerQuery) || prefab.includes(lowerQuery);
    });
  }, [pages, searchQuery]);

  if (!category) {
    return (
      <div className="flex items-center justify-center h-full bg-[#1A1F24]">
        <p className="text-[#8B949E]">Category not found</p>
      </div>
    );
  }

  return (
    <div className="flex flex-col h-full bg-[#1A1F24]">
      {/* Category Header */}
      <div className="px-4 py-3 bg-[#2A2F34] border-b border-[#3A3F44]">
        <div className="flex items-center gap-3">
          {onBack && (
            <button
              onClick={onBack}
              className="text-[#8B949E] hover:text-[#E6EDF3] transition-colors"
            >
              ← Back
            </button>
          )}
          <div>
            <h2 className="text-lg font-bold text-[#E6EDF3]">
              {category.name}
            </h2>
            <p className="text-xs text-[#8B949E]">
              {filteredPages.length === pages.length
                ? `${pages.length} items`
                : `${filteredPages.length} of ${pages.length} items`}
            </p>
          </div>
        </div>
      </div>

      {/* Search Input */}
      <div className="px-4 py-2 border-b border-[#3A3F44] bg-[#2A2F34]">
        <input
          type="text"
          placeholder={`Search in ${category.name}...`}
          value={searchQuery}
          onChange={(e) => onSearchChange(e.target.value)}
          className="w-full px-3 py-2 rounded bg-[#1A1F24] border border-[#3A3F44] text-[#E6EDF3] text-sm placeholder-[#6E7681] outline-none focus:border-[#FF7A18] transition-colors"
        />
      </div>

      {/* Device List - 2 column grid like the game */}
      <div className="flex-1 overflow-y-auto p-3">
        {filteredPages.length === 0 ? (
          <div className="flex items-center justify-center h-full">
            <div className="text-center">
              <p className="text-[#8B949E] mb-2">No items found</p>
              {searchQuery && (
                <p className="text-xs text-[#6E7681]">
                  Try adjusting your search query
                </p>
              )}
            </div>
          </div>
        ) : (
          <div className="grid grid-cols-2 gap-2">
            {filteredPages.map((page) => (
              <DeviceListItemWithThumbnail
                key={page.Key}
                page={page}
                onClick={onSelectDevice}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default CategoryListView;
