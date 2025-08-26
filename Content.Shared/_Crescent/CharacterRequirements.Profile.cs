using System.Linq;
using Content.Shared.Clothing.Loadouts.Prototypes;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Mind;
using Content.Shared.Preferences;
using Content.Shared.Prototypes;
using Content.Shared.Roles;
using Content.Shared.Traits;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Customization.Systems;

public sealed partial class FactionRequirement : CharacterRequirement
{
    [DataField("factionID")] public string FactionID = "";

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
        if (profile.Faction == FactionID)
        {
            reason = null;
            return true;
        }

        if (Inverted)
            reason = $"Your faction must NOT be {FactionID}.";
        else
            reason = $"Your faction must be {FactionID}.";
        return false;
    }

}

[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class WealthRequirement : CharacterRequirement
{
    [DataField("below")] public int below = int.MaxValue;
    [DataField("above")] public int above = 0;

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
        if (profile.BankBalance > above || profile.BankBalance < below)
        {
            reason = $"Your bank balance must be between {below} and {above} !";
            return false;
        }

        reason = null;
        return true;
    }
}
