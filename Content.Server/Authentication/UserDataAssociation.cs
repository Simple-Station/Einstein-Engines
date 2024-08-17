using System.Collections.Immutable;
using System.Net;
using System.Threading.Tasks;
using Content.Server.Administration.Notes;
using Content.Server.Connection;
using Content.Server.Database;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using static Robust.Shared.Network.IServerUserDataAssociation;

namespace Content.Server.Authentication;

/// <summary>
/// Determines which user a public key should map to, or otherwise creates such an entry.
/// Can do migration from WizDen => MV Auth as well.
/// </summary>
public sealed class UserDataAssociation : IServerUserDataAssociation, IPostInjectInit
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly ILogManager _logMan = default!;
    [Dependency] private readonly IAdminNotesManager _adminNotes = default!;
    [Dependency] private readonly IIPInformation _ipInformation = default!;

    private ISawmill _logger = default!;

    public async Task<AssociationResult> AttemptUserDataFromPublicKey(
        ImmutableArray<byte> publicKey, ImmutableArray<byte> hWId, string requestedUserName, IPAddress connectingAddress)
    {
        // Check if public key already has a match in database
        var existingPlayerRecord = await _db.GetPlayerRecordByPublicKey(publicKey);
        if (existingPlayerRecord != null)
        {
            // Already exists, so just use that:
            var userId = new NetUserId(existingPlayerRecord.UserId);
            var userData = new NetUserData(userId, existingPlayerRecord.LastSeenUserName) // (Currently does not allow user to change name.)
            {
                HWId = hWId,
                PublicKey = publicKey
            };

            return new AssociationResult(true, userData);
        }

        // Beyond this point, the public key will either need a new user, or to associate with an existing user.
        // Throttle to prevent a nefarious person from flooding user table with new keys
        // or trying to claim old accounts during migration.
        var floodCheckMaxAccounts = _cfg.GetCVar(CCVars.AuthLimitNewPublicKeysFromIPCount);
        var floodCheckDays = _cfg.GetCVar(CCVars.AuthLimitNewPublicKeysFromIPForDays);

        if (floodCheckDays > 0 && floodCheckMaxAccounts > 0)
        {
            int recentPublicKeyLoginsFromThisIP = await _db.GetCountOfRecentlyUsedPlayerRecordsWithPublicKeyFromIP(
                connectingAddress, floodCheckDays);

            if (recentPublicKeyLoginsFromThisIP > floodCheckMaxAccounts)
            {
                _logger.Info($"Blocking connection from {connectingAddress} / {requestedUserName} due to new account flood check.");
                return new AssociationResult(false, null, "Too many accounts from your IP.  Try one of these:\n1) Use your existing public key if you have one.\n2) Or you may also contact server staff with your public key to be added manually.\n   (Please inform them if your account needs migrating or if you are a new player.)\n3) Alternatively, if the server supports it, you could try guest mode.");
            }
        }

        // Block VPN connections to make it harder to get past flood checking
        var ipResult = await _ipInformation.GetIPInformationAsync(connectingAddress);
        _logger.Debug($"Connection from IP {connectingAddress} is suspicious level: {ipResult.suspiciousScore}");

        if (ipResult.suspiciousIP)
        {
            _logger.Info($"Blocking connection from {connectingAddress} / {requestedUserName} due to VPN IP creating or migrating new account.");
            return new AssociationResult(false, null, "VPN BLOCKED\nHello, it appears you are connecting from a VPN and this server currently blocks VPN connections.  Please use a\nresidential/home IP.  You may also request a whitelist from server staff via the website if you have a good record\non another server or well established furry profile.\nPlease include your requested username and the public key from the launcher\n(visible in account drop down menu.)\nhttps://blepstation.com/"); // I don't know how to get newlines working in FTL/loc
        }

        // Allow server to optionally attempt to associate/migrate user account if history of HWID/Username/IP/whatever
        if (_cfg.GetCVar(CCVars.AuthMigrationViaHwid))
        {
            // Is there an old HWID known?
            var matchingExistingPlayerRecord = await _db.GetPlayerRecordByHWID(hWId);
            if (matchingExistingPlayerRecord != null)
            {
                // Has that HWID already been claimed by another public key?
                if (!matchingExistingPlayerRecord.PublicKey.HasValue || matchingExistingPlayerRecord.PublicKey.Value.Length == 0)
                {
                    // No public key set, so matching can be done.  However, for safety, it's best not to match an admin
                    // privledged account automatically.  HWID's aren't secret, after all.

                    var matchingExistingAdminData = await _db.GetAdminDataForAsync(matchingExistingPlayerRecord.UserId);
                    if (matchingExistingAdminData == null)
                    {
                        // Not an admin, safe to associate account
                        var logMessage = $"Auto-migrating user account from Account Auth to Key Auth:  {matchingExistingPlayerRecord.LastSeenUserName} -> {requestedUserName}.";
                        _logger.Info(logMessage);

                        // Might be useful to have this in the user log in case account was highjacked
                        await _adminNotes.AddAdminRemark(null, matchingExistingPlayerRecord.UserId,
                            Shared.Database.NoteType.Note, logMessage, Shared.Database.NoteSeverity.None, true, null);

                        // Login as the existing user
                        // New public key will be added by DB on next commit
                        var userId = new NetUserId(matchingExistingPlayerRecord.UserId);
                        var userData = new NetUserData(userId, matchingExistingPlayerRecord.LastSeenUserName)
                        {
                            HWId = hWId,
                            PublicKey = publicKey
                        };

                        return new AssociationResult(true, userData);
                    } else {
                        _logger.Info($"Not auto-migrating user account for {matchingExistingPlayerRecord.LastSeenUserName} -> {requestedUserName} since they're an admin.");
                    }
                }
            }
        }

        // Try creating a new association if username isn't already taken
        var desiredUsernameLookupRecord = await _db.GetPlayerRecordByUserName(requestedUserName);
        if (desiredUsernameLookupRecord == null)
        {
            // No one else is using it, so let player have it
            // TODO - rate limit

            var userId = new NetUserId(Guid.NewGuid());

            // It is incredibly unlikely that a guid colission will occur, but given the consequences, we check.
            var safetyCheckUserIDLookup = await _db.GetPlayerRecordByUserId(userId);
            if (safetyCheckUserIDLookup != null)
            {
                // Wow.
                return new AssociationResult(false, null, "Please retry."); // Just bailing out to keep code simple
            }

            // This will create a new user in the database (no need for this function to manually do it)
            var userData = new NetUserData(userId, requestedUserName) // (Currently does not allow user to change name.)
            {
                HWId = hWId,
                PublicKey = publicKey
            };
            return new AssociationResult(true, userData);
        }

        // Username is already taken.
        // TODO - Maybe do .001 etc?

        return new AssociationResult(false, null, "That username is already taken here, please use another."); // Failed
    }

    void IPostInjectInit.PostInject()
    {
        _logger = _logMan.GetSawmill("auth");
    }
}
