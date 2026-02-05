/**
 * Type definitions for Stationpedia data structures
 * Based on the game's Stationpedia.json format
 */

// ============================================
// Core Page Types from Stationpedia.json
// ============================================

export interface BuildStateTool {
  PrefabName: string;
  Quantity?: number;
  IsTool?: boolean;
}

export interface BuildState {
  Tool?: BuildStateTool[];
  ToolExit?: BuildStateTool[];
}

export interface StructureInfo {
  SmallGrid: boolean;
  BuildStates?: BuildState[];
}

export interface SlotInfo {
  SlotClass: string;
  StringHash: number;
  StringKey: string;
  SlotName: string;
}

export interface SlotInsert {
  SlotName: string;
  SlotType: string;
  SlotIndex: string;
}

export interface LogicInsert {
  LogicName: string;
  LogicAccessTypes: string;
}

export interface LogicSlotInsert {
  LogicName: string;
  LogicAccessTypes: string;
}

export interface ModeInsert {
  ModeName: string;
  ModeValue: string;
}

export interface ConnectionInsert {
  LogicName: string;
  LogicAccessTypes: string;
}

export interface ConstructedByKit {
  PrefabName: string;
  PrefabHash: number;
}

export interface LogicInfo {
  [key: string]: unknown;
}

export interface GasInfo {
  SpecificHeat?: number;
  MolarMass?: number;
  LiquidBoilingPoint?: number;
  FreezeTemperature?: number;
  [key: string]: unknown;
}

export interface NutritionInfo {
  FoodQuality?: number;
  Hydration?: number;
  Nutrition?: number;
  [key: string]: unknown;
}

/**
 * Core Stationpedia page structure from Stationpedia.json
 */
export interface StationpediaPage {
  Key: string;
  Title: string;
  Description: string;
  PrefabName: string;
  PrefabHash: number;
  BasePowerDraw: string | null;
  MaxPressure: string | null;
  GrowthTime: string | null;
  SlotInserts: SlotInsert[];
  LogicInsert: LogicInsert[];
  LogicSlotInsert: LogicSlotInsert[];
  ModeInsert: ModeInsert[];
  ConnectionInsert: ConnectionInsert[];
  ConstructedByKits: ConstructedByKit[];
  Slots: SlotInfo[];
  Structure?: StructureInfo;
  LogicInfo?: LogicInfo;
  GasInfo?: GasInfo;
  NutritionInfo?: NutritionInfo;
  StackSize?: number;
  // Additional optional fields
  CustomSprite?: string;
  Category?: string;
  SubCategory?: string;
}

/**
 * Root structure of Stationpedia.json
 */
export interface StationpediaData {
  version: string;
  pages: StationpediaPage[];
}

// ============================================
// Mod Override Types from descriptions.json
// ============================================

export interface OperationalDetailChild {
  title: string;
  tocId?: string;
  collapsible?: boolean;
  description?: string;
  imageFile?: string;
  items?: string[];
  steps?: string[];
  children?: OperationalDetailChild[];
}

export interface OperationalDetail {
  title: string;
  tocId?: string;
  collapsible?: boolean;
  description?: string;
  imageFile?: string;
  items?: string[];
  steps?: string[];
  children?: OperationalDetailChild[];
}

export interface DeviceOverride {
  deviceKey: string;
  displayName?: string;
  pageDescriptionPrepend?: string;
  pageDescriptionAppend?: string;
  pageDescriptionReplace?: string;
  generateToc?: boolean;
  tocTitle?: string;
  tocFlat?: boolean;
  operationalDetailsTitleColor?: string;
  operationalDetailsBackgroundColor?: string;
  OperationalDetails?: OperationalDetail[];
  logicDescriptions?: Record<string, string>;
  slotDescriptions?: Record<string, string>;
  modeDescriptions?: Record<string, string>;
  connectionDescriptions?: Record<string, string>;
}

export interface GenericDescriptions {
  slots: Record<string, string>;
  versions: Record<string, string>;
  logic: Record<string, string>;
  connections: Record<string, string>;
}

/**
 * Root structure of descriptions.json (mod overrides)
 */
export interface ModDescriptionsData {
  $schema?: string;
  genericDescriptions: GenericDescriptions;
  devices: DeviceOverride[];
}

// ============================================
// Merged/Resolved Types for UI
// ============================================

/**
 * A fully resolved device page combining base game data + mod overrides
 */
export interface ResolvedDevicePage {
  // Identity
  key: string;
  prefabName: string;
  prefabHash: number;
  
  // Display
  title: string;
  description: string;
  thumbnailPath: string | null;
  
  // Power & Resources
  basePowerDraw: string | null;
  maxPressure: string | null;
  growthTime: string | null;
  stackSize: number | null;
  
  // Slots
  slots: SlotInfo[];
  slotInserts: SlotInsert[];
  
  // Logic
  logicTypes: ResolvedLogicType[];
  logicSlotTypes: ResolvedLogicSlotType[];
  
