/**
 * Parser: TMP (TextMesh Pro) string → AST
 * Handles:
 * - TextMesh Pro tags: <b>, <i>, <u>, <s>, <color=#HEX>, <link>, <mspace>
 * - Special Stationpedia tags: {THING:Key}, {LINK:Page;Text}
 * - Nested tags and special characters
 */

import {
  ASTNode,
  TextNode,
  BoldNode,
  ItalicNode,
  UnderlineNode,
  StrikethroughNode,
  ColorNode,
  LinkNode,
  CodeBlockNode,
  createTextNode,
  createBoldNode,
  createItalicNode,
  createUnderlineNode,
  createStrikethroughNode,
  createColorNode,
  createInternalLinkNode,
  createExternalLinkNode,
  createCodeBlockNode,
  normalizeHex,
} from '../models/ast';

interface ParserState {
  input: string;
  position: number;
}

/**
 * Parse TMP string to AST
 * @param input TMP formatted string
 * @returns Array of AST nodes
 */
export function parseToAST(input: string): ASTNode[] {
  if (!input || !input.trim()) {
    return [];
  }

  const state: ParserState = {
    input,
    position: 0,
  };

  return parseNodes(state);
}

/**
 * Parse a sequence of nodes
 */
function parseNodes(state: ParserState, endTag?: string): ASTNode[] {
  const nodes: ASTNode[] = [];
  let textBuffer = '';

  while (state.position < state.input.length) {
    const char = state.input[state.position];

    // Handle special tag start markers
    if (char === '<') {
      // Flush text buffer
      if (textBuffer) {
        nodes.push(createTextNode(textBuffer));
        textBuffer = '';
      }

      // Try to parse a tag
      const tag = parseTag(state);
      if (tag === null) {
        // Not a valid tag, treat as text
        textBuffer += char;
        state.position++;
      } else {
        // Check if this is the end tag we're looking for
        if (endTag && tag.type === 'closeTag' && tag.name === endTag) {
          // Stop parsing this level
          return nodes;
        }

        if (tag.type === 'openTag' && tag.node) {
          nodes.push(tag.node);
        } else if (tag.type === 'selfClosing' && tag.node) {
          nodes.push(tag.node);
        }
        // closeTag handled above
      }
    } else if (char === '{') {
      // Flush text buffer
      if (textBuffer) {
        nodes.push(createTextNode(textBuffer));
        textBuffer = '';
      }

      // Try to parse special tag
      const specialTag = parseSpecialTag(state);
      if (specialTag) {
        nodes.push(specialTag);
      } else {
        // Not a valid special tag, treat as text
        textBuffer += char;
        state.position++;
      }
    } else if (char === '\n') {
      // Handle newlines
      textBuffer += char;
      state.position++;
    } else {
      textBuffer += char;
      state.position++;
    }
  }

  // Flush remaining text
  if (textBuffer) {
    nodes.push(createTextNode(textBuffer));
  }

  return nodes;
}

interface TagResult {
  type: 'openTag' | 'closeTag' | 'selfClosing';
  name: string;
  node?: ASTNode;
  attrs?: Record<string, string>;
}

/**
 * Parse a TMP tag like <b>, </b>, <color=#FF0000>, etc.
 */
function parseTag(state: ParserState): TagResult | null {
  if (state.input[state.position] !== '<') {
    return null;
  }

  const startPos = state.position;
  state.position++; // Skip <

  // Check for close tag
  if (state.input[state.position] === '/') {
    state.position++; // Skip /
    const name = parseTagName(state);
    if (state.input[state.position] === '>') {
      state.position++; // Skip >
      return {
        type: 'closeTag',
        name,
      };
    }
    // Invalid close tag, rewind
    state.position = startPos;
    return null;
  }

  // Parse tag name and attributes
  const name = parseTagName(state);
  const attrs: Record<string, string> = {};

  // Parse attributes (e.g., color=#FF0000)
  while (state.input[state.position] !== '>' && state.position < state.input.length) {
    if (state.input[state.position] === '=') {
      state.position++; // Skip =
      const value = parseTagValue(state);
      attrs[name] = value; // Store attribute value (e.g., #FF0000 for color)
      break;
    } else if (state.input[state.position] === ' ') {
      state.position++; // Skip whitespace
    } else {
      break;
    }
  }

  if (state.input[state.position] === '>') {
    state.position++; // Skip >

    // Create node based on tag name
    let node: ASTNode | null = null;

    switch (name) {
      case 'b':
        node = parseContainerTag(state, 'b', createBoldNode);
        break;
      case 'i':
        node = parseContainerTag(state, 'i', createItalicNode);
        break;
      case 'u':
        node = parseContainerTag(state, 'u', createUnderlineNode);
        break;
      case 's':
        node = parseContainerTag(state, 's', createStrikethroughNode);
        break;
      case 'color':
        const color = attrs[name] || '#FFFFFF';
        node = createColorNode(color, parseNodes(state, 'color'));
        break;
      case 'link':
        // <link=tocId>text</link>
        const tocId = attrs[name];
        const linkContent = parseNodes(state, 'link');
        node = createInternalLinkNode('', linkContent, tocId, 'html');
        break;
      case 'mspace':
        // <mspace=0.5em>code</mspace>
        const code = parseRawContent(state, 'mspace');
        node = createCodeBlockNode(code, 'mips');
        break;
      default:
        // Unknown tag, treat as text
        state.position = startPos;
        return null;
    }

    if (node) {
      return {
        type: 'openTag',
        name,
        node,
      };
    }
  }

  // Invalid tag, rewind
  state.position = startPos;
  return null;
}

