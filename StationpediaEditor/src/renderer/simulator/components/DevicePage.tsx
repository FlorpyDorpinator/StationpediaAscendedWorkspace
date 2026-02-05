/**
 * DevicePage component
 * Displays a full device page with all sections from Stationpedia.json
 * Plus mod overrides from descriptions.json (Operational Details, etc.)
 * Matches the actual game's stationpedia layout
 */
import React, { useState, useEffect, useMemo } from 'react';
import { resolveDevicePage, getDeviceOverride } from '../services/stationpediaService';
import { getThumbnailUrl, ResolvedDevicePage, OperationalDetail } from '../types/stationpediaTypes';

// Helper to safely render a value that might be an object
// Some Stationpedia fields like description can be {dataType, range, description}
function safeRender(value: unknown): string {
  if (value === null || value === undefined) return '-';
  if (typeof value === 'string') return value || '-';
  if (typeof value === 'number' || typeof value === 'boolean') return String(value);
  if (typeof value === 'object') {
    // If it has a description property, use that
    const obj = value as Record<string, unknown>;
    if ('description' in obj && typeof obj.description === 'string') {
      return obj.description || '-';
    }
    // Otherwise, try to stringify nicely
    try {
      return JSON.stringify(value);
    } catch {
      return '[Object]';
    }
  }
  return String(value);
}

export interface DevicePageProps {
  deviceKey: string;
  onBack?: () => void;
  onNavigate?: (deviceKey: string) => void;
}

