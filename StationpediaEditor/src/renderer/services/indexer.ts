/**
 * Content indexing and search
 */

import Fuse from 'fuse.js';
import type { DeviceDescription } from '@models/stationpedia';

export class ContentIndexer {
  private index: Fuse<DeviceDescription> | null = null;

  /**
   * Index devices for fast searching
   */
  indexDevices(devices: DeviceDescription[]): void {
    this.index = new Fuse(devices, {
      keys: [
        'deviceKey',
        'displayName',
        'pageDescription',
        'operationalDetails.title',
        'operationalDetails.description',
      ],
      threshold: 0.3,
      ignoreLocation: true,
    });
  }

  /**
   * Search indexed devices
   */
  search(query: string): DeviceDescription[] {
    if (!this.index || !query.trim()) {
      return [];
    }
    return this.index.search(query).map(result => result.item);
  }

  /**
   * Get all indexed devices
   */
  getAll(devices: DeviceDescription[]): DeviceDescription[] {
    return devices;
  }
}

export const indexer = new ContentIndexer();
