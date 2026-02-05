/**
 * JSON Codec: Load/Parse and Save/Serialize workspace JSON
 * Preserves all data through round-trips while normalizing field names
 */

import {
  WorkspaceModel,
  DeviceDocument,
  GuideDocument,
  OperationalDetail,
  normalizeDeviceFields,
  denormalizeDeviceFields,
  createEmptyWorkspace,
} from '../models/contentModel';

/**
 * Parse workspace JSON data into internal model
 * Handles:
 * - Field name normalization (OperationalDetails → operationalDetails)
 * - Unknown field preservation
 * - Nested structure flattening for easier handling
 *
 * @param json Raw JSON object from descriptions.json
 * @returns Normalized workspace model
 */
export function parseWorkspaceJSON(json: any): WorkspaceModel {
  const workspace = createEmptyWorkspace();

  // Preserve generic descriptions and metadata
  if (json.genericDescriptions) {
    workspace.genericDescriptions = json.genericDescriptions;
  }

  // Preserve any top-level metadata
  Object.keys(json).forEach(key => {
    if (key !== 'devices' && key !== 'genericDescriptions') {
      if (!workspace.metadata) {
        workspace.metadata = {};
      }
      workspace.metadata[key] = json[key];
    }
  });

  // Parse devices
  if (json.devices && Array.isArray(json.devices)) {
    workspace.devices = json.devices.map((deviceJson: any) => parseDevice(deviceJson));
  }

  // Parse guides if present
  if (json.guides && Array.isArray(json.guides)) {
    workspace.guides = json.guides.map((guideJson: any) => parseDevice(guideJson));
  }

  // Parse mechanics if present
  if (json.mechanics && Array.isArray(json.mechanics)) {
    workspace.mechanics = json.mechanics.map((mechJson: any) => parseDevice(mechJson));
  }

  return workspace;
}

/**
 * Parse a single device from JSON
 */
function parseDevice(deviceJson: any): DeviceDocument {
  const device: DeviceDocument = { ...deviceJson };

  // Normalize field names
  normalizeOperationalDetails(device);

  // Parse nested operational details
  if (device.operationalDetails && Array.isArray(device.operationalDetails)) {
    device.operationalDetails = parseOperationalDetails(device.operationalDetails);
  }

  return device;
}

/**
 * Normalize OperationalDetails field name
 */
function normalizeOperationalDetails(doc: DeviceDocument): void {
  const anyDoc = doc as any;
  if (anyDoc.OperationalDetails && !doc.operationalDetails) {
    doc.operationalDetails = anyDoc.OperationalDetails;
  }
  // Keep the original for round-trip if needed
}

/**
 * Parse operational details recursively
 */
function parseOperationalDetails(details: any[]): OperationalDetail[] {
  return details.map(detail => parseOperationalDetail(detail));
}

/**
 * Parse a single operational detail
 */
function parseOperationalDetail(detail: any): OperationalDetail {
  const parsed: OperationalDetail = {
    title: detail.title || '',
    ...detail, // Preserve all fields including unknowns
  };

  // Recursively parse children if present
  if (detail.children && Array.isArray(detail.children)) {
    parsed.children = parseOperationalDetails(detail.children);
  }

  return parsed;
}

/**
 * Serialize workspace model back to JSON
 * Handles:
 * - Field name denormalization (operationalDetails → OperationalDetails)
 * - Unknown field preservation
 * - Structural integrity
 *
 * @param workspace Normalized workspace model
 * @returns JSON object ready for serialization
 */
export function serializeWorkspaceJSON(workspace: WorkspaceModel): any {
  const json: any = {};

  // Restore generic descriptions if present
  if (workspace.genericDescriptions) {
    json.genericDescriptions = workspace.genericDescriptions;
  }

  // Restore top-level metadata
  if (workspace.metadata) {
    Object.keys(workspace.metadata).forEach(key => {
      if (key !== 'devices' && key !== 'genericDescriptions') {
        json[key] = workspace.metadata![key];
      }
    });
  }

  // Serialize devices
  if (workspace.devices && workspace.devices.length > 0) {
    json.devices = workspace.devices.map(device => serializeDevice(device));
  }

  // Serialize guides if present
  if (workspace.guides && workspace.guides.length > 0) {
    json.guides = workspace.guides.map(guide => serializeItem(guide));
  }

  // Serialize mechanics if present
  if (workspace.mechanics && workspace.mechanics.length > 0) {
    json.mechanics = workspace.mechanics.map(mechanic => serializeItem(mechanic));
  }

  return json;
}

