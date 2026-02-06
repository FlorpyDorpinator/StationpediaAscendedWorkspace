/**
 * useFormattingActions - Hook for TMP formatting operations on any textarea
 * Extracted from DeviceSectionsEditor to support inline editing
 */
import { useCallback } from 'react';

export interface FormattingActions {
  insertTMPTag: (tag: string) => void;
  insertColorTag: (color: string) => void;
  insertSizeTag: (size: string) => void;
  insertBulletList: () => void;
  insertNumberedList: () => void;
  insertHeader: () => void;
  insertText: (text: string) => void;
  insertAtCursor: (before: string, after?: string) => void;
}

export function useFormattingActions(
  textareaRef: React.RefObject<HTMLTextAreaElement> | null,
  onValueChange: ((newValue: string) => void) | null,
): FormattingActions {
  const getSelectionRange = useCallback(() => {
    if (!textareaRef?.current) return null;
    const start = textareaRef.current.selectionStart;
    const end = textareaRef.current.selectionEnd;
    const text = textareaRef.current.value;
    return { start, end, selectedText: text.substring(start, end) };
  }, [textareaRef]);

  const insertAtCursor = useCallback((before: string, after: string = '') => {
    if (!textareaRef?.current || !onValueChange) return;

    const range = getSelectionRange();
    if (!range) return;

    const { start, end, selectedText } = range;
    const currentText = textareaRef.current.value;

    let newText: string;
    let newCursorPos: number;

    if (selectedText) {
      newText = currentText.substring(0, start) + before + selectedText + after + currentText.substring(end);
      newCursorPos = start + before.length + selectedText.length + after.length;
    } else {
      newText = currentText.substring(0, start) + before + after + currentText.substring(end);
      newCursorPos = start + before.length;
    }

    onValueChange(newText);

    // Restore cursor position after React re-render
    setTimeout(() => {
      if (textareaRef?.current) {
        textareaRef.current.focus();
        textareaRef.current.setSelectionRange(newCursorPos, newCursorPos);
      }
    }, 0);
  }, [textareaRef, onValueChange, getSelectionRange]);

  const insertTMPTag = useCallback((tag: string) => {
    insertAtCursor(`<${tag}>`, `</${tag}>`);
  }, [insertAtCursor]);

  const insertColorTag = useCallback((color: string) => {
    insertAtCursor(`<color=${color}>`, '</color>');
  }, [insertAtCursor]);

  const insertSizeTag = useCallback((size: string) => {
    insertAtCursor(`<size=${size}>`, '</size>');
  }, [insertAtCursor]);

  const insertBulletList = useCallback(() => {
    const range = getSelectionRange();
    if (!range) return;

    if (range.selectedText) {
      const lines = range.selectedText.split('\n');
      const bulletLines = lines.map(line => line.trim() ? `• ${line.trim()}` : '').join('\n');
      insertAtCursor(bulletLines, '');
    } else {
      insertAtCursor('• ', '');
    }
  }, [getSelectionRange, insertAtCursor]);

  const insertNumberedList = useCallback(() => {
    const range = getSelectionRange();
    if (!range) return;

    if (range.selectedText) {
      const lines = range.selectedText.split('\n');
      const numberedLines = lines.map((line, i) => line.trim() ? `${i + 1}. ${line.trim()}` : '').join('\n');
      insertAtCursor(numberedLines, '');
    } else {
      insertAtCursor('1. ', '');
    }
  }, [getSelectionRange, insertAtCursor]);

  const insertHeader = useCallback(() => {
    insertAtCursor('{HEADER:', '}');
  }, [insertAtCursor]);

  const insertText = useCallback((text: string) => {
    insertAtCursor(text, '');
  }, [insertAtCursor]);

  return {
    insertTMPTag,
    insertColorTag,
    insertSizeTag,
    insertBulletList,
    insertNumberedList,
    insertHeader,
    insertText,
    insertAtCursor,
  };
}
