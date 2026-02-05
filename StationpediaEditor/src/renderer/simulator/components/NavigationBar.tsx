/**
 * Navigation Bar for Simulator
 * Provides device navigation controls, mode toggle, home button, breadcrumbs, and global search
 */
import React, { useState } from 'react';
import { Breadcrumb, BreadcrumbItem } from './Breadcrumb';
import { SearchBar } from './SearchBar';
import { CATEGORIES } from '../types/categories';

interface NavigationBarProps {
  currentDeviceKey: string | null;
  devices: any[];
  mode: 'vanilla' | 'ascended';
  onDeviceSelect: (deviceKey: string) => void;
  onModeChange: (mode: 'vanilla' | 'ascended') => void;
  onBack: () => void;
  onForward: () => void;
  view: 'home' | 'category' | 'device' | 'survival-manual' | 'game-mechanics' | 'guides-list' | 'guide-viewer';
  selectedCategory: string | null;
  onHome: () => void;
  onCategoryClick: (categoryId: string) => void;
  themeDebug?: boolean;
  onThemeDebugToggle?: (enabled: boolean) => void;
}

export const NavigationBar: React.FC<NavigationBarProps> = ({
  currentDeviceKey,
  devices,
  mode,
  onDeviceSelect,
  onModeChange,
  onBack,
  onForward,
  view,
  selectedCategory,
  onHome,
  onCategoryClick,
  themeDebug = false,
  onThemeDebugToggle,
}) => {
  const [dropdownOpen, setDropdownOpen] = useState(false);

  const currentDevice = devices.find((d) => d.deviceKey === currentDeviceKey);

  // Build breadcrumb items based on current view
  const buildBreadcrumbs = (): BreadcrumbItem[] => {
    const items: BreadcrumbItem[] = [
      { label: 'Home', onClick: onHome },
    ];

    if (view === 'category' && selectedCategory) {
      const category = CATEGORIES.find((c) => c.id === selectedCategory);
      if (category) {
        items.push({
          label: category.name,
          onClick: undefined, // Current location
        });
      }
    } else if (view === 'device' && selectedCategory) {
      const category = CATEGORIES.find((c) => c.id === selectedCategory);
      if (category) {
        items.push({
          label: category.name,
          onClick: () => onCategoryClick(selectedCategory),
        });
      }
      if (currentDevice) {
        items.push({
          label: currentDevice.displayName || currentDevice.deviceKey,
          onClick: undefined, // Current location
        });
      }
    } else if (view === 'survival-manual') {
      items.push({
        label: 'Survival Manual',
        onClick: undefined, // Current location
      });
    } else if (view === 'game-mechanics') {
      items.push({
        label: 'Game Mechanics',
        onClick: undefined, // Current location
      });
    } else if (view === 'guides-list') {
      items.push({
        label: 'Guides',
        onClick: undefined, // Current location
      });
    } else if (view === 'guide-viewer') {
      items.push({
        label: 'Guides',
        onClick: undefined, // Navigate to guides list should be added via props
      });
    }

    return items;
  };

  const breadcrumbs = buildBreadcrumbs();

  return (
    <div className="bg-stationpedia-surface border-b border-stationpedia-border px-4 py-3 space-y-2">
      {/* Top row: Navigation buttons and mode toggle */}
      <div className="flex items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <button
            onClick={onHome}
            title="Return to home"
            className="px-2 py-1 rounded bg-stationpedia-border hover:bg-stationpedia-border/80 text-stationpedia-text transition-colors text-sm"
          >
            🏠
          </button>
          <button
            onClick={onBack}
            title="Back"
            className="px-2 py-1 rounded bg-stationpedia-border hover:bg-stationpedia-border/80 text-stationpedia-text transition-colors text-sm"
          >
            ← Back
          </button>
          <button
            onClick={onForward}
            title="Forward"
            className="px-2 py-1 rounded bg-stationpedia-border hover:bg-stationpedia-border/80 text-stationpedia-text transition-colors text-sm"
          >
            Forward →
          </button>
        </div>

        <div className="flex items-center gap-2">
          {/* Theme Debug toggle */}
          {onThemeDebugToggle && (
            <button
              onClick={() => onThemeDebugToggle(!themeDebug)}
              className={`px-2 py-1 rounded text-xs font-medium transition-colors ${
                themeDebug
                  ? 'bg-[#FF7A18] text-white'
                  : 'bg-stationpedia-border text-stationpedia-text-muted hover:text-stationpedia-text'
              }`}
              title="Toggle theme debug mode - shows asset filenames"
            >
              🔧 Debug
            </button>
          )}

          {/* Mode toggle */}
          <div className="bg-stationpedia-bg rounded p-1 flex gap-1">
            <button
              onClick={() => onModeChange('vanilla')}
              className={`px-3 py-1 rounded text-sm font-medium transition-colors ${
                mode === 'vanilla'
                  ? 'bg-stationpedia-accent text-stationpedia-bg'
                  : 'text-stationpedia-text-muted hover:text-stationpedia-text'
              }`}
              title="Show vanilla (base game) content"
            >
              Vanilla
            </button>
            <button
              onClick={() => onModeChange('ascended')}
              className={`px-3 py-1 rounded text-sm font-medium transition-colors ${
                mode === 'ascended'
                  ? 'bg-stationpedia-accent text-stationpedia-bg'
                  : 'text-stationpedia-text-muted hover:text-stationpedia-text'
              }`}
              title="Show full content (Ascended mode)"
            >
              Ascended
            </button>
          </div>
        </div>
      </div>

      {/* Breadcrumb Navigation */}
      <div className="px-2 py-1">
        <Breadcrumb items={breadcrumbs} />
      </div>

      {/* Global Search Bar */}
      <div className="flex items-center gap-2">
        <label className="text-sm text-stationpedia-text-muted flex-shrink-0">Search:</label>
        <SearchBar
          devices={devices}
          onSelectDevice={onDeviceSelect}
          placeholder="Search all devices..."
        />
      </div>
    </div>
  );
};
