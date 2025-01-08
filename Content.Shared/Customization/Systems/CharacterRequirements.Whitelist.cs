using Content.Shared.CCVar;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using Robust.Shared.Network;

namespace Content.Shared.Customization.Systems;


/// <summary>
///     Requires the player to be whitelisted if whitelists are enabled
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterWhitelistRequirement : CharacterRequirement
{
    public override bool IsValid(JobPrototype job,
        HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        bool whitelisted,
        IPrototype prototype,
        IEntityManager entityManager,
        IPrototypeManager prototypeManager,
        IConfigurationManager configManager,
        out string? reason,
        int depth = 0)
    {
        reason = Loc.GetString("character-whitelist-requirement", ("inverted", Inverted));
        return !configManager.GetCVar(CCVars.WhitelistEnabled) || whitelisted;
    }
}
