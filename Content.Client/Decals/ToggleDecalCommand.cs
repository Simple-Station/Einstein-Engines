﻿#region

using Robust.Shared.Console;

#endregion


namespace Content.Client.Decals;


public sealed class ToggleDecalCommand : IConsoleCommand
{
    public string Command => "toggledecals";
    public string Description => "Toggles decaloverlay";
    public string Help => $"{Command}";

    public void Execute(IConsoleShell shell, string argStr, string[] args) =>
        EntitySystem.Get<DecalSystem>().ToggleOverlay();
}
