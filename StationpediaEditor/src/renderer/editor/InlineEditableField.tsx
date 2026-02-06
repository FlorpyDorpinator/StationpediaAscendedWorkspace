/**
 * InlineEditableField - Always-visible textarea that registers with ActiveFieldContext on focus
 * Includes TMP rich text preview overlay so formatting tags render visually.
 */
import React, { useState, useRef, useCallback, useEffect, useMemo } from 'react';
import { useActiveField } from './ActiveFieldContext';

/** Convert TMP markup to safe HTML for the preview overlay */
function parseTMPToHTML(text: string): string {
  if (!text) return '';
  let html = text;
  // Escape HTML entities first
  html = html.replace(/&/g, '&amp;');
  html = html.replace(/</g, '&lt;');
  html = html.replace(/>/g, '&gt;');
  // Bold
  html = html.replace(/&lt;b&gt;/gi, '<b>');
  html = html.replace(/&lt;\/b&gt;/gi, '</b>');
  // Italic
  html = html.replace(/&lt;i&gt;/gi, '<i>');
  html = html.replace(/&lt;\/i&gt;/gi, '</i>');
  // Underline
  html = html.replace(/&lt;u&gt;/gi, '<u>');
  html = html.replace(/&lt;\/u&gt;/gi, '</u>');
  // Strikethrough
  html = html.replace(/&lt;s&gt;/gi, '<s>');
  html = html.replace(/&lt;\/s&gt;/gi, '</s>');
  // Color tags: <color=#HEX> or <color=name>
  html = html.replace(/&lt;color=([^&]+)&gt;/gi, '<span style="color:$1">');
  html = html.replace(/&lt;\/color&gt;/gi, '</span>');
  // Size tags: <size=150%> or <size=24>
  html = html.replace(/&lt;size=([^&]+)&gt;/gi, '<span style="font-size:$1">');
  html = html.replace(/&lt;\/size&gt;/gi, '</span>');
  // Links: {LINK:Key;DisplayText}
  html = html.replace(/\{LINK:([^;]+);([^}]+)\}/g, '<span style="color:#FF7A18;text-decoration:underline">$2</span>');
  // Headers: {HEADER:text}
  html = html.replace(/\{HEADER:([^}]+)\}/g, '<span style="font-weight:bold;font-size:120%;color:#FF7A18">$1</span>');
  // Newlines to <br>
  html = html.replace(/\n/g, '<br>');
  return html;
}

interface InlineEditableFieldProps {
  value: string;
  onChange: (value: string) => void;
  onBlur?: () => void;
  onRemove?: () => void;
  placeholder?: string;
  label?: string;
  rows?: number;
  showToolbar?: boolean;
  className?: string;
  mono?: boolean;
}

export const InlineEditableField: React.FC<InlineEditableFieldProps> = ({
  value,
  onChange,
  onBlur,
  onRemove,
  placeholder = 'Enter text...',
  label,
  rows = 1,
  showToolbar = true,
  className = '',
  mono = true,
}) => {
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const overlayRef = useRef<HTMLDivElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);
  const { setActiveField, clearActiveField } = useActiveField();
  const [localValue, setLocalValue] = useState(value);
  const [isFocused, setIsFocused] = useState(false);
  const blurTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  // Auto-resize textarea to fit content
  const autoResize = useCallback(() => {
    const ta = textareaRef.current;
    if (!ta) return;
    ta.style.height = 'auto';
    ta.style.height = `${ta.scrollHeight}px`;
  }, []);

  // Sync external value changes
  useEffect(() => {
    setLocalValue(value);
  }, [value]);

  // Resize when value changes (including external updates)
  useEffect(() => {
    autoResize();
  }, [localValue, autoResize]);

  const handleFocus = useCallback(() => {
    // Cancel any pending blur
    if (blurTimeoutRef.current) {
      clearTimeout(blurTimeoutRef.current);
      blurTimeoutRef.current = null;
    }
    setIsFocused(true);

    if (showToolbar && textareaRef.current && containerRef.current) {
      setActiveField(textareaRef, containerRef.current, (newValue: string) => {
        setLocalValue(newValue);
        onChange(newValue);
      });
    }
  }, [showToolbar, setActiveField, onChange]);

  const handleBlur = useCallback(() => {
    // Delay blur to allow clicking toolbar buttons
    blurTimeoutRef.current = setTimeout(() => {
      setIsFocused(false);
      clearActiveField();
      // Commit the local value
      if (localValue !== value) {
        onChange(localValue);
      }
      onBlur?.();
    }, 200);
  }, [clearActiveField, localValue, value, onChange, onBlur]);

  const handleChange = useCallback((e: React.ChangeEvent<HTMLTextAreaElement>) => {
    const newValue = e.target.value;
    setLocalValue(newValue);
    onChange(newValue);
    // Resize immediately on input
    const ta = e.target;
    ta.style.height = 'auto';
    ta.style.height = `${ta.scrollHeight}px`;
  }, [onChange]);

  // Sync scroll between textarea and overlay
  const handleScroll = useCallback(() => {
    if (textareaRef.current && overlayRef.current) {
      overlayRef.current.scrollTop = textareaRef.current.scrollTop;
      overlayRef.current.scrollLeft = textareaRef.current.scrollLeft;
    }
  }, []);

  // Cleanup timeout on unmount
  useEffect(() => {
    return () => {
      if (blurTimeoutRef.current) {
        clearTimeout(blurTimeoutRef.current);
      }
    };
  }, []);

  const isTitle = rows === 1;

  // Always show the WYSIWYG overlay so raw TMP tags are never visible
  const previewHTML = useMemo(() => parseTMPToHTML(localValue), [localValue]);

  return (
    <div ref={containerRef} className={className}>
      {label && (
        <div className="flex items-center gap-2 mb-1">
          <span className="text-sm font-semibold text-gray-200">{label}</span>
          {onRemove && (
            <button
              onClick={onRemove}
              className="text-gray-500 hover:text-red-400 transition-colors"
              title={`Remove ${label}`}
            >
              <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          )}
        </div>
      )}
      <div className="relative">
        <textarea
          ref={textareaRef}
          value={localValue}
          onChange={handleChange}
          onFocus={handleFocus}
          onBlur={handleBlur}
          onScroll={handleScroll}
          placeholder={placeholder}
          rows={1}
          className={`w-full px-2 py-1.5 bg-stationpedia-surface border border-stationpedia-border rounded text-sm placeholder-gray-500 focus:outline-none focus:border-stationpedia-accent focus:ring-1 focus:ring-stationpedia-accent resize-none overflow-hidden text-transparent caret-white selection:bg-stationpedia-accent/30 ${
            mono ? 'font-mono' : ''
          }`}
        />
        <div
          ref={overlayRef}
          className={`absolute inset-0 px-2 py-1.5 text-sm pointer-events-none overflow-hidden whitespace-pre-wrap break-words ${
            mono ? 'font-mono' : ''
          } ${isTitle ? 'font-semibold text-gray-200' : 'text-gray-300'}`}
          dangerouslySetInnerHTML={{ __html: previewHTML }}
        />
      </div>
    </div>
  );
};
