using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.RadialSelector;

[NetSerializable, Serializable]
public enum RadialSelectorUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class RadialSelectorState(List<EntProtoId> items) : BoundUserInterfaceState
{
    public List<EntProtoId> Items { get; } = items;
}

[Serializable, NetSerializable]
public sealed class RadialSelectorSelectedMessage(EntProtoId selectedItem) : BoundUserInterfaceMessage
{
    public EntProtoId SelectedItem { get; private set; } = selectedItem;
}
