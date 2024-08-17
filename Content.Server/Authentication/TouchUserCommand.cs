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
using Content.Server.Database;
using System.Collections.Immutable;

namespace Content.Server.Authentication;

[AdminCommand(AdminFlags.Moderator)]
internal sealed class TouchUserCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly IServerNetManager _netManager = default!;
    [Dependency] private readonly MVKeyAuthUtilities _mvKeyAuthUtilities = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IAdminNotesManager _adminNotes = default!;
    [Dependency] private readonly IServerDbManager _db = default!;

    public string Command => "touchuser";
    public string Description => "Sets username's last seen time to now.  (Useful if user has multiple public keys to prioritize one)";
    public string Help => "Usage: touchuser <username>";

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteLine("Specify exactly one user name.  Ex:  touchuser CoolPersonHere");
            return;
        }

        // Get data on username
        var username = args[0];
        var sourceUsernameLookupRecord = await _db.GetPlayerRecordByUserName(username);
        if (sourceUsernameLookupRecord == null)
        {
            shell.WriteError($"Username {username} not found.");
            return;
        }

        // Write data right back out to reset the last seen time

        await _db.UpdatePlayerRecordAsync(
            sourceUsernameLookupRecord.UserId,
            sourceUsernameLookupRecord.LastSeenUserName,
            sourceUsernameLookupRecord.LastSeenAddress,
            sourceUsernameLookupRecord.HWId ?? ImmutableArray<byte>.Empty,
            sourceUsernameLookupRecord.PublicKey ?? ImmutableArray<byte>.Empty);

        shell.WriteLine("Done.");
    }
}
