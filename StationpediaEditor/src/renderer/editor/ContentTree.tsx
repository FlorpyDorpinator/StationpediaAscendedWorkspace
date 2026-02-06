/**
 * ContentTree - Tree view of all devices/guides in workspace
 * Shows hierarchical content with search/filter, selection, drag-and-drop reordering,
 * and inline section headers
 */
import React, { useState, useMemo, useCallback, useEffect, useRef } from 'react';
import type { WorkspaceModel, DeviceDocument } from '@models/contentModel';
import Fuse from 'fuse.js';
import { useEditorStore, CategoryType, HeaderItem } from './editorStore';

interface ContentTreeProps {
  workspace: WorkspaceModel | null;
  selectedDeviceKey: string | null;
  onSelectDevice: (deviceKey: string) => void;
}

interface ExpandedState {
  [key: string]: boolean;
}

type ContentCategory = 'manuals' | 'devices' | 'guides' | 'mechanics';

// Map ContentCategory to CategoryType for store actions
const categoryToType = (category: ContentCategory): CategoryType | null => {
  switch (category) {
    case 'devices': return 'device';
    case 'guides': return 'guide';
    case 'mechanics': return 'mechanic';
    default: return null; // manuals not supported for drag-drop
  }
};

// Drag data interface
interface DragData {
  itemKey: string;
  fromCategory: CategoryType;
  fromIndex: number;
  isHeader?: boolean;
}

// Storage key for persisting content tree state
const CONTENT_TREE_STORAGE_KEY = 'stationpedia-content-tree-state';

