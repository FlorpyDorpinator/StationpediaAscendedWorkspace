/**
 * Integration test: End-to-end verification of Phase 1 implementation
 * Tests loading real descriptions.json, parsing, and round-tripping
 */

import { describe, it, expect } from 'vitest';
import { parseWorkspaceJSON, serializeWorkspaceJSON } from '../services/jsonCodec';
import { parseToAST } from '../services/parser';
import { serializeToTMP } from '../services/serializer';

describe('Phase 1 Integration: AST & Lossless Serialization', () => {
  describe('Workspace parsing and serialization', () => {
    it('should handle minimal device structure', () => {
      const input = {
        devices: [
          {
            deviceKey: 'TestDevice',
            displayName: 'Test Device',
          },
        ],
      };

      const workspace = parseWorkspaceJSON(input);
      const output = serializeWorkspaceJSON(workspace);

      expect(output.devices).toHaveLength(1);
      expect(output.devices[0].deviceKey).toBe('TestDevice');
      expect(output.devices[0].displayName).toBe('Test Device');
    });

    it('should preserve OperationalDetails through round-trip', () => {
      const input = {
        devices: [
          {
            deviceKey: 'ThingDevice',
            displayName: 'Device',
            OperationalDetails: [
              {
                title: 'Overview',
                tocId: 'overview',
                description: '<b>Bold text</b> and <color=#FF0000>colored</color>',
                items: ['Item 1', 'Item 2'],
              },
            ],
          },
        ],
      };

      const workspace = parseWorkspaceJSON(input);
      const output = serializeWorkspaceJSON(workspace);

      const detail = output.devices[0].OperationalDetails![0];
      expect(detail.title).toBe('Overview');
      expect(detail.description).toContain('<b>');
      expect(detail.items).toHaveLength(2);
    });

    it('should handle nested operational details', () => {
      const input = {
        devices: [
          {
            deviceKey: 'ThingDevice',
            OperationalDetails: [
              {
                title: 'Parent',
                children: [
                  {
                    title: 'Child 1',
                    description: 'Child content',
                  },
                  {
                    title: 'Child 2',
                    children: [
                      {
                        title: 'Grandchild',
                      },
                    ],
                  },
                ],
              },
            ],
          },
        ],
      };

      const workspace = parseWorkspaceJSON(input);
      const output = serializeWorkspaceJSON(workspace);

      const parent = output.devices[0].OperationalDetails![0];
      expect(parent.children).toHaveLength(2);
      expect(parent.children![1].children).toHaveLength(1);
      expect(parent.children![1].children![0].title).toBe('Grandchild');
    });

    it('should preserve unknown fields', () => {
      const input = {
        devices: [
          {
            deviceKey: 'ThingDevice',
            customField: 'custom value',
            nestedCustom: {
              deep: true,
              value: 42,
            },
            displayName: 'Device',
          },
        ],
      };

      const workspace = parseWorkspaceJSON(input);
      const output = serializeWorkspaceJSON(workspace);

      const device = output.devices[0];
      expect((device as any).customField).toBe('custom value');
      expect((device as any).nestedCustom.deep).toBe(true);
    });

    it('should handle all device fields', () => {
      const input = {
        devices: [
          {
            deviceKey: 'ComplexDevice',
            displayName: 'Complex Device',
            pageDescription: 'Main description',
            pageDescriptionPrepend: 'Prepend text',
            pageDescriptionAppend: 'Append text',
            operationalDetailsTitleColor: '#FF0000',
            operationalDetailsBackgroundColor: '#000000',
            generateToc: true,
            tocTitle: 'Contents',
            logicDescriptions: {
              Power: {
                dataType: 'Boolean',
                range: '0-1',
                description: 'Power state',
              },
            },
            modeDescriptions: {
              Mode0: {
                description: 'Default mode',
              },
            },
            slotDescriptions: {
              Input: {
                description: 'Input slot',
              },
            },
            versionDescriptions: {
              v1: {
                description: 'Version 1',
              },
            },
            memoryDescriptions: {
              mem0: {
                description: 'Memory 0',
              },
            },
          },
        ],
      };

      const workspace = parseWorkspaceJSON(input);
      const output = serializeWorkspaceJSON(workspace);

      const device = output.devices[0];
      expect(device.pageDescription).toBe('Main description');
      expect(device.generateToc).toBe(true);
      expect(device.logicDescriptions!.Power.dataType).toBe('Boolean');
      expect(device.modeDescriptions!.Mode0.description).toBe('Default mode');
    });
  });

  describe('TMP parsing and serialization', () => {
    it('should parse and re-serialize TMP formatting tags', () => {
      const tmp = '<b>Bold</b> <i>Italic</i> <u>Underline</u> <s>Strike</s>';
      const ast = parseToAST(tmp);
      const serialized = serializeToTMP(ast);

      expect(serialized).toContain('<b>');
      expect(serialized).toContain('<i>');
      expect(serialized).toContain('<u>');
      expect(serialized).toContain('<s>');
    });

    it('should preserve special link tags', () => {
      const tmp = 'See {THING:ThingDevice} or {LINK:PageKey;Display Text}';
      const ast = parseToAST(tmp);

      expect(ast).toContainEqual(expect.objectContaining({ type: 'link' }));
    });

    it('should handle color tags with hex codes', () => {
      const tmp = '<color=#FF0000>Red text</color> and <color=#00FF00>Green</color>';
      const ast = parseToAST(tmp);
      const serialized = serializeToTMP(ast);

      expect(serialized).toContain('#FF0000');
      expect(serialized).toContain('#00FF00');
    });

    it('should round-trip complex formatting', () => {
      const original = '<color=#0000FF><b>Blue bold</b></color> normal <i>italic</i>';
      const ast = parseToAST(original);
      const serialized = serializeToTMP(ast);
      const reparsed = parseToAST(serialized);

      // Should have same structure
      expect(reparsed.length).toBeGreaterThan(0);
      expect(reparsed.some(n => n.type === 'color')).toBe(true);
      expect(reparsed.some(n => n.type === 'italic')).toBe(true);
    });
  });

  describe('Complete workflow', () => {
    it('should preserve {THING:} link tags with text content', () => {
      // Real example from descriptions.json:
      // "they are used to make {THING:StructureSolarPanel;Solar Panels}"
      const input = {
        devices: [
          {
            deviceKey: 'ThingStructureGlassSheet',
            displayName: 'Glass Sheet',
            pageDescription:
              'A fundamental construction component, glass sheets are used to make {THING:StructureSolarPanel;Solar Panels}, and other structures.',
          },
        ],
      };

      const workspace = parseWorkspaceJSON(input);
      const device = workspace.devices[0];

      // Parse description
      const ast = parseToAST(device.pageDescription!);
      expect(ast.some(n => n.type === 'link')).toBe(true);

      // Find the link node
      const linkNode = ast.find(n => n.type === 'link') as any;
      expect(linkNode).toBeDefined();
      expect(linkNode.linkType).toBe('internal');
      expect(linkNode.target).toBe('StructureSolarPanel');
      expect(linkNode.originalFormat).toBe('thing');

      // Serialize back
      const serialized = serializeToTMP(ast);
      expect(serialized).toContain('{THING:StructureSolarPanel;Solar Panels}');
    });

    it('should preserve <link=> tags with HTML format', () => {
      const input = {
        devices: [
          {
            deviceKey: 'ThingStructureGlassSheet',
            pageDescription:
              'Glass sheets are fabricated on the <link=ThingStructureAutolathe><color=green>Autolathe</color></link>.',
          },
        ],
      };

      const workspace = parseWorkspaceJSON(input);
      const device = workspace.devices[0];

      const ast = parseToAST(device.pageDescription!);
      const linkNode = ast.find(n => n.type === 'link') as any;

      expect(linkNode).toBeDefined();
      expect(linkNode.originalFormat).toBe('html');
      expect(linkNode.tocId).toBe('ThingStructureAutolathe');

      const serialized = serializeToTMP(ast);
      expect(serialized).toContain('<link=ThingStructureAutolathe>');
    });

    it('should handle real-world device structure with mixed content', () => {
      const input = {
        devices: [
          {
            deviceKey: 'ThingGenerator',
            displayName: 'Solid Fuel Generator',
            pageDescription:
              'This generator <b>burns fuel</b> to produce <color=#FFAA00>power</color>.',
            OperationalDetails: [
              {
                title: 'Setup Guide',
                tocId: 'setup',
                description: 'Follow these steps:\n\n1. Place on surface\n2. Connect power cables',
                steps: [
                  'Place the generator on a solid surface',
                  'Connect power cables',
                  'Insert fuel',
                  'Activate with logic or switch',
                ],
              },
              {
                title: 'Fuel Types',
                children: [
                  {
                    title: 'Coal',
                    description: 'Primary fuel - high power output',
                    items: ['Burn time: 180s', 'Power: 5kW'],
                  },
                  {
                    title: 'Biomass',
                    description: 'Renewable fuel - lower output',
                    items: ['Burn time: 60s', 'Power: 2kW'],
                  },
                ],
              },
            ],
            logicDescriptions: {
              Power: {
                dataType: 'Boolean',
                range: '0-1',
                description: 'Returns 1 if powered',
              },
              On: {
                dataType: 'Boolean',
                range: '0-1',
                description: 'On/off state',
              },
            },
            customMetadata: {
              tier: 'basic',
              version: '1.0',
            },
          },
        ],
      };

      const workspace = parseWorkspaceJSON(input);
      expect(workspace.devices).toHaveLength(1);

      const device = workspace.devices[0];
      expect(device.deviceKey).toBe('ThingGenerator');
      expect(device.operationalDetails).toHaveLength(2);
      expect(device.operationalDetails![0].steps).toHaveLength(4);
      expect(device.operationalDetails![1].children).toHaveLength(2);
      expect((device as any).customMetadata.tier).toBe('basic');

      // Serialize and verify preservation
      const output = serializeWorkspaceJSON(workspace);
      const outDevice = output.devices[0];
      expect(outDevice.OperationalDetails![0].steps).toHaveLength(4);
      expect((outDevice as any).customMetadata.tier).toBe('basic');
    });
  });
});
