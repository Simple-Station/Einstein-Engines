using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Aliens.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class AlienStalkComponent : Component
{

    [DataField("stalkAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? StalkAction = "ActionStalkAlien";

    [DataField("stalkActionEntity")]
    public EntityUid? StalkActionEntity;

    [DataField]
    public int PlasmaCost = 5;

    [ViewVariables]
    public bool IsActive;

    public float Sprint;
}
