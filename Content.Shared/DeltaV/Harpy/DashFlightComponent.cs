using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Map;
using System.Numerics;

namespace Content.Shared.DeltaV.Harpy
{
    /// <summary>
    /// Adds an action to fly quickly to a given location when pressed.
    /// </summary>
    [RegisterComponent, NetworkedComponent(), AutoGenerateComponentState]
    public sealed partial class DashFlightComponent : Component
    {
        /// <summary>
        /// The action id for the flight dash.
        /// </summary>
        [DataField]
        public EntProtoId DashAction = "ActionDashFlight";

        [DataField, AutoNetworkedField]
        public EntityUid? DashActionEntity;

        /// <summary>
        /// How fast is the user moving?
        /// </summary>

        [DataField("dashSpeed")]
        public float DashSpeed = 15f;

        /// <summary>
        /// How long is the user going to be in the air during the dash?
        /// 80% of the dash by default.
        /// </summary>

        [DataField("lungeDuration")]
        public float LungeDuration = 0.8f;


        /// <summary>
        /// How much stamina should the dash drain?
        /// </summary>

        [DataField("staminaDrain")]
        public float StaminaDrain = 6f;

        /// <summary>
        /// Delay until the user starts the dash.
        /// </summary>

        [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
        public float ActivationDelay = 0.5f;

        /// <summary>
        /// Where is the user dashing to?
        /// </summary>

        [ViewVariables(VVAccess.ReadWrite)]
        public EntityCoordinates TargetCoordinates;

        /// <summary>
        /// Sound played when using dash action.
        /// </summary>
        /*[DataField("dashSound"), ViewVariables(VVAccess.ReadWrite)]
        public SoundSpecifier BlinkSound = new SoundPathSpecifier("/Audio/Magic/blink.ogg")
        {
            Params = AudioParams.Default.WithVolume(5f)
        };*/
    }
}

