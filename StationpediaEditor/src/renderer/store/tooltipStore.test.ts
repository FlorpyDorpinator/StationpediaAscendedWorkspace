/**
 * Tests for tooltip store
 */
import { describe, it, expect, beforeEach } from 'vitest';
import { useTooltipStore } from '@renderer/store/tooltipStore';
import type { TooltipDefinition } from '@models/tooltipModel';

describe('tooltipStore', () => {
  beforeEach(() => {
    useTooltipStore.getState().reset();
  });

  describe('initial state', () => {
    it('should have empty collection on init', () => {
      expect(useTooltipStore.getState().collection.tooltips.size).toBe(0);
    });

    it('should have null selected tooltip', () => {
      expect(useTooltipStore.getState().selectedTooltip).toBeNull();
    });

    it('should not be dirty on init', () => {
      expect(useTooltipStore.getState().isDirty).toBe(false);
    });
  });

  describe('loadTooltips', () => {
    it('should load tooltips from JSON', () => {
      const json = {
        ItemBattery: 'Battery description',
        ItemCanister: 'Canister description',
      };

      useTooltipStore.getState().loadTooltips(json);
      expect(useTooltipStore.getState().collection.tooltips.size).toBe(2);
    });

    it('should populate categories', () => {
      const json = {
        ItemBattery: 'Battery',
        StructureAutolathe: 'Autolathe',
      };

      useTooltipStore.getState().loadTooltips(json);
      const categories = Array.from(useTooltipStore.getState().collection.categories);
      expect(categories).toContain('Item');
      expect(categories).toContain('Structure');
    });

    it('should clear dirty flag', () => {
      const json = { ItemBattery: 'Battery' };
      useTooltipStore.getState().loadTooltips(json);
      expect(useTooltipStore.getState().isDirty).toBe(false);
    });

    it('should reset selections', () => {
      const json = {
        ItemBattery: 'Battery',
        ItemCanister: 'Canister',
      };

      useTooltipStore.getState().loadTooltips(json);
      useTooltipStore.getState().selectTooltip('ItemBattery');

      const json2 = { ItemGasCanister: 'Gas Canister' };
      useTooltipStore.getState().loadTooltips(json2);

      expect(useTooltipStore.getState().selectedTooltipKey).toBeNull();
      expect(useTooltipStore.getState().selectedTooltip).toBeNull();
    });
  });

  describe('selectTooltip', () => {
    beforeEach(() => {
      const json = {
        ItemBattery: 'Battery',
        ItemCanister: 'Canister',
      };
      useTooltipStore.getState().loadTooltips(json);
    });

    it('should select a tooltip', () => {
      useTooltipStore.getState().selectTooltip('ItemBattery');
      expect(useTooltipStore.getState().selectedTooltipKey).toBe('ItemBattery');
    });

    it('should update selectedTooltip', () => {
      useTooltipStore.getState().selectTooltip('ItemBattery');
      const selected = useTooltipStore.getState().selectedTooltip;
      expect(selected?.key).toBe('ItemBattery');
      expect(selected?.description).toBe('Battery');
    });

    it('should deselect with null', () => {
      useTooltipStore.getState().selectTooltip('ItemBattery');
      useTooltipStore.getState().selectTooltip(null);
      expect(useTooltipStore.getState().selectedTooltip).toBeNull();
    });

    it('should handle non-existent key', () => {
      useTooltipStore.getState().selectTooltip('NonExistent');
      expect(useTooltipStore.getState().selectedTooltip).toBeNull();
    });
  });

  describe('setSearchQuery', () => {
    beforeEach(() => {
      const json = {
        ItemBattery: 'Standard rechargeable power cell',
        ItemCanister: 'Portable gas storage',
        StructureAutolathe: 'Industrial manufacturing',
      };
      useTooltipStore.getState().loadTooltips(json);
    });

    it('should filter by key', () => {
      useTooltipStore.getState().setSearchQuery('Battery');
      const filtered = useTooltipStore.getState().filteredTooltips;
      expect(filtered.length).toBe(1);
      expect(filtered[0].key).toBe('ItemBattery');
    });

    it('should filter by description', () => {
      useTooltipStore.getState().setSearchQuery('power');
      const filtered = useTooltipStore.getState().filteredTooltips;
      expect(filtered.length).toBe(1);
      expect(filtered[0].key).toBe('ItemBattery');
    });

    it('should be case-insensitive', () => {
      useTooltipStore.getState().setSearchQuery('BATTERY');
      const filtered = useTooltipStore.getState().filteredTooltips;
      expect(filtered.length).toBe(1);
    });

    it('should return all on empty query', () => {
      useTooltipStore.getState().setSearchQuery('');
      const filtered = useTooltipStore.getState().filteredTooltips;
      expect(filtered.length).toBe(3);
    });

    it('should combine with category filter', () => {
      useTooltipStore.getState().setSelectedCategory('Item');
      useTooltipStore.getState().setSearchQuery('Battery');
      const filtered = useTooltipStore.getState().filteredTooltips;
      expect(filtered.length).toBe(1);
      expect(filtered[0].key).toBe('ItemBattery');
    });
  });

  describe('setSelectedCategory', () => {
    beforeEach(() => {
      const json = {
        ItemBattery: 'Standard rechargeable power cell',
        ItemCanister: 'Portable gas storage',
        StructureAutolathe: 'Industrial manufacturing',
      };
      useTooltipStore.getState().loadTooltips(json);
    });

    it('should filter by category', () => {
      useTooltipStore.getState().setSelectedCategory('Item');
      const filtered = useTooltipStore.getState().filteredTooltips;
      expect(filtered.length).toBe(2);
      expect(filtered.every((t) => t.category === 'Item')).toBe(true);
    });

    it('should clear filter with null', () => {
      useTooltipStore.getState().setSelectedCategory('Item');
      useTooltipStore.getState().setSelectedCategory(null);
      const filtered = useTooltipStore.getState().filteredTooltips;
      expect(filtered.length).toBe(3);
    });

    it('should combine with search query', () => {
      useTooltipStore.getState().setSearchQuery('storage');
      useTooltipStore.getState().setSelectedCategory('Item');
      const filtered = useTooltipStore.getState().filteredTooltips;
      expect(filtered.length).toBe(1);
      expect(filtered[0].key).toBe('ItemCanister');
    });
  });

  describe('addNewTooltip', () => {
    beforeEach(() => {
      useTooltipStore.getState().loadTooltips({});
    });

    it('should add new tooltip', () => {
      useTooltipStore.getState().addNewTooltip('ItemBattery', 'Battery description');
      expect(useTooltipStore.getState().collection.tooltips.size).toBe(1);
      expect(useTooltipStore.getState().collection.tooltips.get('ItemBattery')).toBeDefined();
    });

    it('should set dirty flag', () => {
      useTooltipStore.getState().addNewTooltip('ItemBattery', 'Battery description');
      expect(useTooltipStore.getState().isDirty).toBe(true);
    });

    it('should auto-detect category', () => {
      useTooltipStore.getState().addNewTooltip('ItemBattery', 'Battery description');
      const tooltip = useTooltipStore.getState().collection.tooltips.get('ItemBattery');
      expect(tooltip?.category).toBe('Item');
    });

    it('should prevent duplicate keys', () => {
      useTooltipStore.getState().addNewTooltip('ItemBattery', 'Battery description');
      const initialSize = useTooltipStore.getState().collection.tooltips.size;

      useTooltipStore.getState().addNewTooltip('ItemBattery', 'New description');
      expect(useTooltipStore.getState().collection.tooltips.size).toBe(initialSize);
    });
  });

  describe('updateTooltipContent', () => {
    beforeEach(() => {
      useTooltipStore.getState().loadTooltips({
        ItemBattery: 'Original description',
      });
    });

    it('should update tooltip description', () => {
      useTooltipStore.getState().updateTooltipContent('ItemBattery', 'New description');
      const tooltip = useTooltipStore.getState().collection.tooltips.get('ItemBattery');
      expect(tooltip?.description).toBe('New description');
    });

    it('should set dirty flag', () => {
      useTooltipStore.getState().updateTooltipContent('ItemBattery', 'New description');
      expect(useTooltipStore.getState().isDirty).toBe(true);
    });

    it('should update selectedTooltip if selected', () => {
      useTooltipStore.getState().selectTooltip('ItemBattery');
      useTooltipStore.getState().updateTooltipContent('ItemBattery', 'New description');
      expect(useTooltipStore.getState().selectedTooltip?.description).toBe('New description');
    });

    it('should handle non-existent key gracefully', () => {
      const initial = useTooltipStore.getState().collection.tooltips.size;
      useTooltipStore.getState().updateTooltipContent('NonExistent', 'Description');
      expect(useTooltipStore.getState().collection.tooltips.size).toBe(initial);
    });
  });

  describe('deleteTooltip', () => {
    beforeEach(() => {
      useTooltipStore.getState().loadTooltips({
        ItemBattery: 'Battery',
        ItemCanister: 'Canister',
      });
    });

    it('should delete tooltip', () => {
      useTooltipStore.getState().deleteTooltip('ItemBattery');
      expect(useTooltipStore.getState().collection.tooltips.has('ItemBattery')).toBe(false);
    });

    it('should set dirty flag', () => {
      useTooltipStore.getState().deleteTooltip('ItemBattery');
      expect(useTooltipStore.getState().isDirty).toBe(true);
    });

    it('should deselect if deleted tooltip was selected', () => {
      useTooltipStore.getState().selectTooltip('ItemBattery');
      useTooltipStore.getState().deleteTooltip('ItemBattery');
      expect(useTooltipStore.getState().selectedTooltip).toBeNull();
    });

    it('should keep other selections intact', () => {
      useTooltipStore.getState().selectTooltip('ItemCanister');
      useTooltipStore.getState().deleteTooltip('ItemBattery');
      expect(useTooltipStore.getState().selectedTooltipKey).toBe('ItemCanister');
    });

    it('should handle non-existent key gracefully', () => {
      const initial = useTooltipStore.getState().collection.tooltips.size;
      useTooltipStore.getState().deleteTooltip('NonExistent');
      expect(useTooltipStore.getState().collection.tooltips.size).toBe(initial);
    });
  });

  describe('clearDirty', () => {
    it('should clear dirty flag', () => {
      useTooltipStore.getState().loadTooltips({});
      useTooltipStore.getState().addNewTooltip('ItemBattery', 'Battery');
      expect(useTooltipStore.getState().isDirty).toBe(true);

      useTooltipStore.getState().clearDirty();
      expect(useTooltipStore.getState().isDirty).toBe(false);
    });
  });

  describe('getJSON', () => {
    it('should serialize collection to JSON', () => {
      useTooltipStore.getState().loadTooltips({
        ItemBattery: 'Battery description',
        ItemCanister: 'Canister description',
      });

      const json = useTooltipStore.getState().getJSON();
      expect(json.ItemBattery).toBe('Battery description');
      expect(json.ItemCanister).toBe('Canister description');
    });

    it('should preserve order from collection', () => {
      const original = {
        ItemBattery: 'Battery',
        ItemCanister: 'Canister',
        ItemFuel: 'Fuel',
      };

      useTooltipStore.getState().loadTooltips(original);
      const json = useTooltipStore.getState().getJSON();

      expect(Object.keys(json).length).toBe(3);
    });
  });

  describe('reset', () => {
    it('should reset all state', () => {
      useTooltipStore.getState().loadTooltips({
        ItemBattery: 'Battery',
      });
      useTooltipStore.getState().selectTooltip('ItemBattery');
      useTooltipStore.getState().setSearchQuery('test');

      useTooltipStore.getState().reset();

      expect(useTooltipStore.getState().collection.tooltips.size).toBe(0);
      expect(useTooltipStore.getState().selectedTooltip).toBeNull();
      expect(useTooltipStore.getState().searchQuery).toBe('');
      expect(useTooltipStore.getState().isDirty).toBe(false);
    });
  });
});
