/**
 * Serializer: AST → TMP (TextMesh Pro) string
 * Converts AST back to TMP format for functional equivalence
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
  BulletListNode,
  NumberedListNode,
  ImageNode,
  YouTubeLinkNode,
  InternalLinkNode,
  ExternalLinkNode,
} from '../models/ast';

/**
 * Serialize AST nodes back to TMP string
 * @param nodes AST nodes to serialize
 * @returns TMP formatted string
 */
export function serializeToTMP(nodes: ASTNode[]): string {
  return nodes.map(node => serializeNode(node)).join('');
}

/**
 * Serialize a single AST node
 */
function serializeNode(node: ASTNode): string {
  switch (node.type) {
    case 'text':
      return serializeTextNode(node as TextNode);

    case 'bold':
      return serializeBoldNode(node as BoldNode);

    case 'italic':
      return serializeItalicNode(node as ItalicNode);

    case 'underline':
      return serializeUnderlineNode(node as UnderlineNode);

    case 'strikethrough':
      return serializeStrikethroughNode(node as StrikethroughNode);

    case 'color':
      return serializeColorNode(node as ColorNode);

    case 'link':
      return serializeLinkNode(node as LinkNode);

    case 'codeBlock':
      return serializeCodeBlockNode(node as CodeBlockNode);

    case 'bulletList':
      return serializeBulletListNode(node as BulletListNode);

    case 'numberedList':
      return serializeNumberedListNode(node as NumberedListNode);

    case 'image':
      return serializeImageNode(node as ImageNode);

    case 'youtubeLink':
      return serializeYouTubeLinkNode(node as YouTubeLinkNode);

    case 'paragraph':
      // Paragraphs are containers - serialize their content
      return serializeToTMP(node.content || []);

    default:
      return '';
  }
}

/**
 * Serialize text node
 */
function serializeTextNode(node: TextNode): string {
  return node.text;
}

/**
 * Serialize bold node
 */
function serializeBoldNode(node: BoldNode): string {
  const content = serializeToTMP(node.content || []);
  return `<b>${content}</b>`;
}

/**
 * Serialize italic node
 */
function serializeItalicNode(node: ItalicNode): string {
  const content = serializeToTMP(node.content || []);
  return `<i>${content}</i>`;
}

/**
 * Serialize underline node
 */
function serializeUnderlineNode(node: UnderlineNode): string {
  const content = serializeToTMP(node.content || []);
  return `<u>${content}</u>`;
}

/**
 * Serialize strikethrough node
 */
function serializeStrikethroughNode(node: StrikethroughNode): string {
  const content = serializeToTMP(node.content || []);
  return `<s>${content}</s>`;
}

/**
 * Serialize color node
 */
function serializeColorNode(node: ColorNode): string {
  const content = serializeToTMP(node.content || []);
  return `<color=${node.color}>${content}</color>`;
}

/**
 * Serialize link node
 */
function serializeLinkNode(node: LinkNode): string {
  const linkNode = node as LinkNode;

  if (linkNode.linkType === 'internal') {
    const internalLink = linkNode as InternalLinkNode;
    const content = serializeToTMP(internalLink.content || []);

    // Use originalFormat if available to preserve the original tag type
    if (internalLink.originalFormat === 'thing') {
      // Serialize as {THING:target} or {THING:target;displayText}
      const displayText = extractTextFromNodes(internalLink.content || []);
      if (displayText && displayText !== internalLink.target) {
        return `{THING:${internalLink.target};${displayText}}`;
      }
      return `{THING:${internalLink.target}}`;
    } else if (internalLink.originalFormat === 'link') {
      // Serialize as {LINK:target;displayText}
      // Extract display text from content
      const displayText = extractTextFromNodes(internalLink.content || []);
      return `{LINK:${internalLink.target};${displayText}}`;
    } else if (internalLink.originalFormat === 'html' || internalLink.tocId) {
      // Serialize as <link=tocId>text</link>
      if (internalLink.tocId) {
        return `<link=${internalLink.tocId}>${content}</link>`;
      }
    }

    // Fallback: if target looks like a device key, use THING format
    if (internalLink.target && internalLink.target.startsWith('Thing')) {
      const displayText = extractTextFromNodes(internalLink.content || []);
      if (displayText && displayText !== internalLink.target) {
        return `{THING:${internalLink.target};${displayText}}`;
      }
      return `{THING:${internalLink.target}}`;
    }

    // Last resort: return content
    return content;
  } else {
    const externalLink = linkNode as ExternalLinkNode;
    const content = serializeToTMP(externalLink.content || []);
    return `<link="${externalLink.url}">${content}</link>`;
  }
}

/**
 * Extract text content from AST nodes
 */
function extractTextFromNodes(nodes: ASTNode[]): string {
  let text = '';
  for (const node of nodes) {
    if (node.type === 'text') {
      text += (node as any).text;
    } else if (node.content) {
      text += extractTextFromNodes(node.content);
    }
  }
  return text;
}

/**
 * Serialize code block node
 */
function serializeCodeBlockNode(node: CodeBlockNode): string {
  return `<mspace=0.5em>${node.code}</mspace>`;
}

/**
 * Serialize bullet list node
 */
function serializeBulletListNode(node: BulletListNode): string {
  return node.items
    .map(item => {
      const content = serializeToTMP(item);
      return `• ${content}`;
    })
    .join('\n');
}

/**
 * Serialize numbered list node
 */
function serializeNumberedListNode(node: NumberedListNode): string {
  return node.items
    .map((item, index) => {
      const content = serializeToTMP(item);
      return `${index + 1}. ${content}`;
    })
    .join('\n');
}

/**
 * Serialize image node
 */
function serializeImageNode(node: ImageNode): string {
  // Images might be referenced by filename in descriptions
  // Preserve as inline text reference for now
  return `[Image: ${node.src}]`;
}

/**
 * Serialize YouTube link node
 */
function serializeYouTubeLinkNode(node: YouTubeLinkNode): string {
  return `[YouTube: ${node.label || node.videoId}]`;
}
