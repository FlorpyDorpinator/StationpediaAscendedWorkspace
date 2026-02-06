/**
 * EditorApp - Main editor application component
 * Integrates all editor components with the dockable panel system
 */
import React, { useCallback, useEffect, useRef, useState } from 'react';
import { useEditorStore } from './editorStore';
import { useTooltipStore } from '@renderer/store/tooltipStore';
import { PanelSystem } from './PanelSystem';
import { Toolbar } from './Toolbar';
import { StatusBar } from './StatusBar';
import { RecentFilesMenu } from './RecentFilesMenu';
import { ContentTree } from './ContentTree';
import { DeviceSectionsEditor } from './DeviceSectionsEditor';
import { TooltipEditor } from './TooltipEditor';
import { ValidationPanel } from './ValidationPanel';
import { StationpediaRenderer } from '@renderer/simulator/StationpediaRenderer';
import { ConfirmDialog } from '@renderer/components/ConfirmDialog';
import { useKeyboardShortcuts, getKeyboardShortcutsHelp } from '@renderer/hooks/useKeyboardShortcuts';
import { fileService } from '@services/fileService';
import { persistenceService } from '@services/persistence';
import './styles/editor.css';
import type { WorkspaceModel, OperationalDetail } from '@models/contentModel';

export const EditorApp: React.FC = () => {
  const {
    workspace,
    selectedDeviceKey,
    selectedDevice,
    isDirty,
    selectDevice,
    updateDevice,
    setWorkspace,
    addOperationalDetail,
    removeOperationalDetail,
    reorderOperationalDetails,
    importGuideFromJson,
    clearDirty,
    reset,
  } = useEditorStore();

  const { loadTooltips, getJSON: getTooltipJSON } = useTooltipStore();

  const [isLoading, setIsLoading] = useState(false);
  const [workspacePath, setWorkspacePath] = useState<string | null>(null);
  const [showTooltips, setShowTooltips] = useState(false);
  const [showShortcuts, setShowShortcuts] = useState(false);
  const [recentFiles, setRecentFiles] = useState<string[]>([]);
  const [showRecentMenu, setShowRecentMenu] = useState(false);
  const [showConfirmDialog, setShowConfirmDialog] = useState(false);
  const [confirmMessage, setConfirmMessage] = useState('');
  const [validationErrors, setValidationErrors] = useState(0);
  const [validationWarnings, setValidationWarnings] = useState(0);

  // File input ref for JSON import
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Load workspace on startup
  useEffect(() => {
    async function loadLastWorkspace() {
      try {
        // Load recent files
        setRecentFiles(persistenceService.getRecentFiles());

        const settings = await fileService.getSettings();
        if (settings.lastWorkspace) {
          const exists = await fileService.exists(settings.lastWorkspace);
          if (exists) {
            setIsLoading(true);
            const result = await fileService.loadWorkspace(settings.lastWorkspace);
            console.log('[EditorApp] loadWorkspace result:', result);
            console.log('[EditorApp] guides count:', result.data?.guides?.length);
            console.log('[EditorApp] mechanics count:', result.data?.mechanics?.length);
            if (result.success && result.data) {
              const workspace: WorkspaceModel = {
                devices: result.data.descriptions as any,
                guides: result.data.guides as any,
                mechanics: result.data.mechanics as any,
                genericDescriptions: result.data.genericDescriptions,
              };
              console.log('[EditorApp] workspace.guides:', workspace.guides?.length);
              setWorkspace(workspace);
              setWorkspacePath(settings.lastWorkspace);
              persistenceService.setCurrentWorkspacePath(settings.lastWorkspace);

              // Load tooltips
              if (result.data.genericDescriptions) {
                loadTooltips(result.data.genericDescriptions);
              }

              // Auto-select first device
              if (workspace.devices.length > 0) {
                selectDevice(workspace.devices[0].deviceKey);
              }
            }
            setIsLoading(false);
          }
        }
      } catch (error) {
        console.error('Failed to load workspace:', error);
        setIsLoading(false);
      }
    }

    loadLastWorkspace();

    // Start auto-save (every 30 seconds)
    const stopAutoSave = persistenceService.startAutoSave(30000);
    return () => stopAutoSave();
  }, [selectDevice, setWorkspace]);

  // Handle opening workspace
  const handleOpenWorkspace = useCallback(async () => {
    console.log('handleOpenWorkspace called');
    console.log('window.electronAPI:', window.electronAPI);
    try {
      setIsLoading(true);
      const folderPath = await fileService.openFolder();
      console.log('folderPath:', folderPath);
      if (folderPath) {
        const result = await fileService.loadWorkspace(folderPath);
        console.log('loadWorkspace result:', result);
        if (result.success && result.data) {
          const workspace: WorkspaceModel = {
            devices: result.data.descriptions as any,
            guides: result.data.guides as any,
            mechanics: result.data.mechanics as any,
            genericDescriptions: result.data.genericDescriptions,
          };
          setWorkspace(workspace);
          setWorkspacePath(folderPath);
          persistenceService.setCurrentWorkspacePath(folderPath);
          persistenceService.addRecentFile(folderPath);
          setRecentFiles(persistenceService.getRecentFiles());

          // Load tooltips
          if (result.data.genericDescriptions) {
            loadTooltips(result.data.genericDescriptions);
          }

          // Auto-select first device
          if (workspace.devices.length > 0) {
            selectDevice(workspace.devices[0].deviceKey);
          }

          // Save last opened workspace
          await fileService.setSettings({
            lastWorkspace: folderPath,
          });
        }
      }
    } catch (error) {
      console.error('Failed to open workspace:', error);
    } finally {
      setIsLoading(false);
    }
  }, [selectDevice, setWorkspace]);

  // Handle saving
  const handleSave = useCallback(async () => {
    if (!workspacePath) return;

    // Read current state directly from store to avoid stale closures.
    // React's batching may not have re-rendered yet (e.g. blur -> click race),
    // but Zustand's set() is synchronous so getState() always has the latest data.
    const currentWorkspace = useEditorStore.getState().workspace;
    if (!currentWorkspace) return;

    try {
      setIsLoading(true);
      
      // Prepare workspace with updated tooltips
      const tooltipJSON = getTooltipJSON();
      const updatedWorkspace = {
        ...currentWorkspace,
        genericDescriptions: Object.keys(tooltipJSON).length > 0 ? tooltipJSON : undefined,
      };
      
      const result = await fileService.saveDescriptions(workspacePath, {
        ...updatedWorkspace,
        devices: updatedWorkspace.devices,
      } as any);
      
      if (result.success) {
        clearDirty();
      } else {
        console.error('Failed to save:', result.error);
      }
    } catch (error) {
      console.error('Save error:', error);
    } finally {
      setIsLoading(false);
    }
  }, [workspacePath, getTooltipJSON, clearDirty]);

  // Handle closing workspace
  const handleCloseWorkspace = useCallback(() => {
    if (isDirty) {
      setConfirmMessage('You have unsaved changes. Close anyway?');
      setShowConfirmDialog(true);
    } else {
      reset();
      setWorkspacePath(null);
      persistenceService.setCurrentWorkspacePath(null as any);
    }
  }, [isDirty, reset]);

  // Handle device update
  const handleUpdateDevice = useCallback(
    (updates: Record<string, unknown>) => {
      if (!selectedDeviceKey) return;
      updateDevice(selectedDeviceKey, updates);
      persistenceService.setDirtyState(true);
    },
    [selectedDeviceKey, updateDevice]
  );

  // Open recent file
  const handleOpenRecentFile = useCallback(
    async (path: string) => {
      try {
        setIsLoading(true);
        const exists = await fileService.exists(path);
        if (!exists) {
          console.error('Recent file no longer exists:', path);
          persistenceService.clearRecentFiles();
          setRecentFiles([]);
          return;
        }

        const result = await fileService.loadWorkspace(path);
        if (result.success && result.data) {
          const workspace: WorkspaceModel = {
            devices: result.data.descriptions as any,
            guides: result.data.guides as any,
            mechanics: result.data.mechanics as any,
            genericDescriptions: result.data.genericDescriptions,
          };
          setWorkspace(workspace);
          setWorkspacePath(path);
          persistenceService.setCurrentWorkspacePath(path);

          // Load tooltips
          if (result.data.genericDescriptions) {
            loadTooltips(result.data.genericDescriptions);
          }

          // Auto-select first device
          if (workspace.devices.length > 0) {
            selectDevice(workspace.devices[0].deviceKey);
          }
        }
      } catch (error) {
        console.error('Failed to open recent file:', error);
      } finally {
        setIsLoading(false);
      }
    },
    [selectDevice, setWorkspace, loadTooltips]
  );

  // Keyboard shortcuts
  useKeyboardShortcuts({
    save: handleSave,
    open: handleOpenWorkspace,
    close: handleCloseWorkspace,
  });

  // Open simulator window
  const handleOpenSimulator = useCallback(async () => {
    if (!window.electronAPI) {
      console.error('Electron API not available');
      return;
    }
    try {
      await window.electronAPI.openSimulator();
      // Send current devices to simulator
      if (workspace?.devices) {
        window.electronAPI.sendDevicesToSimulator?.(workspace.devices);
      }
    } catch (error) {
      console.error('Failed to open simulator:', error);
    }
  }, [workspace]);

  // Reset layout
  const handleResetLayout = useCallback(() => {
    localStorage.removeItem('stationpedia-editor-layout');
    localStorage.removeItem('stationpedia-editor-layout-v2');
    window.location.reload();
  }, []);

  // Import guide from JSON file
  const handleImportGuide = useCallback(() => {
    fileInputRef.current?.click();
  }, []);

  const handleFileSelect = useCallback(
    async (event: React.ChangeEvent<HTMLInputElement>) => {
      const file = event.target.files?.[0];
      if (!file) return;

      try {
        const text = await file.text();
        const jsonData = JSON.parse(text);
        const newGuideKey = importGuideFromJson(jsonData);
        selectDevice(newGuideKey);
      } catch (error) {
        console.error('Failed to import JSON:', error);
        alert('Failed to import JSON file. Please ensure it is valid JSON.');
      }

      // Reset input so same file can be selected again
      event.target.value = '';
    },
    [importGuideFromJson, selectDevice]
  );

  return (
    <div className="flex flex-col h-screen bg-stationpedia-bg text-gray-100">
      {/* Toolbar */}
      <Toolbar
        isDirty={isDirty}
        workspacePath={workspacePath}
        onOpenWorkspace={handleOpenWorkspace}
        onImportGuide={handleImportGuide}
        onSave={handleSave}
        onClose={handleCloseWorkspace}
        isLoading={isLoading}
        onToggleTooltips={() => setShowTooltips(!showTooltips)}
        showTooltips={showTooltips}
        onToggleShortcuts={() => setShowShortcuts(!showShortcuts)}
        showShortcuts={showShortcuts}
        onOpenSimulator={handleOpenSimulator}
        onResetLayout={handleResetLayout}
      />

      {/* Main Content Area */}
      <div className="flex-1 flex gap-4 overflow-hidden">
        <div className="flex-1 flex flex-col">
          {!workspace ? (
            <div className="flex items-center justify-center h-full bg-stationpedia-bg">
              <div className="text-center">
                <h2 className="text-2xl font-bold text-stationpedia-accent mb-4">Welcome to Stationpedia Editor</h2>
                <div className="flex gap-3 justify-center">
                  <button
                    onClick={handleOpenWorkspace}
                    className="px-6 py-3 bg-stationpedia-accent hover:bg-stationpedia-accent-hover text-white rounded font-semibold transition-colors"
                  >
                    Open Workspace
                  </button>
                  {recentFiles.length > 0 && (
                    <RecentFilesMenu
                      recentFiles={recentFiles}
                      onSelectFile={handleOpenRecentFile}
                      onClearRecent={() => {
                        persistenceService.clearRecentFiles();
                        setRecentFiles([]);
                      }}
                      isOpen={showRecentMenu}
                      onToggle={() => setShowRecentMenu(!showRecentMenu)}
                    />
                  )}
                </div>
              </div>
            </div>
          ) : (
            <>
              <PanelSystem
                workspace={workspace}
                selectedDevice={selectedDevice}
                selectedDeviceKey={selectedDeviceKey}
                onSelectDevice={selectDevice}
                onUpdateDevice={handleUpdateDevice}
                ContentTreeComponent={ContentTree}
                DeviceSectionsEditorComponent={DeviceSectionsEditor}
                PreviewComponent={StationpediaRenderer}
              />
            </>
          )}

          {/* Status Bar */}
          {workspace && (
            <StatusBar
              isDirty={isDirty}
              workspacePath={workspacePath}
              validationErrors={validationErrors}
              validationWarnings={validationWarnings}
            />
          )}
        </div>

        {/* Tooltip Panel */}
        {workspace && showTooltips && (
          <div className="w-80 border-l border-stationpedia-border bg-stationpedia-bg overflow-hidden flex flex-col">
            <TooltipEditor />
          </div>
        )}
      </div>

      {/* Keyboard Shortcuts Modal */}
      {showShortcuts && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50" onClick={() => setShowShortcuts(false)}>
          <div 
            className="bg-stationpedia-surface border border-stationpedia-border rounded-lg p-6 max-w-2xl w-full mx-4 shadow-xl"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="flex justify-between items-center mb-4">
              <h2 className="text-xl font-bold text-stationpedia-accent">⌨️ Keyboard Shortcuts</h2>
              <button 
                onClick={() => setShowShortcuts(false)}
                className="text-gray-400 hover:text-white text-xl"
              >
                ✕
              </button>
            </div>
            <div className="grid grid-cols-2 gap-4">
              {getKeyboardShortcutsHelp().map((shortcut, idx) => (
                <div key={idx} className="flex justify-between items-center gap-4 py-2">
                  <code className="bg-stationpedia-bg px-3 py-1.5 rounded text-sm font-mono text-stationpedia-accent">
                    {shortcut.keys}
                  </code>
                  <span className="text-gray-300 text-sm">{shortcut.action}</span>
                </div>
              ))}
            </div>
            <div className="mt-6 text-center">
              <button
                onClick={() => setShowShortcuts(false)}
                className="px-4 py-2 bg-stationpedia-accent hover:bg-stationpedia-accent-hover text-white rounded font-medium transition-colors"
              >
                Close
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Hidden file input for JSON import */}
      <input
        ref={fileInputRef}
        type="file"
        accept=".json"
        onChange={handleFileSelect}
        style={{ display: 'none' }}
      />

      {/* Confirmation Dialog */}
      <ConfirmDialog
        isOpen={showConfirmDialog}
        title="Unsaved Changes"
        message={confirmMessage}
        confirmText="Close"
        cancelText="Cancel"
        isDangerous={true}
        onConfirm={() => {
          setShowConfirmDialog(false);
          reset();
          setWorkspacePath(null);
          persistenceService.setCurrentWorkspacePath(null as any);
        }}
        onCancel={() => setShowConfirmDialog(false)}
      />
    </div>
  );
};

export default EditorApp;
