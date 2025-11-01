using Godot;
using Waterways;

namespace WaterwaysTest;

public partial class Test01_AddPoint : Node3D
{
    [Export] public RiverManager River;
    [Export] public float PointDistance = 5f;
    [Export] public float DefaultPointWidth = 1f;
    [Export] public float WidthChangeAmount = 0.5f;
    [Export] public float MinWidth = 0.5f;
    [Export] public float MaxWidth = 20f;
    [Export] public bool EnableDebugPrint = true;

    private int _originalPointCount;
    private Curve3D _originalCurve;
    private Godot.Collections.Array<float> _originalWidths;
    private float _currentWidth = 1f;

    public override void _Ready()
    {
        if (River == null)
        {
            GD.PushError("Test01_AddPoint: No RiverManager assigned! Please assign a river in the inspector.");
            return;
        }

        // Store original river state for reset
        _originalPointCount = River.Curve.PointCount;
        _originalCurve = River.Curve.Duplicate() as Curve3D;
        _originalWidths = new Godot.Collections.Array<float>(River.PointWidths);

        _currentWidth = DefaultPointWidth;

        GD.Print("=== Test 1: Add Point at Runtime ===");
        GD.Print($"River has {River.Curve.PointCount} points initially");
        GD.Print("Controls:");
        GD.Print("  E - Add point to end of river");
        GD.Print("  SHIFT+E - Add point to beginning of river");
        GD.Print("  Q - Increase width (wider river)");
        GD.Print("  A - Decrease width (narrower river)");
        GD.Print("  R - Reset river to original state");
        GD.Print($"Current width: {_currentWidth}");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (River == null) return;

        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.E)
            {
                if (keyEvent.ShiftPressed)
                {
                    AddPointToStart();
                }
                else
                {
                    AddPointToEnd();
                }
                GetViewport().SetInputAsHandled();
            }
            else if (keyEvent.Keycode == Key.Q)
            {
                IncreaseWidth();
                GetViewport().SetInputAsHandled();
            }
            else if (keyEvent.Keycode == Key.A)
            {
                DecreaseWidth();
                GetViewport().SetInputAsHandled();
            }
            else if (keyEvent.Keycode == Key.R)
            {
                ResetRiver();
                GetViewport().SetInputAsHandled();
            }
        }
    }

    private void AddPointToEnd()
    {
        // Get the last point and its direction
        var lastIndex = River.Curve.PointCount - 1;
        var lastPos = River.Curve.GetPointPosition(lastIndex);
        var lastOut = River.Curve.GetPointOut(lastIndex);

        // Calculate direction based on the curve's flow
        var direction = lastOut.Normalized();
        if (direction.Length() < 0.01f)
        {
            // If no direction from curve, use forward direction
            direction = Vector3.Forward;
        }

        // Calculate new position
        var newPos = lastPos + direction * PointDistance;

        // Add the point to the river with current width
        River.AddPoint(newPos, direction * PointDistance * 0.25f, -1, _currentWidth);

        if (EnableDebugPrint)
        {
            GD.Print($"✓ Added point to END. River now has {River.Curve.PointCount} points");
            GD.Print($"  Position: {newPos}, Width: {_currentWidth}");
        }
    }

    private void AddPointToStart()
    {
        // Get the first point and its direction
        var firstPos = River.Curve.GetPointPosition(0);
        var firstIn = River.Curve.GetPointIn(0);

        // Calculate direction (opposite of the incoming direction)
        var direction = -firstIn.Normalized();
        if (direction.Length() < 0.01f)
        {
            // If no direction from curve, use backward direction
            direction = -Vector3.Forward;
        }

        // Calculate new position
        var newPos = firstPos + direction * PointDistance;

        // Add the point to the start (index 0) with current width
        River.AddPoint(newPos, direction * PointDistance * 0.25f, 0, _currentWidth);

        if (EnableDebugPrint)
        {
            GD.Print($"✓ Added point to START. River now has {River.Curve.PointCount} points");
            GD.Print($"  Position: {newPos}, Width: {_currentWidth}");
        }
    }

    private void IncreaseWidth()
    {
        _currentWidth = Mathf.Min(_currentWidth + WidthChangeAmount, MaxWidth);

        if (EnableDebugPrint)
        {
            GD.Print($"⬆ Width increased to: {_currentWidth:F1}");
        }
    }

    private void DecreaseWidth()
    {
        _currentWidth = Mathf.Max(_currentWidth - WidthChangeAmount, MinWidth);

        if (EnableDebugPrint)
        {
            GD.Print($"⬇ Width decreased to: {_currentWidth:F1}");
        }
    }

    private void ResetRiver()
    {
        if (_originalCurve == null)
        {
            GD.PushWarning("No original curve stored, cannot reset");
            return;
        }

        // Restore original curve
        River.Curve = _originalCurve.Duplicate() as Curve3D;
        River.PointWidths = new Godot.Collections.Array<float>(_originalWidths);
        River.UpdateMesh();

        if (EnableDebugPrint)
        {
            GD.Print($"↺ River reset to original state ({_originalPointCount} points)");
        }
    }

}
