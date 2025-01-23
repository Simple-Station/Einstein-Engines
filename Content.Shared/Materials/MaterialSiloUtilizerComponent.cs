using Robust.Shared.GameStates;

namespace Content.Shared.Materials;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MaterialSiloUtilizerComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Silo;
}
