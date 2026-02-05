/**
 * Zustand store for managing validation state across the editor
 */

import { create } from 'zustand';
import type {
  ValidationResult,
  ValidationError,
  ValidationSeverity,
} from '@models/validationModel';

interface ValidationStoreState {
  results: Map<string, ValidationResult>;
  isRunning: boolean;
  lastRunTime?: number;
  selectedSeverities: Set<ValidationSeverity>;
  
  setResult: (deviceKey: string, result: ValidationResult) => void;
  getResult: (deviceKey: string) => ValidationResult | undefined;
  getErrorCount: (severity?: ValidationSeverity) => number;
  getDevicesWithErrors: () => string[];
  setRunning: (running: boolean) => void;
  clear: () => void;
  toggleSeverityFilter: (severity: ValidationSeverity) => void;
  getFilteredErrors: () => ValidationError[];
  getState: () => {
    results: Map<string, ValidationResult>;
    isRunning: boolean;
    lastRunTime?: number;
    selectedSeverities: Set<ValidationSeverity>;
  };
}

export const createValidationStore = () => {
  const store = create<ValidationStoreState>((set, get) => ({
    results: new Map(),
    isRunning: false,
    lastRunTime: undefined,
    selectedSeverities: new Set(['error', 'warning', 'info']),

    setResult: (deviceKey: string, result: ValidationResult) => {
      set(state => {
        const newResults = new Map(state.results);
        newResults.set(deviceKey, result);
        return { results: newResults };
      });
    },

    getResult: (deviceKey: string) => {
      return get().results.get(deviceKey);
    },

    getErrorCount: (severity?: ValidationSeverity) => {
      const results = Array.from(get().results.values());
      const allErrors = results.flatMap(r => r.errors);

      if (severity) {
        return allErrors.filter(e => e.severity === severity).length;
      }
      return allErrors.length;
    },

    getDevicesWithErrors: () => {
      const results = Array.from(get().results.values());
      return results
        .filter(r => r.hasErrors || r.hasWarnings)
        .map(r => r.deviceKey);
    },

    setRunning: (running: boolean) => {
      set({ isRunning: running, lastRunTime: running ? undefined : Date.now() });
    },

    clear: () => {
      set({ results: new Map() });
    },

    toggleSeverityFilter: (severity: ValidationSeverity) => {
      set(state => {
        const newSeverities = new Set(state.selectedSeverities);
        if (newSeverities.has(severity)) {
          newSeverities.delete(severity);
        } else {
          newSeverities.add(severity);
        }
        return { selectedSeverities: newSeverities };
      });
    },

    getFilteredErrors: () => {
      const results = Array.from(get().results.values());
      const allErrors = results.flatMap(r => r.errors);
      const severities = get().selectedSeverities;

      return allErrors.filter(e => severities.has(e.severity));
    },

    getState: () => {
      const state = get();
      return {
        results: state.results,
        isRunning: state.isRunning,
        lastRunTime: state.lastRunTime,
        selectedSeverities: state.selectedSeverities,
      };
    },
  }));

  // Return the store state accessor
  return {
    setResult: store.getState().setResult,
    getResult: store.getState().getResult,
    getErrorCount: store.getState().getErrorCount,
    getDevicesWithErrors: store.getState().getDevicesWithErrors,
    setRunning: store.getState().setRunning,
    clear: store.getState().clear,
    toggleSeverityFilter: store.getState().toggleSeverityFilter,
    getFilteredErrors: store.getState().getFilteredErrors,
    getState: store.getState().getState,
  };
};

export const useValidationStore = create<ValidationStoreState>((set, get) => ({
  results: new Map(),
  isRunning: false,
  lastRunTime: undefined,
  selectedSeverities: new Set(['error', 'warning', 'info']),

  setResult: (deviceKey: string, result: ValidationResult) => {
    set(state => {
      const newResults = new Map(state.results);
      newResults.set(deviceKey, result);
      return { results: newResults };
    });
  },

  getResult: (deviceKey: string) => {
    return get().results.get(deviceKey);
  },

  getErrorCount: (severity?: ValidationSeverity) => {
    const results = Array.from(get().results.values());
    const allErrors = results.flatMap(r => r.errors);

    if (severity) {
      return allErrors.filter(e => e.severity === severity).length;
    }
    return allErrors.length;
  },

  getDevicesWithErrors: () => {
    const results = Array.from(get().results.values());
    return results
      .filter(r => r.hasErrors || r.hasWarnings)
      .map(r => r.deviceKey);
  },

  setRunning: (running: boolean) => {
    set({ isRunning: running, lastRunTime: running ? undefined : Date.now() });
  },

  clear: () => {
    set({ results: new Map() });
  },

  toggleSeverityFilter: (severity: ValidationSeverity) => {
    set(state => {
      const newSeverities = new Set(state.selectedSeverities);
      if (newSeverities.has(severity)) {
        newSeverities.delete(severity);
      } else {
        newSeverities.add(severity);
      }
      return { selectedSeverities: newSeverities };
    });
  },

  getFilteredErrors: () => {
    const results = Array.from(get().results.values());
    const allErrors = results.flatMap(r => r.errors);
    const severities = get().selectedSeverities;

    return allErrors.filter(e => severities.has(e.severity));
  },

  getState: () => {
    const state = get();
    return {
      results: state.results,
      isRunning: state.isRunning,
      lastRunTime: state.lastRunTime,
      selectedSeverities: state.selectedSeverities,
    };
  },
}));
