using Godot;

namespace Waterways.Scripts;

[GlobalClass]
public partial class FloatingCube : RigidBody3D
{
    private static readonly float Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    [Export] public RiverFloatSystem FloatSystem { get; set; }
    [Export] public float MaxEffectiveDepth { get; set; } = 2;
    [Export] public float WaterHeightOffset { get; set; } = 0.7f;
    [Export] public float FloatForce { get; set; } = 20; // Increased from 12 for more buoyancy
    [Export] public float FlowForce { get; set; } = 30; // Reduced from 100 for less dominance
    [Export] public float WaterDrag { get; set; } = 0.05f;
    [Export] public float WaterAngularDrag { get; set; } = 0.05f;

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        if (FloatSystem == null)
        {
            return;
        }

        var waterHeight = FloatSystem.GetWaterHeight(GlobalPosition);
        var depth = waterHeight + WaterHeightOffset - GlobalPosition.Y;
        depth = Mathf.Clamp(depth, -1, MaxEffectiveDepth);

        // Only apply forces if we're actually in water (not at default height)
        if (depth <= 0 || waterHeight == FloatSystem.DefaultHeight)
        {
            return;
        }

        var gravity = Gravity * GravityScale;

        // Buoyancy force (vertical)
        var buoyancyForce = Vector3.Up * gravity * depth * FloatForce;

        // Flow force (horizontal) - only apply if significantly in water
        var flowDirection = FloatSystem.GetWaterFlowDirection(GlobalPosition);
        var flowForceVec = flowDirection * FlowForce * depth / MaxEffectiveDepth; // Scale by depth

        state.ApplyForce(buoyancyForce + flowForceVec);

        // Apply drag only when in water
        state.LinearVelocity *= 1 - WaterDrag;
        state.AngularVelocity *= 1 - WaterAngularDrag;
    }
}
