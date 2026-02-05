# Stationeers Logic Documentation Framework

## Overview

This framework provides a systematic approach to documenting all logic types available on every device in Stationeers. The goal is to produce accurate, code-verified documentation that explains not just *what* logic values exist, but *what they actually do*.

## Output Formats

| Format | Location | Use Case |
|--------|----------|----------|
| JSON Database | `data/devices/` | Machine-readable, mod integration, API |
| Markdown | `wiki/` | Human-readable, wiki export |
| Summary CSV | `data/summary.csv` | Quick reference, spreadsheet import |

## Schema

See `schema/device-logic-schema.json` for the complete JSON schema.

### Key Fields Per Device

```json
{
  "prefabName": "Advanced Furnace",
  "prefabHash": -1280984102,
  "className": "AdvancedFurnace",
  "inheritanceChain": ["AdvancedFurnace", "FurnaceBase", "DeviceInputOutputImportExport", "Device"],
  "logicTypes": [
    {
      "name": "Temperature",
      "canRead": true,
      "canWrite": false,
      "readBehavior": {
        "description": "Internal atmosphere temperature",
        "unit": "Kelvin",
        "sourceProperty": "InternalAtmosphere.Temperature"
      }
    }
  ]
}
```

## Extraction Methodology

### Step 1: Identify All ILogicable Classes
```
grep -r "class.*:.*ILogicable" --include="*.cs"
```

### Step 2: Trace Inheritance Chain
For each class, document the full inheritance up to `Thing`:
```
AdvancedFurnace → FurnaceBase → DeviceInputOutputImportExport → DeviceInputOutput → DeviceInput → DeviceAtmospherics → Device → SmallGrid → Structure → Thing
```

### Step 3: Extract CanLogicRead/CanLogicWrite
Find all LogicType values handled in:
- `CanLogicRead(LogicType logicType)`
- `CanLogicWrite(LogicType logicType)`
- `GetLogicValue(LogicType logicType)`
- `SetLogicValue(LogicType logicType, double value)`

### Step 4: Analyze Actual Behavior
For each writable LogicType, trace what `SetLogicValue` actually does:
- Does it call `OnServer.Interact()`?
- Does it set a property directly?
- Is the value auto-overwritten by game logic?
- Are there side effects?

### Step 5: Document Inheritance
Mark which class in the chain provides each logic type:
- Native (defined in this class)
- Inherited from parent
- Base class (Device, Thing)

## Directory Structure

```
docs/logic-documentation/
├── README.md                    # This file
├── schema/
│   └── device-logic-schema.json # JSON Schema
├── data/
│   ├── devices/                 # Individual device JSON files
│   │   ├── atmospheric/
│   │   ├── power/
│   │   ├── logic/
│   │   └── ...
│   ├── logic-types.json         # Master list of all LogicType enums
│   ├── logic-slot-types.json    # Master list of all LogicSlotType enums
│   └── summary.csv              # Quick reference spreadsheet
├── wiki/
│   ├── devices/                 # Markdown per device
│   └── logic-types/             # Markdown per logic type
└── samples/
    └── advanced-furnace.json    # Example fully documented device
```

## Behavior Classification

### Read Behaviors

| Type | Description | Example |
|------|-------------|---------|
| **Direct Property** | Returns a simple property value | `Temperature` → `InternalAtmosphere.Temperature` |
| **Calculated** | Returns computed/derived value | `PowerRequired` → calculated from settings |
| **Conditional** | Value depends on device state | Returns -1 if unpowered |
| **Slot-Based** | Reads from item in slot | `Charge` on battery in slot |

### Write Behaviors

| Type | Description | Example |
|------|-------------|---------|
| **Interactable** | Triggers OnServer.Interact() | `Open` → opens/closes device |
| **Setting** | Sets a configurable value | `Setting` → changes output rate |
| **Trigger** | One-shot action | `Activate` → ignites furnace |
| **Visual Only** | Changes appearance, no function | `Mode` on furnace (auto-overwritten) |
| **No Effect** | Writable but ignored | Some inherited values |

## Quality Checklist

For each documented device:

- [ ] All readable LogicTypes identified
- [ ] All writable LogicTypes identified  
- [ ] Read behavior described with units
- [ ] Write behavior described with valid ranges
- [ ] Side effects documented
- [ ] Auto-overwrite behavior noted
- [ ] Inheritance source identified
- [ ] Slot logic documented (if applicable)
- [ ] Mode values documented (if applicable)
- [ ] Tested against actual game behavior

## Integration Targets

### Wiki Export
Generate MediaWiki-compatible markdown for https://stationeers-wiki.com

### In-Game Mod
JSON database can be loaded by a Stationeers mod to provide:
- Tooltip enhancements
- IC10 IDE autocomplete
- Logic debugger

### IC10 IDE
Provide autocomplete data for:
- Valid LogicTypes per device hash
- Value ranges and units
- Behavioral warnings (e.g., "This value is auto-overwritten")

## Version Tracking

Each extraction should note:
- Game version
- Extraction date
- Any changes from previous version
