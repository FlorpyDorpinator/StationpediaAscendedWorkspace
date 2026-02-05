/**
 * PanelSystem - Dockable panels using react-mosaic-component
 * Layout: ContentTree (left), DeviceSectionsEditor (center), Preview (right)
 */
import React, { useState, useEffect, useCallback } from 'react';
import {
  Mosaic,
  MosaicWindow,
  MosaicBranch,
  createBalancedTreeFromLeaves,
} from 'react-mosaic-component';
import 'react-mosaic-component/react-mosaic-component.css';
import type { WorkspaceModel, DeviceDocument, GuideDocument } from '@models/contentModel';

type SelectableItem = DeviceDocument | GuideDocument;

interface PanelSystemProps {
  workspace: WorkspaceModel | null;
  selectedDevice: SelectableItem | null;
  selectedDeviceKey: string | null;
  onSelectDevice: (deviceKey: string) => void;
  onUpdateDevice: (updates: Record<string, unknown>) => void;
  ContentTreeComponent: React.ComponentType<any>;
  DeviceSectionsEditorComponent: React.ComponentType<any>;
  PreviewComponent: React.ComponentType<any>;
}

type ViewId = 'contentTree' | 'deviceEditor' | 'preview';

const STORAGE_KEY = 'stationpedia-editor-layout-v3'; // bumped to reset layout after removing validation

export const PanelSystem: React.FC<PanelSystemProps> = ({
  workspace,
  selectedDevice,
  selectedDeviceKey,
  onSelectDevice,
  onUpdateDevice,
  ContentTreeComponent,
  DeviceSectionsEditorComponent,
  PreviewComponent,
}) => {
  const [layout, setLayout] = useState<any>(() => {
    // Try to load from localStorage
    const saved = localStorage.getItem(STORAGE_KEY);
    if (saved) {
      try {
        return JSON.parse(saved);
      } catch (e) {
        console.warn('Failed to parse saved layout:', e);
      }
    }
    // Default layout: ContentTree (20%), DeviceEditor (50%), Preview (30%)
    return {
      direction: 'row',
      first: 'contentTree',
      second: {
        direction: 'row',
        first: 'deviceEditor',
        second: 'preview',
        splitPercentage: 60,
      },
      splitPercentage: 20,
    };
  });

  // Save layout changes
  const handleLayoutChange = useCallback((newLayout: any) => {
    setLayout(newLayout);
    if (newLayout) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(newLayout));
    }
  }, []);

  const renderTile = (id: ViewId) => {
    switch (id) {
      case 'contentTree':
        return (
          <div className="w-full h-full flex flex-col bg-stationpedia-bg">
            <ContentTreeComponent
              workspace={workspace}
              selectedDeviceKey={selectedDeviceKey}
              onSelectDevice={onSelectDevice}
            />
          </div>
        );

      case 'deviceEditor':
        return (
          <div className="w-full h-full flex flex-col bg-stationpedia-bg overflow-hidden">
            <DeviceSectionsEditorComponent
              device={selectedDevice}
              onUpdateDevice={onUpdateDevice}
            />
          </div>
        );

      case 'preview':
        return (
          <div className="w-full h-full flex flex-col bg-stationpedia-bg overflow-auto">
            <PreviewComponent device={selectedDevice} />
          </div>
        );

      default:
        return null;
    }
  };

  const getTitleForId = (id: ViewId) => {
    switch (id) {
      case 'contentTree':
        return 'Content Tree';
      case 'deviceEditor':
        return 'Device Editor';
      case 'preview':
        return 'Preview';
      default:
        return id;
    }
  };

  // Force re-render when selected device changes
  const renderKey = selectedDeviceKey || 'no-device';

  return (
    <div className="w-full h-full" style={{ fontSize: '13px' }}>
      <Mosaic<ViewId>
        renderTile={(id) => (
          <MosaicWindow<ViewId>
            key={`${id}-${renderKey}`}
            path={[]}
            title={getTitleForId(id)}
            className="mosaic-window"
          >
            <div className="w-full h-full overflow-hidden">
              {renderTile(id)}
            </div>
          </MosaicWindow>
        )}
        value={layout}
        onChange={handleLayoutChange}
        zeroStateView={<div>No panels</div>}
      />
    </div>
  );
};
