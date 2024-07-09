using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Customization.Systems;


public sealed class CharacterRequirementsSystem : EntitySystem
{
    public bool CheckRequirementsValid(IPrototype prototype, List<CharacterRequirement> requirements, JobPrototype job,
        HumanoidCharacterProfile profile, Dictionary<string, TimeSpan> playTimes,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out List<FormattedMessage> reasons)
    {
        reasons = new List<FormattedMessage>();
        var valid = true;

        foreach (var requirement in requirements)
        {
            // Set valid to false if the requirement is invalid and not inverted
            // If it's inverted set valid to false when it's valid
            if (!requirement.IsValid(prototype, job, profile, playTimes,
                entityManager, prototypeManager, configManager,
                out var reason))
            {
                if (valid)
                    valid = requirement.Inverted;
            }
            else
            {
                if (valid)
                    valid = !requirement.Inverted;
            }

            if (reason != null) // To appease the compiler
                reasons.Add(reason);
        }

        return valid;
    }
}
