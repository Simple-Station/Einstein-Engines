using System.Collections.Generic;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.ViewVariables;

namespace Content.Shared.Vehicle
{
    [RegisterComponent]
    public sealed partial class RiddenVehicleComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("riders")]
        public HashSet<EntityUid> Riders = new();

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("speed")]
        public float Speed = 5f;
    }
}
