/**
 * LinkModal - Modal for inserting links to Stationpedia pages
 * Shows searchable list of all available pages including devices, guides, mechanics
 */
import React, { useState, useMemo, useEffect, useRef } from 'react';
import type { WorkspaceModel } from '@models/contentModel';
import Fuse from 'fuse.js';

interface LinkModalProps {
  workspace: WorkspaceModel | null;
  isOpen: boolean;
  onClose: () => void;
  onSelectLink: (link: string, displayText: string) => void;
}

interface SearchableItem {
  key: string;
  displayName: string;
  type: 'device' | 'guide' | 'mechanic';
}

export const LinkModal: React.FC<LinkModalProps> = ({
  workspace,
  isOpen,
  onClose,
  onSelectLink,
}) => {
  const [searchTerm, setSearchTerm] = useState('');
  const searchInputRef = useRef<HTMLInputElement>(null);

  // Build searchable items list
  const allItems = useMemo<SearchableItem[]>(() => {
    if (!workspace) return [];
    
    const items: SearchableItem[] = [];
    
    // Add devices
    workspace.devices.forEach(device => {
      items.push({
        key: device.deviceKey,
        displayName: device.displayName || device.deviceKey,
        type: 'device',
      });
    });
    
    // Add guides
    workspace.guides?.forEach(guide => {
      items.push({
        key: guide.guideKey || guide.deviceKey || '',
        displayName: guide.displayName || guide.guideKey || guide.deviceKey || '',
        type: 'guide',
      });
    });
    
    // Add mechanics
    workspace.mechanics?.forEach(mechanic => {
      const key = (mechanic as any).guideKey || (mechanic as any).deviceKey || '';
      items.push({
        key,
        displayName: (mechanic as any).displayName || key,
        type: 'mechanic',
      });
    });
    
    return items;
  }, [workspace]);

  // Fuse search
  const fuse = useMemo(() => {
    return new Fuse(allItems, {
      keys: ['displayName', 'key'],
      threshold: 0.3,
      includeScore: true,
    });
  }, [allItems]);

  // Filter items
  const filteredItems = useMemo(() => {
    if (!searchTerm) return allItems;
    return fuse.search(searchTerm).map(result => result.item);
  }, [searchTerm, allItems, fuse]);

  // Focus search input when modal opens
  useEffect(() => {
    if (isOpen) {
      setTimeout(() => searchInputRef.current?.focus(), 100);
    }
  }, [isOpen]);

  const handleSelect = (item: SearchableItem) => {
    // Generate link tag using game format: {LINK:PageKey;Link Text}
    // For custom guides, prefix with CustomGuide_
    let pageKey = item.key;
    if (item.type === 'guide') {
      pageKey = `CustomGuide_${item.key}`;
    }
    
    const link = `{LINK:${pageKey};${item.displayName}}`;
    
    onSelectLink(link, item.displayName);
    onClose();
  };

  if (!isOpen) return null;

  return (
    <div
      className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
      onClick={onClose}
    >
      <div
        className="bg-gray-900 rounded-lg shadow-xl w-[600px] max-h-[80vh] flex flex-col"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="p-4 border-b border-gray-700">
          <h2 className="text-xl font-semibold text-white mb-3">Insert Link</h2>
          <input
            ref={searchInputRef}
            type="text"
            placeholder="Search pages..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full px-3 py-2 bg-gray-800 border border-gray-700 rounded text-white placeholder-gray-500 focus:outline-none focus:border-cyan-500 focus:ring-1 focus:ring-cyan-500"
          />
        </div>

        {/* Results List */}
        <div className="flex-1 overflow-auto p-2">
          {filteredItems.length === 0 ? (
            <div className="p-4 text-center text-gray-500">
              {searchTerm ? 'No pages found' : 'No pages available'}
            </div>
          ) : (
            <div className="space-y-1">
              {filteredItems.map((item) => (
                <button
                  key={`${item.type}-${item.key}`}
                  onClick={() => handleSelect(item)}
                  className="w-full text-left px-3 py-2 rounded hover:bg-gray-800 transition-colors group"
                >
                  <div className="flex items-center justify-between">
                    <span className="text-white group-hover:text-cyan-400 transition-colors">
                      {item.displayName}
                    </span>
                    <span className="text-xs text-gray-500 uppercase">
                      {item.type}
                    </span>
                  </div>
                  <div className="text-xs text-gray-600 mt-1">{item.key}</div>
                </button>
              ))}
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="p-4 border-t border-gray-700 flex justify-end gap-2">
          <button
            onClick={onClose}
            className="px-4 py-2 bg-gray-700 hover:bg-gray-600 text-white rounded transition-colors"
          >
            Cancel
          </button>
        </div>
      </div>
    </div>
  );
};
