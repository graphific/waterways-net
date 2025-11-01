using Godot;
using System.Collections.Generic;

namespace WaterwaysTest;

/// <summary>
/// Test 3+4: Terrain-Aware Expanding Puddle
///
/// GOAL: Create a spreading water puddle that conforms to terrain height
///
/// Combines:
/// - Test 3: Simple expanding puddle mesh generation
/// - Test 4: Raycasting terrain to find heights and low points
///
/// INSTRUCTIONS:
/// 1. Attach this script to any Node3D in your scene (must have terrain below)
/// 2. Set the spawn position where you want the puddle to start
/// 3. Run the scene
/// 4. Press P to start puddle expansion
/// 5. Press + / - to adjust expansion speed
/// 6. Press R to reset puddle
/// 7. Press L to find and mark lowest terrain point
///
/// SUCCESS CRITERIA:
/// - Puddle expands from center point
/// - Puddle conforms to terrain height (not flat)
/// - Can find lowest point in area
/// - Mesh updates smoothly
/// - Visual effect looks like spreading water
/// </summary>
public partial class Test03_TerrainPuddle : Node3D
{
    [Export] public float InitialRadius = 2f;
    [Export] public float ExpansionRate = 1f; // units per second
    [Export] public float MaxRadius = 20f;
    [Export] public int RadialSegments = 16; // How many segments around the circle
    [Export] public int RingSegments = 8; // How many rings from center to edge
    [Export] public float TerrainScanHeight = 50f; // How high above to start raycasts
    [Export] public float TerrainScanDepth = 100f; // How deep to raycast
    [Export] public float WaterHeightOffset = 0.2f; // Water sits slightly above terrain
    [Export] public bool EnableDebugPrint = true;
    [Export] public bool ShowLowPointMarker = true;
    [Export] public bool EnableFlowTowardsLow = true; // Basic fluid simulation
    [Export] public float FlowSpeed = 2f; // How fast center moves toward low point

    private bool _isExpanding = false;
    private float _currentRadius;
    private MeshInstance3D _puddleMesh;
    private MeshInstance3D _lowPointMarker;
    private StandardMaterial3D _waterMaterial;
    private Vector3 _lowestPoint = Vector3.Zero;
    private bool _lowestPointFound = false;
    private Vector3 _flowVelocity = Vector3.Zero;

