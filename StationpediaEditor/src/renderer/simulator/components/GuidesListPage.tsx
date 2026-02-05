/**
 * GuidesListPage component
 * Shows a list of all guides from the mod's Guides folder
 */
import React, { useState, useEffect } from 'react';

// Path to the guides folder
const GUIDES_FOLDER_PATH = 'c:/Dev/12-17-25 Stationeers Respawn Update Code/StationpediaAscended/mod/Guides';

interface GuideInfo {
  name: string;
  filename: string;
  path: string;
}

export interface GuidesListPageProps {
  onBack?: () => void;
  onSelectGuide?: (guidePath: string, guideName: string) => void;
}

export const GuidesListPage: React.FC<GuidesListPageProps> = ({
  onBack,
  onSelectGuide,
}) => {
  const [guides, setGuides] = useState<GuideInfo[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function loadGuides() {
      try {
        setIsLoading(true);
        
        if (!window.electronAPI?.listFiles) {
          throw new Error('Electron API not available');
        }

        const result = await window.electronAPI.listFiles(GUIDES_FOLDER_PATH);
        if (!result.success || !result.data) {
          throw new Error(result.error || 'Failed to list guides');
        }

        // Filter for markdown files and create guide info
        const guideFiles = result.data
          .filter((file: string) => file.endsWith('.md'))
          .map((filename: string) => {
            // Convert filename to display name
            const name = filename
              .replace('.md', '')
              .replace(/-/g, ' ')
              .replace(/\b\w/g, (l: string) => l.toUpperCase());
            
            return {
              name,
              filename,
              path: `${GUIDES_FOLDER_PATH}/${filename}`,
            };
          });

        setGuides(guideFiles);
        setError(null);
      } catch (err) {
        console.error('Failed to load guides:', err);
        setError(err instanceof Error ? err.message : 'Failed to load');
      } finally {
        setIsLoading(false);
      }
    }

    loadGuides();
  }, []);

  return (
    <div className="flex flex-col h-full bg-[#1A1F24]">
      {/* Header */}
      <div className="px-4 py-3 border-b border-[#3A3F44] bg-[#1A1F24] flex items-center gap-3">
        {onBack && (
          <button
            onClick={onBack}
            className="text-[#FF7A18] hover:text-[#FF9A48] transition-colors"
          >
            ← Back
          </button>
        )}
        <h1 className="text-lg font-semibold text-[#FF7A18]">
          Guides
        </h1>
      </div>

      {/* Content */}
      <div className="flex-1 overflow-auto p-4">
        {isLoading ? (
          <div className="text-center text-[#8B949E] py-8">
            <p>Loading guides...</p>
          </div>
        ) : error ? (
          <div className="text-center text-red-400 py-8">
            <p>Error: {error}</p>
          </div>
        ) : guides.length === 0 ? (
          <div className="text-center text-[#8B949E] py-8">
            <p>No guides found</p>
          </div>
        ) : (
          <div className="space-y-2">
            {guides.map((guide) => (
              <button
                key={guide.filename}
                onClick={() => onSelectGuide?.(guide.path, guide.name)}
                className="w-full flex items-center gap-3 p-3 rounded bg-[#0A1520] hover:bg-[#0F1A25] border border-[#3A3F44] hover:border-[#FF7A18] transition-colors text-left"
              >
                <span className="text-2xl">📖</span>
                <span className="text-[#E6EDF3] font-medium">{guide.name}</span>
              </button>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default GuidesListPage;
