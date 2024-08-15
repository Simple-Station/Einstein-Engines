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

namespace Content.Server.Authentication;

[AdminCommand(AdminFlags.Moderator)]
internal sealed class KeySetCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly IServerNetManager _netManager = default!;
    [Dependency] private readonly MVKeyAuthUtilities _mvKeyAuthUtilities = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeTrackingManager = default!;
    [Dependency] private readonly IAdminNotesManager _adminNotes = default!;

    public string Command => "keyset";
    public string Description => "Sets public key for a particular user.";
    public string Help => "Usage: keyset <user> <public key>";

    /// <summary>
    /// How long until log for this should expire?
    /// </summary> <summary>
    private const float DAYS_FOR_LOG = 90;

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteLine("Specify user and publickey.  Ex:  keyset user \"-----PUBLIC...\"");
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

        if (player == null)
        {
            shell.WriteError("Could not find that user.");
            return;
        }

        // Prevent a regular moderator from key setting another admin, since then they could log in as them.
        bool isHost = false;

        if (shell.IsLocal)
            isHost = true;
        else if (shell.Player != null)
        {
            isHost = _adminManager.HasAdminFlag(shell.Player, AdminFlags.Host);
        }

        var playerAdminData = await _db.GetAdminDataForAsync(player.UserId);
        if (playerAdminData != null && !isHost)
        {
            shell.WriteError("Insufficient priviledge level to set the key of another admin -- ask host to do so instead.");
            return;
        }

        // Log the action in case it's necessary to know later
        var logMessage = $"Public key updated by {shell.Player?.Name ?? "Console"}";
        await _adminNotes.AddAdminRemark(shell.Player ?? null, player.UserId,
            Shared.Database.NoteType.Note, logMessage, Shared.Database.NoteSeverity.None, true, DateTime.Now.AddDays(DAYS_FOR_LOG));

        await _db.UpdatePlayerRecordAsync(player.UserId, player.LastSeenUserName, player.LastSeenAddress,
            player.HWId.GetValueOrDefault(), keyBytes.Value);

        shell.WriteLine("Key updated.");
    }
}
