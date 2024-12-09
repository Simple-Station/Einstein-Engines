using Content.Shared.Customization.Systems;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Customization.Systems;

/// <summary>
///     Requires the server to have a specific CVar value. 
/// </summary>
[UsedImplicitly, Serializable, NetSerializable,]
public sealed partial class CVarRequirement : CharacterRequirement
{
    [DataField("cvar", required: true)]
    public string CVar;

    [DataField(required: true)]
    public string RequiredValue;

    public override bool IsValid(
        JobPrototype job,
        HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        bool whitelisted,
        IPrototype prototype,
        IEntityManager entityManager,
        IPrototypeManager prototypeManager,
        IConfigurationManager configManager,
        out string? reason,
        int depth = 0
    )
    {
        if (!configManager.IsCVarRegistered(CVar))
        {
            reason = null;
            return true;
        }

        const string color = "lightblue";
        var cvar = configManager.GetCVar(CVar);
        var isValid = cvar.ToString()! == RequiredValue;

        reason = Loc.GetString(
            "character-cvar-requirement",
            ("inverted", Inverted),
            ("color", color),
            ("cvar", CVar),
            ("value", RequiredValue));

        return isValid;
    }
}
