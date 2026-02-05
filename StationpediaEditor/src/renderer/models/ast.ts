/**
 * Abstract Syntax Tree (AST) node types for Stationpedia TMP rich text
 * Supports lossless round-trip: TMP string → AST → TMP string
 */

// Base AST node types
export type ASTNodeType =
  | 'text'
  | 'paragraph'
  | 'bold'
  | 'italic'
  | 'underline'
  | 'strikethrough'
  | 'color'
  | 'link'
  | 'codeBlock'
  | 'bulletList'
  | 'numberedList'
  | 'image'
  | 'youtubeLink';

// Base interface for all nodes
export interface ASTNode {
  type: ASTNodeType;
  content?: ASTNode[];
  attrs?: Record<string, unknown>;
}

// ============================================================================
// TEXT NODES
// ============================================================================

export interface TextNode extends ASTNode {
  type: 'text';
  text: string;
  content?: never;
}

export interface ParagraphNode extends ASTNode {
  type: 'paragraph';
  content: ASTNode[];
}

// ============================================================================
// FORMATTING NODES
// ============================================================================

export interface BoldNode extends ASTNode {
  type: 'bold';
  content: ASTNode[];
}

export interface ItalicNode extends ASTNode {
  type: 'italic';
  content: ASTNode[];
}

export interface UnderlineNode extends ASTNode {
  type: 'underline';
  content: ASTNode[];
}

export interface StrikethroughNode extends ASTNode {
  type: 'strikethrough';
  content: ASTNode[];
}

export interface ColorNode extends ASTNode {
  type: 'color';
  color: string; // Hex color like #FF0000
  content: ASTNode[];
}

// ============================================================================
// LINK NODES
// ============================================================================

export interface InternalLinkNode {
  type: 'link';
  linkType: 'internal';
  target: string; // Device key or page key
  displayText?: string;
  content: ASTNode[];
  tocId?: string;
  originalFormat?: 'thing' | 'link' | 'html'; // Preserve original format for serialization
}

export interface ExternalLinkNode {
  type: 'link';
  linkType: 'external';
  url: string;
  content: ASTNode[];
}

export type LinkNode = InternalLinkNode | ExternalLinkNode;

// ============================================================================
// CODE BLOCKS
// ============================================================================

export interface CodeBlockNode extends ASTNode {
  type: 'codeBlock';
  language?: string; // e.g., 'mips', 'c'
  code: string;
  lineNumbers?: boolean;
  content?: never;
}

// ============================================================================
// LIST NODES
// ============================================================================

export interface BulletListNode extends ASTNode {
  type: 'bulletList';
  items: ASTNode[][]; // Each item is an array of nodes
  content?: never;
}

export interface NumberedListNode extends ASTNode {
  type: 'numberedList';
  items: ASTNode[][]; // Each item is an array of nodes
  content?: never;
}

// ============================================================================
// MEDIA NODES
// ============================================================================

export interface ImageNode extends ASTNode {
  type: 'image';
  src: string;
  alt?: string;
  width?: number;
  height?: number;
  content?: never;
}

export interface YouTubeLinkNode extends ASTNode {
  type: 'youtubeLink';
  videoId: string;
  label?: string;
  thumbnail?: string;
  content?: never;
}

// ============================================================================
// FACTORY FUNCTIONS
// ============================================================================

export function createTextNode(text: string): TextNode {
  return {
    type: 'text',
    text,
  };
}

export function createBoldNode(content: ASTNode[]): BoldNode {
  return {
    type: 'bold',
    content,
  };
}

export function createItalicNode(content: ASTNode[]): ItalicNode {
  return {
    type: 'italic',
    content,
  };
}

export function createUnderlineNode(content: ASTNode[]): UnderlineNode {
  return {
    type: 'underline',
    content,
  };
}

export function createStrikethroughNode(content: ASTNode[]): StrikethroughNode {
  return {
    type: 'strikethrough',
    content,
  };
}

export function createColorNode(color: string, content: ASTNode[]): ColorNode {
  return {
    type: 'color',
    color: normalizeHex(color),
    content,
  };
}

export function createInternalLinkNode(
  target: string,
  content: ASTNode[],
  tocId?: string,
  originalFormat?: 'thing' | 'link' | 'html'
): InternalLinkNode {
  return {
    type: 'link',
    linkType: 'internal',
    target,
    content,
    tocId,
    originalFormat,
  };
}

export function createExternalLinkNode(url: string, content: ASTNode[]): ExternalLinkNode {
  return {
    type: 'link',
    linkType: 'external',
    url,
    content,
  };
}

export function createCodeBlockNode(
  code: string,
  language?: string,
  lineNumbers?: boolean
): CodeBlockNode {
  return {
    type: 'codeBlock',
    language,
    code,
    lineNumbers,
  };
}

export function createBulletListNode(items: ASTNode[][]): BulletListNode {
  return {
    type: 'bulletList',
    items,
  };
}

export function createNumberedListNode(items: ASTNode[][]): NumberedListNode {
  return {
    type: 'numberedList',
    items,
  };
}

export function createImageNode(src: string, alt?: string): ImageNode {
  return {
    type: 'image',
    src,
    alt,
  };
}

export function createYouTubeLinkNode(videoId: string, label?: string): YouTubeLinkNode {
  return {
    type: 'youtubeLink',
    videoId,
    label,
  };
}

// ============================================================================
// UTILITIES
// ============================================================================

/**
 * Normalize hex color to uppercase #RRGGBB format
 */
export function normalizeHex(color: string): string {
  return color.toUpperCase();
}

/**
 * Check if a node is a leaf node (no children)
 */
export function isLeafNode(node: ASTNode): boolean {
  return (
    node.type === 'text' ||
    node.type === 'codeBlock' ||
    node.type === 'image' ||
    node.type === 'youtubeLink'
  );
}

/**
 * Check if a node is a container node (has content/children)
 */
export function isContainerNode(node: ASTNode): boolean {
  return (
    node.type === 'paragraph' ||
    node.type === 'bold' ||
    node.type === 'italic' ||
    node.type === 'underline' ||
    node.type === 'strikethrough' ||
    node.type === 'color' ||
    node.type === 'link'
  );
}

/**
 * Check if a node is a list node
 */
export function isListNode(node: ASTNode): boolean {
  return node.type === 'bulletList' || node.type === 'numberedList';
}

/**
 * Extract all text content from an AST node tree
 */
export function extractText(nodes: ASTNode[]): string {
  let text = '';
  for (const node of nodes) {
    if (node.type === 'text') {
      text += (node as TextNode).text;
    } else if (node.content) {
      text += extractText(node.content);
    } else if (node.type === 'bulletList' || node.type === 'numberedList') {
      const list = node as BulletListNode | NumberedListNode;
      for (const item of list.items) {
        text += extractText(item);
      }
    }
  }
  return text;
}
