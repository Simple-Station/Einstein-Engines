using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.ViewVariables;

namespace Content.Server.Vehicles.Components
{
    [RegisterComponent]
    public sealed class VehicleComponent : Component
    {
        public override string Name => "Vehicle";

        [DataField("maxOccupants")]
        public int MaxOccupants = 1;

        [DataField("speed")]
        public float Speed = 1.0f;

        [ViewVariables]
        public List<EntityUid> Occupants = new();

        [ViewVariables]
        public EntityUid? Driver;
    }
}
