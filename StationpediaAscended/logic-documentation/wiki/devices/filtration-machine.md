# Kit (Atmospherics) Filtration

| Property | Value |
|----------|-------|
| **Prefab Name** | StructureFiltration |
| **Class** | FiltrationMachine |
| **Type** | Atmospherics Device |
| **Power** | 100W max (scales with pressure) |

## Overview

The **Filtration Machine** is a pipe-mounted atmospherics device that separates gases from a mixed input stream using installed gas filters. It has one input and two outputs - filtered gas goes to Output1, unfiltered/remaining gas goes to Output2. Supports IC10 programming via an internal chip slot.

---

## Slots

| Slot Index | Name | Type | Description |
|------------|------|------|-------------|
| 0 | Filter1 | GasFilter | First gas filter slot |
| 1 | Filter2 | GasFilter | Second gas filter slot |
| 2 | ProgrammableChip | Chip | IC10 chip for automation |

---

## Connections

| Connection | Type | Description |
|------------|------|-------------|
| Input | Pipe | Mixed gas input |
| Output (Filtered) | Pipe | Filtered gas output - gases matching filter types |
| Output2 (Unfiltered) | Pipe | Remaining gas that didn't match any filter |

---

## Logic Types

### Readable Logic Types

| Logic Type | Data Type | Range/Units | Description |
|------------|-----------|-------------|-------------|
| Power | Boolean | 0-1 | Returns 1 if device is powered. |
| On | Boolean | 0-1 | Returns 1 if device is turned on. |
| Mode | Integer | 0-1 | Current operation mode (0=Idle, 1=Active). |
| Error | Boolean | 0-1 | Returns 1 if device has error (not fully connected or chip error). |
| Lock | Boolean | 0-1 | Whether the device is locked. |
| Open | Boolean | 0-1 | Whether the device panel is open. |
| ReferenceId | Integer | - | Unique reference ID for IC10. |
| PressureInput | Float | kPa | Pressure in input pipe network. |
| TemperatureInput | Float | K | Temperature in input pipe network. |
| TotalMolesInput | Float | mol | Total moles in input pipe network. |
| RatioOxygenInput | Float | 0-1 | Oxygen ratio in input pipe. |
| RatioCarbonDioxideInput | Float | 0-1 | CO2 ratio in input pipe. |
| RatioNitrogenInput | Float | 0-1 | Nitrogen ratio in input pipe. |
| RatioPollutantInput | Float | 0-1 | Pollutant (X) ratio in input pipe. |
| RatioVolatilesInput | Float | 0-1 | Volatiles (H2) ratio in input pipe. |
| RatioWaterInput | Float | 0-1 | Water vapor ratio in input pipe. |
| RatioNitrousOxideInput | Float | 0-1 | N2O ratio in input pipe. |
| CombustionInput | Boolean | 0-1 | 1 if input atmosphere is burning. |
| RatioLiquidNitrogenInput | Float | 0-1 | Liquid nitrogen ratio. |
| RatioLiquidOxygenInput | Float | 0-1 | Liquid oxygen ratio. |
| RatioLiquidVolatilesInput | Float | 0-1 | Liquid volatiles ratio. |
| RatioSteamInput | Float | 0-1 | Steam ratio. |
| RatioLiquidCarbonDioxideInput | Float | 0-1 | Liquid CO2 ratio. |
| RatioLiquidPollutantInput | Float | 0-1 | Liquid pollutant ratio. |
| RatioLiquidNitrousOxideInput | Float | 0-1 | Liquid N2O ratio. |
| PressureOutput | Float | kPa | Pressure in filtered output pipe. |
| TemperatureOutput | Float | K | Temperature in filtered output pipe. |
| TotalMolesOutput | Float | mol | Total moles in filtered output pipe. |
| RatioOxygenOutput | Float | 0-1 | Oxygen ratio in filtered output. |
| RatioCarbonDioxideOutput | Float | 0-1 | CO2 ratio in filtered output. |
| RatioNitrogenOutput | Float | 0-1 | Nitrogen ratio in filtered output. |
| RatioPollutantOutput | Float | 0-1 | Pollutant ratio in filtered output. |
| RatioVolatilesOutput | Float | 0-1 | Volatiles ratio in filtered output. |
| RatioWaterOutput | Float | 0-1 | Water ratio in filtered output. |
| RatioNitrousOxideOutput | Float | 0-1 | N2O ratio in filtered output. |
| CombustionOutput | Boolean | 0-1 | 1 if filtered output is burning. |
| TotalMolesOutput | Float | mol | Total moles in filtered output. |
| PressureOutput2 | Float | kPa | Pressure in unfiltered output pipe. |
| TemperatureOutput2 | Float | K | Temperature in unfiltered output pipe. |
| TotalMolesOutput2 | Float | mol | Total moles in unfiltered output pipe. |
| RatioOxygenOutput2 | Float | 0-1 | Oxygen ratio in unfiltered output. |
| RatioCarbonDioxideOutput2 | Float | 0-1 | CO2 ratio in unfiltered output. |
| RatioNitrogenOutput2 | Float | 0-1 | Nitrogen ratio in unfiltered output. |
| RatioPollutantOutput2 | Float | 0-1 | Pollutant ratio in unfiltered output. |
| RatioVolatilesOutput2 | Float | 0-1 | Volatiles ratio in unfiltered output. |
| RatioWaterOutput2 | Float | 0-1 | Water ratio in unfiltered output. |
| RatioNitrousOxideOutput2 | Float | 0-1 | N2O ratio in unfiltered output. |
| CombustionOutput2 | Boolean | 0-1 | 1 if unfiltered output is burning. |

