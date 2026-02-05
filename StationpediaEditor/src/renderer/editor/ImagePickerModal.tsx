/**
 * ImagePickerModal - Modal to browse and select PNG images from extracted assets
 * Uses virtualization to handle 9000+ images efficiently
 */
import React, { useState, useEffect, useMemo, useRef, useCallback } from 'react';

interface ImageInfo {
  filename: string;
  path: string;
  category?: string;
}

// Helper to convert Windows path to local-asset URL
const pathToAssetUrl = (filePath: string): string => {
  // Convert C:\path\to\file.png to local-asset://C/path/to/file.png
  const normalized = filePath.replace(/\\/g, '/');
  // Remove the colon after drive letter
  const withoutColon = normalized.replace(/^([a-zA-Z]):/, '$1');
  return `local-asset://${withoutColon}`;
};

// Constants for virtualization
const ITEM_WIDTH = 130; // Width of each grid item including gap
const ITEM_HEIGHT = 145; // Height of each grid item including gap
const COLUMNS = 4;
const OVERSCAN = 2; // Number of extra rows to render above/below viewport

interface ImagePickerModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSelectImage: (filename: string) => void;
  currentValue?: string;
}

export const ImagePickerModal: React.FC<ImagePickerModalProps> = ({
  isOpen,
  onClose,
  onSelectImage,
  currentValue,
}) => {
  const [images, setImages] = useState<ImageInfo[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [searchFilter, setSearchFilter] = useState('');
  const [selectedImage, setSelectedImage] = useState<string | null>(currentValue || null);
  const [previewImage, setPreviewImage] = useState<string | null>(null);
  const [categoryFilter, setCategoryFilter] = useState<string>('All');
  const [copyStatus, setCopyStatus] = useState<string | null>(null);

  // Load images when modal opens
  useEffect(() => {
    if (isOpen) {
      loadImages();
      setSelectedImage(currentValue || null);
    }
  }, [isOpen, currentValue]);

  const loadImages = async () => {
    setLoading(true);
    setError(null);
    try {
      // Use the Electron API to scan the Texture2D folder
      const result = await window.electronAPI?.scanImageFolder?.();
      if (result?.success && result.images) {
        setImages(result.images);
      } else {
        setError(result?.error || 'Could not load images. Make sure the AssetRipperFiles folder exists.');
      }
    } catch (err) {
      setError(`Failed to load images: ${err}`);
    } finally {
      setLoading(false);
    }
  };

  // Get unique categories
  const categories = useMemo(() => {
    const cats = new Set(images.map(img => img.category || 'Unknown'));
    return ['All', ...Array.from(cats).sort()];
  }, [images]);

  // Filter images based on search and category
  const filteredImages = useMemo(() => {
    let filtered = images;
    
    // Category filter
    if (categoryFilter !== 'All') {
      filtered = filtered.filter(img => img.category === categoryFilter);
    }
    
    // Search filter
    if (searchFilter.trim()) {
      const filter = searchFilter.toLowerCase();
      filtered = filtered.filter(img => img.filename.toLowerCase().includes(filter));
    }
    
    return filtered;
  }, [images, searchFilter, categoryFilter]);

  // Virtualization state
  const scrollContainerRef = useRef<HTMLDivElement>(null);
  const [scrollTop, setScrollTop] = useState(0);
  const [containerHeight, setContainerHeight] = useState(400);

  // Handle scroll for virtualization
  const handleScroll = useCallback((e: React.UIEvent<HTMLDivElement>) => {
    setScrollTop(e.currentTarget.scrollTop);
  }, []);

  // Calculate visible range for virtualization
  const { visibleItems, totalHeight, startIndex } = useMemo(() => {
    const totalRows = Math.ceil(filteredImages.length / COLUMNS);
    const totalHeight = totalRows * ITEM_HEIGHT;
    
    const startRow = Math.max(0, Math.floor(scrollTop / ITEM_HEIGHT) - OVERSCAN);
    const endRow = Math.min(totalRows, Math.ceil((scrollTop + containerHeight) / ITEM_HEIGHT) + OVERSCAN);
    
    const startIndex = startRow * COLUMNS;
    const endIndex = Math.min(filteredImages.length, endRow * COLUMNS);
    
    const visibleItems = filteredImages.slice(startIndex, endIndex);
    
    return { visibleItems, totalHeight, startIndex };
  }, [filteredImages, scrollTop, containerHeight]);

  // Update container height on resize
  useEffect(() => {
    const container = scrollContainerRef.current;
    if (!container) return;
    
    const updateHeight = () => {
      setContainerHeight(container.clientHeight);
    };
    
    updateHeight();
    const observer = new ResizeObserver(updateHeight);
    observer.observe(container);
    
    return () => observer.disconnect();
  }, []);

  // Reset scroll when filter changes
  useEffect(() => {
    if (scrollContainerRef.current) {
      scrollContainerRef.current.scrollTop = 0;
      setScrollTop(0);
    }
  }, [searchFilter, categoryFilter]);

  // Handle image selection - copy to mod folder and return filename
  const handleSelect = async () => {
    if (selectedImage) {
      const imageInfo = images.find(i => i.filename === selectedImage);
      if (imageInfo) {
        setCopyStatus('Copying...');
        try {
          const result = await window.electronAPI?.copyImageToMod?.(imageInfo.path, imageInfo.filename);
          if (result?.success) {
            if (result.copied) {
              setCopyStatus('✓ Copied to mod images folder');
            } else if (result.alreadyExists) {
              setCopyStatus('✓ Already exists in mod images folder');
            }
            // Brief delay to show status, then close
            setTimeout(() => {
              onSelectImage(selectedImage);
              onClose();
            }, 500);
          } else {
            setCopyStatus(`⚠️ Copy failed: ${result?.error || 'Unknown error'}`);
          }
        } catch (err) {
          setCopyStatus(`⚠️ Copy failed: ${err}`);
        }
      } else {
        onSelectImage(selectedImage);
        onClose();
      }
    }
  };

  // Handle double-click to select immediately
  const handleDoubleClick = async (filename: string) => {
    const imageInfo = images.find(i => i.filename === filename);
    if (imageInfo) {
      try {
        await window.electronAPI?.copyImageToMod?.(imageInfo.path, imageInfo.filename);
      } catch (err) {
        console.warn('Failed to copy image:', err);
      }
    }
    onSelectImage(filename);
    onClose();
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/70">
      <div className="bg-stationpedia-panel border border-stationpedia-border rounded-lg shadow-xl w-[900px] max-h-[80vh] flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b border-stationpedia-border">
          <h2 className="text-lg font-semibold text-stationpedia-accent">🖼️ Select Page Image</h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-white text-xl"
          >
            ✕
          </button>
        </div>

        {/* Search Bar */}
        <div className="p-4 border-b border-stationpedia-border">
          {/* Category Tabs */}
          <div className="flex gap-2 mb-3">
            {categories.map(cat => (
              <button
                key={cat}
                onClick={() => setCategoryFilter(cat)}
                className={`px-3 py-1 rounded text-sm transition-colors ${
                  categoryFilter === cat
                    ? 'bg-stationpedia-accent text-white'
                    : 'bg-stationpedia-surface text-gray-400 hover:bg-stationpedia-surface-hover hover:text-white'
                }`}
              >
                {cat}
                {cat !== 'All' && (
                  <span className="ml-1 opacity-60">
                    ({images.filter(i => i.category === cat).length})
                  </span>
                )}
              </button>
            ))}
          </div>
          <input
            type="text"
            value={searchFilter}
            onChange={(e) => setSearchFilter(e.target.value)}
            placeholder="Search images by name..."
            className="w-full px-3 py-2 bg-stationpedia-surface border border-stationpedia-border rounded text-white placeholder-gray-500 focus:outline-none focus:border-stationpedia-accent"
            autoFocus
          />
          <div className="mt-2 text-xs text-gray-400">
            {filteredImages.length} images found {(searchFilter || categoryFilter !== 'All') && `(filtered from ${images.length})`}
          </div>
        </div>

        {/* Content */}
        <div className="flex-1 overflow-hidden flex">
          {/* Image Grid with Virtualization */}
          <div 
            ref={scrollContainerRef}
            className="flex-1 overflow-auto p-4"
            onScroll={handleScroll}
          >
            {loading ? (
              <div className="flex items-center justify-center h-full text-gray-400">
                Loading images...
              </div>
            ) : error ? (
              <div className="flex flex-col items-center justify-center h-full text-red-400">
                <div className="mb-2">⚠️ {error}</div>
                <button
                  onClick={loadImages}
                  className="px-3 py-1 bg-stationpedia-accent hover:bg-stationpedia-accent-hover text-white rounded text-sm"
                >
                  Retry
                </button>
              </div>
            ) : filteredImages.length === 0 ? (
              <div className="flex items-center justify-center h-full text-gray-400">
                No images found matching "{searchFilter}"
              </div>
            ) : (
              <div style={{ height: totalHeight, position: 'relative' }}>
                <div 
                  className="grid grid-cols-4 gap-3"
                  style={{
                    position: 'absolute',
                    top: Math.floor(startIndex / COLUMNS) * ITEM_HEIGHT,
                    left: 0,
                    right: 0,
                  }}
                >
                  {visibleItems.map((img) => (
                    <div
                      key={img.filename}
                      onClick={() => setSelectedImage(img.filename)}
                      onDoubleClick={() => handleDoubleClick(img.filename)}
                      onMouseEnter={() => setPreviewImage(img.path)}
                      onMouseLeave={() => setPreviewImage(null)}
                      className={`relative p-2 rounded cursor-pointer border-2 transition-all ${
                        selectedImage === img.filename
                          ? 'border-stationpedia-accent bg-stationpedia-accent/20'
                          : 'border-transparent bg-stationpedia-surface hover:border-stationpedia-border'
                      }`}
                      title={img.filename}
                    >
                      {/* Thumbnail */}
                      <div className="aspect-square bg-gray-800 rounded flex items-center justify-center overflow-hidden relative">
                        <img
                          src={pathToAssetUrl(img.path)}
                          alt={img.filename}
                          className="max-w-full max-h-full object-contain"
                          loading="lazy"
                          onError={(e) => {
                            // Replace with placeholder on error
                            (e.target as HTMLImageElement).style.display = 'none';
                          }}
                        />
                        <span className="absolute inset-0 flex items-center justify-center text-3xl opacity-30 pointer-events-none">
                          🖼️
                        </span>
                      </div>
                      {/* Filename */}
                      <div className="mt-1 text-xs text-gray-300 truncate text-center">
                        {img.filename.replace('.png', '')}
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}
          </div>

          {/* Preview Panel */}
          <div className="w-64 border-l border-stationpedia-border p-4 flex flex-col">
            <div className="text-sm font-medium text-gray-300 mb-2">Preview</div>
            <div className="flex-1 bg-gray-800 rounded flex items-center justify-center overflow-hidden">
              {previewImage || selectedImage ? (
                <img
                  src={pathToAssetUrl(previewImage || images.find(i => i.filename === selectedImage)?.path || '')}
                  alt="Preview"
                  className="max-w-full max-h-full object-contain"
                  onError={(e) => {
                    (e.target as HTMLImageElement).style.display = 'none';
                  }}
                />
              ) : (
                <span className="text-gray-500 text-sm">Hover over an image to preview</span>
              )}
            </div>
            {selectedImage && (
              <div className="mt-2 text-xs text-gray-400 break-all">
                Selected: {selectedImage}
              </div>
            )}
          </div>
        </div>

        {/* Footer */}
        <div className="flex items-center justify-between p-4 border-t border-stationpedia-border">
          <div className="text-xs text-gray-400">
            {copyStatus ? (
              <span className={copyStatus.startsWith('✓') ? 'text-green-400' : copyStatus.startsWith('⚠️') ? 'text-yellow-400' : 'text-blue-400'}>
                {copyStatus}
              </span>
            ) : (
              'Double-click to select, or select and click "Use Selected". Image will be copied to mod images folder.'
            )}
          </div>
          <div className="flex gap-2">
            <button
              onClick={onClose}
              className="px-4 py-2 bg-gray-700 hover:bg-gray-600 text-white rounded"
            >
              Cancel
            </button>
            <button
              onClick={handleSelect}
              disabled={!selectedImage || copyStatus === 'Copying...'}
              className="px-4 py-2 bg-stationpedia-accent hover:bg-stationpedia-accent-hover text-white rounded disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Use Selected
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ImagePickerModal;
