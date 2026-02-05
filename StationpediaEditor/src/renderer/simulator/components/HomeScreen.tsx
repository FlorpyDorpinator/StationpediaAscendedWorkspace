/**
 * HomeScreen component
 * Main screen displaying category list matching actual game Stationpedia
 * Game has top row buttons, then Guides/Universe tabs, then 2-column category buttons
 * Uses actual thumbnails from the AssetRipper folder
 */
import React, { useEffect, useMemo, useState } from 'react';
import { STATIONPEDIA_CATEGORIES, getThumbnailUrl } from '../types/stationpediaTypes';
import { getCategoryCounts, initializeStationpediaService } from '../services/stationpediaService';

// Import game UI assets  
import iconStationpedia from '../assets/icon-stationpedia.png';
import searchIcon from '../assets/searchicon.png';
// The button background images (orange = beginnerguide, blue = advancedguide)
import beginnerGuideButtonBg from '../assets/beginnerguidebutton-normal.png';
import advancedGuideButtonBg from '../assets/advancedguidebutton-normal.png';

export interface HomeScreenProps {
  onSelectCategory: (categoryId: string) => void;
  onSelectDevice?: (deviceKey: string) => void;
  onSelectGuides?: () => void;
  onSelectSurvivalManual?: () => void;
  onSelectGameMechanics?: () => void;
  activeTab?: string;
  onTabChange?: (tab: string) => void;
}

// Top button styled with game button background image
const TopButton: React.FC<{
  label: string;
  onClick: () => void;
  bgImage: string;
}> = ({ label, onClick, bgImage }) => {
  const [isHovered, setIsHovered] = useState(false);
  
  return (
    <button
      onClick={onClick}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
      className="flex-1 flex items-center justify-center py-4 px-4 font-semibold transition-all text-white"
      style={{
        backgroundImage: `url(${bgImage})`,
        backgroundSize: '100% 100%',
        backgroundRepeat: 'no-repeat',
        backgroundPosition: 'center',
        borderRadius: '4px',
        minHeight: '60px',
        filter: isHovered ? 'brightness(1.2)' : 'brightness(1)',
        textShadow: '1px 1px 2px rgba(0,0,0,0.8)',
      }}
    >
      <span className="text-sm">{label}</span>
    </button>
  );
};

// Tab button component matching game style - Guides/Universe
// Uses the actual game button images as backgrounds
const TabButton: React.FC<{
  label: string;
  isActive: boolean;
  onClick: () => void;
  bgImage: string;
}> = ({ label, isActive, onClick, bgImage }) => {
  const [isHovered, setIsHovered] = useState(false);
  
  return (
    <button
      onClick={onClick}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
      className="flex-1 flex items-center justify-center gap-2 py-3 px-6 text-white font-semibold transition-all"
      style={{
        backgroundImage: `url(${bgImage})`,
        backgroundSize: '100% 100%',
        backgroundRepeat: 'no-repeat',
        backgroundPosition: 'center',
        borderRadius: '4px',
        minHeight: '50px',
        filter: isActive ? 'brightness(1.3)' : (isHovered ? 'brightness(1.15)' : 'brightness(1)'),
        textShadow: '1px 1px 2px rgba(0,0,0,0.8)',
        border: isActive ? '2px solid rgba(255,255,255,0.3)' : '2px solid transparent',
      }}
    >
      <span>{label}</span>
    </button>
  );
};

// Category button using actual game thumbnails
const GameCategoryButton: React.FC<{
  category: typeof STATIONPEDIA_CATEGORIES[0];
  deviceCount: number;
  onClick: (categoryId: string) => void;
}> = ({ category, deviceCount, onClick }) => {
  const [isHovered, setIsHovered] = useState(false);
  const [imageError, setImageError] = useState(false);
  
  // Get thumbnail URL for the category's representative item
  const thumbnailSrc = getThumbnailUrl(category.iconPrefab);
  
  return (
    <button
      onClick={() => onClick(category.id)}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
      className="flex items-center gap-3 w-full text-left transition-all duration-100"
      style={{
        backgroundColor: isHovered ? '#4A4F54' : '#3A3F44',
        border: '1px solid #5A5F64',
        borderRadius: '3px',
        padding: '8px 12px',
        minHeight: '44px',
      }}
    >
      {/* Category Icon - using actual game thumbnail */}
      <div className="w-8 h-8 flex items-center justify-center flex-shrink-0">
        {!imageError ? (
          <img 
            src={thumbnailSrc}
            alt={category.name} 
            className="max-w-full max-h-full object-contain"
            onError={() => setImageError(true)}
          />
        ) : (
          <span className="text-xl">📦</span>
        )}
      </div>

      {/* Category Name */}
      <span 
        className="text-sm font-medium flex-1"
        style={{ color: '#E6EDF3' }}
      >
        {category.name}
      </span>

      {/* Device Count */}
      {deviceCount > 0 && (
        <span 
          className="text-xs opacity-60"
          style={{ color: '#E6EDF3' }}
        >
          ({deviceCount})
        </span>
      )}
    </button>
  );
};