### Writable Logic Types

| Logic Type | Data Type | Range | Description |
|------------|-----------|-------|-------------|
| On | Boolean | 0-1 | Turn device on (1) or off (0). |
| Mode | Integer | 0-1 | Set operation mode (0=Idle, 1=Active). |
| Lock | Boolean | 0-1 | Lock (1) or unlock (0) the device. |
| Open | Boolean | 0-1 | Open (1) or close (0) the panel. |

---

## Mode Values

| Value | Name | Description |
|-------|------|-------------|
| 0 | Idle | Device is not processing gas. |
| 1 | Active | Device actively filters gas from input to outputs. |

---

## Power Consumption

- **Base:** Variable, scales with input pressure
- **Formula:** `Lerp(0, 100W, InputPressure / PressurePerTick)`
- **Maximum:** 100W (EnergyPerAtmosphere constant)

Power draw increases proportionally with the pressure differential being processed.

---

## Special Behaviors

### Filtering Process
Each game tick when On, Powered, Mode=1, and IsOperable:
1. Calculates pressure differential: `InputPressure - Max(Output1Pressure, Output2Pressure)`
2. Takes gas from input proportional to pressure differential
3. Each installed filter extracts matching gas type to Output1 (filtered)
4. Remaining unfiltered gas goes to Output2 (unfiltered)

### Filter Behavior
- Filters are processed in slot order (slot 0 first, then slot 1)
- Each filter extracts its gas type until the filter is exhausted
- `MinimumRatioToFilterAll = 0.001` - filters work until gas ratio drops below 0.1%

### Error Conditions
Error = 1 when:
- Not all three pipe connections are valid (Input, Output, Output2)
- IC10 chip has compilation errors
- `CodeErrorState != 0` (runtime error from IC10)

### IC10 Integration
- Executes 128 instructions per tick when On and Powered
- Can reference 2 external devices via buttons (d0, d1)
- Chip resets when inserted (clears labels and error state)

---

## Example IC10 Code

### Basic Filtration Control

```
# Filtration Controller
alias filtration d0

# Turn on and set to active mode
s filtration On 1
s filtration Mode 1

loop:
# Read input pressure
l r0 filtration PressureInput
# Read output pressure  
l r1 filtration PressureOutput
# Calculate differential
sub r2 r0 r1

# If differential too low, idle
blt r2 10 idle
j loop

idle:
s filtration Mode 0
yield
j loop
```

### Monitoring Gas Ratios

```
alias filtration d0

# Read input gas composition
l r0 filtration RatioOxygenInput
l r1 filtration RatioCarbonDioxideInput
l r2 filtration RatioNitrogenInput
l r3 filtration RatioPollutantInput

# Read filtered output composition
l r4 filtration RatioOxygenOutput
l r5 filtration PressureOutput
```

---

## Slot Logic

### Filter Slots (Index 0, 1)

| LogicSlotType | Description |
|---------------|-------------|
| Occupied | 1 if filter present, 0 otherwise |
| OccupantHash | Prefab hash of filter type |
| Quantity | Filter remaining capacity |
| MaxQuantity | Filter maximum capacity |
| FilterType | Hash of gas type this filter captures |

### Chip Slot (Index 2)

| LogicSlotType | Description |
|---------------|-------------|
| Occupied | 1 if chip present, 0 otherwise |
| OccupantHash | Prefab hash of chip |
| ReferenceId | Reference ID of chip |

---

## Technical Notes

- **Class:** `FiltrationMachine` extends `FiltrationMachineBase` in namespace `Assets.Scripts.Objects.Pipes`
- **Inheritance:** FiltrationMachine → FiltrationMachineBase → DeviceInputOutputCircuit → DeviceInputOutput → DeviceAtmospherics → Device
- **Interfaces:** ICircuitHolder, IDensePoolable
- **Source Files:** 
  - `Assets/Scripts/Objects/Pipes/FiltrationMachine.cs`
  - `Assets/Scripts/Objects/Pipes/FiltrationMachineBase.cs`

---

## See Also

- [Gas Filter](gas-filter.md)
- [Active Vent](active-vent.md)
- [Volume Pump](volume-pump.md)
- [Programmable Chip](programmable-chip.md)

---

*Documentation generated from game version: Respawn Update*
