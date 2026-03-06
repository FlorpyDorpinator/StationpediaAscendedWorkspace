# AIMeE Bug Overview

14 bugs make AIMeE non-functional in multiplayer and partially broken in singleplayer. All stem from the Orbital/Deep Combustion update rewrite of `RobotMining`.

| # | Bug | What happens |
|---|-----|-------------|
| 1 | **Mine deadlock** | Mines one ore, then permanently stops. `_mineCancellation` token never reset. |
| 2 | **Off-state teleport** | Hops/teleports when powered off but receiving logic. No `OnOff`/`Powered` guard in `UpdateEachFrame`. |
| 3 | **Frozen treads on client** | Tread animations don't play on multiplayer clients. `WheelRpm` is 0 because `WheelCollider` is disabled client-side. |
| 4 | **Roam stops exploring** | Sits still when no ore in range. Early `return` added before driving logic + search radius halved (32→16). |
| 5 | **Player launch (drag)** | Picking up AIMeE launches the player. Motor torque transfers through drag joint. |
| 6 | **Queue always 0 on client** | `MineablesInQueue` reads a server-only local list. Always 0 for clients. |
| 7 | **Vicinity overcount** | `MineablesInVicinity` counts entire veins with no spatial bounds check. |
| 8 | **Player launch (carry)** | Carrying AIMeE near walls launches player. `WheelCollider` springs active while dragged. |
| 9 | **Stuck teleport** | Teleports after 60s stuck. `Unstuck()` directly writes position + has `Random.Range(0,1)` int bug. |
| 10 | **Mines distant ore** | No distance sorting in queue — mines ore in array index order, not by proximity. |
| 11 | **Multi-bot starvation** | First AIMeE monopolizes all ore (no queue cap). Reverse loops from missing cooldown. |
| 12 | **Ghost ore on clients** | Mined ore stays visible on clients. `TryMineServer` missing `SetDensityWorldSpace` call. |
| 13 | **Targets underground ore** | `IsNearSurface` checks one voxel — misses cave ceilings, queues unreachable ore. |
| 14 | **Stale mining queue** | Queue never cleared on pickup, power-on, or TargetXYZ write. Stale entries waste time and block other AIMeEs via `AllAimeeQueuedMinables`. |

All 14 are fixed in the [AimeeBugFixes](https://steamcommunity.com/sharedfiles/filedetails/?id=3674960841) mod (v0.15.3). Native fix recommendations in [DEVELOPER_RECOMMENDATIONS.md](DEVELOPER_RECOMMENDATIONS.md). Full analysis in [BUG_REPORT.md](BUG_REPORT.md).
