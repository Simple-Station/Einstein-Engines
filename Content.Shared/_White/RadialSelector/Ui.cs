using Content.Shared.RadialSelector;
using Robust.Shared.Serialization;

namespace Content.Shared._White.RadialSelector;

[Serializable, NetSerializable]
public sealed class TrackedRadialSelectorState(List<RadialSelectorEntry> entries, NetEntity? trackedEntity = null)
    : BoundUserInterfaceState
{
    [DataField(required: true)]
    public List<RadialSelectorEntry> Entries = entries;

    public NetEntity? TrackedEntity { get; } = trackedEntity;
}
