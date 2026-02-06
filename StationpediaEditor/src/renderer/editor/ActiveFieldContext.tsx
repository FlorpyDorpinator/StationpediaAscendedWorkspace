/**
 * ActiveFieldContext - Tracks which textarea is currently active for formatting toolbar
 */
import React, { createContext, useContext, useState, useCallback, useRef } from 'react';

interface ActiveFieldState {
  textareaRef: React.RefObject<HTMLTextAreaElement> | null;
  anchorElement: HTMLElement | null;
  onValueChange: ((newValue: string) => void) | null;
  setActiveField: (
    ref: React.RefObject<HTMLTextAreaElement>,
    anchor: HTMLElement,
    onChange: (newValue: string) => void,
  ) => void;
  clearActiveField: () => void;
}

const ActiveFieldContext = createContext<ActiveFieldState>({
  textareaRef: null,
  anchorElement: null,
  onValueChange: null,
  setActiveField: () => {},
  clearActiveField: () => {},
});

export const useActiveField = () => useContext(ActiveFieldContext);

export const ActiveFieldProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [textareaRef, setTextareaRef] = useState<React.RefObject<HTMLTextAreaElement> | null>(null);
  const [anchorElement, setAnchorElement] = useState<HTMLElement | null>(null);
  const [onValueChange, setOnValueChange] = useState<((newValue: string) => void) | null>(null);

  const setActiveField = useCallback((
    ref: React.RefObject<HTMLTextAreaElement>,
    anchor: HTMLElement,
    onChange: (newValue: string) => void,
  ) => {
    setTextareaRef(ref);
    setAnchorElement(anchor);
    // Wrap in arrow fn to prevent React from treating it as a state updater
    setOnValueChange(() => onChange);
  }, []);

  const clearActiveField = useCallback(() => {
    setTextareaRef(null);
    setAnchorElement(null);
    setOnValueChange(null);
  }, []);

  return (
    <ActiveFieldContext.Provider value={{ textareaRef, anchorElement, onValueChange, setActiveField, clearActiveField }}>
      {children}
    </ActiveFieldContext.Provider>
  );
};
