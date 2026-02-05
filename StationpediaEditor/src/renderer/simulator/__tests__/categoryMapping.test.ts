/**
 * Tests for category mapping system
 */
import { describe, it, expect } from 'vitest';
import {
  getCategoryForDevice,
  DEVICE_CATEGORY_MAP,
  CATEGORY_PREFIXES,
} from '../data/categoryMapping';
import { getCategorizedDevices, getDeviceCountByCategory, searchDevicesInCategory } from '../utils/categorizeDevices';
import { CATEGORIES } from '../types/categories';
import type { DeviceDescription } from '../../models/stationpedia';
import type { CategoryId } from '../types/categories';

describe('Category Mapping', () => {
  describe('getCategoryForDevice', () => {
    it('should map ThingOreIron to ores category', () => {
      expect(getCategoryForDevice('ThingOreIron')).toBe('ores');
    });

    it('should map ThingOreCoal to ores category', () => {
      expect(getCategoryForDevice('ThingOreCoal')).toBe('ores');
    });

    it('should map ThingIngotSteel to ingots category', () => {
      expect(getCategoryForDevice('ThingIngotSteel')).toBe('ingots');
    });

    it('should map ThingIngotGold to ingots category', () => {
      expect(getCategoryForDevice('ThingIngotGold')).toBe('ingots');
    });

    it('should map Autolathe devices to fabricators', () => {
      expect(getCategoryForDevice('ThingStructureAutolathe')).toBe('fabricators');
    });

    it('should map Fabricator devices to fabricators', () => {
      expect(getCategoryForDevice('ThingStructureFabricator')).toBe('fabricators');
    });

    it('should map Printer devices to fabricators', () => {
      expect(getCategoryForDevice('ThingStructurePrinter')).toBe('fabricators');
    });

    it('should map Kit devices to structure-kits', () => {
      expect(getCategoryForDevice('ThingStructureFrameKit')).toBe('structure-kits');
    });

    it('should map gas canister to gases', () => {
      expect(getCategoryForDevice('ThingCanister')).toBe('gases');
    });

    it('should map ThingGasOxygen to gases', () => {
      expect(getCategoryForDevice('ThingGasOxygen')).toBe('gases');
    });

    it('should map Reagent devices to reagents', () => {
      expect(getCategoryForDevice('ThingReagentPlasma')).toBe('reagents');
    });

    it('should map Pipe devices to atmospherics', () => {
      expect(getCategoryForDevice('ThingStructurePipe')).toBe('atmospherics');
    });

    it('should map Vent devices to atmospherics', () => {
      expect(getCategoryForDevice('ThingStructureVent')).toBe('atmospherics');
    });

    it('should map Filter devices to atmospherics', () => {
      expect(getCategoryForDevice('ThingStructureFilter')).toBe('atmospherics');
    });

    it('should map Pump devices to atmospherics', () => {
      expect(getCategoryForDevice('ThingStructurePump')).toBe('atmospherics');
    });

    it('should map Circuit devices to electronics', () => {
      expect(getCategoryForDevice('ThingStructureCircuit')).toBe('electronics');
    });

    it('should map Chip devices to electronics', () => {
      expect(getCategoryForDevice('ThingStructureChip')).toBe('electronics');
    });

    it('should map Cable devices to electronics', () => {
      expect(getCategoryForDevice('ThingStructureCable')).toBe('electronics');
    });

    it('should map Logic devices to logic-devices', () => {
      expect(getCategoryForDevice('ThingStructureLogic')).toBe('logic-devices');
    });

    it('should map IC devices to logic-devices', () => {
      expect(getCategoryForDevice('ThingStructureIC')).toBe('logic-devices');
    });

    it('should map Sensor devices to logic-devices', () => {
      expect(getCategoryForDevice('ThingStructureSensor')).toBe('logic-devices');
    });

    it('should map Plant devices to organics', () => {
      expect(getCategoryForDevice('ThingPlantWheat')).toBe('organics');
    });

    it('should map Seed devices to organics', () => {
      expect(getCategoryForDevice('ThingSeedCorn')).toBe('organics');
    });

    it('should map Food devices to organics', () => {
      expect(getCategoryForDevice('ThingFoodBread')).toBe('organics');
    });

    it('should map Organic devices to organics', () => {
      expect(getCategoryForDevice('ThingOrganicMeat')).toBe('organics');
    });

    it('should map Rocket devices to rockets', () => {
      expect(getCategoryForDevice('ThingRocketFuel')).toBe('rockets');
    });

    it('should map Thruster devices to rockets', () => {
      expect(getCategoryForDevice('ThingThrusterLarge')).toBe('rockets');
    });

    it('should map Fuel devices to rockets', () => {
      expect(getCategoryForDevice('ThingFuelRod')).toBe('rockets');
    });

    it('should map Genetic devices to genetics', () => {
      expect(getCategoryForDevice('ThingGeneticSample')).toBe('genetics');
    });

    it('should map DNA devices to genetics', () => {
      expect(getCategoryForDevice('ThingDNASequence')).toBe('genetics');
    });

    it('should map Trader devices to trading', () => {
      expect(getCategoryForDevice('ThingTraderTerminal')).toBe('trading');
    });

    it('should map Vending machines to trading', () => {
      expect(getCategoryForDevice('ThingVendingMachine')).toBe('trading');
    });

    it('should map Trade devices to trading', () => {
      expect(getCategoryForDevice('ThingTradeBot')).toBe('trading');
    });

    it('should fallback ThingStructure devices to structures', () => {
      expect(getCategoryForDevice('ThingStructureWall')).toBe('structures');
    });

    it('should fallback unknown devices to structures', () => {
      expect(getCategoryForDevice('ThingUnknownDevice')).toBe('structures');
    });

    it('should handle manual overrides from DEVICE_CATEGORY_MAP', () => {
      // Assuming there might be manual overrides, this test ensures they work
      const manualKeys = Object.keys(DEVICE_CATEGORY_MAP);
      if (manualKeys.length > 0) {
        const testKey = manualKeys[0];
        expect(getCategoryForDevice(testKey)).toBe(DEVICE_CATEGORY_MAP[testKey]);
      }
    });
  });

  describe('Category Definitions', () => {
    it('should have all 19 categories defined', () => {
      expect(CATEGORIES).toHaveLength(19);
    });

    it('should have categories in correct order', () => {
      const categoryIds = CATEGORIES.map(c => c.id);
      expect(categoryIds).toEqual([
        'ores',
        'ingots',
        'fabricators',
        'structure-kits',
        'gases',
        'reagents',
        'atmospherics',
        'electronics',
        'logic-devices',
        'structures',
        'organics',
        'rockets',
        'genetics',
        'trading',
        'hand-tools',
        'cartridges',
        'personal',
        'furniture',
        'import-export',
      ]);
    });

    it('each category should have required properties', () => {
      CATEGORIES.forEach(category => {
        expect(category.id).toBeDefined();
        expect(typeof category.id).toBe('string');
        expect(category.name).toBeDefined();
        expect(typeof category.name).toBe('string');
        expect(category.description).toBeDefined();
        expect(typeof category.description).toBe('string');
        expect(category.order).toBeDefined();
        expect(typeof category.order).toBe('number');
      });
    });
  });

  describe('getCategorizedDevices', () => {
    const mockDevices: DeviceDescription[] = [
      { deviceKey: 'ThingOreIron', displayName: 'Iron Ore' },
      { deviceKey: 'ThingIngotSteel', displayName: 'Steel Ingot' },
      { deviceKey: 'ThingStructureAutolathe', displayName: 'Autolathe' },
      { deviceKey: 'ThingStructureWall', displayName: 'Wall' },
      { deviceKey: 'ThingPlantWheat', displayName: 'Wheat' },
    ];

    it('should categorize devices correctly', () => {
      const categorized = getCategorizedDevices(mockDevices);

      expect(categorized.get('ores')).toEqual([mockDevices[0]]);
      expect(categorized.get('ingots')).toEqual([mockDevices[1]]);
      expect(categorized.get('fabricators')).toEqual([mockDevices[2]]);
      expect(categorized.get('structures')).toEqual([mockDevices[3]]);
      expect(categorized.get('organics')).toEqual([mockDevices[4]]);
    });

    it('should return all 19 categories', () => {
      const categorized = getCategorizedDevices(mockDevices);
      expect(categorized.size).toBe(19);
    });

    it('should handle empty array', () => {
      const categorized = getCategorizedDevices([]);
      expect(categorized.size).toBe(19);
      categorized.forEach(devices => {
        expect(devices).toHaveLength(0);
      });
    });

    it('should handle multiple devices in same category', () => {
      const devices: DeviceDescription[] = [
        { deviceKey: 'ThingOreIron', displayName: 'Iron Ore' },
        { deviceKey: 'ThingOreCoal', displayName: 'Coal Ore' },
        { deviceKey: 'ThingOreGold', displayName: 'Gold Ore' },
      ];
      const categorized = getCategorizedDevices(devices);
      expect(categorized.get('ores')).toHaveLength(3);
    });
  });

  describe('getDeviceCountByCategory', () => {
    const mockDevices: DeviceDescription[] = [
      { deviceKey: 'ThingOreIron', displayName: 'Iron Ore' },
      { deviceKey: 'ThingOreCoal', displayName: 'Coal Ore' },
      { deviceKey: 'ThingIngotSteel', displayName: 'Steel Ingot' },
      { deviceKey: 'ThingStructureAutolathe', displayName: 'Autolathe' },
      { deviceKey: 'ThingStructureWall', displayName: 'Wall' },
    ];

    it('should count devices correctly by category', () => {
      const counts = getDeviceCountByCategory(mockDevices);

      expect(counts.get('ores')).toBe(2);
      expect(counts.get('ingots')).toBe(1);
      expect(counts.get('fabricators')).toBe(1);
      expect(counts.get('structures')).toBe(1);
    });

    it('should return map with all 19 categories', () => {
      const counts = getDeviceCountByCategory(mockDevices);
      expect(counts.size).toBe(19);
    });

    it('should return 0 for empty categories', () => {
      const counts = getDeviceCountByCategory(mockDevices);
      expect(counts.get('gases')).toBe(0);
      expect(counts.get('reagents')).toBe(0);
      expect(counts.get('rockets')).toBe(0);
    });
  });

  describe('searchDevicesInCategory', () => {
    const mockDevices: DeviceDescription[] = [
      { deviceKey: 'ThingOreIron', displayName: 'Iron Ore' },
      { deviceKey: 'ThingOreCoal', displayName: 'Coal Ore' },
      { deviceKey: 'ThingOreGold', displayName: 'Gold Ore' },
      { deviceKey: 'ThingIngotSteel', displayName: 'Steel Ingot' },
    ];

    it('should search by deviceKey in category', () => {
      const results = searchDevicesInCategory(mockDevices, 'ores', 'Iron');
      expect(results).toHaveLength(1);
      expect(results[0].deviceKey).toBe('ThingOreIron');
    });

    it('should search by displayName in category', () => {
      const results = searchDevicesInCategory(mockDevices, 'ores', 'Coal');
      expect(results).toHaveLength(1);
      expect(results[0].deviceKey).toBe('ThingOreCoal');
    });

    it('should be case-insensitive', () => {
      const results = searchDevicesInCategory(mockDevices, 'ores', 'gold');
      expect(results).toHaveLength(1);
      expect(results[0].deviceKey).toBe('ThingOreGold');
    });

    it('should return empty array if no matches in category', () => {
      const results = searchDevicesInCategory(mockDevices, 'gases', 'Iron');
      expect(results).toEqual([]);
    });

    it('should return empty array for non-existent category', () => {
      const results = searchDevicesInCategory(mockDevices, 'nonexistent' as CategoryId, 'Iron');
      expect(results).toEqual([]);
    });

    it('should return all devices in category when query is empty', () => {
      const results = searchDevicesInCategory(mockDevices, 'ores', '');
      expect(results).toHaveLength(3);
    });
  });
});
