using System.Linq;
using System.Text;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Customization.Systems;


/// <summary>
///    Requires all of the requirements to be true
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterLogicAndRequirement : CharacterRequirement
{
    [DataField]
    public List<CharacterRequirement> Requirements { get; private set; } = new();

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
        var succeeded = entityManager.EntitySysManager.GetEntitySystem<CharacterRequirementsSystem>()
            .CheckRequirementsValid(Requirements, job, profile, playTimes, whitelisted, prototype, entityManager,
                prototypeManager, configManager, out var reasons, depth + 1);

        if (reasons.Count == 0)
        {
            reason = null;
            return succeeded;
        }

        var reasonBuilder = new StringBuilder();
        foreach (var message in reasons)
            reasonBuilder.Append(Loc.GetString("character-logic-and-requirement-listprefix",
                ("indent", new string(' ', depth * 2))) + message);
        reason = Loc.GetString("character-logic-and-requirement",
            ("inverted", Inverted), ("options", reasonBuilder.ToString()));

        return succeeded;
    }
}

/// <summary>
///     Requires any of the requirements to be true
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterLogicOrRequirement : CharacterRequirement
{
    [DataField]
    public List<CharacterRequirement> Requirements { get; private set; } = new();

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
        var succeeded = false;
        var reasons = new List<string>();
        var characterRequirements = entityManager.EntitySysManager.GetEntitySystem<CharacterRequirementsSystem>();

        foreach (var requirement in Requirements)
        {
            if (characterRequirements.CheckRequirementValid(requirement, job, profile, playTimes, whitelisted, prototype,
                entityManager, prototypeManager, configManager, out var raisin, depth + 1))
            {
                succeeded = true;
                break;
            }

            if (raisin != null)
                reasons.Add(raisin);
        }

        if (reasons.Count == 0)
        {
            reason = null;
            return succeeded;
        }

        var reasonBuilder = new StringBuilder();
        foreach (var message in reasons)
            reasonBuilder.Append(Loc.GetString("character-logic-or-requirement-listprefix",
                ("indent", new string(' ', depth * 2))) + message);
        reason = Loc.GetString("character-logic-or-requirement",
            ("inverted", Inverted), ("options", reasonBuilder.ToString()));

        return succeeded;
    }
}

/// <summary>
///     Requires only one of the requirements to be true
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterLogicXorRequirement : CharacterRequirement
{
    [DataField]
    public List<CharacterRequirement> Requirements { get; private set; } = new();

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
        var reasons = new List<string>();
        var succeeded = false;
        var characterRequirements = entityManager.EntitySysManager.GetEntitySystem<CharacterRequirementsSystem>();

        foreach (var requirement in Requirements)
        {
            if (characterRequirements.CheckRequirementValid(requirement, job, profile, playTimes, whitelisted, prototype,
                entityManager, prototypeManager, configManager, out var raisin, depth + 1))
            {
                if (succeeded)
                {
                    succeeded = false;
                    break;
                }

                succeeded = true;
            }

            if (raisin != null)
                reasons.Add(raisin);
        }

        if (reasons.Count == 0)
        {
            reason = null;
            return succeeded;
        }

        var reasonBuilder = new StringBuilder();
        foreach (var message in reasons)
            reasonBuilder.Append(Loc.GetString("character-logic-xor-requirement-listprefix",
                ("indent", new string(' ', depth * 2))) + message);
        reason = Loc.GetString("character-logic-xor-requirement",
            ("inverted", Inverted), ("options", reasonBuilder.ToString()));

        return succeeded;
    }
}
