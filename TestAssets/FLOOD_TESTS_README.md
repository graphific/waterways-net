# Flooding Tests - Step by Step Guide

This document tracks the progressive tests for implementing dynamic water flooding in Waterways.NET.

## Overview

These tests build upon each other to eventually create a realistic dam breakthrough/flooding simulation. Start with Test 1 and work your way through sequentially.

---

## Test 1: Add Point at Runtime ✅ IMPLEMENTED

**Goal:** Prove we can modify rivers during gameplay

**Location:** `TestAssets/Scripts/Test01_AddPoint.cs`

**Test Scene:** `TestAssets/test01_add_point.tscn`

### How to Use:

1. **Open the test scene:**
   - In Godot, open `TestAssets/test01_add_point.tscn`
   - Or add `Test01_AddPoint.cs` to any existing scene with a RiverManager

2. **Assign the River:**
   - Select the `TestController` node
   - In the Inspector, drag your `RiverManager` node to the "River" property
   - (This is already set up in the test scene)

3. **Run the scene:**
   - Press F5 or click the Play button
   - You should see a simple river in the scene

4. **Controls:**
   - **E** - Add a point to the END of the river
   - **SHIFT+E** - Add a point to the START of the river
   - **R** - Reset river to original state
   - **SPACE** - Spawn floating cube (from existing test setup)

5. **What to Watch For:**
   - River should grow longer with each spacebar press
   - Mesh updates should be smooth (no stuttering)
   - Console shows debug messages about point additions
   - No error messages in the console

### Customization:

In the Inspector, you can adjust:
- **Point Distance** (default: 5.0) - How far apart new points are added
- **Enable Debug Print** (default: true) - Show console messages

### Success Criteria:

✅ River extends when pressing spacebar  
✅ No crashes or errors  
✅ Mesh updates visually  
✅ Can reset to original state  
✅ Runs smoothly (no frame drops)  

### What This Teaches:

- The `RiverManager.AddPoint()` API works at runtime
- Mesh regeneration performance is acceptable
- We can dynamically extend rivers in any direction
- Foundation for programmatic river path creation

### Troubleshooting:

**River doesn't extend:**
- Check that the River property is assigned in the Inspector
- Make sure you're clicking in the Game window (not the editor)
- Check the console for error messages

**Performance issues:**
- This is normal if adding many points (50+)
- Test 2 will explore performance optimization

**River extends in wrong direction:**
- Adjust the `PointDistance` value
- The direction is calculated from the curve's tangent

---

## Test 2: Animate River Width Growing ✅ IMPLEMENTED

**Goal:** Make water appear to "swell" like rising flood

**Location:** `TestAssets/Scripts/Test02_WidthGrowth.cs`

**Test Scene:** `TestAssets/test02_width_growth.tscn`

### How to Use:

1. **Open the test scene:**
   - In Godot, open `TestAssets/test02_width_growth.tscn`

2. **Run the scene:**
   - Press F5

3. **Controls:**
   - **F** - Start/Stop flooding animation
   - **+ / =** - Increase flood speed
   - **-** - Decrease flood speed
   - **R** - Reset river widths
   - **SPACE** - Spawn floating cubes

4. **What to Watch For:**
   - River gradually swells wider
   - Console shows performance stats (updates/sec)
   - Average width printed every second
   - Smooth animation with no stuttering

### Success Criteria:

✅ River swells wider smoothly  
✅ Performance acceptable (check updates/sec in console)  
✅ Can adjust speed in real-time  
✅ Visual effect looks like rising water  

### What This Tests:

- Real-time mesh updates every frame
- Visual "swelling" effect
- Performance with continuous updates
- Foundation for water volume visualization
- Mesh regeneration performance budget

### Performance Notes:

The console prints updates per second. On most systems:
- **60+ updates/sec** = Excellent (every frame at 60fps)
- **30-60 updates/sec** = Good
- **<30 updates/sec** = May need optimization

### Customization:

In the Inspector:
- **Growth Rate** (default: 0.5) - How fast width increases
- **Max Width** (default: 10.0) - Maximum width before stopping
- **Speed Adjust Amount** (default: 0.1) - How much +/- changes speed

---

## Test 3+4: Terrain-Aware Expanding Puddle ✅ IMPLEMENTED

**Goal:** Create expanding puddle that conforms to terrain height and finds low points

**Combines:** Test 3 (mesh generation) + Test 4 (terrain scanning)

**Location:** `TestAssets/Scripts/Test03_TerrainPuddle.cs`

**Test Scene:** `TestAssets/test03_terrain_puddle.tscn`

### How to Use:

1. **Open the test scene:**
   - In Godot, open `TestAssets/test03_terrain_puddle.tscn`

2. **Run the scene:**
   - Press F5
   - Two test areas: FLAT (left) and HILLY (right) terrain

3. **Controls:**
   - **P** - Start/Stop puddle expansion
   - **+ / =** - Increase expansion speed
   - **-** - Decrease expansion speed
   - **R** - Reset puddle
   - **L** - Find and mark lowest terrain point (red sphere)

4. **What to Watch For:**
   - Puddle expands in a circle from spawn point
   - Water mesh conforms to terrain height (not flat plane)
   - Lowest point marker shows where water naturally pools
   - Smooth expansion animation

