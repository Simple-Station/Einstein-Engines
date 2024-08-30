using Content.Shared.Preferences;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Customization.Systems;


/// <summary>
///     Requires any of the requirements to be true
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterLogicOrRequirement : CharacterRequirement
{
    [DataField]
    public List<CharacterRequirement> Requirements { get; private set; } = new();

    public override bool IsValid(JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes, bool whitelisted,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason)
    {
        var charReqs = entityManager.EntitySysManager.GetEntitySystem<CharacterRequirementsSystem>();
        var succeeded = charReqs.CheckRequirementsValid(Requirements, job, profile, playTimes, whitelisted,
            entityManager, prototypeManager, configManager, out var reasons);

        if (reasons.Count == 0)
        {
            reason = null;
            return succeeded;
        }

        reason = new FormattedMessage();
        foreach (var message in reasons)
            reason.AddMessage(FormattedMessage.FromMarkup(
                Loc.GetString("character-logic-or-requirement-listprefix") + message.ToMarkup()));
        reason = FormattedMessage.FromMarkup(Loc.GetString("character-logic-or-requirement",
            ("inverted", Inverted), ("options", reason.ToMarkup())));

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

    public override bool IsValid(JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes, bool whitelisted,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason)
    {
        var reasons = new List<FormattedMessage>();
        var succeeded = false;

        foreach (var requirement in Requirements)
        {
            if (requirement.IsValid(job, profile, playTimes, whitelisted, entityManager, prototypeManager,
                configManager, out var raisin))
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

        reason = new FormattedMessage();
        foreach (var message in reasons)
            reason.AddMessage(FormattedMessage.FromMarkup(
                Loc.GetString("character-logic-xor-requirement-listprefix") + message.ToMarkup()));
        reason = FormattedMessage.FromMarkup(Loc.GetString("character-logic-xor-requirement",
            ("inverted", Inverted), ("options", reason.ToMarkup())));

        return succeeded;
    }
}
