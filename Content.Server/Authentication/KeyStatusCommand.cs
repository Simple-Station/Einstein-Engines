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
using Content.Server.Database;
using System.Security.Cryptography;
using Content.Server.Players.PlayTimeTracking;
using Content.Shared.Players.PlayTimeTracking;
using System.Buffers.Text;

namespace Content.Server.Authentication;

[AdminCommand(AdminFlags.Moderator)]
internal sealed class KeyStatusCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly IServerNetManager _netManager = default!;
    [Dependency] private readonly MVKeyAuthUtilities _mvKeyAuthUtilities = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeTrackingManager = default!;

    public string Command => "keystatus";
    public string Description => "Checks user to see if they have a public key set.";
    public string Help => "Usage: keystatus <username>";

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteLine("Specify username");
            return;
        }

        var record = await _db.GetPlayerRecordByUserName(args[0]);

        if (record == null)
        {
            shell.WriteError("No such user.");
            return;
        }

        if (record.PublicKey == null || record.PublicKey.Value.Length == 0)
        {
            shell.WriteLine("User has no public key.");
            return;
        }

        shell.WriteLine("User has public key:  " + Convert.ToBase64String(record.PublicKey.Value.ToArray()));
    }
}
