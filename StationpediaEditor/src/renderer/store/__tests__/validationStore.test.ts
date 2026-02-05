/**
 * Tests for validation store
 */
import { describe, it, expect, beforeEach } from 'vitest';
import { createValidationStore } from '../validationStore';
import type { ValidationResult, ValidationError } from '@models/validationModel';
import { VALIDATION_RULE_IDS } from '@models/validationModel';

describe('ValidationStore', () => {
  beforeEach(() => {
    // Create a fresh store for each test
  });

  describe('Result Management', () => {
    it('should store validation results', () => {
      const store = createValidationStore();
      const result: ValidationResult = {
        deviceKey: 'Device1',
        errors: [],
        hasErrors: false,
        hasWarnings: false,
        hasInfo: false,
      };

      store.setResult('Device1', result);
      const retrieved = store.getResult('Device1');

      expect(retrieved).toEqual(result);
    });

    it('should update existing results', () => {
      const store = createValidationStore();
      const result1: ValidationResult = {
        deviceKey: 'Device1',
        errors: [],
        hasErrors: false,
        hasWarnings: false,
        hasInfo: false,
      };

      const result2: ValidationResult = {
        deviceKey: 'Device1',
        errors: [
          {
            ruleId: VALIDATION_RULE_IDS.MISSING_DEVICE_KEY,
            severity: 'error',
            message: 'Device key required',
            location: { field: 'deviceKey' },
          },
        ],
        hasErrors: true,
        hasWarnings: false,
        hasInfo: false,
      };

      store.setResult('Device1', result1);
      expect(store.getResult('Device1')?.errors).toHaveLength(0);

      store.setResult('Device1', result2);
      expect(store.getResult('Device1')?.errors).toHaveLength(1);
    });

    it('should return undefined for non-existent results', () => {
      const store = createValidationStore();
      expect(store.getResult('NonExistent')).toBeUndefined();
    });
  });

  describe('Error Counting', () => {
    it('should count all errors', () => {
      const store = createValidationStore();

      store.setResult('Device1', {
        deviceKey: 'Device1',
        errors: [
          {
            ruleId: VALIDATION_RULE_IDS.MISSING_DEVICE_KEY,
            severity: 'error',
            message: 'Device key required',
            location: { field: 'deviceKey' },
          },
          {
            ruleId: VALIDATION_RULE_IDS.BROKEN_LINK,
            severity: 'warning',
            message: 'Broken link',
            location: { field: 'pageDescription' },
          },
        ],
        hasErrors: true,
        hasWarnings: true,
        hasInfo: false,
      });

      store.setResult('Device2', {
        deviceKey: 'Device2',
        errors: [
          {
            ruleId: VALIDATION_RULE_IDS.INVALID_COLOR,
            severity: 'warning',
            message: 'Invalid color',
            location: { field: 'description' },
          },
        ],
        hasErrors: false,
        hasWarnings: true,
        hasInfo: false,
      });

      expect(store.getErrorCount()).toBe(3);
      expect(store.getErrorCount('error')).toBe(1);
      expect(store.getErrorCount('warning')).toBe(2);
      expect(store.getErrorCount('info')).toBe(0);
    });
  });

  describe('Device Error Tracking', () => {
    it('should identify devices with errors', () => {
      const store = createValidationStore();

      store.setResult('Device1', {
        deviceKey: 'Device1',
        errors: [
          {
            ruleId: VALIDATION_RULE_IDS.MISSING_DEVICE_KEY,
            severity: 'error',
            message: 'Error',
            location: {},
          },
        ],
        hasErrors: true,
        hasWarnings: false,
        hasInfo: false,
      });

      store.setResult('Device2', {
        deviceKey: 'Device2',
        errors: [],
        hasErrors: false,
        hasWarnings: false,
        hasInfo: false,
      });

      store.setResult('Device3', {
        deviceKey: 'Device3',
        errors: [
          {
            ruleId: VALIDATION_RULE_IDS.BROKEN_LINK,
            severity: 'warning',
            message: 'Warning',
            location: {},
          },
        ],
        hasErrors: false,
        hasWarnings: true,
        hasInfo: false,
      });

      const devicesWithErrors = store.getDevicesWithErrors();
      expect(devicesWithErrors).toContain('Device1');
      expect(devicesWithErrors).toContain('Device3');
      expect(devicesWithErrors).not.toContain('Device2');
    });
  });

  describe('Running State', () => {
    it('should track validation running state', () => {
      const store = createValidationStore();

      expect(store.getState().isRunning).toBe(false);

      store.setRunning(true);
      expect(store.getState().isRunning).toBe(true);

      store.setRunning(false);
      expect(store.getState().isRunning).toBe(false);
    });
  });

  describe('Severity Filtering', () => {
    it('should toggle severity filters', () => {
      const store = createValidationStore();

      // Initially all severities should be selected
      store.toggleSeverityFilter('error');
      store.toggleSeverityFilter('warning');

      const filtered = store.getFilteredErrors();
      // Should only contain info errors now

      store.setResult('Device1', {
        deviceKey: 'Device1',
        errors: [
          {
            ruleId: VALIDATION_RULE_IDS.MISSING_DEVICE_KEY,
            severity: 'error',
            message: 'Error',
            location: {},
          },
          {
            ruleId: VALIDATION_RULE_IDS.BROKEN_LINK,
            severity: 'warning',
            message: 'Warning',
            location: {},
          },
          {
            ruleId: VALIDATION_RULE_IDS.EMPTY_CONTENT,
            severity: 'info',
            message: 'Info',
            location: {},
          },
        ],
        hasErrors: true,
        hasWarnings: true,
        hasInfo: true,
      });

      const allFiltered = store.getFilteredErrors();
      expect(allFiltered.length).toBeGreaterThan(0);
    });
  });

  describe('Clear', () => {
    it('should clear all results', () => {
      const store = createValidationStore();

      store.setResult('Device1', {
        deviceKey: 'Device1',
        errors: [],
        hasErrors: false,
        hasWarnings: false,
        hasInfo: false,
      });

      store.setResult('Device2', {
        deviceKey: 'Device2',
        errors: [],
        hasErrors: false,
        hasWarnings: false,
        hasInfo: false,
      });

      expect(store.getErrorCount()).toBe(0);

      store.clear();

      expect(store.getResult('Device1')).toBeUndefined();
      expect(store.getResult('Device2')).toBeUndefined();
      expect(store.getDevicesWithErrors()).toHaveLength(0);
    });
  });
});
