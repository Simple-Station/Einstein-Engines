using System;
using System.Linq;
using System.Text;
using Content.Server.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.IoC;
using Robust.Shared.Network;
using Content.Server.Storage.Components;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Content.Server.Administration.Managers;
using Content.Server.Administration.Notes;

namespace Content.Server.Authentication;

[AdminCommand(AdminFlags.Moderator)]
internal sealed class RenameUserCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly IServerNetManager _netManager = default!;
    [Dependency] private readonly MVKeyAuthUtilities _mvKeyAuthUtilities = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IAdminNotesManager _adminNotes = default!;

    public string Command => "renameuser";
    public string Description => "Renames the username to the desired new one.  It safely checks to make sure new one is not already in use.";
    public string Help => "Usage: renameuser <old username> <new username>";

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteLine("Specify both old user name and new user name.  Ex:  renameuser OldyMcOldson NewMcNewson");
            return;
        }

        // This command prevents a regular moderator from renaming a host or other moderator.
        bool isHost = false;

        if (shell.IsLocal)
            isHost = true;
        else if (shell.Player != null)
        {
            isHost = _adminManager.HasAdminFlag(shell.Player, AdminFlags.Host);
        }

        var result = await _mvKeyAuthUtilities.AttemptRenameUser(args[0], args[1], isHost,
            shell.Player,
            shell.Player?.Name ?? "Console");

        if (result.Success)
            shell.WriteLine(result.Message);
        else
            shell.WriteError("Failed:  " + result.Message);
    }
}
