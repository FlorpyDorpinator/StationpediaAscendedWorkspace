import React, { useState } from 'react';
import type { ValidationError } from '@models/stationpedia';

interface ValidationPanelProps {
  errors: ValidationError[];
}

export const ValidationPanel: React.FC<ValidationPanelProps> = ({ errors }) => {
  const [isExpanded, setIsExpanded] = useState(false);
  const [filter, setFilter] = useState<'all' | 'error' | 'warning' | 'info'>('all');

  const errorCount = errors.filter((e) => e.type === 'error').length;
  const warningCount = errors.filter((e) => e.type === 'warning').length;
  const infoCount = errors.filter((e) => e.type === 'info').length;

  const filteredErrors = filter === 'all' 
    ? errors 
    : errors.filter(e => e.type === filter);

  const getIcon = (type: string) => {
    switch (type) {
      case 'error': return '❌';
      case 'warning': return '⚠️';
      case 'info': return 'ℹ️';
      default: return '•';
    }
  };

  return (
    <div className={`bg-stationpedia-surface border-t border-stationpedia-border flex flex-col transition-all ${isExpanded ? 'h-64' : 'h-10'}`}>
      {/* Header */}
      <div 
        className="bg-stationpedia-bg border-b border-stationpedia-border px-4 py-2 flex items-center justify-between cursor-pointer"
        onClick={() => setIsExpanded(!isExpanded)}
      >
        <div className="flex items-center gap-3">
          <span className={`transform transition-transform ${isExpanded ? 'rotate-90' : ''}`}>▶</span>
          <h3 className="font-bold text-stationpedia-text">Problems</h3>
          <div className="flex gap-3 text-xs">
            {errorCount > 0 && (
              <span className="text-red-400 flex items-center gap-1">
                <span className="w-4 h-4 rounded-full bg-red-500 text-white text-[10px] flex items-center justify-center font-bold">
                  {errorCount}
                </span>
                Errors
              </span>
            )}
            {warningCount > 0 && (
              <span className="text-yellow-400 flex items-center gap-1">
                <span className="w-4 h-4 rounded-full bg-yellow-500 text-black text-[10px] flex items-center justify-center font-bold">
                  {warningCount}
                </span>
                Warnings
              </span>
            )}
            {errors.length === 0 && (
              <span className="text-green-400">✓ No issues</span>
            )}
          </div>
        </div>
        
        {isExpanded && (
          <div className="flex gap-1" onClick={e => e.stopPropagation()}>
            {(['all', 'error', 'warning', 'info'] as const).map(type => (
              <button
                key={type}
                onClick={() => setFilter(type)}
                className={`px-2 py-0.5 text-xs rounded ${
                  filter === type
                    ? 'bg-stationpedia-accent text-stationpedia-bg'
                    : 'bg-stationpedia-border text-stationpedia-text hover:bg-stationpedia-border/80'
                }`}
              >
                {type.charAt(0).toUpperCase() + type.slice(1)}
              </button>
            ))}
          </div>
        )}
      </div>

      {/* Error List */}
      {isExpanded && (
        <div className="flex-1 overflow-y-auto p-3">
          {filteredErrors.length === 0 ? (
            <div className="text-stationpedia-text-muted text-sm text-center py-4">
              {errors.length === 0 ? 'No validation issues found' : 'No issues match the current filter'}
            </div>
          ) : (
            <div className="space-y-1">
              {filteredErrors.map((error, index) => (
                <div
                  key={index}
                  className={`p-2 rounded text-sm flex items-start gap-2 hover:bg-stationpedia-border/30 cursor-pointer ${
                    error.type === 'error'
                      ? 'text-red-200'
                      : error.type === 'warning'
                      ? 'text-yellow-200'
                      : 'text-blue-200'
                  }`}
                >
                  <span>{getIcon(error.type)}</span>
                  <div className="flex-1 min-w-0">
                    <span className="font-medium">{error.message}</span>
                    {error.field && (
                      <span className="ml-2 text-xs opacity-70">
                        [{error.field}]
                      </span>
                    )}
                    {error.deviceKey && (
                      <span className="ml-2 text-xs text-stationpedia-accent">
                        {error.deviceKey}
                      </span>
                    )}
                  </div>
                  {error.line && (
                    <span className="text-xs opacity-70">Ln {error.line}</span>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
};
