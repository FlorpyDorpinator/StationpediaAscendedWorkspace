/**
 * Toolbar - Application toolbar with File menu and mode toggle
 */
import React, { useCallback } from 'react';

interface ToolbarProps {
  isDirty: boolean;
  workspacePath?: string | null;
  onOpenWorkspace?: () => void;
  onImportGuide?: () => void;
  onSave?: () => void;
  onClose?: () => void;
  isLoading?: boolean;
  onToggleTooltips?: () => void;
  showTooltips?: boolean;
  onToggleShortcuts?: () => void;
  showShortcuts?: boolean;
  onOpenSimulator?: () => void;
  onResetLayout?: () => void;
}

export const Toolbar: React.FC<ToolbarProps> = ({
  isDirty,
  workspacePath,
  onOpenWorkspace,
  onImportGuide,
  onSave,
  onClose,
  isLoading = false,
  onToggleTooltips,
  showTooltips = false,
  onToggleShortcuts,
  showShortcuts = false,
  onOpenSimulator,
  onResetLayout,
}) => {
  return (
    <div className="bg-stationpedia-surface border-b border-stationpedia-border px-4 py-3 flex items-center justify-between text-sm">
      {/* Left: Title and Path */}
      <div className="flex items-center gap-4">
        <h1 className="font-bold text-stationpedia-accent text-lg">
          Stationpedia Editor
        </h1>
        {workspacePath && (
          <div className="text-gray-400 truncate">
            {workspacePath.split(/[/\\]/).pop()}
          </div>
        )}
        {isDirty && (
          <div className="text-yellow-400 font-semibold text-xs px-2 py-1 bg-yellow-900/30 rounded">
            ● Unsaved
          </div>
        )}
      </div>

      {/* Right: Actions */}
      <div className="flex items-center gap-3">
        {isLoading && (
          <div className="text-stationpedia-accent text-xs animate-pulse">Loading...</div>
        )}

        <button
          onClick={onOpenSimulator}
          disabled={!workspacePath}
          className={`px-3 py-1.5 rounded text-xs font-medium transition-colors ${
            workspacePath
              ? 'bg-purple-600 hover:bg-purple-700 text-white'
              : 'bg-gray-800 text-gray-500 cursor-not-allowed'
          }`}
          title="Open simulator preview window"
        >
          🎮 Simulator
        </button>

        <button
          onClick={onToggleTooltips}
          className={`px-3 py-1.5 rounded text-xs font-medium transition-colors ${
            showTooltips
              ? 'bg-stationpedia-accent hover:bg-stationpedia-accent-hover text-white'
              : 'bg-stationpedia-surface hover:bg-gray-700 text-gray-300'
          }`}
          title="Toggle global tooltips panel"
        >
          💬 Global Tooltips
        </button>

        <button
          onClick={onToggleShortcuts}
          className={`px-3 py-1.5 rounded text-xs font-medium transition-colors ${
            showShortcuts
              ? 'bg-stationpedia-accent hover:bg-stationpedia-accent-hover text-white'
              : 'bg-stationpedia-surface hover:bg-gray-700 text-gray-300'
          }`}
          title="Show keyboard shortcuts"
        >
          ⌨️ Shortcuts
        </button>

        <button
          onClick={onResetLayout}
          className="px-3 py-1.5 rounded bg-stationpedia-surface hover:bg-gray-700 text-gray-300 hover:text-white transition-colors text-xs font-medium"
          title="Reset panel layout to default"
        >
          🔲 Reset Layout
        </button>

        <button
          onClick={onOpenWorkspace}
          className="px-3 py-1.5 rounded bg-stationpedia-surface hover:bg-gray-700 text-gray-300 hover:text-white transition-colors text-xs font-medium"
          title="Open a workspace (Ctrl+O)"
        >
          📁 Open
        </button>

        <button
          onClick={onImportGuide}
          disabled={!workspacePath}
          className={`px-3 py-1.5 rounded text-xs font-medium transition-colors ${
            workspacePath
              ? 'bg-green-600 hover:bg-green-700 text-white'
              : 'bg-gray-800 text-gray-500 cursor-not-allowed'
          }`}
          title="Import a guide from a JSON file"
        >
          📥 Import JSON
        </button>

        <button
          onClick={onSave}
          disabled={!isDirty || !workspacePath || isLoading}
          className={`px-3 py-1.5 rounded text-xs font-medium transition-colors ${
            isDirty && workspacePath && !isLoading
              ? 'bg-stationpedia-accent hover:bg-stationpedia-accent-hover text-white'
              : 'bg-stationpedia-surface text-gray-500 cursor-not-allowed'
          }`}
          title="Save changes (Ctrl+S)"
        >
          💾 Save
        </button>

        <button
          onClick={onClose}
          className="px-3 py-1.5 rounded bg-stationpedia-surface hover:bg-gray-700 text-gray-300 hover:text-white transition-colors text-xs font-medium"
          title="Close workspace"
        >
          ✕ Close
        </button>
      </div>
    </div>
  );
};
