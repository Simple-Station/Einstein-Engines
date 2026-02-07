using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Sandboxing;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Content.Client._White.UserInterface;


/// <summary>
/// Used for buttons that open a window when pressed, and, if pressed again, close that window instead of opening another one.
/// This kills the boilerplate.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class WindowTracker<T> where T : BaseWindow, new()
{
    public T? Window { get; private set; }
    public bool IsOpen => Window is not null;

    private static ISandboxHelper? _sandbox = null;

    [MemberNotNullWhen(true, nameof(Window))]
    private bool Toggle()
    {
        _sandbox ??= IoCManager.Resolve<ISandboxHelper>();
        if (Window is not null)
        {
            Window.Close();
            return false;
        }
        Window = (T) _sandbox.CreateInstance(typeof(T));
        Window.OnClose += () => Window = null;
        return true;
    }

    public bool Close()
    {
        if (Window is null)
            return false;
        Window.Close();
        return true;
    }

    public bool TryOpen()
    {
        if (!Toggle())
            return false;
        Window.Open();
        return true;
    }

    public bool TryOpenCentered()
    {
        if (!Toggle())
            return false;
        Window.OpenCentered();
        return true;
    }

    public bool TryOpenCenteredAt(Vector2 pos)
    {
        if (!Toggle())
            return false;
        Window.OpenCenteredAt(pos);
        return true;
    }

    public bool TryOpenCenteredLeft()
    {
        if (!Toggle())
            return false;
        Window.OpenCenteredLeft();
        return true;
    }

    public bool TryOpenCenteredRight()
    {
        if (!Toggle())
            return false;
        Window.OpenCenteredRight();
        return true;
    }

    // why the fuck does it take clyde as an arg
    //public bool TryOpenScreenAt()
    //{
    //    if (!Toggle())
    //        return false;
    //    Window.OpenScreenAt();
    //    return true;
    //}

    public bool TryOpenToLeft()
    {
        if (!Toggle())
            return false;
        Window.OpenToLeft();
        return true;
    }

    public bool TryOpenToRight()
    {
        if (!Toggle())
            return false;
        Window.OpenToRight();
        return true;
    }
}

