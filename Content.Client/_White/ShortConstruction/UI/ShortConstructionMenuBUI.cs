using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.Input;

// ReSharper disable InconsistentNaming

namespace Content.Client._White.ShortConstruction.UI;

[UsedImplicitly]
public sealed class ShortConstructionMenuBUI(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [Dependency] private readonly IClyde _displayManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    private ShortConstructionMenu? _menu;

    protected override void Open()
    {
        _menu = new ShortConstructionMenu(Owner);
        _menu.OnClose += Close;

        var vpSize = _displayManager.ScreenSize;
        _menu.OpenCenteredAt(_inputManager.MouseScreenPosition.Position / vpSize);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        _menu?.Dispose();
    }
}
