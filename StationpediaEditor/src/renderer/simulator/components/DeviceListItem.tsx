/**
 * DeviceListItem component
 * Displays a device with name, key, hover effects, and selection state
 */
import React from 'react';
import type { DeviceDescription } from '@renderer/models/stationpedia';

export interface DeviceListItemProps {
  device: DeviceDescription;
  onClick: (deviceKey: string) => void;
  isSelected: boolean;
}

export const DeviceListItem: React.FC<DeviceListItemProps> = ({
  device,
  onClick,
  isSelected,
}) => {
  return (
    <button
      onClick={() => onClick(device.deviceKey)}
      className={`
        w-full text-left px-3 py-2 rounded
        transition-colors
        ${
          isSelected
            ? 'bg-stationpedia-accent/30 text-stationpedia-accent'
            : 'text-stationpedia-text hover:bg-stationpedia-accent/10'
        }
        border border-transparent hover:border-stationpedia-accent
      `}
    >
      <div className="font-medium">{device.displayName || device.deviceKey}</div>
      {device.displayName && (
        <div className="text-xs text-stationpedia-text-muted">
          {device.deviceKey}
        </div>
      )}
    </button>
  );
};

export default DeviceListItem;
