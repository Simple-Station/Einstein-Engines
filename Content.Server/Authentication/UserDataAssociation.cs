using System.Collections.Immutable;
using System.Threading.Tasks;
using Content.Server.Database;
using Robust.Shared.Network;
using static Robust.Shared.Network.IServerUserDataAssociation;

namespace Content.Server.Authentication;

/// <summary>
/// Determines which user a public key should map to, or otherwise creates such an entry.
/// Can do migration from WizDen => MV Auth as well.
/// </summary>
public sealed class UserDataAssociation : IServerUserDataAssociation
{
    [Dependency] private readonly IServerDbManager _db = default!;

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

        // TODO - Allow server to optionally attempt to associate/migrate user account if history of HWID/Username/IP/whatever

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
}
