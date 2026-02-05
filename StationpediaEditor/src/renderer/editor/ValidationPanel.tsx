/**
 * ValidationPanel - Display validation errors/warnings in the editor
 */

import React, { useMemo, useState } from 'react';
import type { ValidationResult, ValidationSeverity, ValidationError } from '@models/validationModel';

interface ValidationPanelProps {
  results?: Map<string, ValidationResult>;
  selectedSeverities?: Set<ValidationSeverity>;
  isRunning?: boolean;
  onToggleSeverity?: (severity: ValidationSeverity) => void;
  onSelectError?: (error: ValidationError, deviceKey: string) => void;
}

const severityColors = {
  error: 'bg-red-900 border-red-600 text-red-100',
  warning: 'bg-yellow-900 border-yellow-600 text-yellow-100',
  info: 'bg-blue-900 border-blue-600 text-blue-100',
};

const severityIcons = {
  error: '✕',
  warning: '⚠',
  info: 'ℹ',
};

// Default empty map and set for when props are not provided
const emptyResults = new Map<string, ValidationResult>();
const defaultSeverities = new Set<ValidationSeverity>(['error', 'warning', 'info']);

export const ValidationPanel: React.FC<ValidationPanelProps> = ({
  results = emptyResults,
  selectedSeverities: selectedSeveritiesProp,
  isRunning = false,
  onToggleSeverity,
  onSelectError,
}) => {
  // Local state for severities if no callback provided
  const [localSeverities, setLocalSeverities] = useState<Set<ValidationSeverity>>(defaultSeverities);
  const selectedSeverities = selectedSeveritiesProp ?? localSeverities;

  const handleToggleSeverity = (severity: ValidationSeverity) => {
    if (onToggleSeverity) {
      onToggleSeverity(severity);
    } else {
      setLocalSeverities(prev => {
        const newSet = new Set(prev);
        if (newSet.has(severity)) {
          newSet.delete(severity);
        } else {
          newSet.add(severity);
        }
        return newSet;
      });
    }
  };

  const filteredErrors = useMemo(() => {
    const errors: Array<{ error: ValidationError; deviceKey: string }> = [];
    
    // Safety check - results must be iterable (Map)
    if (!results || typeof results.entries !== 'function') {
      return errors;
    }

    for (const [deviceKey, result] of results) {
      for (const error of result.errors) {
        if (selectedSeverities.has(error.severity)) {
          errors.push({ error, deviceKey });
        }
      }
    }

    return errors;
  }, [results, selectedSeverities]);

  const errorsByDevice = useMemo(() => {
    const grouped = new Map<string, Array<{ error: ValidationError; deviceKey: string }>>();

    for (const item of filteredErrors) {
      if (!grouped.has(item.deviceKey)) {
        grouped.set(item.deviceKey, []);
      }
      grouped.get(item.deviceKey)!.push(item);
    }

    return grouped;
  }, [filteredErrors]);

  const totalErrors = filteredErrors.length;
  
  // Safe iteration over results with null check
  const resultsArray = results && typeof results.values === 'function' ? Array.from(results.values()) : [];
  const errorsCount = resultsArray.reduce((sum, r) => sum + r.errors.filter(e => e.severity === 'error').length, 0);
  const warningsCount = resultsArray.reduce((sum, r) => sum + r.errors.filter(e => e.severity === 'warning').length, 0);
  const infosCount = resultsArray.reduce((sum, r) => sum + r.errors.filter(e => e.severity === 'info').length, 0);

  return (
    <div className="flex flex-col h-full bg-gray-900 border-t border-gray-700">
      {/* Header */}
      <div className="p-3 border-b border-gray-700 bg-gray-800">
        <div className="flex items-center justify-between mb-2">
          <h3 className="font-semibold text-gray-100">Validation</h3>
          {isRunning && <div className="text-xs text-cyan-400 animate-pulse">Running...</div>}
        </div>

        {/* Summary */}
        {totalErrors === 0 && !isRunning ? (
          <div className="text-sm text-green-400">✓ No issues found</div>
        ) : (
          <div className="text-xs text-gray-300">
            {totalErrors} issue{totalErrors !== 1 ? 's' : ''} found
          </div>
        )}
      </div>

      {/* Severity Filters */}
      <div className="flex gap-2 p-2 bg-gray-800 border-b border-gray-700">
        {(['error', 'warning', 'info'] as const).map(severity => (
          <button
            key={severity}
            onClick={() => handleToggleSeverity(severity)}
            className={`px-2 py-1 text-xs rounded transition-colors ${
              selectedSeverities.has(severity)
                ? `${severityColors[severity]} border`
                : 'bg-gray-700 text-gray-400 border border-gray-600'
            }`}
          >
            {severity.charAt(0).toUpperCase() + severity.slice(1)} ({
              severity === 'error'
                ? errorsCount
                : severity === 'warning'
                  ? warningsCount
                  : infosCount
            })
          </button>
        ))}
      </div>

      {/* Error List */}
      <div className="flex-1 overflow-auto">
        {filteredErrors.length === 0 ? (
          <div className="p-4 text-center text-gray-500 text-sm">
            No issues with selected filters
          </div>
        ) : (
          <div className="divide-y divide-gray-700">
            {Array.from(errorsByDevice).map(([deviceKey, deviceErrors]) => (
              <div key={deviceKey} className="border-b border-gray-700">
                <div className="px-3 py-2 bg-gray-800 font-semibold text-sm text-gray-300">
                  {deviceKey}
                </div>

                <div className="divide-y divide-gray-700">
                  {deviceErrors.map((item, index) => (
                    <button
                      key={`${deviceKey}-${index}`}
                      onClick={() => onSelectError?.(item.error, item.deviceKey)}
                      className={`w-full text-left px-3 py-2 transition-colors hover:bg-gray-700 ${severityColors[item.error.severity]} border-l-4 ${
                        item.error.severity === 'error'
                          ? 'border-red-600'
                          : item.error.severity === 'warning'
                            ? 'border-yellow-600'
                            : 'border-blue-600'
                      }`}
                    >
                      <div className="flex items-start gap-2">
                        <span className="flex-shrink-0 mt-0.5">
                          {severityIcons[item.error.severity]}
                        </span>
                        <div className="flex-1 min-w-0">
                          <div className="font-semibold text-sm">{item.error.message}</div>
                          {item.error.location.field && (
                            <div className="text-xs opacity-75 truncate">
                              {item.error.location.field}
                              {item.error.location.sectionIndex !== undefined &&
                                ` [${item.error.location.sectionIndex}]`}
                            </div>
                          )}
                        </div>
                      </div>
                    </button>
                  ))}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};
