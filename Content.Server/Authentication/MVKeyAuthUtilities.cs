using System.Collections.Immutable;
using System.Threading.Tasks;
using Content.Server.Administration.Notes;
using Content.Server.Database;
using Content.Shared.CCVar;
using Robust.Server.Player;
using Robust.Shared.AuthLib;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using static Robust.Shared.Network.IServerUserDataAssociation;

namespace Content.Server.Authentication;

/// <summary>
/// Various tools to handle managing usernames with Key Auth
/// </summary>
public sealed class MVKeyAuthUtilities : IPostInjectInit
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly ILogManager _logMan = default!;
    [Dependency] private readonly IAdminNotesManager _adminNotes = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IServerNetManager _netManager = default!;

    private ISawmill _logger = default!;

    void IPostInjectInit.PostInject()
    {
        _logger = _logMan.GetSawmill("auth");
    }

    public struct UtilityResult
    {
        public UtilityResult(bool success, string message)
        {
            this.Success = success;
            this.Message = message;
        }
        public bool Success;
        public string Message;
    }

    public async Task<UtilityResult> AttemptRenameUser(string oldUserName, string newUserName, bool requestorCanModifyAdmin)
    {
        // Verify username rules
        if (!UsernameHelpers.IsNameValid(newUserName, out var reason))
        {
            return new UtilityResult(false, $"Invalid destination username '{newUserName}' due to: {reason}.");
        }

        // Is new username already in use by someone else?
        var destinationUsernameLookupRecord = await _db.GetPlayerRecordByUserName(newUserName);
        if (destinationUsernameLookupRecord != null)
        {
            return new UtilityResult(false, $"New username {newUserName} already in use.");
        }

        // Get data on old username
        var sourceUsernameLookupRecord = await _db.GetPlayerRecordByUserName(oldUserName);
        if (sourceUsernameLookupRecord == null)
        {
            return new UtilityResult(false, $"Old username {oldUserName} not found.");
        }

        // Check if old user is an admin account -- this requires higher privledges
        var matchingExistingAdminData = await _db.GetAdminDataForAsync(sourceUsernameLookupRecord.UserId);
        if (matchingExistingAdminData != null && !requestorCanModifyAdmin)
        {
            return new UtilityResult(false, $"You are trying to modify another admin account {oldUserName} but you don't have requisite permission.");
        }

        // If old username player is currently playing, kick them from server so they get new account info
        if (_player.TryGetSessionById(sourceUsernameLookupRecord.UserId, out var activeSession))
        {
            _netManager.DisconnectChannel(activeSession.Channel, $"Please reconnect, an admin renamed your UserName to {newUserName}");
        }

        try
        {
            await _db.UpdatePlayerRecordAsync(sourceUsernameLookupRecord.UserId,
                newUserName,
                sourceUsernameLookupRecord.LastSeenAddress,
                sourceUsernameLookupRecord.HWId ?? ImmutableArray<byte>.Empty,
                sourceUsernameLookupRecord.PublicKey ?? ImmutableArray<byte>.Empty);
        } catch (Exception e) {
            _logger.Error("Exception in AttemptRenameUser: ", e);
            return new UtilityResult(false, "Exception.");
        }

        return new UtilityResult(true, $"Successfully renamed '{oldUserName}' -> '{newUserName}'.");
    }
}
