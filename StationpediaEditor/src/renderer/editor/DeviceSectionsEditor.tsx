/**
 * DeviceSectionsEditor - Unified editor for ALL device sections
 * Shows all editable parts of a device: description, operational details, logic, modes, etc.
 * Top: Rich text editor for currently selected field (resizable)
 * Bottom: Tree view of all sections with click-to-select (resizable)
 */
import React, { useState, useCallback, useEffect, useRef } from 'react';
import {
  DndContext,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
} from '@dnd-kit/core';
import {
  SortableContext,
  sortableKeyboardCoordinates,
  verticalListSortingStrategy,
  useSortable,
} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import type { DeviceDocument, OperationalDetail, WorkspaceModel, TableRow } from '@models/contentModel';
import { LinkModal } from './LinkModal';
import { ImagePickerModal } from './ImagePickerModal';
import { useEditorStore } from './editorStore';

// Resizable Splitter component - tracks cumulative delta from drag start
const ResizableSplitter: React.FC<{
  direction: 'horizontal' | 'vertical';
  onResize: (delta: number) => void;
}> = ({ direction, onResize }) => {
  const handleMouseDown = (e: React.MouseEvent) => {
    e.preventDefault();
    const startPos = direction === 'horizontal' ? e.clientX : e.clientY;
    let lastPos = startPos;
    
    const handleMouseMove = (moveEvent: MouseEvent) => {
      const currentPos = direction === 'horizontal' ? moveEvent.clientX : moveEvent.clientY;
      // Only trigger resize if moved at least 3 pixels (reduces sensitivity)
      const delta = currentPos - lastPos;
      if (Math.abs(delta) >= 3) {
        onResize(delta);
        lastPos = currentPos;
      }
    };
    
    const handleMouseUp = () => {
      document.removeEventListener('mousemove', handleMouseMove);
      document.removeEventListener('mouseup', handleMouseUp);
    };
    
    document.addEventListener('mousemove', handleMouseMove);
    document.addEventListener('mouseup', handleMouseUp);
  };
  
  return (
    <div
      onMouseDown={handleMouseDown}
      className={`
        flex-shrink-0 bg-stationpedia-border hover:bg-stationpedia-accent transition-colors
        ${direction === 'vertical' 
          ? 'h-2 w-full cursor-row-resize' 
          : 'w-2 h-full cursor-col-resize'}
      `}
      title="Drag to resize"
    />
  );
};

interface DeviceSectionsEditorProps {
  device: DeviceDocument | null;
  onUpdateDevice: (updates: Record<string, unknown>) => void;
}

// Selection types for what can be edited
type SelectionType = 
  | { type: 'displayName' }
  | { type: 'pageDescription' }
  | { type: 'pageDescriptionPrepend' }
  | { type: 'pageDescriptionAppend' }
  | { type: 'operationalDetail'; path: number[]; field: 'title' | 'description' | 'items' | 'steps' }
  | { type: 'logicDescription'; key: string }
  | { type: 'modeDescription'; key: string }
  | { type: 'slotDescription'; key: string }
  | { type: 'connectionDescription'; key: string }
  | { type: 'tocTitle' }
  | null;

// 6-dot drag handle icon
const DragHandleIcon: React.FC<{ className?: string }> = ({ className }) => (
  <svg className={className} viewBox="0 0 24 24" fill="currentColor">
    <circle cx="8" cy="6" r="2" />
    <circle cx="16" cy="6" r="2" />
    <circle cx="8" cy="12" r="2" />
    <circle cx="16" cy="12" r="2" />
    <circle cx="8" cy="18" r="2" />
    <circle cx="16" cy="18" r="2" />
  </svg>
);

// Sortable operational detail item
interface SortableDetailItemProps {
  id: string;
  detail: OperationalDetail;
  path: number[];
  depth: number;
  selectedPath: number[] | null;
  selectedField: 'title' | 'description' | 'items' | 'steps' | null;
  onSelect: (path: number[], field: 'title' | 'description' | 'items' | 'steps') => void;
  onAdd: (path: number[]) => void;
  onRemove: (path: number[]) => void;
  onAddTable?: (path: number[]) => void;
  onAddImage?: (path: number[]) => void;
  onAddVideo?: (path: number[]) => void;
  onAddHeader?: (path: number[]) => void;
  hideTitle?: boolean; // Hide title row (used when parent already shows it)
  hideDragHandle?: boolean; // Hide drag handle (used in nested guide sections)
}

// Check if two paths are equal
const pathsEqual = (a: number[] | null, b: number[]): boolean => {
  if (!a) return false;
  if (a.length !== b.length) return false;
  return a.every((v, i) => v === b[i]);
};

const SortableDetailItem: React.FC<SortableDetailItemProps> = ({
  id,
  detail,
  path,
  depth,
  selectedPath,
  selectedField,
  onSelect,
  onAdd,
  onRemove,
  onAddTable,
  onAddImage,
  onAddVideo,
  onAddHeader,
  hideTitle = false,
  hideDragHandle = false,
}) => {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } =
    useSortable({ id });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1,
  };

  // Check if THIS item is selected (exact path match)
  const isThisSelected = pathsEqual(selectedPath, path);
  
  // Add dropdown state for nested items
  const [isAddDropdownOpen, setIsAddDropdownOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  
  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsAddDropdownOpen(false);
      }
    };
    if (isAddDropdownOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [isAddDropdownOpen]);

  return (
    <div
      ref={setNodeRef}
      style={{ ...style, marginLeft: `${depth * 16}px` }}
      className={`mb-2 rounded border ${
        isThisSelected 
          ? 'border-stationpedia-accent bg-stationpedia-accent/10' 
          : 'border-stationpedia-border bg-stationpedia-surface/50'
      } ${isDragging ? 'shadow-lg' : ''}`}
    >
      <div className="flex items-start gap-2 p-2">
        {/* Drag Handle - 6 dots (hidden when hideDragHandle=true) */}
        {!hideDragHandle && (
          <div
            {...attributes}
            {...listeners}
            className="flex-shrink-0 cursor-grab active:cursor-grabbing text-gray-500 hover:text-stationpedia-accent p-1 mt-1"
            title="Drag to reorder"
          >
            <DragHandleIcon className="w-4 h-4" />
          </div>
        )}

        {/* Content */}
        <div className="flex-1 min-w-0">
          {/* Title - clickable to edit (hidden when hideTitle=true) */}
          {!hideTitle && (
            <div
              onClick={(e) => {
                e.stopPropagation();
                onSelect(path, 'title');
              }}
              className={`font-semibold cursor-pointer hover:bg-stationpedia-accent/20 px-2 py-1 rounded ${
                isThisSelected && selectedField === 'title'
                  ? 'bg-stationpedia-accent/30 text-white'
                  : 'text-gray-200'
              }`}
            >
              {detail.title || '(untitled)'}
            </div>
          )}

          {/* Description - clickable to edit, full text shown */}
          <div
            onClick={(e) => {
              e.stopPropagation();
              onSelect(path, 'description');
            }}
            className={`text-sm cursor-pointer hover:bg-stationpedia-accent/20 px-2 py-1 rounded mt-1 whitespace-pre-wrap ${
              isThisSelected && selectedField === 'description'
                ? 'bg-stationpedia-accent/30 text-white'
                : detail.description ? 'text-gray-300' : 'text-gray-500 italic'
            }`}
          >
            {detail.description || '(no description - click to add)'}
          </div>

          {/* Show additional fields if present */}
          {detail.items && detail.items.length > 0 && (
            <div 
              onClick={(e) => {
                e.stopPropagation();
                onSelect(path, 'items');
              }}
              className={`mt-2 px-2 py-1 text-xs cursor-pointer rounded ${
                isThisSelected && selectedField === 'items'
                  ? 'bg-stationpedia-accent/30 text-white'
                  : 'text-gray-400 hover:bg-stationpedia-accent/20'
              }`}
            >
              <span className="font-medium">• Items:</span> {detail.items.length} bullet points (click to edit)
            </div>
          )}
          {detail.steps && detail.steps.length > 0 && (
            <div 
              onClick={(e) => {
                e.stopPropagation();
                onSelect(path, 'steps');
              }}
              className={`mt-1 px-2 py-1 text-xs cursor-pointer rounded ${
                isThisSelected && selectedField === 'steps'
                  ? 'bg-stationpedia-accent/30 text-white'
                  : 'text-gray-400 hover:bg-stationpedia-accent/20'
              }`}
            >
              <span className="font-medium">1. Steps:</span> {detail.steps.length} numbered steps (click to edit)
            </div>
          )}
          {detail.imageFile && (
            <div className="mt-1 px-2 text-xs text-gray-400">
              <span className="font-medium">Image:</span> {detail.imageFile}
            </div>
          )}
          {detail.youtubeUrl && (
            <div className="mt-1 px-2 text-xs text-gray-400">
              <span className="font-medium">Video:</span> {detail.youtubeLabel || detail.youtubeUrl}
            </div>
          )}
          {detail.collapsible && (
            <div className="mt-1 px-2 text-xs text-blue-400">
              📁 Collapsible section
            </div>
          )}
          {detail.tocId && (
            <div className="mt-1 px-2 text-xs text-purple-400">
              🔗 TOC ID: {detail.tocId}
            </div>
          )}
          {detail.table && detail.table.length > 0 && (
            <div 
              onClick={(e) => {
                e.stopPropagation();
                console.log('[Table Click] Path:', path, 'Detail:', detail);
                onSelect(path, 'description'); // Select to show table in properties panel
              }}
              className="mt-1 px-2 text-xs text-green-400 cursor-pointer hover:bg-stationpedia-accent/20 rounded py-1"
            >
              📊 Table: {detail.table.length} rows × {detail.table[0]?.cells?.length || 0} columns (click to edit)
            </div>
          )}
        </div>

        {/* Actions - Add dropdown + Delete button */}
        <div className="flex-shrink-0 flex flex-col gap-1">
          {/* Add dropdown */}
          <div className="relative" ref={dropdownRef}>
            <button
              onClick={(e) => {
                e.stopPropagation();
                setIsAddDropdownOpen(!isAddDropdownOpen);
              }}
              className="px-2 py-1 bg-stationpedia-accent hover:bg-stationpedia-accent-hover text-white rounded text-xs flex items-center gap-1"
              title="Add content"
            >
              + Add
              <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" clipRule="evenodd" />
              </svg>
            </button>
            {isAddDropdownOpen && (
              <div className="absolute right-0 top-full mt-1 bg-stationpedia-surface border border-stationpedia-border rounded shadow-lg z-50 min-w-[140px]">
                {onAddTable && (
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      onAddTable(path);
                      setIsAddDropdownOpen(false);
                    }}
                    className="w-full px-3 py-2 text-left text-xs text-gray-200 hover:bg-stationpedia-accent/30 flex items-center gap-2"
                  >
                    📊 Table
                  </button>
                )}
                {onAddHeader && (
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      onAddHeader(path);
                      setIsAddDropdownOpen(false);
                    }}
                    className="w-full px-3 py-2 text-left text-xs text-gray-200 hover:bg-stationpedia-accent/30 flex items-center gap-2"
                  >
                    📋 Header
                  </button>
                )}
                <button
                  onClick={(e) => {
                    e.stopPropagation();
                    onAdd(path);
                    setIsAddDropdownOpen(false);
                  }}
                  className="w-full px-3 py-2 text-left text-xs text-gray-200 hover:bg-stationpedia-accent/30 flex items-center gap-2"
                >
                  📁 Subsection
                </button>
                {onAddImage && (
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      onAddImage(path);
                      setIsAddDropdownOpen(false);
                    }}
                    className="w-full px-3 py-2 text-left text-xs text-gray-200 hover:bg-stationpedia-accent/30 flex items-center gap-2"
                  >
                    🖼️ Image
                  </button>
                )}
                {onAddVideo && (
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      console.log('[Video Button Click] Path:', path);
                      onAddVideo(path);
                      setIsAddDropdownOpen(false);
                    }}
                    className="w-full px-3 py-2 text-left text-xs text-gray-200 hover:bg-stationpedia-accent/30 flex items-center gap-2"
                  >
                    📺 Video
                  </button>
                )}
              </div>
            )}
          </div>
          <button
            onClick={(e) => {
              e.stopPropagation();
              onRemove(path);
            }}
            className="p-1 text-gray-400 hover:text-red-400 hover:bg-red-400/10 rounded transition-colors"
            title="Remove section"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
            </svg>
          </button>
        </div>
      </div>

      {/* Children */}
      {detail.children && detail.children.length > 0 && (
        <div className="border-t border-stationpedia-border/50 pt-2 pb-1 px-2">
          {detail.children.map((child, i) => (
            <SortableDetailItem
              key={`${id}-${i}`}
              id={`${id}-${i}`}
              detail={child}
              path={[...path, i]}
              depth={depth + 1}
              selectedPath={selectedPath}
              selectedField={selectedField}
              onSelect={onSelect}
              onAdd={onAdd}
              onRemove={onRemove}
              onAddTable={onAddTable}
              onAddImage={onAddImage}
              onAddVideo={onAddVideo}
              onAddHeader={onAddHeader}
            />
          ))}
        </div>
      )}
    </div>
  );
};

