using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Aliens.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class QueenEvolutionComponent : Component
{
    [DataField]
    public ProtoId<PolymorphPrototype> QueenPolymorphPrototype = "AlienEvolutionQueen";

    [DataField("queenEvolutionAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? QueenEvolutionAction = "ActionEvolveQueen";

    [DataField("queenEvolutionActionEntity")]
    public EntityUid? QueenEvolutionActionEntity;

    [DataField("plasmaCost")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float PlasmaCost = 500f;
}
