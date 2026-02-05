/**
 * Shared State Service - IPC-based state synchronization
 * Provides state management for simulator and main editor windows
 */

export type SimulatorMode = 'vanilla' | 'ascended';

export interface SharedDeviceState {
  currentDeviceKey: string | null;
  mode: SimulatorMode;
}

export interface StateChangeListener {
  (state: SharedDeviceState): void;
}

class SharedStateService {
  private listeners: Set<StateChangeListener> = new Set();
  private currentState: SharedDeviceState = {
    currentDeviceKey: null,
    mode: 'ascended',
  };

  /**
   * Subscribe to state changes
   */
  subscribe(listener: StateChangeListener): () => void {
    this.listeners.add(listener);

    // Return unsubscribe function
    return () => {
      this.listeners.delete(listener);
    };
  }

  /**
   * Notify all listeners of state change
   */
  private notifyListeners() {
    this.listeners.forEach((listener) => {
      listener(this.currentState);
    });
  }

  /**
   * Update state
   */
  updateState(updates: Partial<SharedDeviceState>) {
    this.currentState = { ...this.currentState, ...updates };
    this.notifyListeners();
  }

  /**
   * Get current state
   */
  getState(): SharedDeviceState {
    return { ...this.currentState };
  }

  /**
   * Set current device
   */
  setCurrentDevice(deviceKey: string | null) {
    this.updateState({ currentDeviceKey: deviceKey });
  }

  /**
   * Set mode
   */
  setMode(mode: SimulatorMode) {
    this.updateState({ mode });
    
    // Persist mode preference
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem('simulator-mode', mode);
    }
  }

  /**
   * Load persisted preferences
   */
  loadPreferences() {
    if (typeof localStorage !== 'undefined') {
      const savedMode = localStorage.getItem('simulator-mode') as SimulatorMode | null;
      if (savedMode === 'vanilla' || savedMode === 'ascended') {
        this.currentState.mode = savedMode;
      }
    }
  }

  /**
   * Clear all listeners
   */
  clearListeners() {
    this.listeners.clear();
  }
}

export const sharedState = new SharedStateService();
