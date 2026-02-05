/**
 * useKeyboardShortcuts Hook
 * Manages global keyboard shortcuts for the editor
 */
import { useEffect } from 'react';

export interface KeyboardShortcuts {
  save?: () => void;
  open?: () => void;
  saveAs?: () => void;
  new?: () => void;
  refresh?: () => void;
  undo?: () => void;
  redo?: () => void;
  close?: () => void;
}

export const useKeyboardShortcuts = (shortcuts: KeyboardShortcuts) => {
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      // Avoid conflicts with text editing
      const target = event.target as HTMLElement;
      const isTyping = target.tagName === 'INPUT' || target.tagName === 'TEXTAREA' || target.contentEditable === 'true';

      // Ctrl+S / Cmd+S: Save
      if ((event.ctrlKey || event.metaKey) && event.key === 's' && !isTyping) {
        event.preventDefault();
        shortcuts.save?.();
      }
      // Ctrl+O / Cmd+O: Open
      else if ((event.ctrlKey || event.metaKey) && event.key === 'o' && !isTyping) {
        event.preventDefault();
        shortcuts.open?.();
      }
      // Ctrl+Shift+S / Cmd+Shift+S: Save As
      else if ((event.ctrlKey || event.metaKey) && event.shiftKey && event.key === 'S' && !isTyping) {
        event.preventDefault();
        shortcuts.saveAs?.();
      }
      // Ctrl+N / Cmd+N: New
      else if ((event.ctrlKey || event.metaKey) && event.key === 'n' && !isTyping) {
        event.preventDefault();
        shortcuts.new?.();
      }
      // F5: Refresh Preview
      else if (event.key === 'F5' && !isTyping) {
        event.preventDefault();
        shortcuts.refresh?.();
      }
      // Ctrl+W / Cmd+W: Close
      else if ((event.ctrlKey || event.metaKey) && event.key === 'w' && !isTyping) {
        event.preventDefault();
        shortcuts.close?.();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [shortcuts]);
};

/**
 * Get keyboard shortcut documentation
 */
export const getKeyboardShortcutsHelp = (): Array<{ keys: string; action: string }> => [
  { keys: 'Ctrl+S', action: 'Save changes' },
  { keys: 'Ctrl+O', action: 'Open workspace' },
  { keys: 'Ctrl+Shift+S', action: 'Save As' },
  { keys: 'Ctrl+N', action: 'New workspace' },
  { keys: 'F5', action: 'Refresh preview' },
  { keys: 'Ctrl+W', action: 'Close workspace' },
  { keys: 'Ctrl+Z', action: 'Undo' },
  { keys: 'Ctrl+Y', action: 'Redo' },
  { keys: 'Ctrl+B', action: 'Bold' },
  { keys: 'Ctrl+I', action: 'Italic' },
  { keys: 'Ctrl+U', action: 'Underline' },
];