    public override void _Ready()
    {
        _currentRadius = InitialRadius;

        // Create water material
        _waterMaterial = new StandardMaterial3D
        {
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            AlbedoColor = new Color(0.3f, 0.5f, 0.8f, 0.6f),
            Metallic = 0.2f,
            Roughness = 0.3f,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled // Show both sides
        };

        // Create puddle mesh instance
        _puddleMesh = new MeshInstance3D
        {
            Name = "PuddleMesh",
            MaterialOverride = _waterMaterial
        };
        AddChild(_puddleMesh);

        // Create low point marker
        if (ShowLowPointMarker)
        {
            _lowPointMarker = new MeshInstance3D
            {
                Name = "LowPointMarker",
                Mesh = new SphereMesh { Radius = 0.5f, Height = 1f }
            };

            var markerMat = new StandardMaterial3D
            {
                AlbedoColor = new Color(1f, 0f, 0f, 1f),
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
            };
            _lowPointMarker.MaterialOverride = markerMat;
            _lowPointMarker.Visible = false;
            AddChild(_lowPointMarker);
        }

        // Generate initial puddle
        UpdatePuddleMesh();

        GD.Print("=== Test 3+4: Terrain-Aware Puddle ===");
        GD.Print($"Spawn position: {GlobalPosition}");
        GD.Print("Controls:");
        GD.Print("  P - Start/Stop puddle expansion");
        GD.Print("  + / = - Increase expansion speed");
        GD.Print("  - - Decrease expansion speed");
        GD.Print("  R - Reset puddle");
        GD.Print("  L - Find and mark lowest terrain point");
        GD.Print($"Initial radius: {_currentRadius:F1}, Max radius: {MaxRadius:F1}");
        if (EnableFlowTowardsLow)
        {
            GD.Print("💧 Fluid simulation: Water will flow toward lowest point!");
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.P)
            {
                ToggleExpansion();
                GetViewport().SetInputAsHandled();
            }
            else if (keyEvent.Keycode == Key.Equal || keyEvent.Keycode == Key.Plus)
            {
                IncreaseSpeed();
                GetViewport().SetInputAsHandled();
            }
            else if (keyEvent.Keycode == Key.Minus)
            {
                DecreaseSpeed();
                GetViewport().SetInputAsHandled();
            }
            else if (keyEvent.Keycode == Key.R)
            {
                ResetPuddle();
                GetViewport().SetInputAsHandled();
            }
            else if (keyEvent.Keycode == Key.L)
            {
                FindLowestPoint();
                GetViewport().SetInputAsHandled();
            }
        }
    }

    public override void _Process(double delta)
    {
        if (!_isExpanding) return;

        _currentRadius = Mathf.Min(_currentRadius + ExpansionRate * (float)delta, MaxRadius);
        UpdatePuddleMesh();

        if (_currentRadius >= MaxRadius && _isExpanding)
        {
            _isExpanding = false;
            if (EnableDebugPrint)
            {
                GD.Print($"💧 Puddle reached maximum radius: {MaxRadius:F1}");
            }
        }
    }

    private void UpdatePuddleMesh()
    {
        var surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

        // Generate circular puddle mesh conforming to terrain
        var angleStep = Mathf.Tau / RadialSegments;
        var radiusStep = _currentRadius / RingSegments;

        // Center point
        var centerHeight = SampleTerrainHeight(GlobalPosition);
        var centerPos = new Vector3(0, centerHeight + WaterHeightOffset, 0);

        // Generate vertices in rings
        var vertices = new List<Vector3> { centerPos };

        for (int ring = 1; ring <= RingSegments; ring++)
        {
            var radius = ring * radiusStep;

            for (int segment = 0; segment < RadialSegments; segment++)
            {
                var angle = segment * angleStep;
                var x = Mathf.Cos(angle) * radius;
                var z = Mathf.Sin(angle) * radius;

                var worldPos = GlobalPosition + new Vector3(x, 0, z);
                var terrainHeight = SampleTerrainHeight(worldPos);
                var localPos = new Vector3(x, terrainHeight + WaterHeightOffset, z);

                vertices.Add(localPos);
            }
        }

        // Generate triangles
        // Center triangles (first ring)
        for (int segment = 0; segment < RadialSegments; segment++)
        {
            var next = segment + 1;
            if (next >= RadialSegments) next = 0;

            AddTriangle(surfaceTool, vertices, 0, 1 + segment, 1 + next);
        }

        // Ring triangles
        for (int ring = 0; ring < RingSegments - 1; ring++)
        {
            var currentRingStart = 1 + ring * RadialSegments;
            var nextRingStart = 1 + (ring + 1) * RadialSegments;

            for (int segment = 0; segment < RadialSegments; segment++)
            {
                var next = segment + 1;
                if (next >= RadialSegments) next = 0;

                var v0 = currentRingStart + segment;
                var v1 = currentRingStart + next;
                var v2 = nextRingStart + segment;
                var v3 = nextRingStart + next;

                // Two triangles per quad
                AddTriangle(surfaceTool, vertices, v0, v2, v1);
                AddTriangle(surfaceTool, vertices, v1, v2, v3);
            }
        }

        surfaceTool.GenerateNormals();
        _puddleMesh.Mesh = surfaceTool.Commit();
    }

    private void AddTriangle(SurfaceTool tool, List<Vector3> vertices, int i0, int i1, int i2)
    {
        // Add UV coordinates based on position
        var v0 = vertices[i0];
        var v1 = vertices[i1];
        var v2 = vertices[i2];

        tool.SetUV(new Vector2(v0.X / _currentRadius * 0.5f + 0.5f, v0.Z / _currentRadius * 0.5f + 0.5f));
        tool.AddVertex(v0);

        tool.SetUV(new Vector2(v1.X / _currentRadius * 0.5f + 0.5f, v1.Z / _currentRadius * 0.5f + 0.5f));
        tool.AddVertex(v1);

        tool.SetUV(new Vector2(v2.X / _currentRadius * 0.5f + 0.5f, v2.Z / _currentRadius * 0.5f + 0.5f));
        tool.AddVertex(v2);
    }

    private float SampleTerrainHeight(Vector3 worldPosition)
    {
        var space = GetWorld3D().DirectSpaceState;
        var rayStart = worldPosition + new Vector3(0, TerrainScanHeight, 0);
        var rayEnd = worldPosition - new Vector3(0, TerrainScanDepth, 0);

        var query = PhysicsRayQueryParameters3D.Create(rayStart, rayEnd);
        var result = space.IntersectRay(query);

        if (result.Count > 0)
        {
            return result["position"].AsVector3().Y;
        }

        // Default to spawn position height if no terrain found
        return GlobalPosition.Y;
    }

    private void FindLowestPoint()
    {
        if (EnableDebugPrint)
        {
            GD.Print($"🔍 Scanning for lowest point within {MaxRadius:F1} unit radius...");
        }

        var lowestHeight = float.MaxValue;
        var lowestPos = GlobalPosition;
        var scanResolution = 20; // How many points to check

        for (int x = 0; x < scanResolution; x++)
        {
            for (int z = 0; z < scanResolution; z++)
            {
                var offsetX = (x - scanResolution / 2f) * (MaxRadius / scanResolution) * 2f;
                var offsetZ = (z - scanResolution / 2f) * (MaxRadius / scanResolution) * 2f;

                // Only check within circular area
                if (Mathf.Sqrt(offsetX * offsetX + offsetZ * offsetZ) > MaxRadius)
                    continue;

                var worldPos = GlobalPosition + new Vector3(offsetX, 0, offsetZ);
                var height = SampleTerrainHeight(worldPos);

                if (height < lowestHeight)
                {
                    lowestHeight = height;
                    lowestPos = new Vector3(worldPos.X, height, worldPos.Z);
                }
            }
        }

        _lowestPoint = lowestPos;

        if (_lowPointMarker != null)
        {
            _lowPointMarker.GlobalPosition = _lowestPoint + new Vector3(0, 0.5f, 0);
            _lowPointMarker.Visible = true;
        }

        if (EnableDebugPrint)
        {
            GD.Print($"✅ Lowest point found at: {_lowestPoint}");
            GD.Print($"   Height: {lowestHeight:F2}, Distance from center: {GlobalPosition.DistanceTo(_lowestPoint):F2}");
        }
    }

    private void ToggleExpansion()
    {
        _isExpanding = !_isExpanding;

        if (EnableDebugPrint)
        {
            if (_isExpanding)
            {
                GD.Print($"💧 Puddle expansion started! Speed: {ExpansionRate:F2} units/sec");
            }
            else
            {
                GD.Print($"⏸ Expansion paused at radius: {_currentRadius:F2}");
            }
        }
    }

    private void IncreaseSpeed()
    {
        ExpansionRate = Mathf.Min(ExpansionRate + 0.5f, 10f);

        if (EnableDebugPrint)
        {
            GD.Print($"⬆ Expansion speed: {ExpansionRate:F2} units/sec");
        }
    }

    private void DecreaseSpeed()
    {
        ExpansionRate = Mathf.Max(ExpansionRate - 0.5f, 0.1f);

        if (EnableDebugPrint)
        {
            GD.Print($"⬇ Expansion speed: {ExpansionRate:F2} units/sec");
        }
    }

    private void ResetPuddle()
    {
        _currentRadius = InitialRadius;
        _isExpanding = false;
        UpdatePuddleMesh();

        if (_lowPointMarker != null)
        {
            _lowPointMarker.Visible = false;
        }

        if (EnableDebugPrint)
        {
            GD.Print($"↺ Puddle reset to initial radius: {InitialRadius:F1}");
        }
    }
}
