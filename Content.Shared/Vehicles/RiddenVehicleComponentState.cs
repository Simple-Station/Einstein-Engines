using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared.Vehicle
{
    [Serializable, NetSerializable]
    public sealed class RiddenVehicleComponentState : ComponentState
    {
        public List<EntityUid> Riders;

        public RiddenVehicleComponentState(List<EntityUid> riders)
        {
            Riders = riders;
        }
    }
}