  // Modes & Connections
  modes: ModeInsert[];
  connections: ResolvedConnection[];
  
  // Construction
  structure: StructureInfo | null;
  constructedByKits: ConstructedByKit[];
  
  // Specialized
  gasInfo: GasInfo | null;
  nutritionInfo: NutritionInfo | null;
  
  // Mod Overrides
  hasModOverrides: boolean;
  operationalDetails: OperationalDetail[];
  generateToc: boolean;
  tocTitle: string | null;
  tocFlat: boolean;
  prependDescription: string | null;
  appendDescription: string | null;
}

export interface ResolvedLogicType {
  name: string;
  rawName: string;
  accessTypes: string;
  description: string | null;
}

export interface ResolvedLogicSlotType {
  name: string;
  rawName: string;
  slotIndices: string;
  description: string | null;
}

export interface ResolvedConnection {
  name: string;
  index: string;
  description: string | null;
}

// ============================================
// Category Types
// ============================================

export interface StationpediaCategory {
  id: string;
  key: string;
  name: string;
  iconPrefab: string;
  sortOrder: number;
}

export const STATIONPEDIA_CATEGORIES: StationpediaCategory[] = [
  { id: 'ores', key: 'OreCategoryKey', name: 'Ores', iconPrefab: 'ItemCopperOre', sortOrder: 1 },
  { id: 'ingots', key: 'IngotsCategoryKey', name: 'Ingots', iconPrefab: 'ItemStelliteIngot', sortOrder: 2 },
  { id: 'fabricators', key: 'FabricatorCategoryKey', name: 'Fabricators', iconPrefab: 'ToolPrinterMod', sortOrder: 3 },
  { id: 'structure-kits', key: 'ConstructionKitsCategoryKey', name: 'Structure Kits', iconPrefab: 'ItemKitSleeper', sortOrder: 4 },
  { id: 'gases', key: 'GasesCategoryKey', name: 'Gases', iconPrefab: 'ItemGasFilterOxygenM', sortOrder: 5 },
  { id: 'reagents', key: 'ReagentsCategoryKey', name: 'Reagents', iconPrefab: 'ItemReagentMix', sortOrder: 6 },
  { id: 'atmospherics', key: 'AtmopshericsCategoryKey', name: 'Atmospherics', iconPrefab: 'ItemKitPipe', sortOrder: 7 },
  { id: 'electronics', key: 'PowerCategoryKey', name: 'Electronics', iconPrefab: 'ItemCableCoil', sortOrder: 8 },
  { id: 'logic-devices', key: 'LogicCategoryKey', name: 'Logic Devices', iconPrefab: 'ItemIntegratedCircuit10', sortOrder: 9 },
  { id: 'structures', key: 'StructureCategoryKey', name: 'Structures', iconPrefab: 'ItemIronFrames', sortOrder: 10 },
  { id: 'organics', key: 'OrganicsCategoryKey', name: 'Organics and Food', iconPrefab: 'ItemPumpkin', sortOrder: 11 },
  { id: 'rockets', key: 'RocketCategoryKey', name: 'Rockets', iconPrefab: 'StructurePumpedLiquidEngine', sortOrder: 12 },
  { id: 'genetics', key: 'GeneticsCategoryKey', name: 'Genetics', iconPrefab: 'AppliancePlantGeneticAnalyzer', sortOrder: 13 },
  { id: 'trading', key: 'TradingCategoryKey', name: 'Trading', iconPrefab: 'StructureSatelliteDish', sortOrder: 14 },
  { id: 'hand-tools', key: 'HandToolsCategoryKey', name: 'Hand Tools', iconPrefab: 'ItemDrill', sortOrder: 15 },
  { id: 'cartridges', key: 'CartidgeCategoryKey', name: 'Cartridges', iconPrefab: 'CartridgeNetworkAnalyser', sortOrder: 16 },
  { id: 'personal', key: 'PersonalCategoryKey', name: 'Personal', iconPrefab: 'ItemSpaceHelmet', sortOrder: 17 },
  { id: 'furniture', key: 'FurnitureCategoryKey', name: 'Furniture', iconPrefab: 'StructureChair', sortOrder: 18 },
  { id: 'import-export', key: 'ImportExportCategoryKey', name: 'Import/Export', iconPrefab: 'StructureChuteJunction', sortOrder: 19 },
];

// Path to AssetRipper thumbnails
export const THUMBNAIL_BASE_PATH = 'c:/Dev/StationpediaAscendedWorkspace/AssetRipperFiles/ExportedProject/Assets/Resources/ui/thumbnails';

/**
 * Get the raw file path for a thumbnail (for IPC file operations)
 */
export function getThumbnailPath(prefabName: string): string {
  return `${THUMBNAIL_BASE_PATH}/${prefabName}.png`;
}

/**
 * Get the thumbnail URL for use in img src (uses custom protocol)
 */
export function getThumbnailUrl(prefabName: string): string {
  return `local-asset://${THUMBNAIL_BASE_PATH}/${prefabName}.png`;
}