### Success Criteria:

✅ Puddle expands from center  
✅ Mesh conforms to terrain (follows hills/valleys)  
✅ Can find lowest point in scan area  
✅ Smooth performance  
✅ Visual effect looks like spreading water  

### What This Tests:

- **Circular mesh generation** - Creating radial geometry
- **Terrain raycasting** - Sampling ground height at multiple points
- **Vertex positioning** - Placing vertices at terrain height
- **Lowest point detection** - Scanning area for water accumulation point
- **Dynamic mesh updates** - Real-time mesh regeneration
- **Foundation for water spreading** - How water would naturally expand

### Technical Details:

**Mesh Generation:**
- Radial segments (default: 24) - Points around the circle
- Ring segments (default: 10) - Concentric rings from center
- Each vertex raycasts down to find terrain height
- Generates triangles using SurfaceTool

**Terrain Scanning:**
- Raycasts from above (50 units) down (100 units)
- Samples terrain in a grid within puddle radius
- Finds lowest elevation point
- Marks with red sphere for visualization

### Customization:

In the Inspector:
- **Initial Radius** (default: 2.0) - Starting puddle size
- **Expansion Rate** (default: 1.0) - Growth speed
- **Max Radius** (default: 20.0) - Maximum spread
- **Radial Segments** (default: 16) - Mesh detail (circular)
- **Ring Segments** (default: 8) - Mesh detail (radial)
- **Water Height Offset** (default: 0.2) - How high water sits above terrain

---

## Test 5: Height-Field Water Grid 🔄 TODO

**Goal:** Grid of water heights (no flow yet)

**Status:** Not yet implemented

**What it will test:**
- Grid-based water representation
- Height storage and visualization
- Mesh generation from grid data
- Foundation for water simulation

---

## Test 6: Water Flow Simulation ⭐ CORE TEST - TODO

**Goal:** Water flows to neighboring lower cells

**Status:** Not yet implemented

**What it will test:**
- **THE CRITICAL ALGORITHM** - water pressure/gravity simulation
- Multi-directional water spread
- Conservation of mass
- Terrain-aware flow
- Real flooding behavior!

---

## Test 7: River Integration 🔄 TODO

**Goal:** Connect flood system to existing RiverManager

**Status:** Not yet implemented

**What it will test:**
- Triggering flood from river location
- Coordinating between river and flood systems
- Dam breach event handling
- Complete flooding scenario

---

## Progress Tracker

| Test | Status | Difficulty | Est. Time | Completed |
|------|--------|-----------|-----------|-----------|
| Test 1: Add Point | ✅ Done | ⭐ Easy | 10 min | ✅ |
| Test 2: Width Growth | ✅ Done | ⭐ Easy | 15 min | ✅ |
| Test 3+4: Terrain Puddle | ✅ Done | ⭐⭐ Medium | 45 min | ✅ |
| Test 5: Water Grid | 🔄 TODO | ⭐⭐⭐ Hard | 1-2 hrs | ⬜ |
| Test 6: Water Flow | 🔄 TODO | ⭐⭐⭐⭐ Hard | 2-4 hrs | ⬜ |
| Test 7: Integration | 🔄 TODO | ⭐⭐ Medium | 1 hr | ⬜ |

---

## Quick Start Guide

### If you're starting from scratch:

1. **Week 1 - Get Familiar:**
   - ✅ Do Test 1 (done!)
   - Do Test 2 to understand performance
   - Do Test 4 to learn terrain queries

2. **Week 2 - Build Core:**
   - Do Test 5 to create grid structure
   - Do Test 6 - THE BIG ONE! Water flow algorithm

3. **Week 3 - Polish:**
   - Do Test 3 for visual ideas
   - Do Test 7 to connect everything
   - Add effects, foam, particles

### If you want to jump to a specific feature:

- **Just want rivers to grow?** → Test 1 is done!
- **Want water spreading?** → Jump to Test 6 (requires Test 5 first)
- **Want full flooding?** → Complete all tests in order

---

## Integration with Main Project

These tests are designed to be:
- **Standalone** - Each test can run independently
- **Modular** - Copy the code you need into your game
- **Educational** - Understand each piece before combining
- **Incremental** - Build complexity gradually

Once you complete all tests, you'll have:
- Runtime river modification
- Height-field water simulation
- Terrain-aware water flow
- Dam breach/flooding scenarios
- Performance-tested implementation

---

## Notes

- Tests are in `TestAssets/Scripts/Test##_*.cs`
- Test scenes are in `TestAssets/test##_*.tscn`
- All tests work with the existing Waterways.NET plugin
- No modifications to core plugin files needed
- Safe to experiment - won't break your existing rivers

---

## Next Steps

After completing Test 1, try:
1. Experiment with different `PointDistance` values
2. Try adding points in a loop (automated river extension)
3. Add points based on terrain height (use raycasts)
4. Visualize the growth with debug lines

Then move on to **Test 2: Width Growth** to make rivers swell!

---

**Questions or Issues?**
- Check console output for debug messages
- Ensure River is assigned in Inspector
- Verify you're using Godot 4.5 with .NET 8.0
- Review the code comments in each test script
