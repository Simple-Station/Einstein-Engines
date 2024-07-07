using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.Vehicle
{
    [RegisterComponent]
    public sealed partial class VehicleComponent : Component
    {


        [DataField("maxOccupants")]
        public int MaxOccupants = 1;

        [DataField("maxDrivers")]
        public int MaxDrivers = 1;

        [DataField("canMove")]
        public bool CanMove = true;

        [DataField("keyType")]
        public string? KeyType;

        [ViewVariables]
        public List<EntityUid> Occupants = new();
    }
}
