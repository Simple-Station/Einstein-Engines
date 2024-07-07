using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.ViewVariables;

namespace Content.Shared.Vehicle
{
    [RegisterComponent]
    public sealed partial class RiddenVehicleComponent : Component
    {
        [DataField("maxRiders")]
        public int MaxRiders = 1;

        [ViewVariables]
        public List<EntityUid> Riders = new();
    }
}
