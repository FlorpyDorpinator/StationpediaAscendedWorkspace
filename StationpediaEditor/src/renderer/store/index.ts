/**
 * Zustand Store for Application State
 */
import { create } from 'zustand';
import type { DeviceDescription, ValidationError, OperationalDetail } from '@models/stationpedia';

export interface WorkspaceState {
  // Workspace
  workspacePath: string | null;
  isLoading: boolean;
  lastError: string | null;

  // Content
  devices: DeviceDescription[];
  guides: DeviceDescription[];
  mechanics: DeviceDescription[];
  allContent: DeviceDescription[];

  // Selection
  selectedItem: DeviceDescription | null;
  selectedType: 'device' | 'guide' | 'mechanic' | null;

  // Editor
  isDirty: boolean;
  editorContent: string;
  isSourceView: boolean;

  // Validation
  validationErrors: ValidationError[];

  // Actions
  setWorkspacePath: (path: string | null) => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
  setDevices: (devices: DeviceDescription[]) => void;
  setGuides: (guides: DeviceDescription[]) => void;
  setMechanics: (mechanics: DeviceDescription[]) => void;
  setAllContent: (content: DeviceDescription[]) => void;
  selectItem: (item: DeviceDescription | null, type?: 'device' | 'guide' | 'mechanic') => void;
  setDirty: (dirty: boolean) => void;
  setEditorContent: (content: string) => void;
  toggleSourceView: () => void;
  setValidationErrors: (errors: ValidationError[]) => void;
  updateSelectedItem: (updates: Partial<DeviceDescription>) => void;
  updateOperationalDetail: (index: number, detail: OperationalDetail) => void;
  addOperationalDetail: (detail: OperationalDetail) => void;
  removeOperationalDetail: (index: number) => void;
  reorderOperationalDetails: (fromIndex: number, toIndex: number) => void;
  reset: () => void;
}

const initialState = {
  workspacePath: null,
  isLoading: false,
  lastError: null,
  devices: [],
  guides: [],
  mechanics: [],
  allContent: [],
  selectedItem: null,
  selectedType: null,
  isDirty: false,
  editorContent: '',
  isSourceView: false,
  validationErrors: [],
};

export const useStore = create<WorkspaceState>((set, get) => ({
  ...initialState,

  setWorkspacePath: (path) => set({ workspacePath: path }),
  setLoading: (loading) => set({ isLoading: loading }),
  setError: (error) => set({ lastError: error }),
  
  setDevices: (devices) => set({ devices }),
  setGuides: (guides) => set({ guides }),
  setMechanics: (mechanics) => set({ mechanics }),
  setAllContent: (content) => set({ allContent: content }),

  selectItem: (item, type = 'device') => set({
    selectedItem: item,
    selectedType: type,
    editorContent: item?.pageDescription || '',
    isDirty: false,
  }),

  setDirty: (dirty) => set({ isDirty: dirty }),
  setEditorContent: (content) => set({ editorContent: content, isDirty: true }),
  toggleSourceView: () => set((state) => ({ isSourceView: !state.isSourceView })),
  setValidationErrors: (errors) => set({ validationErrors: errors }),

  updateSelectedItem: (updates) => {
    const { selectedItem, devices, guides, mechanics } = get();
    if (!selectedItem) return;

    const updated = { ...selectedItem, ...updates };
    set({ selectedItem: updated, isDirty: true });

    // Update in the appropriate list
    const updateList = (list: DeviceDescription[]) =>
      list.map((item) =>
        item.deviceKey === selectedItem.deviceKey ? updated : item
      );

    set({
      devices: updateList(devices),
      guides: updateList(guides),
      mechanics: updateList(mechanics),
    });
  },

  updateOperationalDetail: (index, detail) => {
    const { selectedItem } = get();
    if (!selectedItem) return;

    const details = [...(selectedItem.operationalDetails || [])];
    details[index] = detail;
    get().updateSelectedItem({ operationalDetails: details });
  },

  addOperationalDetail: (detail) => {
    const { selectedItem } = get();
    if (!selectedItem) return;

    const details = [...(selectedItem.operationalDetails || []), detail];
    get().updateSelectedItem({ operationalDetails: details });
  },

  removeOperationalDetail: (index) => {
    const { selectedItem } = get();
    if (!selectedItem) return;

    const details = [...(selectedItem.operationalDetails || [])];
    details.splice(index, 1);
    get().updateSelectedItem({ operationalDetails: details });
  },

  reorderOperationalDetails: (fromIndex, toIndex) => {
    const { selectedItem } = get();
    if (!selectedItem) return;

    const details = [...(selectedItem.operationalDetails || [])];
    const [removed] = details.splice(fromIndex, 1);
    details.splice(toIndex, 0, removed);
    get().updateSelectedItem({ operationalDetails: details });
  },

  reset: () => set(initialState),
}));
