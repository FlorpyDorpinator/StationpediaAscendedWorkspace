/**
 * TooltipItemEditor - Edit a single tooltip
 * Allows editing the description with rich text support
 */
import React, { useCallback } from 'react';
import { RichTextEditor } from './RichTextEditor';
import { useEditorStore } from './editorStore';
import { TooltipPreview } from '@simulator/TooltipPreview';
import type { TooltipDefinition } from '@models/tooltipModel';

interface TooltipItemEditorProps {
  tooltip: TooltipDefinition | null;
  onUpdate?: (tooltip: TooltipDefinition) => void;
  onCancel?: () => void;
}

export const TooltipItemEditor: React.FC<TooltipItemEditorProps> = ({
  tooltip,
  onUpdate,
  onCancel,
}) => {
  const workspace = useEditorStore((state) => state.workspace);
  const [localDescription, setLocalDescription] = React.useState(tooltip?.description || '');

  // Update local state when tooltip changes
  React.useEffect(() => {
    setLocalDescription(tooltip?.description || '');
  }, [tooltip]);

  const handleSave = useCallback(() => {
    if (tooltip && onUpdate) {
      onUpdate({
        ...tooltip,
        description: localDescription,
      });
    }
  }, [tooltip, localDescription, onUpdate]);

  const handleCancel = useCallback(() => {
    setLocalDescription(tooltip?.description || '');
    onCancel?.();
  }, [tooltip, onCancel]);

  if (!tooltip) {
    return (
      <div className="flex items-center justify-center h-full text-gray-500">
        <p>Select a tooltip to edit</p>
      </div>
    );
  }

  return (
    <div className="flex flex-col h-full bg-gray-950">
      {/* Header with tooltip key */}
      <div className="border-b border-gray-700 p-4 bg-gray-900">
        <div className="text-xs uppercase tracking-wide text-gray-500 mb-1">Tooltip Key</div>
        <div className="text-sm font-mono text-gray-300">{tooltip.key}</div>
        {tooltip.category && (
          <div className="text-xs text-gray-500 mt-2">Category: {tooltip.category}</div>
        )}
      </div>

      {/* Main editing area */}
      <div className="flex flex-1 gap-4 p-4 overflow-hidden">
        {/* Editor side */}
        <div className="flex-1 flex flex-col min-w-0">
          <label className="text-xs uppercase tracking-wide text-gray-500 mb-2">
            Description
          </label>
          <div className="flex-1 overflow-hidden rounded border border-gray-700 bg-gray-900">
            <RichTextEditor
              content={localDescription}
              onChange={setLocalDescription}
              placeholder="Enter tooltip description..."
              workspace={workspace}
            />
          </div>
        </div>

        {/* Preview side */}
        <div className="flex-1 flex flex-col min-w-0">
          <label className="text-xs uppercase tracking-wide text-gray-500 mb-2">Preview</label>
          <div className="flex-1 overflow-auto rounded border border-gray-700 bg-gray-900">
            <TooltipPreview
              tooltip={{
                ...tooltip,
                description: localDescription,
              }}
              maxWidth={250}
            />
          </div>
        </div>
      </div>

      {/* Footer with actions */}
      <div className="border-t border-gray-700 p-4 bg-gray-900 flex gap-2 justify-end">
        <button
          onClick={handleCancel}
          className="px-4 py-2 rounded text-sm font-medium bg-gray-800 text-gray-300 hover:bg-gray-700 transition-colors"
        >
          Cancel
        </button>
        <button
          onClick={handleSave}
          className="px-4 py-2 rounded text-sm font-medium bg-cyan-600 text-white hover:bg-cyan-500 transition-colors"
        >
          Save
        </button>
      </div>
    </div>
  );
};
