/**
 * Stationpedia Data Service
 * Loads and merges Stationpedia.json (base) with descriptions.json (mod overrides)
 * The mod's descriptions.json takes precedence when it has data for a device
 */

import type {
  StationpediaData,
  StationpediaPage,
  ModDescriptionsData,
  DeviceOverride,
  ResolvedDevicePage,
  ResolvedLogicType,
  ResolvedLogicSlotType,
  ResolvedConnection,
  GenericDescriptions,
  STATIONPEDIA_CATEGORIES,
  StationpediaCategory,
} from '../types/stationpediaTypes';
import { getThumbnailPath } from '../types/stationpediaTypes';

// File paths
const STATIONPEDIA_JSON_PATH = 'c:/Dev/StationpediaAscendedWorkspace/Stationeers/Stationpedia/Stationpedia.json';
const DESCRIPTIONS_JSON_PATH = 'c:/Dev/StationpediaAscendedWorkspace/StationpediaAscended/mod/descriptions.json';

/**
 * Cache for loaded data
 */
let stationpediaCache: StationpediaData | null = null;
let descriptionsCache: ModDescriptionsData | null = null;
let pagesMapCache: Map<string, StationpediaPage> | null = null;
let overridesMapCache: Map<string, DeviceOverride> | null = null;

/**
 * Strip Unity rich text tags from a string
 */
function stripRichTextTags(text: string): string {
  if (!text) return '';
  // Remove <link=...>...</link>, <color=...>...</color>, <N:EN:...> patterns
  return text
    .replace(/<link=[^>]*>/g, '')
    .replace(/<\/link>/g, '')
    .replace(/<color=[^>]*>/g, '')
    .replace(/<\/color>/g, '')
    .replace(/<N:EN:([^>]*)>/g, '$1')
    .replace(/<b>/g, '')
    .replace(/<\/b>/g, '')
    .replace(/\\n/g, '\n');
}

/**
 * Extract the logic type name from a rich text string
 * e.g., "<link=LogicTypePower><color=orange>Power</color></link>" -> "Power"
 */
function extractLogicName(richText: string): { name: string; rawName: string } {
  const match = richText.match(/<link=LogicType([^>]*)>.*?<color=[^>]*>([^<]*)<\/color>/);
  if (match) {
    return { name: match[2], rawName: match[1] };
  }
  const slotMatch = richText.match(/<link=LogicSlotType([^>]*)>.*?<color=[^>]*>([^<]*)<\/color>/);
  if (slotMatch) {
    return { name: slotMatch[2], rawName: slotMatch[1] };
  }
  return { name: stripRichTextTags(richText), rawName: richText };
}

/**
 * Load the base Stationpedia.json file
 */
export async function loadStationpediaData(): Promise<StationpediaData> {
  if (stationpediaCache) {
    return stationpediaCache;
  }

  try {
    // Must use electron's IPC to read local files
    if (!window.electronAPI?.readFile) {
      throw new Error('Electron API not available - cannot read local files');
    }
    
    const result = await window.electronAPI.readFile(STATIONPEDIA_JSON_PATH);
    if (!result.success || !result.data) {
      throw new Error(result.error || 'Failed to read Stationpedia.json');
    }
    stationpediaCache = JSON.parse(result.data) as StationpediaData;
    
    // Build the pages map
    pagesMapCache = new Map();
    for (const page of stationpediaCache.pages) {
      pagesMapCache.set(page.Key, page);
    }
    
    console.log(`Loaded ${stationpediaCache.pages.length} pages from Stationpedia.json`);
    return stationpediaCache;
  } catch (error) {
    console.error('Failed to load Stationpedia.json:', error);
    throw error;
  }
}

/**
 * Load the mod's descriptions.json file
 */
export async function loadDescriptionsData(): Promise<ModDescriptionsData> {
  if (descriptionsCache) {
    return descriptionsCache;
  }

  try {
    // Must use electron's IPC to read local files
    if (!window.electronAPI?.readFile) {
      throw new Error('Electron API not available - cannot read local files');
    }
    
    const result = await window.electronAPI.readFile(DESCRIPTIONS_JSON_PATH);
    if (!result.success || !result.data) {
      throw new Error(result.error || 'Failed to read descriptions.json');
    }
    descriptionsCache = JSON.parse(result.data) as ModDescriptionsData;
    
    // Build the overrides map
    overridesMapCache = new Map();
    for (const device of descriptionsCache.devices) {
      overridesMapCache.set(device.deviceKey, device);
    }
    
    console.log(`Loaded ${descriptionsCache.devices.length} device overrides from descriptions.json`);
    return descriptionsCache;
  } catch (error) {
    console.error('Failed to load descriptions.json:', error);
    throw error;
  }
}

/**
 * Get a device override from descriptions.json
 */
export function getDeviceOverride(deviceKey: string): DeviceOverride | null {
  return overridesMapCache?.get(deviceKey) ?? null;
}

