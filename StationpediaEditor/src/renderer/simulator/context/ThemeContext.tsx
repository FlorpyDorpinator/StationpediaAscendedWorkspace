/**
 * ThemeContext - Provides game theme configuration and debug mode
 * Loads theme from stationpediaTheme.json and provides access throughout the app
 */

import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import themeConfig from '../stationpediaTheme.json';

// Theme types
export interface ThemeColors {
  background: string;
  backgroundSecondary: string;
  border: string;
  accent: string;
  accentHover: string;
  link: string;
  linkHover: string;
  textPrimary: string;
  textSecondary: string;
  textMuted: string;
  success: string;
  warning: string;
  error: string;
}

export interface ThemeAssets {
  windowBackground: string;
  dialogBackground: string;
  buttonNormal: string;
  buttonHover: string;
  buttonBackground: string;
  homeButton: string;
  backButton: string;
  forwardButton: string;
  closeButton: string;
  expandButton: string;
  shrinkButton: string;
  searchIcon: string;
  scrollbarBackground: string;
  scrollbarHandle: string;
  iconStationpedia: string;
}

export interface NineSlice {
  top: number;
  right: number;
  bottom: number;
  left: number;
}

export interface ThemeConfig {
  colors: ThemeColors;
  assets: ThemeAssets;
  categoryIcons: Record<string, string>;
  nineSlice: Record<string, NineSlice>;
  sectionTitles: Record<string, string>;
  debugMode: boolean;
}

export interface ThemeContextType {
  theme: ThemeConfig;
  debugMode: boolean;
  setDebugMode: (enabled: boolean) => void;
  getAssetPath: (assetKey: keyof ThemeAssets) => string;
  getCategoryIcon: (category: string) => string | null;
  getNineSlice: (assetKey: string) => NineSlice | null;
  getSectionTitle: (sectionKey: string) => string;
}

const defaultTheme: ThemeConfig = themeConfig as ThemeConfig;

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

export interface ThemeProviderProps {
  children: ReactNode;
  initialDebugMode?: boolean;
}

/**
 * ThemeProvider component
 * Wrap your app with this to access theme configuration
 */
export const ThemeProvider: React.FC<ThemeProviderProps> = ({ 
  children, 
  initialDebugMode = false 
}) => {
  const [debugMode, setDebugMode] = useState(initialDebugMode);
  const [theme] = useState<ThemeConfig>(defaultTheme);

  // Apply CSS variables to document root
  useEffect(() => {
    const root = document.documentElement;
    Object.entries(theme.colors).forEach(([key, value]) => {
      root.style.setProperty(`--game-${key}`, value);
    });
  }, [theme.colors]);

  const getAssetPath = (assetKey: keyof ThemeAssets): string => {
    return theme.assets[assetKey] || '';
  };

  const getCategoryIcon = (category: string): string | null => {
    return theme.categoryIcons[category] || null;
  };

  const getNineSlice = (assetKey: string): NineSlice | null => {
    return theme.nineSlice[assetKey] || null;
  };

  const getSectionTitle = (sectionKey: string): string => {
    return theme.sectionTitles[sectionKey] || sectionKey;
  };

  const contextValue: ThemeContextType = {
    theme,
    debugMode,
    setDebugMode,
    getAssetPath,
    getCategoryIcon,
    getNineSlice,
    getSectionTitle,
  };

  return (
    <ThemeContext.Provider value={contextValue}>
      {children}
    </ThemeContext.Provider>
  );
};

/**
 * Hook to access theme context
 */
export const useTheme = (): ThemeContextType => {
  const context = useContext(ThemeContext);
  if (context === undefined) {
    throw new Error('useTheme must be used within a ThemeProvider');
  }
  return context;
};

/**
 * Debug overlay component to show asset paths
 */
export interface DebugOverlayProps {
  assetKey: string;
  assetPath: string;
  children: ReactNode;
}

export const DebugOverlay: React.FC<DebugOverlayProps> = ({ 
  assetKey, 
  assetPath, 
  children 
}) => {
  const { debugMode } = useTheme();

  if (!debugMode) {
    return <>{children}</>;
  }

  return (
    <div className="relative group">
      {children}
      <div 
        className="absolute bottom-0 left-0 right-0 bg-black/90 text-xs text-[#FF7A18] p-1 opacity-0 group-hover:opacity-100 transition-opacity z-50 pointer-events-none whitespace-nowrap overflow-hidden text-ellipsis"
        title={`${assetKey}: ${assetPath}`}
      >
        {assetPath ? `📁 ${assetPath}` : `⚠️ Missing: ${assetKey}`}
      </div>
    </div>
  );
};

export default ThemeContext;
