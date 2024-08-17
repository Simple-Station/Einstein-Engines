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
using Content.Server.Administration.Notes;
using Robust.Shared.AuthLib;
using System.Collections.Immutable;
using System.Net;

namespace Content.Server.Authentication;

[AdminCommand(AdminFlags.Moderator)]
internal sealed class AddUserCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly IServerNetManager _netManager = default!;
    [Dependency] private readonly MVKeyAuthUtilities _mvKeyAuthUtilities = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeTrackingManager = default!;
    [Dependency] private readonly IAdminNotesManager _adminNotes = default!;

    public string Command => "adduser";
    public string Description => "Creates a new user with public key.";
    public string Help => "Usage: adduser <user> <public key>";

    /// <summary>
    /// How long until log for this should expire?
    /// </summary> <summary>
    private const float DAYS_FOR_LOG = 90;

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteLine("Specify user and publickey.  Ex:  adduser user \"-----PUBLIC...\"");
            return;
        }

        if (args.Length > 2)
        {
            shell.WriteLine("Too many parameters.  Try passing public key with quote marks surrounding it?");
            return;
        }

        // Parse key
        var keyBytes = _mvKeyAuthUtilities.UserInputtedPublicKeyToBytes(args[1]);
        if (keyBytes == null)
        {
            shell.WriteError("Invalid key.");
            return;
        }

        var player = await _db.GetPlayerRecordByUserName(args[0]);

        if (player != null)
        {
            shell.WriteError("That user already exists.");

            // Provide some extra info so mod can understand situation
            var output = new StringBuilder();
            output.AppendLine($"FYI:  Existing user stats for {player.LastSeenUserName}:");
            var playTime = (await _db.GetPlayTimes(player.UserId)).Find(p => p.Tracker == PlayTimeTrackingShared.TrackerOverall)?.TimeSpent ?? TimeSpan.Zero;
            output.AppendLine(String.Format("Playtime: {0:dd\\:hh\\:mm\\:ss}", playTime));
            if (player.PublicKey == null)
                output.AppendLine("Public Key: none");
            else
                output.AppendLine("Public Key:" + Convert.ToBase64String(player.PublicKey.Value.ToArray()));
            output.AppendLine("UID: " + player.UserId);

            shell.WriteLine(output.ToString());
            return;
        }

        // Verify username valid
        var newUserName = args[0];
        if (!UsernameHelpers.IsNameValid(newUserName, out var reason))
        {
            shell.WriteError("Invalid username.");
            return;
        }

        // Add user entry to DB
        var userId = new NetUserId(Guid.NewGuid());
        await _db.UpdatePlayerRecordAsync(userId, newUserName, IPAddress.None, ImmutableArray<byte>.Empty, keyBytes.Value);

        shell.WriteLine("User entry added.");
    }
}
