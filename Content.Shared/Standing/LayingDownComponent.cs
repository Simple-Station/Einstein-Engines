using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Content.Shared.DrawDepth;

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

    [DataField, AutoNetworkedField]
    public int NormalDrawDepth = (int) DrawDepth.DrawDepth.Mobs;

    [DataField, AutoNetworkedField]
    public int CrawlingDrawDepth = (int) DrawDepth.DrawDepth.SmallMobs;
}

[Serializable, NetSerializable]
public sealed class ChangeLayingDownEvent : CancellableEntityEventArgs;

[Serializable, NetSerializable]
public sealed class CheckAutoGetUpEvent(NetEntity user) : CancellableEntityEventArgs
{
    public NetEntity User = user;
}

[Serializable, NetSerializable]
public sealed class DrawDownedEvent(NetEntity uid) : EntityEventArgs
{
    public NetEntity Uid = uid;
}

[Serializable, NetSerializable]
public sealed class DrawStoodEvent(NetEntity uid) : EntityEventArgs
{
    public NetEntity Uid = uid;
}