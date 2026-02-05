/**
 * Tests for tooltip model
 */
import { describe, it, expect } from 'vitest';
import {
  extractCategory,
  createTooltipDefinition,
  createEmptyTooltipCollection,
  parseTooltipsFromJSON,
  serializeTooltipsToJSON,
  getTooltipsByCategory,
  searchTooltips,
  addTooltip,
  removeTooltip,
  updateTooltip,
  getSortedCategories,
} from '@models/tooltipModel';

describe('tooltipModel', () => {
  describe('extractCategory', () => {
    it('should extract category from ItemBattery', () => {
      expect(extractCategory('ItemBattery')).toBe('Item');
    });

    it('should extract category from StructureAutolathe', () => {
      expect(extractCategory('StructureAutolathe')).toBe('Structure');
    });

    it('should extract category from KitBasic', () => {
      expect(extractCategory('KitBasic')).toBe('Kit');
    });

    it('should return Other for non-standard keys', () => {
      expect(extractCategory('unknown')).toBe('Other');
    });

    it('should handle single letter prefixes', () => {
      expect(extractCategory('AKey')).toBe('A');
    });
  });

  describe('createTooltipDefinition', () => {
    it('should create tooltip with auto-detected category', () => {
      const tooltip = createTooltipDefinition('ItemBattery', 'Battery description');
      expect(tooltip.key).toBe('ItemBattery');
      expect(tooltip.description).toBe('Battery description');
      expect(tooltip.category).toBe('Item');
    });

    it('should allow explicit category override', () => {
      const tooltip = createTooltipDefinition('ItemBattery', 'Battery description', 'Custom');
      expect(tooltip.category).toBe('Custom');
    });
  });

  describe('createEmptyTooltipCollection', () => {
    it('should create empty collection', () => {
      const collection = createEmptyTooltipCollection();
      expect(collection.tooltips.size).toBe(0);
      expect(collection.categories.size).toBe(0);
    });
  });

  describe('parseTooltipsFromJSON', () => {
    it('should parse simple tooltips', () => {
      const json = {
        ItemBattery: 'Battery description',
        ItemCanister: 'Canister description',
      };

      const collection = parseTooltipsFromJSON(json);
      expect(collection.tooltips.size).toBe(2);
      expect(collection.tooltips.get('ItemBattery')?.description).toBe('Battery description');
    });

    it('should auto-detect categories', () => {
      const json = {
        ItemBattery: 'Battery description',
        StructureAutolathe: 'Autolathe description',
      };

      const collection = parseTooltipsFromJSON(json);
      expect(collection.categories.has('Item')).toBe(true);
      expect(collection.categories.has('Structure')).toBe(true);
    });

    it('should handle empty JSON', () => {
      const collection = parseTooltipsFromJSON({});
      expect(collection.tooltips.size).toBe(0);
    });

    it('should ignore non-string values', () => {
      const json = {
        ItemBattery: 'Battery description',
        OtherField: 123,
      };

      const collection = parseTooltipsFromJSON(json as any);
      expect(collection.tooltips.size).toBe(1);
    });
  });

  describe('serializeTooltipsToJSON', () => {
    it('should serialize collection back to JSON', () => {
      const collection = parseTooltipsFromJSON({
        ItemBattery: 'Battery description',
        ItemCanister: 'Canister description',
      });

      const json = serializeTooltipsToJSON(collection);
      expect(json.ItemBattery).toBe('Battery description');
      expect(json.ItemCanister).toBe('Canister description');
    });

    it('should round-trip correctly', () => {
      const original = {
        ItemBattery: 'Battery description',
        StructureAutolathe: 'Autolathe description',
      };

      const collection = parseTooltipsFromJSON(original);
      const json = serializeTooltipsToJSON(collection);

      expect(json).toEqual(original);
    });
  });

  describe('getTooltipsByCategory', () => {
    it('should filter tooltips by category', () => {
      const collection = parseTooltipsFromJSON({
        ItemBattery: 'Battery',
        ItemCanister: 'Canister',
        StructureAutolathe: 'Autolathe',
      });

      const items = getTooltipsByCategory(collection, 'Item');
      expect(items.length).toBe(2);
      expect(items.every((t) => t.category === 'Item')).toBe(true);
    });

    it('should return empty array for non-existent category', () => {
      const collection = parseTooltipsFromJSON({
        ItemBattery: 'Battery',
      });

      const items = getTooltipsByCategory(collection, 'NonExistent');
      expect(items.length).toBe(0);
    });
  });

  describe('searchTooltips', () => {
    const collection = parseTooltipsFromJSON({
      ItemBattery: 'Standard rechargeable power cell',
      ItemGasCanister: 'Portable gas storage container',
      StructureAutolathe: 'Industrial manufacturing machine',
    });

    it('should search by key', () => {
      const results = searchTooltips(collection, 'Battery');
      expect(results.length).toBe(1);
      expect(results[0].key).toBe('ItemBattery');
    });

    it('should search in description', () => {
      const results = searchTooltips(collection, 'power');
      expect(results.length).toBe(1);
      expect(results[0].key).toBe('ItemBattery');
    });

    it('should be case-insensitive', () => {
      const results = searchTooltips(collection, 'BATTERY');
      expect(results.length).toBe(1);
    });

    it('should return all on empty query', () => {
      const results = searchTooltips(collection, '');
      expect(results.length).toBe(3);
    });

    it('should return no results if no match', () => {
      const results = searchTooltips(collection, 'NonExistent');
      expect(results.length).toBe(0);
    });
  });

  describe('addTooltip', () => {
    it('should add new tooltip to collection', () => {
      let collection = createEmptyTooltipCollection();
      const tooltip = createTooltipDefinition('ItemBattery', 'Battery description');

      collection = addTooltip(collection, tooltip);
      expect(collection.tooltips.size).toBe(1);
      expect(collection.tooltips.get('ItemBattery')).toEqual(tooltip);
    });

    it('should add category when tooltip is added', () => {
      let collection = createEmptyTooltipCollection();
      const tooltip = createTooltipDefinition('ItemBattery', 'Battery description');

      collection = addTooltip(collection, tooltip);
      expect(collection.categories.has('Item')).toBe(true);
    });

    it('should not mutate original collection', () => {
      const collection = createEmptyTooltipCollection();
      const tooltip = createTooltipDefinition('ItemBattery', 'Battery description');

      const updated = addTooltip(collection, tooltip);
      expect(collection.tooltips.size).toBe(0);
      expect(updated.tooltips.size).toBe(1);
    });

    it('should overwrite existing tooltip with same key', () => {
      let collection = createEmptyTooltipCollection();
      const tooltip1 = createTooltipDefinition('ItemBattery', 'Description 1');
      const tooltip2 = createTooltipDefinition('ItemBattery', 'Description 2');

      collection = addTooltip(collection, tooltip1);
      collection = addTooltip(collection, tooltip2);

      expect(collection.tooltips.size).toBe(1);
      expect(collection.tooltips.get('ItemBattery')?.description).toBe('Description 2');
    });
  });

  describe('removeTooltip', () => {
    it('should remove tooltip from collection', () => {
      let collection = parseTooltipsFromJSON({
        ItemBattery: 'Battery',
        ItemCanister: 'Canister',
      });

      collection = removeTooltip(collection, 'ItemBattery');
      expect(collection.tooltips.size).toBe(1);
      expect(collection.tooltips.has('ItemBattery')).toBe(false);
    });

    it('should rebuild categories after removal', () => {
      let collection = parseTooltipsFromJSON({
        ItemBattery: 'Battery',
      });

      collection = removeTooltip(collection, 'ItemBattery');
      expect(collection.categories.has('Item')).toBe(false);
    });

    it('should not mutate original collection', () => {
      const collection = parseTooltipsFromJSON({
        ItemBattery: 'Battery',
      });

      const updated = removeTooltip(collection, 'ItemBattery');
      expect(collection.tooltips.size).toBe(1);
      expect(updated.tooltips.size).toBe(0);
    });

    it('should handle non-existent key gracefully', () => {
      const collection = createEmptyTooltipCollection();
      const updated = removeTooltip(collection, 'NonExistent');
      expect(updated.tooltips.size).toBe(0);
    });
  });

  describe('updateTooltip', () => {
    it('should update tooltip description', () => {
      let collection = parseTooltipsFromJSON({
        ItemBattery: 'Old description',
      });

      collection = updateTooltip(collection, 'ItemBattery', {
        description: 'New description',
      });

      expect(collection.tooltips.get('ItemBattery')?.description).toBe('New description');
    });

    it('should not mutate original collection', () => {
      const collection = parseTooltipsFromJSON({
        ItemBattery: 'Description',
      });

      const updated = updateTooltip(collection, 'ItemBattery', {
        description: 'New description',
      });

      expect(collection.tooltips.get('ItemBattery')?.description).toBe('Description');
      expect(updated.tooltips.get('ItemBattery')?.description).toBe('New description');
    });

    it('should handle non-existent key gracefully', () => {
      const collection = createEmptyTooltipCollection();
      const updated = updateTooltip(collection, 'NonExistent', { description: 'New' });
      expect(updated.tooltips.size).toBe(0);
    });
  });

  describe('getSortedCategories', () => {
    it('should return sorted categories', () => {
      const collection = parseTooltipsFromJSON({
        ItemBattery: 'Battery',
        StructureAutolathe: 'Autolathe',
        KitBasic: 'Kit',
      });

      const sorted = getSortedCategories(collection);
      expect(sorted).toEqual(['Item', 'Kit', 'Structure']);
    });

    it('should return empty array for empty collection', () => {
      const collection = createEmptyTooltipCollection();
      const sorted = getSortedCategories(collection);
      expect(sorted).toEqual([]);
    });
  });
});
