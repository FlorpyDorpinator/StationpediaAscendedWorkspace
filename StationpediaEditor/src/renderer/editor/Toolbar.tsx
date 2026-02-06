/**
 * Toolbar - Application toolbar with File menu and mode toggle
 */
import React from 'react';

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

const RefreshIcon: React.FC<{ className?: string }> = ({ className }) => (
  <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M21 2v6h-6" />
    <path d="M3 12a9 9 0 0 1 15-6.7L21 8" />
    <path d="M3 22v-6h6" />
    <path d="M21 12a9 9 0 0 1-15 6.7L3 16" />
  </svg>
);

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
  const disabledStyle = 'bg-gray-800 text-gray-500 cursor-not-allowed';

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
      <div className="flex items-center gap-2">
        {isLoading && (
          <div className="text-stationpedia-accent text-xs animate-pulse">Loading...</div>
        )}

        {/* Simulator */}
        <button
          onClick={onOpenSimulator}
          disabled={!workspacePath}
          className={`px-4 py-2 rounded text-sm font-medium transition-colors ${
            workspacePath
              ? 'bg-purple-600 hover:bg-purple-700 text-white'
              : disabledStyle
          }`}
          title="Open simulator preview window"
        >
          <span className="text-base mr-1">🎮</span> Simulator
        </button>

        {/* Global Tooltips */}
        <button
          onClick={onToggleTooltips}
          className={`px-4 py-2 rounded text-sm font-medium transition-colors ${
            showTooltips
              ? 'bg-blue-600 hover:bg-blue-700 text-white'
              : 'bg-blue-600/30 hover:bg-blue-600/50 text-blue-300'
          }`}
          title="Toggle global tooltips panel"
        >
          <span className="text-base mr-1">💬</span> Global Tooltips
        </button>

        {/* Shortcuts */}
        <button
          onClick={onToggleShortcuts}
          className={`px-4 py-2 rounded text-sm font-medium transition-colors ${
            showShortcuts
              ? 'bg-teal-600 hover:bg-teal-700 text-white'
              : 'bg-teal-600/30 hover:bg-teal-600/50 text-teal-300'
          }`}
          title="Show keyboard shortcuts"
        >
          <span className="text-base mr-1">⌨️</span> Shortcuts
        </button>

        {/* Refresh (was Reset Layout) */}
        <button
          onClick={onResetLayout}
          className="px-4 py-2 rounded bg-slate-600 hover:bg-slate-700 text-white transition-colors text-sm font-medium"
          title="Refresh panel layout"
        >
          <RefreshIcon className="w-4 h-4 inline mr-1" /> Refresh
        </button>

        {/* Open */}
        <button
          onClick={onOpenWorkspace}
          className="px-4 py-2 rounded bg-indigo-600 hover:bg-indigo-700 text-white transition-colors text-sm font-medium"
          title="Open a workspace (Ctrl+O)"
        >
          <span className="text-base mr-1">📁</span> Open
        </button>

        {/* Import JSON */}
        <button
          onClick={onImportGuide}
          disabled={!workspacePath}
          className={`px-4 py-2 rounded text-sm font-medium transition-colors ${
            workspacePath
              ? 'bg-green-600 hover:bg-green-700 text-white'
              : disabledStyle
          }`}
          title="Import a guide from a JSON file"
        >
          <span className="text-base mr-1">📥</span> Import JSON
        </button>

        {/* Save */}
        <button
          onClick={onSave}
          disabled={!isDirty || !workspacePath || isLoading}
          className={`px-4 py-2 rounded text-sm font-medium transition-colors ${
            isDirty && workspacePath && !isLoading
              ? 'bg-stationpedia-accent hover:bg-stationpedia-accent-hover text-white'
              : disabledStyle
          }`}
          title="Save changes (Ctrl+S)"
        >
          <span className="text-base mr-1">💾</span> Save
        </button>

        {/* Close */}
        <button
          onClick={onClose}
          className="px-4 py-2 rounded bg-red-600 hover:bg-red-700 text-white transition-colors text-sm font-medium"
          title="Close workspace"
        >
          <span className="text-base mr-1">✕</span> Close
        </button>
      </div>
    </div>
  );
};
