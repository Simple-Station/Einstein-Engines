using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Psionics.Abilities
{
    [RegisterComponent]
    public sealed partial class PyrokinesisPowerComponent : Component
    {
        [DataField("pyrokinesisPrechargeActionId",
        customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? PyrokinesisPrechargeActionId = "ActionPrechargePyrokinesis";

        [DataField("pyrokinesisPrechargeActionEntity")]
        public EntityUid? PyrokinesisPrechargeActionEntity;

        [DataField("pyrokinesisActionId",
        customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? PyrokinesisActionId = "ActionPyrokinesis";

        [DataField("pyrokinesisActionEntity")]
        public EntityUid? PyrokinesisActionEntity;

        [DataField("pyrokinesisFeedback")]
        public string PyrokinesisFeedback = "pyrokinesis-feedback";

        [DataField]
        public string PyrokinesisObviousPopup = "pyrokinesis-obvious";

        [DataField]
        public string PyrokinesisSubtlePopup = "pyrokinesis-subtle";

        [DataField]
        public string PyrokinesisRefundCooldown = "pyrokinesis-refund-cooldown";

        public DoAfterId? ResetDoAfter;
        public bool FireballThrown;

        [DataField]
        public SoundSpecifier SoundUse = new SoundPathSpecifier("/Audio/Items/welder.ogg");
        [DataField]
        public TimeSpan ResetDuration = TimeSpan.FromSeconds(7);
    }
}