/**
 * Get the generic descriptions (for logic types, slots, etc.)
 */
export function getGenericDescriptions(): GenericDescriptions | null {
  return descriptionsCache?.genericDescriptions ?? null;
}

/**
 * Resolve a full device page by merging base data with mod overrides
 */
export function resolveDevicePage(deviceKey: string): ResolvedDevicePage | null {
  const basePage = pagesMapCache?.get(deviceKey);
  if (!basePage) {
    return null;
  }
  
  const override = overridesMapCache?.get(deviceKey);
  const genericDescriptions = descriptionsCache?.genericDescriptions;
  
  // Resolve logic types with descriptions
  const logicTypes: ResolvedLogicType[] = basePage.LogicInsert.map(logic => {
    const { name, rawName } = extractLogicName(logic.LogicName);
    let description: string | null = null;
    
    // Check device-specific override first
    if (override?.logicDescriptions?.[name]) {
      description = override.logicDescriptions[name];
    } else if (override?.logicDescriptions?.[rawName]) {
      description = override.logicDescriptions[rawName];
    } else if (genericDescriptions?.logic[name]) {
      description = genericDescriptions.logic[name];
    } else if (genericDescriptions?.logic[rawName]) {
      description = genericDescriptions.logic[rawName];
    }
    
    return {
      name,
      rawName,
      accessTypes: logic.LogicAccessTypes,
      description,
    };
  });
  
  // Resolve logic slot types
  const logicSlotTypes: ResolvedLogicSlotType[] = basePage.LogicSlotInsert.map(slot => {
    const { name, rawName } = extractLogicName(slot.LogicName);
    let description: string | null = null;
    
    if (override?.slotDescriptions?.[name]) {
      description = override.slotDescriptions[name];
    } else if (genericDescriptions?.slots[name]) {
      description = genericDescriptions.slots[name];
    }
    
    return {
      name,
      rawName,
      slotIndices: slot.LogicAccessTypes,
      description,
    };
  });
  
  // Resolve connections
  const connections: ResolvedConnection[] = basePage.ConnectionInsert.map(conn => {
    let description: string | null = null;
    
    if (override?.connectionDescriptions?.[conn.LogicName]) {
      description = override.connectionDescriptions[conn.LogicName];
    } else if (genericDescriptions?.connections[conn.LogicName]) {
      description = genericDescriptions.connections[conn.LogicName];
    }
    
    return {
      name: conn.LogicName,
      index: conn.LogicAccessTypes,
      description,
    };
  });
  
  // Build description with prepend/append/replace
  let description = stripRichTextTags(basePage.Description);
  if (override?.pageDescriptionReplace) {
    description = override.pageDescriptionReplace;
  } else {
    if (override?.pageDescriptionPrepend) {
      description = override.pageDescriptionPrepend + description;
    }
    if (override?.pageDescriptionAppend) {
      description = description + override.pageDescriptionAppend;
    }
  }
  
  // Determine thumbnail path
  let thumbnailPath: string | null = null;
  if (basePage.PrefabName) {
    thumbnailPath = getThumbnailPath(basePage.PrefabName);
  }
  
  return {
    key: basePage.Key,
    prefabName: basePage.PrefabName,
    prefabHash: basePage.PrefabHash,
    title: override?.displayName ?? stripRichTextTags(basePage.Title),
    description,
    thumbnailPath,
    basePowerDraw: basePage.BasePowerDraw ? stripRichTextTags(basePage.BasePowerDraw) : null,
    maxPressure: basePage.MaxPressure ? stripRichTextTags(basePage.MaxPressure) : null,
    growthTime: basePage.GrowthTime ? stripRichTextTags(basePage.GrowthTime) : null,
    stackSize: basePage.StackSize ?? null,
    slots: basePage.Slots,
    slotInserts: basePage.SlotInserts,
    logicTypes,
    logicSlotTypes,
    modes: basePage.ModeInsert,
    connections,
    structure: basePage.Structure ?? null,
    constructedByKits: basePage.ConstructedByKits,
    gasInfo: basePage.GasInfo ?? null,
    nutritionInfo: basePage.NutritionInfo ?? null,
    hasModOverrides: !!override,
    operationalDetails: override?.OperationalDetails ?? [],
    generateToc: override?.generateToc ?? false,
    tocTitle: override?.tocTitle ?? null,
    tocFlat: override?.tocFlat ?? false,
    prependDescription: override?.pageDescriptionPrepend ?? null,
    appendDescription: override?.pageDescriptionAppend ?? null,
  };
}

/**
 * Get all pages from Stationpedia.json
 */
export function getAllPages(): StationpediaPage[] {
  return stationpediaCache?.pages ?? [];
}

/**
 * Get pages by category (based on prefab naming patterns)
 */