/**
 * Serialize a single device, guide, or mechanic to JSON
 */
function serializeItem(item: DeviceDocument | GuideDocument): any {
  const json = { ...item };

  // Denormalize field names (operationalDetails → OperationalDetails)
  if (json.operationalDetails) {
    (json as any).OperationalDetails = serializeOperationalDetails(json.operationalDetails);
    delete json.operationalDetails;
  }

  return json;
}

/**
 * Serialize a single device to JSON (legacy name, delegates to serializeItem)
 */
function serializeDevice(device: DeviceDocument): any {
  return serializeItem(device);
}

/**
 * Serialize operational details recursively
 */
function serializeOperationalDetails(details: OperationalDetail[]): any[] {
  return details.map(detail => serializeOperationalDetail(detail));
}

/**
 * Serialize a single operational detail
 */
function serializeOperationalDetail(detail: OperationalDetail): any {
  const json = { ...detail };

  // Recursively serialize children if present
  if (detail.children && detail.children.length > 0) {
    json.children = serializeOperationalDetails(detail.children);
  }

  return json;
}

/**
 * Validate workspace model
 * @param workspace Workspace to validate
 * @returns Array of validation errors (empty if valid)
 */
export function validateWorkspace(workspace: WorkspaceModel): string[] {
  const errors: string[] = [];

  if (!workspace.devices || !Array.isArray(workspace.devices)) {
    errors.push('Workspace must have a devices array');
  }

  workspace.devices?.forEach((device, index) => {
    if (!device.deviceKey) {
      errors.push(`Device at index ${index} missing deviceKey`);
    }

    device.operationalDetails?.forEach((detail, detailIndex) => {
      if (!detail.title) {
        errors.push(
          `Device ${device.deviceKey} operational detail ${detailIndex} missing title`
        );
      }
    });
  });

  return errors;
}

/**
 * Merge two workspace models
 * Later device keys override earlier ones
 */
export function mergeWorkspaces(workspace1: WorkspaceModel, workspace2: WorkspaceModel): WorkspaceModel {
  const merged = createEmptyWorkspace();

  // Merge devices by key
  const deviceMap = new Map<string, DeviceDocument>();
  workspace1.devices?.forEach(device => {
    deviceMap.set(device.deviceKey, device);
  });
  workspace2.devices?.forEach(device => {
    deviceMap.set(device.deviceKey, device);
  });

  merged.devices = Array.from(deviceMap.values());

  // Merge metadata
  if (workspace1.genericDescriptions || workspace2.genericDescriptions) {
    merged.genericDescriptions = {
      ...workspace1.genericDescriptions,
      ...workspace2.genericDescriptions,
    };
  }

  return merged;
}

/**
 * Find a device by key
 */
export function findDeviceByKey(workspace: WorkspaceModel, key: string): DeviceDocument | undefined {
  return workspace.devices?.find(d => d.deviceKey === key);
}

/**
 * Find all devices by name prefix
 */
export function findDevicesByNamePrefix(workspace: WorkspaceModel, prefix: string): DeviceDocument[] {
  return (workspace.devices || []).filter(d => 
    d.displayName?.toLowerCase().startsWith(prefix.toLowerCase())
  );
}

/**
 * Count operational details in a device (including nested)
 */
export function countOperationalDetails(device: DeviceDocument): number {
  if (!device.operationalDetails) {
    return 0;
  }

  let count = 0;
  for (const detail of device.operationalDetails) {
    count++;
    if (detail.children) {
      count += countOperationalDetailsInList(detail.children);
    }
  }

  return count;
}

/**
 * Count operational details in a list (recursive helper)
 */
function countOperationalDetailsInList(details: OperationalDetail[]): number {
  let count = 0;
  for (const detail of details) {
    count++;
    if (detail.children) {
      count += countOperationalDetailsInList(detail.children);
    }
  }
  return count;
}
