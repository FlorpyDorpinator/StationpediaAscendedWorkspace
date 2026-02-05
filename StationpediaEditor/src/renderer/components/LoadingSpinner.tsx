import React from 'react';

interface LoadingSpinnerProps {
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

export const LoadingSpinner: React.FC<LoadingSpinnerProps> = ({
  size = 'md',
  className = '',
}) => {
  const sizeClasses = {
    sm: 'w-4 h-4 border-2',
    md: 'w-6 h-6 border-2',
    lg: 'w-10 h-10 border-3',
  };

  return (
    <div
      className={`
        ${sizeClasses[size]}
        border-stationpedia-accent
        border-t-transparent
        rounded-full
        animate-spin
        ${className}
      `}
      role="status"
      aria-label="Loading"
    />
  );
};

// Full page loading overlay
export const LoadingOverlay: React.FC<{ message?: string }> = ({ message }) => {
  return (
    <div className="fixed inset-0 bg-stationpedia-bg/80 flex flex-col items-center justify-center z-50">
      <LoadingSpinner size="lg" />
      {message && (
        <p className="mt-4 text-stationpedia-text-muted text-sm animate-pulse">
          {message}
        </p>
      )}
    </div>
  );
};

// Skeleton loading placeholder
export const Skeleton: React.FC<{
  className?: string;
  variant?: 'text' | 'circular' | 'rectangular';
}> = ({ className = '', variant = 'text' }) => {
  const variantClasses = {
    text: 'h-4 rounded',
    circular: 'rounded-full',
    rectangular: 'rounded',
  };

  return (
    <div
      className={`
        animate-pulse
        bg-stationpedia-border
        ${variantClasses[variant]}
        ${className}
      `}
    />
  );
};

// Card skeleton for sidebar items
export const SidebarItemSkeleton: React.FC = () => {
  return (
    <div className="px-4 py-3 animate-pulse">
      <div className="flex items-center gap-2">
        <div className="w-5 h-5 rounded bg-stationpedia-border" />
        <div className="flex-1">
          <div className="h-4 bg-stationpedia-border rounded w-3/4 mb-2" />
          <div className="h-3 bg-stationpedia-border/50 rounded w-1/2" />
        </div>
      </div>
    </div>
  );
};

// Editor skeleton
export const EditorSkeleton: React.FC = () => {
  return (
    <div className="flex-1 flex flex-col bg-stationpedia-surface border-r border-stationpedia-border">
      {/* Toolbar skeleton */}
      <div className="bg-stationpedia-bg border-b border-stationpedia-border p-2 flex gap-2">
        {[1, 2, 3, 4, 5, 6].map((i) => (
          <div
            key={i}
            className="w-8 h-8 rounded bg-stationpedia-border animate-pulse"
          />
        ))}
      </div>
      
      {/* Content skeleton */}
      <div className="flex-1 p-4 space-y-4">
        <div className="h-6 bg-stationpedia-border rounded w-1/3 animate-pulse" />
        <div className="space-y-2">
          <div className="h-4 bg-stationpedia-border/70 rounded w-full animate-pulse" />
          <div className="h-4 bg-stationpedia-border/70 rounded w-5/6 animate-pulse" />
          <div className="h-4 bg-stationpedia-border/70 rounded w-4/5 animate-pulse" />
        </div>
        <div className="h-4 bg-stationpedia-border/50 rounded w-2/3 animate-pulse" />
      </div>
    </div>
  );
};
