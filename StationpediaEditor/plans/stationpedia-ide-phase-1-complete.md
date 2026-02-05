## Phase 1 Complete: Content AST & Lossless Serialization

The foundational AST layer for TMP (TextMesh Pro) rich text has been implemented with full lossless round-tripping. All parsing, serialization, and JSON codec functionality is working correctly with comprehensive test coverage.

**Files created/changed:**
- src/renderer/models/ast.ts
- src/renderer/models/contentModel.ts
- src/renderer/services/parser.ts
- src/renderer/services/serializer.ts
- src/renderer/services/jsonCodec.ts
- src/renderer/models/ast.test.ts
- src/renderer/services/parser.test.ts
- src/renderer/services/serializer.test.ts
- src/renderer/services/jsonCodec.test.ts
- src/renderer/services/integration.test.ts

**Functions created/changed:**
- AST Node Types: TextNode, ParagraphNode, BoldNode, ItalicNode, UnderlineNode, StrikethroughNode, ColorNode, InternalLinkNode, ExternalLinkNode, CodeBlockNode, BulletListNode, NumberedListNode, ImageNode, YouTubeLinkNode
- Factory Functions: createTextNode, createBoldNode, createItalicNode, createColorNode, createLinkNode, createBulletListNode, createNumberedListNode, createImageNode, createYouTubeLinkNode
- Parser: parseToAST, parseNodes, parseTag, parseSpecialTag, extractTagName, extractTagAttributes
- Serializer: serializeToTMP, serializeNode (with originalFormat preservation)
- JSON Codec: parseWorkspaceJSON, serializeWorkspaceJSON, parseDeviceDocument, serializeDeviceDocument
- Utilities: normalizeHex, isLeafNode, extractText, normalizeDeviceFields, denormalizeDeviceFields

**Tests created/changed:**
- ast.test.ts: 13 tests for AST node creation and utilities
- parser.test.ts: 20 tests for TMP string parsing
- serializer.test.ts: 22 tests for AST to TMP serialization
- jsonCodec.test.ts: 16 tests for JSON round-trip codec
- integration.test.ts: 12 tests with real descriptions.json excerpts

**Review Status:** APPROVED

**Key Implementation Details:**
1. **13 AST Node Types**: Full coverage of TMP formatting including bold, italic, underline, strikethrough, color, links (internal/external), code blocks, lists, images, and YouTube embeds
2. **Original Format Preservation**: InternalLinkNode stores `originalFormat?: 'thing' | 'link' | 'html'` to preserve serialization format ({THING:}, {LINK:}, or <link>)
3. **Field Name Normalization**: JSON codec handles `OperationalDetails` ↔ `operationalDetails` conversion automatically
4. **Unknown Field Preservation**: All unknown JSON fields are preserved through round-trips via index signatures
5. **Color Normalization**: Colors normalized to uppercase hex (#FF0000) internally

**Git Commit Message:**
```
feat: Add Content AST & lossless serialization layer

- Add 13 AST node types for TMP rich text representation
- Implement TMP string parser with tag support (<b>, <i>, <u>, <s>, <color>, <link>, <mspace>, {THING:}, {LINK:})
- Implement AST serializer with originalFormat preservation for links
- Add JSON codec with field name normalization and unknown field preservation
- Add comprehensive test suite (83 tests passing)
```
