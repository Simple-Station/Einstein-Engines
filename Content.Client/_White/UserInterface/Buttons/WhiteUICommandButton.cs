using Robust.Client.UserInterface.CustomControls;

namespace Content.Client._White.UserInterface.Buttons;

public sealed class WhiteUICommandButton : WhiteCommandButton
{
    public Type? WindowType { get; set; }
    private DefaultWindow? _window;

    protected override void Execute(ButtonEventArgs obj)
    {
        if (WindowType == null)
            return;

        var windowInstance = IoCManager.Resolve<IDynamicTypeFactory>().CreateInstance(WindowType);
        if (windowInstance is not DefaultWindow window)
            return;

        _window = window;
        _window.OpenCentered();
    }
}
