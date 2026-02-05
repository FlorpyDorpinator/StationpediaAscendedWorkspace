/**
 * Device to category mapping system
 * Handles categorization of devices with both manual overrides and prefix-based matching
 */

import type { CategoryId } from '../types/categories';

/**
 * Manual category overrides for specific device keys
 * These take precedence over prefix-based matching
 */
export const DEVICE_CATEGORY_MAP: Record<string, CategoryId> = {
  // Add manual overrides here as needed
  // Example: 'SpecialDevice': 'structures'
};

/**
 * Prefix patterns for auto-categorization
 * Ordered by specificity - more specific patterns should come first
 * Each pattern is checked against the deviceKey using includes()
 */
export const CATEGORY_PREFIXES: Array<[string, CategoryId]> = [
  // Ores
  ['ThingOre', 'ores'],

  // Ingots
  ['ThingIngot', 'ingots'],

  // Fabricators (check before generic structures)
  ['Autolathe', 'fabricators'],
  ['Fabricator', 'fabricators'],
  ['Printer', 'fabricators'],

  // Structure Kits
  ['Kit', 'structure-kits'],

  // Gases
  ['ThingGas', 'gases'],
  ['Canister', 'gases'],

  // Reagents
  ['Reagent', 'reagents'],

  // Atmospherics (check before generic structures)
  ['Pipe', 'atmospherics'],
  ['Vent', 'atmospherics'],
  ['Filter', 'atmospherics'],
  ['Pump', 'atmospherics'],

  // Electronics (check before generic structures)
  ['Circuit', 'electronics'],
  ['Chip', 'electronics'],
  ['Cable', 'electronics'],

  // Logic Devices (check before generic structures)
  ['Logic', 'logic-devices'],
  ['IC', 'logic-devices'],
  ['Sensor', 'logic-devices'],

  // Organics and Food
  ['Plant', 'organics'],
  ['Seed', 'organics'],
  ['Food', 'organics'],
  ['Organic', 'organics'],

  // Rockets
  ['Rocket', 'rockets'],
  ['Thruster', 'rockets'],
  ['Fuel', 'rockets'],

  // Genetics
  ['Genetic', 'genetics'],
  ['DNA', 'genetics'],

  // Trading
  ['Trader', 'trading'],
  ['Vending', 'trading'],
  ['Trade', 'trading'],

  // Default structures (this should be last specific match)
  ['ThingStructure', 'structures'],
];

/**
 * Get the category for a device based on its key
 * Checks manual map first, then tries prefix matching, then defaults to 'structures'
 *
 * @param deviceKey - The device key (e.g., 'ThingOreIron', 'ThingStructureAutolathe')
 * @returns The CategoryId for this device
 */
export function getCategoryForDevice(deviceKey: string): CategoryId {
  // Check manual overrides first
  if (deviceKey in DEVICE_CATEGORY_MAP) {
    return DEVICE_CATEGORY_MAP[deviceKey];
  }

  // Check prefix patterns
  for (const [pattern, categoryId] of CATEGORY_PREFIXES) {
    if (deviceKey.includes(pattern)) {
      return categoryId;
    }
  }

  // Default fallback
  return 'structures';
}
