using System.Text;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Customization.Systems;


public sealed class CharacterRequirementsSystem : EntitySystem
{
    public bool CheckRequirementsValid(List<CharacterRequirement> requirements, JobPrototype job,
        HumanoidCharacterProfile profile, Dictionary<string, TimeSpan> playTimes, bool whitelisted,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out List<FormattedMessage> reasons)
    {
        reasons = new List<FormattedMessage>();
        var valid = true;

        foreach (var requirement in requirements)
        {
            // Set valid to false if the requirement is invalid and not inverted
            // If it's inverted set valid to false when it's valid
            if (!requirement.IsValid(job, profile, playTimes, whitelisted,
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

            if (reason != null)
                reasons.Add(reason);
        }

        return valid;
    }


    /// <summary>
    ///     Gets the reason text from <see cref="CheckRequirementsValid"/> as a <see cref="FormattedMessage"/>.
    /// </summary>
    public FormattedMessage GetRequirementsText(List<FormattedMessage> reasons)
    {
        var text = new StringBuilder();
        foreach (var reason in reasons)
            text.Append($"\n{reason.ToMarkup()}");

        return FormattedMessage.FromMarkup(text.ToString().Trim());
    }

    /// <summary>
    ///     Gets the reason text from <see cref="CheckRequirementsValid"/> as a markup string.
    /// </summary>
    public string GetRequirementsMarkup(List<FormattedMessage> reasons)
    {
        var text = new StringBuilder();
        foreach (var reason in reasons)
            text.Append($"\n{reason.ToMarkup()}");

        return text.ToString().Trim();
    }
}
