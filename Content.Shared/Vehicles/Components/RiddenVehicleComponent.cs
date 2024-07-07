using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.ViewVariables;

namespace Content.Shared.Vehicle
{
    [RegisterComponent]
    public partial class RiddenVehicleComponent : Component
    {
        public override string Name => "RiddenVehicle";

        [DataField("maxRiders")]
        public int MaxRiders = 1;

        [ViewVariables]
        public List<EntityUid> Riders = new();
    }
}
