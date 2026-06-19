# Aimee (Mining Robot)

| Property | Value |
|----------|-------|
| **Prefab Name** | ItemRobotAimee |
| **Class** | RobotMining |
| **Type** | Robot |
| **Power** | Battery-powered (5W idle + motor) |

## Overview

**Aimee** is an autonomous mining robot that can be programmed via IC10 to navigate terrain, locate and mine ore deposits, and transport mined resources. She features an internal programmable chip slot, battery slot for power, and multiple storage slots for mined ore.

---

## Slots

| Slot Index | Name | Type | Description |
|------------|------|------|-------------|
| 0 | Battery | Battery | Powers the robot. Required for operation. |
| 1 | ProgrammableChip | Chip | IC10 chip for robot programming. |
| 2+ | Storage | Ore | Storage slots for mined ore (HidesOccupant slots). |

---

## Logic Types

### Readable Logic Types

| Logic Type | Data Type | Range/Units | Description |
|------------|-----------|-------------|-------------|
| Power | Boolean | 0-1 | Returns 1 if robot has battery power, 0 otherwise. |
| On | Boolean | 0-1 | Returns 1 if robot is turned on. |
| Mode | Integer | 0-6 | Current operational mode (see Mode Values). |
| Error | Boolean | 0-1 | Returns 1 if robot has error (no chip or compilation error). |
| Lock | Boolean | 0-1 | Whether the robot is locked. |
| Activate | Boolean | 0-1 | Activation state. |
| ReferenceId | Integer | - | Unique reference ID for IC10 device identification. |
| PressureExternal | Float | kPa | Atmospheric pressure at robot's location. |
| TemperatureExternal | Float | K | Temperature at robot's location. |
| PositionX | Float | meters | World X coordinate. |
| PositionY | Float | meters | World Y coordinate (altitude). |
| PositionZ | Float | meters | World Z coordinate. |
| VelocityMagnitude | Float | m/s | Total speed of the robot. |
| VelocityRelativeX | Float | m/s | Velocity along robot's local X axis (sideways). |
| VelocityRelativeY | Float | m/s | Velocity along robot's local Y axis (vertical). |
| VelocityRelativeZ | Float | m/s | Velocity along robot's local Z axis (forward). |
| VelocityX | Float | m/s | World velocity X component. |
| VelocityY | Float | m/s | World velocity Y component. |
| VelocityZ | Float | m/s | World velocity Z component. |
| ForwardX | Float | -1 to 1 | X component of forward direction vector. |
| ForwardY | Float | -1 to 1 | Y component of forward direction vector. |
| ForwardZ | Float | -1 to 1 | Z component of forward direction vector. |
| Orientation | Float | degrees | Robot's heading/yaw orientation. |
| MineablesInVicinity | Integer | count | Number of minable ore deposits within 32-block range. |

### Writable Logic Types

| Logic Type | Data Type | Range | Description |
|------------|-----------|-------|-------------|
| On | Boolean | 0-1 | Turn robot on (1) or off (0). |
| Mode | Integer | 0-6 | Set operational mode (see Mode Values). |
| Lock | Boolean | 0-1 | Lock (1) or unlock (0) the robot. |
| Activate | Boolean | 0-1 | Set activation state. |
| TargetX | Float | meters | Target X coordinate for movement modes. |
| TargetY | Float | meters | Target Y coordinate for movement modes. |
| TargetZ | Float | meters | Target Z coordinate for movement modes. |

---

## Mode Values

| Value | Name | Description |
|-------|------|-------------|
| 0 | None | Idle state. Brakes applied, no autonomous movement. |
| 1 | Follow | Follows the nearest human player. |
| 2 | MoveToTarget | Moves directly toward TargetX/Y/Z coordinates (may get stuck on obstacles). |
| 3 | Roam | Randomly roams and automatically mines nearby ore deposits. |
| 4 | Unload | Attempts to unload ore to nearby Robot Input devices (within 3m). |
| 5 | PathToTarget | Pathfinds to TargetX/Y/Z coordinates, avoiding obstacles. |
| 6 | StorageFull | Indicates storage is full. Auto-set when storage fills during Roam mode. |

