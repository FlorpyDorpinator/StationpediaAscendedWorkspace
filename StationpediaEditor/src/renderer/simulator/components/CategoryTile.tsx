/**
 * CategoryTile component
 * Displays a category with icon, name, and device count badge
 * Uses actual game assets with 9-slice borders
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

// Import game UI assets for 9-slice borders
import dialogBg from '../assets/dialog-bg.png';

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
};

export interface CategoryTileProps {
  category: Category;
  deviceCount: number;
  onClick: (categoryId: string) => void;
}

export const CategoryTile: React.FC<CategoryTileProps> = ({
  category,
  deviceCount,
  onClick,
}) => {
  const iconSrc = CATEGORY_ICONS[category.id];

  // 9-slice border style using game dialog-bg.png asset
  const tileStyle: React.CSSProperties = {
    borderImage: `url(${dialogBg}) 18 fill / 18px / 0 stretch`,
    borderStyle: 'solid',
    borderWidth: '18px',
  };

  return (
    <button
      onClick={() => onClick(category.id)}
      className="relative p-4 text-left transition-all hover:brightness-110"
      style={tileStyle}
    >
      {/* Icon */}
      <div className="w-16 h-16 mb-4 flex items-center justify-center">
        {iconSrc ? (
          <img 
            src={iconSrc} 
            alt={category.name} 
            className="max-w-full max-h-full object-contain"
          />
        ) : (
          <span className="text-5xl">📦</span>
        )}
      </div>

      {/* Category Name */}
      <h3 className="text-lg font-semibold text-stationpedia-text mb-2">
        {category.name}
      </h3>

      {/* Description */}
      <p className="text-sm text-stationpedia-text-muted mb-4">
        {category.description}
      </p>

      {/* Device Count Badge */}
      <div className="flex justify-between items-center">
        <span className="text-xs text-stationpedia-text-muted">
          {deviceCount === 1 ? '1 item' : `${deviceCount} items`}
        </span>
        <span className="inline-flex items-center justify-center w-6 h-6 rounded-full bg-stationpedia-accent text-white text-xs font-bold">
          {deviceCount}
        </span>
      </div>
    </button>
  );
};

export default CategoryTile;
