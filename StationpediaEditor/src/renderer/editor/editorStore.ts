/**
 * Editor store using Zustand
 * Manages editor state: workspace, selected device, dirty flag, operational details
 */
import { create } from 'zustand';
import type { WorkspaceModel, DeviceDocument, GuideDocument, OperationalDetail } from '@models/contentModel';
import { normalizeDocumentTables } from '@models/contentModel';

// Helper type for items that can be selected (devices or guides)
type SelectableItem = DeviceDocument | GuideDocument;

// Navigation history entry
interface NavigationEntry {
  deviceKey: string;
  itemType: 'device' | 'guide' | 'mechanic';
}

// Category types for drag and drop
export type CategoryType = 'device' | 'guide' | 'mechanic';

// Header item for organizing categories
export interface HeaderItem {
  isHeader: true;
  headerKey: string;
  headerTitle: string;
  sortOrder?: number;
}

export interface EditorState {
  // Workspace
  workspace: WorkspaceModel | null;
  selectedDeviceKey: string | null;
  selectedItemType: 'device' | 'guide' | 'mechanic' | null;
  isDirty: boolean;

  // Navigation history
  navigationHistory: NavigationEntry[];
  navigationIndex: number;

  // Scroll-to-section request (increments counter to trigger even for same index)
  scrollToSectionIndex: number | null;
  scrollToSectionCounter: number;

  // Computed
  selectedDevice: SelectableItem | null;

  // Actions
  requestScrollToSection: (index: number) => void;
  clearScrollToSection: () => void;
  setWorkspace: (workspace: WorkspaceModel) => void;
  selectDevice: (deviceKey: string) => void;
  updateDevice: (deviceKey: string, updates: Partial<SelectableItem>) => void;
  addOperationalDetail: (detail: OperationalDetail, deviceKey?: string) => void;
  removeOperationalDetail: (index: number, deviceKey?: string) => void;
  reorderOperationalDetails: (fromIndex: number, toIndex: number, deviceKey?: string) => void;
  addGuide: () => string; // Returns the new guide key
  importGuideFromJson: (jsonData: any) => string; // Import a guide from JSON data
  deleteGuide: (guideKey: string) => void;
  addMechanic: () => string; // Returns the new mechanic key
  deleteMechanic: (mechanicKey: string) => void;
  clearDirty: () => void;
  reset: () => void;
  
  // Navigation actions
  canGoBack: () => boolean;
  canGoForward: () => boolean;
  goBack: () => void;
  goForward: () => void;

  // Category management actions
  moveToCategory: (itemKey: string, fromCategory: CategoryType, toCategory: CategoryType, insertIndex?: number) => void;
  reorderItem: (category: CategoryType, fromIndex: number, toIndex: number) => void;
  addHeader: (category: CategoryType, title: string, insertIndex?: number) => string; // Returns header key
  deleteHeader: (category: CategoryType, headerKey: string) => void;
  updateHeader: (category: CategoryType, headerKey: string, newTitle: string) => void;
}

