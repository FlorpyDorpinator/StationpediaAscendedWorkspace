# Advanced Furnace - Logic Documentation

**Prefab Hash:** `-1280984102`  
**Class:** `AdvancedFurnace`  
**Category:** Production  

## Overview

The Advanced Furnace is used for creating superalloys by combining ingots with specific gas mixtures at high temperatures. It features gas input/output ports for atmosphere control and item import/export chutes for materials.

## Readable Logic Types

| Logic Type | Description | Unit | Notes |
|------------|-------------|------|-------|
| `Power` | Receiving power | 0/1 | 1 if powered |
| `Open` | Mold hatch open | 0/1 | 1 if open |
| `Mode` | Recipe validity indicator | 0/1 | 1 if valid recipe loaded |
| `Error` | Error state | int | 0 = no error |
| `Pressure` | Internal pressure | kPa | |
| `Temperature` | Internal temperature | K | |
| `On` | Device enabled | 0/1 | |
| `Lock` | Locked state | 0/1 | |
| `RequiredPower` | Power consumption | W | Varies with settings |
| `RecipeHash` | Current recipe output | hash | 0 if no recipe |
| `ImportCount` | Items imported | int | |
| `ExportCount` | Items exported | int | |
| `SettingInput` | Gas input rate | L/tick | 0-10 |
| `SettingOutput` | Gas output rate | L/tick | 0-10 |
| `TotalMoles` | Gas in atmosphere | mol | |
| `RatioOxygen` | O2 ratio | 0-1 | |
| `RatioCarbonDioxide` | CO2 ratio | 0-1 | |
| `RatioNitrogen` | N2 ratio | 0-1 | |
| `RatioPollutant` | Pollutant ratio | 0-1 | |
| `RatioVolatiles` | Volatiles ratio | 0-1 | |
| `RatioWater` | H2O ratio | 0-1 | |
| `RatioNitrousOxide` | N2O ratio | 0-1 | |

## Writable Logic Types

| Logic Type | Valid Values | Effect |
|------------|--------------|--------|
| `Open` | 0, 1 | 0 = close mold (allow smelting), 1 = open mold (allow export) |
| `Mode` | 0, 1 | **Visual only** - Auto-overwritten by game. Does not affect smelting. |
| `Activate` | >0 | Triggers ignition spark. Adds 5J to atmosphere. Auto-resets to 0. |
| `Lock` | 0, 1 | Prevents manual player interaction |
| `On` | 0, 1 | Enables/disables gas pumps |
| `SettingInput` | 0-10 | Gas input pump rate (L/tick) |
| `SettingOutput` | 0-10 | Gas output pump rate (L/tick) |

## Slot Logic

### Slot 0: Import
| Slot Logic Type | Read | Write | Description |
|-----------------|------|-------|-------------|
| `Occupied` | ✓ | ✗ | Whether slot has an item |
| `OccupantHash` | ✓ | ✗ | Prefab hash of item |
| `Quantity` | ✓ | ✗ | Stack size |

### Slot 1: Export
| Slot Logic Type | Read | Write | Description |
|-----------------|------|-------|-------------|
| `Occupied` | ✓ | ✗ | Whether slot has an item |
| `OccupantHash` | ✓ | ✗ | Prefab hash of item |
| `Quantity` | ✓ | ✗ | Stack size |

## Mode Values

| Value | Name | Description |
|-------|------|-------------|
| 0 | InvalidSmelt | No valid recipe - red indicator |
| 1 | ValidSmelt | Valid recipe loaded - green indicator |

> ⚠️ **Warning:** The `Mode` value is automatically overwritten every frame based on whether a valid recipe is detected. Writing to it has no functional effect on smelting behavior.

## IC10 Examples

### Basic Furnace Monitor
```
alias furnace d0

main:
l r0 furnace Temperature    # Read temperature
l r1 furnace Pressure       # Read pressure
l r2 furnace RecipeHash     # Check for valid recipe
bgtz r2 hasRecipe
j main

hasRecipe:
s furnace Activate 1        # Ignite if recipe ready
j main
```

### Automated Gas Control
```
alias furnace d0
define targetPressure 1000  # 1000 kPa

main:
l r0 furnace Pressure
l r1 furnace On
slt r2 r0 targetPressure    # Is pressure below target?
s furnace SettingInput r2   # Enable input if low
sgt r3 r0 targetPressure    # Is pressure above target?
mul r3 r3 10                # Full output if high
s furnace SettingOutput r3
j main
```

## Notes

- Smelting occurs when:
  1. Mold is closed (`Open` = 0)
  2. Temperature reaches material flashpoint
  3. Valid reagent mixture present
- Gas output goes to Output pipe, liquids go to Output2 pipe
- Power consumption scales with input/output settings
