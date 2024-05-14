using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Medical;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class VomitActionComponent : Component
{
    [DataField("vomitAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? VomitAction = "ActionVomit";

    [DataField("vomitActionEntity")]
    public EntityUid? VomitActionEntity;

    [DataField("thirstAdded")]
    public float ThirstAdded = 40f;

    [DataField("hungerAdded")]
    public float HungerAdded = 40f;

    public Container Stomach = default!;
}
