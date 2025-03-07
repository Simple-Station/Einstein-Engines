using System.Linq;
using Content.Shared.Implants.Components;
using Content.Shared.Mind;
using Content.Shared.Mindshield.Components;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Customization.Systems;

/// <summary>
///     Requires the player to have a mindshield
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterMindshieldRequirement : CharacterRequirement
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
        int depth = 0,
        MindComponent? mind = null)
    {
        reason = Loc.GetString("character-mindshield-requirement", ("inverted", Inverted));

        if (mind == null)
            return false;

        return entityManager.HasComponent<MindShieldComponent>(mind.CurrentEntity) != Inverted;
    }
}
