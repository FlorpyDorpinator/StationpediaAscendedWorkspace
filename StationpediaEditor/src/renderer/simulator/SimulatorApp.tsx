/**
 * SimulatorApp - Main Simulator Window Component
 * Displays live preview of the currently selected device
 */
import React, { useEffect, useState, useCallback } from 'react';
import { StationpediaRenderer } from './StationpediaRenderer';
import { NavigationBar } from './components/NavigationBar';
import { HomeScreen } from './components/HomeScreen';
import { CategoryListView } from './components/CategoryListView';
import { DevicePage } from './components/DevicePage';
import { SurvivalManualPage } from './components/SurvivalManualPage';
import { GuidesListPage } from './components/GuidesListPage';
import { GuideViewerPage } from './components/GuideViewerPage';
import { sharedState } from '@services/sharedState';
import type { DeviceDocument } from '@models/contentModel';
import { LoadingSpinner } from '@components/LoadingSpinner';
// TabBar removed - now using game-accurate list layout
import { STATIONPEDIA_CATEGORIES } from './types/stationpediaTypes';
import { ThemeProvider } from './context/ThemeContext';
import { initializeStationpediaService } from './services/stationpediaService';

// Declare window.simulatorAPI for TypeScript
declare global {
  interface Window {
    simulatorAPI: {
      getCurrentDevice: () => Promise<any>;
      getDevice: (deviceKey: string) => Promise<any>;
      getAllDevices: () => Promise<any[]>;
      navigateToDevice: (deviceKey: string) => void;
      setMode: (mode: 'vanilla' | 'ascended') => void;
      getMode: () => Promise<'vanilla' | 'ascended'>;
      onDeviceChanged: (callback: (deviceKey: string) => void) => void;
      onModeChanged: (callback: (mode: 'vanilla' | 'ascended') => void) => void;
      removeDeviceChangeListener: () => void;
      removeModeChangeListener: () => void;
    };
  }
}

