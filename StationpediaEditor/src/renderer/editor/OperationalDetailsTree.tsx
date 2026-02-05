/**
 * OperationalDetailsTree with drag-and-drop reordering and inline editing
 * Renders operational details in a tree structure with drag handles and editable fields
 */
import React, { useState, useCallback } from 'react';
import {
  DndContext,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
} from '@dnd-kit/core';
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
  verticalListSortingStrategy,
  useSortable,
} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import type { OperationalDetail } from '@models/contentModel';

interface OperationalDetailsTreeProps {
  details: OperationalDetail[];
  onUpdate?: (index: number, detail: OperationalDetail) => void;
  onAdd?: (parentIndex?: number) => void;
  onRemove?: (index: number) => void;
  onReorder?: (fromIndex: number, toIndex: number) => void;
}

interface SortableItemProps {
  id: string;
  index: number;
  detail: OperationalDetail;
  depth: number;
  onAdd?: (parentIndex?: number) => void;
  onRemove?: (index: number) => void;
  onUpdate?: (index: number, detail: OperationalDetail) => void;
  editingIndex?: number | null;
  editingField?: 'title' | 'description' | null;
  onSetEditing?: (index: number | null, field: 'title' | 'description' | null) => void;
}

const SortableItem: React.FC<SortableItemProps> = ({
  id,
  index,
  detail,
  depth,
  onAdd,
  onRemove,
  onUpdate,
  editingIndex,
  editingField,
  onSetEditing,
}) => {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } =
    useSortable({ id });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1,
  };

  const isEditing = editingIndex === index;

  const handleTitleChange = (value: string) => {
    onUpdate?.(index, { ...detail, title: value });
  };

  const handleDescriptionChange = (value: string) => {
    onUpdate?.(index, { ...detail, description: value });
  };

  const handleFinishEditing = () => {
    onSetEditing?.(null, null);
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && editingField === 'title') {
      e.preventDefault();
      handleFinishEditing();
    }
    if (e.key === 'Escape') {
      e.preventDefault();
      handleFinishEditing();
    }
  };

  return (
    <div
      ref={setNodeRef}
      style={{ ...style, marginLeft: `${depth * 16}px` }}
      className={`mb-2 p-3 rounded border border-stationpedia-border bg-stationpedia-surface/50 ${
        isDragging ? 'shadow-lg' : ''
      }`}
    >
      <div className="flex items-center gap-2">
        {/* Drag Handle */}
        <div
          data-testid="drag-handle"
          {...attributes}
          {...listeners}
          className="flex-shrink-0 cursor-grab active:cursor-grabbing text-gray-500 hover:text-stationpedia-accent p-1"
          title="Drag to reorder"
        >
          <svg
            className="w-4 h-4"
            fill="currentColor"
            viewBox="0 0 20 20"
          >
            <path d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" />
          </svg>
        </div>

        {/* Content - Editable */}
        <div className="flex-1 min-w-0">
          {isEditing && editingField === 'title' ? (
            <input
              autoFocus
              value={detail.title}
              onChange={(e) => handleTitleChange(e.target.value)}
              onBlur={handleFinishEditing}
              onKeyDown={handleKeyDown}
              className="w-full px-2 py-1 bg-stationpedia-bg border border-stationpedia-border rounded text-sm text-white placeholder-gray-500 focus:outline-none focus:border-stationpedia-accent focus:ring-1 focus:ring-stationpedia-accent"
              placeholder="Enter title..."
            />
          ) : (
            <div
              onClick={() => onSetEditing?.(index, 'title')}
              className="font-semibold text-stationpedia-accent truncate cursor-text hover:underline"
              title="Click to edit title"
            >
              {detail.title}
            </div>
          )}

          {isEditing && editingField === 'description' ? (
            <textarea
              autoFocus
              value={detail.description || ''}
              onChange={(e) => handleDescriptionChange(e.target.value)}
              onBlur={handleFinishEditing}
              onKeyDown={handleKeyDown}
              className="w-full mt-1 px-2 py-1 bg-stationpedia-bg border border-stationpedia-border rounded text-sm text-white placeholder-gray-500 focus:outline-none focus:border-stationpedia-accent focus:ring-1 focus:ring-stationpedia-accent"
              placeholder="Enter description..."
              rows={3}
            />
          ) : detail.description ? (
            <div
              onClick={() => onSetEditing?.(index, 'description')}
              className="text-sm text-gray-400 truncate cursor-text hover:underline mt-1"
              title="Click to edit description"
            >
              {detail.description}
            </div>
          ) : (
            <div
              onClick={() => onSetEditing?.(index, 'description')}
              className="text-sm text-gray-500 italic cursor-text hover:underline mt-1"
              title="Click to add description"
            >
              (no description)
            </div>
          )}
        </div>

        {/* Actions */}
        <div className="flex-shrink-0 flex gap-1">
          <button
            onClick={() => onAdd?.(index)}
            className="p-1 text-gray-400 hover:text-green-400 hover:bg-green-400/10 rounded transition-colors"
            title="Add child detail"
            aria-label="add"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
            </svg>
          </button>
          <button
            onClick={() => onRemove?.(index)}
            className="p-1 text-gray-400 hover:text-red-400 hover:bg-red-400/10 rounded transition-colors"
            title="Remove detail"
            aria-label="remove"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
            </svg>
          </button>
        </div>
      </div>

      {/* Children */}
      {detail.children && detail.children.length > 0 && (
        <div className="mt-2">
          {detail.children.map((child, i) => (
            <SortableItem
              key={`${id}-child-${i}`}
              id={`${id}-child-${i}`}
              index={i}
              detail={child}
              depth={depth + 1}
              onAdd={onAdd}
              onRemove={onRemove}
              onUpdate={onUpdate}
              editingIndex={editingIndex}
              editingField={editingField}
              onSetEditing={onSetEditing}
            />
          ))}
        </div>
      )}
    </div>
  );
};

