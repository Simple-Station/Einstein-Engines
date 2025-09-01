using Robust.Shared.GameStates;

namespace Content.Shared._Crescent.Hardpoints;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HardpointAnchorableOnlyComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid? anchoredTo;
    [ViewVariables(VVAccess.ReadWrite), DataField("class")]
    public weaponTypes CompatibleTypes = weaponTypes.Ballistic;
    [ViewVariables(VVAccess.ReadWrite), DataField("size")]
    public weaponSizes CompatibleSizes = weaponSizes.Small;
}

