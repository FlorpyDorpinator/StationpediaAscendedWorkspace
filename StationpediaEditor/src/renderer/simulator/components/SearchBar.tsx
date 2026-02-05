/**
 * SearchBar - Global device search component
 * Searches devices by name and key, groups results by category
 * Renders as dropdown in navigation bar
 */
import React, { useState, useMemo } from 'react';
import { CATEGORIES } from '../types/categories';

interface Device {
  deviceKey: string;
  displayName?: string | null;
  categoryId?: string;
}

interface SearchBarProps {
  devices: Device[];
  onSelectDevice: (deviceKey: string) => void;
  placeholder?: string;
}

interface GroupedResults {
  [categoryId: string]: Device[];
}

export const SearchBar: React.FC<SearchBarProps> = ({
  devices,
  onSelectDevice,
  placeholder = 'Search devices...',
}) => {
  const [searchQuery, setSearchQuery] = useState('');
  const [isOpen, setIsOpen] = useState(false);

  // Filter and group results by category
  const groupedResults = useMemo(() => {
    if (!searchQuery) {
      return {};
    }

    const lower = searchQuery.toLowerCase();
    const filtered = devices.filter(
      (d) =>
        d.displayName?.toLowerCase().includes(lower) ||
        d.deviceKey.toLowerCase().includes(lower)
    );

    const grouped: GroupedResults = {};
    filtered.forEach((device) => {
      const categoryId = device.categoryId || 'other';
      if (!grouped[categoryId]) {
        grouped[categoryId] = [];
      }
      grouped[categoryId].push(device);
    });

    return grouped;
  }, [searchQuery, devices]);

  const hasResults = Object.keys(groupedResults).length > 0;
  const categoryOrder = CATEGORIES.map((c) => c.id);

  const handleSelectDevice = (deviceKey: string) => {
    onSelectDevice(deviceKey);
    setSearchQuery('');
    setIsOpen(false);
  };

  const handleClear = () => {
    setSearchQuery('');
  };

  return (
    <div className="relative flex-1">
      <div className="flex items-center gap-2 px-3 py-2 rounded bg-stationpedia-bg border border-stationpedia-border hover:border-stationpedia-accent transition-colors">
        {/* Search icon */}
        <span className="text-stationpedia-accent text-lg">🔍</span>

        {/* Input field */}
        <input
          type="text"
          placeholder={placeholder}
          value={searchQuery}
          onChange={(e) => {
            setSearchQuery(e.target.value);
            setIsOpen(true);
          }}
          onFocus={() => setIsOpen(true)}
          className="flex-1 bg-transparent text-stationpedia-text placeholder-stationpedia-text-muted text-sm outline-none"
          data-testid="search-input"
        />

        {/* Clear button */}
        {searchQuery && (
          <button
            onClick={handleClear}
            className="text-stationpedia-text-muted hover:text-stationpedia-text transition-colors text-lg flex-shrink-0"
            title="Clear search"
            data-testid="clear-button"
          >
            ✕
          </button>
        )}
      </div>

      {/* Search Results Dropdown */}
      {isOpen && searchQuery && (
        <div
          className="absolute top-full left-0 right-0 mt-2 bg-stationpedia-bg border border-stationpedia-accent rounded shadow-lg z-50 max-h-96 overflow-y-auto"
          data-testid="search-results"
        >
          {hasResults ? (
            <div className="py-2">
              {categoryOrder.map((categoryId) => {
                if (!groupedResults[categoryId]) return null;

                const category = CATEGORIES.find((c) => c.id === categoryId);
                const results = groupedResults[categoryId];

                return (
                  <div key={categoryId}>
                    {/* Category header */}
                    <div className="px-3 py-1 text-xs font-semibold text-stationpedia-accent uppercase tracking-wider">
                      {category?.name || categoryId}
                    </div>

                    {/* Category results */}
                    {results.map((device) => (
                      <button
                        key={device.deviceKey}
                        onClick={() => handleSelectDevice(device.deviceKey)}
                        className="w-full text-left px-3 py-2 hover:bg-stationpedia-accent/20 transition-colors"
                        data-testid={`search-result-${device.deviceKey}`}
                      >
                        <div className="font-medium text-stationpedia-text">
                          {device.displayName || device.deviceKey}
                        </div>
                        <div className="text-xs text-stationpedia-text-muted">
                          {device.deviceKey}
                        </div>
                      </button>
                    ))}
                  </div>
                );
              })}
            </div>
          ) : (
            <div className="px-3 py-4 text-center text-stationpedia-text-muted text-sm">
              No devices found
            </div>
          )}
        </div>
      )}

      {/* Close dropdown on click outside */}
      {isOpen && (
        <div
          className="fixed inset-0 z-40"
          onClick={() => setIsOpen(false)}
          data-testid="search-backdrop"
        />
      )}
    </div>
  );
};

export default SearchBar;
