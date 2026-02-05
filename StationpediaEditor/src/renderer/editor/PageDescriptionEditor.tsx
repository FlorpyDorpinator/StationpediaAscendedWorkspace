/**
 * PageDescriptionEditor - Editor for device page description
 */
import React, { useCallback } from 'react';
import { RichTextEditor } from './RichTextEditor';
import { useEditorStore } from './editorStore';
import type { DeviceDocument } from '@models/contentModel';

interface PageDescriptionEditorProps {
  device: DeviceDocument | null;
  onUpdateDevice?: (updates: Record<string, unknown>) => void;
}

export const PageDescriptionEditor: React.FC<PageDescriptionEditorProps> = ({
  device,
  onUpdateDevice,
}) => {
  const workspace = useEditorStore((state) => state.workspace);
  
  const handleChange = useCallback(
    (content: string) => {
      onUpdateDevice?.({ pageDescription: content });
    },
    [onUpdateDevice]
  );

  if (!device) {
    return (
      <div className="flex items-center justify-center h-full text-gray-500">
        Select a device to edit
      </div>
    );
  }

  return (
    <div className="flex flex-col h-full gap-2 p-2">
      <div className="flex items-center justify-between px-2">
        <h3 className="font-semibold text-stationpedia-accent">
          {device.displayName || device.deviceKey}
        </h3>
        <div className="text-xs text-gray-500">{device.deviceKey}</div>
      </div>

      <RichTextEditor
        content={device.pageDescription || ''}
        onChange={handleChange}
        placeholder="Enter device page description..."
        workspace={workspace}
      />
    </div>
  );
};
