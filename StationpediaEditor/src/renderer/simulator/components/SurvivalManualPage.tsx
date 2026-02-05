/**
 * SurvivalManualPage component
 * Displays the Stationeers Survival Manual guide content
 * Loaded from the markdown file in the mod's Guides folder
 */
import React, { useEffect, useState } from 'react';

// Path to the survival manual markdown file
const SURVIVAL_MANUAL_PATH = 'c:/Dev/12-17-25 Stationeers Respawn Update Code/StationpediaAscended/mod/Guides/Stationeers Survival Manual.md';

export interface SurvivalManualPageProps {
  onBack?: () => void;
  onNavigate?: (deviceKey: string) => void;
}

// Collapsible section component
const CollapsibleSection: React.FC<{
  title: string;
  children: React.ReactNode;
  defaultExpanded?: boolean;
  level?: number;
}> = ({ title, children, defaultExpanded = true, level = 1 }) => {
  const [isExpanded, setIsExpanded] = useState(defaultExpanded);

  const bgColors = ['#0A1520', '#0F1A25', '#141F2A'];
  const bgColor = bgColors[Math.min(level - 1, bgColors.length - 1)];
  const titleColors = ['#FF7A18', '#00AAFF', '#AAAAAA'];
  const titleColor = titleColors[Math.min(level - 1, titleColors.length - 1)];

  return (
    <div className="mb-2 rounded overflow-hidden" style={{ backgroundColor: bgColor }}>
      <button
        onClick={() => setIsExpanded(!isExpanded)}
        className="w-full flex items-center justify-between px-3 py-2 text-left font-medium"
        style={{ color: titleColor, backgroundColor: 'rgba(255,255,255,0.05)' }}
      >
        <span>{title}</span>
        <span className="text-sm">{isExpanded ? '▼' : '▶'}</span>
      </button>
      {isExpanded && (
        <div className="px-3 py-2 text-sm text-[#E6EDF3]">
          {children}
        </div>
      )}
    </div>
  );
};

// Parse markdown content into sections
interface ParsedSection {
  title: string;
  level: number;
  content: string;
  children: ParsedSection[];
}

