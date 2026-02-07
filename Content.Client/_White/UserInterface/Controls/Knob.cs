using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.Input;
using Robust.Shared.Timing;
using Range = Robust.Client.UserInterface.Controls.Range;

namespace Content.Client._White.UserInterface.Controls;

//TODO: Add some style thinks
public sealed class Knob : Range
{
    private readonly IGameTiming _gameTiming;

    public event Action<Knob>? OnGrabbed;
    public event Action<Knob>? OnReleased;

    private bool _grabbed;

    public bool Grabbed => _grabbed;

    public float AngleMultiplier { get; set; } = 1f;

    public Angle StartAngle { get; set; }

    public Angle Angle => StartAngle + new Angle(GetAsRatio() * Math.PI * AngleMultiplier);

    public bool Disabled { get; set; }

    public Knob()
    {
        _gameTiming = IoCManager.Resolve<IGameTiming>();
        MouseFilter = MouseFilterMode.Stop;
        MinSize = new Vector2(64, 64);
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        var center = Position + new Vector2(Width / 2, Height / 2);

        var radius = float.Min(Width, Height) / 2;

        const int segments = 8;

        for (var i = 0; i <= segments; i++)
        {
            var angle = i / (float) segments * MathHelper.Pi + MathHelper.PiOver2;
            var pos = new Vector2(MathF.Sin(angle), MathF.Cos(angle));

            handle.DrawLine(center, center + pos*radius, Color.White);
        }

        handle.DrawCircle(center, radius*0.8f, Color.FromHex("#424245"));

        handle.DrawCircle(center, radius*0.6f, Color.FromHex("#757585"));

        var newMatrix = Matrix3x2.Identity * Matrix3x2.CreateRotation((float) Angle.Theta) * Matrix3x2.CreateTranslation(center);

        var len = 2;

        handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, new List<Vector2>()
        {
            Vector2.Transform(new Vector2(-radius*0.75f, -len*4), newMatrix),
            Vector2.Transform(new Vector2(7, -len*4), newMatrix),
            Vector2.Transform(new Vector2(9, -len), newMatrix),
            Vector2.Transform(new Vector2(9, len), newMatrix),
            Vector2.Transform(new Vector2(7, len*4), newMatrix),
            Vector2.Transform(new Vector2(-radius*0.75f, len*4), newMatrix),
            Vector2.Transform(new Vector2(-radius*0.8f, 0), newMatrix),
        }, Color.FromHex("#858585"));

        handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, new List<Vector2>()
        {
            Vector2.Transform(new Vector2(-radius*0.85f, -len), newMatrix),
            Vector2.Transform(new Vector2(5, -len), newMatrix),
            Vector2.Transform(new Vector2(6, 0), newMatrix),
            Vector2.Transform(new Vector2(5, len), newMatrix),
            Vector2.Transform(new Vector2(-radius*0.85f, len), newMatrix),
            Vector2.Transform(new Vector2(-radius*0.9f, 0), newMatrix),
        }, Color.White);
    }

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);

        if (args.Function != EngineKeyFunctions.UIClick || Disabled)
            return;

        _grabbed = true;
        OnGrabbed?.Invoke(this);
    }

    protected override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        base.KeyBindUp(args);

        if (args.Function != EngineKeyFunctions.UIClick || !_grabbed)
            return;

        _grabbed = false;
        OnReleased?.Invoke(this);
    }

    protected override void MouseMove(GUIMouseMoveEventArgs args)
    {
        base.MouseMove(args);

        if (Disabled)
            return;

        if (!_grabbed)
            return;

        var ratio = (args.Relative.X - args.Relative.Y) * 0.01f;
        SetAsRatio(GetAsRatio() + ratio);
    }

    private TimeSpan? _lastMouseWheelTime;

    protected override void MouseWheel(GUIMouseWheelEventArgs args)
    {
        base.MouseWheel(args);

        if (Disabled)
        {
            args.Handle();
            return;
        }

        _lastMouseWheelTime = _gameTiming.CurTime;

        var ratio = args.Delta.Y * 0.05f;
        SetAsRatio(GetAsRatio() + ratio);

        args.Handle();
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if(!_lastMouseWheelTime.HasValue || _lastMouseWheelTime.Value + TimeSpan.FromMilliseconds(100) > _gameTiming.CurTime)
            return;

        OnReleased?.Invoke(this);

        _lastMouseWheelTime = null;
    }
}
