/**
 * Validation model types for comprehensive content validation
 */

/**
 * Severity level for validation issues
 */
export type ValidationSeverity = 'error' | 'warning' | 'info';

/**
 * Location information for validation errors
 */
export interface ValidationLocation {
  deviceKey?: string;
  field?: string;
  sectionIndex?: number;
  childPath?: number[];
  line?: number;
  column?: number;
}

/**
 * A single validation error with location and context
 */
export interface ValidationError {
  ruleId: string;
  severity: ValidationSeverity;
  message: string;
  location: ValidationLocation;
}

/**
 * Result of validation for a device
 */
export interface ValidationResult {
  deviceKey: string;
  errors: ValidationError[];
  hasErrors: boolean;
  hasWarnings: boolean;
  hasInfo: boolean;
}

/**
 * Validation context containing device and workspace information
 */
export interface ValidationContext {
  device: any; // DeviceDocument
  knownDeviceKeys: Set<string>;
  assetRegistry: Map<string, { path: string; usageCount: number }>;
  workspaceRoot?: string;
}

/**
 * Validation rule definition
 */
export interface ValidationRule {
  id: string;
  name: string;
  description: string;
  severity: ValidationSeverity;
  validate: (context: ValidationContext) => ValidationError[];
}

/**
 * Store for managing validation state across the editor
 */
export interface ValidationStoreState {
  results: Map<string, ValidationResult>;
  isRunning: boolean;
  lastRunTime?: number;
  selectedSeverities: Set<ValidationSeverity>;
  
  // Actions
  setResult: (deviceKey: string, result: ValidationResult) => void;
  getResult: (deviceKey: string) => ValidationResult | undefined;
  getErrorCount: (severity?: ValidationSeverity) => number;
  getDevicesWithErrors: () => string[];
  setRunning: (running: boolean) => void;
  clear: () => void;
  toggleSeverityFilter: (severity: ValidationSeverity) => void;
  getFilteredErrors: () => ValidationError[];
}

/**
 * Asset registry entry
 */
export interface AssetRegistryEntry {
  path: string;
  filename: string;
  relativePath: string;
  size?: number;
  mtime?: number;
  usageCount: number;
}

/**
 * Common validation rule IDs
 */
export const VALIDATION_RULE_IDS = {
  // TMP (Text Markup) issues
  UNCLOSED_TAG: 'unclosed-tag',
  INVALID_COLOR: 'invalid-color',
  INVALID_TMP_TAG: 'invalid-tmp-tag',
  
  // Link validation
  BROKEN_LINK: 'broken-link',
  SELF_LINK: 'self-link',
  
  // Asset validation
  MISSING_ASSET: 'missing-asset',
  UNUSED_ASSET: 'unused-asset',
  
  // Field validation
  EMPTY_TITLE: 'empty-title',
  MISSING_DEVICE_KEY: 'missing-device-key',
  MISSING_DISPLAY_NAME: 'missing-display-name',
  
  // Structure validation
  DUPLICATE_TOC_ID: 'duplicate-toc-id',
  EMPTY_CONTENT: 'empty-content',
  INVALID_SECTION_STRUCTURE: 'invalid-section-structure',
} as const;
