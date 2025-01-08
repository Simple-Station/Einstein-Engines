using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.SimpleStation14.Clothing;

[RegisterComponent, NetworkedComponent]
public sealed partial class ClothingGrantComponentComponent : Component
{
    [DataField("component", required: true)]
    [AlwaysPushInheritance]
    public ComponentRegistry Components { get; private set; } = new();

    [ViewVariables(VVAccess.ReadWrite)]
    public bool IsActive = false;
}
