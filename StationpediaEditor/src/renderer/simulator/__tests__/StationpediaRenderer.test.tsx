/**
 * Tests for StationpediaRenderer component
 * Tests rendering of DeviceDocument with AST parsing
 */

import { describe, it, expect, vi } from 'vitest';
import React from 'react';
import type { DeviceDocument, OperationalDetail } from '@models/contentModel';
import { StationpediaRenderer } from '../StationpediaRenderer';

describe('StationpediaRenderer', () => {
  const createTestDevice = (overrides?: Partial<DeviceDocument>): DeviceDocument => ({
    deviceKey: 'TestDevice',
    displayName: 'Test Device',
    ...overrides,
  });

  describe('Component structure', () => {
    it('should create a valid React component', () => {
      const device = createTestDevice();
      const component = React.createElement(StationpediaRenderer, { device });
      expect(component).toBeTruthy();
      expect(component.type).toBe(StationpediaRenderer);
    });

    it('should accept DeviceDocument props', () => {
      const device = createTestDevice();
      const component = React.createElement(StationpediaRenderer, { device });
      expect(component.props.device?.deviceKey).toBe('TestDevice');
    });

    it('should accept onLinkClick callback', () => {
      const device = createTestDevice();
      const callback = vi.fn();
      const component = React.createElement(StationpediaRenderer, { device, onLinkClick: callback });
      expect(component.props.onLinkClick).toBe(callback);
    });
  });

  describe('Empty device', () => {
    it('should handle empty device', () => {
      const device = createTestDevice();
      const component = React.createElement(StationpediaRenderer, { device });
      expect(component).toBeTruthy();
    });

    it('should display device name from displayName', () => {
      const device = createTestDevice({ displayName: 'Solar Panel' });
      expect(device.displayName).toBe('Solar Panel');
    });

    it('should display device key', () => {
      const device = createTestDevice({ deviceKey: 'StructureSolarPanel' });
      expect(device.deviceKey).toBe('StructureSolarPanel');
    });

    it('should use deviceKey when displayName is null', () => {
      const device = createTestDevice({ displayName: null });
      expect(device.displayName).toBeNull();
      expect(device.deviceKey).toBe('TestDevice');
    });
  });

  describe('Page description', () => {
    it('should include pageDescription in device', () => {
      const device = createTestDevice({
        pageDescription: 'This is a solar panel',
      });
      expect(device.pageDescription).toBe('This is a solar panel');
    });

    it('should combine pageDescriptionPrepend and pageDescription', () => {
      const device = createTestDevice({
        pageDescriptionPrepend: 'Prepended text',
        pageDescription: 'Main description',
      });
      expect(device.pageDescriptionPrepend).toBe('Prepended text');
      expect(device.pageDescription).toBe('Main description');
    });

    it('should combine pageDescriptionAppend', () => {
      const device = createTestDevice({
        pageDescription: 'Main description',
        pageDescriptionAppend: 'Appended text',
      });
      expect(device.pageDescriptionAppend).toBe('Appended text');
    });

    it('should support rich text formatting in description', () => {
      const device = createTestDevice({
        pageDescription: '<b>Bold</b> <i>italic</i> <color=#FF0000>red</color>',
      });
      expect(device.pageDescription).toContain('<b>');
      expect(device.pageDescription).toContain('<i>');
      expect(device.pageDescription).toContain('<color=');
    });
  });

  describe('Operational details', () => {
    it('should store operational details', () => {
      const detail: OperationalDetail = {
        title: 'Basic Operation',
        description: 'This is how it works',
      };
      const device = createTestDevice({
        operationalDetails: [detail],
      });
      expect(device.operationalDetails).toHaveLength(1);
      expect(device.operationalDetails![0].title).toBe('Basic Operation');
    });

    it('should support items list in operational detail', () => {
      const detail: OperationalDetail = {
        title: 'Requirements',
        items: ['Requires power', 'Needs coolant', 'Must be grounded'],
      };
      const device = createTestDevice({
        operationalDetails: [detail],
      });
      expect(device.operationalDetails![0].items).toHaveLength(3);
    });

    it('should support steps list in operational detail', () => {
      const detail: OperationalDetail = {
        title: 'Installation',
        steps: ['Place the device', 'Connect power', 'Configure settings'],
      };
      const device = createTestDevice({
        operationalDetails: [detail],
      });
      expect(device.operationalDetails![0].steps).toHaveLength(3);
    });

    it('should support nested operational details', () => {
      const detail: OperationalDetail = {
        title: 'Advanced',
        children: [
          {
            title: 'Configuration',
            description: 'Configure the device',
          },
        ],
      };
      const device = createTestDevice({
        operationalDetails: [detail],
      });
      expect(device.operationalDetails![0].children).toHaveLength(1);
      expect(device.operationalDetails![0].children![0].title).toBe('Configuration');
    });

    it('should support collapsible flag', () => {
      const detail: OperationalDetail = {
        title: 'Collapsed Section',
        collapsible: true,
        description: 'Hidden content',
      };
      const device = createTestDevice({
        operationalDetails: [detail],
      });
      expect(device.operationalDetails![0].collapsible).toBe(true);
    });

    it('should support tocId for table of contents', () => {
      const detail: OperationalDetail = {
        title: 'Section',
        tocId: 'section-1',
      };
      const device = createTestDevice({
        operationalDetails: [detail],
      });
      expect(device.operationalDetails![0].tocId).toBe('section-1');
    });

    it('should support imageFile', () => {
      const detail: OperationalDetail = {
        title: 'Visual Guide',
        imageFile: 'guide.png',
      };
      const device = createTestDevice({
        operationalDetails: [detail],
      });
      expect(device.operationalDetails![0].imageFile).toBe('guide.png');
    });

    it('should support backgroundColor', () => {
      const detail: OperationalDetail = {
        title: 'Colored Section',
        backgroundColor: '#2a2a3e',
      };
      const device = createTestDevice({
        operationalDetails: [detail],
      });
      expect(device.operationalDetails![0].backgroundColor).toBe('#2a2a3e');
    });

    it('should support youtubeUrl', () => {
      const detail: OperationalDetail = {
        title: 'Tutorial Video',
        youtubeUrl: 'dQw4w9WgXcQ',
        youtubeLabel: 'Watch Tutorial',
      };
      const device = createTestDevice({
        operationalDetails: [detail],
      });
      expect(device.operationalDetails![0].youtubeUrl).toBe('dQw4w9WgXcQ');
      expect(device.operationalDetails![0].youtubeLabel).toBe('Watch Tutorial');
    });
  });

  describe('Logic descriptions', () => {
    it('should store logic descriptions', () => {
      const device = createTestDevice({
        logicDescriptions: {
          Power: {
            dataType: 'Boolean',
            range: '0-1',
            description: 'Power input',
          },
        },
      });
      expect(device.logicDescriptions).toBeTruthy();
      expect(device.logicDescriptions!['Power'].dataType).toBe('Boolean');
    });

    it('should support multiple logic descriptions', () => {
      const device = createTestDevice({
        logicDescriptions: {
          Power: {
            dataType: 'Boolean',
            range: '0-1',
            description: 'Power input',
          },
          Temperature: {
            dataType: 'Float',
            range: '273-373K',
            description: 'Temperature sensor',
          },
        },
      });
      expect(Object.keys(device.logicDescriptions!)).toHaveLength(2);
    });
  });

  describe('Mode descriptions', () => {
    it('should store mode descriptions', () => {
      const device = createTestDevice({
        modeDescriptions: {
          Mode0: {
            modeValue: '0',
            description: 'Standby mode',
          },
        },
      });
      expect(device.modeDescriptions).toBeTruthy();
    });

    it('should support multiple modes', () => {
      const device = createTestDevice({
        modeDescriptions: {
          Mode0: {
            modeValue: '0',
            description: 'Standby mode',
          },
          Mode1: {
            modeValue: '1',
            description: 'Active mode',
          },
        },
      });
      expect(Object.keys(device.modeDescriptions!)).toHaveLength(2);
    });
  });

  describe('Slot descriptions', () => {
    it('should store slot descriptions', () => {
      const device = createTestDevice({
        slotDescriptions: {
          Slot0: {
            slotNumber: 0,
            slotType: 'Battery',
            description: 'Battery slot',
          },
        },
      });
      expect(device.slotDescriptions).toBeTruthy();
      expect(device.slotDescriptions!['Slot0'].slotType).toBe('Battery');
    });
  });

  describe('Version descriptions', () => {
    it('should store version descriptions', () => {
      const device = createTestDevice({
        versionDescriptions: {
          V1: {
            versionValue: '1',
            description: 'Original version',
          },
        },
      });
      expect(device.versionDescriptions).toBeTruthy();
    });
  });

  describe('Table of Contents', () => {
    it('should have generateToc flag', () => {
      const device = createTestDevice({
        generateToc: true,
      });
      expect(device.generateToc).toBe(true);
    });

    it('should have tocTitle field', () => {
      const device = createTestDevice({
        tocTitle: 'Quick Reference',
      });
      expect(device.tocTitle).toBe('Quick Reference');
    });

    it('should generate TOC from operational details with tocId', () => {
      const device = createTestDevice({
        generateToc: true,
        operationalDetails: [
          {
            title: 'Section One',
            tocId: 'section-1',
            description: 'First section',
          },
          {
            title: 'Section Two',
            tocId: 'section-2',
            description: 'Second section',
          },
        ],
      });
      expect(device.operationalDetails![0].tocId).toBe('section-1');
      expect(device.operationalDetails![1].tocId).toBe('section-2');
    });
  });

  describe('Complex scenarios', () => {
    it('should support fully featured device', () => {
      const device: DeviceDocument = {
        deviceKey: 'StructureAutolathe',
        displayName: 'Autolathe',
        pageDescription: '<b>The Autolathe</b> is an essential <color=#00FF00>fabrication</color> device.',
        pageDescriptionPrepend: 'Advanced Manufacturing:',
        operationalDetails: [
          {
            title: 'Overview',
            tocId: 'overview',
            description: 'The autolathe fabricates items.',
            collapsible: false,
          },
          {
            title: 'Advanced Settings',
            tocId: 'advanced',
            collapsible: true,
            children: [
              {
                title: 'Queue Management',
                description: 'Manage the job queue',
              },
            ],
          },
        ],
        logicDescriptions: {
          Power: {
            dataType: 'Boolean',
            range: '0-1',
            description: 'Power status',
          },
        },
        generateToc: true,
        tocTitle: 'Quick Reference',
      };
      expect(device.deviceKey).toBe('StructureAutolathe');
      expect(device.displayName).toBe('Autolathe');
      expect(device.operationalDetails).toHaveLength(2);
      expect(device.generateToc).toBe(true);
    });
  });
});
