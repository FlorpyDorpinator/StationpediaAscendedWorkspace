/**
 * Tests for AST to TMP serializer
 */
import { describe, it, expect } from 'vitest';
import { serializeToTMP } from './serializer';
import { parseToAST } from './parser';
import { TextNode, BoldNode, ColorNode } from '@models/ast';

describe('Serializer - AST to TMP', () => {
  describe('Basic serialization', () => {
    it('should serialize text nodes', () => {
      const nodes: TextNode[] = [
        { type: 'text', text: 'Hello world' },
      ];
      const result = serializeToTMP(nodes);
      expect(result).toBe('Hello world');
    });

    it('should serialize bold nodes', () => {
      const nodes = [
        {
          type: 'bold' as const,
          content: [{ type: 'text' as const, text: 'bold text' } as TextNode],
        } as BoldNode,
      ];
      const result = serializeToTMP(nodes);
      expect(result).toContain('<b>');
      expect(result).toContain('bold text');
      expect(result).toContain('</b>');
    });
  });

  describe('Formatting tags', () => {
    it('should serialize italic tags', () => {
      const nodes = [
        {
          type: 'italic',
          content: [{ type: 'text', text: 'italic' }],
        },
      ] as any[];
      const result = serializeToTMP(nodes);
      expect(result).toContain('<i>');
      expect(result).toContain('</i>');
    });

    it('should serialize underline tags', () => {
      const nodes = [
        {
          type: 'underline',
          content: [{ type: 'text', text: 'underlined' }],
        },
      ] as any[];
      const result = serializeToTMP(nodes);
      expect(result).toContain('<u>');
      expect(result).toContain('</u>');
    });

    it('should serialize color tags', () => {
      const nodes = [
        {
          type: 'color' as const,
          color: '#FF0000',
          content: [{ type: 'text' as const, text: 'red' } as TextNode],
        } as ColorNode,
      ];
      const result = serializeToTMP(nodes);
      expect(result).toContain('<color=#FF0000>');
      expect(result).toContain('</color>');
    });
  });

  describe('Round-trip serialization', () => {
    it('should round-trip bold text', () => {
      const original = '<b>bold text</b>';
      const ast = parseToAST(original);
      const result = serializeToTMP(ast);
      // Functional equivalence - might not be byte-identical but renders same
      const reparse = parseToAST(result);
      expect(reparse).toHaveLength(ast.length);
      expect(reparse[0].type).toBe('bold');
    });

    it('should round-trip colored text', () => {
      const original = '<color=#FF0000>red text</color>';
      const ast = parseToAST(original);
      const result = serializeToTMP(ast);
      const reparse = parseToAST(result);
      expect(reparse[0].type).toBe('color');
    });

    it('should round-trip nested formatting', () => {
      const original = '<color=#0000FF><b>blue bold</b></color>';
      const ast = parseToAST(original);
      const result = serializeToTMP(ast);
      const reparse = parseToAST(result);
      expect(reparse).toHaveLength(1);
    });

    it('should preserve {THING:} tags through round-trip', () => {
      const original = '{THING:ThingDevice}';
      const ast = parseToAST(original);
      const result = serializeToTMP(ast);
      expect(result).toBe('{THING:ThingDevice}');
    });

    it('should preserve {THING:;} with display text through round-trip', () => {
      const original = '{THING:StructureSolarPanel;Solar Panels}';
      const ast = parseToAST(original);
      const result = serializeToTMP(ast);
      expect(result).toBe('{THING:StructureSolarPanel;Solar Panels}');
    });

    it('should preserve {LINK:;} tags through round-trip', () => {
      const original = '{LINK:PageKey;Display Text}';
      const ast = parseToAST(original);
      const result = serializeToTMP(ast);
      expect(result).toBe('{LINK:PageKey;Display Text}');
    });

    it('should preserve <link> tags through round-trip', () => {
      const original = '<link=section-id>Link Text</link>';
      const ast = parseToAST(original);
      const result = serializeToTMP(ast);
      expect(result).toContain('<link=section-id>');
      expect(result).toContain('Link Text');
      expect(result).toContain('</link>');
    });
  });

  describe('Lists', () => {
    it('should serialize bullet lists', () => {
      const nodes = [
        {
          type: 'bulletList',
          items: [
            [{ type: 'text', text: 'Item 1' }],
            [{ type: 'text', text: 'Item 2' }],
          ],
        },
      ] as any[];
      const result = serializeToTMP(nodes);
      expect(result).toContain('Item 1');
      expect(result).toContain('Item 2');
    });

    it('should serialize numbered lists', () => {
      const nodes = [
        {
          type: 'numberedList',
          items: [
            [{ type: 'text', text: 'First' }],
            [{ type: 'text', text: 'Second' }],
          ],
        },
      ] as any[];
      const result = serializeToTMP(nodes);
      expect(result).toContain('First');
      expect(result).toContain('Second');
    });
  });

  describe('Code blocks', () => {
    it('should serialize code blocks', () => {
      const nodes = [
        {
          type: 'codeBlock',
          language: 'mips',
          code: 'mov r0 r1\nadd r0 r2',
        },
      ] as any[];
      const result = serializeToTMP(nodes);
      expect(result).toContain('<mspace=0.5em>');
      expect(result).toContain('mov r0 r1');
    });
  });

  describe('Links', () => {
    it('should serialize THING links with originalFormat', () => {
      const nodes = [
        {
          type: 'link',
          linkType: 'internal',
          target: 'ThingDevice',
          content: [{ type: 'text', text: 'Device' }],
          originalFormat: 'thing' as const,
        },
      ] as any[];
      const result = serializeToTMP(nodes);
      expect(result).toBe('{THING:ThingDevice;Device}');
    });

    it('should serialize THING links without display text when same as target', () => {
      const nodes = [
        {
          type: 'link',
          linkType: 'internal',
          target: 'ThingDevice',
          content: [{ type: 'text', text: 'ThingDevice' }],
          originalFormat: 'thing' as const,
        },
      ] as any[];
      const result = serializeToTMP(nodes);
      expect(result).toBe('{THING:ThingDevice}');
    });

    it('should serialize LINK tags with display text', () => {
      const nodes = [
        {
          type: 'link',
          linkType: 'internal',
          target: 'PageKey',
          content: [{ type: 'text', text: 'Custom Display' }],
          originalFormat: 'link' as const,
        },
      ] as any[];
      const result = serializeToTMP(nodes);
      expect(result).toBe('{LINK:PageKey;Custom Display}');
    });

    it('should serialize HTML links with tocId', () => {
      const nodes = [
        {
          type: 'link',
          linkType: 'internal',
          target: '',
          tocId: 'my-section',
          content: [{ type: 'text', text: 'Link Text' }],
          originalFormat: 'html' as const,
        },
      ] as any[];
      const result = serializeToTMP(nodes);
      expect(result).toContain('<link=my-section>');
      expect(result).toContain('Link Text');
      expect(result).toContain('</link>');
    });

    it('should fallback to THING format when target starts with Thing', () => {
      const nodes = [
        {
          type: 'link',
          linkType: 'internal',
          target: 'ThingDevice',
          content: [{ type: 'text', text: 'Device' }],
          // No originalFormat specified
        },
      ] as any[];
      const result = serializeToTMP(nodes);
      expect(result).toBe('{THING:ThingDevice;Device}');
    });
  });

  describe('Media', () => {
    it('should serialize image nodes', () => {
      const nodes = [
        {
          type: 'image',
          src: 'path/to/image.png',
          alt: 'Test image',
        },
      ] as any[];
      const result = serializeToTMP(nodes);
      expect(result).toBeDefined();
    });

    it('should serialize YouTube links', () => {
      const nodes = [
        {
          type: 'youtubeLink',
          videoId: 'dQw4w9WgXcQ',
          label: 'Watch Video',
        },
      ] as any[];
      const result = serializeToTMP(nodes);
      expect(result).toBeDefined();
    });
  });
});
