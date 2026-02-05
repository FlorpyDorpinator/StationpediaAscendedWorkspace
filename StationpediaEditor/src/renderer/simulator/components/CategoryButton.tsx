/**
 * CategoryButton component
 * Displays a category as a horizontal button row matching actual game Stationpedia
 * Game uses grey button-normal.png background with icon on left, name on right
 */
import React from 'react';
import { Category } from '../types/categories';

// Import category icons from game assets
import oresIcon from '../assets/categories/ores.png';
import ingotsIcon from '../assets/categories/ingots.png';
import fabricatorsIcon from '../assets/categories/fabricators.png';
import structureKitsIcon from '../assets/categories/structure-kits.png';
import gasesIcon from '../assets/categories/gases.png';
import reagentsIcon from '../assets/categories/reagents.png';
import atmosphericsIcon from '../assets/categories/atmospherics.png';
import electronicsIcon from '../assets/categories/electronics.png';
import logicDevicesIcon from '../assets/categories/logic-devices.png';
import structuresIcon from '../assets/categories/structures.png';
import organicsIcon from '../assets/categories/organics.png';
import rocketsIcon from '../assets/categories/rockets.png';
import geneticsIcon from '../assets/categories/genetics.png';
import tradingIcon from '../assets/categories/trading.png';
import handToolsIcon from '../assets/categories/hand-tools.png';
import cartridgesIcon from '../assets/categories/cartridges.png';
import personalIcon from '../assets/categories/personal.png';
import furnitureIcon from '../assets/categories/furniture.png';
import importExportIcon from '../assets/categories/import-export.png';

// Import game button assets
import buttonNormal from '../assets/button-normal.png';
import buttonHover from '../assets/button-hover.png';

const CATEGORY_ICONS: Record<string, string> = {
  ores: oresIcon,
  ingots: ingotsIcon,
  fabricators: fabricatorsIcon,
  'structure-kits': structureKitsIcon,
  gases: gasesIcon,
  reagents: reagentsIcon,
  atmospherics: atmosphericsIcon,
  electronics: electronicsIcon,
  'logic-devices': logicDevicesIcon,
  structures: structuresIcon,
  organics: organicsIcon,
  rockets: rocketsIcon,
  genetics: geneticsIcon,
  trading: tradingIcon,
  'hand-tools': handToolsIcon,
  'cartridges': cartridgesIcon,
  'personal': personalIcon,
  'furniture': furnitureIcon,
  'import-export': importExportIcon,
};

export interface CategoryButtonProps {
  category: Category;
  deviceCount?: number;
  onClick: (categoryId: string) => void;
}

export const CategoryButton: React.FC<CategoryButtonProps> = ({
  category,
  deviceCount,
  onClick,
}) => {
  const iconSrc = CATEGORY_ICONS[category.id];
  const [isHovered, setIsHovered] = React.useState(false);

  // Game uses grey buttons (#3A3F44 background with slight border)
  // The button-normal.png is a 9-slice grey button texture
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
        // Use button-normal.png as background for 9-slice effect
        backgroundImage: `url(${isHovered ? buttonHover : buttonNormal})`,
        backgroundSize: '100% 100%',
        backgroundRepeat: 'no-repeat',
      }}
    >
      {/* Category Icon */}
      <div className="w-7 h-7 flex items-center justify-center flex-shrink-0">
        {iconSrc ? (
          <img 
            src={iconSrc} 
            alt={category.name} 
            className="max-w-full max-h-full object-contain"
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

      {/* Optional Device Count */}
      {deviceCount !== undefined && deviceCount > 0 && (
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

export default CategoryButton;
