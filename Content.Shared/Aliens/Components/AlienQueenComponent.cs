using Content.Shared.Actions;
using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Aliens.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class AlienQueenComponent : Component
{
    [DataField("plasmaCostNode")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float PlasmaCostEgg = 75f;

    /// <summary>
    /// The egg prototype to use.
    /// </summary>
    [DataField("eggPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string EggPrototype = "AlienEggGrowing";

    [DataField("eggAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? EggAction = "ActionAlienEgg";

    [DataField("eggActionEntity")] public EntityUid? EggActionEntity;

    /// <summary>
    /// The entity needed to actually make acid. This will be granted (and removed) upon the entity's creation.
    /// </summary>
    [DataField(required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public EntProtoId Action;

    [DataField("actionEntity")]
    public EntityUid? ActionEntity;

    /// <summary>
    /// This will subtract (not add, don't get this mixed up) from the current plasma of the mob making acid.
    /// </summary>
    [DataField("plasmaCost")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float PlasmaCostRoyalLarva = 300f;

    [DataField("praetorianEvolutionPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<PolymorphPrototype>))]
    public string PraetorianPolymorphPrototype = "AlienEvolutionPraetorian";
}

public sealed partial class AlienEggActionEvent : InstantActionEvent { }

public sealed partial class RoyalLarvaActionEvent : EntityTargetActionEvent { }
