using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Aliens.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class AlienEvolutionComponent : Component
{
    [DataField]
    public ProtoId<PolymorphPrototype> DronePolymorphPrototype = "AlienEvolutionDrone";

    [DataField("droneEvolutionAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? DroneEvolutionAction = "ActionEvolveDrone";

    [DataField("droneEvolutionActionEntity")]
    public EntityUid? DroneEvolutionActionEntity;

    [DataField]
    public ProtoId<PolymorphPrototype> SentinelPolymorphPrototype = "AlienEvolutionSentinel";

    [DataField("sentinelEvolutionAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? SentinelEvolutionAction = "ActionEvolveSentinel";

    [DataField("sentinelEvolutionActionEntity")]
    public EntityUid? SentinelEvolutionActionEntity;

    [DataField]
    public ProtoId<PolymorphPrototype> HunterPolymorphPrototype = "AlienEvolutionHunter";

    [DataField("hunterEvolutionAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? HunterEvolutionAction = "ActionEvolveHunter";

    [DataField("hunterEvolutionActionEntity")]
    public EntityUid? HunterEvolutionActionEntity;

    [DataField("evolutionCooldown")]
    public TimeSpan EvolutionCooldown;


}
