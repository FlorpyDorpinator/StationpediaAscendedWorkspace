/**
 * RecentFilesMenu - Dropdown menu for recent workspace files
 */
import React, { useCallback, useState } from 'react';

export interface RecentFilesMenuProps {
  recentFiles: string[];
  onSelectFile: (path: string) => void;
  onClearRecent: () => void;
  isOpen: boolean;
  onToggle: () => void;
}

export const RecentFilesMenu: React.FC<RecentFilesMenuProps> = ({
  recentFiles,
  onSelectFile,
  onClearRecent,
  isOpen,
  onToggle,
}) => {
  const [hoveredIndex, setHoveredIndex] = useState<number | null>(null);

  const handleSelectFile = useCallback(
    (path: string) => {
      onSelectFile(path);
      onToggle(); // Close menu after selection
    },
    [onSelectFile, onToggle]
  );

  return (
    <div className="relative">
      {/* Toggle Button */}
      <button
        onClick={onToggle}
        className="px-3 py-1.5 rounded bg-gray-800 hover:bg-gray-700 text-gray-300 hover:text-white transition-colors text-xs font-medium"
        title="Recent workspaces"
      >
        📂 Recent
      </button>

      {/* Dropdown Menu */}
      {isOpen && (
        <div className="absolute right-0 mt-1 w-64 bg-gray-800 border border-gray-700 rounded shadow-lg z-50">
          {recentFiles.length === 0 ? (
            <div className="px-4 py-3 text-gray-400 text-sm">
              No recent files
            </div>
          ) : (
            <>
              {recentFiles.map((path, idx) => (
                <button
                  key={idx}
                  onClick={() => handleSelectFile(path)}
                  onMouseEnter={() => setHoveredIndex(idx)}
                  onMouseLeave={() => setHoveredIndex(null)}
                  className={`w-full text-left px-4 py-2 text-sm transition-colors ${
                    hoveredIndex === idx
                      ? 'bg-gray-700 text-white'
                      : 'text-gray-300 hover:text-gray-200'
                  }`}
                  title={path}
                >
                  <div className="truncate font-medium">
                    {path.split(/[/\\]/).pop()}
                  </div>
                  <div className="text-xs text-gray-500 truncate">
                    {path}
                  </div>
                </button>
              ))}

              {/* Separator */}
              <div className="border-t border-gray-700 my-1" />

              {/* Clear Recent Files */}
              <button
                onClick={() => {
                  onClearRecent();
                  onToggle();
                }}
                className="w-full text-left px-4 py-2 text-sm text-gray-400 hover:text-red-400 transition-colors"
              >
                ✕ Clear Recent Files
              </button>
            </>
          )}
        </div>
      )}
    </div>
  );
};
