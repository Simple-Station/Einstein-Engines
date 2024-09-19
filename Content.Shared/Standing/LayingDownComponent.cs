using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Standing;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LayingDownComponent : Component
{
    [DataField, AutoNetworkedField]
    public float StandingUpTime { get; set; } = 1f;

    [DataField, AutoNetworkedField]
    public float SpeedModify { get; set; } = 0.4f;

    [DataField, AutoNetworkedField]
    public bool AutoGetUp;
}

[Serializable, NetSerializable]
public sealed class ChangeLayingDownEvent : CancellableEntityEventArgs;

[Serializable, NetSerializable]
public sealed class CheckAutoGetUpEvent(NetEntity user) : CancellableEntityEventArgs
{
    public NetEntity User = user;
}
