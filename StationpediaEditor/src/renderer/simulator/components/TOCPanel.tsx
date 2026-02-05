/**
 * TOCPanel - Table of Contents panel component
 * Auto-generate from operational details with tocId
 * Smooth scroll navigation
 */

import React, { useMemo } from 'react';
import type { OperationalDetail } from '@models/contentModel';

interface TOCItem {
  id: string;
  title: string;
  depth: number;
}

interface TOCPanelProps {
  operationalDetails?: OperationalDetail[];
  title?: string;
  onItemClick?: (tocId: string) => void;
}

/**
 * Extract TOC items from operational details
 */
function extractTocItems(details: OperationalDetail[], depth = 0): TOCItem[] {
  const items: TOCItem[] = [];

  details.forEach((detail) => {
    if (detail.tocId) {
      items.push({
        id: detail.tocId,
        title: detail.title,
        depth,
      });
    }

    // Recursively extract from children
    if (detail.children && detail.children.length > 0) {
      items.push(...extractTocItems(detail.children, depth + 1));
    }
  });

  return items;
}

export const TOCPanel: React.FC<TOCPanelProps> = ({
  operationalDetails,
  title = 'Table of Contents',
  onItemClick,
}) => {
  const tocItems = useMemo(() => {
    if (!operationalDetails || operationalDetails.length === 0) {
      return [];
    }
    return extractTocItems(operationalDetails);
  }, [operationalDetails]);

  if (tocItems.length === 0) {
    return null;
  }

  const handleClick = (tocId: string) => {
    if (onItemClick) {
      onItemClick(tocId);
    }

    // Smooth scroll to element
    setTimeout(() => {
      const element = document.getElementById(tocId);
      if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    }, 0);
  };

  return (
    <nav className="toc-panel mb-4 p-4 rounded border border-[#264D73] bg-black/30" data-testid="toc">
      <h3 className="text-sm font-bold text-[#FF7A18] mb-3">{title}</h3>

      <ul className="space-y-1 text-sm">
        {tocItems.map((item) => (
          <li key={item.id} style={{ marginLeft: `${item.depth * 1.25}rem` }}>
            <button
              onClick={() => handleClick(item.id)}
              className="text-gray-300 hover:text-[#008AE6] transition-colors text-left hover:underline"
            >
              {item.title}
            </button>
          </li>
        ))}
      </ul>
    </nav>
  );
};

export default TOCPanel;
