// /Content.Shared/Vehicles/Components/VehicleKeyComponent.cs
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.Vehicles.Components
{
    [RegisterComponent]
    public sealed class VehicleKeyComponent : Component
    {
        [DataField("keyId")]
        public string KeyId { get; set; } = string.Empty;

        [DataField("isLocked")]
        public bool IsLocked { get; set; } = true;
    }
}
