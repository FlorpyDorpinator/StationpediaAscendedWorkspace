/**
 * RichTextEditor - TipTap-based rich text editor
 * Parses TMP format to AST and back
 */
import React, { useState } from 'react';
import { useEditor, EditorContent, Editor } from '@tiptap/react';
import StarterKit from '@tiptap/starter-kit';
import Underline from '@tiptap/extension-underline';
import Link from '@tiptap/extension-link';
import Image from '@tiptap/extension-image';
import Placeholder from '@tiptap/extension-placeholder';
import type { WorkspaceModel } from '@models/contentModel';
import { LinkModal } from './LinkModal';

interface RichTextEditorProps {
  content?: string;
  onChange?: (content: string) => void;
  placeholder?: string;
  readOnly?: boolean;
  workspace?: WorkspaceModel | null;
}

interface MenuBarProps {
  editor: Editor | null;
  onOpenLinkModal: () => void;
  onInsertHeader: () => void;
}

const MenuBar: React.FC<MenuBarProps> = ({ editor, onOpenLinkModal, onInsertHeader }) => {
  if (!editor) return null;

  return (
    <div className="flex flex-wrap gap-1 p-2 bg-gray-900 border-b border-gray-700 rounded-t">
      <button
        onClick={() => editor.chain().focus().toggleBold().run()}
        disabled={!editor.can().chain().focus().toggleBold().run()}
        className={`px-3 py-1 rounded text-sm font-semibold transition-colors ${
          editor.isActive('bold')
            ? 'bg-cyan-600 text-white'
            : 'bg-gray-800 text-gray-300 hover:bg-gray-700'
        }`}
        title="Bold (Ctrl+B)"
      >
        <strong>B</strong>
      </button>

      <button
        onClick={() => editor.chain().focus().toggleItalic().run()}
        disabled={!editor.can().chain().focus().toggleItalic().run()}
        className={`px-3 py-1 rounded text-sm transition-colors ${
          editor.isActive('italic')
            ? 'bg-cyan-600 text-white'
            : 'bg-gray-800 text-gray-300 hover:bg-gray-700'
        }`}
        title="Italic (Ctrl+I)"
      >
        <em>I</em>
      </button>

      <button
        onClick={() => editor.chain().focus().toggleUnderline().run()}
        disabled={!editor.can().chain().focus().toggleUnderline().run()}
        className={`px-3 py-1 rounded text-sm transition-colors ${
          editor.isActive('underline')
            ? 'bg-cyan-600 text-white'
            : 'bg-gray-800 text-gray-300 hover:bg-gray-700'
        }`}
        title="Underline (Ctrl+U)"
      >
        <u>U</u>
      </button>

      <button
        onClick={() => editor.chain().focus().toggleStrike().run()}
        disabled={!editor.can().chain().focus().toggleStrike().run()}
        className={`px-3 py-1 rounded text-sm transition-colors ${
          editor.isActive('strike')
            ? 'bg-cyan-600 text-white'
            : 'bg-gray-800 text-gray-300 hover:bg-gray-700'
        }`}
        title="Strikethrough"
      >
        <s>S</s>
      </button>

      <div className="w-px bg-gray-700" />

      <button
        onClick={() => editor.chain().focus().toggleHeading({ level: 2 }).run()}
        className={`px-3 py-1 rounded text-sm transition-colors ${
          editor.isActive('heading', { level: 2 })
            ? 'bg-cyan-600 text-white'
            : 'bg-gray-800 text-gray-300 hover:bg-gray-700'
        }`}
        title="Heading 2"
      >
        H2
      </button>

      <button
        onClick={() => editor.chain().focus().toggleHeading({ level: 3 }).run()}
        className={`px-3 py-1 rounded text-sm transition-colors ${
          editor.isActive('heading', { level: 3 })
            ? 'bg-cyan-600 text-white'
            : 'bg-gray-800 text-gray-300 hover:bg-gray-700'
        }`}
        title="Heading 3"
      >
        H3
      </button>

      <button
        onClick={onInsertHeader}
        className="px-3 py-1 rounded text-sm bg-orange-600 hover:bg-orange-700 text-white transition-colors"
        title="Insert Game Header {HEADER:Text}"
      >
        📋 Header
      </button>

      <div className="w-px bg-gray-700" />

      <button
        onClick={() => editor.chain().focus().toggleBulletList().run()}
        className={`px-3 py-1 rounded text-sm transition-colors ${
          editor.isActive('bulletList')
            ? 'bg-cyan-600 text-white'
            : 'bg-gray-800 text-gray-300 hover:bg-gray-700'
        }`}
        title="Bullet List"
      >
        •
      </button>

      <button
        onClick={() => editor.chain().focus().toggleOrderedList().run()}
        className={`px-3 py-1 rounded text-sm transition-colors ${
          editor.isActive('orderedList')
            ? 'bg-cyan-600 text-white'
            : 'bg-gray-800 text-gray-300 hover:bg-gray-700'
        }`}
        title="Numbered List"
      >
        1.
      </button>

      <div className="w-px bg-gray-700" />

      <button
        onClick={() => editor.chain().focus().toggleCodeBlock().run()}
        className={`px-3 py-1 rounded text-sm transition-colors ${
          editor.isActive('codeBlock')
            ? 'bg-cyan-600 text-white'
            : 'bg-gray-800 text-gray-300 hover:bg-gray-700'
        }`}
        title="Code Block"
      >
        &lt;/&gt;
      </button>

      <button
        onClick={onOpenLinkModal}
        className={`px-3 py-1 rounded text-sm transition-colors ${
          editor.isActive('link')
            ? 'bg-cyan-600 text-white'
            : 'bg-gray-800 text-gray-300 hover:bg-gray-700'
        }`}
        title="Insert Stationpedia Link"
      >
        🔗
      </button>

      <div className="w-px bg-gray-700" />

      <button
        onClick={() => editor.chain().focus().undo().run()}
        disabled={!editor.can().chain().focus().undo().run()}
        className="px-3 py-1 rounded text-sm bg-gray-800 text-gray-300 hover:bg-gray-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        title="Undo"
      >
        ↶
      </button>

      <button
        onClick={() => editor.chain().focus().redo().run()}
        disabled={!editor.can().chain().focus().redo().run()}
        className="px-3 py-1 rounded text-sm bg-gray-800 text-gray-300 hover:bg-gray-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        title="Redo"
      >
        ↷
      </button>
    </div>
  );
};