export function getPagesByCategory(categoryId: string): StationpediaPage[] {
  const allPages = getAllPages();
  
  // Category matching based on prefab patterns
  const categoryPatterns: Record<string, (page: StationpediaPage) => boolean> = {
    'ores': (p) => p.PrefabName.includes('Ore') && !p.PrefabName.includes('OreScanner'),
    'ingots': (p) => p.PrefabName.includes('Ingot'),
    'fabricators': (p) => p.PrefabName.includes('Printer') || p.PrefabName.includes('Autolathe') || p.PrefabName.includes('Fabricator'),
    'structure-kits': (p) => p.PrefabName.startsWith('ItemKit') || p.PrefabName.startsWith('Kit'),
    'gases': (p) => p.PrefabName.includes('GasFilter') || p.Key.includes('Gas') && !p.PrefabName.includes('GasCanister'),
    'reagents': (p) => p.PrefabName.includes('Reagent'),
    'atmospherics': (p) => p.PrefabName.includes('Pipe') || p.PrefabName.includes('Vent') || p.PrefabName.includes('Pump') || p.PrefabName.includes('Filter') || p.PrefabName.includes('Condenser') || p.PrefabName.includes('Evaporator'),
    'electronics': (p) => p.PrefabName.includes('Cable') || p.PrefabName.includes('Battery') || p.PrefabName.includes('Generator') || p.PrefabName.includes('Solar') || p.PrefabName.includes('Transformer') || p.PrefabName.includes('APC'),
    'logic-devices': (p) => p.PrefabName.includes('Logic') || p.PrefabName.includes('Circuit') || p.PrefabName.includes('Console') || p.PrefabName.includes('Dial') || p.PrefabName.includes('Switch') || p.PrefabName.includes('Sensor'),
    'structures': (p) => p.PrefabName.startsWith('Structure') && !p.PrefabName.includes('Chair') && !p.PrefabName.includes('Table') && !p.PrefabName.includes('Bed'),
    'organics': (p) => p.PrefabName.includes('Plant') || p.PrefabName.includes('Seed') || p.PrefabName.includes('Food') || p.PrefabName.includes('Egg') || p.NutritionInfo != null,
    'rockets': (p) => p.PrefabName.includes('Rocket') || p.PrefabName.includes('Engine') || p.PrefabName.includes('Fuselage') || p.PrefabName.includes('Landing'),
    'genetics': (p) => p.PrefabName.includes('Genetic'),
    'trading': (p) => p.PrefabName.includes('Trader') || p.PrefabName.includes('Vending') || p.PrefabName.includes('SatelliteDish'),
    'hand-tools': (p) => p.PrefabName.includes('Tool') || p.PrefabName.includes('Drill') || p.PrefabName.includes('Welder') || p.PrefabName.includes('Wrench') || p.PrefabName.includes('Crowbar'),
    'cartridges': (p) => p.PrefabName.includes('Cartridge'),
    'personal': (p) => p.PrefabName.includes('Suit') || p.PrefabName.includes('Helmet') || p.PrefabName.includes('Jetpack') || p.PrefabName.includes('Backpack'),
    'furniture': (p) => p.PrefabName.includes('Chair') || p.PrefabName.includes('Table') || p.PrefabName.includes('Bed') || p.PrefabName.includes('Locker') || p.PrefabName.includes('Desk'),
    'import-export': (p) => p.PrefabName.includes('Chute') || p.PrefabName.includes('Conveyor'),
  };
  
  const matcher = categoryPatterns[categoryId];
  if (!matcher) {
    return [];
  }
  
  return allPages.filter(matcher);
}

/**
 * Search pages by title or prefab name
 */
export function searchPages(query: string): StationpediaPage[] {
  const allPages = getAllPages();
  const lowerQuery = query.toLowerCase();
  
  return allPages.filter(page => {
    const title = stripRichTextTags(page.Title).toLowerCase();
    const prefab = page.PrefabName.toLowerCase();
    return title.includes(lowerQuery) || prefab.includes(lowerQuery);
  });
}

/**
 * Get the count of pages per category
 */
export function getCategoryCounts(): Map<string, number> {
  const counts = new Map<string, number>();
  const categories = ['ores', 'ingots', 'fabricators', 'structure-kits', 'gases', 'reagents', 
    'atmospherics', 'electronics', 'logic-devices', 'structures', 'organics', 'rockets',
    'genetics', 'trading', 'hand-tools', 'cartridges', 'personal', 'furniture', 'import-export'];
  
  for (const cat of categories) {
    counts.set(cat, getPagesByCategory(cat).length);
  }
  
  return counts;
}

/**
 * Initialize the data service - load both files
 */
export async function initializeStationpediaService(): Promise<void> {
  await Promise.all([
    loadStationpediaData(),
    loadDescriptionsData(),
  ]);
}

/**
 * Clear all caches (for reloading)
 */
export function clearCaches(): void {
  stationpediaCache = null;
  descriptionsCache = null;
  pagesMapCache = null;
  overridesMapCache = null;
}
