using System.Linq;
using Content.Shared._EE.Contractors.Prototypes;
using Content.Shared.CCVar;
using Content.Shared.Clothing.Loadouts.Prototypes;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Mind;
using Content.Shared.Preferences;
using Content.Shared.Prototypes;
using Content.Shared.Roles;
using Content.Shared.Traits;
using JetBrains.Annotations;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Customization.Systems;

/// <summary>
///     Requires the profile to have one of a list of nationalities
/// </summary>
[UsedImplicitly, Serializable, NetSerializable]
public sealed partial class CharacterNationalityRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public HashSet<ProtoId<NationalityPrototype>> Nationalities;

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
        int depth = 0,
        MindComponent? mind = null
    )
    {
        if (!configManager.GetCVar(CCVars.ContractorsEnabled) ||
            !configManager.GetCVar(CCVars.ContractorsCharacterRequirementsEnabled))
        {
            reason = "";
            return true;
        }

        var localeString = "character-nationality-requirement";
        const string color = "green";
        reason = Loc.GetString(
            localeString,
            ("inverted", Inverted),
            ("nationality", $"[color={color}]{string.Join($"[/color], [color={color}]",
                Nationalities.Select(s => Loc.GetString(prototypeManager.Index(s).NameKey)))}[/color]"));
        return Nationalities.Any(o => o == profile.Nationality);
    }
}

/// <summary>
///     Requires the profile to have one of a list of employers
/// </summary>
[UsedImplicitly, Serializable, NetSerializable]
public sealed partial class CharacterEmployerRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public HashSet<ProtoId<EmployerPrototype>> Employers;

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
        int depth = 0,
        MindComponent? mind = null
    )
    {
        if (!configManager.GetCVar(CCVars.ContractorsEnabled) ||
            !configManager.GetCVar(CCVars.ContractorsCharacterRequirementsEnabled))
        {
            reason = "";
            return true;
        }

        var localeString = "character-employer-requirement";
        const string color = "green";
        reason = Loc.GetString(
            localeString,
            ("inverted", Inverted),
            ("employers", $"[color={color}]{string.Join($"[/color], [color={color}]",
                Employers.Select(s => Loc.GetString(prototypeManager.Index(s).NameKey)))}[/color]"));
        return Employers.Any(o => o == profile.Employer);
    }
}

/// <summary>
///     Requires the profile to have one of a list of lifepaths
/// </summary>
[UsedImplicitly, Serializable, NetSerializable]
public sealed partial class CharacterLifepathRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public HashSet<ProtoId<LifepathPrototype>> Lifepaths;

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
        int depth = 0,
        MindComponent? mind = null
    )
    {
        if (!configManager.GetCVar(CCVars.ContractorsEnabled) ||
            !configManager.GetCVar(CCVars.ContractorsCharacterRequirementsEnabled))
        {
            reason = "";
            return true;
        }

        var localeString = "character-lifepath-requirement";
        const string color = "green";
        reason = Loc.GetString(
            localeString,
            ("inverted", Inverted),
            ("lifepaths", $"[color={color}]{string.Join($"[/color], [color={color}]",
                Lifepaths.Select(s => Loc.GetString(prototypeManager.Index(s).NameKey)))}[/color]"));
        return Lifepaths.Any(o => o == profile.Lifepath);
    }
}