/**
 * Parse tag name
 */
function parseTagName(state: ParserState): string {
  let name = '';
  while (
    state.position < state.input.length &&
    /[a-zA-Z0-9]/.test(state.input[state.position])
  ) {
    name += state.input[state.position];
    state.position++;
  }
  return name;
}

/**
 * Parse tag attribute value (e.g., #FF0000)
 */
function parseTagValue(state: ParserState): string {
  let value = '';
  while (
    state.position < state.input.length &&
    state.input[state.position] !== '>' &&
    state.input[state.position] !== ' '
  ) {
    value += state.input[state.position];
    state.position++;
  }
  return value;
}

/**
 * Parse container tag with content
 */
function parseContainerTag(
  state: ParserState,
  tagName: string,
  factory: (content: ASTNode[]) => ASTNode
): ASTNode {
  const content = parseNodes(state, tagName);
  return factory(content);
}

/**
 * Parse raw content until end tag (for code blocks)
 */
function parseRawContent(state: ParserState, tagName: string): string {
  let content = '';
  let depth = 1;

  while (state.position < state.input.length && depth > 0) {
    // Look for end tag
    if (
      state.input[state.position] === '<' &&
      state.input[state.position + 1] === '/' &&
      state.input.substring(state.position + 2).startsWith(tagName + '>')
    ) {
      depth--;
      if (depth === 0) {
        // Found closing tag
        state.position += tagName.length + 3; // Skip </tagName>
        break;
      }
    }

    content += state.input[state.position];
    state.position++;
  }

  return content;
}

/**
 * Parse special Stationpedia tags: {THING:Key}, {LINK:Page;Text}
 */
function parseSpecialTag(state: ParserState): ASTNode | null {
  if (state.input[state.position] !== '{') {
    return null;
  }

  const startPos = state.position;
  state.position++; // Skip {

  // Parse tag type
  let tagType = '';
  while (
    state.position < state.input.length &&
    state.input[state.position] !== ':' &&
    state.input[state.position] !== '}'
  ) {
    tagType += state.input[state.position];
    state.position++;
  }

  if (state.input[state.position] !== ':') {
    // Invalid special tag
    state.position = startPos;
    return null;
  }

  state.position++; // Skip :

  // Parse tag content
  let content = '';
  while (state.position < state.input.length && state.input[state.position] !== '}') {
    content += state.input[state.position];
    state.position++;
  }

  if (state.input[state.position] !== '}') {
    // Invalid special tag
    state.position = startPos;
    return null;
  }

  state.position++; // Skip }

  // Handle tag types
  if (tagType === 'THING') {
    // {THING:DeviceKey} or {THING:DeviceKey;DisplayText}
    const parts = content.split(';');
    const target = parts[0];
    const displayText = parts[1] || target;
    const textNode = createTextNode(displayText);
    return createInternalLinkNode(target, [textNode], undefined, 'thing');
  } else if (tagType === 'LINK') {
    // {LINK:PageKey;DisplayText}
    const parts = content.split(';');
    const pageKey = parts[0];
    const displayText = parts[1] || pageKey;
    const textNode = createTextNode(displayText);
    return createInternalLinkNode(pageKey, [textNode], undefined, 'link');
  } else if (tagType === 'HEADER') {
    // {HEADER:HeaderText} - renders as bold orange text
    const headerText = content;
    const textNode = createTextNode(headerText);
    // Create bold node wrapped in orange color
    const boldNode = createBoldNode([textNode]);
    return createColorNode('#FF7A18', [boldNode]);
  }

  // Unknown special tag
  state.position = startPos;
  return null;
}
