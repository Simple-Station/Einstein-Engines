using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Aliens.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class AlienAcidComponent : Component
{
    [DataField("corrosiveAcidPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string AcidPrototype = "CorrosiveAcid";
}