// Collapsible section component
const CollapsibleSection: React.FC<{
  title: string;
  children: React.ReactNode;
  defaultExpanded?: boolean;
  titleColor?: string;
  backgroundColor?: string;
}> = ({ title, children, defaultExpanded = true, titleColor = '#FF7A18', backgroundColor = '#0A1520' }) => {
  const [isExpanded, setIsExpanded] = useState(defaultExpanded);

  return (
    <div className="mb-3 rounded overflow-hidden" style={{ backgroundColor }}>
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

// Operational Detail renderer (for mod content)
const OperationalDetailSection: React.FC<{
  detail: OperationalDetail;
  level?: number;
}> = ({ detail, level = 0 }) => {
  const [isExpanded, setIsExpanded] = useState(!detail.collapsible);
  const indent = level * 16;

  if (detail.collapsible) {
    return (
      <div className="mb-2" style={{ marginLeft: indent }}>
        <button
          onClick={() => setIsExpanded(!isExpanded)}
          className="flex items-center gap-2 text-[#FF7A18] hover:text-[#FFB800] transition-colors"
        >
          <span className="text-xs">{isExpanded ? '▼' : '▶'}</span>
          <span className="font-medium">{detail.title}</span>
        </button>
        {isExpanded && (
          <div className="mt-1 ml-4 text-sm text-[#E6EDF3]">
            {detail.description && <p className="mb-2 whitespace-pre-wrap">{detail.description}</p>}
            {detail.items && (
              <ul className="list-disc list-inside space-y-1 text-[#8B949E]">
                {detail.items.map((item, i) => <li key={i}>{item}</li>)}
              </ul>
            )}
            {detail.steps && (
              <ol className="list-decimal list-inside space-y-1 text-[#8B949E]">
                {detail.steps.map((step, i) => <li key={i}>{step}</li>)}
              </ol>
            )}
            {detail.children?.map((child, i) => (
              <OperationalDetailSection key={i} detail={child} level={level + 1} />
            ))}
          </div>
        )}
      </div>
    );
  }

  return (
    <div className="mb-3" style={{ marginLeft: indent }}>
      <h4 className="font-medium text-[#FF7A18] mb-1">{detail.title}</h4>
      {detail.description && <p className="text-sm text-[#E6EDF3] whitespace-pre-wrap">{detail.description}</p>}
      {detail.items && (
        <ul className="list-disc list-inside text-sm text-[#8B949E] mt-1">
          {detail.items.map((item, i) => <li key={i}>{item}</li>)}
        </ul>
      )}
      {detail.steps && (
        <ol className="list-decimal list-inside text-sm text-[#8B949E] mt-1">
          {detail.steps.map((step, i) => <li key={i}>{step}</li>)}
        </ol>
      )}
      {detail.children?.map((child, i) => (
        <OperationalDetailSection key={i} detail={child} level={level + 1} />
      ))}
    </div>
  );
};

export const DevicePage: React.FC<DevicePageProps> = ({
  deviceKey,
  onBack,
  onNavigate,
}) => {
  const [page, setPage] = useState<ResolvedDevicePage | null>(null);
  const [imageError, setImageError] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    setIsLoading(true);
    setImageError(false);
    const resolved = resolveDevicePage(deviceKey);
    setPage(resolved);
    setIsLoading(false);
  }, [deviceKey]);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-full bg-[#1A1F24]">
        <p className="text-[#8B949E]">Loading...</p>
      </div>
    );
  }

  if (!page) {
    return (
      <div className="flex flex-col h-full bg-[#1A1F24]">
        <div className="px-4 py-3 bg-[#2A2F34] border-b border-[#3A3F44]">
          {onBack && (
            <button onClick={onBack} className="text-[#8B949E] hover:text-[#E6EDF3]">
              ← Back
            </button>
          )}
        </div>
        <div className="flex items-center justify-center flex-1">
          <p className="text-[#8B949E]">Device not found: {deviceKey}</p>
        </div>
      </div>
    );
  }

  const thumbnailSrc = page.prefabName ? getThumbnailUrl(page.prefabName) : null;

  return (
    <div className="flex flex-col h-full bg-[#1A1F24]">
      {/* Header with back button */}
      <div className="px-4 py-3 bg-[#2A2F34] border-b border-[#3A3F44] flex items-center gap-3">
        {onBack && (
          <button onClick={onBack} className="text-[#8B949E] hover:text-[#E6EDF3]">
            ← Back
          </button>
        )}
        <h1 className="text-lg font-bold text-[#E6EDF3] flex-1">{page.title}</h1>
        {page.hasModOverrides && (
          <span className="text-xs px-2 py-1 bg-[#FF7A18] text-white rounded">
            Mod Enhanced
          </span>
        )}
      </div>

      {/* Scrollable content */}
      <div className="flex-1 overflow-y-auto">
        <div className="p-4">
          {/* Title and Image Row */}
          <div className="flex gap-4 mb-4">
            {/* Thumbnail */}
            <div className="w-32 h-32 flex items-center justify-center bg-[#2A2F34] rounded border border-[#3A3F44] flex-shrink-0">
              {thumbnailSrc && !imageError ? (
                <img
                  src={thumbnailSrc}
                  alt={page.title}
                  className="max-w-full max-h-full object-contain"
                  onError={() => setImageError(true)}
                />
              ) : (
                <span className="text-4xl text-[#6E7681]">📦</span>
              )}
            </div>

            {/* Basic Info */}
            <div className="flex-1">
              <h2 className="text-xl font-bold text-[#E6EDF3] mb-2">{page.title}</h2>
              <div className="text-xs text-[#8B949E] space-y-1">
                <p><span className="text-[#6E7681]">Prefab:</span> {page.prefabName}</p>
                <p><span className="text-[#6E7681]">Hash:</span> {page.prefabHash}</p>
                {page.basePowerDraw && (
                  <p><span className="text-[#6E7681]">Power:</span> <span className="text-yellow-400">{page.basePowerDraw}</span></p>
                )}
                {page.maxPressure && (
                  <p><span className="text-[#6E7681]">Max Pressure:</span> {page.maxPressure}</p>
                )}
                {page.stackSize && (
                  <p><span className="text-[#6E7681]">Stack Size:</span> {page.stackSize}</p>
                )}
              </div>
            </div>
          </div>

          {/* Description */}
          <div className="mb-4 p-3 bg-[#2A2F34] rounded border border-[#3A3F44]">
            <p className="text-sm text-[#E6EDF3] whitespace-pre-wrap leading-relaxed">
              {page.description}
            </p>
          </div>

          {/* Operational Details (from mod) */}
          {page.operationalDetails.length > 0 && (
            <CollapsibleSection title="📋 Operational Details">
              {page.operationalDetails.map((detail, i) => (
                <OperationalDetailSection key={i} detail={detail} />
              ))}
            </CollapsibleSection>
          )}

          {/* Slots */}
          {page.slotInserts.length > 0 && (
            <CollapsibleSection title="📥 Slots">
              <table className="w-full text-sm">
                <thead>
                  <tr className="text-[#8B949E] border-b border-[#3A3F44]">
                    <th className="text-left py-1">Index</th>
                    <th className="text-left py-1">Name</th>
                    <th className="text-left py-1">Type</th>
                  </tr>
                </thead>
                <tbody>
                  {page.slotInserts.map((slot, i) => (
                    <tr key={i} className="border-b border-[#3A3F44]/50">
                      <td className="py-1 text-[#8B949E]">{slot.SlotIndex}</td>
                      <td className="py-1">{slot.SlotName}</td>
                      <td className="py-1 text-[#008AE6]">{slot.SlotType}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </CollapsibleSection>
          )}

          {/* Logic Types */}
          {page.logicTypes.length > 0 && (
            <CollapsibleSection title="🔌 Logic Types">
              <table className="w-full text-sm">
                <thead>
                  <tr className="text-[#8B949E] border-b border-[#3A3F44]">
                    <th className="text-left py-1">Name</th>
                    <th className="text-left py-1">Access</th>
                    <th className="text-left py-1">Description</th>
                  </tr>
                </thead>
                <tbody>
                  {page.logicTypes.map((logic, i) => (
                    <tr key={i} className="border-b border-[#3A3F44]/50">
                      <td className="py-1 text-[#FF7A18]">{safeRender(logic.name)}</td>
                      <td className="py-1 text-[#8B949E]">{safeRender(logic.accessTypes)}</td>
                      <td className="py-1 text-[#6E7681] text-xs">{safeRender(logic.description)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </CollapsibleSection>
          )}

          {/* Logic Slot Types */}
          {page.logicSlotTypes.length > 0 && (
            <CollapsibleSection title="📊 Logic Slot Types">
              <table className="w-full text-sm">
                <thead>
                  <tr className="text-[#8B949E] border-b border-[#3A3F44]">
                    <th className="text-left py-1">Name</th>
                    <th className="text-left py-1">Slots</th>
                    <th className="text-left py-1">Description</th>
                  </tr>
                </thead>
                <tbody>
                  {page.logicSlotTypes.map((slot, i) => (
                    <tr key={i} className="border-b border-[#3A3F44]/50">
                      <td className="py-1 text-[#FF7A18]">{safeRender(slot.name)}</td>
                      <td className="py-1 text-[#8B949E]">{safeRender(slot.slotIndices)}</td>
                      <td className="py-1 text-[#6E7681] text-xs">{safeRender(slot.description)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </CollapsibleSection>
          )}

          {/* Modes */}
          {page.modes.length > 0 && (
            <CollapsibleSection title="⚙️ Modes">
              <table className="w-full text-sm">
                <thead>
                  <tr className="text-[#8B949E] border-b border-[#3A3F44]">
                    <th className="text-left py-1">Mode</th>
                    <th className="text-left py-1">Value</th>
                  </tr>
                </thead>
                <tbody>
                  {page.modes.map((mode, i) => (
                    <tr key={i} className="border-b border-[#3A3F44]/50">
                      <td className="py-1">{safeRender(mode.ModeName)}</td>
                      <td className="py-1 text-[#008AE6]">{safeRender(mode.ModeValue)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </CollapsibleSection>
          )}

          {/* Connections */}
          {page.connections.length > 0 && (
            <CollapsibleSection title="🔗 Connections">
              <table className="w-full text-sm">
                <thead>
                  <tr className="text-[#8B949E] border-b border-[#3A3F44]">
                    <th className="text-left py-1">Index</th>
                    <th className="text-left py-1">Connection</th>
                    <th className="text-left py-1">Description</th>
                  </tr>
                </thead>
                <tbody>
                  {page.connections.map((conn, i) => (
                    <tr key={i} className="border-b border-[#3A3F44]/50">
                      <td className="py-1 text-[#8B949E]">{safeRender(conn.index)}</td>
                      <td className="py-1">{safeRender(conn.name)}</td>
                      <td className="py-1 text-[#6E7681] text-xs">{safeRender(conn.description)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </CollapsibleSection>
          )}

          {/* Construction */}
          {page.structure && page.structure.BuildStates && page.structure.BuildStates.length > 0 && (
            <CollapsibleSection title="🔨 Construction">
              <div className="space-y-2">
                {page.structure.BuildStates.map((state, stateIdx) => (
                  <div key={stateIdx} className="p-2 bg-[#1A1F24] rounded">
                    <p className="text-xs text-[#8B949E] mb-1">Build State {stateIdx + 1}</p>
                    {state.Tool && state.Tool.length > 0 && (
                      <div className="mb-1">
                        <span className="text-xs text-[#6E7681]">Required: </span>
                        {state.Tool.map((tool, i) => (
                          <span key={i} className="text-xs">
                            <span className="text-[#008AE6]">{tool.PrefabName}</span>
                            {tool.Quantity && tool.Quantity > 1 && ` x${tool.Quantity}`}
                            {i < state.Tool!.length - 1 && ', '}
                          </span>
                        ))}
                      </div>
                    )}
                    {state.ToolExit && state.ToolExit.length > 0 && (
                      <div>
                        <span className="text-xs text-[#6E7681]">Deconstruct: </span>
                        {state.ToolExit.map((tool, i) => (
                          <span key={i} className="text-xs text-[#FF7A18]">
                            {tool.PrefabName}
                            {i < state.ToolExit!.length - 1 && ', '}
                          </span>
                        ))}
                      </div>
                    )}
                  </div>
                ))}
              </div>
            </CollapsibleSection>
          )}

          {/* Gas Info */}
          {page.gasInfo && Object.keys(page.gasInfo).length > 0 && (
            <CollapsibleSection title="💨 Gas Properties">
              <div className="grid grid-cols-2 gap-2 text-sm">
                {page.gasInfo.SpecificHeat !== undefined && (
                  <div>
                    <span className="text-[#6E7681]">Specific Heat:</span>{' '}
                    <span>{page.gasInfo.SpecificHeat}</span>
                  </div>
                )}
                {page.gasInfo.MolarMass !== undefined && (
                  <div>
                    <span className="text-[#6E7681]">Molar Mass:</span>{' '}
                    <span>{page.gasInfo.MolarMass}</span>
                  </div>
                )}
                {page.gasInfo.LiquidBoilingPoint !== undefined && (
                  <div>
                    <span className="text-[#6E7681]">Boiling Point:</span>{' '}
                    <span>{page.gasInfo.LiquidBoilingPoint}K</span>
                  </div>
                )}
                {page.gasInfo.FreezeTemperature !== undefined && (
                  <div>
                    <span className="text-[#6E7681]">Freeze Temp:</span>{' '}
                    <span>{page.gasInfo.FreezeTemperature}K</span>
                  </div>
                )}
              </div>
            </CollapsibleSection>
          )}

          {/* Nutrition Info */}
          {page.nutritionInfo && Object.keys(page.nutritionInfo).length > 0 && (
            <CollapsibleSection title="🍎 Nutrition">
              <div className="grid grid-cols-2 gap-2 text-sm">
                {page.nutritionInfo.Nutrition !== undefined && (
                  <div>
                    <span className="text-[#6E7681]">Nutrition:</span>{' '}
                    <span className="text-green-400">{page.nutritionInfo.Nutrition}</span>
                  </div>
                )}
                {page.nutritionInfo.Hydration !== undefined && (
                  <div>
                    <span className="text-[#6E7681]">Hydration:</span>{' '}
                    <span className="text-blue-400">{page.nutritionInfo.Hydration}</span>
                  </div>
                )}
                {page.nutritionInfo.FoodQuality !== undefined && (
                  <div>
                    <span className="text-[#6E7681]">Quality:</span>{' '}
                    <span className="text-yellow-400">{page.nutritionInfo.FoodQuality}</span>
                  </div>
                )}
              </div>
            </CollapsibleSection>
          )}

          {/* Constructed By Kits */}
          {page.constructedByKits.length > 0 && (
            <CollapsibleSection title="📦 Constructed By">
              <div className="flex flex-wrap gap-2">
                {page.constructedByKits.map((kit, i) => (
                  <button
                    key={i}
                    onClick={() => onNavigate?.(`Thing${kit.PrefabName}`)}
                    className="px-2 py-1 text-xs bg-[#1A1F24] rounded border border-[#3A3F44] hover:border-[#FF7A18] text-[#008AE6] transition-colors"
                  >
                    {kit.PrefabName}
                  </button>
                ))}
              </div>
            </CollapsibleSection>
          )}
        </div>
      </div>
    </div>
  );
};

export default DevicePage;