export const RichTextEditor: React.FC<RichTextEditorProps> = ({
  content = '',
  onChange,
  placeholder = 'Enter rich text...',
  readOnly = false,
  workspace = null,
}) => {
  const [isLinkModalOpen, setIsLinkModalOpen] = useState(false);

  const editor = useEditor({
    extensions: [
      StarterKit,
      Underline,
      Link.configure({
        openOnClick: false,
      }),
      Image,
      Placeholder.configure({
        placeholder,
      }),
    ],
    content,
    editable: !readOnly,
    onUpdate: ({ editor: editorInstance }) => {
      if (onChange) {
        onChange(editorInstance.getHTML());
      }
    },
  });

  const handleInsertLink = (link: string, displayText: string) => {
    if (editor) {
      // Insert the raw game format link tag directly
      const { from, to } = editor.state.selection;
      editor.chain().focus().insertContentAt({ from, to }, link).run();
    }
  };

  const handleInsertHeader = () => {
    if (editor) {
      const headerText = prompt('Enter header text:');
      if (headerText) {
        // Insert game format header: {HEADER:Text}
        const headerTag = `{HEADER:${headerText}}`;
        const { from, to } = editor.state.selection;
        editor.chain().focus().insertContentAt({ from, to }, headerTag).run();
      }
    }
  };

  if (!editor) return null;

  return (
    <>
      <div className="flex flex-col h-full bg-gray-950 rounded border border-gray-700">
        <MenuBar 
          editor={editor} 
          onOpenLinkModal={() => setIsLinkModalOpen(true)}
          onInsertHeader={handleInsertHeader}
        />
        <EditorContent
          editor={editor}
          className="flex-1 overflow-auto p-4 text-gray-200"
        />
      </div>
      <LinkModal
        workspace={workspace}
        isOpen={isLinkModalOpen}
        onClose={() => setIsLinkModalOpen(false)}
        onSelectLink={handleInsertLink}
      />
    </>
  );
};
