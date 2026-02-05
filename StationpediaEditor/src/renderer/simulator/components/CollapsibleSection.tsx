/**
 * CollapsibleSection - Collapsible section component with expand/collapse animation
 * Uses game-accurate colors: Background #0F1F38, Border #264D73, Orange #FF7A18
 */

import React, { useState } from 'react';

interface CollapsibleSectionProps {
  title: string;
  children: React.ReactNode;
  defaultOpen?: boolean;
  depth?: number;
  titleColor?: string;
  backgroundColor?: string;
}

export const CollapsibleSection: React.FC<CollapsibleSectionProps> = ({
  title,
  children,
  defaultOpen = true,
  depth = 0,
  titleColor,
  backgroundColor,
}) => {
  const [isOpen, setIsOpen] = useState(defaultOpen);

  const bgStyle = backgroundColor
    ? { backgroundColor }
    : depth === 0
      ? { backgroundColor: 'rgba(10, 21, 32, 0.8)' } // #0A1520 with opacity
      : depth === 1
        ? { backgroundColor: 'rgba(15, 31, 56, 0.5)' } // #0F1F38 with opacity
        : {};

  const titleStyle = titleColor ? { color: titleColor } : {};
  const buttonArrowColor = { color: '#FF7A18' }; // Game-accurate orange accent

  return (
    <div
      className={`rounded overflow-hidden ${depth === 0 ? 'mb-3' : 'mb-2'}`}
      data-testid="collapsible-section"
    >
      <button
        onClick={() => setIsOpen(!isOpen)}
        className={`w-full flex items-center gap-2 px-4 py-3 hover:bg-opacity-75 transition-all duration-200`}
        style={{
          ...bgStyle,
          opacity: 0.9,
        }}
        data-testid="section-toggle"
      >
        <span
          className={`transform transition-transform duration-300 text-sm flex-shrink-0 w-4 h-4 flex items-center justify-center ${
            isOpen ? 'rotate-90' : ''
          }`}
          style={buttonArrowColor}
        >
          ▶
        </span>
        <span
          className={`font-semibold text-left flex-1 ${depth === 0 ? 'text-base' : 'text-sm'}`}
          style={titleStyle}
        >
          {title}
        </span>
      </button>

      {isOpen && (
        <div
          className={`px-4 py-3 ${depth > 0 ? 'ml-4 border-l border-[#264D73]' : ''}`}
          style={{ backgroundColor: 'rgba(0, 0, 0, 0.2)' }}
          data-testid="section-content"
        >
          {children}
        </div>
      )}
    </div>
  );
};

export default CollapsibleSection;
