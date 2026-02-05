/**
 * Complete content model for devices, guides, and mechanics
 * Supports full Stationpedia Ascended feature set with unknown field preservation
 */

/**
 * Represents a single row in a markdown-style table.
 * The first row is treated as the header row with bold text.
 */
export interface TableRow {
  cells: string[];
}

/**
 * Operational detail for device documentation
 * Supports hierarchical sections with various content types
 */
export interface OperationalDetail {
  title: string;
  tocId?: string;
  collapsible?: boolean;
  description?: string; // Rich text (TMP format, parsed to AST)
  items?: string[]; // Bullet list
  steps?: string[]; // Numbered list
  children?: OperationalDetail[]; // Nested sections
  imageFile?: string;
  backgroundColor?: string;
  youtubeUrl?: string;
  youtubeLabel?: string;
  videoFile?: string;
  table?: TableRow[]; // Markdown-style table (first row = headers)
  // Preserve unknown fields
  [key: string]: unknown;
}

/**
 * Logic description for device logic ports
 */
export interface LogicDescription {
  dataType: string; // e.g., 'Boolean', 'Float', 'Integer', 'Hash'
  range: string; // e.g., '0-1', 'Any', specific range
  description: string;
  // Preserve unknown fields
  [key: string]: unknown;
}

/**
 * Mode description for device operating modes
 */
export interface ModeDescription {
  modeValue?: string;
  description?: string;
  // Preserve unknown fields
  [key: string]: unknown;
}

/**
 * Slot description for device slots
 */
export interface SlotDescription {
  slotNumber?: number;
  slotType?: string;
  description?: string;
  // Preserve unknown fields
  [key: string]: unknown;
}

/**
 * Version description (manufacturing tiers, variants, etc.)
 */
export interface VersionDescription {
  versionValue?: string;
  description?: string;
  // Preserve unknown fields
  [key: string]: unknown;
}

/**
 * Memory description (for IC10 chips and similar)
 */
export interface MemoryDescription {
  address?: number;
  description?: string;
  size?: number;
  // Preserve unknown fields
  [key: string]: unknown;
}

/**
 * Complete device document with ALL fields
 * Supports full round-trip preservation
 */
export interface DeviceDocument {
  // Core identification
  deviceKey: string;
  displayName?: string | null;

  // Description fields
  pageDescription?: string;
  pageDescriptionPrepend?: string;
  pageDescriptionAppend?: string;

  // Operational details section
  operationalDetails?: OperationalDetail[];
  OperationalDetails?: OperationalDetail[]; // Handle old format
  operationalDetailsTitleColor?: string;
  operationalDetailsBackgroundColor?: string;
  generateToc?: boolean;
  tocTitle?: string;
  tocFlat?: boolean; // If true, TOC entries are flat (no nested indentation)

  // Descriptive sections
  logicDescriptions?: Record<string, LogicDescription>;
  modeDescriptions?: Record<string, ModeDescription>;
  slotDescriptions?: Record<string, SlotDescription>;
  versionDescriptions?: Record<string, VersionDescription>;
  memoryDescriptions?: Record<string, MemoryDescription>;
  connectionDescriptions?: Record<string, string>;

  // Legacy aliases
  slots?: number;
  prefabs?: string[];
  tag?: string;
  title?: string;

  // Preserve unknown fields for round-trip compatibility
  [key: string]: unknown;
}

/**
 * Guide document for custom guides in descriptions.json
 * These appear under the "Guides" button in Stationpedia
 */
export interface GuideDocument {
  // Core identification (guides use guideKey, but we add deviceKey for compatibility)
  guideKey: string;
  deviceKey?: string; // Optional alias for guideKey for compatibility
  displayName?: string | null;

  // Description field
  pageDescription?: string;

  // Operational details section (same format as devices)
  operationalDetails?: OperationalDetail[];
  OperationalDetails?: OperationalDetail[]; // Handle old format
  operationalDetailsTitleColor?: string;
  operationalDetailsBackgroundColor?: string;
  generateToc?: boolean;
  tocTitle?: string;
  tocFlat?: boolean; // If true, TOC entries are flat (no nested indentation)

