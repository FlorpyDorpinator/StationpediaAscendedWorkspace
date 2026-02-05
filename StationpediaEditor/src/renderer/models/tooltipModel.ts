/**
 * Tooltip model for generic descriptions
 * Tooltips are simple key-value pairs shown when hovering over items
 */

/**
 * Single tooltip definition
 */
export interface TooltipDefinition {
  key: string; // e.g., "ItemBattery", "StructureAutolathe"
  description: string; // TMP formatted text
  category?: string; // Auto-detected from key prefix (Item, Structure, Kit, etc.)
}

/**
 * Collection of tooltips with category organization
 */
export interface TooltipCollection {
  tooltips: Map<string, TooltipDefinition>;
  categories: Set<string>;
}

/**
 * Extract category from tooltip key
 * e.g., "ItemBattery" → "Item", "StructureAutolathe" → "Structure", "AKey" → "A"
 */
export function extractCategory(key: string): string {
  // Match capital letter followed by lowercase letters, or just a capital letter
  const match = key.match(/^([A-Z][a-z]*)/);
  return match ? match[1] : 'Other';
}

/**
 * Create empty tooltip collection
 */
export function createEmptyTooltipCollection(): TooltipCollection {
  return {
    tooltips: new Map(),
    categories: new Set(),
  };
}

/**
 * Create tooltip definition
 */
export function createTooltipDefinition(
  key: string,
  description: string,
  category?: string
): TooltipDefinition {
  return {
    key,
    description,
    category: category || extractCategory(key),
  };
}

/**
 * Parse tooltips from generic descriptions JSON object
 * Handles nested structure: { slots: {...}, logic: {...}, connections: {...}, properties: {...}, versions: {...} }
 */
export function parseTooltipsFromJSON(
  genericDescriptions: Record<string, unknown>
): TooltipCollection {
  const collection = createEmptyTooltipCollection();

  if (!genericDescriptions) {
    return collection;
  }

  // Handle nested structure with category keys
  const categoryKeys = ['slots', 'logic', 'connections', 'properties', 'versions', 'memory'];
  
  Object.entries(genericDescriptions).forEach(([key, value]) => {
    if (categoryKeys.includes(key) && typeof value === 'object' && value !== null) {
      // Nested category: { slots: { "SuitSlot": "description" } }
      const categoryName = key.charAt(0).toUpperCase() + key.slice(1);
      Object.entries(value as Record<string, unknown>).forEach(([tooltipKey, description]) => {
        if (typeof description === 'string') {
          const tooltip = createTooltipDefinition(tooltipKey, description, categoryName);
          collection.tooltips.set(tooltipKey, tooltip);
          collection.categories.add(categoryName);
        } else if (typeof description === 'object' && description !== null) {
          // Handle complex objects like properties: { Flashpoint: { type: "...", description: "..." } }
          const desc = (description as any).description || JSON.stringify(description);
          const tooltip = createTooltipDefinition(tooltipKey, desc, categoryName);
          collection.tooltips.set(tooltipKey, tooltip);
          collection.categories.add(categoryName);
        }
      });
    } else if (typeof value === 'string') {
      // Flat format: { "ItemBattery": "description" }
      const tooltip = createTooltipDefinition(key, value);
      collection.tooltips.set(key, tooltip);
      if (tooltip.category) {
        collection.categories.add(tooltip.category);
      }
    }
  });

  return collection;
}

/**
 * Serialize tooltip collection back to JSON object
 */
export function serializeTooltipsToJSON(collection: TooltipCollection): Record<string, string> {
  const json: Record<string, string> = {};
  collection.tooltips.forEach((tooltip) => {
    json[tooltip.key] = tooltip.description;
  });
  return json;
}

/**
 * Get tooltips by category
 */
export function getTooltipsByCategory(
  collection: TooltipCollection,
  category: string
): TooltipDefinition[] {
  return Array.from(collection.tooltips.values()).filter((t) => t.category === category);
}

/**
 * Search tooltips by key or description
 */
export function searchTooltips(
  collection: TooltipCollection,
  query: string
): TooltipDefinition[] {
  const lowerQuery = query.toLowerCase();
  return Array.from(collection.tooltips.values()).filter(
    (t) =>
      t.key.toLowerCase().includes(lowerQuery) ||
      t.description.toLowerCase().includes(lowerQuery)
  );
}

/**
 * Add tooltip to collection
 */
export function addTooltip(
  collection: TooltipCollection,
  tooltip: TooltipDefinition
): TooltipCollection {
  const updated = {
    tooltips: new Map(collection.tooltips),
    categories: new Set(collection.categories),
  };
  updated.tooltips.set(tooltip.key, tooltip);
  if (tooltip.category) {
    updated.categories.add(tooltip.category);
  }
  return updated;
}

/**
 * Remove tooltip from collection
 */
export function removeTooltip(
  collection: TooltipCollection,
  key: string
): TooltipCollection {
  const updated = {
    tooltips: new Map(collection.tooltips),
    categories: new Set(collection.categories),
  };
  updated.tooltips.delete(key);

  // Rebuild categories
  updated.categories.clear();
  updated.tooltips.forEach((tooltip) => {
    if (tooltip.category) {
      updated.categories.add(tooltip.category);
    }
  });

  return updated;
}

/**
 * Update tooltip in collection
 */
export function updateTooltip(
  collection: TooltipCollection,
  key: string,
  updates: Partial<TooltipDefinition>
): TooltipCollection {
  const existing = collection.tooltips.get(key);
  if (!existing) {
    return collection;
  }

  const updated = {
    tooltips: new Map(collection.tooltips),
    categories: new Set(collection.categories),
  };

  const merged = { ...existing, ...updates };
  updated.tooltips.set(key, merged);

  // Rebuild categories
  updated.categories.clear();
  updated.tooltips.forEach((tooltip) => {
    if (tooltip.category) {
      updated.categories.add(tooltip.category);
    }
  });

  return updated;
}

/**
 * Get sorted categories
 */
export function getSortedCategories(collection: TooltipCollection): string[] {
  return Array.from(collection.categories).sort();
}
