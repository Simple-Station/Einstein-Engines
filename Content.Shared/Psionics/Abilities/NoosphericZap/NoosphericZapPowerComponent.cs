using Content.Shared.Actions;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Psionics.Abilities
{
    [RegisterComponent]
    public sealed partial class NoosphericZapPowerComponent : Component
    {
        [DataField("noosphericZapActionId",
        customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? NoosphericZapActionId = "ActionNoosphericZap";

        [DataField("noosphericZapActionEntity")]
        public EntityUid? NoosphericZapActionEntity;

        [DataField("noosphericZapFeedback")]
        public string NoosphericZapFeedback = "noospheric-zap-feedback";
    }
}
