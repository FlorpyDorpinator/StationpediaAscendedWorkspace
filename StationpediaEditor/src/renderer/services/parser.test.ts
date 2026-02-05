/**
 * Tests for TMP string parser
 */
import { describe, it, expect } from 'vitest';
import { parseToAST } from './parser';
import { TextNode, BoldNode, ColorNode, LinkNode } from '@models/ast';

describe('Parser - TMP to AST', () => {
  describe('Basic text parsing', () => {
    it('should parse plain text', () => {
      const result = parseToAST('Hello world');
      expect(result).toHaveLength(1);
      const node = result[0] as TextNode;
      expect(node.type).toBe('text');
      expect(node.text).toBe('Hello world');
    });

    it('should parse text with newlines', () => {
      const result = parseToAST('Line 1\nLine 2');
      expect(result).toContainEqual(expect.objectContaining({ type: 'text' }));
    });
  });

  describe('Basic formatting tags', () => {
    it('should parse bold tags', () => {
      const result = parseToAST('<b>bold text</b>');
      expect(result).toHaveLength(1);
      const node = result[0] as BoldNode;
      expect(node.type).toBe('bold');
      expect(node.content).toHaveLength(1);
      const textNode = node.content![0] as TextNode;
      expect(textNode.text).toBe('bold text');
    });

    it('should parse italic tags', () => {
      const result = parseToAST('<i>italic</i>');
      expect(result).toHaveLength(1);
      expect(result[0].type).toBe('italic');
    });

    it('should parse underline tags', () => {
      const result = parseToAST('<u>underline</u>');
      expect(result).toHaveLength(1);
      expect(result[0].type).toBe('underline');
    });

    it('should parse strikethrough tags', () => {
      const result = parseToAST('<s>strikethrough</s>');
      expect(result).toHaveLength(1);
      expect(result[0].type).toBe('strikethrough');
    });
  });

  describe('Color tags', () => {
    it('should parse color with hex code', () => {
      const result = parseToAST('<color=#FF0000>red text</color>');
      expect(result).toHaveLength(1);
      const node = result[0] as ColorNode;
      expect(node.type).toBe('color');
      expect(node.color).toBe('#FF0000');
      const textNode = node.content![0] as TextNode;
      expect(textNode.text).toBe('red text');
    });

    it('should normalize color hex codes to uppercase', () => {
      const result = parseToAST('<color=#ff00ff>text</color>');
      const node = result[0] as ColorNode;
      expect(node.color).toBe('#FF00FF');
    });
  });

  describe('Nested tags', () => {
    it('should parse nested bold and italic', () => {
      const result = parseToAST('<b><i>bold italic</i></b>');
      expect(result).toHaveLength(1);
      const bold = result[0] as BoldNode;
      expect(bold.type).toBe('bold');
      expect(bold.content).toHaveLength(1);
      const italic = bold.content![0];
      expect(italic.type).toBe('italic');
    });

    it('should parse mixed nested formatting', () => {
      const result = parseToAST('<color=#0000FF><b>blue bold</b></color>');
      expect(result).toHaveLength(1);
      const color = result[0] as ColorNode;
      expect(color.type).toBe('color');
      expect(color.content![0].type).toBe('bold');
    });
  });

  describe('Special link tags', () => {
    it('should parse THING references', () => {
      const result = parseToAST('{THING:ThingDevice}');
      expect(result).toHaveLength(1);
      const node = result[0] as LinkNode;
      expect(node.type).toBe('link');
      if (node.linkType === 'internal') {
        expect(node.target).toBe('ThingDevice');
      }
    });

    it('should parse THING references with display text', () => {
      const result = parseToAST('{THING:StructureSolarPanel;Solar Panels}');
      expect(result).toHaveLength(1);
      const node = result[0] as LinkNode;
      expect(node.type).toBe('link');
      if (node.linkType === 'internal') {
        expect(node.target).toBe('StructureSolarPanel');
        expect(node.originalFormat).toBe('thing');
      }
    });

    it('should parse LINK references with display text', () => {
      const result = parseToAST('{LINK:PageKey;Display Text}');
      expect(result).toHaveLength(1);
      const node = result[0] as LinkNode;
      expect(node.type).toBe('link');
      if (node.linkType === 'internal') {
        expect(node.target).toBe('PageKey');
      }
    });
  });

  describe('Code blocks with mspace', () => {
    it('should parse mspace as code block', () => {
      const code = 'mov r0 r1\nadd r0 r2';
      const result = parseToAST(`<mspace=0.5em>${code}</mspace>`);
      expect(result).toHaveLength(1);
      const node = result[0];
      expect(node.type).toBe('codeBlock');
    });
  });

  describe('Complex scenarios', () => {
    it('should parse text with mixed formatting', () => {
      const text = 'Normal <b>bold</b> and <i>italic</i> text';
      const result = parseToAST(text);
      expect(result.length).toBeGreaterThan(1);
      expect(result.some(n => n.type === 'bold')).toBe(true);
      expect(result.some(n => n.type === 'italic')).toBe(true);
    });

    it('should handle unclosed tags gracefully', () => {
      const result = parseToAST('<b>unclosed bold');
      expect(result.length).toBeGreaterThan(0);
    });

    it('should preserve text around tags', () => {
      const text = 'Start <b>middle</b> end';
      const result = parseToAST(text);
      expect(result.some(n => (n as TextNode).text === 'Start ')).toBe(true);
      expect(result.some(n => n.type === 'bold')).toBe(true);
      expect(result.some(n => (n as TextNode).text === ' end')).toBe(true);
    });
  });

  describe('Edge cases', () => {
    it('should handle empty string', () => {
      const result = parseToAST('');
      expect(result).toBeDefined();
    });

    it('should handle only whitespace', () => {
      const result = parseToAST('   \n  ');
      expect(result).toBeDefined();
    });

    it('should preserve text with special characters', () => {
      const text = 'Text with ampersand & and quote " characters';
      const result = parseToAST(text);
      expect(result.length).toBeGreaterThanOrEqual(1);
      const content = result.map(n => (n as TextNode).text || '').join('');
      expect(content).toContain('&');
      expect(content).toContain('"');
    });
  });
});
