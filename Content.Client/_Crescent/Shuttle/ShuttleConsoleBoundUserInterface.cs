using Content.Client.Shuttles.UI;
using Content.Shared.NamedModules.Components;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Events;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Map;

namespace Content.Client.Shuttles.BUI;

public sealed partial class ShuttleConsoleBoundUserInterface : BoundUserInterface
{
    private void HullrotOpen()
    {
        _window ??= new ShuttleConsoleWindow();
        _window.NavScreen.modulePressed += (index) => SendMessage(new NavConsoleGroupPressedMessage(index));
        _window.NavScreen.OnRename += (args) => SendMessage(new ModuleNamingChangeEvent(args));
    }
}