> **Note:** Mode 6 (StorageFull) is automatically set by the game when storage becomes full during Roam mode. Setting this manually has limited usefulness as it simply applies brakes.

---

## Power Consumption

- **Idle (On):** 5W constant draw
- **Motor Power:** Additional 10W × CurrentMotorPower when moving
- **IC10 Execution:** 2.5W per execution cycle (128 instructions per tick)

The robot displays a power level indicator (1-5) based on remaining battery charge:

| Level | Battery Charge |
|-------|----------------|
| 1 | ≤20% |
| 2 | ≤40% |
| 3 | ≤60% |
| 4 | ≤80% |
| 5 | >80% |

---

## Special Behaviors

### IC10 Execution
When the robot is On, has power, contains a valid chip without compilation errors, and the game is running, it executes up to **128 IC10 instructions per game tick**.

### Chip Reset
When a new ProgrammableChip is inserted:
- TargetX, TargetY, TargetZ are reset to 0
- Mode is set to 0 (None)
- The chip program counter is reset

### Stuck Detection
If the robot moves less than **0.1 meters over 60 seconds** while in an active movement mode (Follow, MoveToTarget, Roam, or PathToTarget), it attempts to unstick itself by teleporting slightly in a random direction.

### Mining Behavior (Roam Mode)
In Roam mode:
- Robot searches for ore within **32 blocks horizontally** and **2 blocks below surface**
- When ore is found, robot navigates to it and mines automatically
- Mined ore is stored in internal storage slots
- When storage is full, Mode automatically changes to 6 (StorageFull)

### Unload Behavior
In Unload mode:
- Searches for IRobotInput devices within **3 meters**
- Deposits one ore stack every **3 seconds**
- Returns to Mode 0 when storage is empty or no input device is found

### Ore Generation
Aimee triggers procedural ore vein generation in a **32×32×32 block area** around itself as she moves through the world. This is the same system used by players to generate ore as they explore.

---

## Example IC10 Code

### Basic Mining Loop

```
# Aimee Mining Controller
alias robot d0

start:
# Check if storage is full
l r0 robot Mode
beq r0 6 unload  # Mode 6 = StorageFull

# Set to Roam mode for mining
s robot Mode 3
j start

unload:
# Move to unload position (set your coordinates)
s robot TargetX 100
s robot TargetY 0
s robot TargetZ 50
s robot Mode 5  # PathToTarget

wait_arrive:
l r0 robot VelocityMagnitude
bgt r0 0.1 wait_arrive

# Unload
s robot Mode 4
yield
j start
```

### Reading Position

```
alias robot d0

l r0 robot PositionX
l r1 robot PositionY
l r2 robot PositionZ
l r3 robot Orientation
l r4 robot MineablesInVicinity
```

---

## Slot Logic

### Battery Slot (Index 0)

| LogicSlotType | Description |
|---------------|-------------|
| Occupied | 1 if battery present, 0 otherwise |
| OccupantHash | Prefab hash of battery |
| Charge | Current charge in watts |
| ChargeRatio | Charge as ratio (0-1) |

### Chip Slot (Index 1)

| LogicSlotType | Description |
|---------------|-------------|
| Occupied | 1 if chip present, 0 otherwise |
| OccupantHash | Prefab hash of chip |
| ReferenceId | Reference ID of chip |

### Storage Slots (Index 2+)

| LogicSlotType | Description |
|---------------|-------------|
| Occupied | 1 if ore present, 0 otherwise |
| OccupantHash | Prefab hash of ore type |
| Quantity | Stack size of ore |
| MaxQuantity | Maximum stack size |

---

## Technical Notes

- **Class:** `RobotMining` in namespace `Assets.Scripts.Objects`
- **Inheritance:** RobotMining → WheeledBase → DraggableThing → DynamicThing → Thing
- **Interfaces:** IBatteryPowered, IPowered, ICircuitHolder, IMiningTool, ITransmitable, ILogicable, IRepairable, IGenerateMinables
- **Source File:** `Assets/Scripts/Objects/RobotMining.cs`

---

## See Also

- [Programmable Chip](programmable-chip.md)
- [IC10](../../ic10/README.md)
- [Robot Input](robot-input.md)
- [Ore](ore.md)

---

*Documentation generated from game version: Respawn Update*
