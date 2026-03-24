using Content.Shared.Preferences.Loadouts.Effects;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Preferences;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;

namespace Content.Shared._Orehum.Loadouts;

/// <summary>
/// Checks for a specific player GUID.
/// </summary>
public sealed partial class PlayerGUIDLoadoutEffect : LoadoutEffect
{
    [DataField(required: true)]
    public string Guid;

    private Guid? _guid;

    public override bool Validate(HumanoidCharacterProfile profile, RoleLoadout loadout, ICommonSession? session, IDependencyCollection collection, [NotNullWhen(false)] out FormattedMessage? reason)
    {
        if (session == null)
        {
            reason = FormattedMessage.Empty;
            return false;
        }

        _guid ??= new Guid(Guid);

        if (session.UserId == _guid)
        {
            reason = null;
            return true;
        }
        reason = FormattedMessage.FromUnformatted(Loc.GetString("loadout-group-player-restriction"));
        return false;
    }
}
