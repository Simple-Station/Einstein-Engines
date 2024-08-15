using System.Collections.Immutable;
using System.Threading.Tasks;
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

    private ISawmill _logger = default!;

    public async Task<AssociationResult> AttemptUserDataFromPublicKey(
        ImmutableArray<byte> publicKey, ImmutableArray<byte> hWId, string requestedUserName)
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

        // TODO - Throttle connections here to prevent a nefarious person from flooding user table with keys
        // or trying to claim old accounts during migration.

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
                        _logger.Info($"Auto-migrating user account for {matchingExistingPlayerRecord.LastSeenUserName} -> {requestedUserName}.");

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
