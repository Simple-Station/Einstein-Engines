using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.Harpy
{
    [RegisterComponent, NetworkedComponent(), AutoGenerateComponentState]
    [Access(typeof(SharedFlightSystem))]
    public sealed partial class FlightComponent : Component
    {
        [DataField("toggleAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? ToggleAction = "ActionToggleFlight";

        [DataField, AutoNetworkedField]
        public EntityUid? ToggleActionEntity;

        [ViewVariables(VVAccess.ReadWrite), DataField("on"), AutoNetworkedField]
        public bool On;

        [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
        public float StaminaDrainRate = 3f;

        [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
        public float ActivationDelay = 1.0f;

        [DataField("flapSound")]
        public SoundSpecifier FlapSound = new SoundCollectionSpecifier("WingFlaps");

        [DataField("flapInterval")]
        public float FlapInterval = 1.25f;
        public float TimeUntilFlap;
    }
}