using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.DeltaV.Harpy
{
    /// <summary>
    /// Adds an action that allows the user to become temporarily
    /// weightless at the cost of stamina and hand usage.
    /// </summary>
    [RegisterComponent, NetworkedComponent(), AutoGenerateComponentState]
    public sealed partial class FlightComponent : Component
    {
        [DataField("toggleAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? ToggleAction = "ActionToggleFlight";

        [DataField, AutoNetworkedField]
        public EntityUid? ToggleActionEntity;

        [ViewVariables(VVAccess.ReadWrite), DataField("on"), AutoNetworkedField]
        public bool On;

        /// <summary>
        /// Stamina drain per second when flying
        /// </summary>

        [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
        public float StaminaDrainRate = 3f;

        /// <summary>
        /// Delay until the user becomes weightless.
        /// </summary>

        [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
        public float ActivationDelay = 1.0f;

        [DataField("flapSound")]
        public SoundSpecifier FlapSound = new SoundCollectionSpecifier("WingFlaps");

        /// <summary>
        /// Time between flap sounds being played
        /// </summary>
        [DataField("flapInterval")]
        public float FlapInterval = 1.25f;
        public float TimeUntilFlap;
    }
}