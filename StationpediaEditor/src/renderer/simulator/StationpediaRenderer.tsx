/**
 * StationpediaRenderer - Main renderer component
 * Takes a DeviceDocument and renders it as styled HTML matching in-game Stationpedia
 * Parses TMP to AST and renders all device fields
 * 
 * For GUIDES: Sections render directly without "Operational Details" wrapper
 * For DEVICES: Sections still render inside "Operational Details" for backward compat
 */

import React, { useMemo } from 'react';
import type { DeviceDocument, GuideDocument } from '@models/contentModel';
import { DeviceHeader } from './components/DeviceHeader';
import { RichTextRenderer } from './components/RichTextRenderer';
import { CollapsibleSection } from './components/CollapsibleSection';
import { TOCPanel } from './components/TOCPanel';
import { OperationalDetailSection } from './components/OperationalDetailSection';
import { LogicSection } from './components/LogicSection';

// CSS is loaded separately - import it in the app entry point if needed

interface StationpediaRendererProps {
  device: DeviceDocument | GuideDocument | null;
  onLinkClick?: (target: string) => void;
}

export const StationpediaRenderer: React.FC<StationpediaRendererProps> = ({
  device,
  onLinkClick,
}) => {
  // Handle null device - show placeholder
  if (!device) {
    return (
      <div className="stationpedia-renderer p-6 flex items-center justify-center h-full text-gray-500">
        <div className="text-center">
          <div className="text-4xl mb-4">📖</div>
          <div>Select a device to preview</div>
        </div>
      </div>
    );
  }

  // Handle markdown guides - render as plain text/markdown preview
  const isMarkdownContent = (device as any)._isMarkdown === true;
  
  // Detect if this is a guide (has guideKey) vs device (has deviceKey)
  const isGuide = !!(device as any).guideKey;
  
  // Get the key - either deviceKey or guideKey
  const itemKey = (device as any).deviceKey || (device as any).guideKey || 'Unknown';
  
  if (isMarkdownContent) {
    return (
      <div className="stationpedia-renderer p-6" data-testid="stationpedia-renderer">
        <DeviceHeader
          displayName={device.displayName}
          deviceKey={itemKey}
          titleColor="#ff6a00"
        />
        <div className="bg-stationpedia-panel border border-stationpedia-border rounded p-4 mt-4">
          <pre className="whitespace-pre-wrap text-white font-sans text-sm leading-relaxed">
            {device.pageDescription || '(No content)'}
          </pre>
        </div>
      </div>
    );
  }

  // Combine page description parts
  const fullPageDescription = useMemo(() => {
    const parts: string[] = [];

    if (device.pageDescriptionPrepend && typeof device.pageDescriptionPrepend === 'string') {
      parts.push(device.pageDescriptionPrepend);
    }

    if (device.pageDescription) {
      parts.push(device.pageDescription);
    }

    if (device.pageDescriptionAppend && typeof device.pageDescriptionAppend === 'string') {
      parts.push(device.pageDescriptionAppend);
    }

    return parts.join('\n\n');
  }, [device?.pageDescription, device?.pageDescriptionPrepend, device?.pageDescriptionAppend]);

  return (
    <div className="stationpedia-renderer p-6" data-testid="stationpedia-renderer">
      <DeviceHeader
        displayName={device.displayName}
        deviceKey={itemKey}
        titleColor={device.operationalDetailsTitleColor}
      />

      {/* TOC Panel - shown for both guides and devices when enabled */}
      {device.generateToc && device.operationalDetails && device.operationalDetails.length > 0 && (
        <TOCPanel
          operationalDetails={device.operationalDetails}
          title={device.tocTitle || 'Contents'}
          onItemClick={onLinkClick}
        />
      )}

      {/* Page Description - always shown if present */}
      {fullPageDescription && (
        <CollapsibleSection
          title="Description"
          defaultOpen={true}
          depth={0}
          backgroundColor={device.operationalDetailsBackgroundColor}
        >
          <div className="page-description">
            <RichTextRenderer
              content={fullPageDescription}
              onLinkClick={onLinkClick}
              className="text-sm text-gray-200"
            />
          </div>
        </CollapsibleSection>
      )}

      {/* GUIDES: Render sections directly without "Operational Details" wrapper */}
      {isGuide && device.operationalDetails && device.operationalDetails.length > 0 && (
        <div className="space-y-3 mt-3">
          {device.operationalDetails.map((detail, i) => (
            <OperationalDetailSection
              key={i}
              detail={detail}
              depth={0}
              onLinkClick={onLinkClick}
            />
          ))}
        </div>
      )}

      {/* DEVICES: Render sections inside "Operational Details" wrapper for backward compat */}
      {!isGuide && device.operationalDetails && device.operationalDetails.length > 0 && (
        <CollapsibleSection
          title="Operational Details"
          defaultOpen={true}
          depth={0}
          titleColor={device.operationalDetailsTitleColor}
          backgroundColor={device.operationalDetailsBackgroundColor}
        >
          <div className="space-y-2">
            {device.operationalDetails.map((detail, i) => (
              <OperationalDetailSection
                key={i}
                detail={detail}
                depth={0}
                onLinkClick={onLinkClick}
              />
            ))}
          </div>
        </CollapsibleSection>
      )}

      {(device as any).logicDescriptions && typeof (device as any).logicDescriptions === 'object' && Object.keys((device as any).logicDescriptions).length > 0 && (
        <CollapsibleSection
          title="Logic Types"
          defaultOpen={true}
          depth={0}
        >
          <LogicSection logicDescriptions={(device as any).logicDescriptions} onLinkClick={onLinkClick} />
        </CollapsibleSection>
      )}

      {(device as any).modeDescriptions && Object.keys((device as any).modeDescriptions).length > 0 && (
        <CollapsibleSection
          title="Modes"
          defaultOpen={false}
          depth={0}
        >
          <div className="space-y-2">
            {Object.entries((device as any).modeDescriptions).map(([key, mode]: [string, any]) => (
              <div key={key} className="p-2 bg-black/20 rounded text-sm">
                <div className="font-semibold text-[#FF7A18]">{mode.modeValue || key}</div>
                {mode.description && (
                  <div className="text-gray-300 text-xs mt-1">
                    <RichTextRenderer
                      content={mode.description}
                      onLinkClick={onLinkClick}
                    />
                  </div>
                )}
              </div>
            ))}
          </div>
        </CollapsibleSection>
      )}

      {(device as any).slotDescriptions && Object.keys((device as any).slotDescriptions).length > 0 && (
        <CollapsibleSection
          title="Slots"
          defaultOpen={false}
          depth={0}
        >
          <div className="space-y-2">
            {Object.entries((device as any).slotDescriptions).map(([key, slot]: [string, any]) => (
              <div key={key} className="p-2 bg-black/20 rounded text-sm">
                <div className="flex justify-between">
                  <span className="font-semibold text-[#FF7A18]">Slot {slot.slotNumber}</span>
                  <span className="text-gray-400 text-xs font-mono">{slot.slotType}</span>
                </div>
                {slot.description && (
                  <div className="text-gray-300 text-xs mt-1">
                    <RichTextRenderer
                      content={slot.description}
                      onLinkClick={onLinkClick}
                    />
                  </div>
                )}
              </div>
            ))}
          </div>
        </CollapsibleSection>
      )}

      {/* Version Descriptions */}
      {(device as any).versionDescriptions && Object.keys((device as any).versionDescriptions).length > 0 && (
        <CollapsibleSection
          title="Versions"
          defaultOpen={false}
          depth={0}
        >
          <div className="space-y-2">
            {Object.entries((device as any).versionDescriptions).map(([key, version]: [string, any]) => (
              <div key={key} className="p-2 bg-black/20 rounded text-sm">
                <div className="font-semibold text-[#FF7A18]">{version.versionValue || key}</div>
                {version.description && (
                  <div className="text-gray-300 text-xs mt-1">
                    <RichTextRenderer
                      content={version.description}
                      onLinkClick={onLinkClick}
                    />
                  </div>
                )}
              </div>
            ))}
          </div>
        </CollapsibleSection>
      )}

      {(device as any).memoryDescriptions && Object.keys((device as any).memoryDescriptions).length > 0 && (
        <CollapsibleSection
          title="Memory"
          defaultOpen={false}
          depth={0}
        >
          <div className="space-y-2">
            {Object.entries((device as any).memoryDescriptions).map(([key, memory]: [string, any]) => (
              <div key={key} className="p-2 bg-black/20 rounded text-sm font-mono">
                <div className="flex justify-between text-xs">
                  <span className="text-[#FF7A18]">Address {memory.address}</span>
                  {memory.size && <span className="text-gray-400">Size: {memory.size}</span>}
                </div>
                {memory.description && (
                  <div className="text-gray-300 text-xs mt-1">
                    <RichTextRenderer
                      content={memory.description}
                      onLinkClick={onLinkClick}
                    />
                  </div>
                )}
              </div>
            ))}
          </div>
        </CollapsibleSection>
      )}
    </div>
  );
};

export default StationpediaRenderer;
