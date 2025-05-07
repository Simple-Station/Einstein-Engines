using System.Numerics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;

namespace Content.Client.UserInterface.Controls;

public class ResizableControl : Control
{
    public DragMode CurrentDrag = DragMode.None;
    public DragMode AllowedResizeDirection { get; set; } = DragMode.Any;
    public Vector2 DragOffsetTopLeft;
    public Vector2 DragOffsetBottomRight;
    public bool Resizable { get; set; } = true;
    //TODO: Un-hardcode this
    public const int DragMarginSize = 7;


    public ResizableControl()
    {
        MouseFilter = MouseFilterMode.Stop;
    }


    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);

        if (args.Function != EngineKeyFunctions.UIClick
            || (CurrentDrag = GetDragModeFor(args.RelativePosition)) == DragMode.None)
            return;

        if ((CurrentDrag & AllowedResizeDirection) != CurrentDrag)
        {
            CurrentDrag = DragMode.None;
            return;
        }

        DragOffsetTopLeft = args.PointerLocation.Position / UIScale - Position;
        DragOffsetBottomRight = Position + Size - args.PointerLocation.Position / UIScale;
    }

    protected override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        base.KeyBindUp(args);

        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        DragOffsetTopLeft = DragOffsetBottomRight = Vector2.Zero;
        CurrentDrag = DragMode.None;

        UserInterfaceManager.KeyboardFocused?.ReleaseKeyboardFocus();
    }

    protected override void MouseMove(GUIMouseMoveEventArgs args)
    {
        base.MouseMove(args);

        if (!Resizable)
            return;

        if (CurrentDrag == DragMode.None)
        {
            var cursor = CursorShape.Arrow;
            var previewDragMode = GetDragModeFor(args.RelativePosition);
            if ((previewDragMode & AllowedResizeDirection) == CurrentDrag)
                return;

            cursor = previewDragMode switch
            {
                DragMode.Top or DragMode.Bottom => CursorShape.VResize,
                DragMode.Left or DragMode.Right => CursorShape.HResize,
                (DragMode.Bottom | DragMode.Left) or (DragMode.Top | DragMode.Right) => CursorShape.Crosshair,
                (DragMode.Bottom | DragMode.Right) or (DragMode.Top | DragMode.Left) => CursorShape.Crosshair,
                _ => cursor,
            };

            DefaultCursorShape = cursor;
        }
        else
        {
            var (left, top) = Position;
            var (right, bottom) = Position + SetSize;

            if (float.IsNaN(SetSize.X))
                right = Position.X + Size.X;
            if (float.IsNaN(SetSize.Y))
                bottom = Position.Y + Size.Y;

            if ((CurrentDrag & DragMode.Top) == DragMode.Top)
                top = Math.Min(args.GlobalPosition.Y - DragOffsetTopLeft.Y, Math.Min(bottom, bottom - MinSize.Y));
            else if ((CurrentDrag & DragMode.Bottom) == DragMode.Bottom)
                bottom = Math.Max(args.GlobalPosition.Y + DragOffsetBottomRight.Y, Math.Max(top, top + MinSize.Y));

            if ((CurrentDrag & DragMode.Left) == DragMode.Left)
                left = Math.Min(args.GlobalPosition.X - DragOffsetTopLeft.X, Math.Min(right, right - MinSize.X));
            else if ((CurrentDrag & DragMode.Right) == DragMode.Right)
                right = Math.Max(args.GlobalPosition.X + DragOffsetBottomRight.X, Math.Max(left, left + MinSize.X));

            var rect = new UIBox2(left, top, right, bottom);
            LayoutContainer.SetPosition(this, rect.TopLeft);
            SetSize = rect.Size;
        }
    }

    protected override void MouseExited()
    {
        base.MouseExited();

        if (Resizable && CurrentDrag == DragMode.None)
            DefaultCursorShape = CursorShape.Arrow;
    }

    protected virtual DragMode GetDragModeFor(Vector2 relativeMousePos)
    {
        var mode = DragMode.None;

        if (Resizable)
        {
            if (relativeMousePos.Y < DragMarginSize)
                mode = DragMode.Top;
            else if (relativeMousePos.Y > Size.Y - DragMarginSize)
                mode = DragMode.Bottom;

            if (relativeMousePos.X < DragMarginSize)
                mode |= DragMode.Left;
            else if (relativeMousePos.X > Size.X - DragMarginSize)
                mode |= DragMode.Right;
        }

        return mode;
    }

    [Flags]
    public enum DragMode : byte
    {
        None = 0,
        Move = 1,

        Top = 1 << 1,
        Bottom = 1 << 2,
        Vertical = Top | Bottom,

        Left = 1 << 3,
        Right = 1 << 4,
        Horizontal = Left | Right,

        Any = Vertical | Horizontal,
    }
}
