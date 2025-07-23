using System.Linq;
using Content.Shared.Mind;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Customization.Systems;

/// <summary>
///     Requires the player to be a specific antagonist
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterAntagonistRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public List<ProtoId<AntagPrototype>> Antagonists;

    public override bool IsValid(JobPrototype job,
        HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        bool whitelisted,
        IPrototype prototype,
        IEntityManager entityManager,
        IPrototypeManager prototypeManager,
        IConfigurationManager configManager,
        out string? reason,
        int depth = 0,
        MindComponent? mind = null)
    {
        // Considering this will not be used in the character creation menu, players will likely never see this text.
        reason = Loc.GetString("character-antagonist-requirement", ("inverted", Inverted));

        if (mind == null)
            return false;

        foreach (var mindRoleComponent in mind.MindRoles.Select(entityManager.GetComponent<MindRoleComponent>))
        {
            if (!mindRoleComponent.AntagPrototype.HasValue)
                continue;

            if (Antagonists.Contains(mindRoleComponent.AntagPrototype.Value))
                return !Inverted;
        }

        return Inverted;
    }
}