// Section header component with Add dropdown
const SectionHeader: React.FC<{
  title: string;
  isExpanded: boolean;
  onToggle: () => void;
  onTitleClick?: () => void; // For editing the title
  onAddTable?: () => void;
  onAddHeader?: () => void;
  onAddSubsection?: () => void;
  onAddImage?: () => void;
  onAddVideo?: () => void;
  showDelete?: boolean;
  onDelete?: () => void;
  isSelected?: boolean;
  showDragHandle?: boolean; // Show drag handle for reordering
  dragHandleProps?: any; // Props from useSortable for drag handle
}> = ({ title, isExpanded, onToggle, onTitleClick, onAddTable, onAddHeader, onAddSubsection, onAddImage, onAddVideo, showDelete, onDelete, isSelected, showDragHandle, dragHandleProps }) => {
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsDropdownOpen(false);
      }
    };
    if (isDropdownOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [isDropdownOpen]);

  const hasAnyAddAction = onAddTable || onAddHeader || onAddSubsection || onAddImage || onAddVideo;

  return (
    <div className={`flex items-center justify-between py-2 px-3 bg-stationpedia-surface/80 rounded-t border ${isSelected ? 'border-stationpedia-accent bg-stationpedia-accent/20' : 'border-stationpedia-border'}`}>
      <div className="flex items-center gap-2">
        {/* Drag Handle - 6 dots (only shown when showDragHandle=true) */}
        {showDragHandle && dragHandleProps && (
          <div
            {...dragHandleProps}
            className="flex-shrink-0 cursor-grab active:cursor-grabbing text-gray-500 hover:text-stationpedia-accent p-1"
            title="Drag to reorder section"
          >
            <DragHandleIcon className="w-4 h-4" />
          </div>
        )}
        <button
          onClick={onToggle}
          className="text-xs text-gray-400 hover:text-white p-1"
          title={isExpanded ? 'Collapse' : 'Expand'}
        >
          {isExpanded ? '▼' : '▶'}
        </button>
        {onTitleClick ? (
          <span
            onClick={onTitleClick}
            className={`font-semibold cursor-pointer hover:bg-stationpedia-accent/30 px-2 py-1 rounded ${isSelected ? 'text-white bg-stationpedia-accent/30' : 'text-gray-200'}`}
            title="Click to edit title"
          >
            {title}
          </span>
        ) : (
          <span className="font-semibold text-gray-200">{title}</span>
        )}
      </div>
      <div className="flex items-center gap-2">
        {showDelete && onDelete && (
          <button
            onClick={(e) => {
              e.stopPropagation();
              if (confirm(`Delete section "${title}"?`)) {
                onDelete();
              }
            }}
            className="px-2 py-1 bg-red-600 hover:bg-red-700 text-white rounded text-xs transition-colors"
            title="Delete this section"
          >
            🗑️ Delete
          </button>
        )}
        {hasAnyAddAction && (
          <div className="relative" ref={dropdownRef}>
            <button
              onClick={(e) => {
                e.stopPropagation();
                setIsDropdownOpen(!isDropdownOpen);
              }}
              className="px-3 py-1 bg-stationpedia-accent hover:bg-stationpedia-accent-hover text-white rounded text-xs transition-colors flex items-center gap-1"
            >
              + Add
              <svg className={`w-3 h-3 transition-transform ${isDropdownOpen ? 'rotate-180' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
              </svg>
            </button>
            {isDropdownOpen && (
              <div className="absolute right-0 top-full mt-1 bg-stationpedia-surface border border-stationpedia-border rounded shadow-lg z-50 min-w-[140px]">
                {onAddTable && (
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      onAddTable();
                      setIsDropdownOpen(false);
                    }}
                    className="w-full px-3 py-2 text-left text-xs text-gray-200 hover:bg-stationpedia-accent/30 flex items-center gap-2"
                  >
                    📊 Table
                  </button>
                )}
                {onAddHeader && (
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      onAddHeader();
                      setIsDropdownOpen(false);
                    }}
                    className="w-full px-3 py-2 text-left text-xs text-gray-200 hover:bg-stationpedia-accent/30 flex items-center gap-2"
                  >
                    📋 Header
                  </button>
                )}
                {onAddSubsection && (
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      onAddSubsection();
                      setIsDropdownOpen(false);
                    }}
                    className="w-full px-3 py-2 text-left text-xs text-gray-200 hover:bg-stationpedia-accent/30 flex items-center gap-2"
                  >
                    📁 Subsection
                  </button>
                )}
                {onAddImage && (
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      onAddImage();
                      setIsDropdownOpen(false);
                    }}
                    className="w-full px-3 py-2 text-left text-xs text-gray-200 hover:bg-stationpedia-accent/30 flex items-center gap-2"
                  >
                    🖼️ Image
                  </button>
                )}
                {onAddVideo && (
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      onAddVideo();
                      setIsDropdownOpen(false);
                    }}
                    className="w-full px-3 py-2 text-left text-xs text-gray-200 hover:bg-stationpedia-accent/30 flex items-center gap-2"
                  >
                    📺 Video
                  </button>
                )}
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

// Sortable section wrapper for guide sections
interface SortableSectionProps {
  id: string;
  children: (props: { dragHandleProps: any; style: React.CSSProperties; isDragging: boolean }) => React.ReactNode;
}

const SortableSection: React.FC<SortableSectionProps> = ({ id, children }) => {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } =
    useSortable({ id });

  const style: React.CSSProperties = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1,
  };

  return (
    <div ref={setNodeRef} style={style}>
      {children({ dragHandleProps: { ...attributes, ...listeners }, style, isDragging })}
    </div>
  );
};

// Guide-level Add dropdown (for adding top-level sections)
const GuideAddDropdown: React.FC<{
  onAddHeader: () => void;
  onAddSection: () => void;
}> = ({ onAddHeader, onAddSection }) => {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };
    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [isOpen]);

  return (
    <div ref={dropdownRef}>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="px-3 py-1 bg-stationpedia-accent hover:bg-stationpedia-accent-hover text-white rounded text-xs transition-colors flex items-center gap-1"
      >
        + Add
        <svg className={`w-3 h-3 transition-transform ${isOpen ? 'rotate-180' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
        </svg>
      </button>
      {isOpen && (
        <div className="absolute right-0 top-full mt-1 bg-stationpedia-surface border border-stationpedia-border rounded shadow-lg z-50 min-w-[140px]">
          <button
            onClick={() => {
              onAddHeader();
              setIsOpen(false);
            }}
            className="w-full px-3 py-2 text-left text-xs text-gray-200 hover:bg-stationpedia-accent/30 flex items-center gap-2"
          >
            📋 Header
          </button>
          <button
            onClick={() => {
              onAddSection();
              setIsOpen(false);
            }}
            className="w-full px-3 py-2 text-left text-xs text-gray-200 hover:bg-stationpedia-accent/30 flex items-center gap-2"
          >
            📁 Section
          </button>
        </div>
      )}
    </div>
  );
};

export const DeviceSectionsEditor: React.FC<DeviceSectionsEditorProps> = ({
  device,
  onUpdateDevice,
}) => {
  const { workspace } = useEditorStore();
  const [selection, setSelection] = useState<SelectionType>(null);
  const [isLinkModalOpen, setIsLinkModalOpen] = useState(false);
  const [isImagePickerOpen, setIsImagePickerOpen] = useState(false);
  const [imagePickerTarget, setImagePickerTarget] = useState<'page' | 'section'>('page');
  const [expandedSections, setExpandedSections] = useState<Record<string, boolean>>({
    description: true,
    operationalDetails: true,
    pageImage: true,
    logic: false,
    modes: false,
    slots: false,
    connections: false,
  });
  const [editorHeight, setEditorHeight] = useState(200); // Resizable editor height
  const editorRef = useRef<HTMLTextAreaElement>(null);
  
  // Local state for textarea to prevent immediate filtering of empty lines
  const [localEditorValue, setLocalEditorValue] = useState<string>('');
  const [isLocalValueDirty, setIsLocalValueDirty] = useState(false);

  // Get the currently selected text value
  const getSelectedValue = useCallback((): string => {
    if (!device || !selection) return '';

    switch (selection.type) {
      case 'displayName':
        return device.displayName || '';
      case 'pageDescription':
        return device.pageDescription || '';
      case 'pageDescriptionPrepend':
        return device.pageDescriptionPrepend || '';
      case 'pageDescriptionAppend':
        return device.pageDescriptionAppend || '';
      case 'tocTitle':
        return device.tocTitle || '';
      case 'operationalDetail': {
        let detail: OperationalDetail | undefined = device.operationalDetails?.[selection.path[0]];
        for (let i = 1; i < selection.path.length; i++) {
          detail = detail?.children?.[selection.path[i]];
        }
        if (selection.field === 'title') {
          return detail?.title || '';
        } else if (selection.field === 'description') {
          return detail?.description || '';
        } else if (selection.field === 'items') {
          return (detail?.items || []).join('\n');
        } else if (selection.field === 'steps') {
          return (detail?.steps || []).join('\n');
        }
        return '';
      }
      case 'logicDescription':
        return device.logicDescriptions?.[selection.key]?.description || '';
      case 'modeDescription':
        return device.modeDescriptions?.[selection.key]?.description || '';
      case 'slotDescription':
        return device.slotDescriptions?.[selection.key]?.description || '';
      case 'connectionDescription':
        return device.connectionDescriptions?.[selection.key] || '';
      default:
        return '';
    }
  }, [device, selection]);

  // Get label for what's being edited
  const getSelectionLabel = useCallback((): string => {
    if (!selection) return 'Select a field to edit';

    switch (selection.type) {
      case 'displayName':
        return '✏️ Display Name';
      case 'pageDescription':
        return '📝 Page Description';
      case 'pageDescriptionPrepend':
        return '📝 Description Prepend';
      case 'pageDescriptionAppend':
        return '📝 Description Append';
      case 'tocTitle':
        return '📑 Table of Contents Title';
      case 'operationalDetail': {
        // Get the section title for better context
        if (device?.operationalDetails && selection.path.length > 0) {
          let detail: OperationalDetail | undefined = device.operationalDetails[selection.path[0]];
          for (let i = 1; i < selection.path.length; i++) {
            detail = detail?.children?.[selection.path[i]];
          }
          const sectionTitle = detail?.title || 'Section';
          const fieldLabels: Record<string, string> = {
            title: '📋 Title',
            description: '📝 Description', 
            items: '• Items (bullet points)',
            steps: '1. Steps (numbered list)'
          };
          return `${fieldLabels[selection.field] || selection.field} — ${sectionTitle}`;
        }
        return `${selection.field}`;
      }
      case 'logicDescription':
        return `⚡ Logic: ${selection.key}`;
      case 'modeDescription':
        return `🔧 Mode: ${selection.key}`;
      case 'slotDescription':
        return `📦 Slot: ${selection.key}`;
      case 'connectionDescription':
        return `🔌 Connection: ${selection.key}`;
      default:
        return 'Unknown';
    }
  }, [selection, device]);

  // Handle editor value change
  const handleEditorChange = useCallback((value: string) => {
    if (!device || !selection) return;

    switch (selection.type) {
      case 'displayName':
        onUpdateDevice({ displayName: value });
        break;
      case 'pageDescription':
        onUpdateDevice({ pageDescription: value });
        break;
      case 'pageDescriptionPrepend':
        onUpdateDevice({ pageDescriptionPrepend: value });
        break;
      case 'pageDescriptionAppend':
        onUpdateDevice({ pageDescriptionAppend: value });
        break;
      case 'tocTitle':
        onUpdateDevice({ tocTitle: value });
        break;
      case 'operationalDetail': {
        const newDetails = JSON.parse(JSON.stringify(device.operationalDetails || []));
        let detail = newDetails[selection.path[0]];
        for (let i = 1; i < selection.path.length; i++) {
          detail = detail.children[selection.path[i]];
        }
        if (selection.field === 'title') {
          detail.title = value;
        } else if (selection.field === 'description') {
          detail.description = value;
        } else if (selection.field === 'items') {
          // Split by newlines and filter empty
          detail.items = value.split('\n').filter(line => line.trim());
        } else if (selection.field === 'steps') {
          // Split by newlines and filter empty
          detail.steps = value.split('\n').filter(line => line.trim());
        }
        onUpdateDevice({ operationalDetails: newDetails });
        break;
      }
      case 'logicDescription': {
        const newLogic = { ...device.logicDescriptions };
        if (!newLogic[selection.key]) {
          newLogic[selection.key] = { dataType: '', range: '', description: '' };
        }
        newLogic[selection.key].description = value;
        onUpdateDevice({ logicDescriptions: newLogic });
        break;
      }
      case 'modeDescription': {
        const newModes = { ...device.modeDescriptions };
        if (!newModes[selection.key]) {
          newModes[selection.key] = { description: '' };
        }
        newModes[selection.key].description = value;
        onUpdateDevice({ modeDescriptions: newModes });
        break;
      }
      case 'slotDescription': {
        const newSlots = { ...device.slotDescriptions };
        if (!newSlots[selection.key]) {
          newSlots[selection.key] = { description: '' };
        }
        newSlots[selection.key].description = value;
        onUpdateDevice({ slotDescriptions: newSlots });
        break;
      }
      case 'connectionDescription': {
        const newConnections = { ...device.connectionDescriptions };
        newConnections[selection.key] = value;
        onUpdateDevice({ connectionDescriptions: newConnections });
        break;
      }
    }
  }, [device, selection, onUpdateDevice]);

  // Handle editor resize
  const handleEditorResize = useCallback((delta: number) => {
    setEditorHeight(prev => Math.max(100, Math.min(500, prev + delta)));
  }, []);

  // Toggle section expansion
  const toggleSection = (section: string) => {
    setExpandedSections(prev => ({ ...prev, [section]: !prev[section] }));
  };

  // Add operational detail
  const addOperationalDetail = (parentPath?: number[]) => {
    if (!device) return;
    
    const newDetails = JSON.parse(JSON.stringify(device.operationalDetails || []));
    const newDetail: OperationalDetail = { title: 'New Section', description: '' };
    
    if (parentPath && parentPath.length > 0) {
      let parent = newDetails[parentPath[0]];
      for (let i = 1; i < parentPath.length; i++) {
        parent = parent.children[parentPath[i]];
      }
      parent.children = [...(parent.children || []), newDetail];
    } else {
      newDetails.push(newDetail);
    }
    
    onUpdateDevice({ operationalDetails: newDetails });
  };

  // Add inline header (flat section with just title + description, no children)
  const addInlineHeader = (afterPath?: number[]) => {
    if (!device) return;
    
    const newDetails = JSON.parse(JSON.stringify(device.operationalDetails || []));
    const inlineHeader: OperationalDetail = { 
      title: 'New Header', 
      description: '',
      collapsible: false // Inline headers are not collapsible
    };
    
    if (afterPath && afterPath.length > 0) {
      // Insert as a sibling after the specified path
      if (afterPath.length === 1) {
        // Top level - insert after this index
        newDetails.splice(afterPath[0] + 1, 0, inlineHeader);
      } else {
        // Nested - add as child of parent
        let parent = newDetails[afterPath[0]];
        for (let i = 1; i < afterPath.length - 1; i++) {
          parent = parent.children[afterPath[i]];
        }
        parent.children = [...(parent.children || []), inlineHeader];
      }
    } else {
      // Add at end
      newDetails.push(inlineHeader);
    }
    
    onUpdateDevice({ operationalDetails: newDetails });
  };

  // Add a table to a section
  const addTableToSection = (path: number[]) => {
    if (!device) return;
    
    console.log('[addTableToSection] Called with path:', path);
    
    const newDetails = JSON.parse(JSON.stringify(device.operationalDetails || []));
    let detail = newDetails[path[0]];
    for (let i = 1; i < path.length; i++) {
      detail = detail.children[path[i]];
    }
    
    // Add default 3x3 table
    detail.table = [
      { cells: ['Header 1', 'Header 2', 'Header 3'] },
      { cells: ['Cell 1', 'Cell 2', 'Cell 3'] },
      { cells: ['Cell 4', 'Cell 5', 'Cell 6'] },
    ];
    
    console.log('[addTableToSection] Table added to detail:', detail);
    onUpdateDevice({ operationalDetails: newDetails });
    
    // Select this section to show the table in properties
    console.log('[addTableToSection] Selecting path for editing:', path);
    selectOperationalDetail(path, 'description');
    console.log('[addTableToSection] Selection completed');
  };

  // Add an image to a section (opens image picker)
  const addImageToSection = (path: number[]) => {
    if (!device) return;
    // Select this section first so the image picker knows where to add
    selectOperationalDetail(path, 'description');
    setImagePickerTarget('section');
    setIsImagePickerOpen(true);
  };

  // Add a video URL to a section
  const addVideoToSection = (path: number[]) => {
    if (!device) return;
    
    console.log('[addVideoToSection] Called with path:', path);
    
    // Don't use prompt() - Electron blocks it
    // Instead, add empty video fields and let user fill them in the properties panel
    const newDetails = JSON.parse(JSON.stringify(device.operationalDetails || []));
    let detail = newDetails[path[0]];
    for (let i = 1; i < path.length; i++) {
      detail = detail.children[path[i]];
    }
    
    detail.youtubeUrl = '';
    detail.youtubeLabel = 'Watch on YouTube';
    
    console.log('[addVideoToSection] Video placeholders added to detail:', detail);
    onUpdateDevice({ operationalDetails: newDetails });
    
    // Select this section to show the video inputs in properties panel
    selectOperationalDetail(path, 'description');
    console.log('[addVideoToSection] Selection set to path:', path, '- now fill in YouTube video ID in properties panel');
  };

  // Update a property of an operational detail
  const updateDetailProperty = (path: number[], property: string, value: any) => {
    if (!device) return;
    
    const newDetails = JSON.parse(JSON.stringify(device.operationalDetails || []));
    let detail = newDetails[path[0]];
    for (let i = 1; i < path.length; i++) {
      detail = detail.children[path[i]];
    }
    detail[property] = value;
    onUpdateDevice({ operationalDetails: newDetails });
  };

  // Get detail at path
  const getDetailAtPath = (details: OperationalDetail[], path: number[]): OperationalDetail | null => {
    if (!details || path.length === 0) return null;
    let detail = details[path[0]];
    if (!detail) return null;
    for (let i = 1; i < path.length; i++) {
      if (!detail.children || !detail.children[path[i]]) return null;
      detail = detail.children[path[i]];
    }
    return detail;
  };

  // Remove operational detail
  const removeOperationalDetail = (path: number[]) => {
    if (!device || path.length === 0) return;
    
    const newDetails = JSON.parse(JSON.stringify(device.operationalDetails || []));
    
    if (path.length === 1) {
      newDetails.splice(path[0], 1);
    } else {
      let parent = newDetails[path[0]];
      for (let i = 1; i < path.length - 1; i++) {
        parent = parent.children[path[i]];
      }
      parent.children.splice(path[path.length - 1], 1);
    }
    
    onUpdateDevice({ operationalDetails: newDetails });
    setSelection(null);
  };

  // Select operational detail
  const selectOperationalDetail = (path: number[], field: 'title' | 'description' | 'items' | 'steps') => {
    console.log('[selectOperationalDetail] Setting selection:', { type: 'operationalDetail', path, field });
    setSelection({ type: 'operationalDetail', path, field });
  };

  // Generate TOC IDs
  const generateTocIds = () => {
    if (!device?.operationalDetails) return;
    
    const newDetails = JSON.parse(JSON.stringify(device.operationalDetails));
    
    const assignTocIds = (details: OperationalDetail[], prefix = '') => {
      details.forEach((detail, i) => {
        const id = detail.title
          .toLowerCase()
          .replace(/[^a-z0-9]+/g, '-')
          .replace(/^-|-$/g, '');
        detail.tocId = prefix ? `${prefix}-${id}` : id;
        if (detail.children) {
          assignTocIds(detail.children, detail.tocId);
        }
      });
    };
    
    assignTocIds(newDetails);
    onUpdateDevice({ 
      operationalDetails: newDetails,
      generateToc: true,
      tocTitle: device.tocTitle || 'Quick Navigation'
    });
  };

  // Rich text formatting helpers
  const getSelectionRange = useCallback(() => {
    if (!editorRef.current) return null;
    const start = editorRef.current.selectionStart;
    const end = editorRef.current.selectionEnd;
    const text = editorRef.current.value;
    return { start, end, selectedText: text.substring(start, end) };
  }, []);

  const insertAtCursor = useCallback((before: string, after: string = '') => {
    if (!editorRef.current || !selection) return;
    
    const range = getSelectionRange();
    if (!range) return;
    
    const { start, end, selectedText } = range;
    const currentText = editorRef.current.value;
    
    let newText: string;
    let newCursorPos: number;
    
    if (selectedText) {
      // Wrap selected text
      newText = currentText.substring(0, start) + before + selectedText + after + currentText.substring(end);
      newCursorPos = start + before.length + selectedText.length + after.length;
    } else {
      // Insert at cursor
      newText = currentText.substring(0, start) + before + after + currentText.substring(end);
      newCursorPos = start + before.length;
    }
    
    // Update local state first
    setLocalEditorValue(newText);
    setIsLocalValueDirty(true);
    
    // For items/steps, don't sync immediately
    if (selection?.type === 'operationalDetail' && 
        (selection.field === 'items' || selection.field === 'steps')) {
      // Will sync on blur
    } else {
      handleEditorChange(newText);
    }
    
    // Restore cursor position after state update
    setTimeout(() => {
      if (editorRef.current) {
        editorRef.current.focus();
        editorRef.current.setSelectionRange(newCursorPos, newCursorPos);
      }
    }, 0);
  }, [selection, getSelectionRange, handleEditorChange]);

  const insertTMPTag = useCallback((tag: string) => {
    insertAtCursor(`<${tag}>`, `</${tag}>`);
  }, [insertAtCursor]);

  const insertColorTag = useCallback((color: string) => {
    insertAtCursor(`<color=${color}>`, '</color>');
  }, [insertAtCursor]);

  const insertSizeTag = useCallback((size: string) => {
    insertAtCursor(`<size=${size}>`, '</size>');
  }, [insertAtCursor]);

  const insertBulletList = useCallback(() => {
    const range = getSelectionRange();
    if (!range) return;
    
    if (range.selectedText) {
      // Convert selected lines to bullet list
      const lines = range.selectedText.split('\n');
      const bulletLines = lines.map(line => line.trim() ? `• ${line.trim()}` : '').join('\n');
      insertAtCursor(bulletLines, '');
    } else {
      insertAtCursor('• ', '');
    }
  }, [getSelectionRange, insertAtCursor]);

  const insertNumberedList = useCallback(() => {
    const range = getSelectionRange();
    if (!range) return;
    
    if (range.selectedText) {
      // Convert selected lines to numbered list
      const lines = range.selectedText.split('\n');
      const numberedLines = lines.map((line, i) => line.trim() ? `${i + 1}. ${line.trim()}` : '').join('\n');
      insertAtCursor(numberedLines, '');
    } else {
      insertAtCursor('1. ', '');
    }
  }, [getSelectionRange, insertAtCursor]);

  const insertLink = useCallback(() => {
    setIsLinkModalOpen(true);
  }, []);

  const handleInsertLink = useCallback((linkText: string, displayText: string) => {
    // LinkText comes in format {LINK:PageKey;DisplayText}
    // Insert it at current cursor position
    insertAtCursor(linkText, '');
    setIsLinkModalOpen(false);
  }, [insertAtCursor]);

  const insertHeader = useCallback(() => {
    // Insert header placeholder that user can edit
    insertAtCursor('{HEADER:', '}');
  }, [insertAtCursor]);

  const insertText = useCallback((text: string) => {
    insertAtCursor(text, '');
  }, [insertAtCursor]);

  // DnD sensors
  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 8 } }),
    useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates })
  );

  // Handle drag end for reordering
  const handleDragEnd = useCallback((event: any) => {
    const { active, over } = event;
    if (!over || active.id === over.id || !device?.operationalDetails) return;

    // Parse IDs to get paths
    const activeIndex = parseInt(active.id.split('-')[0]);
    const overIndex = parseInt(over.id.split('-')[0]);

    if (activeIndex !== overIndex) {
      const newDetails = [...device.operationalDetails];
      const [removed] = newDetails.splice(activeIndex, 1);
      newDetails.splice(overIndex, 0, removed);
      onUpdateDevice({ operationalDetails: newDetails });
    }
  }, [device, onUpdateDevice]);

  // Focus editor when selection changes
  useEffect(() => {
    if (selection && editorRef.current) {
      editorRef.current.focus();
    }
    // Reset dirty flag when selection changes so new value loads
    setIsLocalValueDirty(false);
  }, [selection]);

  // Sync local editor value when selection changes or device data changes externally
  useEffect(() => {
    if (!isLocalValueDirty) {
      const newValue = getSelectedValue();
      setLocalEditorValue(newValue);
    }
  }, [selection, device, isLocalValueDirty, getSelectedValue]);

  // Handle local textarea change - update local state immediately
  const handleLocalChange = useCallback((value: string) => {
    setLocalEditorValue(value);
    setIsLocalValueDirty(true);
    
    // For items/steps, we want to allow typing newlines freely
    // Only sync to device for non-list fields, or debounce for list fields
    if (selection?.type === 'operationalDetail' && 
        (selection.field === 'items' || selection.field === 'steps')) {
      // Don't sync immediately for list fields to allow Enter to work
      return;
    }
    
    // For non-list fields, sync immediately
    handleEditorChange(value);
  }, [selection, handleEditorChange]);

  // Handle blur - sync items/steps to device when leaving the textarea
  const handleEditorBlur = useCallback(() => {
    if (selection?.type === 'operationalDetail' && 
        (selection.field === 'items' || selection.field === 'steps') &&
        isLocalValueDirty) {
      handleEditorChange(localEditorValue);
    }
    setIsLocalValueDirty(false);
  }, [selection, localEditorValue, isLocalValueDirty, handleEditorChange]);

  if (!device) {
    return (
      <div className="flex items-center justify-center h-full text-gray-500">
        Select a device to edit
      </div>
    );
  }

  // Handle markdown guides/mechanics - show a simple text editor (read-only reference)
  const isMarkdownContent = (device as any)._isMarkdown === true;
  
  if (isMarkdownContent) {
    return (
      <div className="flex flex-col h-full bg-stationpedia-bg p-4">
        <div className="flex items-center justify-between mb-2">
          <span className="text-lg font-bold text-stationpedia-accent">
            📖 {device.displayName || (device as any).guideKey || device.deviceKey}
          </span>
          <span className="text-xs text-gray-500">Markdown Guide (Reference Only)</span>
        </div>
        <div className="flex-1 flex flex-col">
          <label className="text-sm text-gray-400 mb-1">Content (Markdown - Read Only)</label>
          <textarea
            className="flex-1 bg-stationpedia-panel border border-stationpedia-border rounded p-3 text-white font-mono text-sm resize-none opacity-70"
            value={device.pageDescription || ''}
            readOnly
            placeholder="Markdown content..."
          />
          <p className="text-xs text-yellow-500 mt-2">
            ⚠️ Markdown guides are for reference only. Create JSON guides in descriptions.json to edit them.
          </p>
        </div>
      </div>
    );
  }

  // Check if this is a JSON guide (has guideKey instead of deviceKey)
  const isJsonGuide = !!(device as any).guideKey;
  const itemKey = isJsonGuide ? (device as any).guideKey : device.deviceKey;

  const currentValue = getSelectedValue();
  const ids = (device.operationalDetails || []).map((_, i) => `${i}-item`);

  // Check if selection is for an operational detail
  const isOpDetailSelected = (path: number[], field: 'title' | 'description' | 'items' | 'steps') => {
    if (selection?.type !== 'operationalDetail') return false;
    return selection.path.join(',') === path.join(',') && selection.field === field;
  };

  // Navigation from store
  const canGoBack = useEditorStore((s) => s.canGoBack());
  const canGoForward = useEditorStore((s) => s.canGoForward());
  const goBack = useEditorStore((s) => s.goBack);
  const goForward = useEditorStore((s) => s.goForward);

  return (
    <div className="flex flex-col h-full bg-stationpedia-bg">
      {/* Navigation Bar */}
      <div className="flex items-center justify-between px-3 py-1.5 border-b border-stationpedia-border bg-stationpedia-surface/50 flex-shrink-0">
        <div className="flex items-center gap-1">
          <button
            onClick={goBack}
            disabled={!canGoBack}
            className={`p-1.5 rounded transition-colors ${
              canGoBack 
                ? 'text-gray-300 hover:text-white hover:bg-gray-700' 
                : 'text-gray-600 cursor-not-allowed'
            }`}
            title="Go back (previous device)"
          >
            <svg className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M15 18l-6-6 6-6" />
            </svg>
          </button>
          <button
            onClick={goForward}
            disabled={!canGoForward}
            className={`p-1.5 rounded transition-colors ${
              canGoForward 
                ? 'text-gray-300 hover:text-white hover:bg-gray-700' 
                : 'text-gray-600 cursor-not-allowed'
            }`}
            title="Go forward (next device)"
          >
            <svg className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M9 18l6-6-6-6" />
            </svg>
          </button>
        </div>
        <span className="text-xs text-gray-400 truncate max-w-[200px]" title={device?.displayName || undefined}>
          {device?.displayName || 'No device selected'}
        </span>
      </div>
      
      {/* Top: Rich Text Editor (resizable) */}
      <div className="border-b border-stationpedia-border p-3 flex-shrink-0 flex flex-col overflow-hidden" style={{ minHeight: editorHeight, maxHeight: editorHeight }}>
        <div className="flex items-center justify-between mb-2 flex-shrink-0">
          <span className="text-sm font-medium text-stationpedia-accent">
            {getSelectionLabel()}
          </span>
          {selection && (
            <button
              onClick={() => setSelection(null)}
              className="text-xs text-gray-400 hover:text-white"
            >
              ✕ Clear
            </button>
          )}
        </div>
        
        {/* Rich Text Formatting Toolbar - NOW ABOVE textarea */}
        <div className="flex flex-wrap gap-1 mb-2 p-2 bg-stationpedia-surface/50 rounded border border-stationpedia-border flex-shrink-0">
          {/* Text Formatting */}
          <button
            onClick={() => insertTMPTag('b')}
            disabled={!selection}
            className="px-2 py-1 text-xs bg-stationpedia-surface hover:bg-gray-700 rounded text-gray-300 disabled:opacity-50 font-bold"
            title="Bold: <b>text</b>"
          >
            B
          </button>
          <button
            onClick={() => insertTMPTag('i')}
            disabled={!selection}
            className="px-2 py-1 text-xs bg-stationpedia-surface hover:bg-gray-700 rounded text-gray-300 disabled:opacity-50 italic"
            title="Italic: <i>text</i>"
          >
            I
          </button>
          <button
            onClick={() => insertTMPTag('u')}
            disabled={!selection}
            className="px-2 py-1 text-xs bg-stationpedia-surface hover:bg-gray-700 rounded text-gray-300 disabled:opacity-50 underline"
            title="Underline: <u>text</u>"
          >
            U
          </button>
          <button
            onClick={() => insertTMPTag('s')}
            disabled={!selection}
            className="px-2 py-1 text-xs bg-stationpedia-surface hover:bg-gray-700 rounded text-gray-300 disabled:opacity-50 line-through"
            title="Strikethrough: <s>text</s>"
          >
            S
          </button>
          
          <div className="w-px h-6 bg-stationpedia-border mx-1" />
          
          {/* Colors */}
          <button
            onClick={() => insertColorTag('#FF7A18')}
            disabled={!selection}
            className="px-2 py-1 text-xs bg-stationpedia-surface hover:bg-gray-700 rounded disabled:opacity-50"
            style={{ color: '#FF7A18' }}
            title="Orange: <color=#FF7A18>text</color>"
          >
            🟠
          </button>
          <button
            onClick={() => insertColorTag('#00FF00')}
            disabled={!selection}
            className="px-2 py-1 text-xs bg-stationpedia-surface hover:bg-gray-700 rounded disabled:opacity-50"
            style={{ color: '#00FF00' }}
            title="Green: <color=#00FF00>text</color>"
          >
            🟢
          </button>
          <button
            onClick={() => insertColorTag('#FF0000')}
            disabled={!selection}
            className="px-2 py-1 text-xs bg-stationpedia-surface hover:bg-gray-700 rounded disabled:opacity-50"
            style={{ color: '#FF0000' }}
            title="Red: <color=#FF0000>text</color>"
          >
            🔴
          </button>
          <button
            onClick={() => insertColorTag('#FFFF00')}
            disabled={!selection}
            className="px-2 py-1 text-xs bg-stationpedia-surface hover:bg-gray-700 rounded disabled:opacity-50"
            style={{ color: '#FFFF00' }}
            title="Yellow: <color=#FFFF00>text</color>"
          >
            🟡
          </button>
          <button
            onClick={() => insertColorTag('#00FFFF')}
            disabled={!selection}
            className="px-2 py-1 text-xs bg-stationpedia-surface hover:bg-gray-700 rounded disabled:opacity-50"
            style={{ color: '#00FFFF' }}
            title="Cyan: <color=#00FFFF>text</color>"
          >
            🔵
          </button>
          
          <div className="w-px h-6 bg-stationpedia-border mx-1" />
          
          {/* Size */}
          <button
            onClick={() => insertSizeTag('150%')}
            disabled={!selection}
            className="px-2 py-1 text-xs bg-stationpedia-surface hover:bg-gray-700 rounded text-gray-300 disabled:opacity-50"
            title="Large text: <size=150%>text</size>"
          >
            A+
          </button>
          <button
            onClick={() => insertSizeTag('75%')}
            disabled={!selection}
            className="px-2 py-1 text-xs bg-stationpedia-surface hover:bg-gray-700 rounded text-gray-300 disabled:opacity-50 text-[10px]"
            title="Small text: <size=75%>text</size>"
          >
            A-
          </button>
          
          <div className="w-px h-6 bg-stationpedia-border mx-1" />
          
          {/* Lists & Structure */}
          <button
            onClick={() => insertBulletList()}
            disabled={!selection}
            className="px-2 py-1 text-xs bg-stationpedia-surface hover:bg-gray-700 rounded text-gray-300 disabled:opacity-50"
            title="Bullet list"
          >
            • List
          </button>
          <button
            onClick={() => insertNumberedList()}
            disabled={!selection}
            className="px-2 py-1 text-xs bg-stationpedia-surface hover:bg-gray-700 rounded text-gray-300 disabled:opacity-50"
            title="Numbered list"
          >
            1. List
          </button>
          
          <div className="w-px h-6 bg-stationpedia-border mx-1" />
          
          {/* Links & Headers */}
          <button
            onClick={() => insertLink()}
            disabled={!selection}
            className="px-2 py-1 text-xs bg-stationpedia-surface hover:bg-gray-700 rounded text-gray-300 disabled:opacity-50"
            title="Link to page: {LINK:PageKey;DisplayText}"
          >
            🔗 Link
          </button>
          <button
            onClick={() => insertHeader()}
            disabled={!selection}
            className="px-2 py-1 text-xs bg-orange-600 hover:bg-orange-700 rounded text-white disabled:opacity-50"
            title="Insert In-Line Header: {HEADER:Text}"
          >
            📋 In-Line Header
          </button>
          <button
            onClick={() => insertText('\n')}
            disabled={!selection}
            className="px-2 py-1 text-xs bg-stationpedia-surface hover:bg-gray-700 rounded text-gray-300 disabled:opacity-50"
            title="New line"
          >
            ↵
          </button>
        </div>
        
        {/* Textarea - flex-1 to fill remaining space */}
        <div className="flex-1 flex flex-col min-h-0 overflow-auto">
          <textarea
            ref={editorRef}
            value={localEditorValue}
            onChange={(e) => handleLocalChange(e.target.value)}
            onBlur={handleEditorBlur}
            disabled={!selection}
            placeholder={selection ? 'Enter text...' : 'Click a field below to edit'}
            className="flex-1 w-full px-3 py-2 bg-stationpedia-surface border border-stationpedia-border rounded text-sm text-white placeholder-gray-500 focus:outline-none focus:border-stationpedia-accent focus:ring-1 focus:ring-stationpedia-accent resize-none disabled:opacity-50 disabled:cursor-not-allowed font-mono min-h-[80px]"
          />
        
          {/* Properties Panel - shown when operational detail is selected */}
          {selection?.type === 'operationalDetail' && (() => {
            const detail = getDetailAtPath(device.operationalDetails!, selection.path);
            console.log('[Properties Panel] Selection:', selection, 'Detail found:', detail);
            if (!detail) {
              console.log('[Properties Panel] No detail found at path:', selection.path);
              return null;
            }
          
            return (
              <div className="section-properties-panel mt-2 flex-shrink-0">
              <div className="section-properties-header">📋 Section Properties</div>
              <div className="section-properties-grid">
                {/* Collapsible checkbox */}
                <label className="section-property-checkbox-label">
                  <input
                    type="checkbox"
                    checked={detail.collapsible || false}
                    onChange={(e) => updateDetailProperty(selection.path, 'collapsible', e.target.checked)}
                    className="section-property-checkbox"
                  />
                  📁 Collapsible
                </label>
                
                {/* TOC ID input */}
                <div className="section-property-row">
                  <span className="section-property-label">🔗 TOC ID:</span>
                  <input
                    type="text"
                    value={detail.tocId || ''}
                    onChange={(e) => updateDetailProperty(selection.path, 'tocId', e.target.value)}
                    placeholder="optional"
                    className="section-property-input"
                  />
                </div>
                
                {/* Image file input with browse button */}
                <div className="section-property-row section-property-full-width">
                  <span className="section-property-label">🖼️ Image:</span>
                  <div className="flex gap-1 flex-1">
                    <input
                      type="text"
                      value={detail.imageFile || ''}
                      onChange={(e) => updateDetailProperty(selection.path, 'imageFile', e.target.value)}
                      placeholder="image-filename.png"
                      className="section-property-input flex-1"
                    />
                    <button
                      onClick={() => {
                        setImagePickerTarget('section');
                        setIsImagePickerOpen(true);
                      }}
                      className="px-2 py-0.5 bg-stationpedia-accent hover:bg-stationpedia-accent-hover text-white rounded text-xs flex-shrink-0"
                      title="Browse extracted game images"
                    >
                      📂
                    </button>
                    {detail.imageFile && (
                      <button
                        onClick={() => updateDetailProperty(selection.path, 'imageFile', '')}
                        className="px-2 py-0.5 bg-red-600 hover:bg-red-700 text-white rounded text-xs flex-shrink-0"
                        title="Remove image"
                      >
                        ✕
                      </button>
                    )}
                  </div>
                </div>
                
                {/* YouTube URL input */}
                <div className="section-property-row section-property-full-width">
                  <span className="section-property-label">📺 YouTube:</span>
                  <input
                    type="text"
                    value={detail.youtubeUrl || ''}
                    onChange={(e) => updateDetailProperty(selection.path, 'youtubeUrl', e.target.value)}
                    placeholder="Video ID"
                    className="section-property-input"
                  />
                </div>
                
                {/* YouTube Label input */}
                {detail.youtubeUrl && (
                  <div className="section-property-row section-property-full-width">
                    <span className="section-property-label">Label:</span>
                    <input
                      type="text"
                      value={detail.youtubeLabel || ''}
                      onChange={(e) => updateDetailProperty(selection.path, 'youtubeLabel', e.target.value)}
                      placeholder="Video title"
                      className="section-property-input"
                    />
                  </div>
                )}
                
                {/* Table editor */}
                <div className="section-property-row section-property-full-width col-span-2">
                  <div className="flex items-center justify-between w-full mb-2">
                    <span className="section-property-label">📊 Table:</span>
                    {!detail.table || detail.table.length === 0 ? (
                      <button
                        onClick={() => {
                          // Add default 3x3 table
                          const defaultTable: TableRow[] = [
                            { cells: ['Header 1', 'Header 2', 'Header 3'] },
                            { cells: ['Cell 1', 'Cell 2', 'Cell 3'] },
                            { cells: ['Cell 4', 'Cell 5', 'Cell 6'] },
                          ];
                          updateDetailProperty(selection.path, 'table', defaultTable);
                        }}
                        className="px-2 py-1 text-xs bg-green-600 hover:bg-green-700 text-white rounded"
                      >
                        + Add Table
                      </button>
                    ) : (
                      <button
                        onClick={() => {
                          if (confirm('Delete this table?')) {
                            updateDetailProperty(selection.path, 'table', null);
                          }
                        }}
                        className="px-2 py-1 text-xs bg-red-600 hover:bg-red-700 text-white rounded"
                      >
                        🗑️ Remove Table
                      </button>
                    )}
                  </div>
                  
                  {/* Table editor grid */}
                  {detail.table && detail.table.length > 0 && (
                    <div className="w-full space-y-2 mt-2">
                      {/* Table controls */}
                      <div className="flex gap-2 text-xs">
                        <button
                          onClick={() => {
                            const newTable = [...detail.table!];
                            const colCount = newTable[0]?.cells?.length || 3;
                            newTable.push({ cells: Array(colCount).fill('') });
                            updateDetailProperty(selection.path, 'table', newTable);
                          }}
                          className="px-2 py-1 bg-blue-600 hover:bg-blue-700 text-white rounded"
                        >
                          + Row
                        </button>
                        <button
                          onClick={() => {
                            const newTable = detail.table!.map(row => ({
                              cells: [...(row.cells || []), '']
                            }));
                            updateDetailProperty(selection.path, 'table', newTable);
                          }}
                          className="px-2 py-1 bg-blue-600 hover:bg-blue-700 text-white rounded"
                        >
                          + Column
                        </button>
                      </div>
                      
                      {/* Editable table grid */}
                      <div className="overflow-x-auto">
                        <table className="w-full border-collapse text-xs">
                          <tbody>
                            {detail.table.map((row, rowIndex) => (
                              <tr key={rowIndex} className={rowIndex === 0 ? 'bg-orange-900/30' : ''}>
                                {row.cells?.map((cell, colIndex) => (
                                  <td key={colIndex} className="border border-stationpedia-border p-1">
                                    <input
                                      type="text"
                                      value={cell}
                                      onChange={(e) => {
                                        const newTable = JSON.parse(JSON.stringify(detail.table));
                                        newTable[rowIndex].cells[colIndex] = e.target.value;
                                        updateDetailProperty(selection.path, 'table', newTable);
                                      }}
                                      className={`w-full min-w-[80px] px-2 py-1 bg-stationpedia-surface border border-stationpedia-border/50 rounded text-white text-center ${rowIndex === 0 ? 'font-bold text-orange-400' : ''}`}
                                      placeholder={rowIndex === 0 ? 'Header' : 'Cell'}
                                    />
                                  </td>
                                ))}
                                {/* Row delete button */}
                                <td className="border border-stationpedia-border p-1 w-6">
                                  {detail.table!.length > 1 && (
                                    <button
                                      onClick={() => {
                                        const newTable = detail.table!.filter((_, i) => i !== rowIndex);
                                        updateDetailProperty(selection.path, 'table', newTable);
                                      }}
                                      className="text-red-400 hover:text-red-300"
                                      title="Delete row"
                                    >
                                      ✕
                                    </button>
                                  )}
                                </td>
                              </tr>
                            ))}
                            {/* Column delete buttons row */}
                            {detail.table[0]?.cells && detail.table[0].cells.length > 1 && (
                              <tr>
                                {detail.table[0].cells.map((_, colIndex) => (
                                  <td key={colIndex} className="p-1 text-center">
                                    <button
                                      onClick={() => {
                                        const newTable = detail.table!.map(row => ({
                                          cells: row.cells?.filter((_, i) => i !== colIndex) || []
                                        }));
                                        updateDetailProperty(selection.path, 'table', newTable);
                                      }}
                                      className="text-red-400 hover:text-red-300 text-xs"
                                      title="Delete column"
                                    >
                                      ✕
                                    </button>
                                  </td>
                                ))}
                                <td></td>
                              </tr>
                            )}
                          </tbody>
                        </table>
                      </div>
                      <div className="text-xs text-gray-500">
                        First row = headers (bold, orange). All cells center-aligned.
                      </div>
                    </div>
                  )}
                </div>
              </div>
            </div>
          );
        })()}
        </div> {/* End of scrollable wrapper for textarea + properties */}
      </div>

      {/* Resizable Splitter */}
      <ResizableSplitter direction="vertical" onResize={handleEditorResize} />

      {/* Bottom: Section Tree */}
      <div className="flex-1 overflow-auto p-3">
        {/* Device/Guide Info Section */}
        <div className="mb-3">
          <SectionHeader
            title="Device/Guide Info"
            isExpanded={expandedSections.deviceInfo !== false}
            onToggle={() => toggleSection('deviceInfo')}
          />
          {expandedSections.deviceInfo !== false && (
            <div className="border border-t-0 border-stationpedia-border rounded-b p-2 space-y-2">
              {/* Display Name */}
              <div
                onClick={() => setSelection({ type: 'displayName' })}
                className={`p-2 rounded cursor-pointer ${
                  selection?.type === 'displayName'
                    ? 'bg-stationpedia-accent/30 border border-stationpedia-accent'
                    : 'bg-stationpedia-surface/50 hover:bg-stationpedia-accent/20'
                }`}
              >
                <div className="text-xs text-gray-400 mb-1">✏️ Display Name (shown in Stationpedia)</div>
                <div className="text-sm text-stationpedia-accent font-semibold">
                  {device.displayName || '(not set - using key)'}
                </div>
              </div>

              {/* Key (read-only) */}
              <div className="p-2 rounded bg-stationpedia-surface/30 border border-stationpedia-border/50">
                <div className="text-xs text-gray-400 mb-1">🔑 {isJsonGuide ? 'Guide Key' : 'Device Key'} (read-only)</div>
                <div className="text-xs text-gray-500 font-mono">
                  {itemKey}
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Page Description Section */}
        <div className="mb-3">
          <SectionHeader
            title="Page Description"
            isExpanded={expandedSections.description}
            onToggle={() => toggleSection('description')}
          />
          {expandedSections.description && (
            <div className="border border-t-0 border-stationpedia-border rounded-b p-2 space-y-2">
              {/* Prepend */}
              <div
                onClick={() => setSelection({ type: 'pageDescriptionPrepend' })}
                className={`p-2 rounded cursor-pointer ${
                  selection?.type === 'pageDescriptionPrepend'
                    ? 'bg-stationpedia-accent/30 border border-stationpedia-accent'
                    : 'bg-stationpedia-surface/50 hover:bg-stationpedia-accent/20'
                }`}
              >
                <div className="text-xs text-gray-400 mb-1">Prepend (shows before main description)</div>
                <div className="text-sm text-gray-300 whitespace-pre-wrap">
                  {device.pageDescriptionPrepend || '(empty)'}
                </div>
              </div>

              {/* Main Description */}
              <div
                onClick={() => setSelection({ type: 'pageDescription' })}
                className={`p-2 rounded cursor-pointer ${
                  selection?.type === 'pageDescription'
                    ? 'bg-stationpedia-accent/30 border border-stationpedia-accent'
                    : 'bg-stationpedia-surface/50 hover:bg-stationpedia-accent/20'
                }`}
              >
                <div className="text-xs text-gray-400 mb-1">Main Description</div>
                <div className="text-sm text-gray-300 whitespace-pre-wrap">
                  {device.pageDescription || '(uses game default)'}
                </div>
              </div>

              {/* Append */}
              <div
                onClick={() => setSelection({ type: 'pageDescriptionAppend' })}
                className={`p-2 rounded cursor-pointer ${
                  selection?.type === 'pageDescriptionAppend'
                    ? 'bg-stationpedia-accent/30 border border-stationpedia-accent'
                    : 'bg-stationpedia-surface/50 hover:bg-stationpedia-accent/20'
                }`}
              >
                <div className="text-xs text-gray-400 mb-1">Append (shows after main description)</div>
                <div className="text-sm text-gray-300 whitespace-pre-wrap">
                  {device.pageDescriptionAppend || '(empty)'}
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Sections - For Guides show as independent sections, for Devices wrap in Operational Details */}
        {isJsonGuide ? (
          // GUIDES: Show each root operationalDetail as independent section
          <>
            {/* Guide Page Image Section */}
            <div className="mb-3">
              <SectionHeader
                title="🖼️ Page Image"
                isExpanded={expandedSections.pageImage !== false}
                onToggle={() => toggleSection('pageImage')}
              />
              {expandedSections.pageImage !== false && (
                <div className="border border-t-0 border-stationpedia-border rounded-b p-3">
                  <div className="text-xs text-gray-400 mb-2">
                    Optional header image displayed at the top of the guide page (like vanilla guides)
                  </div>
                  <div className="flex items-center gap-2">
                    <input
                      type="text"
                      value={(device as any).pageImage || ''}
                      onChange={(e) => onUpdateDevice({ pageImage: e.target.value })}
                      placeholder="image-filename.png"
                      className="flex-1 px-2 py-1 bg-stationpedia-surface border border-stationpedia-border rounded text-sm text-white placeholder-gray-500"
                    />
                    <button
                      onClick={() => {
                        setImagePickerTarget('page');
                        setIsImagePickerOpen(true);
                      }}
                      className="px-3 py-1 bg-stationpedia-accent hover:bg-stationpedia-accent-hover text-white rounded text-sm"
                      title="Browse extracted game images"
                    >
                      📂 Browse
                    </button>
                    {(device as any).pageImage && (
                      <button
                        onClick={() => onUpdateDevice({ pageImage: '' })}
                        className="px-2 py-1 bg-red-600 hover:bg-red-700 text-white rounded text-sm"
                        title="Remove image"
                      >
                        ✕
                      </button>
                    )}
                  </div>
                  {(device as any).pageImage && (
                    <div className="mt-2 p-2 bg-gray-800 rounded text-xs text-gray-400">
                      Current: {(device as any).pageImage}
                    </div>
                  )}
                </div>
              )}
            </div>

            <div className="mb-3 p-2 bg-stationpedia-surface/30 border border-stationpedia-border rounded">
              <div className="flex items-center justify-between mb-2">
                <span className="text-sm font-semibold text-gray-300">📑 Guide Sections</span>
                <div className="relative">
                  <GuideAddDropdown
                    onAddHeader={() => addInlineHeader()}
                    onAddSection={() => addOperationalDetail()}
                  />
                </div>
              </div>
              
              {/* TOC Settings for Guides */}
              <div className="flex items-center gap-2 mb-2 p-2 bg-stationpedia-surface/50 rounded">
                <label className="flex items-center gap-2 text-sm text-gray-300">
                  <input
                    type="checkbox"
                    checked={device.generateToc || false}
                    onChange={(e) => onUpdateDevice({ generateToc: e.target.checked })}
                    className="rounded"
                  />
                  Generate Table of Contents
                </label>
                {device.generateToc && (
                  <label className="flex items-center gap-2 text-sm text-gray-300">
                    <input
                      type="checkbox"
                      checked={!device.tocFlat}
                      onChange={(e) => onUpdateDevice({ tocFlat: !e.target.checked })}
                      className="rounded"
                    />
                    Nested
                  </label>
                )}
                <button
                  onClick={generateTocIds}
                  className="px-2 py-1 text-xs bg-purple-600 hover:bg-purple-700 text-white rounded"
                  title="Auto-generate TOC IDs from section titles"
                >
                  🔗 Generate TOC IDs
                </button>
                {device.generateToc && (
                  <div
                    onClick={() => setSelection({ type: 'tocTitle' })}
                    className={`flex-1 px-2 py-1 rounded cursor-pointer text-sm ${
                      selection?.type === 'tocTitle'
                        ? 'bg-stationpedia-accent/30'
                        : 'hover:bg-stationpedia-accent/20'
                    }`}
                  >
                    Title: {device.tocTitle || 'Contents'}
                  </div>
                )}
              </div>
              
              <div className="text-xs text-gray-400">
                <b>Sections</b> are collapsible categories. <b>Headers</b> are flat inline titles with text below.
              </div>
            </div>

            {(device.operationalDetails?.length || 0) === 0 ? (
              <div className="text-center text-gray-500 py-8 text-sm border border-dashed border-stationpedia-border rounded">
                No sections yet. Click "+ Section" or "+ Header" to create one.
              </div>
            ) : (
              <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={handleDragEnd}>
                <SortableContext items={ids} strategy={verticalListSortingStrategy}>
                  {device.operationalDetails!.map((detail, i) => (
                    <SortableSection key={`${i}-item`} id={`${i}-item`}>
                      {({ dragHandleProps, isDragging }) => (
                        <div className={`mb-3 ${isDragging ? 'shadow-lg ring-2 ring-stationpedia-accent' : ''}`}>
                          <SectionHeader
                            title={detail.title || 'Untitled Section'}
                            isExpanded={expandedSections[`section_${i}`] !== false}
                            onToggle={() => toggleSection(`section_${i}`)}
                            onTitleClick={() => selectOperationalDetail([i], 'title')}
                            onAddTable={() => addTableToSection([i])}
                            onAddHeader={() => addInlineHeader([i])}
                            onAddSubsection={() => addOperationalDetail([i])}
                            onAddImage={() => addImageToSection([i])}
                            onAddVideo={() => addVideoToSection([i])}
                            showDelete={true}
                            onDelete={() => removeOperationalDetail([i])}
                            isSelected={selection?.type === 'operationalDetail' && selection.path.length === 1 && selection.path[0] === i && selection.field === 'title'}
                            showDragHandle={true}
                            dragHandleProps={dragHandleProps}
                          />
                          {expandedSections[`section_${i}`] !== false && (
                            <div className="border border-t-0 border-stationpedia-border rounded-b p-2">
                              <SortableDetailItem
                                id={`${i}-item-content`}
                                detail={detail}
                                path={[i]}
                                depth={0}
                                selectedPath={
                                  selection?.type === 'operationalDetail'
                                    ? selection.path
                                    : null
                                }
                                selectedField={
                                  selection?.type === 'operationalDetail'
                                    ? selection.field
                                    : null
                                }
                                onSelect={selectOperationalDetail}
                                onAdd={addOperationalDetail}
                                onRemove={removeOperationalDetail}
                                onAddTable={addTableToSection}
                                onAddImage={addImageToSection}
                                onAddVideo={addVideoToSection}
                                onAddHeader={addInlineHeader}
                                hideTitle={true}  // Title already shown in SectionHeader above
                                hideDragHandle={true}  // Reorder via SectionHeader, not drag handle
                              />
                            </div>
                          )}
                        </div>
                      )}
                    </SortableSection>
                  ))}
                </SortableContext>
              </DndContext>
            )}
          </>
        ) : (
          // DEVICES: Keep the Operational Details wrapper
          <div className="mb-3">
            <SectionHeader
              title="Operational Details"
              isExpanded={expandedSections.operationalDetails}
              onToggle={() => toggleSection('operationalDetails')}
              onAddTable={() => {
                // Add a new section with a table
                const newDetails = JSON.parse(JSON.stringify(device.operationalDetails || []));
                const newDetail: OperationalDetail = { 
                  title: 'New Table Section', 
                  description: '',
                  table: [
                    { cells: ['Header 1', 'Header 2', 'Header 3'] },
                    { cells: ['Cell 1', 'Cell 2', 'Cell 3'] },
                    { cells: ['Cell 4', 'Cell 5', 'Cell 6'] },
                  ]
                };
                newDetails.push(newDetail);
                onUpdateDevice({ operationalDetails: newDetails });
              }}
              onAddHeader={() => addInlineHeader()}
              onAddSubsection={() => addOperationalDetail()}
              onAddImage={() => {
                // Add a new section and open image picker
                const newDetails = JSON.parse(JSON.stringify(device.operationalDetails || []));
                const newDetail: OperationalDetail = { title: 'New Image Section', description: '' };
                newDetails.push(newDetail);
                onUpdateDevice({ operationalDetails: newDetails });
                // Select the new section to add image to it
                setTimeout(() => {
                  selectOperationalDetail([newDetails.length - 1], 'description');
                  setImagePickerTarget('section');
                  setIsImagePickerOpen(true);
                }, 100);
              }}
              onAddVideo={() => {
                const newDetails = JSON.parse(JSON.stringify(device.operationalDetails || []));
                const newDetail: OperationalDetail = { 
                  title: 'Video Section', 
                  description: 'Enter a description for this video section.',
                  youtubeUrl: '',
                  youtubeLabel: 'Watch on YouTube'
                };
                newDetails.push(newDetail);
                onUpdateDevice({ operationalDetails: newDetails });
                // Select the new section so user can fill in video ID in properties panel
                setTimeout(() => {
                  selectOperationalDetail([newDetails.length - 1], 'description');
                }, 100);
              }}
            />
            {expandedSections.operationalDetails && (
            <div className="border border-t-0 border-stationpedia-border rounded-b p-2">
              {/* TOC Settings */}
              <div className="flex items-center gap-2 mb-3 p-2 bg-stationpedia-surface/30 rounded">
                <label className="flex items-center gap-2 text-sm text-gray-300">
                  <input
                    type="checkbox"
                    checked={device.generateToc || false}
                    onChange={(e) => onUpdateDevice({ generateToc: e.target.checked })}
                    className="rounded"
                  />
                  Generate Table of Contents
                </label>
                {device.generateToc && (
                  <label className="flex items-center gap-2 text-sm text-gray-300">
                    <input
                      type="checkbox"
                      checked={!device.tocFlat}
                      onChange={(e) => onUpdateDevice({ tocFlat: !e.target.checked })}
                      className="rounded"
                    />
                    Nested
                  </label>
                )}
                <button
                  onClick={generateTocIds}
                  className="px-2 py-1 text-xs bg-purple-600 hover:bg-purple-700 text-white rounded"
                  title="Auto-generate TOC IDs from section titles"
                >
                  🔗 Generate TOC IDs
                </button>
                {device.generateToc && (
                  <div
                    onClick={() => setSelection({ type: 'tocTitle' })}
                    className={`flex-1 px-2 py-1 rounded cursor-pointer text-sm ${
                      selection?.type === 'tocTitle'
                        ? 'bg-stationpedia-accent/30'
                        : 'hover:bg-stationpedia-accent/20'
                    }`}
                  >
                    Title: {device.tocTitle || 'Quick Navigation'}
                  </div>
                )}
              </div>

              {/* Operational Details Tree */}
              {(device.operationalDetails?.length || 0) === 0 ? (
                <div className="text-center text-gray-500 py-4 text-sm">
                  No operational details. Click "+ Add Section" to create one.
                </div>
              ) : (
                <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={handleDragEnd}>
                  <SortableContext items={ids} strategy={verticalListSortingStrategy}>
                    {device.operationalDetails!.map((detail, i) => (
                      <SortableDetailItem
                        key={`${i}-item`}
                        id={`${i}-item`}
                        detail={detail}
                        path={[i]}
                        depth={0}
                        selectedPath={
                          selection?.type === 'operationalDetail'
                            ? selection.path
                            : null
                        }
                        selectedField={
                          selection?.type === 'operationalDetail'
                            ? selection.field
                            : null
                        }
                        onSelect={selectOperationalDetail}
                        onAdd={addOperationalDetail}
                        onRemove={removeOperationalDetail}
                        onAddTable={addTableToSection}
                        onAddImage={addImageToSection}
                        onAddVideo={addVideoToSection}
                        onAddHeader={addInlineHeader}
                      />
                    ))}
                  </SortableContext>
                </DndContext>
              )}
            </div>
          )}
        </div>
        )}

        {/* Logic Descriptions Section */}
        {device.logicDescriptions && Object.keys(device.logicDescriptions).length > 0 && (
          <div className="mb-3">
            <SectionHeader
              title={`Logic Descriptions (${Object.keys(device.logicDescriptions).length})`}
              isExpanded={expandedSections.logic}
              onToggle={() => toggleSection('logic')}
            />
            {expandedSections.logic && (
              <div className="border border-t-0 border-stationpedia-border rounded-b p-2 space-y-1">
                {Object.entries(device.logicDescriptions).map(([key, value]) => (
                  <div
                    key={key}
                    onClick={() => setSelection({ type: 'logicDescription', key })}
                    className={`p-2 rounded cursor-pointer ${
                      selection?.type === 'logicDescription' && selection.key === key
                        ? 'bg-stationpedia-accent/30 border border-stationpedia-accent'
                        : 'bg-stationpedia-surface/50 hover:bg-stationpedia-accent/20'
                    }`}
                  >
                    <div className="text-sm font-medium text-stationpedia-accent">{key}</div>
                    <div className="text-xs text-gray-400">
                      {value.dataType} | {value.range}
                    </div>
                    <div className="text-sm text-gray-300 whitespace-pre-wrap mt-1">
                      {value.description || '(no description)'}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {/* Mode Descriptions Section */}
        {device.modeDescriptions && Object.keys(device.modeDescriptions).length > 0 && (
          <div className="mb-3">
            <SectionHeader
              title={`Mode Descriptions (${Object.keys(device.modeDescriptions).length})`}
              isExpanded={expandedSections.modes}
              onToggle={() => toggleSection('modes')}
            />
            {expandedSections.modes && (
              <div className="border border-t-0 border-stationpedia-border rounded-b p-2 space-y-1">
                {Object.entries(device.modeDescriptions).map(([key, value]) => (
                  <div
                    key={key}
                    onClick={() => setSelection({ type: 'modeDescription', key })}
                    className={`p-2 rounded cursor-pointer ${
                      selection?.type === 'modeDescription' && selection.key === key
                        ? 'bg-stationpedia-accent/30 border border-stationpedia-accent'
                        : 'bg-stationpedia-surface/50 hover:bg-stationpedia-accent/20'
                    }`}
                  >
                    <div className="text-sm font-medium text-stationpedia-accent">{key}</div>
                    <div className="text-sm text-gray-300 whitespace-pre-wrap">
                      {value.description || '(no description)'}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {/* Slot Descriptions Section */}
        {device.slotDescriptions && Object.keys(device.slotDescriptions).length > 0 && (
          <div className="mb-3">
            <SectionHeader
              title={`Slot Descriptions (${Object.keys(device.slotDescriptions).length})`}
              isExpanded={expandedSections.slots}
              onToggle={() => toggleSection('slots')}
            />
            {expandedSections.slots && (
              <div className="border border-t-0 border-stationpedia-border rounded-b p-2 space-y-1">
                {Object.entries(device.slotDescriptions).map(([key, value]) => (
                  <div
                    key={key}
                    onClick={() => setSelection({ type: 'slotDescription', key })}
                    className={`p-2 rounded cursor-pointer ${
                      selection?.type === 'slotDescription' && selection.key === key
                        ? 'bg-stationpedia-accent/30 border border-stationpedia-accent'
                        : 'bg-stationpedia-surface/50 hover:bg-stationpedia-accent/20'
                    }`}
                  >
                    <div className="text-sm font-medium text-stationpedia-accent">Slot {key}</div>
                    <div className="text-sm text-gray-300 whitespace-pre-wrap">
                      {value.description || '(no description)'}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {/* Connection Descriptions Section */}
        {device.connectionDescriptions && Object.keys(device.connectionDescriptions).length > 0 && (
          <div className="mb-3">
            <SectionHeader
              title={`Connection Descriptions (${Object.keys(device.connectionDescriptions).length})`}
              isExpanded={expandedSections.connections}
              onToggle={() => toggleSection('connections')}
            />
            {expandedSections.connections && (
              <div className="border border-t-0 border-stationpedia-border rounded-b p-2 space-y-1">
                {Object.entries(device.connectionDescriptions).map(([key, value]) => (
                  <div
                    key={key}
                    onClick={() => setSelection({ type: 'connectionDescription', key })}
                    className={`p-2 rounded cursor-pointer ${
                      selection?.type === 'connectionDescription' && selection.key === key
                        ? 'bg-stationpedia-accent/30 border border-stationpedia-accent'
                        : 'bg-stationpedia-surface/50 hover:bg-stationpedia-accent/20'
                    }`}
                  >
                    <div className="text-sm font-medium text-stationpedia-accent">{key}</div>
                    <div className="text-sm text-gray-300 whitespace-pre-wrap">
                      {value || '(no description)'}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}
      </div>

      {/* Link Modal */}
      <LinkModal
        workspace={workspace}
        isOpen={isLinkModalOpen}
        onClose={() => setIsLinkModalOpen(false)}
        onSelectLink={handleInsertLink}
      />

      {/* Image Picker Modal */}
      <ImagePickerModal
        isOpen={isImagePickerOpen}
        onClose={() => setIsImagePickerOpen(false)}
        onSelectImage={(filename) => {
          if (imagePickerTarget === 'section' && selection?.type === 'operationalDetail') {
            updateDetailProperty(selection.path, 'imageFile', filename);
          } else {
            onUpdateDevice({ pageImage: filename });
          }
        }}
        currentValue={
          imagePickerTarget === 'section' && selection?.type === 'operationalDetail'
            ? getDetailAtPath(device.operationalDetails!, selection.path)?.imageFile
            : (device as any)?.pageImage
        }
      />
    </div>
  );
};

export default DeviceSectionsEditor;
