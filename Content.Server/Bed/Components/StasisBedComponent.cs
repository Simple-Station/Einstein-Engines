using Content.Shared.Construction.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Bed.Components
{
    [RegisterComponent]
    public sealed partial class StasisBedComponent : Component
    {
        /// <summary>
        ///     What the metabolic update rate will be multiplied by (higher = slower metabolism)
        /// </summary>
        [DataField]
        public float Multiplier = 10f;

        [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<MachinePartPrototype>))]
        public string MachinePartMetabolismModifier = "Capacitor";
    }
}
