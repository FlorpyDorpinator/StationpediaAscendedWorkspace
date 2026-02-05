/**
 * Tests for validator service with comprehensive validation rules
 */
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { ValidatorService } from '../validator';
import type { ValidationContext, ValidationError } from '@models/validationModel';
import { VALIDATION_RULE_IDS } from '@models/validationModel';

describe('ValidatorService', () => {
  let validator: ValidatorService;

  beforeEach(() => {
    validator = new ValidatorService();
  });

  describe('TMP Tag Validation', () => {
    it('should detect unclosed bold tags', () => {
      const context: ValidationContext = {
        device: {
          deviceKey: 'TestDevice',
          pageDescription: 'This is <b>bold text without closing',
          operationalDetails: [],
        },
        knownDeviceKeys: new Set(['TestDevice']),
        assetRegistry: new Map(),
      };

      const errors = validator.validate(context);
      const unclosedError = errors.find(e => e.ruleId === VALIDATION_RULE_IDS.UNCLOSED_TAG);
      expect(unclosedError).toBeDefined();
      expect(unclosedError?.message.toLowerCase()).toContain('unclosed');
    });

    it('should detect unclosed italic tags', () => {
      const context: ValidationContext = {
        device: {
          deviceKey: 'TestDevice',
          pageDescription: 'This is <i>italic without close',
        },
        knownDeviceKeys: new Set(['TestDevice']),
        assetRegistry: new Map(),
      };

      const errors = validator.validate(context);
      const unclosedError = errors.find(e => e.ruleId === VALIDATION_RULE_IDS.UNCLOSED_TAG);
      expect(unclosedError).toBeDefined();
    });

    it('should allow properly closed tags', () => {
      const context: ValidationContext = {
        device: {
          deviceKey: 'TestDevice',
          pageDescription: 'This is <b>bold</b> and <i>italic</i>',
        },
        knownDeviceKeys: new Set(['TestDevice']),
        assetRegistry: new Map(),
      };

      const errors = validator.validate(context);
      const unclosedErrors = errors.filter(e => e.ruleId === VALIDATION_RULE_IDS.UNCLOSED_TAG);
      expect(unclosedErrors).toHaveLength(0);
    });

    it('should detect invalid TMP tags', () => {
      const context: ValidationContext = {
        device: {
          deviceKey: 'TestDevice',
          pageDescription: 'This is <invalid>text</invalid>',
        },
        knownDeviceKeys: new Set(['TestDevice']),
        assetRegistry: new Map(),
      };

      const errors = validator.validate(context);
      const invalidTagError = errors.find(e => e.ruleId === VALIDATION_RULE_IDS.INVALID_TMP_TAG);
      expect(invalidTagError).toBeDefined();
    });

    it('should detect invalid color format', () => {
      const context: ValidationContext = {
        device: {
          deviceKey: 'TestDevice',
          pageDescription: 'This is <color=#GGGGGG>text</color>',
        },
        knownDeviceKeys: new Set(['TestDevice']),
        assetRegistry: new Map(),
      };

      const errors = validator.validate(context);
      const colorError = errors.find(e => e.ruleId === VALIDATION_RULE_IDS.INVALID_COLOR);
      expect(colorError).toBeDefined();
    });

    it('should allow valid hex colors', () => {
      const context: ValidationContext = {
        device: {
          deviceKey: 'TestDevice',
          pageDescription: 'This is <color=#FF00FF>text</color>',
        },
        knownDeviceKeys: new Set(['TestDevice']),
        assetRegistry: new Map(),
      };

      const errors = validator.validate(context);
      const colorError = errors.find(e => e.ruleId === VALIDATION_RULE_IDS.INVALID_COLOR);
      expect(colorError).toBeUndefined();
    });
  });

  describe('Link Validation', () => {
    it('should detect broken THING links', () => {
      const context: ValidationContext = {
        device: {
          deviceKey: 'Device1',
          pageDescription: 'See {THING:NonExistentDevice}',
        },
        knownDeviceKeys: new Set(['Device1', 'Device2']),
        assetRegistry: new Map(),
      };

      const errors = validator.validate(context);
      const brokenLinkError = errors.find(e => e.ruleId === VALIDATION_RULE_IDS.BROKEN_LINK);
      expect(brokenLinkError).toBeDefined();
      expect(brokenLinkError?.message).toContain('NonExistentDevice');
    });

    it('should validate correct THING links', () => {
      const context: ValidationContext = {
        device: {
          deviceKey: 'Device1',
          pageDescription: 'See {THING:Device2}',
        },
        knownDeviceKeys: new Set(['Device1', 'Device2']),
        assetRegistry: new Map(),
      };

      const errors = validator.validate(context);
      const brokenLinkErrors = errors.filter(e => e.ruleId === VALIDATION_RULE_IDS.BROKEN_LINK);
      expect(brokenLinkErrors).toHaveLength(0);
    });

    it('should detect self-references', () => {
      const context: ValidationContext = {
        device: {
          deviceKey: 'Device1',
          pageDescription: 'See {THING:Device1} for more',
        },
        knownDeviceKeys: new Set(['Device1']),
        assetRegistry: new Map(),
      };

      const errors = validator.validate(context);
      const selfLinkError = errors.find(e => e.ruleId === VALIDATION_RULE_IDS.SELF_LINK);
      expect(selfLinkError).toBeDefined();
    });
  });

  describe('Asset Validation', () => {
    it('should detect missing image files', () => {
      const assetRegistry = new Map();
      // Add some assets so the check is active
      assetRegistry.set('existing.png', { path: '/assets/existing.png', usageCount: 0 });

      const context: ValidationContext = {
        device: {
          deviceKey: 'Device1',
          operationalDetails: [
            {
              title: 'Section',
              imageFile: 'nonexistent.png',
            },
          ],
        },
        knownDeviceKeys: new Set(['Device1']),
        assetRegistry,
      };

      const errors = validator.validate(context);
      const missingAssetError = errors.find(e => e.ruleId === VALIDATION_RULE_IDS.MISSING_ASSET);
      expect(missingAssetError).toBeDefined();
      expect(missingAssetError?.message).toContain('nonexistent.png');
    });

    it('should validate existing image files', () => {
      const assetRegistry = new Map();
      assetRegistry.set('test.png', { path: '/assets/test.png', usageCount: 0 });

      const context: ValidationContext = {
        device: {
          deviceKey: 'Device1',
          operationalDetails: [
            {
              title: 'Section',
              imageFile: 'test.png',
            },
          ],
        },
        knownDeviceKeys: new Set(['Device1']),
        assetRegistry,
      };

      const errors = validator.validate(context);
      const missingAssetErrors = errors.filter(e => e.ruleId === VALIDATION_RULE_IDS.MISSING_ASSET);
      expect(missingAssetErrors).toHaveLength(0);
    });
  });

  describe('Field Validation', () => {
    it('should require deviceKey', () => {
      const context: ValidationContext = {
        device: {
          pageDescription: 'No device key',
        },
        knownDeviceKeys: new Set(),
        assetRegistry: new Map(),
      };

      const errors = validator.validate(context);
      const missingKeyError = errors.find(e => e.ruleId === VALIDATION_RULE_IDS.MISSING_DEVICE_KEY);
      expect(missingKeyError).toBeDefined();
    });

    it('should warn about empty operational detail title', () => {
      const context: ValidationContext = {
        device: {
          deviceKey: 'Device1',
          operationalDetails: [
            {
              title: '',
              description: 'Some content',
            },
          ],
        },
        knownDeviceKeys: new Set(['Device1']),
        assetRegistry: new Map(),
      };

      const errors = validator.validate(context);
      const emptyTitleError = errors.find(e => e.ruleId === VALIDATION_RULE_IDS.EMPTY_TITLE);
      expect(emptyTitleError).toBeDefined();
    });

    it('should allow non-empty titles', () => {
      const context: ValidationContext = {
        device: {
          deviceKey: 'Device1',
          operationalDetails: [
            {
              title: 'Valid Title',
              description: 'Content',
            },
          ],
        },
        knownDeviceKeys: new Set(['Device1']),
        assetRegistry: new Map(),
      };

      const errors = validator.validate(context);
      const emptyTitleErrors = errors.filter(e => e.ruleId === VALIDATION_RULE_IDS.EMPTY_TITLE);
      expect(emptyTitleErrors).toHaveLength(0);
    });
  });

  describe('Structure Validation', () => {
    it('should detect empty content sections', () => {
      const context: ValidationContext = {
        device: {
          deviceKey: 'Device1',
          operationalDetails: [
            {
              title: 'Empty Section',
              // No description, items, steps, or children
            },
          ],
        },
        knownDeviceKeys: new Set(['Device1']),
        assetRegistry: new Map(),
      };

      const errors = validator.validate(context);
      const emptyContentError = errors.find(e => e.ruleId === VALIDATION_RULE_IDS.EMPTY_CONTENT);
      expect(emptyContentError).toBeDefined();
    });

    it('should allow sections with any content type', () => {
      const context: ValidationContext = {
        device: {
          deviceKey: 'Device1',
          operationalDetails: [
            {
              title: 'Section 1',
              description: 'Description only',
            },
            {
              title: 'Section 2',
              items: ['Item 1', 'Item 2'],
            },
            {
              title: 'Section 3',
              steps: ['Step 1'],
            },
            {
              title: 'Section 4',
              children: [{ title: 'Child', description: 'Content' }],
            },
          ],
        },
        knownDeviceKeys: new Set(['Device1']),
        assetRegistry: new Map(),
      };

      const errors = validator.validate(context);
      const emptyContentErrors = errors.filter(e => e.ruleId === VALIDATION_RULE_IDS.EMPTY_CONTENT);
      expect(emptyContentErrors).toHaveLength(0);
    });

    it('should detect duplicate tocIds', () => {
      const context: ValidationContext = {
        device: {
          deviceKey: 'Device1',
          operationalDetails: [
            {
              title: 'Section 1',
              tocId: 'duplicate',
              description: 'Content',
            },
            {
              title: 'Section 2',
              tocId: 'duplicate',
              description: 'Content',
            },
          ],
        },
        knownDeviceKeys: new Set(['Device1']),
        assetRegistry: new Map(),
      };

      const errors = validator.validate(context);
      const duplicateTocError = errors.find(e => e.ruleId === VALIDATION_RULE_IDS.DUPLICATE_TOC_ID);
      expect(duplicateTocError).toBeDefined();
    });
  });

  describe('Nested Content Validation', () => {
    it('should validate nested operational details recursively', () => {
      const assetRegistry = new Map();
      assetRegistry.set('existing.png', { path: '/assets/existing.png', usageCount: 0 });

      const context: ValidationContext = {
        device: {
          deviceKey: 'Device1',
          operationalDetails: [
            {
              title: 'Parent',
              description: 'Parent content',
              children: [
                {
                  title: 'Child',
                  imageFile: 'missing.png',
                },
              ],
            },
          ],
        },
        knownDeviceKeys: new Set(['Device1']),
        assetRegistry,
      };

      const errors = validator.validate(context);
      const missingAssetError = errors.find(e => e.ruleId === VALIDATION_RULE_IDS.MISSING_ASSET);
      expect(missingAssetError).toBeDefined();
      expect(missingAssetError?.location.childPath).toEqual([0, 0]);
    });

    it('should track nested validation location correctly', () => {
      const context: ValidationContext = {
        device: {
          deviceKey: 'Device1',
          operationalDetails: [
            {
              title: 'Parent',
              description: 'Content',
              children: [
                {
                  title: 'Child',
                  description: 'Broken link: {THING:Invalid}',
                },
              ],
            },
          ],
        },
        knownDeviceKeys: new Set(['Device1']),
        assetRegistry: new Map(),
      };

      const errors = validator.validate(context);
      const linkError = errors.find(e => e.ruleId === VALIDATION_RULE_IDS.BROKEN_LINK);
      expect(linkError).toBeDefined();
      expect(linkError?.location.childPath).toEqual([0, 0]);
    });
  });

  describe('Multiple Content Locations', () => {
    it('should validate links in all description fields', () => {
      const context: ValidationContext = {
        device: {
          deviceKey: 'Device1',
          pageDescription: 'See {THING:Invalid}',
          operationalDetails: [
            {
              title: 'Section',
              description: 'Also see {THING:Invalid}',
            },
          ],
        },
        knownDeviceKeys: new Set(['Device1']),
        assetRegistry: new Map(),
      };

      const errors = validator.validate(context);
      const linkErrors = errors.filter(e => e.ruleId === VALIDATION_RULE_IDS.BROKEN_LINK);
      expect(linkErrors.length).toBeGreaterThanOrEqual(1);
    });
  });

  describe('Batch Validation', () => {
    it('should validate multiple devices and report by device', () => {
      const devices = [
        {
          deviceKey: 'Device1',
          pageDescription: 'Valid device',
        },
        {
          // Missing deviceKey
          pageDescription: 'Invalid device',
        },
        {
          deviceKey: 'Device3',
          pageDescription: 'Another {THING:Device1}',
        },
      ];

      const allDeviceKeys = new Set(['Device1', 'Device3']);
      const results = devices.map(device => {
        const context: ValidationContext = {
          device,
          knownDeviceKeys: allDeviceKeys,
          assetRegistry: new Map(),
        };
        const errors = validator.validate(context);
        return {
          deviceKey: device.deviceKey || 'UNKNOWN',
          errors,
          hasErrors: errors.some(e => e.severity === 'error'),
        };
      });

      expect(results[1].hasErrors).toBe(true);
      expect(results[1].errors.some(e => e.ruleId === VALIDATION_RULE_IDS.MISSING_DEVICE_KEY)).toBe(true);
    });
  });
});
