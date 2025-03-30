using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Bingle;

[RegisterComponent, NetworkedComponent]
public sealed partial class BingleComponent : Component
{
    [DataField]
    public bool Upgraded = false;
    [DataField]
    public bool Prime = false;

    [DataField]
    public EntityUid? MyPit;
}

[Serializable, NetSerializable]
public enum BingleVisual : byte
{
    Upgraded,
    Combat
}
