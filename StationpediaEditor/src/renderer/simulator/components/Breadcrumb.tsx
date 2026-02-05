/**
 * Breadcrumb component
 * Displays navigation path with clickable segments and orange styling
 */
import React from 'react';

export interface BreadcrumbItem {
  label: string;
  onClick?: () => void;
}

export interface BreadcrumbProps {
  items: BreadcrumbItem[];
}

export const Breadcrumb: React.FC<BreadcrumbProps> = ({ items }) => {
  return (
    <div className="flex items-center gap-2">
      {items.map((item, index) => (
        <React.Fragment key={index}>
          {index > 0 && <span className="text-stationpedia-text-muted">&gt;</span>}
          {item.onClick ? (
            <button
              onClick={item.onClick}
              className="text-stationpedia-accent hover:text-[#ff8533] transition-colors hover:underline"
            >
              {item.label}
            </button>
          ) : (
            <span className="text-stationpedia-text">{item.label}</span>
          )}
        </React.Fragment>
      ))}
    </div>
  );
};

export default Breadcrumb;
