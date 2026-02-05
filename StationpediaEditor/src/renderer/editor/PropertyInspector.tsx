/**
 * PropertyInspector - Edit properties of selected operational detail or device
 */
import React, { useState, useCallback, useEffect } from 'react';
import type { OperationalDetail, DeviceDocument } from '@models/contentModel';

interface PropertyInspectorProps {
  device?: DeviceDocument | null;
  detail?: OperationalDetail | null;
  onChange?: (updates: Record<string, unknown>) => void;
}

export const PropertyInspector: React.FC<PropertyInspectorProps> = ({
  device,
  detail,
  onChange,
}) => {
  const [localUpdates, setLocalUpdates] = useState<Record<string, unknown>>({});

  // Reset on device/detail change
  useEffect(() => {
    setLocalUpdates({});
  }, [device, detail]);

  const handleChange = useCallback(
    (field: string, value: unknown) => {
      const updates = { ...localUpdates, [field]: value };
      setLocalUpdates(updates);

      // Debounce onChange calls
      const timer = setTimeout(() => {
        onChange?.(updates);
      }, 500);

      return () => clearTimeout(timer);
    },
    [localUpdates, onChange]
  );

  const renderField = (label: string, field: string, type: 'text' | 'textarea' | 'checkbox' = 'text', value: unknown = '') => {
    return (
      <div key={field} className="mb-4">
        <label className="block text-sm font-semibold text-gray-300 mb-1">
          {label}
        </label>
        {type === 'textarea' ? (
          <textarea
            value={String(value)}
            onChange={(e) => handleChange(field, e.target.value)}
            className="w-full px-3 py-2 bg-stationpedia-surface border border-stationpedia-border rounded text-sm text-white placeholder-gray-500 focus:outline-none focus:border-stationpedia-accent focus:ring-1 focus:ring-stationpedia-accent"
            rows={4}
          />
        ) : type === 'checkbox' ? (
          <input
            type="checkbox"
            checked={Boolean(value)}
            onChange={(e) => handleChange(field, e.target.checked)}
            className="w-4 h-4 rounded accent-cyan-600"
          />
        ) : (
          <input
            type={type}
            value={String(value)}
            onChange={(e) => handleChange(field, e.target.value)}
            className="w-full px-3 py-2 bg-stationpedia-surface border border-stationpedia-border rounded text-sm text-white placeholder-gray-500 focus:outline-none focus:border-stationpedia-accent focus:ring-1 focus:ring-stationpedia-accent"
          />
        )}
      </div>
    );
  };

  if (!device && !detail) {
    return (
      <div className="flex flex-col h-full bg-stationpedia-bg border-l border-stationpedia-border p-4 text-center text-gray-500">
        <div className="py-8">Select a device or detail to edit</div>
      </div>
    );
  }

  const title = detail ? 'Operational Detail' : 'Device';

  return (
    <div className="flex flex-col h-full bg-stationpedia-bg border-l border-stationpedia-border overflow-auto">
      <div className="p-4 border-b border-stationpedia-border">
        <h3 className="text-lg font-semibold text-stationpedia-accent">{title} Properties</h3>
      </div>

      <div className="flex-1 overflow-auto p-4 space-y-4">
        {detail ? (
          <>
            {renderField('Title', 'title', 'text', detail.title)}
            {renderField('Description', 'description', 'textarea', detail.description)}
            {renderField('TOC ID', 'tocId', 'text', detail.tocId)}
            {renderField('Collapsible', 'collapsible', 'checkbox', detail.collapsible)}
            {renderField('Background Color', 'backgroundColor', 'text', detail.backgroundColor)}
            {renderField('Image File', 'imageFile', 'text', detail.imageFile)}
            {renderField('YouTube URL', 'youtubeUrl', 'text', detail.youtubeUrl)}
            {renderField('YouTube Label', 'youtubeLabel', 'text', detail.youtubeLabel)}
            {renderField('Video File', 'videoFile', 'text', detail.videoFile)}
          </>
        ) : device ? (
          <>
            {renderField('Device Key', 'deviceKey', 'text', device.deviceKey)}
            {renderField('Display Name', 'displayName', 'text', device.displayName)}
            {renderField('Page Description', 'pageDescription', 'textarea', device.pageDescription)}
            {renderField(
              'Page Description Prepend',
              'pageDescriptionPrepend',
              'textarea',
              device.pageDescriptionPrepend
            )}
            {renderField(
              'Page Description Append',
              'pageDescriptionAppend',
              'textarea',
              device.pageDescriptionAppend
            )}
            {renderField('Generate TOC', 'generateToc', 'checkbox', device.generateToc)}
            {renderField('TOC Title', 'tocTitle', 'text', device.tocTitle)}
            {renderField(
              'Operational Details Title Color',
              'operationalDetailsTitleColor',
              'text',
              device.operationalDetailsTitleColor
            )}
            {renderField(
              'Operational Details Background Color',
              'operationalDetailsBackgroundColor',
              'text',
              device.operationalDetailsBackgroundColor
            )}
          </>
        ) : null}
      </div>
    </div>
  );
};
