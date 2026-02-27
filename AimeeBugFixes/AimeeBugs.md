Subject: AIMeE has a few bugs — here's what I found

I had a little more free time so I dug through the code to figure out what's going on more thoroughly and was able to do a bit more comparison so I understood better what you were going for — here's what I found:

**1. She stops mining after the first ore**
_mineCancellation never gets reset after Mine() finishes. So MovingToMineable() sees it's still initialized and just returns true forever — she never moves on to the next target. A quick _mineCancellation.Cancel() in OnMinedOre() fixes it.

**2. She stops driving when there's no ore nearby**
In Roam(), if _minableDataQueue is empty it just returns immediately and skips all the driving code. The old version had her always driving around searching, but now she just sits there. If you let the code fall through to the driving logic (obstacle avoidance, random steering, motor power) instead of returning early, she explores again and the queue system still works fine.

**3. Multiple AIMeEs starve each other**
GetAimeeMinableQueue dumps every reachable ore into the static AllAimeeQueuedMinables HashSet. So the first AIMeE to search claims everything — the rest find nothing because AllAimeeQueuedMinables.Contains filters it all out. Capping the queue per robot (like 10 items each) would let them share.

**4. Search range got halved**
MinableSearchArea went from 32 down to 16, probably for performance since the queue searches less often. But combined with #2 (stops on empty queue), she ends up with a tiny effective range and freezes constantly. Bumping it back to 32 works great with the queue system since the search only fires when the queue runs out anyway.

**5. MineablesInVicinity is way too high**
GetNumberOfReachableMinables counts all near-surface ore in the entire vein without checking if it's actually within the search area. The queue code (GetAimeeMinables) does the searchBounds.Contains check per ore — just needs the same check in the vicinity counter.

**6. She teleports when turned off**
TryUnstuck() and PathToTarget() don't check if she's actually on. So if she's off but still getting logic signals, those methods fire and she hops around or teleports.

**Smaller stuff:**
- MineablesInQueue is always 0 on multiplayer clients — the queue is server-only and never networked
- Picking her up can launch the player because wheel motor torque goes through the drag joint
- Tread animations are frozen on clients because WheelRpm isn't set for non-authority instances----<this one is a real bummer because her treads just dont move haha. 

Anyway, the queue rework is a for sure solid upgrade — it just needs the driving fallback when the queue's empty and some multi-robot fairness and I think you'll be home free. 
