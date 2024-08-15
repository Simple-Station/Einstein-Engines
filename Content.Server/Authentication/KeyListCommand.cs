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

namespace Content.Server.Authentication;

[AdminCommand(AdminFlags.Moderator)]
internal sealed class KeyListCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly IServerNetManager _netManager = default!;
    [Dependency] private readonly MVKeyAuthUtilities _mvKeyAuthUtilities = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeTrackingManager = default!;

    public string Command => "keylist";
    public string Description => "Lists all users with a particular public key.";
    public string Help => "Usage: keylist <public key>";

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length == 0)
        {
            shell.WriteLine("Specify public key");
            return;
        }

        if (args.Length > 1)
        {
            shell.WriteLine("Too many parameters.  Try passing public key with quote marks surrounding it?");
            return;
        }

        // Parse key
        var keyBytes = _mvKeyAuthUtilities.UserInputtedPublicKeyToBytes(args[0]);
        if (keyBytes == null)
        {
            shell.WriteError("Invalid key.");
            return;
        }

        var records = await _db.GetAllPlayerRecordsWithPublicKey(keyBytes.Value);

        var sb = new StringBuilder();

        sb.AppendLine($"{"User Name",20} {"Last Seen",30} {"Playing Time (database)",30}");
        sb.AppendLine("-------------------------------------------------------------------------------");

        foreach (var player in records)
        {
            var playTime = (await _db.GetPlayTimes(player.UserId)).Find(p => p.Tracker == PlayTimeTrackingShared.TrackerOverall)?.TimeSpent ?? TimeSpan.Zero;

            sb.AppendLine(string.Format("{0,20} {1,30} {2,30:dd\\:hh\\:mm\\:ss}",
                player.LastSeenUserName,
                player.LastSeenTime,
                playTime));
        }

        shell.WriteLine(sb.ToString());
    }
}
