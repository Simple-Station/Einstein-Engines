using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.Harpy
{
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