export const OperationalDetailsTree: React.FC<OperationalDetailsTreeProps> = ({
  details,
  onUpdate,
  onAdd,
  onRemove,
  onReorder,
}) => {
  const [editingIndex, setEditingIndex] = useState<number | null>(null);
  const [editingField, setEditingField] = useState<'title' | 'description' | null>(null);

  const sensors = useSensors(
    useSensor(PointerSensor, {
      distance: 8,
    } as any),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  const handleDragEnd = useCallback(
    (event: any) => {
      const { active, over } = event;

      if (over && active.id !== over.id) {
        const activeIndex = parseInt(active.id.split('-')[0]);
        const overIndex = parseInt(over.id.split('-')[0]);

        if (activeIndex !== overIndex && onReorder) {
          onReorder(activeIndex, overIndex);
        }
      }
    },
    [onReorder]
  );

  const ids = details.map((_, i) => `${i}-item`);

  const handleSetEditing = useCallback(
    (index: number | null, field: 'title' | 'description' | null) => {
      setEditingIndex(index);
      setEditingField(field);
    },
    []
  );

  return (
    <div className="flex flex-col h-full overflow-auto p-4 bg-stationpedia-bg">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-lg font-semibold text-stationpedia-accent">Operational Details</h3>
        <button
          onClick={() => onAdd?.()}
          className="px-2 py-1 bg-stationpedia-accent hover:bg-stationpedia-accent-hover text-white rounded text-sm transition-colors"
          title="Add top-level detail"
        >
          + Add Detail
        </button>
      </div>

      {details.length === 0 ? (
        <div className="text-center text-gray-500 py-8">No operational details</div>
      ) : (
        <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={handleDragEnd}>
          <SortableContext items={ids} strategy={verticalListSortingStrategy}>
            <div className="space-y-2">
              {details.map((detail, i) => (
                <SortableItem
                  key={`${i}-item`}
                  id={`${i}-item`}
                  index={i}
                  detail={detail}
                  depth={0}
                  onAdd={onAdd}
                  onRemove={onRemove}
                  onUpdate={onUpdate}
                  editingIndex={editingIndex}
                  editingField={editingField}
                  onSetEditing={handleSetEditing}
                />
              ))}
            </div>
          </SortableContext>
        </DndContext>
      )}
    </div>
  );
};
