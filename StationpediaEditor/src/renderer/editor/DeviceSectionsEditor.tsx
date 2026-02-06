/**
 * DeviceSectionsEditor - Unified editor for ALL device sections
 * All fields are inline editable with InlineEditableField components.
 * FloatingFormattingToolbar appears when a field with showToolbar=true is focused.
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
import { InlineEditableField } from './InlineEditableField';
import { FloatingFormattingToolbar } from './FloatingFormattingToolbar';
import { ActiveFieldProvider, useActiveField } from './ActiveFieldContext';
import { useFormattingActions } from './useFormattingActions';

interface DeviceSectionsEditorProps {
  device: DeviceDocument | null;
  onUpdateDevice: (updates: Record<string, unknown>) => void;
}

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

// Check if two paths are equal
const pathsEqual = (a: number[] | null, b: number[]): boolean => {
  if (!a) return false;
  if (a.length !== b.length) return false;
  return a.every((v, i) => v === b[i]);
};

// Sortable operational detail item with inline editing
interface SortableDetailItemProps {
  id: string;
  detail: OperationalDetail;
  path: number[];
  depth: number;
  activeSectionPath: number[] | null;
  onFieldChange: (path: number[], field: string, value: string) => void;
  onActivateSection: (path: number[]) => void;
  onAdd: (path: number[]) => void;
  onRemove: (path: number[]) => void;
  onAddTable?: (path: number[]) => void;
  onAddImage?: (path: number[]) => void;
  onAddVideo?: (path: number[]) => void;
  onAddHeader?: (path: number[]) => void;
  hideTitle?: boolean;
  hideDragHandle?: boolean;
}

const SortableDetailItem: React.FC<SortableDetailItemProps> = ({
  id,
  detail,
  path,
  depth,
  activeSectionPath,
  onFieldChange,
  onActivateSection,
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

  const isThisActive = pathsEqual(activeSectionPath, path);

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
      data-section-index={depth === 0 ? path[0] : undefined}
      style={{ ...style, marginLeft: `${depth * 16}px` }}
      className={`mb-2 rounded border ${
        isThisActive
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
        <div className="flex-1 min-w-0" onFocus={() => onActivateSection(path)}>
          {/* Title - inline editable (hidden when hideTitle=true) */}
          {!hideTitle && (
            <InlineEditableField
              value={detail.title || ''}
              onChange={(v) => onFieldChange(path, 'title', v)}
              placeholder="Section title..."
              rows={1}
              showToolbar={true}
            />
          )}

          {/* Description - inline editable */}
          <InlineEditableField
            value={detail.description || ''}
            onChange={(v) => onFieldChange(path, 'description', v)}
            placeholder="Description..."
            rows={3}
            showToolbar={true}
            className="mt-1"
          />

          {/* Bullets - only shown when explicitly added */}
          {Array.isArray(detail.items) && (
            <InlineEditableField
              value={detail.items.join('\n')}
              onChange={(v) => onFieldChange(path, 'items', v)}
              onRemove={() => onFieldChange(path, 'items_remove', '')}
              placeholder="Bullet points (one per line)..."
              rows={2}
              label="• Bullets"
              showToolbar={true}
              className="mt-1"
            />
          )}

          {/* Steps - only shown when explicitly added */}
          {Array.isArray(detail.steps) && (
            <InlineEditableField
              value={detail.steps.join('\n')}
              onChange={(v) => onFieldChange(path, 'steps', v)}
              onRemove={() => onFieldChange(path, 'steps_remove', '')}
              placeholder="Numbered steps (one per line)..."
              rows={2}
              label="1. Steps"
              showToolbar={true}
              className="mt-1"
            />
          )}

          {/* Metadata badges (read-only) */}
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
              Collapsible section
            </div>
          )}
          {detail.tocId && (
            <div className="mt-1 px-2 text-xs text-purple-400">
              TOC ID: {detail.tocId}
            </div>
          )}
          {detail.table && detail.table.length > 0 && (
            <div className="mt-1 px-2 text-xs text-green-400">
              Table: {detail.table.length} rows x {detail.table[0]?.cells?.length || 0} columns
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
                {!Array.isArray(detail.items) && (
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      onFieldChange(path, 'items_init', '');
                      setIsAddDropdownOpen(false);
                    }}
                    className="w-full px-3 py-2 text-left text-xs text-gray-200 hover:bg-stationpedia-accent/30 flex items-center gap-2"
                  >
                    • Bullets
                  </button>
                )}
                {!Array.isArray(detail.steps) && (
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      onFieldChange(path, 'steps_init', '');
                      setIsAddDropdownOpen(false);
                    }}
                    className="w-full px-3 py-2 text-left text-xs text-gray-200 hover:bg-stationpedia-accent/30 flex items-center gap-2"
                  >
                    1. Steps
                  </button>
                )}
                {onAddTable && (
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      onAddTable(path);
                      setIsAddDropdownOpen(false);
                    }}
                    className="w-full px-3 py-2 text-left text-xs text-gray-200 hover:bg-stationpedia-accent/30 flex items-center gap-2"
                  >
                    Table
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
                    Header
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
                  Subsection
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
                    Image
                  </button>
                )}
                {onAddVideo && (
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      onAddVideo(path);
                      setIsAddDropdownOpen(false);
                    }}
                    className="w-full px-3 py-2 text-left text-xs text-gray-200 hover:bg-stationpedia-accent/30 flex items-center gap-2"
                  >
                    Video
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
              activeSectionPath={activeSectionPath}
              onFieldChange={onFieldChange}
              onActivateSection={onActivateSection}
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
            Delete
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
                    Table
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
                    Header
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
                    Subsection
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
                    Image
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
                    Video
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
            Header
          </button>
          <button
            onClick={() => {
              onAddSection();
              setIsOpen(false);
            }}
            className="w-full px-3 py-2 text-left text-xs text-gray-200 hover:bg-stationpedia-accent/30 flex items-center gap-2"
          >
            Section
          </button>
        </div>
      )}
    </div>
  );
};

