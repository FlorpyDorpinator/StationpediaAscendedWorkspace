/**
 * Tooltip store using Zustand
 * Manages tooltip editing state, search, filter, and dirty tracking
 */
import { create } from 'zustand';
import type {
  TooltipCollection,
  TooltipDefinition,
} from '@models/tooltipModel';
import {
  createEmptyTooltipCollection,
  parseTooltipsFromJSON,
  serializeTooltipsToJSON,
  addTooltip,
  removeTooltip,
  updateTooltip,
  searchTooltips,
  getTooltipsByCategory,
  createTooltipDefinition,
  extractCategory,
} from '@models/tooltipModel';

export interface TooltipStoreState {
  // State
  collection: TooltipCollection;
  selectedTooltipKey: string | null;
  searchQuery: string;
  selectedCategory: string | null;
  isDirty: boolean;

  // Computed
  selectedTooltip: TooltipDefinition | null;
  filteredTooltips: TooltipDefinition[];

  // Actions
  loadTooltips: (genericDescriptions: Record<string, unknown>) => void;
  selectTooltip: (key: string | null) => void;
  setSearchQuery: (query: string) => void;
  setSelectedCategory: (category: string | null) => void;
  addNewTooltip: (key: string, description: string) => void;
  updateTooltipContent: (key: string, description: string) => void;
  deleteTooltip: (key: string) => void;
  clearDirty: () => void;
  reset: () => void;

  // Serialization
  getJSON: () => Record<string, string>;
}

export const useTooltipStore = create<TooltipStoreState>((set, get) => ({
  collection: createEmptyTooltipCollection(),
  selectedTooltipKey: null,
  searchQuery: '',
  selectedCategory: null,
  isDirty: false,
  selectedTooltip: null,
  filteredTooltips: [],

  loadTooltips: (genericDescriptions) => {
    const collection = parseTooltipsFromJSON(genericDescriptions);
    set((state) => ({
      collection,
      isDirty: false,
      selectedTooltipKey: null,
      selectedTooltip: null,
      searchQuery: '',
      selectedCategory: null,
      filteredTooltips: Array.from(collection.tooltips.values()),
    }));
  },

  selectTooltip: (key) => {
    set((state) => {
      const tooltip = key ? state.collection.tooltips.get(key) : null;
      return {
        selectedTooltipKey: key,
        selectedTooltip: tooltip || null,
      };
    });
  },

  setSearchQuery: (query) => {
    set((state) => {
      const results = query
        ? searchTooltips(state.collection, query)
        : Array.from(state.collection.tooltips.values());

      return {
        searchQuery: query,
        filteredTooltips: state.selectedCategory
          ? results.filter((t) => t.category === state.selectedCategory)
          : results,
      };
    });
  },

  setSelectedCategory: (category) => {
    set((state) => {
      const allFiltered = state.searchQuery
        ? searchTooltips(state.collection, state.searchQuery)
        : Array.from(state.collection.tooltips.values());

      const filtered = category
        ? allFiltered.filter((t) => t.category === category)
        : allFiltered;

      return {
        selectedCategory: category,
        filteredTooltips: filtered,
      };
    });
  },

  addNewTooltip: (key, description) => {
    set((state) => {
      if (state.collection.tooltips.has(key)) {
        console.warn(`Tooltip with key "${key}" already exists`);
        return state;
      }

      const newTooltip = createTooltipDefinition(key, description);
      const updated = addTooltip(state.collection, newTooltip);

      return {
        collection: updated,
        isDirty: true,
      };
    });
  },

  updateTooltipContent: (key, description) => {
    set((state) => {
      if (!state.collection.tooltips.has(key)) {
        console.warn(`Tooltip with key "${key}" not found`);
        return state;
      }

      const updated = updateTooltip(state.collection, key, {
        description,
      });

      const newSelectedTooltip =
        state.selectedTooltipKey === key
          ? { ...state.selectedTooltip!, description }
          : state.selectedTooltip;

      return {
        collection: updated,
        selectedTooltip: newSelectedTooltip,
        isDirty: true,
      };
    });
  },

  deleteTooltip: (key) => {
    set((state) => {
      if (!state.collection.tooltips.has(key)) {
        console.warn(`Tooltip with key "${key}" not found`);
        return state;
      }

      const updated = removeTooltip(state.collection, key);
      const isSelected = state.selectedTooltipKey === key;

      return {
        collection: updated,
        selectedTooltipKey: isSelected ? null : state.selectedTooltipKey,
        selectedTooltip: isSelected ? null : state.selectedTooltip,
        isDirty: true,
      };
    });
  },

  clearDirty: () => {
    set({
      isDirty: false,
    });
  },

  reset: () => {
    set({
      collection: createEmptyTooltipCollection(),
      selectedTooltipKey: null,
      searchQuery: '',
      selectedCategory: null,
      isDirty: false,
      selectedTooltip: null,
      filteredTooltips: [],
    });
  },

  getJSON: () => {
    return serializeTooltipsToJSON(get().collection);
  },
}));
