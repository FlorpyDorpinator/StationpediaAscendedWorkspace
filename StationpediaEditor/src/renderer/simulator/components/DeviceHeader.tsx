/**
 * DeviceHeader - Device name/title header with game-like styling
 * Uses game-accurate orange accent color (#FF7A18) for title
 */

import React from 'react';

interface DeviceHeaderProps {
  displayName?: string | null;
  deviceKey: string;
  titleColor?: string;
}

export const DeviceHeader: React.FC<DeviceHeaderProps> = ({
  displayName,
  deviceKey,
  titleColor = '#FF7A18', // Game-accurate orange accent
}) => {
  const titleStyle = titleColor ? { color: titleColor } : {};

  return (
    <div
      className="device-header mb-6 pb-4 border-b border-[#264D73]"
      data-testid="device-header"
    >
      <h1
        className="text-3xl font-bold mb-2 font-mono"
        style={titleStyle}
      >
        {displayName || deviceKey}
      </h1>

      <code className="text-xs bg-black/50 px-2 py-1 rounded text-[#6E7681] font-mono">
        {deviceKey}
      </code>
    </div>
  );
};

export default DeviceHeader;
