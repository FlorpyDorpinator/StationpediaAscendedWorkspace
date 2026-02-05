/**
 * LogicSection - Render logic descriptions in table format
 * Show data type, range, description
 */

import React from 'react';
import type { LogicDescription } from '@models/contentModel';

interface LogicSectionProps {
  logicDescriptions?: Record<string, LogicDescription>;
  onLinkClick?: (target: string) => void;
}

export const LogicSection: React.FC<LogicSectionProps> = ({
  logicDescriptions,
}) => {
  if (!logicDescriptions || Object.keys(logicDescriptions).length === 0) {
    return null;
  }

  return (
    <div className="logic-section">
      <table className="w-full text-sm border-collapse">
        <thead>
          <tr className="border-b border-[#264D73]">
            <th className="text-left py-2 px-3 text-[#FF7A18] font-semibold">Name</th>
            <th className="text-left py-2 px-3 text-[#FF7A18] font-semibold">Type</th>
            <th className="text-left py-2 px-3 text-[#FF7A18] font-semibold">Range</th>
            <th className="text-left py-2 px-3 text-[#FF7A18] font-semibold">Description</th>
          </tr>
        </thead>
        <tbody>
          {Object.entries(logicDescriptions).map(([key, logic], idx) => (
            <tr
              key={key}
              className={`border-b border-gray-700/50 ${idx % 2 === 0 ? 'bg-black/20' : ''}`}
            >
              <td className="py-2 px-3 font-mono text-gray-200">{key}</td>
              <td className="py-2 px-3 text-gray-300">{logic.dataType}</td>
              <td className="py-2 px-3 text-gray-400 font-mono text-xs">{logic.range}</td>
              <td className="py-2 px-3 text-gray-300 text-xs">{logic.description}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default LogicSection;