function parseMarkdownToSections(markdown: string): ParsedSection[] {
  const lines = markdown.split('\n');
  const sections: ParsedSection[] = [];
  const stack: ParsedSection[] = [];
  let currentContent: string[] = [];

  for (const line of lines) {
    const h1Match = line.match(/^# (.+)$/);
    const h2Match = line.match(/^## (.+)$/);
    const h3Match = line.match(/^### (.+)$/);

    if (h1Match || h2Match || h3Match) {
      // Save current content to the last section in stack
      if (stack.length > 0 && currentContent.length > 0) {
        stack[stack.length - 1].content = currentContent.join('\n').trim();
        currentContent = [];
      }

      const level = h1Match ? 1 : h2Match ? 2 : 3;
      const title = (h1Match || h2Match || h3Match)?.[1] || '';

      const newSection: ParsedSection = {
        title,
        level,
        content: '',
        children: [],
      };

      // Pop stack until we find a parent with lower level
      while (stack.length > 0 && stack[stack.length - 1].level >= level) {
        stack.pop();
      }

      if (stack.length === 0) {
        sections.push(newSection);
      } else {
        stack[stack.length - 1].children.push(newSection);
      }

      stack.push(newSection);
    } else {
      currentContent.push(line);
    }
  }

  // Save any remaining content
  if (stack.length > 0 && currentContent.length > 0) {
    stack[stack.length - 1].content = currentContent.join('\n').trim();
  }

  return sections;
}

// Convert markdown content to JSX
function markdownToJsx(content: string): React.ReactNode {
  if (!content) return null;

  // Process line by line
  const lines = content.split('\n');
  const elements: React.ReactNode[] = [];

  for (let i = 0; i < lines.length; i++) {
    let line = lines[i];

    // Skip horizontal rules
    if (line.trim().startsWith('---')) continue;

    // Convert bold
    line = line.replace(/\*\*([^*]+)\*\*/g, '<b>$1</b>');
    // Convert italic
    line = line.replace(/(?<!\*)\*([^*]+)\*(?!\*)/g, '<i>$1</i>');
    // Convert inline code
    line = line.replace(/`([^`]+)`/g, '<code>$1</code>');
    // Convert bullet points
    if (line.trim().startsWith('- ')) {
      line = '• ' + line.trim().substring(2);
    }

    // Add paragraph spacing
    if (line.trim() === '') {
      elements.push(<div key={i} className="h-2" />);
    } else {
      elements.push(
        <p
          key={i}
          className="mb-1"
          dangerouslySetInnerHTML={{ __html: line }}
        />
      );
    }
  }

  return <>{elements}</>;
}

// Render a section recursively
const SectionRenderer: React.FC<{ section: ParsedSection; defaultExpanded?: boolean }> = ({
  section,
  defaultExpanded = false,
}) => {
  return (
    <CollapsibleSection
      title={section.title}
      defaultExpanded={defaultExpanded}
      level={section.level}
    >
      {section.content && (
        <div className="mb-2">{markdownToJsx(section.content)}</div>
      )}
      {section.children.map((child, index) => (
        <SectionRenderer key={index} section={child} />
      ))}
    </CollapsibleSection>
  );
};

export const SurvivalManualPage: React.FC<SurvivalManualPageProps> = ({
  onBack,
  onNavigate,
}) => {
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [sections, setSections] = useState<ParsedSection[]>([]);

  useEffect(() => {
    async function loadManual() {
      try {
        setIsLoading(true);
        
        if (!window.electronAPI?.readFile) {
          throw new Error('Electron API not available');
        }

        const result = await window.electronAPI.readFile(SURVIVAL_MANUAL_PATH);
        if (!result.success || !result.data) {
          throw new Error(result.error || 'Failed to read Survival Manual');
        }

        const parsed = parseMarkdownToSections(result.data);
        setSections(parsed);
        setError(null);
      } catch (err) {
        console.error('Failed to load Survival Manual:', err);
        setError(err instanceof Error ? err.message : 'Failed to load');
      } finally {
        setIsLoading(false);
      }
    }

    loadManual();
  }, []);

  return (
    <div className="flex flex-col h-full bg-[#1A1F24]">
      {/* Header */}
      <div className="px-4 py-3 border-b border-[#3A3F44] bg-[#1A1F24] flex items-center gap-3">
        {onBack && (
          <button
            onClick={onBack}
            className="text-[#FF7A18] hover:text-[#FF9A48] transition-colors"
          >
            ← Back
          </button>
        )}
        <h1 className="text-lg font-semibold text-[#FF7A18]">
          Stationeers Survival Manual
        </h1>
      </div>

      {/* Content */}
      <div className="flex-1 overflow-auto p-4">
        {isLoading ? (
          <div className="text-center text-[#8B949E] py-8">
            <p>Loading Survival Manual...</p>
          </div>
        ) : error ? (
          <div className="text-center text-red-400 py-8">
            <p>Error: {error}</p>
            <p className="text-sm mt-2 text-[#6E7681]">
              Make sure the file exists at:
            </p>
            <p className="text-xs text-[#8B949E] font-mono mt-1">
              {SURVIVAL_MANUAL_PATH}
            </p>
          </div>
        ) : (
          <div className="space-y-2">
            {/* Show number of sections found */}
            {sections.length === 0 ? (
              <div className="p-4 bg-[#0A1520] rounded text-yellow-400">
                <p>No sections found in the markdown file.</p>
                <p className="text-xs mt-2 text-[#8B949E]">
                  The file was loaded but no # headers were detected.
                </p>
              </div>
            ) : (
              <>
                <div className="p-3 bg-[#0A1520] rounded mb-4 text-sm text-[#8B949E]">
                  📖 {sections.length} sections loaded
                </div>
                {/* Sections */}
                {sections.map((section, index) => (
                  <SectionRenderer
                    key={index}
                    section={section}
                    defaultExpanded={true}
                  />
                ))}
              </>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default SurvivalManualPage;
