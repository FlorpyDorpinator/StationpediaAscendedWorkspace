/**
 * Tests for JSON codec - workspace parsing and serialization
 */
import { describe, it, expect } from 'vitest';
import { parseWorkspaceJSON, serializeWorkspaceJSON } from './jsonCodec';

describe('JSON Codec', () => {
  describe('parseWorkspaceJSON', () => {
    it('should parse a minimal device', () => {
      const json = {
        devices: [
          {
            deviceKey: 'ThingDevice',
            displayName: 'Test Device',
          },
        ],
      };
      const result = parseWorkspaceJSON(json);
      expect(result.devices).toHaveLength(1);
      expect(result.devices[0].deviceKey).toBe('ThingDevice');
    });

    it('should parse device with operationalDetails', () => {
      const json = {
        devices: [
          {
            deviceKey: 'ThingDevice',
            OperationalDetails: [
              {
                title: 'Overview',
                description: 'Test description',
              },
            ],
          },
        ],
      };
      const result = parseWorkspaceJSON(json);
      const device = result.devices[0];
      expect(device.operationalDetails).toBeDefined();
      expect(device.operationalDetails![0].title).toBe('Overview');
    });

    it('should normalize OperationalDetails to operationalDetails', () => {
      const json = {
        devices: [
          {
            deviceKey: 'ThingDevice',
            OperationalDetails: [{ title: 'Section' }],
          },
        ],
      };
      const result = parseWorkspaceJSON(json);
      expect(result.devices[0].operationalDetails).toBeDefined();
    });

    it('should preserve unknown fields', () => {
      const json = {
        devices: [
          {
            deviceKey: 'ThingDevice',
            unknownField: 'preserved',
            customData: { nested: true },
          },
        ],
      };
      const result = parseWorkspaceJSON(json);
      const device = result.devices[0];
      expect((device as any).unknownField).toBe('preserved');
      expect((device as any).customData.nested).toBe(true);
    });

    it('should handle nested operational details', () => {
      const json = {
        devices: [
          {
            deviceKey: 'ThingDevice',
            OperationalDetails: [
              {
                title: 'Parent',
                children: [
                  {
                    title: 'Child',
                    description: 'Nested content',
                  },
                ],
              },
            ],
          },
        ],
      };
      const result = parseWorkspaceJSON(json);
      const parent = result.devices[0].operationalDetails![0];
      expect(parent.children).toHaveLength(1);
      expect(parent.children![0].title).toBe('Child');
    });

    it('should handle logicDescriptions', () => {
      const json = {
        devices: [
          {
            deviceKey: 'ThingDevice',
            logicDescriptions: {
              Power: {
                dataType: 'Boolean',
                range: '0-1',
                description: 'Power state',
              },
            },
          },
        ],
      };
      const result = parseWorkspaceJSON(json);
      expect(result.devices[0].logicDescriptions).toBeDefined();
      expect(result.devices[0].logicDescriptions!.Power.dataType).toBe('Boolean');
    });

    it('should parse all device fields', () => {
      const json = {
        devices: [
          {
            deviceKey: 'ThingDevice',
            displayName: 'Device',
            pageDescription: 'Page',
            pageDescriptionPrepend: 'Prepend',
            pageDescriptionAppend: 'Append',
            operationalDetailsTitleColor: '#FF0000',
            operationalDetailsBackgroundColor: '#000000',
            generateToc: true,
            tocTitle: 'Contents',
            modeDescriptions: { Mode0: { description: 'Test' } },
            slotDescriptions: { Slot: { description: 'Test' } },
            versionDescriptions: { v1: { description: 'Test' } },
            memoryDescriptions: { mem: { description: 'Test' } },
          },
        ],
      };
      const result = parseWorkspaceJSON(json);
      const device = result.devices[0];
      expect(device.displayName).toBe('Device');
      expect(device.pageDescription).toBe('Page');
      expect(device.operationalDetailsTitleColor).toBe('#FF0000');
    });
  });

  describe('serializeWorkspaceJSON', () => {
    it('should serialize back to original structure', () => {
      const original = {
        devices: [
          {
            deviceKey: 'ThingDevice',
            displayName: 'Test',
          },
        ],
      };
      const parsed = parseWorkspaceJSON(original);
      const serialized = serializeWorkspaceJSON(parsed);
      expect(serialized.devices).toHaveLength(1);
      expect(serialized.devices[0].deviceKey).toBe('ThingDevice');
    });

    it('should convert operationalDetails back to OperationalDetails', () => {
      const json = {
        devices: [
          {
            deviceKey: 'ThingDevice',
            OperationalDetails: [{ title: 'Section' }],
          },
        ],
      };
      const parsed = parseWorkspaceJSON(json);
      const serialized = serializeWorkspaceJSON(parsed);
      expect(serialized.devices[0].OperationalDetails).toBeDefined();
    });

    it('should preserve unknown fields in serialization', () => {
      const json = {
        devices: [
          {
            deviceKey: 'ThingDevice',
            customField: 'custom value',
          },
        ],
      };
      const parsed = parseWorkspaceJSON(json);
      const serialized = serializeWorkspaceJSON(parsed);
      expect((serialized.devices[0] as any).customField).toBe('custom value');
    });

    it('should handle nested operational details on serialize', () => {
      const json = {
        devices: [
          {
            deviceKey: 'ThingDevice',
            OperationalDetails: [
              {
                title: 'Parent',
                children: [{ title: 'Child' }],
              },
            ],
          },
        ],
      };
      const parsed = parseWorkspaceJSON(json);
      const serialized = serializeWorkspaceJSON(parsed);
      const parent = serialized.devices[0].OperationalDetails![0];
      expect(parent.children).toHaveLength(1);
    });
  });

  describe('Round-trip preservation', () => {
    it('should preserve all data in round-trip', () => {
      const original = {
        devices: [
          {
            deviceKey: 'ThingDevice',
            displayName: 'Test Device',
            OperationalDetails: [
              {
                title: 'Overview',
                tocId: 'overview',
                description: '<b>Bold</b> text',
                items: ['Item 1', 'Item 2'],
                children: [
                  {
                    title: 'Child',
                    description: 'Child description',
                  },
                ],
              },
            ],
            logicDescriptions: {
              Power: {
                dataType: 'Boolean',
                range: '0-1',
                description: 'Power state',
              },
            },
            customField: 'should round-trip',
          },
        ],
      };
      const parsed = parseWorkspaceJSON(original);
      const serialized = serializeWorkspaceJSON(parsed);
      
      // Verify key fields
      expect(serialized.devices).toHaveLength(1);
      const device = serialized.devices[0];
      expect(device.deviceKey).toBe('ThingDevice');
      expect(device.displayName).toBe('Test Device');
      expect(device.OperationalDetails![0].title).toBe('Overview');
      expect(device.logicDescriptions!.Power.dataType).toBe('Boolean');
      expect((device as any).customField).toBe('should round-trip');
    });
  });

  describe('Empty and edge cases', () => {
    it('should handle empty devices array', () => {
      const json = { devices: [] };
      const result = parseWorkspaceJSON(json);
      expect(result.devices).toHaveLength(0);
    });

    it('should handle missing optional fields', () => {
      const json = {
        devices: [{ deviceKey: 'ThingDevice' }],
      };
      const result = parseWorkspaceJSON(json);
      expect(result.devices[0].deviceKey).toBe('ThingDevice');
      expect(result.devices[0].displayName).toBeUndefined();
    });

    it('should handle null values in descriptions', () => {
      const json = {
        devices: [
          {
            deviceKey: 'ThingDevice',
            displayName: null,
            pageDescription: null,
          },
        ],
      };
      const result = parseWorkspaceJSON(json);
      const device = result.devices[0];
      expect(device.displayName).toBeNull();
    });

    it('should handle complex nested structures', () => {
      const json = {
        devices: [
          {
            deviceKey: 'ThingDevice',
            OperationalDetails: [
              {
                title: 'L1',
                children: [
                  {
                    title: 'L2',
                    children: [
                      {
                        title: 'L3',
                        description: 'Deep nesting',
                      },
                    ],
                  },
                ],
              },
            ],
          },
        ],
      };
      const result = parseWorkspaceJSON(json);
      const l3 = result.devices[0].operationalDetails![0].children![0].children![0];
      expect(l3.title).toBe('L3');
      expect(l3.description).toBe('Deep nesting');
    });
  });
});
