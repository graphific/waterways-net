# Dynamic Water Flooding System - Implementation Guide

**For Technical Artists & Programmers**

A step-by-step guide to implementing realistic water flooding simulation in Godot 4.5 / C# .NET 8.0, built on top of the Waterways.NET plugin.

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Test 1: Runtime River Extension](#test-1-runtime-river-extension)
4. [Test 2: Animated Width Growth](#test-2-animated-width-growth)
5. [Test 3+4: Terrain-Aware Puddle](#test-34-terrain-aware-puddle)
6. [Test 5: Height-Field Water Grid](#test-5-height-field-water-grid-todo)
7. [Test 6: Water Flow Simulation](#test-6-water-flow-simulation-todo)
8. [Test 7: Dam Breach Integration](#test-7-dam-breach-integration-todo)
9. [Architecture Overview](#architecture-overview)
10. [Performance Considerations](#performance-considerations)
11. [Troubleshooting](#troubleshooting)

---

## Overview

### Goal
Create a realistic dam breakthrough/flooding system where:
- Water can expand rivers dynamically at runtime
- Water spreads across terrain based on topology
- Water flows downhill and pools in valleys
- Floating objects respond to water flow
- Performance remains acceptable (30+ FPS)

### Approach
Progressive complexity through 7 incremental tests, each building on the previous one.

### Current Status
| Test | Status | Ready to Use |
|------|--------|--------------|
| Test 1: Runtime River Extension | ✅ DONE | ✅ Working |
| Test 2: Animated Width Growth | ✅ DONE | ✅ Working |
| Test 3+4: Terrain-Aware Puddle | ✅ DONE | ✅ Working |
| Test 5: Height-Field Water Grid | ⬜ TODO | Next Step |
| Test 6: Water Flow Simulation | ⬜ TODO | Core Algorithm |
| Test 7: Dam Breach Integration | ⬜ TODO | Final Integration |

---

## Prerequisites

### Required
- **Godot Engine**: 4.5.0 or higher
- **.NET SDK**: 8.0
- **Waterways.NET Plugin**: Installed in `addons/waterways_net/`
- **C# Support**: Project configured with C# solution

### Recommended
- **Terrain System**: Zylann's HeightMap or similar (for realistic testing)
- **Physics Layers**: Configure water collision on dedicated layer (default: layer 9)

### Setup
```bash
# 1. Clone/download Waterways.NET
# 2. Create C# solution
Project → Tools → C# → Create C# Solution

# 3. Build project
Build → Build Project

# 4. Enable plugin
Project Settings → Plugins → Waterways.NET (check enabled)
```

---

## Test 1: Runtime River Extension

### Purpose
Prove that rivers can be modified dynamically during gameplay.

### What It Does
- Add points to river spline at runtime
- Control river width per point
- Reset to original state
- Foundation for programmatic river generation

### Files
- **Script**: `TestAssets/Scripts/Test01_AddPoint.cs`
- **Scene**: `TestAssets/test01_add_point.tscn`

### Usage

```csharp
// In your scene
var riverManager = GetNode<RiverManager>("RiverManager");

// Add point to end
var lastIndex = riverManager.Curve.PointCount - 1;
var lastPos = riverManager.Curve.GetPointPosition(lastIndex);
var newPos = lastPos + new Vector3(0, 0, 5); // 5 units forward
riverManager.AddPoint(newPos, Vector3.Zero, -1, 2.0f); // width = 2.0
```

### Controls
| Key | Action |
|-----|--------|
| **E** | Add point to river end |
| **SHIFT+E** | Add point to river start |
| **Q** | Increase width |
| **A** | Decrease width |
| **R** | Reset river |

### Key Learning
✅ `RiverManager.AddPoint()` works at runtime  
✅ Mesh updates are performant  
✅ Width can be controlled per point  

### API Reference

```csharp
// Add point to river
public void AddPoint(
    Vector3 position,      // Local position
    Vector3 direction,     // Tangent direction
    int index = -1,        // Insert at index (-1 = end)
    float width = -1       // Width at point (-1 = inherit)
)

// Update mesh after modifications
public void UpdateMesh()

// Access curve data
public Curve3D Curve { get; set; }
public Array<float> PointWidths { get; set; }
```

---

## Test 2: Animated Width Growth

### Purpose
Make rivers "swell" like rising water - visual representation of increasing water volume.

### What It Does
- Animate all river widths simultaneously
- Adjustable growth speed
- Real-time mesh updates (every frame)
- Performance testing for continuous updates

### Files
- **Script**: `TestAssets/Scripts/Test02_WidthGrowth.cs`
- **Scene**: `TestAssets/test02_width_growth.tscn`

### Usage

```csharp
// Manual width animation
for (int i = 0; i < riverManager.PointWidths.Count; i++)
{
    riverManager.PointWidths[i] += growthRate * delta;
}
riverManager.UpdateMesh();

// Or use Test02_WidthGrowth component
var widthGrowth = GetNode<Test02_WidthGrowth>("TestController");
widthGrowth.GrowthRate = 1.0f; // units per second
// Toggle with F key or via code:
// widthGrowth._isFlooding = true;
```

### Controls
| Key | Action |
|-----|--------|
| **F** | Start/Stop flooding animation |
| **+** | Increase flood speed |
| **-** | Decrease flood speed |
| **R** | Reset widths |

### Key Learning
✅ Can update mesh every frame without performance issues  
✅ Visual "swelling" effect achieves flooding appearance  
✅ Foundation for volumetric water representation  

### Performance Notes
- Console prints updates/sec
- **60+ updates/sec** = Excellent
- **30-60 updates/sec** = Good
- **<30 updates/sec** = May need optimization

---

## Test 3+4: Terrain-Aware Puddle

### Purpose
Create expanding water that conforms to terrain topology and finds lowest points.

### What It Does
- Generate circular water mesh
- Raycast terrain for each vertex
- Position vertices at terrain height
- Find lowest elevation point
- Foundation for water spreading mechanics

### Files
- **Script**: `TestAssets/Scripts/Test03_TerrainPuddle.cs`
- **Script**: `TestAssets/Scripts/HillyTerrain.cs` (terrain generator)
- **Scene**: `TestAssets/test03_terrain_puddle.tscn`

### Usage

```csharp
// Create terrain-aware puddle
var puddle = new Test03_TerrainPuddle
{
    InitialRadius = 2f,
    MaxRadius = 20f,
    RadialSegments = 24,
    RingSegments = 10
};
AddChild(puddle);

// Position at spawn point
puddle.GlobalPosition = spawnPosition;

// Start expansion
// (In Test03 script, use 'P' key or call programmatically)
```

### Controls
| Key | Action |
|-----|--------|
| **P** | Start/Stop puddle expansion |
| **+** | Increase expansion speed |
| **-** | Decrease expansion speed |
| **L** | Find and mark lowest point |
| **R** | Reset puddle |

### Key Learning
✅ Circular mesh generation works  
✅ Terrain raycasting at scale is viable  
✅ Can find accumulation points (valleys)  
✅ Mesh conforms to terrain realistically  

### Technical Details

**Mesh Generation Algorithm:**
```csharp
// Generate radial mesh
for (int ring = 1; ring <= RingSegments; ring++)
{
    var radius = ring * radiusStep;
    for (int segment = 0; segment < RadialSegments; segment++)
    {
        var angle = segment * angleStep;
        var x = Cos(angle) * radius;
        var z = Sin(angle) * radius;
        
        // Raycast to find terrain height
        var worldPos = GlobalPosition + new Vector3(x, 0, z);
        var terrainHeight = SampleTerrainHeight(worldPos);
        var vertex = new Vector3(x, terrainHeight, z);
        
        vertices.Add(vertex);
    }
}
```

**Terrain Raycasting:**
```csharp
private float SampleTerrainHeight(Vector3 worldPosition)
{
    var space = GetWorld3D().DirectSpaceState;
    var rayStart = worldPosition + Vector3.Up * TerrainScanHeight;
    var rayEnd = worldPosition - Vector3.Up * TerrainScanDepth;
    
    var query = PhysicsRayQueryParameters3D.Create(rayStart, rayEnd);
    var result = space.IntersectRay(query);
    
    return result.Count > 0 
        ? result["position"].AsVector3().Y 
        : DefaultHeight;
}
```

---

## Test 5: Height-Field Water Grid [TODO]

### Purpose
Create a 2D grid storing water and terrain heights - the foundation for flow simulation.

### What It Will Do
- Create NxN grid (e.g., 32x32 cells)
- Store water height at each cell
- Store terrain height at each cell
- Visualize grid as mesh
- Provide query interface for water levels

### Planned Implementation

```csharp
public partial class WaterGrid : Node3D
{
    [Export] public Vector2I GridSize = new(32, 32);
    [Export] public float CellSize = 1f;
    
    private float[,] _waterHeight;   // Water level at each cell
    private float[,] _terrainHeight; // Ground elevation
    
    public override void _Ready()
    {
        _waterHeight = new float[GridSize.X, GridSize.Y];
        _terrainHeight = new float[GridSize.X, GridSize.Y];
        
        // Sample terrain heights
        SampleTerrainIntoGrid();
        
        // Generate mesh
        UpdateGridMesh();
    }
    
    public float GetWaterLevel(int x, int z)
    {
        return _terrainHeight[x, z] + _waterHeight[x, z];
    }
    
    public void AddWater(int x, int z, float amount)
    {
        _waterHeight[x, z] += amount;
    }
}
```

### Expected Controls
- **W** - Add water at center
- **P** - Start simulation
- **R** - Reset grid
- **G** - Toggle grid visualization

### Key Objectives
✅ Grid data structure for water storage  
✅ Efficient terrain sampling  
✅ Mesh visualization of water surface  
✅ Foundation for Test 6 flow algorithm  

---

## Test 6: Water Flow Simulation [TODO]

### Purpose
**THE CORE ALGORITHM** - Make water flow from high cells to low cells.

### What It Will Do
- Calculate pressure gradients between cells
- Move water from high to low elevation
- Simulate water spreading
- Conservation of mass
- Realistic flooding behavior

### Planned Implementation

```csharp
public override void _Process(double delta)
{
    SimulateWaterFlow((float)delta);
    UpdateGridMesh();
}

private void SimulateWaterFlow(float delta)
{
    var newWaterHeights = (float[,])_waterHeight.Clone();
    
    for (int x = 1; x < GridSize.X - 1; x++)
    {
        for (int z = 1; z < GridSize.Y - 1; z++)
        {
            var currentWater = _waterHeight[x, z];
            if (currentWater < 0.01f) continue; // No water
            
            var currentLevel = _terrainHeight[x, z] + currentWater;
            
            // Check 4 neighbors
            var neighbors = new[]
            {
                (x + 1, z), (x - 1, z),
                (x, z + 1), (x, z - 1)
            };
            
            foreach (var (nx, nz) in neighbors)
            {
                var neighborLevel = _terrainHeight[nx, nz] + _waterHeight[nx, nz];
                var heightDiff = currentLevel - neighborLevel;
                
                if (heightDiff > 0)
                {
                    // Flow water downhill
                    var flowAmount = Mathf.Min(
                        heightDiff * FlowSpeed * delta,
                        currentWater / 4f // Divide among 4 neighbors
                    );
                    
                    newWaterHeights[x, z] -= flowAmount;
                    newWaterHeights[nx, nz] += flowAmount;
                }
            }
        }
    }
    
    _waterHeight = newWaterHeights;
}
```

### Expected Controls
- **P** - Start/Pause simulation
- **W** - Add water source
- **D** - Add drain
- **+/-** - Adjust flow speed

### Key Objectives
✅ Water flows downhill naturally  
✅ Pools in valleys before overflowing  
✅ Multiple flow paths simultaneously  
✅ Realistic flood spreading  
✅ Acceptable performance (target: 30+ FPS with 32x32 grid)  

---

## Test 7: Dam Breach Integration [TODO]

### Purpose
Integrate all systems - trigger flood from river, spreading via grid.

### What It Will Do
- Detect dam breach event
- Transfer water from river to grid
- Expand river width at breach point
- Grid simulation handles overflow
- Coordinate between river and flood systems

### Planned Implementation

```csharp
public partial class DamBreachSimulator : Node3D
{
    [Export] public RiverManager UpstreamRiver;
    [Export] public WaterGrid FloodGrid;
    [Export] public int BreachPointIndex = 0; // River point where dam breaks
    [Export] public float WaterVolume = 1000f; // m³
    [Export] public float BreachFlowRate = 100f; // m³/sec
    
    private bool _damBreached = false;
    
    public void TriggerBreach()
    {
        _damBreached = true;
        
        // Widen river at breach point
        UpstreamRiver.PointWidths[BreachPointIndex] *= 3f;
        UpstreamRiver.UpdateMesh();
        
        GD.Print("🌊 DAM BREACHED!");
    }
    
    public override void _Process(double delta)
    {
        if (!_damBreached) return;
        
        // Add water to grid at breach point
        var breachPos = UpstreamRiver.ToGlobal(
            UpstreamRiver.Curve.GetPointPosition(BreachPointIndex)
        );
        
        // Convert to grid coordinates
        var gridPos = FloodGrid.WorldToGrid(breachPos);
        
        // Add water flow
        FloodGrid.AddWater(gridPos.X, gridPos.Y, BreachFlowRate * (float)delta);
        
        // Decrease reservoir volume
        WaterVolume -= BreachFlowRate * (float)delta;
        
        if (WaterVolume <= 0)
        {
            _damBreached = false;
            GD.Print("Reservoir empty");
        }
    }
}
```

### Expected Controls
- **B** - Breach dam
- **R** - Reset scenario
- **P** - Pause simulation

### Key Objectives
✅ Seamless transition from river to grid  
✅ Realistic breach event  
✅ Coordinated visual effects  
✅ Complete flood scenario  

---

## Architecture Overview

### System Components

```
┌─────────────────────────────────────────────────────────────┐
│                     FLOODING SYSTEM                          │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────┐         ┌──────────────────┐          │
│  │  RiverManager    │◄────────┤  Test01/Test02   │          │
│  │  (Waterways.NET) │         │  (Extensions)    │          │
│  └──────────────────┘         └──────────────────┘          │
│         │                                                     │
│         │ Breach Event                                       │
│         ▼                                                     │
│  ┌──────────────────┐         ┌──────────────────┐          │
│  │  DamBreach       │────────►│  WaterGrid       │          │
│  │  Coordinator     │         │  (Test05)        │          │
│  └──────────────────┘         └──────────────────┘          │
│                                        │                      │
│                                        │ Flow Simulation     │
│                                        ▼                      │
│                                ┌──────────────────┐          │
│                                │  FlowSimulator   │          │
│                                │  (Test06)        │          │
│                                └──────────────────┘          │
│                                        │                      │
│                                        ▼                      │
│  ┌──────────────────┐         ┌──────────────────┐          │
│  │ FloatingObjects  │◄────────┤ RiverFloatSystem │          │
│  │ (Physics)        │         │ (Existing)       │          │
│  └──────────────────┘         └──────────────────┘          │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### Data Flow

```
User Input (Dam Breach)
    │
    ▼
DamBreachSimulator
    │
    ├──► RiverManager (visual: widen river)
    │
    └──► WaterGrid (add water volume)
         │
         ▼
    FlowSimulator (spread water)
         │
         ▼
    TerrainPuddle/Mesh (visualize)
         │
         ▼
    RiverFloatSystem (physics interaction)
         │
         ▼
    FloatingObjects (respond to flow)
```

---

## Performance Considerations

### Mesh Generation
**Cost**: High - involves vertex generation, normal calculation, triangulation

**Optimization Strategies:**
```csharp
// 1. Update only when necessary
if (widthChanged)
{
    River.UpdateMesh(); // Don't call every frame
}

// 2. Reduce subdivision for distant objects
var subdivisions = distance > 50f ? 4 : 8;

// 3. Use LOD for large rivers
if (distanceToCamera > LOD_Distance)
{
    // Use simpler mesh
}
```

### Raycasting
**Cost**: Medium - physics queries can be expensive

**Optimization Strategies:**
```csharp
// 1. Cache terrain heights
private Dictionary<Vector2I, float> _heightCache = new();

// 2. Sample at lower resolution
var step = gridCellSize * 2; // Sample every other cell

// 3. Spread raycasts over multiple frames
var raysPerFrame = 100;
for (int i = 0; i < raysPerFrame; i++)
{
    // Process subset of raycasts
}
```

### Water Flow Simulation
**Cost**: Very High - runs every frame for all cells

**Optimization Strategies:**
```csharp
// 1. Use fixed time step
private float _accumulator = 0f;
private const float FixedStep = 1f / 30f; // 30 updates/sec

public override void _Process(double delta)
{
    _accumulator += (float)delta;
    while (_accumulator >= FixedStep)
    {
        SimulateWaterFlow(FixedStep);
        _accumulator -= FixedStep;
    }
}

// 2. Skip cells with no water
if (_waterHeight[x, z] < 0.01f) continue;

// 3. Spatial partitioning - only simulate active regions
var activeRegion = GetActiveWaterBounds();
for (int x = activeRegion.Min.X; x < activeRegion.Max.X; x++)
{
    // ...
}

// 4. Use compute shaders (advanced)
// Move simulation to GPU for massive parallelization
```

### Target Performance
- **Grid Size**: 32x32 = **Good**, 64x64 = **Challenging**, 128x128 = **GPU Required**
- **Update Rate**: 30 updates/sec = **Minimum**, 60 updates/sec = **Ideal**
- **Raycast Budget**: <1000 rays/frame
- **Mesh Vertices**: <10,000 per water surface

---

## Troubleshooting

### Issue: Puddle floats above terrain

**Cause**: Spawn position too high or raycasts not hitting terrain

**Solution**:
```csharp
// Ensure terrain has collision
// Check physics layers match
// Lower spawn position
GlobalPosition = new Vector3(x, terrainHeight + 0.2f, z);

// Increase raycast range
TerrainScanHeight = 100f;
TerrainScanDepth = 200f;
```

### Issue: Water doesn't flow

**Cause**: No height difference between cells or flow speed too low

**Solution**:
```csharp
// Check terrain heights are different
GD.Print($"Height diff: {heightDiff}");

// Increase flow speed
FlowSpeed = 2f; // Start with 2, adjust

// Ensure water volume is sufficient
if (waterHeight < 0.1f) // Too little water
```

### Issue: Performance drops

**Cause**: Too many updates or high grid resolution

**Solution**:
```csharp
// Reduce grid size
GridSize = new Vector2I(16, 16); // Start small

// Use fixed timestep
// See optimization strategies above

// Profile with Godot profiler
// Check: _Process time, Physics time, Rendering time
```

### Issue: Mesh looks wrong

**Cause**: Normal generation issues or vertex order

**Solution**:
```csharp
// Ensure correct winding order (CCW)
surfaceTool.AddVertex(v0);
surfaceTool.AddVertex(v1);
surfaceTool.AddVertex(v2);

// Always generate normals
surfaceTool.GenerateNormals();

// Check mesh in inspector
// Look for: inverted faces, missing triangles
```

---

## Next Steps

### For Beginners
1. ✅ Complete Tests 1-3 (follow README in `TestAssets/`)
2. Study the code comments
3. Experiment with parameters in Inspector
4. Move to Test 5 when ready

### For Experienced Developers
1. Review Tests 1-3 code
2. Implement Test 5 (Water Grid) based on planned implementation
3. Implement Test 6 (Flow Simulation)
4. Optimize for your use case

### For Production Use
1. Complete all 7 tests
2. Profile performance with your terrain
3. Implement LOD system
4. Add visual effects (foam, particles, splash)
5. Integrate with game events
6. Add save/load for water state

---

## Additional Resources

### Documentation
- **Waterways.NET**: `addons/waterways_net/README.md`
- **Test Scenarios**: `TestAssets/FLOOD_TESTS_README.md`
- **Godot Mesh API**: https://docs.godotengine.org/en/stable/classes/class_surfacetool.html
- **Physics Raycasting**: https://docs.godotengine.org/en/stable/tutorials/physics/ray-casting.html

### References
- Height-field water simulation (Müller et al.)
- Shallow water equations
- SPH (Smoothed Particle Hydrodynamics) for advanced use

### Community
- Godot Discord: #scripting, #3d-discussion
- GitHub Issues: Report bugs in Waterways.NET repo

---

## Credits

**Waterways.NET Plugin**: Tshmofen / Timofey Ivanov  
**Original Waterways**: Arnklit (Godot 3 version)  
**Flooding System**: Incremental development framework

---

## License

Follow the Waterways.NET plugin license. Test scripts are provided as examples for educational and commercial use.

---

**Last Updated**: 2025-01-11  
**Godot Version**: 4.5.0  
**.NET Version**: 8.0  
**Plugin Version**: Waterways.NET 1.0.0+
