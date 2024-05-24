using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Pinpointer;

[Serializable, NetSerializable, Prototype("MultiplePinpointer")]
public sealed class MultiplePinpointerPrototype : IPrototype
{
    [IdDataField, ViewVariables]
    public string ID { get; private set; } = default!;
}
