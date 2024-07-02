using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Psionics.Abilities
{
    [RegisterComponent]
    public sealed partial class RegenerativeStasisPowerComponent : Component
    {
        [DataField("regenerativeStasisActionId",
        customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? RegenerativeStasisActionId = "ActionRegenerativeStasis";

        [DataField("regenerativeStasisActionEntity")]
        public EntityUid? RegenerativeStasisActionEntity;

        [DataField("regenerativeStasisFeedback")]
        public string RegenerativeStasisFeedback = "regenerative-stasis-feedback";
    }
}