export const ContentTree: React.FC<ContentTreeProps> = ({
  workspace,
  selectedDeviceKey,
  onSelectDevice,
}) => {
  const requestScrollToSection = useEditorStore((s) => s.requestScrollToSection);
  // Load saved state from localStorage
  const [searchTerm, setSearchTerm] = useState('');
  const [expanded, setExpanded] = useState<ExpandedState>(() => {
    try {
      const saved = localStorage.getItem(CONTENT_TREE_STORAGE_KEY);
      if (saved) {
        const parsed = JSON.parse(saved);
        return parsed.expanded || {};
      }
    } catch (e) {}
    return {};
  });
  const [activeCategory, setActiveCategory] = useState<ContentCategory>(() => {
    try {
      const saved = localStorage.getItem(CONTENT_TREE_STORAGE_KEY);
      if (saved) {
        const parsed = JSON.parse(saved);
        return parsed.activeCategory || 'devices';
      }
    } catch (e) {}
    return 'devices';
  });

  // Drag and drop state
  const [draggedItem, setDraggedItem] = useState<DragData | null>(null);
  const [dropTarget, setDropTarget] = useState<{ index: number; position: 'before' | 'after' } | null>(null);
  const [dragOverCategory, setDragOverCategory] = useState<ContentCategory | null>(null);
  
  // Header editing state
  const [editingHeader, setEditingHeader] = useState<string | null>(null);
  const [headerEditValue, setHeaderEditValue] = useState('');
  const headerInputRef = useRef<HTMLInputElement>(null);

  // Focus header input when editing starts
  useEffect(() => {
    if (editingHeader && headerInputRef.current) {
      headerInputRef.current.focus();
      headerInputRef.current.select();
    }
  }, [editingHeader]);

  // Save state to localStorage when it changes
  useEffect(() => {
    localStorage.setItem(CONTENT_TREE_STORAGE_KEY, JSON.stringify({
      expanded,
      activeCategory,
    }));
  }, [expanded, activeCategory]);

  // Get items based on active category
  const categoryItems = useMemo(() => {
    if (!workspace) return [];
    
    // Helper to get the key for guide/mechanic (they may use guideKey or deviceKey)
    const getKey = (item: any): string => item.guideKey || item.deviceKey || '';
    
    switch (activeCategory) {
      case 'manuals':
        // Filter guides where key is 'SurvivalManual'
        return ((workspace.guides || []) as any[]).filter((g: any) => getKey(g) === 'SurvivalManual');
      case 'devices':
        return workspace.devices || [];
      case 'guides':
        // All guides except Survival Manual
        return ((workspace.guides || []) as any[]).filter((g: any) => getKey(g) !== 'SurvivalManual');
      case 'mechanics':
        return (workspace.mechanics || []) as any[];
      default:
        return [];
    }
  }, [workspace, activeCategory]);

  // Fuse search setup
  const fuse = useMemo(() => {
    if (!categoryItems.length) return null;

    return new Fuse(categoryItems, {
      keys: ['displayName', 'deviceKey', 'guideKey', 'title', 'operationalDetails.title'],
      threshold: 0.4,
    });
  }, [categoryItems]);

  // Helper to get the key for an item (deviceKey or guideKey)
  const getItemKey = useCallback((item: any): string => {
    return item.deviceKey || item.guideKey || 'unknown';
  }, []);

  // Filtered items
  const filteredItems = useMemo(() => {
    if (!categoryItems.length) return [];

    if (!searchTerm.trim()) {
      return categoryItems;
    }

    const results = fuse?.search(searchTerm) || [];
    return results.map((r) => r.item);
  }, [categoryItems, searchTerm, fuse]);

  const toggleExpanded = useCallback((deviceKey: string) => {
    setExpanded((prev) => ({
      ...prev,
      [deviceKey]: !prev[deviceKey],
    }));
  }, []);

  const handleSelectDevice = useCallback(
    (deviceKey: string) => {
      onSelectDevice(deviceKey);
    },
    [onSelectDevice]
  );

  // Drag and drop handlers
  const handleDragStart = useCallback((e: React.DragEvent, item: any, index: number) => {
    e.stopPropagation();
    
    const itemKey = item.deviceKey || item.guideKey || item.headerKey || 'unknown';
    const categoryType = categoryToType(activeCategory);
    
    if (!categoryType) {
      e.preventDefault();
      return;
    }

    const dragData: DragData = {
      itemKey,
      fromCategory: categoryType,
      fromIndex: index,
      isHeader: item.isHeader || false,
    };

    setDraggedItem(dragData);
    
    // Set data and effect BEFORE anything else
    e.dataTransfer.effectAllowed = 'move';
    e.dataTransfer.setData('text/plain', JSON.stringify(dragData));
    
    // Add dragging class for visual feedback
    if (e.currentTarget instanceof HTMLElement) {
      setTimeout(() => {
        (e.target as HTMLElement).classList.add('opacity-50');
      }, 0);
    }
  }, [activeCategory]);

  const handleDragEnd = useCallback((e: React.DragEvent) => {
    setDraggedItem(null);
    setDropTarget(null);
    setDragOverCategory(null);
    
    if (e.currentTarget instanceof HTMLElement) {
      e.currentTarget.classList.remove('opacity-50');
    }
  }, []);

  const handleDragOverItem = useCallback((e: React.DragEvent, index: number) => {
    e.preventDefault();
    e.stopPropagation();
    
    // Determine if we're in the top or bottom half of the item
    const rect = (e.currentTarget as HTMLElement).getBoundingClientRect();
    const midY = rect.top + rect.height / 2;
    const position: 'before' | 'after' = e.clientY < midY ? 'before' : 'after';
    
    setDropTarget({ index, position });
    e.dataTransfer.dropEffect = 'move';
  }, []);

  const handleDragLeaveItem = useCallback((e: React.DragEvent) => {
    // Only clear if we're leaving the item entirely
    const relatedTarget = e.relatedTarget as HTMLElement;
    if (!relatedTarget || !e.currentTarget.contains(relatedTarget)) {
      setDropTarget(null);
    }
  }, []);

  const handleDropOnItem = useCallback((e: React.DragEvent, targetIndex: number) => {
    e.preventDefault();
    e.stopPropagation();
    
    try {
      const data = JSON.parse(e.dataTransfer.getData('text/plain')) as DragData;
      const targetCategory = categoryToType(activeCategory);
      
      if (!targetCategory) return;

      // Calculate actual insert index based on position
      let insertIndex = dropTarget?.position === 'after' ? targetIndex + 1 : targetIndex;
      
      // Same category = reorder
      if (data.fromCategory === targetCategory) {
        // Adjust index if moving down in the same array
        if (data.fromIndex < insertIndex) {
          insertIndex--;
        }
        if (data.fromIndex !== insertIndex) {
          useEditorStore.getState().reorderItem(data.fromCategory, data.fromIndex, insertIndex);
        }
      } else {
        // Different category = move
        useEditorStore.getState().moveToCategory(data.itemKey, data.fromCategory, targetCategory, insertIndex);
        // Switch to the target category to show the moved item
        setActiveCategory(activeCategory);
      }
    } catch (err) {
      console.error('Drop failed:', err);
    }
    
    setDraggedItem(null);
    setDropTarget(null);
    setDragOverCategory(null);
  }, [activeCategory, dropTarget]);

  // Category tab drop handlers
  const handleDragOverCategory = useCallback((e: React.DragEvent, category: ContentCategory) => {
    e.preventDefault();
    const categoryType = categoryToType(category);
    if (!categoryType || category === 'manuals') return;
    
    setDragOverCategory(category);
    e.dataTransfer.dropEffect = 'move';
  }, []);

  const handleDragLeaveCategory = useCallback((_e: React.DragEvent) => {
    setDragOverCategory(null);
  }, []);

  const handleDropOnCategory = useCallback((e: React.DragEvent, category: ContentCategory) => {
    e.preventDefault();
    
    try {
      const data = JSON.parse(e.dataTransfer.getData('text/plain')) as DragData;
      const targetCategory = categoryToType(category);
      
      if (!targetCategory || category === 'manuals') return;
      
      // If dropping on a different category, move to end of that category
      if (data.fromCategory !== targetCategory) {
        useEditorStore.getState().moveToCategory(data.itemKey, data.fromCategory, targetCategory);
        // Switch to show the dropped item
        setActiveCategory(category);
      }
    } catch (err) {
      console.error('Category drop failed:', err);
    }
    
    setDraggedItem(null);
    setDropTarget(null);
    setDragOverCategory(null);
  }, []);

  // Header management handlers
  const handleAddHeader = useCallback(() => {
    const categoryType = categoryToType(activeCategory);
    if (!categoryType) return;
    
    const headerKey = useEditorStore.getState().addHeader(categoryType, 'New Section');
    // Start editing the new header
    setEditingHeader(headerKey);
    setHeaderEditValue('New Section');
  }, [activeCategory]);

  const handleStartEditHeader = useCallback((headerKey: string, currentTitle: string) => {
    setEditingHeader(headerKey);
    setHeaderEditValue(currentTitle);
  }, []);

  const handleSaveHeader = useCallback(() => {
    if (!editingHeader) return;
    
    const categoryType = categoryToType(activeCategory);
    if (!categoryType) return;
    
    useEditorStore.getState().updateHeader(categoryType, editingHeader, headerEditValue);
    setEditingHeader(null);
    setHeaderEditValue('');
  }, [activeCategory, editingHeader, headerEditValue]);

  const handleCancelEditHeader = useCallback(() => {
    setEditingHeader(null);
    setHeaderEditValue('');
  }, []);

  const handleDeleteHeader = useCallback((headerKey: string) => {
    const categoryType = categoryToType(activeCategory);
    if (!categoryType) return;
    
    if (confirm('Delete this section header?')) {
      useEditorStore.getState().deleteHeader(categoryType, headerKey);
    }
  }, [activeCategory]);

  if (!workspace) {
    return (
      <div className="flex flex-col h-full bg-stationpedia-bg p-4 text-gray-400">
        <div className="text-center">No workspace loaded</div>
      </div>
    );
  }

  // Get counts for badges
  const getKey = (item: any): string => item.guideKey || item.deviceKey || '';
  const manualsCount = (workspace.guides || []).filter((g: any) => getKey(g) === 'SurvivalManual').length;
  const deviceCount = workspace.devices?.length || 0;
  const guideCount = (workspace.guides || []).filter((g: any) => getKey(g) !== 'SurvivalManual').length;
  const mechanicCount = workspace.mechanics?.length || 0;
  
  // Debug logging
  console.log('[ContentTree] workspace.guides:', workspace.guides);
  console.log('[ContentTree] workspace.mechanics:', workspace.mechanics);
  console.log('[ContentTree] manualsCount:', manualsCount, 'guideCount:', guideCount, 'mechanicCount:', mechanicCount);

  return (
    <div className="flex flex-col h-full bg-stationpedia-bg border-r border-stationpedia-border">
      {/* Category Tabs */}
      <div className="flex border-b border-stationpedia-border">
        <button
          onClick={() => setActiveCategory('manuals')}
          className={`flex-1 px-3 py-2 text-xs font-medium transition-colors ${
            activeCategory === 'manuals'
              ? 'bg-stationpedia-accent/20 text-stationpedia-accent border-b-2 border-stationpedia-accent'
              : 'text-gray-400 hover:text-gray-200 hover:bg-stationpedia-surface'
          }`}
        >
          Manuals {manualsCount > 0 && <span className="ml-1 opacity-60">({manualsCount})</span>}
        </button>
        <button
          onClick={() => setActiveCategory('devices')}
          onDragOver={(e) => handleDragOverCategory(e, 'devices')}
          onDragLeave={handleDragLeaveCategory}
          onDrop={(e) => handleDropOnCategory(e, 'devices')}
          className={`flex-1 px-3 py-2 text-xs font-medium transition-colors ${
            activeCategory === 'devices'
              ? 'bg-stationpedia-accent/20 text-stationpedia-accent border-b-2 border-stationpedia-accent'
              : dragOverCategory === 'devices'
              ? 'bg-blue-500/30 text-blue-300 border-b-2 border-blue-400'
              : 'text-gray-400 hover:text-gray-200 hover:bg-stationpedia-surface'
          }`}
        >
          Devices {deviceCount > 0 && <span className="ml-1 opacity-60">({deviceCount})</span>}
        </button>
        <button
          onClick={() => setActiveCategory('guides')}
          onDragOver={(e) => handleDragOverCategory(e, 'guides')}
          onDragLeave={handleDragLeaveCategory}
          onDrop={(e) => handleDropOnCategory(e, 'guides')}
          className={`flex-1 px-3 py-2 text-xs font-medium transition-colors ${
            activeCategory === 'guides'
              ? 'bg-stationpedia-accent/20 text-stationpedia-accent border-b-2 border-stationpedia-accent'
              : dragOverCategory === 'guides'
              ? 'bg-blue-500/30 text-blue-300 border-b-2 border-blue-400'
              : 'text-gray-400 hover:text-gray-200 hover:bg-stationpedia-surface'
          }`}
        >
          Guides {guideCount > 0 && <span className="ml-1 opacity-60">({guideCount})</span>}
        </button>
        <button
          onClick={() => setActiveCategory('mechanics')}
          onDragOver={(e) => handleDragOverCategory(e, 'mechanics')}
          onDragLeave={handleDragLeaveCategory}
          onDrop={(e) => handleDropOnCategory(e, 'mechanics')}
          className={`flex-1 px-3 py-2 text-xs font-medium transition-colors ${
            activeCategory === 'mechanics'
              ? 'bg-stationpedia-accent/20 text-stationpedia-accent border-b-2 border-stationpedia-accent'
              : dragOverCategory === 'mechanics'
              ? 'bg-blue-500/30 text-blue-300 border-b-2 border-blue-400'
              : 'text-gray-400 hover:text-gray-200 hover:bg-stationpedia-surface'
          }`}
        >
          Mechanics {mechanicCount > 0 && <span className="ml-1 opacity-60">({mechanicCount})</span>}
        </button>
      </div>

      {/* Search Bar */}
      <div className="p-3 border-b border-stationpedia-border">
        <div className="flex gap-2">
          <input
            type="text"
            placeholder={`Search ${activeCategory}...`}
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="flex-1 px-3 py-2 bg-stationpedia-surface border border-stationpedia-border rounded text-sm text-white placeholder-gray-500 focus:outline-none focus:border-stationpedia-accent focus:ring-1 focus:ring-stationpedia-accent"
          />
          {/* Add New Guide Button - only show when in Guides category */}
          {activeCategory === 'guides' && (
            <>
              <button
                onClick={() => {
                  const newGuideKey = useEditorStore.getState().addGuide();
                  // Select the newly created guide
                  handleSelectDevice(newGuideKey);
                }}
                className="px-3 py-2 bg-green-600 hover:bg-green-700 text-white rounded text-sm font-medium transition-colors whitespace-nowrap"
                title="Create a new guide"
              >
                + New
              </button>
              <button
                onClick={handleAddHeader}
                className="px-3 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded text-sm font-medium transition-colors whitespace-nowrap"
                title="Add a section header (appears in-game)"
              >
                + Header
              </button>
              {selectedDeviceKey && (
                <button
                  onClick={() => {
                    if (confirm(`Are you sure you want to delete this guide?`)) {
                      useEditorStore.getState().deleteGuide(selectedDeviceKey);
                      onSelectDevice(''); // Deselect
                    }
                  }}
                  className="px-3 py-2 bg-red-600 hover:bg-red-700 text-white rounded text-sm font-medium transition-colors whitespace-nowrap"
                  title="Delete selected guide"
                >
                  Delete
                </button>
              )}
            </>
          )}
          {/* Add New Mechanic Button - only show when in Mechanics category */}
          {activeCategory === 'mechanics' && (
            <>
              <button
                onClick={() => {
                  const newMechanicKey = useEditorStore.getState().addMechanic();
                  // Select the newly created mechanic
                  handleSelectDevice(newMechanicKey);
                }}
                className="px-3 py-2 bg-green-600 hover:bg-green-700 text-white rounded text-sm font-medium transition-colors whitespace-nowrap"
                title="Create a new game mechanic"
              >
                + New
              </button>
              <button
                onClick={handleAddHeader}
                className="px-3 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded text-sm font-medium transition-colors whitespace-nowrap"
                title="Add a section header (appears in-game)"
              >
                + Header
              </button>
              {selectedDeviceKey && (
                <button
                  onClick={() => {
                    if (confirm(`Are you sure you want to delete this game mechanic?`)) {
                      useEditorStore.getState().deleteMechanic(selectedDeviceKey);
                      onSelectDevice(''); // Deselect
                    }
                  }}
                  className="px-3 py-2 bg-red-600 hover:bg-red-700 text-white rounded text-sm font-medium transition-colors whitespace-nowrap"
                  title="Delete selected mechanic"
                >
                  Delete
                </button>
              )}
            </>
          )}
        </div>
      </div>

      {/* Content List */}
      <div className="flex-1 overflow-auto">
        {filteredItems.length === 0 ? (
          <div className="p-4 text-center text-gray-500 text-sm">
            {searchTerm ? `No ${activeCategory} found` : `No ${activeCategory} in workspace`}
          </div>
        ) : (
          <div className="p-2">
            {filteredItems.map((item, index) => {
              const itemKey = getItemKey(item);
              const isHeader = item.isHeader === true;
              const isSelected = itemKey === selectedDeviceKey;
              const isExpandedLocal = expanded[itemKey] || false;
              const hasChildren = !isHeader && item.operationalDetails && item.operationalDetails.length > 0;
              const isDragging = draggedItem?.itemKey === itemKey;
              const isDropTarget = dropTarget?.index === index;
              const canDrag = activeCategory !== 'manuals';

              // Render header item
              if (isHeader) {
                return (
                  <div 
                    key={itemKey} 
                    className="mb-1"
                    draggable={canDrag}
                    onDragStart={(e) => handleDragStart(e, item, index)}
                    onDragEnd={handleDragEnd}
                    onDragOver={(e) => handleDragOverItem(e, index)}
                    onDragLeave={handleDragLeaveItem}
                    onDrop={(e) => handleDropOnItem(e, index)}
                  >
                    {/* Drop indicator - before */}
                    {isDropTarget && dropTarget?.position === 'before' && (
                      <div className="h-1 bg-blue-500 rounded-full mb-1 mx-2" />
                    )}
                    
                    <div
                      className={`flex items-center gap-2 px-3 py-2 rounded transition-colors ${
                        isDragging ? 'opacity-50' : ''
                      } bg-purple-900/30 border border-purple-500/40 cursor-grab`}
                    >
                      {/* Drag handle */}
                      <span className="text-purple-400 cursor-grab">⋮⋮</span>
                      
                      {/* Header icon */}
                      <span className="text-purple-400">§</span>
                      
                      {/* Editable header title */}
                      {editingHeader === item.headerKey ? (
                        <input
                          ref={headerInputRef}
                          type="text"
                          value={headerEditValue}
                          onChange={(e) => setHeaderEditValue(e.target.value)}
                          onKeyDown={(e) => {
                            if (e.key === 'Enter') handleSaveHeader();
                            if (e.key === 'Escape') handleCancelEditHeader();
                          }}
                          onBlur={handleSaveHeader}
                          className="flex-1 bg-purple-900/50 border border-purple-400 rounded px-2 py-1 text-sm text-purple-200 focus:outline-none"
                        />
                      ) : (
                        <div 
                          className="flex-1 font-bold text-purple-300 cursor-text"
                          onDoubleClick={() => handleStartEditHeader(item.headerKey, item.headerTitle)}
                        >
                          {item.headerTitle}
                        </div>
                      )}
                      
                      {/* Delete button */}
                      <button
                        onClick={() => handleDeleteHeader(item.headerKey)}
                        className="text-red-400 hover:text-red-300 text-sm px-1"
                        title="Delete header"
                      >
                        ✕
                      </button>
                    </div>
                    
                    {/* Drop indicator - after */}
                    {isDropTarget && dropTarget?.position === 'after' && (
                      <div className="h-1 bg-blue-500 rounded-full mt-1 mx-2" />
                    )}
                  </div>
                );
              }

              // Render regular item
              return (
                <div 
                  key={itemKey} 
                  className="mb-1"
                  draggable={canDrag}
                  onDragStart={(e) => handleDragStart(e, item, index)}
                  onDragEnd={handleDragEnd}
                  onDragOver={(e) => handleDragOverItem(e, index)}
                  onDragLeave={handleDragLeaveItem}
                  onDrop={(e) => handleDropOnItem(e, index)}
                >
                  {/* Drop indicator - before */}
                  {isDropTarget && dropTarget?.position === 'before' && (
                    <div className="h-1 bg-blue-500 rounded-full mb-1 mx-2" />
                  )}
                  
                  {/* Item Entry */}
                  <div
                    data-selected={isSelected}
                    className={`flex items-center gap-2 px-3 py-2 rounded cursor-pointer transition-colors ${
                      isDragging ? 'opacity-50' : ''
                    } ${
                      isSelected
                        ? 'bg-stationpedia-accent/30 text-stationpedia-accent border border-stationpedia-accent/50'
                        : 'hover:bg-stationpedia-surface text-gray-300'
                    }`}
                  >
                    {/* Drag handle */}
                    {canDrag && (
                      <span className="text-gray-500 cursor-grab">⋮⋮</span>
                    )}
                    
                    {/* Expand/Collapse Button */}
                    {hasChildren && (
                      <button
                        onClick={() => toggleExpanded(itemKey)}
                        className="flex-shrink-0 p-0 hover:text-stationpedia-accent text-gray-500 transition-colors"
                        aria-label={isExpandedLocal ? 'collapse' : 'expand'}
                      >
                        <svg
                          className={`w-4 h-4 transition-transform ${
                            isExpandedLocal ? 'rotate-90' : ''
                          }`}
                          fill="none"
                          stroke="currentColor"
                          viewBox="0 0 24 24"
                        >
                          <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={2}
                            d="M9 5l7 7-7 7"
                          />
                        </svg>
                      </button>
                    )}

                    {/* Item Name */}
                    <div
                      onClick={() => handleSelectDevice(itemKey)}
                      className="flex-1 min-w-0"
                    >
                      <div className="font-semibold truncate">
                        {item.displayName || item.title || itemKey}
                      </div>
                      <div className="text-xs opacity-60 truncate">{itemKey}</div>
                    </div>
                  </div>
                  
                  {/* Drop indicator - after */}
                  {isDropTarget && dropTarget?.position === 'after' && (
                    <div className="h-1 bg-blue-500 rounded-full mt-1 mx-2" />
                  )}

                  {/* Operational Details (when expanded) */}
                  {hasChildren && isExpandedLocal && (
                    <div className="ml-4 mt-1 space-y-1 border-l border-stationpedia-border pl-3">
                      {item.operationalDetails!.map((detail: any, idx: number) => (
                        <div
                          key={`${itemKey}-detail-${idx}`}
                          onClick={(e) => {
                            e.stopPropagation();
                            // Select the device if not already selected
                            if (!isSelected) {
                              handleSelectDevice(itemKey);
                            }
                            // Request scroll via store (DeviceSectionsEditor listens)
                            // Use setTimeout when switching devices so sections render first
                            if (!isSelected) {
                              setTimeout(() => requestScrollToSection(idx), 150);
                            } else {
                              requestScrollToSection(idx);
                            }
                          }}
                          className="px-3 py-1 text-sm text-gray-400 hover:text-stationpedia-accent rounded hover:bg-stationpedia-surface/50 truncate cursor-pointer transition-colors"
                          title={detail.title}
                        >
                          {detail.title}
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
};
