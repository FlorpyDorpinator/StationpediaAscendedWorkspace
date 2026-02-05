/**
 * RichTextRenderer - Renders AST nodes to React elements
 * Supports all AST node types: bold, italic, underline, strikethrough, color, links, lists, images, code blocks, YouTube embeds
 */

import React, { useMemo } from 'react';
import type {
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
  ParagraphNode,
} from '@models/ast';
import { parseToAST } from '@services/parser';

interface RichTextRendererProps {
  content?: string; // TMP format string to parse
  ast?: ASTNode[]; // Pre-parsed AST (alternative to content)
  className?: string;
  onLinkClick?: (target: string) => void;
}

/**
 * Render a single AST node to a React element
 */
function renderNode(
  node: ASTNode,
  index: number,
  onLinkClick?: (target: string) => void
): React.ReactNode {
  switch (node.type) {
    case 'text': {
      const textNode = node as TextNode;
      return <span key={index}>{textNode.text}</span>;
    }

    case 'paragraph': {
      const paraNode = node as ParagraphNode;
      return (
        <p key={index} className="mb-2">
          {paraNode.content?.map((child, i) => renderNode(child, i, onLinkClick))}
        </p>
      );
    }

    case 'bold': {
      const boldNode = node as BoldNode;
      return (
        <strong key={index}>
          {boldNode.content?.map((child, i) => renderNode(child, i, onLinkClick))}
        </strong>
      );
    }

    case 'italic': {
      const italicNode = node as ItalicNode;
      return (
        <em key={index}>
          {italicNode.content?.map((child, i) => renderNode(child, i, onLinkClick))}
        </em>
      );
    }

    case 'underline': {
      const underlineNode = node as UnderlineNode;
      return (
        <u key={index}>
          {underlineNode.content?.map((child, i) => renderNode(child, i, onLinkClick))}
        </u>
      );
    }

    case 'strikethrough': {
      const strikeNode = node as StrikethroughNode;
      return (
        <s key={index}>
          {strikeNode.content?.map((child, i) => renderNode(child, i, onLinkClick))}
        </s>
      );
    }

    case 'color': {
      const colorNode = node as ColorNode;
      return (
        <span key={index} style={{ color: colorNode.color }}>
          {colorNode.content?.map((child, i) => renderNode(child, i, onLinkClick))}
        </span>
      );
    }

    case 'link': {
      const linkNode = node as LinkNode;
      if (linkNode.linkType === 'internal') {
        const internalLink = linkNode as any;
        return (
          <a
            key={index}
            href={`#${internalLink.tocId || internalLink.target}`}
            className="text-[#008AE6] underline hover:text-[#00A8FF] cursor-pointer"
            onClick={(e) => {
              e.preventDefault();
              if (onLinkClick) {
                onLinkClick(internalLink.target);
              }
            }}
          >
            {linkNode.content?.map((child, i) => renderNode(child, i, onLinkClick))}
          </a>
        );
      } else {
        const externalLink = linkNode as any;
        return (
          <a
            key={index}
            href={externalLink.url}
            target="_blank"
            rel="noopener noreferrer"
            className="text-[#008AE6] underline hover:text-[#00A8FF]"
          >
            {linkNode.content?.map((child, i) => renderNode(child, i, onLinkClick))}
          </a>
        );
      }
    }

    case 'codeBlock': {
      const codeNode = node as CodeBlockNode;
      return (
        <pre key={index} className="bg-black/50 border border-gray-700 rounded p-3 mb-2 overflow-x-auto">
          <code className="text-sm font-mono text-gray-300">{codeNode.code}</code>
        </pre>
      );
    }

    case 'bulletList': {
      const listNode = node as BulletListNode;
      return (
        <ul key={index} className="list-disc list-inside mb-2 ml-2">
          {listNode.items?.map((item, i) => (
            <li key={i} className="text-sm">
              {item.map((child, j) => renderNode(child, j, onLinkClick))}
            </li>
          ))}
        </ul>
      );
    }

    case 'numberedList': {
      const listNode = node as NumberedListNode;
      return (
        <ol key={index} className="list-decimal list-inside mb-2 ml-2">
          {listNode.items?.map((item, i) => (
            <li key={i} className="text-sm">
              {item.map((child, j) => renderNode(child, j, onLinkClick))}
            </li>
          ))}
        </ol>
      );
    }

    case 'image': {
      const imgNode = node as ImageNode;
      return (
        <img
          key={index}
          src={imgNode.src}
          alt={imgNode.alt || 'Image'}
          width={imgNode.width || 'auto'}
          height={imgNode.height || 'auto'}
          className="max-w-full h-auto rounded mb-2"
        />
      );
    }

    case 'youtubeLink': {
      const youtubeNode = node as YouTubeLinkNode;
      return (
        <div key={index} className="mb-2">
          <iframe
            width="100%"
            height="315"
            src={`https://www.youtube.com/embed/${youtubeNode.videoId}`}
            title={youtubeNode.label || 'YouTube Video'}
            frameBorder="0"
            allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
            allowFullScreen
            className="rounded"
          />
        </div>
      );
    }

    default:
      return null;
  }
}

/**
 * Main RichTextRenderer component
 */
export const RichTextRenderer: React.FC<RichTextRendererProps> = ({
  content,
  ast,
  className = '',
  onLinkClick,
}) => {
  const nodes = useMemo(() => {
    if (ast) {
      return ast;
    }
    if (content) {
      return parseToAST(content);
    }
    return [];
  }, [content, ast]);

  if (nodes.length === 0) {
    return null;
  }

  return (
    <div className={`rich-text-content ${className}`}>
      {nodes.map((node, i) => renderNode(node, i, onLinkClick))}
    </div>
  );
};

export default RichTextRenderer;
