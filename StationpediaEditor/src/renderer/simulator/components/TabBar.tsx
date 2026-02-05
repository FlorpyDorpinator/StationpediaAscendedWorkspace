/**
 * TabBar component
 * Displays tabs for switching between Guides and Universe views
 * Uses game-accurate colors: Orange #FF7A18
 */
import React from 'react';

export type TabType = 'guides' | 'universe';

export interface TabBarProps {
  activeTab: TabType;
  onTabChange: (tab: TabType) => void;
}

export const TabBar: React.FC<TabBarProps> = ({ activeTab, onTabChange }) => {
  return (
    <div className="flex border-b border-[#264D73]" style={{ backgroundColor: 'rgba(10, 21, 32, 0.9)' }}>
      <button
        onClick={() => onTabChange('guides')}
        className={`px-6 py-3 font-medium transition-colors ${
          activeTab === 'guides'
            ? 'border-b-2 border-[#FF7A18] text-[#FF7A18]'
            : 'text-[#E6EDF3] hover:text-[#FF7A18]'
        }`}
      >
        Guides
      </button>
      <button
        onClick={() => onTabChange('universe')}
        className={`px-6 py-3 font-medium transition-colors ${
          activeTab === 'universe'
            ? 'border-b-2 border-[#FF7A18] text-[#FF7A18]'
            : 'text-[#E6EDF3] hover:text-[#FF7A18]'
        }`}
      >
        Universe
      </button>
    </div>
  );
};

export default TabBar;
