using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.RadialSelector;

[RegisterComponent, NetworkedComponent]
public sealed partial class RadialSelectorComponent : Component
{
    [DataField(required: true)]
    public List<EntProtoId> Prototypes = new();
}

[NetSerializable, Serializable]
public enum RadialSelectorUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class RadialSelectorSelectedMessage(EntProtoId selectedItem) : BoundUserInterfaceMessage
{
    public EntProtoId SelectedItem { get; private set; } = selectedItem;
}
