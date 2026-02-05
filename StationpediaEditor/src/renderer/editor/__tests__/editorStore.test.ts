/**
 * Tests for editor store
 */
import { describe, it, expect, beforeEach } from 'vitest';
import { useEditorStore } from '../editorStore';
import type { WorkspaceModel, DeviceDocument, OperationalDetail } from '@models/contentModel';

describe('editorStore', () => {
  beforeEach(() => {
    // Reset store state between tests
    useEditorStore.getState().reset();
  });

  describe('workspace management', () => {
    it('should set and get workspace', () => {
      const workspace: WorkspaceModel = {
        devices: [
          {
            deviceKey: 'Cpu',
            displayName: 'Logic Processor',
            operationalDetails: [],
          },
        ],
      };

      useEditorStore.getState().setWorkspace(workspace);
      expect(useEditorStore.getState().workspace).toEqual(workspace);
    });

    it('should start with null workspace', () => {
      expect(useEditorStore.getState().workspace).toBeNull();
    });

    it('should mark as dirty after setWorkspace', () => {
      const workspace: WorkspaceModel = { devices: [] };
      useEditorStore.getState().setWorkspace(workspace);
      expect(useEditorStore.getState().isDirty).toBe(true);
    });
  });

  describe('device selection', () => {
    beforeEach(() => {
      const workspace: WorkspaceModel = {
        devices: [
          {
            deviceKey: 'Cpu',
            displayName: 'Logic Processor',
            operationalDetails: [],
          },
          {
            deviceKey: 'Speaker',
            displayName: 'Speaker',
            operationalDetails: [],
          },
        ],
      };
      useEditorStore.getState().setWorkspace(workspace);
    });

    it('should select a device', () => {
      useEditorStore.getState().selectDevice('Cpu');
      expect(useEditorStore.getState().selectedDeviceKey).toBe('Cpu');
    });

    it('should update selectedDevice when a device is selected', () => {
      useEditorStore.getState().selectDevice('Cpu');
      const selected = useEditorStore.getState().selectedDevice;
      expect(selected?.deviceKey).toBe('Cpu');
    });

    it('should return null selectedDevice if device not found', () => {
      useEditorStore.getState().selectDevice('NonExistent');
      expect(useEditorStore.getState().selectedDevice).toBeNull();
    });
  });

  describe('device updates', () => {
    beforeEach(() => {
      const workspace: WorkspaceModel = {
        devices: [
          {
            deviceKey: 'Cpu',
            displayName: 'Logic Processor',
            operationalDetails: [],
          },
        ],
      };
      useEditorStore.getState().setWorkspace(workspace);
      useEditorStore.getState().selectDevice('Cpu');
    });

    it('should update device properties', () => {
      useEditorStore.getState().updateDevice('Cpu', {
        displayName: 'Updated CPU',
      });

      const device = useEditorStore.getState().selectedDevice;
      expect(device?.displayName).toBe('Updated CPU');
    });

    it('should mark as dirty on update', () => {
      useEditorStore.getState().updateDevice('Cpu', {
        displayName: 'Updated CPU',
      });
      expect(useEditorStore.getState().isDirty).toBe(true);
    });

    it('should update in the workspace devices array', () => {
      useEditorStore.getState().updateDevice('Cpu', {
        displayName: 'Updated CPU',
      });

      const device = useEditorStore.getState().workspace?.devices[0];
      expect(device?.displayName).toBe('Updated CPU');
    });
  });

  describe('operational details', () => {
    beforeEach(() => {
      const workspace: WorkspaceModel = {
        devices: [
          {
            deviceKey: 'Cpu',
            displayName: 'Logic Processor',
            operationalDetails: [
              {
                title: 'Operation',
                description: 'Basic operation',
              },
            ],
          },
        ],
      };
      useEditorStore.getState().setWorkspace(workspace);
      useEditorStore.getState().selectDevice('Cpu');
    });

    it('should add operational detail', () => {
      const detail: OperationalDetail = {
        title: 'Advanced Features',
        description: 'More info',
      };

      useEditorStore.getState().addOperationalDetail(detail);

      const device = useEditorStore.getState().selectedDevice;
      expect(device?.operationalDetails?.length).toBe(2);
      expect(device?.operationalDetails?.[1].title).toBe('Advanced Features');
    });

    it('should remove operational detail by index', () => {
      useEditorStore.getState().removeOperationalDetail(0);

      const device = useEditorStore.getState().selectedDevice;
      expect(device?.operationalDetails?.length).toBe(0);
    });

    it('should reorder operational details', () => {
      const detail: OperationalDetail = {
        title: 'Second',
        description: 'Second item',
      };
      useEditorStore.getState().addOperationalDetail(detail);

      useEditorStore.getState().reorderOperationalDetails(0, 1);

      const device = useEditorStore.getState().selectedDevice;
      expect(device?.operationalDetails?.[0].title).toBe('Second');
      expect(device?.operationalDetails?.[1].title).toBe('Operation');
    });

    it('should mark as dirty on add', () => {
      const detail: OperationalDetail = { title: 'New' };
      useEditorStore.getState().addOperationalDetail(detail);
      expect(useEditorStore.getState().isDirty).toBe(true);
    });

    it('should mark as dirty on remove', () => {
      useEditorStore.getState().removeOperationalDetail(0);
      expect(useEditorStore.getState().isDirty).toBe(true);
    });

    it('should mark as dirty on reorder', () => {
      const detail: OperationalDetail = { title: 'Second' };
      useEditorStore.getState().addOperationalDetail(detail);
      useEditorStore.getState().reorderOperationalDetails(0, 1);
      expect(useEditorStore.getState().isDirty).toBe(true);
    });
  });

  describe('dirty flag management', () => {
    it('should clear dirty flag', () => {
      useEditorStore.getState().setWorkspace({ devices: [] });
      expect(useEditorStore.getState().isDirty).toBe(true);

      useEditorStore.getState().clearDirty();
      expect(useEditorStore.getState().isDirty).toBe(false);
    });
  });

  describe('reset', () => {
    it('should reset all state', () => {
      const workspace: WorkspaceModel = { devices: [] };
      useEditorStore.getState().setWorkspace(workspace);
      useEditorStore.getState().selectDevice('Cpu');

      useEditorStore.getState().reset();

      expect(useEditorStore.getState().workspace).toBeNull();
      expect(useEditorStore.getState().selectedDeviceKey).toBeNull();
      expect(useEditorStore.getState().isDirty).toBe(false);
    });
  });

  describe('moveToCategory', () => {
    beforeEach(() => {
      const workspace: WorkspaceModel = {
        devices: [
          {
            deviceKey: 'Cpu',
            displayName: 'Logic Processor',
            operationalDetails: [],
          },
        ],
        guides: [
          {
            guideKey: 'Guide1',
            displayName: 'Guide 1',
            operationalDetails: [],
          },
        ],
        mechanics: [
          {
            guideKey: 'Mechanic1',
            displayName: 'Mechanic 1',
            operationalDetails: [],
          },
        ],
      };
      useEditorStore.getState().setWorkspace(workspace);
    });

    it('should move device to guide (device → guide key transformation)', () => {
      useEditorStore.getState().moveToCategory('Cpu', 'guide');

      const workspace = useEditorStore.getState().workspace;
      expect(workspace?.devices).toHaveLength(0);
      expect(workspace?.guides).toHaveLength(2);
      
      const movedGuide = workspace?.guides?.find((g) => g.guideKey === 'Cpu');
      expect(movedGuide).toBeDefined();
      expect(movedGuide?.displayName).toBe('Logic Processor');
      expect((movedGuide as any)?.deviceKey).toBeUndefined();
    });

    it('should move device to mechanic with key transformation', () => {
      useEditorStore.getState().moveToCategory('Cpu', 'mechanic');

      const workspace = useEditorStore.getState().workspace;
      expect(workspace?.devices).toHaveLength(0);
      expect(workspace?.mechanics).toHaveLength(2);
      
      const movedMechanic = workspace?.mechanics?.find((m: any) => m.guideKey === 'Cpu');
      expect(movedMechanic).toBeDefined();
      expect((movedMechanic as any)?.displayName).toBe('Logic Processor');
    });

    it('should move guide to device (guide → device key transformation)', () => {
      useEditorStore.getState().moveToCategory('Guide1', 'device');

      const workspace = useEditorStore.getState().workspace;
      expect(workspace?.guides).toHaveLength(0);
      expect(workspace?.devices).toHaveLength(2);
      
      const movedDevice = workspace?.devices.find((d) => d.deviceKey === 'Guide1');
      expect(movedDevice).toBeDefined();
      expect(movedDevice?.displayName).toBe('Guide 1');
    });

    it('should move guide to mechanic (no key change)', () => {
      useEditorStore.getState().moveToCategory('Guide1', 'mechanic');

      const workspace = useEditorStore.getState().workspace;
      expect(workspace?.guides).toHaveLength(0);
      expect(workspace?.mechanics).toHaveLength(2);
      
      const movedMechanic = workspace?.mechanics?.find((m: any) => m.guideKey === 'Guide1');
      expect(movedMechanic).toBeDefined();
      expect((movedMechanic as any)?.displayName).toBe('Guide 1');
    });

    it('should insert at specified index', () => {
      useEditorStore.getState().moveToCategory('Cpu', 'guide', 0);

      const workspace = useEditorStore.getState().workspace;
      expect(workspace?.guides?.[0].guideKey).toBe('Cpu');
      expect(workspace?.guides?.[1].guideKey).toBe('Guide1');
    });

    it('should append to end by default', () => {
      useEditorStore.getState().moveToCategory('Cpu', 'guide');

      const workspace = useEditorStore.getState().workspace;
      expect(workspace?.guides?.[1].guideKey).toBe('Cpu');
    });

    it('should clear selection if moved item was selected', () => {
      useEditorStore.getState().selectDevice('Cpu');
      expect(useEditorStore.getState().selectedDeviceKey).toBe('Cpu');

      useEditorStore.getState().moveToCategory('Cpu', 'guide');

      expect(useEditorStore.getState().selectedDeviceKey).toBeNull();
      expect(useEditorStore.getState().selectedItemType).toBeNull();
    });

    it('should mark as dirty', () => {
      useEditorStore.getState().clearDirty();
      expect(useEditorStore.getState().isDirty).toBe(false);

      useEditorStore.getState().moveToCategory('Cpu', 'guide');

      expect(useEditorStore.getState().isDirty).toBe(true);
    });
  });

  describe('reorderItem', () => {
    beforeEach(() => {
      const workspace: WorkspaceModel = {
        devices: [
          {
            deviceKey: 'Device1',
            displayName: 'Device 1',
            operationalDetails: [],
          },
          {
            deviceKey: 'Device2',
            displayName: 'Device 2',
            operationalDetails: [],
          },
          {
            deviceKey: 'Device3',
            displayName: 'Device 3',
            operationalDetails: [],
          },
        ],
        guides: [
          {
            guideKey: 'Guide1',
            displayName: 'Guide 1',
            operationalDetails: [],
          },
          {
            guideKey: 'Guide2',
            displayName: 'Guide 2',
            operationalDetails: [],
          },
        ],
      };
      useEditorStore.getState().setWorkspace(workspace);
    });

    it('should reorder item in device array', () => {
      useEditorStore.getState().reorderItem('Device1', 2, 'device');

      const workspace = useEditorStore.getState().workspace;
      expect(workspace?.devices.map((d) => d.deviceKey)).toEqual([
        'Device2',
        'Device3',
        'Device1',
      ]);
    });

    it('should move item forward', () => {
      useEditorStore.getState().reorderItem('Device3', 0, 'device');

      const workspace = useEditorStore.getState().workspace;
      expect(workspace?.devices.map((d) => d.deviceKey)).toEqual([
        'Device3',
        'Device1',
        'Device2',
      ]);
    });

    it('should reorder item in guide array', () => {
      useEditorStore.getState().reorderItem('Guide2', 0, 'guide');

      const workspace = useEditorStore.getState().workspace;
      expect(workspace?.guides?.map((g) => g.guideKey)).toEqual([
        'Guide2',
        'Guide1',
      ]);
    });

    it('should mark as dirty', () => {
      useEditorStore.getState().clearDirty();
      expect(useEditorStore.getState().isDirty).toBe(false);

      useEditorStore.getState().reorderItem('Device1', 2, 'device');

      expect(useEditorStore.getState().isDirty).toBe(true);
    });

    it('should not move if item not found', () => {
      const before = useEditorStore.getState().workspace?.devices.map((d) => d.deviceKey);
      
      useEditorStore.getState().reorderItem('NonExistent', 1, 'device');

      const after = useEditorStore.getState().workspace?.devices.map((d) => d.deviceKey);
      expect(before).toEqual(after);
    });
  });

  describe('addHeader', () => {
    beforeEach(() => {
      const workspace: WorkspaceModel = {
        guides: [
          {
            guideKey: 'Guide1',
            displayName: 'Guide 1',
            operationalDetails: [],
          },
        ],
        mechanics: [
          {
            guideKey: 'Mechanic1',
            displayName: 'Mechanic 1',
            operationalDetails: [],
          },
        ],
      };
      useEditorStore.getState().setWorkspace(workspace);
    });

    it('should create and add header to guides', () => {
      const headerKey = useEditorStore.getState().addHeader('guide', 'My Section');

      const workspace = useEditorStore.getState().workspace;
      expect(workspace?.guides).toHaveLength(2);

      const header = workspace?.guides?.find((g) => g.guideKey === headerKey);
      expect(header).toBeDefined();
      expect(header?.displayName).toBe('My Section');
      expect((header as any)?.isHeader).toBe(true);
    });

    it('should return unique header key', () => {
      const key1 = useEditorStore.getState().addHeader('guide', 'Header 1');
      const key2 = useEditorStore.getState().addHeader('guide', 'Header 2');

      expect(key1).toBeDefined();
      expect(key2).toBeDefined();
      expect(key1).not.toBe(key2);
      expect(key1).toMatch(/^Header_/);
      expect(key2).toMatch(/^Header_/);
    });

    it('should insert at specified index', () => {
      useEditorStore.getState().addHeader('guide', 'New Header', 0);

      const workspace = useEditorStore.getState().workspace;
      expect(workspace?.guides?.[0].displayName).toBe('New Header');
      expect(workspace?.guides?.[1].displayName).toBe('Guide 1');
    });

    it('should append to end by default', () => {
      const headerKey = useEditorStore.getState().addHeader('guide', 'New Header');

      const workspace = useEditorStore.getState().workspace;
      expect(workspace?.guides?.length).toBe(2);
      expect(workspace?.guides?.[1].guideKey).toBe(headerKey);
    });

    it('should create header in mechanics', () => {
      const headerKey = useEditorStore.getState().addHeader('mechanic', 'Mechanic Header');

      const workspace = useEditorStore.getState().workspace;
      expect(workspace?.mechanics).toHaveLength(2);

      const header = workspace?.mechanics?.find((m: any) => m.guideKey === headerKey);
      expect(header).toBeDefined();
      expect((header as any)?.isHeader).toBe(true);
    });

    it('should mark as dirty', () => {
      useEditorStore.getState().clearDirty();
      expect(useEditorStore.getState().isDirty).toBe(false);

      useEditorStore.getState().addHeader('guide', 'Header');

      expect(useEditorStore.getState().isDirty).toBe(true);
    });
  });

  describe('deleteHeader', () => {
    beforeEach(() => {
      const workspace: WorkspaceModel = {
        guides: [
          {
            guideKey: 'Header_123',
            displayName: 'Header 1',
            isHeader: true,
          },
          {
            guideKey: 'Guide1',
            displayName: 'Guide 1',
            operationalDetails: [],
          },
        ],
        mechanics: [
          {
            guideKey: 'Header_456',
            displayName: 'Mechanic Header',
            isHeader: true,
          },
          {
            guideKey: 'Mechanic1',
            displayName: 'Mechanic 1',
            operationalDetails: [],
          },
        ],
      };
      useEditorStore.getState().setWorkspace(workspace);
    });

    it('should delete header from guides', () => {
      useEditorStore.getState().deleteHeader('guide', 'Header_123');

      const workspace = useEditorStore.getState().workspace;
      expect(workspace?.guides).toHaveLength(1);
      expect(workspace?.guides?.[0].guideKey).toBe('Guide1');
    });

    it('should delete header from mechanics', () => {
      useEditorStore.getState().deleteHeader('mechanic', 'Header_456');

      const workspace = useEditorStore.getState().workspace;
      expect(workspace?.mechanics).toHaveLength(1);
      expect(workspace?.mechanics?.[0].guideKey).toBe('Mechanic1');
    });

    it('should clear selection if deleted header was selected', () => {
      useEditorStore.getState().selectDevice('Header_123');
      expect(useEditorStore.getState().selectedDeviceKey).toBe('Header_123');

      useEditorStore.getState().deleteHeader('guide', 'Header_123');

      expect(useEditorStore.getState().selectedDeviceKey).toBeNull();
      expect(useEditorStore.getState().selectedItemType).toBeNull();
    });

    it('should mark as dirty', () => {
      useEditorStore.getState().clearDirty();
      expect(useEditorStore.getState().isDirty).toBe(false);

      useEditorStore.getState().deleteHeader('guide', 'Header_123');

      expect(useEditorStore.getState().isDirty).toBe(true);
    });

    it('should not fail if header not found', () => {
      const before = useEditorStore.getState().workspace?.guides?.length;
      
      useEditorStore.getState().deleteHeader('guide', 'NonExistent');

      const after = useEditorStore.getState().workspace?.guides?.length;
      expect(before).toEqual(after);
    });
  });
});