export const useEditorStore = create<EditorState>((set, get) => ({
  workspace: null,
  selectedDeviceKey: null,
  selectedItemType: null,
  isDirty: false,
  selectedDevice: null,
  navigationHistory: [],
  navigationIndex: -1,
  scrollToSectionIndex: null,
  scrollToSectionCounter: 0,

  requestScrollToSection: (index: number) => {
    set((state) => ({
      scrollToSectionIndex: index,
      scrollToSectionCounter: state.scrollToSectionCounter + 1,
    }));
  },

  clearScrollToSection: () => {
    set({ scrollToSectionIndex: null });
  },

  setWorkspace: (workspace) => {
    set({
      workspace,
      isDirty: false,
      selectedDeviceKey: null,
      selectedItemType: null,
      selectedDevice: null,
      navigationHistory: [],
      navigationIndex: -1,
    });
  },

  selectDevice: (deviceKey) => {
    const { workspace, selectedDeviceKey, navigationHistory, navigationIndex } = get();
    
    // Don't add to history if selecting the same device
    if (deviceKey === selectedDeviceKey) return;
    
    // Search across devices, guides, and mechanics
    let device: SelectableItem | null = workspace?.devices.find((d) => d.deviceKey === deviceKey) || null;
    let itemType: 'device' | 'guide' | 'mechanic' | null = device ? 'device' : null;
    
    if (!device && workspace?.guides) {
      // For guides, search by guideKey
      const guide = workspace.guides.find((g) => g.guideKey === deviceKey);
      if (guide) {
        device = guide as any; // Cast to SelectableItem
        itemType = 'guide';
      }
    }
    if (!device && workspace?.mechanics) {
      device = workspace.mechanics.find((d) => (d as any).guideKey === deviceKey || (d as any).deviceKey === deviceKey) as any || null;
      if (device) itemType = 'mechanic';
    }
    
    // Update navigation history - truncate forward history when navigating to new item
    let newHistory = navigationHistory.slice(0, navigationIndex + 1);
    if (itemType) {
      newHistory.push({ deviceKey, itemType });
    }
    // Limit history to 50 items
    if (newHistory.length > 50) {
      newHistory = newHistory.slice(-50);
    }
    
    set({
      selectedDeviceKey: deviceKey,
      selectedItemType: itemType,
      selectedDevice: device,
      navigationHistory: newHistory,
      navigationIndex: newHistory.length - 1,
    });
  },

  updateDevice: (deviceKey, updates) => {
    set((state) => {
      if (!state.workspace) return state;

      // Determine item type based on where the deviceKey is found, not selectedItemType
      // This ensures updates work even when selection changes
      let itemType: 'device' | 'guide' | 'mechanic' | null = null;
      
      if (state.workspace.devices.some((d) => d.deviceKey === deviceKey)) {
        itemType = 'device';
      } else if (state.workspace.guides?.some((g) => g.guideKey === deviceKey)) {
        itemType = 'guide';
      } else if (state.workspace.mechanics?.some((m: any) => m.guideKey === deviceKey || m.deviceKey === deviceKey)) {
        itemType = 'mechanic';
      }
      
      if (!itemType) return state;

      // Update the correct array based on item type
      let newWorkspace = { ...state.workspace };
      
      if (itemType === 'guide' && newWorkspace.guides) {
        newWorkspace.guides = newWorkspace.guides.map((g) =>
          g.guideKey === deviceKey ? { ...g, ...updates } : g
        );
      } else if (itemType === 'mechanic' && newWorkspace.mechanics) {
        newWorkspace.mechanics = newWorkspace.mechanics.map((m: any) =>
          (m.guideKey === deviceKey || m.deviceKey === deviceKey) ? { ...m, ...updates } : m
        );
      } else if (itemType === 'device') {
        newWorkspace.devices = newWorkspace.devices.map((d) =>
          d.deviceKey === deviceKey ? { ...d, ...updates } : d
        );
      }

      const newSelectedDevice =
        state.selectedDeviceKey === deviceKey
          ? { ...state.selectedDevice!, ...updates }
          : state.selectedDevice;

      return {
        workspace: newWorkspace,
        selectedDevice: newSelectedDevice,
        isDirty: true,
      };
    });
  },

  addOperationalDetail: (detail, deviceKey) => {
    set((state) => {
      if (!state.workspace) return state;

      const targetKey = deviceKey || state.selectedDeviceKey;
      if (!targetKey) return state;

      const newWorkspace = {
        ...state.workspace,
        devices: state.workspace.devices.map((d) => {
          if (d.deviceKey !== targetKey) return d;
          return {
            ...d,
            operationalDetails: [...(d.operationalDetails || []), detail],
          };
        }),
      };

      const newSelectedDevice =
        state.selectedDeviceKey === targetKey
          ? {
              ...state.selectedDevice!,
              operationalDetails: [...(state.selectedDevice?.operationalDetails || []), detail],
            }
          : state.selectedDevice;

      return {
        workspace: newWorkspace,
        selectedDevice: newSelectedDevice,
        isDirty: true,
      };
    });
  },

  removeOperationalDetail: (index, deviceKey) => {
    set((state) => {
      if (!state.workspace) return state;

      const targetKey = deviceKey || state.selectedDeviceKey;
      if (!targetKey) return state;

      const newWorkspace = {
        ...state.workspace,
        devices: state.workspace.devices.map((d) => {
          if (d.deviceKey !== targetKey) return d;
          const details = d.operationalDetails || [];
          return {
            ...d,
            operationalDetails: details.filter((_, i) => i !== index),
          };
        }),
      };

      const newSelectedDevice =
        state.selectedDeviceKey === targetKey
          ? {
              ...state.selectedDevice!,
              operationalDetails: (state.selectedDevice?.operationalDetails || []).filter(
                (_, i) => i !== index
              ),
            }
          : state.selectedDevice;

      return {
        workspace: newWorkspace,
        selectedDevice: newSelectedDevice,
        isDirty: true,
      };
    });
  },

  reorderOperationalDetails: (fromIndex, toIndex, deviceKey) => {
    set((state) => {
      if (!state.workspace) return state;

      const targetKey = deviceKey || state.selectedDeviceKey;
      if (!targetKey) return state;

      const newWorkspace = {
        ...state.workspace,
        devices: state.workspace.devices.map((d) => {
          if (d.deviceKey !== targetKey) return d;

          const details = [...(d.operationalDetails || [])];
          const [removed] = details.splice(fromIndex, 1);
          details.splice(toIndex, 0, removed);

          return {
            ...d,
            operationalDetails: details,
          };
        }),
      };

      const newSelectedDevice =
        state.selectedDeviceKey === targetKey
          ? {
              ...state.selectedDevice!,
              operationalDetails: (() => {
                const details = [...(state.selectedDevice?.operationalDetails || [])];
                const [removed] = details.splice(fromIndex, 1);
                details.splice(toIndex, 0, removed);
                return details;
              })(),
            }
          : state.selectedDevice;

      return {
        workspace: newWorkspace,
        selectedDevice: newSelectedDevice,
        isDirty: true,
      };
    });
  },

  clearDirty: () => {
    set({ isDirty: false });
  },

  addGuide: () => {
    let newGuideKey = '';
    set((state) => {
      if (!state.workspace) return state;

      // Generate unique guide key
      const baseKey = 'NewGuide';
      let counter = 1;
      let candidateKey = baseKey;
      
      while (state.workspace.guides?.some(g => g.guideKey === candidateKey)) {
        candidateKey = `${baseKey}${counter}`;
        counter++;
      }
      
      newGuideKey = candidateKey;
      
      // Create new guide
      const newGuide: GuideDocument = {
        guideKey: newGuideKey,
        displayName: 'New Guide',
        pageDescription: 'Guide description goes here...',
        operationalDetails: [],
        deviceKey: newGuideKey, // For compatibility
      };
      
      const newWorkspace = {
        ...state.workspace,
        guides: [...(state.workspace.guides || []), newGuide],
      };
      
      return {
        workspace: newWorkspace,
        isDirty: true,
      };
    });
    return newGuideKey;
  },

  importGuideFromJson: (jsonData) => {
    let importedGuideKey = '';
    set((state) => {
      if (!state.workspace) return state;

      // Use guideKey from JSON or generate new one
      let guideKey = jsonData.guideKey || 'ImportedGuide';
      let counter = 1;
      let candidateKey = guideKey;
      
      // Ensure unique key
      while (state.workspace.guides?.some(g => g.guideKey === candidateKey)) {
        candidateKey = `${guideKey}${counter}`;
        counter++;
      }
      
      importedGuideKey = candidateKey;
      
      // Map OperationalDetails to operationalDetails (handle case differences)
      const operationalDetails = jsonData.OperationalDetails || jsonData.operationalDetails || [];
      
      // Create new guide from imported data
      let newGuide: GuideDocument = {
        guideKey: importedGuideKey,
        displayName: jsonData.displayName || 'Imported Guide',
        pageDescription: jsonData.pageDescription || '',
        operationalDetails: operationalDetails,
        // Optional fields from JSON
        ...(jsonData.generateToc !== undefined && { generateToc: jsonData.generateToc }),
        ...(jsonData.tocTitle && { tocTitle: jsonData.tocTitle }),
        ...(jsonData.buttonColor && { buttonColor: jsonData.buttonColor }),
        ...(jsonData.sortOrder !== undefined && { sortOrder: jsonData.sortOrder }),
      };
      
      // Normalize table formats to prevent {headers, rows} format from being saved
      newGuide = normalizeDocumentTables(newGuide);
      
      const newWorkspace = {
        ...state.workspace,
        guides: [...(state.workspace.guides || []), newGuide],
      };
      
      return {
        workspace: newWorkspace,
        isDirty: true,
      };
    });
    return importedGuideKey;
  },

  deleteGuide: (guideKey) => {
    set((state) => {
      if (!state.workspace || !state.workspace.guides) return state;

      const newWorkspace = {
        ...state.workspace,
        guides: state.workspace.guides.filter(g => g.guideKey !== guideKey),
      };

      // If the deleted guide was selected, clear selection
      const updates: Partial<EditorState> = {
        workspace: newWorkspace,
        isDirty: true,
      };

      if (state.selectedDeviceKey === guideKey) {
        updates.selectedDeviceKey = null;
        updates.selectedItemType = null;
        updates.selectedDevice = null;
      }

      return updates;
    });
  },

  addMechanic: () => {
    let newMechanicKey = '';
    set((state) => {
      if (!state.workspace) return state;

      // Generate unique mechanic key
      const baseKey = 'NewMechanic';
      let counter = 1;
      let candidateKey = baseKey;
      
      while (state.workspace.mechanics?.some((m: any) => 
        m.guideKey === candidateKey || m.deviceKey === candidateKey
      )) {
        candidateKey = `${baseKey}${counter}`;
        counter++;
      }
      
      newMechanicKey = candidateKey;
      
      // Create new mechanic
      const newMechanic: any = {
        guideKey: newMechanicKey,
        displayName: 'New Game Mechanic',
        pageDescription: 'Game mechanic description goes here...',
        operationalDetails: [],
        deviceKey: newMechanicKey, // For compatibility
      };
      
      const newWorkspace = {
        ...state.workspace,
        mechanics: [...(state.workspace.mechanics || []), newMechanic],
      };
      
      return {
        workspace: newWorkspace,
        isDirty: true,
      };
    });
    return newMechanicKey;
  },

  deleteMechanic: (mechanicKey) => {
    set((state) => {
      if (!state.workspace || !state.workspace.mechanics) return state;

      const newWorkspace = {
        ...state.workspace,
        mechanics: state.workspace.mechanics.filter((m: any) => 
          m.guideKey !== mechanicKey && m.deviceKey !== mechanicKey
        ),
      };

      // If the deleted mechanic was selected, clear selection
      const updates: Partial<EditorState> = {
        workspace: newWorkspace,
        isDirty: true,
      };

      if (state.selectedDeviceKey === mechanicKey) {
        updates.selectedDeviceKey = null;
        updates.selectedItemType = null;
        updates.selectedDevice = null;
      }

      return updates;
    });
  },

  reset: () => {
    set({
      workspace: null,
      selectedDeviceKey: null,
      selectedItemType: null,
      isDirty: false,
      selectedDevice: null,
      navigationHistory: [],
      navigationIndex: -1,
    });
  },

  // Navigation methods
  canGoBack: () => {
    const { navigationIndex } = get();
    return navigationIndex > 0;
  },

  canGoForward: () => {
    const { navigationIndex, navigationHistory } = get();
    return navigationIndex < navigationHistory.length - 1;
  },

  goBack: () => {
    const { navigationHistory, navigationIndex, workspace } = get();
    if (navigationIndex <= 0) return;

    const newIndex = navigationIndex - 1;
    const entry = navigationHistory[newIndex];
    
    // Find the device/guide/mechanic
    let device: SelectableItem | null = null;
    if (entry.itemType === 'device') {
      device = workspace?.devices.find((d) => d.deviceKey === entry.deviceKey) || null;
    } else if (entry.itemType === 'guide') {
      device = workspace?.guides?.find((g) => g.guideKey === entry.deviceKey) as any || null;
    } else if (entry.itemType === 'mechanic') {
      device = workspace?.mechanics?.find((m: any) => m.guideKey === entry.deviceKey) as any || null;
    }

    set({
      selectedDeviceKey: entry.deviceKey,
      selectedItemType: entry.itemType,
      selectedDevice: device,
      navigationIndex: newIndex,
    });
  },

  goForward: () => {
    const { navigationHistory, navigationIndex, workspace } = get();
    if (navigationIndex >= navigationHistory.length - 1) return;

    const newIndex = navigationIndex + 1;
    const entry = navigationHistory[newIndex];
    
    // Find the device/guide/mechanic
    let device: SelectableItem | null = null;
    if (entry.itemType === 'device') {
      device = workspace?.devices.find((d) => d.deviceKey === entry.deviceKey) || null;
    } else if (entry.itemType === 'guide') {
      device = workspace?.guides?.find((g) => g.guideKey === entry.deviceKey) as any || null;
    } else if (entry.itemType === 'mechanic') {
      device = workspace?.mechanics?.find((m: any) => m.guideKey === entry.deviceKey) as any || null;
    }

    set({
      selectedDeviceKey: entry.deviceKey,
      selectedItemType: entry.itemType,
      selectedDevice: device,
      navigationIndex: newIndex,
    });
  },

  // Move item between categories (guides, mechanics, devices)
  moveToCategory: (itemKey, fromCategory, toCategory, insertIndex) => {
    set((state) => {
      if (!state.workspace) return state;
      if (fromCategory === toCategory) return state;

      // Get source and target arrays
      const getArray = (category: CategoryType): any[] => {
        switch (category) {
          case 'device': return [...(state.workspace!.devices || [])];
          case 'guide': return [...(state.workspace!.guides || [])];
          case 'mechanic': return [...(state.workspace!.mechanics || [])];
        }
      };

      const sourceArray = getArray(fromCategory);
      const targetArray = getArray(toCategory);

      // Find item in source array
      const findItem = (arr: any[], key: string) => 
        arr.findIndex((item) => item.deviceKey === key || item.guideKey === key);
      
      const sourceIndex = findItem(sourceArray, itemKey);
      if (sourceIndex === -1) return state;

      // Remove from source
      const [item] = sourceArray.splice(sourceIndex, 1);

      // Transform key field based on target category
      const transformedItem = { ...item };
      
      // Remove old key fields
      delete transformedItem.deviceKey;
      delete transformedItem.guideKey;
      
      // Add appropriate key for target category
      const keyValue = item.deviceKey || item.guideKey || itemKey;
      if (toCategory === 'device') {
        transformedItem.deviceKey = keyValue;
      } else {
        transformedItem.guideKey = keyValue;
      }

      // Insert into target at specified index or end
      const idx = insertIndex !== undefined ? insertIndex : targetArray.length;
      targetArray.splice(idx, 0, transformedItem);

      // Build new workspace
      const newWorkspace = { ...state.workspace };
      
      switch (fromCategory) {
        case 'device': newWorkspace.devices = sourceArray; break;
        case 'guide': newWorkspace.guides = sourceArray; break;
        case 'mechanic': newWorkspace.mechanics = sourceArray; break;
      }
      
      switch (toCategory) {
        case 'device': newWorkspace.devices = targetArray; break;
        case 'guide': newWorkspace.guides = targetArray; break;
        case 'mechanic': newWorkspace.mechanics = targetArray; break;
      }

      // Update selection to new category
      const updates: Partial<EditorState> = {
        workspace: newWorkspace,
        isDirty: true,
      };

      if (state.selectedDeviceKey === itemKey) {
        updates.selectedItemType = toCategory;
      }

      return updates;
    });
  },

  // Reorder item within a category
  reorderItem: (category, fromIndex, toIndex) => {
    set((state) => {
      if (!state.workspace) return state;
      if (fromIndex === toIndex) return state;

      const newWorkspace = { ...state.workspace };

      const reorder = (arr: any[]) => {
        const newArr = [...arr];
        const [removed] = newArr.splice(fromIndex, 1);
        newArr.splice(toIndex, 0, removed);
        return newArr;
      };

      switch (category) {
        case 'device':
          newWorkspace.devices = reorder(newWorkspace.devices || []);
          break;
        case 'guide':
          newWorkspace.guides = reorder(newWorkspace.guides || []);
          break;
        case 'mechanic':
          newWorkspace.mechanics = reorder(newWorkspace.mechanics || []);
          break;
      }

      return {
        workspace: newWorkspace,
        isDirty: true,
      };
    });
  },

  // Add inline header to a category
  addHeader: (category, title, insertIndex) => {
    let headerKey = '';
    set((state) => {
      if (!state.workspace) return state;

      // Generate unique header key
      const baseKey = 'Header';
      let counter = 1;
      const allItems = [
        ...(state.workspace.devices || []),
        ...(state.workspace.guides || []),
        ...(state.workspace.mechanics || []),
      ];
      
      let candidateKey = `${baseKey}_${Date.now()}`;
      while (allItems.some((item: any) => 
        item.headerKey === candidateKey || item.guideKey === candidateKey || item.deviceKey === candidateKey
      )) {
        candidateKey = `${baseKey}_${Date.now()}_${counter}`;
        counter++;
      }
      
      headerKey = candidateKey;

      // Create header item
      const headerItem: any = {
        isHeader: true,
        headerKey: headerKey,
        headerTitle: title,
      };

      // For guides/mechanics, add guideKey for compatibility
      if (category === 'guide' || category === 'mechanic') {
        headerItem.guideKey = headerKey;
      } else {
        headerItem.deviceKey = headerKey;
      }

      const newWorkspace = { ...state.workspace };

      const insertIntoArray = (arr: any[]) => {
        const newArr = [...arr];
        const idx = insertIndex !== undefined ? insertIndex : newArr.length;
        newArr.splice(idx, 0, headerItem);
        return newArr;
      };

      switch (category) {
        case 'device':
          newWorkspace.devices = insertIntoArray(newWorkspace.devices || []);
          break;
        case 'guide':
          newWorkspace.guides = insertIntoArray(newWorkspace.guides || []);
          break;
        case 'mechanic':
          newWorkspace.mechanics = insertIntoArray(newWorkspace.mechanics || []);
          break;
      }

      return {
        workspace: newWorkspace,
        isDirty: true,
      };
    });
    return headerKey;
  },

  // Delete a header from a category
  deleteHeader: (category, headerKey) => {
    set((state) => {
      if (!state.workspace) return state;

      const newWorkspace = { ...state.workspace };

      const filterHeaders = (arr: any[]) => 
        arr.filter((item: any) => item.headerKey !== headerKey);

      switch (category) {
        case 'device':
          newWorkspace.devices = filterHeaders(newWorkspace.devices || []);
          break;
        case 'guide':
          newWorkspace.guides = filterHeaders(newWorkspace.guides || []);
          break;
        case 'mechanic':
          newWorkspace.mechanics = filterHeaders(newWorkspace.mechanics || []);
          break;
      }

      return {
        workspace: newWorkspace,
        isDirty: true,
      };
    });
  },

  // Update header title
  updateHeader: (category, headerKey, newTitle) => {
    set((state) => {
      if (!state.workspace) return state;

      const newWorkspace = { ...state.workspace };

      const updateInArray = (arr: any[]) => 
        arr.map((item: any) => 
          item.headerKey === headerKey 
            ? { ...item, headerTitle: newTitle }
            : item
        );

      switch (category) {
        case 'device':
          newWorkspace.devices = updateInArray(newWorkspace.devices || []);
          break;
        case 'guide':
          newWorkspace.guides = updateInArray(newWorkspace.guides || []);
          break;
        case 'mechanic':
          newWorkspace.mechanics = updateInArray(newWorkspace.mechanics || []);
          break;
      }

      return {
        workspace: newWorkspace,
        isDirty: true,
      };
    });
  },
}));
