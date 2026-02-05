/**
 * TooltipEditor - Main tooltip editing panel
 * Shows list of tooltips, search, category filter, add/delete/edit operations
 */
import React, { useCallback, useState, useEffect } from 'react';
import { useTooltipStore } from '@renderer/store/tooltipStore';
import type { TooltipDefinition } from '@models/tooltipModel';
import { getSortedCategories } from '@models/tooltipModel';

interface TooltipEditorProps {
  onSelectTooltip?: (tooltip: TooltipDefinition | null) => void;
  onUpdateTooltip?: (tooltip: TooltipDefinition) => void;
}

export const TooltipEditor: React.FC<TooltipEditorProps> = ({
  onSelectTooltip,
  onUpdateTooltip,
}) => {
  const {
    collection,
    selectedTooltipKey,
    selectedTooltip,
    searchQuery,
    selectedCategory,
    filteredTooltips,
    selectTooltip,
    setSearchQuery,
    setSelectedCategory,
    addNewTooltip,
    updateTooltipContent,
    deleteTooltip,
  } = useTooltipStore();

  const [showAddDialog, setShowAddDialog] = useState(false);
  const [newKey, setNewKey] = useState('');
  const [newDescription, setNewDescription] = useState('');
  const [editingKey, setEditingKey] = useState<string | null>(null);
  const [editingDescription, setEditingDescription] = useState('');

  const categories = getSortedCategories(collection);

  // When editing key changes, load the description
  useEffect(() => {
    if (editingKey) {
      const tooltip = collection.tooltips.get(editingKey);
      setEditingDescription(tooltip?.description || '');
    }
  }, [editingKey, collection.tooltips]);

  const handleSelectTooltip = useCallback(
    (tooltip: TooltipDefinition) => {
      selectTooltip(tooltip.key);
      onSelectTooltip?.(tooltip);
      // Start editing when clicked
      setEditingKey(tooltip.key);
      setEditingDescription(tooltip.description);
    },
    [selectTooltip, onSelectTooltip]
  );

  const handleAddNew = useCallback(() => {
    if (newKey.trim()) {
      addNewTooltip(newKey, newDescription);
      const added = collection.tooltips.get(newKey);
      if (added) {
        handleSelectTooltip(added);
        onUpdateTooltip?.(added);
      }
      setNewKey('');
      setNewDescription('');
      setShowAddDialog(false);
    }
  }, [newKey, newDescription, addNewTooltip, collection.tooltips, handleSelectTooltip, onUpdateTooltip]);

  const handleDelete = useCallback(
    (key: string) => {
      if (confirm(`Delete tooltip "${key}"?`)) {
        deleteTooltip(key);
        selectTooltip(null);
        onSelectTooltip?.(null);
        setEditingKey(null);
      }
    },
    [deleteTooltip, selectTooltip, onSelectTooltip]
  );

  const handleSaveEdit = useCallback(() => {
    if (editingKey && editingDescription !== undefined) {
      updateTooltipContent(editingKey, editingDescription);
      const updated = collection.tooltips.get(editingKey);
      if (updated) {
        onUpdateTooltip?.({ ...updated, description: editingDescription });
      }
    }
  }, [editingKey, editingDescription, updateTooltipContent, collection.tooltips, onUpdateTooltip]);

  const handleCancelEdit = useCallback(() => {
    setEditingKey(null);
    setEditingDescription('');
  }, []);

  return (
    <div className="flex flex-col h-full bg-gray-950">
      {/* Header */}
      <div className="border-b border-gray-700 p-4 bg-gray-900">
        <h2 className="text-lg font-bold text-white mb-4">Global Tooltips</h2>

        {/* Search bar */}
        <div className="mb-4">
          <input
            type="text"
            placeholder="Search tooltips..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full px-3 py-2 rounded bg-gray-800 border border-gray-700 text-white placeholder-gray-500 text-sm"
          />
        </div>

        {/* Category filter */}
        {categories.length > 0 && (
          <div className="mb-4">
            <div className="text-xs uppercase tracking-wide text-gray-500 mb-2">Category</div>
            <div className="flex flex-wrap gap-2">
              <button
                onClick={() => setSelectedCategory(null)}
                className={`px-2 py-1 rounded text-xs font-medium transition-colors ${
                  selectedCategory === null
                    ? 'bg-cyan-600 text-white'
                    : 'bg-gray-800 text-gray-300 hover:bg-gray-700'
                }`}
              >
                All
              </button>
              {categories.map((cat) => (
                <button
                  key={cat}
                  onClick={() => setSelectedCategory(cat)}
                  className={`px-2 py-1 rounded text-xs font-medium transition-colors ${
                    selectedCategory === cat
                      ? 'bg-cyan-600 text-white'
                      : 'bg-gray-800 text-gray-300 hover:bg-gray-700'
                  }`}
                >
                  {cat}
                </button>
              ))}
            </div>
          </div>
        )}

        {/* Add button */}
        <button
          onClick={() => setShowAddDialog(true)}
          className="w-full px-3 py-2 rounded text-sm font-medium bg-cyan-600 text-white hover:bg-cyan-500 transition-colors"
        >
          + Add Tooltip
        </button>
      </div>

      {/* Add dialog */}
      {showAddDialog && (
        <div className="border-b border-gray-700 p-4 bg-gray-900">
          <div className="space-y-3">
            <div>
              <label className="text-xs uppercase tracking-wide text-gray-500 mb-1 block">
                Key
              </label>
              <input
                type="text"
                placeholder="e.g., ItemBattery"
                value={newKey}
                onChange={(e) => setNewKey(e.target.value)}
                className="w-full px-3 py-2 rounded bg-gray-800 border border-gray-700 text-white placeholder-gray-500 text-sm"
                autoFocus
              />
            </div>
            <div>
              <label className="text-xs uppercase tracking-wide text-gray-500 mb-1 block">
                Description
              </label>
              <textarea
                placeholder="Enter tooltip description..."
                value={newDescription}
                onChange={(e) => setNewDescription(e.target.value)}
                className="w-full px-3 py-2 rounded bg-gray-800 border border-gray-700 text-white placeholder-gray-500 text-sm"
                rows={3}
              />
            </div>
            <div className="flex gap-2 justify-end">
              <button
                onClick={() => {
                  setShowAddDialog(false);
                  setNewKey('');
                  setNewDescription('');
                }}
                className="px-3 py-2 rounded text-sm font-medium bg-gray-800 text-gray-300 hover:bg-gray-700 transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={handleAddNew}
                disabled={!newKey.trim()}
                className="px-3 py-2 rounded text-sm font-medium bg-cyan-600 text-white hover:bg-cyan-500 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Add
              </button>
            </div>
          </div>
        </div>
      )}

      {/* List of tooltips */}
      <div className="flex-1 overflow-auto">
        {filteredTooltips.length === 0 ? (
          <div className="p-4 text-center text-gray-500">
            <p>No tooltips found</p>
          </div>
        ) : (
          <div className="divide-y divide-gray-700">
            {filteredTooltips.map((tooltip) => (
              <div
                key={tooltip.key}
                className={`transition-colors ${
                  selectedTooltipKey === tooltip.key
                    ? 'bg-cyan-900 border-l-2 border-cyan-600'
                    : 'bg-gray-900 hover:bg-gray-800 border-l-2 border-transparent'
                }`}
              >
                {/* Edit mode for this tooltip */}
                {editingKey === tooltip.key ? (
                  <div className="p-3">
                    <div className="font-mono text-sm text-cyan-400 mb-2">{tooltip.key}</div>
                    {tooltip.category && (
                      <div className="text-xs text-gray-500 mb-2">{tooltip.category}</div>
                    )}
                    <textarea
                      value={editingDescription}
                      onChange={(e) => setEditingDescription(e.target.value)}
                      className="w-full px-3 py-2 rounded bg-gray-800 border border-cyan-600 text-white text-sm focus:outline-none focus:ring-2 focus:ring-cyan-500"
                      rows={4}
                      autoFocus
                      placeholder="Enter tooltip description..."
                    />
                    <div className="flex gap-2 mt-2 justify-end">
                      <button
                        onClick={handleCancelEdit}
                        className="px-3 py-1 rounded text-xs font-medium bg-gray-700 text-gray-300 hover:bg-gray-600 transition-colors"
                      >
                        Cancel
                      </button>
                      <button
                        onClick={handleSaveEdit}
                        className="px-3 py-1 rounded text-xs font-medium bg-cyan-600 text-white hover:bg-cyan-500 transition-colors"
                      >
                        Save
                      </button>
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          handleDelete(tooltip.key);
                        }}
                        className="px-3 py-1 rounded text-xs font-medium bg-red-900 text-red-200 hover:bg-red-800 transition-colors"
                      >
                        Delete
                      </button>
                    </div>
                  </div>
                ) : (
                  <div
                    onClick={() => handleSelectTooltip(tooltip)}
                    className="p-3 cursor-pointer"
                  >
                    <div className="flex items-start justify-between gap-2">
                      <div className="flex-1 min-w-0">
                        <div className="font-mono text-sm text-white truncate">{tooltip.key}</div>
                        {tooltip.category && (
                          <div className="text-xs text-gray-500 mt-1">{tooltip.category}</div>
                        )}
                        <div className="text-xs text-gray-400 mt-1 line-clamp-2">
                          {tooltip.description || '(click to edit)'}
                        </div>
                      </div>
                      <span className="text-xs text-gray-500 flex-shrink-0">✏️ Edit</span>
                    </div>
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Footer info */}
      <div className="border-t border-gray-700 p-3 bg-gray-900 text-xs text-gray-500">
        {filteredTooltips.length} of {collection.tooltips.size} tooltips
      </div>
    </div>
  );
};
