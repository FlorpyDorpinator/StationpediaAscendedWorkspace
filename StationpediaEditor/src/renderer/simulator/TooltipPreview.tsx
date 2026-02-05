/**
 * TooltipPreview - Preview component for tooltips as they would appear in-game
 * Shows tooltip in a game-like tooltip container with dark background
 */
import React from 'react';
import type { TooltipDefinition } from '@models/tooltipModel';

interface TooltipPreviewProps {
  tooltip: TooltipDefinition | null;
  maxWidth?: number; // pixels
}

export const TooltipPreview: React.FC<TooltipPreviewProps> = ({ tooltip, maxWidth = 300 }) => {
  if (!tooltip) {
    return (
      <div className="flex items-center justify-center h-full text-gray-500">
        <p>Select a tooltip to preview</p>
      </div>
    );
  }

  return (
    <div className="p-4 flex items-start justify-center">
      {/* Tooltip container - mimics in-game style */}
      <div
        className="relative rounded border border-gray-600 bg-gray-950 shadow-lg p-3 text-sm leading-relaxed"
        style={{ maxWidth }}
      >
        {/* Tooltip content */}
        <div className="text-gray-200 whitespace-pre-wrap break-words">{tooltip.description}</div>

        {/* Tooltip arrow/pointer (optional) */}
        <div
          className="absolute -bottom-1 left-4 w-2 h-2 bg-gray-950 rotate-45 border-r border-b border-gray-600"
          style={{ width: '6px', height: '6px' }}
        />
      </div>
    </div>
  );
};
