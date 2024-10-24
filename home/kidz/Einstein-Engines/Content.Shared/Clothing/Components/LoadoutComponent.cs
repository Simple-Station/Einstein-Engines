using Content.Shared.Roles;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Clothing.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LoadoutComponent : Component
{
    /// A list of starting gears, of which one will be given.
    /// All elements are weighted the same in the list.
    [DataField("prototypes")]
    [AutoNetworkedField]
    public List<ProtoId<StartingGearPrototype>>? StartingGear;
}
