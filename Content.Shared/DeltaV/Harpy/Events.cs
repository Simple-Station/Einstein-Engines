using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Serialization;
using Content.Shared.DoAfter;

namespace Content.Shared.DeltaV.Harpy.Events
{
    [Serializable, NetSerializable]
    public sealed partial class DashDoAfterEvent : SimpleDoAfterEvent
    {
    }

    [Serializable, NetSerializable]
    public sealed partial class FlightDoAfterEvent : SimpleDoAfterEvent
    {
    }

    [Serializable, NetSerializable]
    public sealed class FlightEvent : EntityEventArgs
    {
        public NetEntity Uid { get; }
        public bool IsFlying { get; }

        public FlightEvent(NetEntity uid, bool isFlying)
        {
            Uid = uid;
            IsFlying = isFlying;
        }
    }

}