  // Guide-specific fields
  buttonColor?: string; // "blue", "orange", "green", or hex color
  sortOrder?: number; // Lower numbers appear first
  flatStructure?: boolean; // If true, renders sections directly without "Operational Details" wrapper

  // Preserve unknown fields for round-trip compatibility
  [key: string]: unknown;
}

/**
 * Complete workspace model
 */
export interface WorkspaceModel {
  devices: DeviceDocument[];
  guides?: GuideDocument[];
  mechanics?: GuideDocument[];
  genericDescriptions?: Record<string, unknown>;
  metadata?: Record<string, unknown>;
}

/**
 * Normalize table format from object {headers, rows} to array [TableRow]
 * The mod expects table to be List<TableRow> where first row contains headers
 */
function normalizeTable(table: any): TableRow[] | undefined {
  if (!table) return undefined;
  
  // Already correct format: array of TableRow
  if (Array.isArray(table)) {
    return table;
  }
  
  // Wrong format: object with headers and rows
  if (table.headers && table.rows) {
    const normalizedTable: TableRow[] = [];
    // First row is the headers
    normalizedTable.push({ cells: table.headers });
    // Rest are data rows
    for (const row of table.rows) {
      if (row.cells) {
        normalizedTable.push({ cells: row.cells });
      }
    }
    return normalizedTable;
  }
  
  return undefined;
}

/**
 * Recursively normalize all tables in operational details
 */
function normalizeOperationalDetailTables(details: OperationalDetail[]): OperationalDetail[] {
  return details.map(detail => ({
    ...detail,
    table: normalizeTable(detail.table),
    children: detail.children ? normalizeOperationalDetailTables(detail.children) : undefined,
  }));
}

/**
 * Normalize a document's tables (devices, guides, mechanics)
 */
export function normalizeDocumentTables<T extends { operationalDetails?: OperationalDetail[]; OperationalDetails?: OperationalDetail[] }>(doc: T): T {
  const normalized = { ...doc };
  
  // Handle both field name variants
  const details = (doc as any).OperationalDetails || doc.operationalDetails;
  if (details && Array.isArray(details)) {
    normalized.operationalDetails = normalizeOperationalDetailTables(details);
    // Also update OperationalDetails if present
    if ((doc as any).OperationalDetails) {
      (normalized as any).OperationalDetails = normalized.operationalDetails;
    }
  }
  
  return normalized;
}

/**
 * Helper to normalize field names (OperationalDetails → operationalDetails)
 */
export function normalizeDeviceFields(doc: DeviceDocument): DeviceDocument {
  const normalized = { ...doc };

  // Normalize OperationalDetails to operationalDetails
  if ((doc as any).OperationalDetails && !normalized.operationalDetails) {
    normalized.operationalDetails = (doc as any).OperationalDetails;
  }

  // Remove the old field name
  if ((normalized as any).OperationalDetails) {
    delete (normalized as any).OperationalDetails;
  }
  
  // Normalize tables
  if (normalized.operationalDetails) {
    normalized.operationalDetails = normalizeOperationalDetailTables(normalized.operationalDetails);
  }

  return normalized;
}

/**
 * Helper to denormalize field names (operationalDetails → OperationalDetails)
 * Used when serializing back to JSON to match original format
 */
export function denormalizeDeviceFields(doc: DeviceDocument): any {
  const denormalized = { ...doc };

  // Convert operationalDetails back to OperationalDetails
  if (denormalized.operationalDetails) {
    (denormalized as any).OperationalDetails = denormalized.operationalDetails;
    delete denormalized.operationalDetails;
  }

  return denormalized;
}

/**
 * Create an empty device document
 */
export function createEmptyDevice(deviceKey: string): DeviceDocument {
  return {
    deviceKey,
  };
}

/**
 * Create an empty workspace model
 */
export function createEmptyWorkspace(): WorkspaceModel {
  return {
    devices: [],
  };
}
