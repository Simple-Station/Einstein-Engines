using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Aliens.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(true)]
public sealed partial class AlienComponent : Component
{
    // Actions
    [DataField]
    public EntProtoId ToggleLightingAction = "ActionToggleLightingAlien";

    [DataField, AutoNetworkedField]
    public EntityUid? ToggleLightingActionEntity;

    [DataField("devourAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? DevourAction = "ActionDevour";

    /// <summary>
    /// This will subtract (not add, don't get this mixed up) from the current plasma of the mob making node.
    /// </summary>
    [DataField("plasmaCostNode")]
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public float PlasmaCostNode = 50f;

    /// <summary>
    /// The node prototype to use.
    /// </summary>
    [DataField("nodePrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string WeednodePrototype = "AlienWeednode";

    [DataField("nodeAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? WeednodeAction = "ActionAlienNode";



    [DataField("nodeActionEntity")] public EntityUid? WeednodeActionEntity;

    [DataField(required: true)]
    public DamageSpecifier WeedHeal;

}

public sealed partial class ToggleLightingAlienActionEvent : InstantActionEvent { }

public sealed partial class WeednodeActionEvent : InstantActionEvent { }
