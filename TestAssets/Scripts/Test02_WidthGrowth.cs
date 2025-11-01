using Godot;
using Waterways;

namespace WaterwaysTest;

// - River swells wider smoothly when flooding active
// - Performance stays acceptable (no frame drops)
// - Can control flood speed in real-time

public partial class Test02_WidthGrowth : Node3D
{
    [Export] public RiverManager River;
    [Export] public float GrowthRate = 0.5f; // units per second
    [Export] public float MinWidth = 0.5f;
    [Export] public float MaxWidth = 10f;
    [Export] public float SpeedAdjustAmount = 0.1f;
    [Export] public bool EnableDebugPrint = true;

    private bool _isFlooding = false;
    private Godot.Collections.Array<float> _originalWidths;
    private float _lastUpdateTime = 0f;
    private int _updateCounter = 0;

    public override void _Ready()
    {
        if (River == null)
        {
            GD.PushError("Test02_WidthGrowth: No RiverManager assigned!");
            return;
        }

        // Store original widths for reset
        _originalWidths = new Godot.Collections.Array<float>(River.PointWidths);

        GD.Print("=== Test 2: Width Growth Animation ===");
        GD.Print($"River has {River.Curve.PointCount} points");
        GD.Print("Controls:");
        GD.Print("  F - Start/Stop flooding");
        GD.Print("  + (Plus/=) - Increase flood speed");
        GD.Print("  - (Minus) - Decrease flood speed");
        GD.Print("  R - Reset river widths");
        GD.Print($"Current growth rate: {GrowthRate:F2} units/sec");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (River == null) return;

        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.F)
            {
                ToggleFlooding();
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
                ResetWidths();
                GetViewport().SetInputAsHandled();
            }
        }
    }

    public override void _Process(double delta)
    {
        if (!_isFlooding || River == null) return;

        bool needsUpdate = false;

        // Gradually increase all point widths
        for (int i = 0; i < River.PointWidths.Count; i++)
        {
            var currentWidth = River.PointWidths[i];
            var newWidth = Mathf.Min(currentWidth + GrowthRate * (float)delta, MaxWidth);

            if (Mathf.Abs(newWidth - currentWidth) > 0.001f)
            {
                River.PointWidths[i] = newWidth;
                needsUpdate = true;
            }
        }

        // Update mesh only if widths changed
        if (needsUpdate)
        {
            River.UpdateMesh();
            _updateCounter++;

            // Performance monitoring - print every second
            _lastUpdateTime += (float)delta;
            if (_lastUpdateTime >= 1.0f && EnableDebugPrint)
            {
                var avgWidth = CalculateAverageWidth();
                GD.Print($"💧 Flooding... Avg width: {avgWidth:F2}, Updates/sec: {_updateCounter}");
                _lastUpdateTime = 0f;
                _updateCounter = 0;
            }
        }
        else if (_isFlooding && EnableDebugPrint)
        {
            // All widths reached max
            GD.Print($"🌊 Maximum flood level reached! (Width: {MaxWidth})");
            _isFlooding = false;
        }
    }

    private void ToggleFlooding()
    {
        _isFlooding = !_isFlooding;

        if (EnableDebugPrint)
        {
            if (_isFlooding)
            {
                GD.Print($"🌊 FLOODING STARTED! Growth rate: {GrowthRate:F2} units/sec");
            }
            else
            {
                GD.Print("⏸ Flooding paused");
            }
        }
    }

    private void IncreaseSpeed()
    {
        GrowthRate = Mathf.Min(GrowthRate + SpeedAdjustAmount, 5f);

        if (EnableDebugPrint)
        {
            GD.Print($"⬆ Flood speed increased: {GrowthRate:F2} units/sec");
        }
    }

    private void DecreaseSpeed()
    {
        GrowthRate = Mathf.Max(GrowthRate - SpeedAdjustAmount, 0.1f);

        if (EnableDebugPrint)
        {
            GD.Print($"⬇ Flood speed decreased: {GrowthRate:F2} units/sec");
        }
    }

    private void ResetWidths()
    {
        if (_originalWidths == null || _originalWidths.Count != River.PointWidths.Count)
        {
            GD.PushWarning("Cannot reset - original widths not stored or point count mismatch");
            return;
        }

        for (int i = 0; i < River.PointWidths.Count; i++)
        {
            River.PointWidths[i] = _originalWidths[i];
        }

        River.UpdateMesh();
        _isFlooding = false;

        if (EnableDebugPrint)
        {
            GD.Print("↺ River widths reset to original");
        }
    }

    private float CalculateAverageWidth()
    {
        if (River.PointWidths.Count == 0) return 0f;

        float sum = 0f;
        foreach (var width in River.PointWidths)
        {
            sum += width;
        }
        return sum / River.PointWidths.Count;
    }
}
