using Godot;

namespace WaterwaysTest;

/// <summary>
/// Creates a hilly terrain mesh by deforming vertices
/// </summary>
[Tool]
public partial class HillyTerrain : MeshInstance3D
{
    [Export] public Vector2 Size = new(50, 50);
    [Export] public int SubdivideWidth = 30;
    [Export] public int SubdivideDepth = 30;
    [Export] public float HillHeight = 5f;
    [Export] public float HillFrequency = 0.1f;
    [Export] public Color TerrainColor = new(0.3f, 0.5f, 0.3f, 1f);

    public override void _Ready()
    {
        GenerateHillyMesh();
    }

    private void GenerateHillyMesh()
    {
        var surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

        // Generate vertices with height variation
        var vertices = new Vector3[(SubdivideWidth + 1) * (SubdivideDepth + 1)];
        var stepX = Size.X / SubdivideWidth;
        var stepZ = Size.Y / SubdivideDepth;

        int index = 0;
        for (int z = 0; z <= SubdivideDepth; z++)
        {
            for (int x = 0; x <= SubdivideWidth; x++)
            {
                var posX = x * stepX - Size.X / 2;
                var posZ = z * stepZ - Size.Y / 2;

                // Create hills using sine waves
                var height = Mathf.Sin(posX * HillFrequency) * Mathf.Sin(posZ * HillFrequency) * HillHeight;
                height += Mathf.Sin(posX * HillFrequency * 2) * Mathf.Cos(posZ * HillFrequency * 1.5f) * (HillHeight * 0.5f);

                vertices[index] = new Vector3(posX, height, posZ);
                index++;
            }
        }

        // Generate triangles
        for (int z = 0; z < SubdivideDepth; z++)
        {
            for (int x = 0; x < SubdivideWidth; x++)
            {
                var i0 = z * (SubdivideWidth + 1) + x;
                var i1 = i0 + 1;
                var i2 = i0 + (SubdivideWidth + 1);
                var i3 = i2 + 1;

                // First triangle
                surfaceTool.SetUV(new Vector2((float)x / SubdivideWidth, (float)z / SubdivideDepth));
                surfaceTool.AddVertex(vertices[i0]);

                surfaceTool.SetUV(new Vector2((float)(x + 1) / SubdivideWidth, (float)z / SubdivideDepth));
                surfaceTool.AddVertex(vertices[i1]);

                surfaceTool.SetUV(new Vector2((float)x / SubdivideWidth, (float)(z + 1) / SubdivideDepth));
                surfaceTool.AddVertex(vertices[i2]);

                // Second triangle
                surfaceTool.SetUV(new Vector2((float)(x + 1) / SubdivideWidth, (float)z / SubdivideDepth));
                surfaceTool.AddVertex(vertices[i1]);

                surfaceTool.SetUV(new Vector2((float)(x + 1) / SubdivideWidth, (float)(z + 1) / SubdivideDepth));
                surfaceTool.AddVertex(vertices[i3]);

                surfaceTool.SetUV(new Vector2((float)x / SubdivideWidth, (float)(z + 1) / SubdivideDepth));
                surfaceTool.AddVertex(vertices[i2]);
            }
        }

        surfaceTool.GenerateNormals();
        Mesh = surfaceTool.Commit();

        // Apply material
        var material = new StandardMaterial3D
        {
            AlbedoColor = TerrainColor,
            Roughness = 1.0f
        };
        MaterialOverride = material;

        // Generate collision mesh for raycasting
        GenerateCollisionMesh();
    }

    private void GenerateCollisionMesh()
    {
        // Check if we're a child of a StaticBody3D
        var parent = GetParent();
        if (parent is not StaticBody3D staticBody)
        {
            GD.PrintErr($"HillyTerrain '{Name}' must be a child of StaticBody3D for collision to work!");
            return;
        }

        // Remove any existing collision shapes from parent
        foreach (var child in staticBody.GetChildren())
        {
            if (child is CollisionShape3D)
            {
                child.QueueFree();
            }
        }

        // Create trimesh collision from our mesh
        if (Mesh == null)
        {
            GD.PrintErr("Cannot create collision - Mesh is null!");
            return;
        }

        CreateTrimeshCollision();

        // The CreateTrimeshCollision() method adds the CollisionShape3D as a child of this MeshInstance3D
        // We need to move it to the StaticBody3D parent
        CollisionShape3D collisionShape = null;
        foreach (var child in GetChildren())
        {
            if (child is CollisionShape3D cs)
            {
                collisionShape = cs;
                break;
            }
        }

        if (collisionShape != null)
        {
            // Move the collision shape to the parent StaticBody3D
            RemoveChild(collisionShape);
            staticBody.AddChild(collisionShape);
            collisionShape.Owner = staticBody.Owner; // Preserve scene ownership

            GD.Print($"✓ Generated trimesh collision for '{Name}'");
        }
        else
        {
            GD.PrintErr("Failed to create trimesh collision shape!");
        }
    }
}
