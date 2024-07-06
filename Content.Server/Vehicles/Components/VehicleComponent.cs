// /Content.Server/Vehicles/Components/VehicleComponent.cs
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server.Vehicles.Components
{
    [RegisterComponent]
    public sealed class VehicleComponent : Component
    {
        [DataField("maxOccupants")]
        public int MaxOccupants { get; set; } = 4;

        [DataField("maxDrivers")]
        public int MaxDrivers { get; set; } = 1;

        public List<EntityUid> Occupants { get; set; } = new();
        public EntityUid? Driver { get; set; }
    }
}
