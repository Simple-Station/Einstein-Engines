using Content.Shared.Customization.Systems;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared.Customization;

/// <summary>
/// Collection of job, antag, and ghost-role job requirements for per-server requirement overrides.
/// </summary>
[Prototype]
public sealed partial class CharacterRequirementOverridePrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public Dictionary<ProtoId<JobPrototype>, List<CharacterRequirement>> Jobs = new ();

    [DataField]
    public Dictionary<ProtoId<AntagPrototype>, List<CharacterRequirement>> Antags = new ();
}
