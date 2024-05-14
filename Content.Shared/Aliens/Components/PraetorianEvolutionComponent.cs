using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Aliens.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class PraetorianEvolutionComponent : Component
{
    [DataField]
    public ProtoId<PolymorphPrototype> PraetorianPolymorphPrototype = "AlienEvolutionPraetorian";

    [DataField("praetorianEvolutionAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? PraetorianEvolutionAction = "ActionEvolvePraetorian";

    [DataField("praetorianEvolutionActionEntity")]
    public EntityUid? PraetorianEvolutionActionEntity;

    [DataField("plasmaCost")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float PlasmaCost = 150f;
}
