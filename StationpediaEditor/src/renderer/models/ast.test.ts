/**
 * Tests for AST node types and functionality
 */
import { describe, it, expect } from 'vitest';
import {
  TextNode,
  ParagraphNode,
  BoldNode,
  ItalicNode,
  UnderlineNode,
  StrikethroughNode,
  ColorNode,
  LinkNode,
  CodeBlockNode,
  BulletListNode,
  NumberedListNode,
  ImageNode,
  YouTubeLinkNode,
  ASTNode,
  createTextNode,
  createBoldNode,
  createColorNode,
} from './ast';

describe('AST Nodes', () => {
  describe('TextNode', () => {
    it('should create a text node', () => {
      const node = createTextNode('Hello');
      expect(node.type).toBe('text');
      expect(node.text).toBe('Hello');
    });

    it('should preserve special characters', () => {
      const text = 'Special <>&"\' chars';
      const node = createTextNode(text);
      expect(node.text).toBe(text);
    });
  });

  describe('BoldNode', () => {
    it('should create a bold node with children', () => {
      const child = createTextNode('bold text');
      const node = createBoldNode([child]);
      expect(node.type).toBe('bold');
      expect(node.content).toHaveLength(1);
      const textNode = node.content![0];
      if (textNode.type === 'text') {
        expect((textNode as TextNode).text).toBe('bold text');
      }
    });
  });

  describe('ColorNode', () => {
    it('should create a color node with hex color', () => {
      const child = createTextNode('colored');
      const node = createColorNode('#FF0000', [child]);
      expect(node.type).toBe('color');
      expect(node.color).toBe('#FF0000');
      expect(node.content).toHaveLength(1);
    });

    it('should normalize color hex to uppercase', () => {
      const child = createTextNode('text');
      const node = createColorNode('#ff00ff', [child]);
      expect(node.color).toBe('#FF00FF');
    });
  });

  describe('LinkNode', () => {
    it('should create an internal link node', () => {
      const child = createTextNode('Device Link');
      const node: LinkNode = {
        type: 'link',
        linkType: 'internal',
        target: 'ThingDevice',
        content: [child],
      };
      expect(node.linkType).toBe('internal');
      expect(node.target).toBe('ThingDevice');
    });

    it('should create an external link node', () => {
      const child = createTextNode('External');
      const node: LinkNode = {
        type: 'link',
        linkType: 'external',
        url: 'https://example.com',
        content: [child],
      };
      expect(node.linkType).toBe('external');
      expect(node.url).toBe('https://example.com');
    });
  });

  describe('CodeBlockNode', () => {
    it('should create a code block node', () => {
      const node: CodeBlockNode = {
        type: 'codeBlock',
        language: 'mips',
        code: 'mov r0 r1\nadd r2 r3',
      };
      expect(node.type).toBe('codeBlock');
      expect(node.code).toContain('mov r0 r1');
    });
  });

  describe('ListNodes', () => {
    it('should create a bullet list node', () => {
      const item1 = createTextNode('Item 1');
      const item2 = createTextNode('Item 2');
      const node: BulletListNode = {
        type: 'bulletList',
        items: [[item1], [item2]],
      };
      expect(node.type).toBe('bulletList');
      expect(node.items).toHaveLength(2);
    });

    it('should create a numbered list node', () => {
      const item1 = createTextNode('First');
      const node: NumberedListNode = {
        type: 'numberedList',
        items: [[item1]],
      };
      expect(node.type).toBe('numberedList');
      expect(node.items).toHaveLength(1);
    });
  });

  describe('MediaNodes', () => {
    it('should create an image node', () => {
      const node: ImageNode = {
        type: 'image',
        src: 'path/to/image.png',
        alt: 'Description',
      };
      expect(node.type).toBe('image');
      expect(node.src).toBe('path/to/image.png');
    });

    it('should create a YouTube link node', () => {
      const node: YouTubeLinkNode = {
        type: 'youtubeLink',
        videoId: 'dQw4w9WgXcQ',
        label: 'Watch Video',
      };
      expect(node.type).toBe('youtubeLink');
      expect(node.videoId).toBe('dQw4w9WgXcQ');
    });
  });

  describe('ParagraphNode', () => {
    it('should create a paragraph node with mixed content', () => {
      const bold = createBoldNode([createTextNode('bold')]);
      const text = createTextNode(' and regular');
      const node: ParagraphNode = {
        type: 'paragraph',
        content: [bold, text],
      };
      expect(node.type).toBe('paragraph');
      expect(node.content).toHaveLength(2);
    });
  });
});
