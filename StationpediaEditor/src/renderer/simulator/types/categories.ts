/**
 * Category system types and definitions
 * Defines all in-game Stationpedia categories matching SPDAOverride.xml
 */

export type CategoryId =
  | 'ores'
  | 'ingots'
  | 'fabricators'
  | 'structure-kits'
  | 'gases'
  | 'reagents'
  | 'atmospherics'
  | 'electronics'
  | 'logic-devices'
  | 'structures'
  | 'organics'
  | 'rockets'
  | 'genetics'
  | 'trading'
  | 'hand-tools'
  | 'cartridges'
  | 'personal'
  | 'furniture'
  | 'import-export';

export interface Category {
  id: CategoryId;
  name: string;
  description: string;
  order: number;
  icon?: string;
  linkedPageKey?: string; // For linking to the game's page
}

/**
 * All categories in game order matching SPDAOverride.xml HomePageButtonsOverride
 */
export const CATEGORIES: Category[] = [
  {
    id: 'ores',
    name: 'Ores',
    description: 'Raw ore materials',
    order: 1,
    linkedPageKey: 'OrePage',
  },
  {
    id: 'ingots',
    name: 'Ingots',
    description: 'Processed metal ingots',
    order: 2,
    linkedPageKey: 'IngotPage',
  },
  {
    id: 'fabricators',
    name: 'Fabricators',
    description: 'Devices that fabricate items',
    order: 3,
    linkedPageKey: 'FabricatorPage',
  },
  {
    id: 'structure-kits',
    name: 'Structure Kits',
    description: 'Kits for building structures',
    order: 4,
    linkedPageKey: 'KitPage',
  },
  {
    id: 'gases',
    name: 'Gases',
    description: 'Gaseous materials and containers',
    order: 5,
    linkedPageKey: 'GasPage',
  },
  {
    id: 'reagents',
    name: 'Reagents',
    description: 'Chemical reagents and compounds',
    order: 6,
    linkedPageKey: 'ReagentPage',
  },
  {
    id: 'atmospherics',
    name: 'Atmospherics',
    description: 'Atmospheric control devices',
    order: 7,
    linkedPageKey: 'AtmosphericPage',
  },
  {
    id: 'electronics',
    name: 'Electronics',
    description: 'Electronic components and devices',
    order: 8,
    linkedPageKey: 'ElectronicPage',
  },
  {
    id: 'logic-devices',
    name: 'Logic Devices',
    description: 'Logic circuits and control devices',
    order: 9,
    linkedPageKey: 'LogicUnitPage',
  },
  {
    id: 'structures',
    name: 'Structures',
    description: 'Static structures and building blocks',
    order: 10,
    linkedPageKey: 'StructurePage',
  },
  {
    id: 'organics',
    name: 'Organics and Food',
    description: 'Organic materials and food items',
    order: 11,
    linkedPageKey: 'OrganicPage',
  },
  {
    id: 'rockets',
    name: 'Rockets',
    description: 'Rocket components and propulsion',
    order: 12,
    linkedPageKey: 'AutomatedRocketPage',
  },
  {
    id: 'genetics',
    name: 'Genetics',
    description: 'Genetic and DNA-related items',
    order: 13,
    linkedPageKey: 'GeneticsPage',
  },
  {
    id: 'trading',
    name: 'Trading',
    description: 'Trading and commerce devices',
    order: 14,
    linkedPageKey: 'TradingPage',
  },
  {
    id: 'hand-tools',
    name: 'Hand Tools',
    description: 'Manual tools and equipment',
    order: 15,
    linkedPageKey: 'ToolPage',
  },
  {
    id: 'cartridges',
    name: 'Cartridges',
    description: 'Tablet cartridges and data cards',
    order: 16,
    linkedPageKey: 'CartridgePage',
  },
  {
    id: 'personal',
    name: 'Personal',
    description: 'Personal equipment and clothing',
    order: 17,
    linkedPageKey: 'ClothingPage',
  },
  {
    id: 'furniture',
    name: 'Furniture',
    description: 'Furniture and decorative items',
    order: 18,
    linkedPageKey: 'FurniturePage',
  },
  {
    id: 'import-export',
    name: 'Import/Export',
    description: 'Chutes and cargo handling',
    order: 19,
    linkedPageKey: 'ImportExportPage',
  },
];
