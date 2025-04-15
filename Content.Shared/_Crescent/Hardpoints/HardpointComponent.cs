using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Crescent.Hardpoints;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HardpointComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid? anchoring;
    [ViewVariables(VVAccess.ReadWrite), DataField("range")]
    public int CannonRangeCheckRange = 25;
    [ViewVariables(VVAccess.ReadWrite), DataField("class"), AutoNetworkedField]
    public weaponTypes CompatibleTypes = weaponTypes.Ballistic;
    [ViewVariables(VVAccess.ReadWrite), DataField("size"), AutoNetworkedField]
    public weaponSizes CompatibleSizes = weaponSizes.Small;
}

[Flags, Serializable, NetSerializable]
public enum weaponTypes
{
    Energy = 1<<1,
    Ballistic = 1<<2,
    Missile = 1<<3,
    Universal = 14,

}
[Serializable, NetSerializable]
public enum weaponSizes
{
    Small = 1,
    Medium = 2,
    Large = 3
}

public class HardpointCannonAnchoredEvent : EntityEventArgs
{
    public EntityUid cannonUid;
    public EntityUid gridUid;
}

public class HardpointCannonDeanchoredEvent : EntityEventArgs
{
    public EntityUid CannonUid;
    public EntityUid gridUid;
}
