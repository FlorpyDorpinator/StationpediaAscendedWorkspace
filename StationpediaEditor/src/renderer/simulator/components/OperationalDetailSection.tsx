/**
 * OperationalDetailSection - Render a single operational detail section
 * Handle title, description, items, steps, children (recursive)
 * Support background colors, images
 */

import React from 'react';
import type { OperationalDetail } from '@models/contentModel';
import { CollapsibleSection } from './CollapsibleSection';
import { RichTextRenderer } from './RichTextRenderer';

interface OperationalDetailSectionProps {
  detail: OperationalDetail;
  depth?: number;
  onLinkClick?: (target: string) => void;
}

export const OperationalDetailSection: React.FC<OperationalDetailSectionProps> = ({
  detail,
  depth = 0,
  onLinkClick,
}) => {
  const isCollapsible = detail.collapsible !== false;
  const defaultOpen = depth === 0 ? true : depth < 2;

  const sectionContent = (
    <div className="space-y-3">
      {/* Description with rich text */}
      {detail.description && (
        <div className="text-sm text-gray-200">
          <RichTextRenderer
            content={detail.description}
            onLinkClick={onLinkClick}
            className="prose prose-sm prose-invert"
          />
        </div>
      )}

      {/* Image if present */}
      {detail.imageFile && (
        <div className="my-3 rounded overflow-hidden">
          <img
            src={detail.imageFile}
            alt={detail.title}
            className="max-w-full h-auto"
          />
        </div>
      )}

      {/* Items list (bullet points) */}
      {detail.items && detail.items.length > 0 && (
        <ul className="ml-4 space-y-1 text-sm text-gray-300">
          {detail.items.map((item, i) => (
            <li key={i} className="flex items-start gap-2">
              <span className="text-[#FF7A18] flex-shrink-0 mt-0.5">•</span>
              <span className="flex-1"><RichTextRenderer content={item} onLinkClick={onLinkClick} /></span>
            </li>
          ))}
        </ul>
      )}

      {/* Steps list (numbered) */}
      {detail.steps && detail.steps.length > 0 && (
        <ol className="ml-4 space-y-1 text-sm text-gray-300">
          {detail.steps.map((step, i) => (
            <li key={i} className="flex items-start gap-2">
              <span className="text-[#FF7A18] flex-shrink-0 mt-0.5 font-semibold">{i + 1}.</span>
              <span className="flex-1"><RichTextRenderer content={step} onLinkClick={onLinkClick} /></span>
            </li>
          ))}
        </ol>
      )}

      {/* YouTube embed if present */}
      {detail.youtubeUrl && (
        <div className="my-3 rounded overflow-hidden">
          <iframe
            width="100%"
            height="315"
            src={`https://www.youtube.com/embed/${detail.youtubeUrl}`}
            title={detail.youtubeLabel || detail.title}
            frameBorder="0"
            allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
            allowFullScreen
          />
        </div>
      )}

      {/* Table if present - markdown-style with header row */}
      {detail.table && detail.table.length > 0 && (
        <div className="my-3 rounded overflow-hidden bg-[#0a1929]/80 border border-[#1e3a5f]/50">
          <table className="w-full text-sm text-gray-200">
            <thead>
              {detail.table[0]?.cells && (
                <tr className="border-b border-gray-600/50">
                  {detail.table[0].cells.map((cell, colIndex) => (
                    <th
                      key={colIndex}
                      className="px-3 py-2 text-center font-bold text-[#FFA500]"
                    >
                      <RichTextRenderer content={cell} onLinkClick={onLinkClick} />
                    </th>
                  ))}
                </tr>
              )}
            </thead>
            <tbody>
              {detail.table.slice(1).map((row, rowIndex) => (
                <tr
                  key={rowIndex}
                  className={rowIndex % 2 === 1 ? 'bg-white/5' : ''}
                >
                  {row.cells?.map((cell, colIndex) => (
                    <td key={colIndex} className="px-3 py-2 text-center">
                      <RichTextRenderer content={cell} onLinkClick={onLinkClick} />
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Nested children */}
      {detail.children && detail.children.length > 0 && (
        <div className="mt-2 space-y-2">
          {detail.children.map((child, i) => (
            <OperationalDetailSection
              key={i}
              detail={child}
              depth={depth + 1}
              onLinkClick={onLinkClick}
            />
          ))}
        </div>
      )}
    </div>
  );

  if (isCollapsible) {
    return (
      <CollapsibleSection
        title={detail.title}
        defaultOpen={defaultOpen}
        depth={depth}
        backgroundColor={detail.backgroundColor}
      >
        {sectionContent}
      </CollapsibleSection>
    );
  }

  // Non-collapsible section
  return (
    <div
      className="operational-detail-section mb-3 rounded p-3"
      style={detail.backgroundColor ? { backgroundColor: detail.backgroundColor } : {}}
      id={detail.tocId}
    >
      <h4
        className={`font-semibold mb-2 ${
          depth === 0 ? 'text-base text-[#FF7A18]' : 'text-sm text-gray-300'
        }`}
      >
        {detail.title}
      </h4>
      {sectionContent}
    </div>
  );
};

export default OperationalDetailSection;