export const SimulatorApp: React.FC = () => {
  const [currentDevice, setCurrentDevice] = useState<DeviceDocument | null>(null);
  const [devices, setDevices] = useState<any[]>([]);
  const [mode, setMode] = useState<'vanilla' | 'ascended'>('ascended');
  const [isLoading, setIsLoading] = useState(true);
  const [history, setHistory] = useState<string[]>([]);
  const [historyIndex, setHistoryIndex] = useState(-1);
  const [viewState, setViewState] = useState<'home' | 'category' | 'device' | 'survival-manual' | 'game-mechanics' | 'guides-list' | 'guide-viewer'>('home');
  const [selectedCategory, setSelectedCategory] = useState<string | null>(null);
  const [selectedDeviceKey, setSelectedDeviceKey] = useState<string | null>(null);
  const [selectedGuidePath, setSelectedGuidePath] = useState<string | null>(null);
  const [selectedGuideName, setSelectedGuideName] = useState<string | null>(null);
  const [themeDebug, setThemeDebug] = useState(false);
  // activeTab removed - using game-accurate simple layout

  // Load preferences and initial state
  useEffect(() => {
    sharedState.loadPreferences();
    const state = sharedState.getState();
    setMode(state.mode);
  }, []);

  // Load devices from main process and initialize stationpedia service
  useEffect(() => {
    async function loadData() {
      try {
        // Initialize stationpedia service (loads Stationpedia.json + descriptions.json)
        await initializeStationpediaService();
        console.log('Stationpedia service initialized');

        // Load devices from editor if available
        if (window.simulatorAPI) {
          const allDevices = await window.simulatorAPI.getAllDevices();
          setDevices(allDevices || []);

          // Load current device
          const currentDev = await window.simulatorAPI.getCurrentDevice();
          if (currentDev) {
            setCurrentDevice(currentDev);
            if (currentDev.deviceKey) {
              setHistory([currentDev.deviceKey]);
              setHistoryIndex(0);
            }
          }
        }
      } catch (error) {
        console.error('Failed to load data:', error);
      } finally {
        setIsLoading(false);
      }
    }

    loadData();
  }, []);

  // Subscribe to device changes
  useEffect(() => {
    if (!window.simulatorAPI) return;

    window.simulatorAPI.onDeviceChanged((deviceKey: string) => {
      handleDeviceSelect(deviceKey);
    });

    return () => {
      window.simulatorAPI.removeDeviceChangeListener();
    };
  }, []);

  // Subscribe to mode changes
  useEffect(() => {
    if (!window.simulatorAPI) return;

    window.simulatorAPI.onModeChanged((newMode: 'vanilla' | 'ascended') => {
      setMode(newMode);
      sharedState.setMode(newMode);
    });

    return () => {
      window.simulatorAPI.removeModeChangeListener();
    };
  }, []);

  // Subscribe to shared state changes
  useEffect(() => {
    const unsubscribe = sharedState.subscribe((state) => {
      setMode(state.mode);
    });

    return unsubscribe;
  }, []);

  // Helper function to get category by ID
  const getCategoryById = (categoryId: string) => {
    return STATIONPEDIA_CATEGORIES.find((c) => c.id === categoryId) || STATIONPEDIA_CATEGORIES[0];
  };

  const handleDeviceSelect = useCallback(
    async (deviceKey: string) => {
      // Always set the selected device key for the new stationpedia viewer
      setSelectedDeviceKey(deviceKey);
      
      // Update history
      const newHistory = history.slice(0, historyIndex + 1);
      newHistory.push(deviceKey);
      setHistory(newHistory);
      setHistoryIndex(newHistory.length - 1);

      // Also try to load from simulator API if available (for editor integration)
      if (window.simulatorAPI) {
        try {
          const device = await window.simulatorAPI.getDevice(deviceKey);
          if (device) {
            setCurrentDevice(device);
            sharedState.setCurrentDevice(deviceKey);
          }
        } catch (error) {
          // Device not found in editor, but that's OK - we have stationpedia data
          console.log('Device not in editor cache, using stationpedia data');
        }
      }
    },
    [history, historyIndex]
  );

  const handleModeChange = useCallback((newMode: 'vanilla' | 'ascended') => {
    setMode(newMode);
    sharedState.setMode(newMode);

    if (window.simulatorAPI) {
      window.simulatorAPI.setMode(newMode);
    }
  }, []);

  const handleBack = useCallback(() => {
    if (historyIndex > 0) {
      const newIndex = historyIndex - 1;
      setHistoryIndex(newIndex);
      handleDeviceSelect(history[newIndex]);
    }
  }, [history, historyIndex, handleDeviceSelect]);

  const handleForward = useCallback(() => {
    if (historyIndex < history.length - 1) {
      const newIndex = historyIndex + 1;
      setHistoryIndex(newIndex);
      handleDeviceSelect(history[newIndex]);
    }
  }, [history, historyIndex, handleDeviceSelect]);

  const handleLinkClick = (target: string) => {
    // Handle links like {THING:StructureAutolathe}
    if (target && target.startsWith('THING:')) {
      const deviceKey = target.replace('THING:', '');
      handleDeviceSelect(deviceKey);
    }
  };

  const handleSelectCategory = useCallback((categoryId: string) => {
    setSelectedCategory(categoryId);
    setViewState('category');
  }, []);

  const handleHome = useCallback(() => {
    setViewState('home');
    setSelectedCategory(null);
  }, []);

  const handleCategoryClick = useCallback((categoryId: string) => {
    setSelectedCategory(categoryId);
    setViewState('category');
  }, []);

  const [categorySearchQuery, setCategorySearchQuery] = useState('');

  return (
    <ThemeProvider initialDebugMode={themeDebug}>
      <div className="flex flex-col h-screen bg-[#0F1F38] text-[#E6EDF3]">
        {/* Theme Debug Indicator */}
        {themeDebug && (
          <div className="bg-[#FF7A18] text-white text-xs text-center py-1">
            🔧 Theme Debug Mode - Hover over UI elements to see asset paths
          </div>
        )}

        {/* Navigation Bar - show when viewing device, category, or guides */}
        {(viewState === 'device' || viewState === 'category' || viewState === 'survival-manual' || viewState === 'game-mechanics' || viewState === 'guides-list' || viewState === 'guide-viewer') && (
          <NavigationBar
            currentDeviceKey={currentDevice?.deviceKey || null}
            devices={devices}
            mode={mode}
            onDeviceSelect={handleDeviceSelect}
            onModeChange={handleModeChange}
            onBack={handleBack}
            onForward={handleForward}
            view={viewState}
            selectedCategory={selectedCategory}
            onHome={handleHome}
            onCategoryClick={handleCategoryClick}
            themeDebug={themeDebug}
            onThemeDebugToggle={setThemeDebug}
          />
        )}

        {/* Content Area */}
        <div className="flex-1 overflow-auto">
          {isLoading ? (
            <div className="flex items-center justify-center h-full gap-3">
              <LoadingSpinner />
              <span className="text-[#8B949E]">Loading simulator...</span>
            </div>
          ) : viewState === 'home' ? (
            <HomeScreen
              onSelectCategory={handleSelectCategory}
              onSelectDevice={(deviceKey) => {
                handleDeviceSelect(deviceKey);
                setViewState('device');
              }}
              onSelectGuides={() => setViewState('guides-list')}
              onSelectSurvivalManual={() => setViewState('survival-manual')}
              onSelectGameMechanics={() => setViewState('game-mechanics')}
            />
          ) : viewState === 'survival-manual' ? (
            <SurvivalManualPage
              onBack={handleHome}
              onNavigate={(deviceKey) => {
                handleDeviceSelect(deviceKey);
                setViewState('device');
              }}
            />
          ) : viewState === 'guides-list' ? (
            <GuidesListPage
              onBack={handleHome}
              onSelectGuide={(path, name) => {
                setSelectedGuidePath(path);
                setSelectedGuideName(name);
                setViewState('guide-viewer');
              }}
            />
          ) : viewState === 'guide-viewer' && selectedGuidePath && selectedGuideName ? (
            <GuideViewerPage
              guidePath={selectedGuidePath}
              guideName={selectedGuideName}
              onBack={() => setViewState('guides-list')}
              onNavigate={(deviceKey) => {
                handleDeviceSelect(deviceKey);
                setViewState('device');
              }}
            />
          ) : viewState === 'game-mechanics' ? (
            <div className="flex flex-col h-full bg-[#1A1F24]">
              <div className="px-4 py-3 border-b border-[#3A3F44] bg-[#1A1F24] flex items-center gap-3">
                <button
                  onClick={handleHome}
                  className="text-[#FF7A18] hover:text-[#FF9A48] transition-colors"
                >
                  ← Back
                </button>
                <h1 className="text-lg font-semibold text-[#008AE6]">
                  Game Mechanics
                </h1>
              </div>
              <div className="flex-1 overflow-auto p-4">
                <div className="text-center text-[#8B949E] py-8">
                  <p>Game Mechanics guide coming soon...</p>
                  <p className="text-xs mt-2">This will contain detailed information about game systems.</p>
                </div>
              </div>
            </div>
          ) : viewState === 'category' && selectedCategory ? (
            <CategoryListView
              categoryId={selectedCategory}
              onSelectDevice={(deviceKey) => {
                handleDeviceSelect(deviceKey);
                setViewState('device');
              }}
              searchQuery={categorySearchQuery}
              onSearchChange={setCategorySearchQuery}
              onBack={handleHome}
            />
          ) : viewState === 'device' && selectedDeviceKey ? (
            <DevicePage
              deviceKey={selectedDeviceKey}
              onBack={() => {
                setViewState(selectedCategory ? 'category' : 'home');
              }}
              onNavigate={(deviceKey) => {
                handleDeviceSelect(deviceKey);
              }}
            />
          ) : (
            <div className="flex items-center justify-center h-full">
              <div className="text-center">
                <p className="text-[#8B949E] mb-2">No device selected</p>
                <p className="text-xs text-[#8B949E]">
                  Select a device from the dropdown above to preview it
                </p>
              </div>
            </div>
          )}
        </div>
      </div>
    </ThemeProvider>
  );
};

export default SimulatorApp;