// Inner component that uses ActiveFieldContext
const DeviceSectionsEditorInner: React.FC<DeviceSectionsEditorProps> = ({
  device,
  onUpdateDevice,
}) => {
  const { workspace } = useEditorStore();
  const scrollToSectionIndex = useEditorStore((s) => s.scrollToSectionIndex);
  const scrollToSectionCounter = useEditorStore((s) => s.scrollToSectionCounter);
  const clearScrollToSection = useEditorStore((s) => s.clearScrollToSection);
  const [activeSectionPath, setActiveSectionPath] = useState<number[] | null>(null);
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

  // Active field context for link insertion
  const { textareaRef: activeTextareaRef, onValueChange: activeOnValueChange } = useActiveField();
  const formattingActions = useFormattingActions(activeTextareaRef, activeOnValueChange);

  // Toggle section expansion
  const toggleSection = (section: string) => {
    setExpandedSections(prev => ({ ...prev, [section]: !prev[section] }));
  };

  // Handle detail field change - updates device field at a given path
  const handleDetailFieldChange = useCallback((path: number[], field: string, value: string) => {
    if (!device) return;
    const newDetails = JSON.parse(JSON.stringify(device.operationalDetails || []));
    let detail = newDetails[path[0]];
    for (let i = 1; i < path.length; i++) {
      detail = detail.children[path[i]];
    }
    if (field === 'items') {
      detail.items = value.split('\n').filter((line: string) => line.trim());
    } else if (field === 'items_init') {
      detail.items = [];
    } else if (field === 'items_remove') {
      delete detail.items;
    } else if (field === 'steps') {
      detail.steps = value.split('\n').filter((line: string) => line.trim());
    } else if (field === 'steps_init') {
      detail.steps = [];
    } else if (field === 'steps_remove') {
      delete detail.steps;
    } else {
      detail[field] = value;
    }
    onUpdateDevice({ operationalDetails: newDetails });
  }, [device, onUpdateDevice]);

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
      collapsible: false
    };

    if (afterPath && afterPath.length > 0) {
      if (afterPath.length === 1) {
        newDetails.splice(afterPath[0] + 1, 0, inlineHeader);
      } else {
        let parent = newDetails[afterPath[0]];
        for (let i = 1; i < afterPath.length - 1; i++) {
          parent = parent.children[afterPath[i]];
        }
        parent.children = [...(parent.children || []), inlineHeader];
      }
    } else {
      newDetails.push(inlineHeader);
    }

    onUpdateDevice({ operationalDetails: newDetails });
  };

  // Add a table to a section
  const addTableToSection = (path: number[]) => {
    if (!device) return;

    const newDetails = JSON.parse(JSON.stringify(device.operationalDetails || []));
    let detail = newDetails[path[0]];
    for (let i = 1; i < path.length; i++) {
      detail = detail.children[path[i]];
    }

    detail.table = [
      { cells: ['Header 1', 'Header 2', 'Header 3'] },
      { cells: ['Cell 1', 'Cell 2', 'Cell 3'] },
      { cells: ['Cell 4', 'Cell 5', 'Cell 6'] },
    ];

    onUpdateDevice({ operationalDetails: newDetails });
    setActiveSectionPath(path);
  };

  // Add an image to a section (opens image picker)
  const addImageToSection = (path: number[]) => {
    if (!device) return;
    setActiveSectionPath(path);
    setImagePickerTarget('section');
    setIsImagePickerOpen(true);
  };

  // Add a video URL to a section
  const addVideoToSection = (path: number[]) => {
    if (!device) return;

    const newDetails = JSON.parse(JSON.stringify(device.operationalDetails || []));
    let detail = newDetails[path[0]];
    for (let i = 1; i < path.length; i++) {
      detail = detail.children[path[i]];
    }

    detail.youtubeUrl = '';
    detail.youtubeLabel = 'Watch on YouTube';

    onUpdateDevice({ operationalDetails: newDetails });
    setActiveSectionPath(path);
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
    setActiveSectionPath(null);
  };

  // Scroll to a specific section by index using DOM query
  const scrollToSection = useCallback((index: number) => {
    const el = document.querySelector(`[data-section-index="${index}"]`) as HTMLElement | null;
    if (el) {
      el.scrollIntoView({ behavior: 'smooth', block: 'start' });
      setActiveSectionPath([index]);
    }
  }, []);

  // Watch store for scroll-to-section requests from ContentTree
  useEffect(() => {
    if (scrollToSectionIndex !== null) {
      scrollToSection(scrollToSectionIndex);
      clearScrollToSection();
    }
  }, [scrollToSectionCounter]);

  // Scroll preview to matching section when editor section is activated
  const [previewTarget, setPreviewTarget] = useState<string | null>(null);

  // When activeSectionPath changes, update preview target to the root section index
  useEffect(() => {
    if (activeSectionPath && activeSectionPath.length > 0) {
      setPreviewTarget(`section-${activeSectionPath[0]}`);
    }
  }, [activeSectionPath]);

  // Scroll preview panel when previewTarget changes
  useEffect(() => {
    if (!previewTarget) return;
    // Small delay to let the preview re-render if content changed
    const timer = setTimeout(() => {
      const el = document.querySelector(`[data-preview-section="${previewTarget}"]`);
      if (el) {
        el.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
      }
    }, 50);
    return () => clearTimeout(timer);
  }, [previewTarget]);

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

  // DnD sensors
  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 8 } }),
    useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates })
  );

  // Handle drag end for reordering
  const handleDragEnd = useCallback((event: any) => {
    const { active, over } = event;
    if (!over || active.id === over.id || !device?.operationalDetails) return;

    const activeIndex = parseInt(active.id.split('-')[0]);
    const overIndex = parseInt(over.id.split('-')[0]);

    if (activeIndex !== overIndex) {
      const newDetails = [...device.operationalDetails];
      const [removed] = newDetails.splice(activeIndex, 1);
      newDetails.splice(overIndex, 0, removed);
      onUpdateDevice({ operationalDetails: newDetails });
    }
  }, [device, onUpdateDevice]);

  // Handle link insertion from modal using active field context
  const handleInsertLink = useCallback((linkText: string, displayText: string) => {
    formattingActions.insertAtCursor(linkText, '');
    setIsLinkModalOpen(false);
  }, [formattingActions]);

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
            {device.displayName || (device as any).guideKey || device.deviceKey}
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
            Markdown guides are for reference only. Create JSON guides in descriptions.json to edit them.
          </p>
        </div>
      </div>
    );
  }

  // Check if this is a JSON guide (has guideKey instead of deviceKey)
  const isJsonGuide = !!(device as any).guideKey;
  const itemKey = isJsonGuide ? (device as any).guideKey : device.deviceKey;
  const ids = (device.operationalDetails || []).map((_, i) => `${i}-item`);

  // Navigation from store
  const canGoBack = useEditorStore((s) => s.canGoBack());
  const canGoForward = useEditorStore((s) => s.canGoForward());
  const goBack = useEditorStore((s) => s.goBack);
  const goForward = useEditorStore((s) => s.goForward);

  // Collapse all / Expand all
  const collapseAll = useCallback(() => {
    const collapsed: Record<string, boolean> = {};
    // Collapse all top-level groups
    for (const key of Object.keys(expandedSections)) {
      collapsed[key] = false;
    }
    // Collapse all per-section keys
    (device?.operationalDetails || []).forEach((_, i) => {
      collapsed[`section_${i}`] = false;
    });
    setExpandedSections(collapsed);
  }, [expandedSections, device?.operationalDetails]);

  const expandAll = useCallback(() => {
    const expanded: Record<string, boolean> = {};
    for (const key of Object.keys(expandedSections)) {
      expanded[key] = true;
    }
    (device?.operationalDetails || []).forEach((_, i) => {
      expanded[`section_${i}`] = true;
    });
    setExpandedSections(expanded);
  }, [expandedSections, device?.operationalDetails]);

  // Get active detail for section properties panel
  const activeDetail = activeSectionPath && device.operationalDetails
    ? getDetailAtPath(device.operationalDetails, activeSectionPath)
    : null;

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
        <div className="flex items-center gap-2">
          <button
            onClick={collapseAll}
            className="px-2 py-1 text-xs text-gray-400 hover:text-white hover:bg-gray-700 rounded transition-colors"
            title="Collapse all sections"
          >
            Collapse All
          </button>
          <button
            onClick={expandAll}
            className="px-2 py-1 text-xs text-gray-400 hover:text-white hover:bg-gray-700 rounded transition-colors"
            title="Expand all sections"
          >
            Expand All
          </button>
          <span className="text-xs text-gray-400 truncate max-w-[200px]" title={device?.displayName || undefined}>
            {device?.displayName || 'No device selected'}
          </span>
        </div>
      </div>

      {/* Section Properties Panel (conditional - shown when a section is active) */}
      {activeSectionPath && activeDetail && (
        <div className="section-properties-panel border-b border-stationpedia-border p-3 flex-shrink-0">
          <div className="flex items-center justify-between mb-2">
            <div className="section-properties-header">Section Properties</div>
            <button
              onClick={() => setActiveSectionPath(null)}
              className="text-xs text-gray-400 hover:text-white"
            >
              Clear
            </button>
          </div>
          <div className="section-properties-grid">
            {/* Collapsible checkbox */}
            <label className="section-property-checkbox-label">
              <input
                type="checkbox"
                checked={activeDetail.collapsible || false}
                onChange={(e) => updateDetailProperty(activeSectionPath, 'collapsible', e.target.checked)}
                className="section-property-checkbox"
              />
              <span className="text-lg">📁</span> Collapsible
            </label>

            {/* TOC ID input */}
            <div className="section-property-row">
              <span className="section-property-label"><span className="text-lg">🔗</span> TOC ID:</span>
              <input
                type="text"
                value={activeDetail.tocId || ''}
                onChange={(e) => updateDetailProperty(activeSectionPath, 'tocId', e.target.value)}
                placeholder="optional"
                className="section-property-input"
              />
            </div>

            {/* Image file input with browse button */}
            <div className="section-property-row section-property-full-width">
              <span className="section-property-label"><span className="text-lg">🖼️</span> Image:</span>
              <div className="flex gap-1 flex-1">
                <input
                  type="text"
                  value={activeDetail.imageFile || ''}
                  onChange={(e) => updateDetailProperty(activeSectionPath, 'imageFile', e.target.value)}
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
                  Browse
                </button>
                {activeDetail.imageFile && (
                  <button
                    onClick={() => updateDetailProperty(activeSectionPath, 'imageFile', '')}
                    className="px-2 py-0.5 bg-red-600 hover:bg-red-700 text-white rounded text-xs flex-shrink-0"
                    title="Remove image"
                  >
                    X
                  </button>
                )}
              </div>
            </div>

            {/* YouTube URL input */}
            <div className="section-property-row section-property-full-width">
              <span className="section-property-label"><span className="text-lg">📺</span> YouTube:</span>
              <input
                type="text"
                value={activeDetail.youtubeUrl || ''}
                onChange={(e) => updateDetailProperty(activeSectionPath, 'youtubeUrl', e.target.value)}
                placeholder="Video ID"
                className="section-property-input"
              />
            </div>

            {/* YouTube Label input */}
            {activeDetail.youtubeUrl && (
              <div className="section-property-row section-property-full-width">
                <span className="section-property-label">Label:</span>
                <input
                  type="text"
                  value={activeDetail.youtubeLabel || ''}
                  onChange={(e) => updateDetailProperty(activeSectionPath, 'youtubeLabel', e.target.value)}
                  placeholder="Video title"
                  className="section-property-input"
                />
              </div>
            )}

            {/* Table editor */}
            <div className="section-property-row section-property-full-width col-span-2">
              <div className="flex items-center justify-between w-full mb-2">
                <span className="section-property-label"><span className="text-lg">📊</span> Table:</span>
                {!activeDetail.table || activeDetail.table.length === 0 ? (
                  <button
                    onClick={() => {
                      const defaultTable: TableRow[] = [
                        { cells: ['Header 1', 'Header 2', 'Header 3'] },
                        { cells: ['Cell 1', 'Cell 2', 'Cell 3'] },
                        { cells: ['Cell 4', 'Cell 5', 'Cell 6'] },
                      ];
                      updateDetailProperty(activeSectionPath, 'table', defaultTable);
                    }}
                    className="px-2 py-1 text-xs bg-green-600 hover:bg-green-700 text-white rounded"
                  >
                    + Add Table
                  </button>
                ) : (
                  <button
                    onClick={() => {
                      if (confirm('Delete this table?')) {
                        updateDetailProperty(activeSectionPath, 'table', null);
                      }
                    }}
                    className="px-2 py-1 text-xs bg-red-600 hover:bg-red-700 text-white rounded"
                  >
                    Remove Table
                  </button>
                )}
              </div>

              {/* Table editor grid */}
              {activeDetail.table && activeDetail.table.length > 0 && (
                <div className="w-full space-y-2 mt-2">
                  {/* Table controls */}
                  <div className="flex gap-2 text-xs">
                    <button
                      onClick={() => {
                        const newTable = [...activeDetail.table!];
                        const colCount = newTable[0]?.cells?.length || 3;
                        newTable.push({ cells: Array(colCount).fill('') });
                        updateDetailProperty(activeSectionPath, 'table', newTable);
                      }}
                      className="px-2 py-1 bg-blue-600 hover:bg-blue-700 text-white rounded"
                    >
                      + Row
                    </button>
                    <button
                      onClick={() => {
                        const newTable = activeDetail.table!.map(row => ({
                          cells: [...(row.cells || []), '']
                        }));
                        updateDetailProperty(activeSectionPath, 'table', newTable);
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
                        {activeDetail.table.map((row, rowIndex) => (
                          <tr key={rowIndex} className={rowIndex === 0 ? 'bg-orange-900/30' : ''}>
                            {row.cells?.map((cell, colIndex) => (
                              <td key={colIndex} className="border border-stationpedia-border p-1">
                                <input
                                  type="text"
                                  value={cell}
                                  onChange={(e) => {
                                    const newTable = JSON.parse(JSON.stringify(activeDetail.table));
                                    newTable[rowIndex].cells[colIndex] = e.target.value;
                                    updateDetailProperty(activeSectionPath, 'table', newTable);
                                  }}
                                  className={`w-full min-w-[80px] px-2 py-1 bg-stationpedia-surface border border-stationpedia-border/50 rounded text-white text-center ${rowIndex === 0 ? 'font-bold text-orange-400' : ''}`}
                                  placeholder={rowIndex === 0 ? 'Header' : 'Cell'}
                                />
                              </td>
                            ))}
                            {/* Row delete button */}
                            <td className="border border-stationpedia-border p-1 w-6">
                              {activeDetail.table!.length > 1 && (
                                <button
                                  onClick={() => {
                                    const newTable = activeDetail.table!.filter((_, i) => i !== rowIndex);
                                    updateDetailProperty(activeSectionPath, 'table', newTable);
                                  }}
                                  className="text-red-400 hover:text-red-300"
                                  title="Delete row"
                                >
                                  X
                                </button>
                              )}
                            </td>
                          </tr>
                        ))}
                        {/* Column delete buttons row */}
                        {activeDetail.table[0]?.cells && activeDetail.table[0].cells.length > 1 && (
                          <tr>
                            {activeDetail.table[0].cells.map((_, colIndex) => (
                              <td key={colIndex} className="p-1 text-center">
                                <button
                                  onClick={() => {
                                    const newTable = activeDetail.table!.map(row => ({
                                      cells: row.cells?.filter((_, i) => i !== colIndex) || []
                                    }));
                                    updateDetailProperty(activeSectionPath, 'table', newTable);
                                  }}
                                  className="text-red-400 hover:text-red-300 text-xs"
                                  title="Delete column"
                                >
                                  X
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
      )}

      {/* Floating Formatting Toolbar */}
      <FloatingFormattingToolbar onOpenLinkModal={() => setIsLinkModalOpen(true)} />

      {/* Scrollable content */}
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
              {/* Display Name - inline editable */}
              <InlineEditableField
                value={device.displayName || ''}
                onChange={(v) => onUpdateDevice({ displayName: v })}
                placeholder="Display name..."
                rows={1}
                label="Display Name"
                showToolbar={false}
                mono={false}
              />

              {/* Key (read-only) */}
              <div className="p-2 rounded bg-stationpedia-surface/30 border border-stationpedia-border/50">
                <div className="text-xs text-gray-400 mb-1">{isJsonGuide ? 'Guide Key' : 'Device Key'} (read-only)</div>
                <div className="text-xs text-gray-500 font-mono">
                  {itemKey}
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Page Description Section */}
        <div className="mb-3" onFocus={() => setPreviewTarget('description')}>
          <SectionHeader
            title="Page Description"
            isExpanded={expandedSections.description}
            onToggle={() => toggleSection('description')}
          />
          {expandedSections.description && (
            <div className="border border-t-0 border-stationpedia-border rounded-b p-2 space-y-2">
              {/* Prepend */}
              <InlineEditableField
                value={device.pageDescriptionPrepend || ''}
                onChange={(v) => onUpdateDevice({ pageDescriptionPrepend: v })}
                placeholder="Prepend text (shows before main description)..."
                rows={3}
                label="Prepend (shows before main description)"
                showToolbar={true}
              />

              {/* Main Description */}
              <InlineEditableField
                value={device.pageDescription || ''}
                onChange={(v) => onUpdateDevice({ pageDescription: v })}
                placeholder="Main description (uses game default if empty)..."
                rows={3}
                label="Main Description"
                showToolbar={true}
              />

              {/* Append */}
              <InlineEditableField
                value={device.pageDescriptionAppend || ''}
                onChange={(v) => onUpdateDevice({ pageDescriptionAppend: v })}
                placeholder="Append text (shows after main description)..."
                rows={3}
                label="Append (shows after main description)"
                showToolbar={true}
              />
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
                title="Page Image"
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
                      Browse
                    </button>
                    {(device as any).pageImage && (
                      <button
                        onClick={() => onUpdateDevice({ pageImage: '' })}
                        className="px-2 py-1 bg-red-600 hover:bg-red-700 text-white rounded text-sm"
                        title="Remove image"
                      >
                        X
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
                <span className="text-sm font-semibold text-gray-300">Guide Sections</span>
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
                  Generate TOC IDs
                </button>
                {device.generateToc && (
                  <InlineEditableField
                    value={device.tocTitle || ''}
                    onChange={(v) => onUpdateDevice({ tocTitle: v })}
                    placeholder="Contents"
                    rows={1}
                    label="TOC Title"
                    showToolbar={false}
                    mono={false}
                    className="flex-1"
                  />
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
                            onTitleClick={() => setActiveSectionPath([i])}
                            onAddTable={() => addTableToSection([i])}
                            onAddHeader={() => addInlineHeader([i])}
                            onAddSubsection={() => addOperationalDetail([i])}
                            onAddImage={() => addImageToSection([i])}
                            onAddVideo={() => addVideoToSection([i])}
                            showDelete={true}
                            onDelete={() => removeOperationalDetail([i])}
                            isSelected={pathsEqual(activeSectionPath, [i])}
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
                                activeSectionPath={activeSectionPath}
                                onFieldChange={handleDetailFieldChange}
                                onActivateSection={setActiveSectionPath}
                                onAdd={addOperationalDetail}
                                onRemove={removeOperationalDetail}
                                onAddTable={addTableToSection}
                                onAddImage={addImageToSection}
                                onAddVideo={addVideoToSection}
                                onAddHeader={addInlineHeader}
                                hideTitle={true}
                                hideDragHandle={true}
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
                const newDetails = JSON.parse(JSON.stringify(device.operationalDetails || []));
                const newDetail: OperationalDetail = { title: 'New Image Section', description: '' };
                newDetails.push(newDetail);
                onUpdateDevice({ operationalDetails: newDetails });
                setTimeout(() => {
                  setActiveSectionPath([newDetails.length - 1]);
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
                setTimeout(() => {
                  setActiveSectionPath([newDetails.length - 1]);
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
                  Generate TOC IDs
                </button>
                {device.generateToc && (
                  <InlineEditableField
                    value={device.tocTitle || ''}
                    onChange={(v) => onUpdateDevice({ tocTitle: v })}
                    placeholder="Quick Navigation"
                    rows={1}
                    label="TOC Title"
                    showToolbar={false}
                    mono={false}
                    className="flex-1"
                  />
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
                          activeSectionPath={activeSectionPath}
                          onFieldChange={handleDetailFieldChange}
                          onActivateSection={setActiveSectionPath}
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
          <div className="mb-3" onFocus={() => setPreviewTarget('logic')}>
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
                    className="p-2 rounded bg-stationpedia-surface/50"
                  >
                    <div className="text-sm font-medium text-stationpedia-accent">{key}</div>
                    <div className="text-xs text-gray-400">
                      {value.dataType} | {value.range}
                    </div>
                    <InlineEditableField
                      value={value.description || ''}
                      onChange={(v) => {
                        const newLogic = { ...device.logicDescriptions };
                        if (!newLogic[key]) {
                          newLogic[key] = { dataType: '', range: '', description: '' };
                        }
                        newLogic[key].description = v;
                        onUpdateDevice({ logicDescriptions: newLogic });
                      }}
                      placeholder="Logic description..."
                      rows={2}
                      showToolbar={true}
                      className="mt-1"
                    />
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {/* Mode Descriptions Section */}
        {device.modeDescriptions && Object.keys(device.modeDescriptions).length > 0 && (
          <div className="mb-3" onFocus={() => setPreviewTarget('modes')}>
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
                    className="p-2 rounded bg-stationpedia-surface/50"
                  >
                    <div className="text-sm font-medium text-stationpedia-accent">{key}</div>
                    <InlineEditableField
                      value={value.description || ''}
                      onChange={(v) => {
                        const newModes = { ...device.modeDescriptions };
                        if (!newModes[key]) {
                          newModes[key] = { description: '' };
                        }
                        newModes[key].description = v;
                        onUpdateDevice({ modeDescriptions: newModes });
                      }}
                      placeholder="Mode description..."
                      rows={2}
                      showToolbar={true}
                      className="mt-1"
                    />
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {/* Slot Descriptions Section */}
        {device.slotDescriptions && Object.keys(device.slotDescriptions).length > 0 && (
          <div className="mb-3" onFocus={() => setPreviewTarget('slots')}>
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
                    className="p-2 rounded bg-stationpedia-surface/50"
                  >
                    <div className="text-sm font-medium text-stationpedia-accent">Slot {key}</div>
                    <InlineEditableField
                      value={value.description || ''}
                      onChange={(v) => {
                        const newSlots = { ...device.slotDescriptions };
                        if (!newSlots[key]) {
                          newSlots[key] = { description: '' };
                        }
                        newSlots[key].description = v;
                        onUpdateDevice({ slotDescriptions: newSlots });
                      }}
                      placeholder="Slot description..."
                      rows={2}
                      showToolbar={true}
                      className="mt-1"
                    />
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {/* Connection Descriptions Section */}
        {device.connectionDescriptions && Object.keys(device.connectionDescriptions).length > 0 && (
          <div className="mb-3" onFocus={() => setPreviewTarget('connections')}>
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
                    className="p-2 rounded bg-stationpedia-surface/50"
                  >
                    <div className="text-sm font-medium text-stationpedia-accent">{key}</div>
                    <InlineEditableField
                      value={value || ''}
                      onChange={(v) => {
                        const newConnections = { ...device.connectionDescriptions };
                        newConnections[key] = v;
                        onUpdateDevice({ connectionDescriptions: newConnections });
                      }}
                      placeholder="Connection description..."
                      rows={2}
                      showToolbar={true}
                      className="mt-1"
                    />
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
          if (imagePickerTarget === 'section' && activeSectionPath) {
            updateDetailProperty(activeSectionPath, 'imageFile', filename);
          } else {
            onUpdateDevice({ pageImage: filename });
          }
        }}
        currentValue={
          imagePickerTarget === 'section' && activeSectionPath
            ? getDetailAtPath(device.operationalDetails!, activeSectionPath)?.imageFile
            : (device as any)?.pageImage
        }
      />
    </div>
  );
};

// Outer component that wraps in ActiveFieldProvider
export const DeviceSectionsEditor: React.FC<DeviceSectionsEditorProps> = (props) => {
  return (
    <ActiveFieldProvider>
      <DeviceSectionsEditorInner {...props} />
    </ActiveFieldProvider>
  );
};

export default DeviceSectionsEditor;
