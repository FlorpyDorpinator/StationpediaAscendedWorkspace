/**
 * Device categorization utilities
 * Functions for organizing and searching devices by category
 */

import { CATEGORIES } from '../types/categories';
import { getCategoryForDevice } from '../data/categoryMapping';
import type { DeviceDescription } from '../../models/stationpedia';
import type { CategoryId } from '../types/categories';

/**
 * Categorize a list of devices into a map by category
 * Ensures all 19 categories are present in the result, even if empty
 *
 * @param devices - Array of device descriptions
 * @returns Map of CategoryId to array of devices in that category
 */
export function getCategorizedDevices(
  devices: DeviceDescription[]
): Map<CategoryId, DeviceDescription[]> {
  const categorized = new Map<CategoryId, DeviceDescription[]>();

  // Initialize all 19 categories
  CATEGORIES.forEach(category => {
    categorized.set(category.id, []);
  });

  // Categorize each device
  devices.forEach(device => {
    const categoryId = getCategoryForDevice(device.deviceKey);
    const categoryDevices = categorized.get(categoryId);
    if (categoryDevices) {
      categoryDevices.push(device);
    }
  });

  return categorized;
}

/**
 * Get the count of devices in each category
 *
 * @param devices - Array of device descriptions
 * @returns Map of CategoryId to count of devices in that category
 */
export function getDeviceCountByCategory(
  devices: DeviceDescription[]
): Map<CategoryId, number> {
  const counts = new Map<CategoryId, number>();

  // Initialize all 19 categories with 0
  CATEGORIES.forEach(category => {
    counts.set(category.id, 0);
  });

  // Count devices in each category
  devices.forEach(device => {
    const categoryId = getCategoryForDevice(device.deviceKey);
    const currentCount = counts.get(categoryId) || 0;
    counts.set(categoryId, currentCount + 1);
  });

  return counts;
}

/**
 * Search for devices within a specific category
 * Searches across deviceKey and displayName fields
 *
 * @param devices - Array of device descriptions
 * @param categoryId - The category to search within
 * @param query - Search query string (case-insensitive)
 * @returns Array of matching devices in the category
 */
export function searchDevicesInCategory(
  devices: DeviceDescription[],
  categoryId: CategoryId,
  query: string
): DeviceDescription[] {
  // Filter devices to only those in the specified category
  const categoryDevices = devices.filter(
    device => getCategoryForDevice(device.deviceKey) === categoryId
  );

  // If query is empty, return all devices in the category
  if (!query || query.trim() === '') {
    return categoryDevices;
  }

  // Search within category
  const lowerQuery = query.toLowerCase();
  return categoryDevices.filter(device => {
    const deviceKeyMatch = device.deviceKey.toLowerCase().includes(lowerQuery);
    const displayNameMatch = device.displayName?.toLowerCase().includes(lowerQuery) ?? false;
    return deviceKeyMatch || displayNameMatch;
  });
}
