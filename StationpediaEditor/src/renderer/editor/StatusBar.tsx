/**
 * StatusBar - Status bar component showing file info, dirty state, validation summary
 */
import React from 'react';

export interface StatusBarProps {
  isDirty: boolean;
  workspacePath?: string | null;
  validationErrors?: number;
  validationWarnings?: number;
  characterCount?: number;
  wordCount?: number;
}

export const StatusBar: React.FC<StatusBarProps> = ({
  isDirty,
  workspacePath,
  validationErrors = 0,
  validationWarnings = 0,
  characterCount = 0,
  wordCount = 0,
}) => {
  return (
    <div className="bg-stationpedia-surface border-t border-stationpedia-border px-4 py-2 flex items-center justify-between text-xs text-gray-400 h-8">
      {/* Left: File info */}
      <div className="flex items-center gap-6 flex-1 min-w-0">
        {/* Dirty indicator */}
        {isDirty && (
          <div className="flex items-center gap-2 text-yellow-400 whitespace-nowrap">
            <span className="text-lg">●</span>
            <span>Unsaved</span>
          </div>
        )}

        {/* File path */}
        {workspacePath && (
          <div className="truncate text-gray-500 hover:text-gray-400" title={workspacePath}>
            {workspacePath}
          </div>
        )}
      </div>

      {/* Right: Validation and stats */}
      <div className="flex items-center gap-4">
        {/* Validation summary */}
        {(validationErrors > 0 || validationWarnings > 0) && (
          <div className="flex items-center gap-2">
            {validationErrors > 0 && (
              <div className="flex items-center gap-1 text-red-400">
                <span>🔴</span>
                <span>{validationErrors}</span>
              </div>
            )}
            {validationWarnings > 0 && (
              <div className="flex items-center gap-1 text-yellow-400">
                <span>🟡</span>
                <span>{validationWarnings}</span>
              </div>
            )}
          </div>
        )}

        {/* Character and word count */}
        {(characterCount > 0 || wordCount > 0) && (
          <div className="flex items-center gap-3 text-gray-500">
            {characterCount > 0 && <span>{characterCount} chars</span>}
            {wordCount > 0 && <span>{wordCount} words</span>}
          </div>
        )}
      </div>
    </div>
  );
};
