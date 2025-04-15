using Content.Shared.DeviceLinking;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Crescent.Hardpoints;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class HardpointFixedMountComponent : Component
{
    [DataField("toggle")]
    public ProtoId<SinkPortPrototype> Toggle = "Toggle";

    [DataField("trigger")]
    public ProtoId<SinkPortPrototype> Trigger = "SpaceArtilleryFire";
}
