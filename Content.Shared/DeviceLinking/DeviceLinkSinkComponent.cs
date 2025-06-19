using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Shared.DeviceLinking;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedDeviceLinkSystem))]
public sealed partial class DeviceLinkSinkComponent : Component
{
    /// <summary>
    /// The ports this sink has
    /// </summary>
    [DataField(customTypeSerializer: typeof(PrototypeIdHashSetSerializer<SinkPortPrototype>))]
    public HashSet<string>? Ports;

    /// <summary>
    /// Used for removing a sink from all linked sources when it gets removed
    /// </summary>
    [DataField("links")]
    public HashSet<EntityUid> LinkedSources = new();

    /// <summary>
    /// How high the invoke counter is allowed to get before the links to the sink are removed and the DeviceLinkOverloadedEvent gets raised
    /// If the invoke limit is 0 or less, the limit is ignored.
    /// </summary>
    [DataField]
    public int InvokeLimit = 10;
}
