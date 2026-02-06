/**
 * FloatingFormattingToolbar - Formatting toolbar that appears near the active textarea
 * Shows B/I/U/S, colors, sizes, lists, links, headers
 */
import React from 'react';
import { useActiveField } from './ActiveFieldContext';
import { useFormattingActions } from './useFormattingActions';

interface FloatingFormattingToolbarProps {
  onOpenLinkModal: () => void;
}

export const FloatingFormattingToolbar: React.FC<FloatingFormattingToolbarProps> = ({
  onOpenLinkModal,
}) => {
  const { textareaRef, anchorElement, onValueChange } = useActiveField();
  const actions = useFormattingActions(textareaRef, onValueChange);

  if (!anchorElement || !textareaRef) return null;

  // Prevent mousedown from stealing focus from the active textarea
  const preventFocusLoss = (e: React.MouseEvent) => {
    e.preventDefault();
  };

  const btnClass = "px-2 py-1 text-xs bg-stationpedia-surface hover:bg-gray-700 rounded text-gray-300 transition-colors";
  const divider = <div className="w-px h-6 bg-stationpedia-border mx-1" />;

  return (
    <div
      className="flex flex-wrap gap-1 p-2 bg-stationpedia-surface/80 backdrop-blur-sm rounded border border-stationpedia-accent/30 mb-1 sticky top-0 z-10"
      onMouseDown={preventFocusLoss}
    >
      {/* Text Formatting */}
      <button onClick={() => actions.insertTMPTag('b')} className={`${btnClass} font-bold`} title="Bold: <b>text</b>">
        B
      </button>
      <button onClick={() => actions.insertTMPTag('i')} className={`${btnClass} italic`} title="Italic: <i>text</i>">
        I
      </button>
      <button onClick={() => actions.insertTMPTag('u')} className={`${btnClass} underline`} title="Underline: <u>text</u>">
        U
      </button>
      <button onClick={() => actions.insertTMPTag('s')} className={`${btnClass} line-through`} title="Strikethrough: <s>text</s>">
        S
      </button>

      {divider}

      {/* Colors */}
      <button onClick={() => actions.insertColorTag('#FF7A18')} className={btnClass} style={{ color: '#FF7A18' }} title="Orange">
        🟠
      </button>
      <button onClick={() => actions.insertColorTag('#00FF00')} className={btnClass} style={{ color: '#00FF00' }} title="Green">
        🟢
      </button>
      <button onClick={() => actions.insertColorTag('#FF0000')} className={btnClass} style={{ color: '#FF0000' }} title="Red">
        🔴
      </button>
      <button onClick={() => actions.insertColorTag('#FFFF00')} className={btnClass} style={{ color: '#FFFF00' }} title="Yellow">
        🟡
      </button>
      <button onClick={() => actions.insertColorTag('#00FFFF')} className={btnClass} style={{ color: '#00FFFF' }} title="Cyan">
        🔵
      </button>

      {divider}

      {/* Size */}
      <button onClick={() => actions.insertSizeTag('150%')} className={btnClass} title="Large text">
        A+
      </button>
      <button onClick={() => actions.insertSizeTag('75%')} className={`${btnClass} text-[10px]`} title="Small text">
        A-
      </button>

      {divider}

      {/* Lists & Structure */}
      <button onClick={() => actions.insertBulletList()} className={btnClass} title="Bullet list">
        • List
      </button>
      <button onClick={() => actions.insertNumberedList()} className={btnClass} title="Numbered list">
        1. List
      </button>

      {divider}

      {/* Links & Headers */}
      <button onClick={onOpenLinkModal} className={btnClass} title="Link to page: {LINK:PageKey;DisplayText}">
        🔗 Link
      </button>
      <button onClick={() => actions.insertHeader()} className="px-2 py-1 text-xs bg-orange-600 hover:bg-orange-700 rounded text-white transition-colors" title="Insert In-Line Header">
        📋 Header
      </button>
      <button onClick={() => actions.insertText('\n')} className={btnClass} title="New line">
        ↵
      </button>
    </div>
  );
};
