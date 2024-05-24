using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Shared.Pinpointer;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MultiplePinpointerComponent : Component
{
    [DataField(required: true, customTypeSerializer: typeof(PrototypeIdHashSetSerializer<MultiplePinpointerPrototype>))]
    public HashSet<string> Modes = new();

    [ViewVariables, AutoNetworkedField]
    public uint CurrentEntry = 0;
}
