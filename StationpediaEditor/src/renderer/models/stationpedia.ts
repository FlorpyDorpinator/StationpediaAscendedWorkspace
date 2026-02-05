/**
 * Stationpedia type definitions
 * Matches the structure from descriptions.json
 */

export interface DeviceDescription {
  deviceKey: string;
  displayName?: string;
  pageDescription?: string;
  generateToc?: boolean;
  tocTitle?: string;
  tocFlat?: boolean;
  operationalDetails?: OperationalDetail[];
  logicDescriptions?: Record<string, LogicDescription>;
  slots?: SlotDefinition[];
  prefabs?: string[];
  tag?: string;
  // Allow unknown fields to round-trip
  [key: string]: unknown;
}

export interface OperationalDetail {
  title: string;
  tocId?: string;
  collapsible?: boolean;
  description?: string;
  items?: string[];
  steps?: string[];
  imageFile?: string;
  children?: OperationalDetail[];
  // Allow unknown fields
  [key: string]: unknown;
}

export interface LogicDescription {
  dataType: string;
  range: string;
  description: string;
  [key: string]: unknown;
}

export interface SlotDefinition {
  slotNumber: number;
  slotType: string;
  description?: string;
  [key: string]: unknown;
}

export interface ValidationError {
  type: 'error' | 'warning' | 'info';
  line?: number;
  message: string;
  field?: string;
  deviceKey?: string;
}

export interface EditorState {
  currentDevice: DeviceDescription | null;
  devices: DeviceDescription[];
  isDirty: boolean;
  validationErrors: ValidationError[];
}

// Content type categories
export type ContentType = 'device' | 'guide' | 'mechanic';

export interface ContentItem {
  type: ContentType;
  data: DeviceDescription;
  filePath?: string;
}

// Rich text AST types (TipTap-compatible)
export interface RichTextNode {
  type: string;
  attrs?: Record<string, unknown>;
  content?: RichTextNode[];
  marks?: RichTextMark[];
  text?: string;
}

export interface RichTextMark {
  type: string;
  attrs?: Record<string, unknown>;
}

// Link target for internal Stationpedia links
export interface LinkTarget {
  key: string;
  displayName: string;
  type: ContentType;
}