export const HomeScreen: React.FC<HomeScreenProps> = ({
  onSelectCategory,
  onSelectDevice,
  onSelectGuides,
  onSelectSurvivalManual,
  onSelectGameMechanics,
}) => {
  const [searchQuery, setSearchQuery] = useState('');
  const [activeTab, setActiveTab] = useState<'guides' | 'universe'>('universe');
  const [isLoading, setIsLoading] = useState(true);
  const [categoryCounts, setCategoryCounts] = useState<Map<string, number>>(new Map());
  const [error, setError] = useState<string | null>(null);

  // Initialize the stationpedia service on mount
  useEffect(() => {
    async function loadData() {
      try {
        setIsLoading(true);
        await initializeStationpediaService();
        setCategoryCounts(getCategoryCounts());
        setError(null);
      } catch (err) {
        console.error('Failed to load stationpedia data:', err);
        setError(err instanceof Error ? err.message : 'Failed to load data');
      } finally {
        setIsLoading(false);
      }
    }
    loadData();
  }, []);

  // Filter categories based on search
  const filteredCategories = useMemo(() => {
    if (!searchQuery.trim()) return STATIONPEDIA_CATEGORIES;
    const query = searchQuery.toLowerCase();
    return STATIONPEDIA_CATEGORIES.filter(cat => 
      cat.name.toLowerCase().includes(query)
    );
  }, [searchQuery]);

  return (
    <div className="flex flex-col h-full bg-[#1A1F24]">
      {/* Header - matches game's dark header with title */}
      <div className="px-4 py-3 border-b border-[#3A3F44] bg-[#1A1F24]">
        <div className="flex items-center gap-2">
          <img src={iconStationpedia} alt="Stationpedia" className="w-6 h-6" />
          <h1 className="text-lg font-semibold text-[#FF7A18]">
            Stationpedia
          </h1>
          <span className="text-lg font-semibold text-[#008AE6]">
            Ascended
          </span>
        </div>
      </div>

      {/* Search Bar - game-style */}
      <div className="px-4 py-2 bg-[#2A2F34] border-b border-[#3A3F44]">
        <div className="relative">
          <img 
            src={searchIcon} 
            alt="Search" 
            className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 opacity-60"
          />
          <input
            type="text"
            placeholder="Search"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full bg-[#1A1F24] border border-[#3A3F44] rounded px-10 py-2 text-[#E6EDF3] placeholder-[#6E7681] focus:outline-none focus:border-[#FF7A18] text-sm"
          />
        </div>
      </div>

      {/* Top Row Buttons - Survival Manual (orange) / Game Mechanics (blue) */}
      <div className="px-4 py-2 bg-[#2A2F34]">
        <div className="flex gap-2">
          <TopButton
            label="Stationeers Survival Manual"
            onClick={() => onSelectSurvivalManual?.()}
            bgImage={beginnerGuideButtonBg}
          />
          <TopButton
            label="Game Mechanics"
            onClick={() => onSelectGameMechanics?.()}
            bgImage={advancedGuideButtonBg}
          />
        </div>
      </div>

      {/* Guides / Universe Tabs - using actual game button images */}
      <div className="px-4 py-2 bg-[#2A2F34] border-b border-[#3A3F44]">
        <div className="flex gap-2">
          <TabButton
            label="Guides"
            isActive={activeTab === 'guides'}
            onClick={() => setActiveTab('guides')}
            bgImage={beginnerGuideButtonBg}
          />
          <TabButton
            label="Universe"
            isActive={activeTab === 'universe'}
            onClick={() => setActiveTab('universe')}
            bgImage={advancedGuideButtonBg}
          />
        </div>
      </div>

      {/* Main Content - Category List */}
      <div className="flex-1 overflow-auto bg-[#2A2F34]">
        <div className="p-3">
          {isLoading ? (
            <div className="text-center text-[#8B949E] py-8">
              <p className="text-sm">Loading Stationpedia data...</p>
            </div>
          ) : error ? (
            <div className="text-center text-red-400 py-8">
              <p className="text-sm">Error: {error}</p>
              <p className="text-xs mt-2">Check that Stationpedia.json exists</p>
            </div>
          ) : activeTab === 'guides' ? (
            // Guides tab content - show a button to open the guides list
            <div className="space-y-2">
              <p className="text-[#8B949E] text-sm mb-4 px-1">
                In-game tutorials and guides from Stationpedia Ascended
              </p>
              <button
                onClick={() => onSelectGuides?.()}
                className="w-full flex items-center gap-3 p-4 rounded bg-[#0A1520] hover:bg-[#0F1A25] border border-[#3A3F44] hover:border-[#FF7A18] transition-colors text-left"
              >
                <span className="text-3xl">📚</span>
                <div>
                  <span className="text-[#FF7A18] font-semibold text-lg block">Browse All Guides</span>
                  <span className="text-[#8B949E] text-sm">View tutorials, beginner guides, and more</span>
                </div>
              </button>
            </div>
          ) : (
            // Universe tab - 2-column grid matching game layout
            <div className="grid grid-cols-2 gap-2">
              {filteredCategories.map((category) => (
                <GameCategoryButton
                  key={category.id}
                  category={category}
                  deviceCount={categoryCounts.get(category.id) || 0}
                  onClick={onSelectCategory}
                />
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default HomeScreen;
