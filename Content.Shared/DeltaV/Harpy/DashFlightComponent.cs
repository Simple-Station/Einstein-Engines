using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
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

        [DataField("dashSpeed")]
        public float DashSpeed = 30f;

        [DataField("maxDashDistance")]
        public float MaxDashDistance = 500f; //Placeholder until i figure out what the scale for this translates to

        [DataField("minDashDistance")]
        public float MinDashDistance = 1f;

        [ViewVariables(VVAccess.ReadWrite)]
        public bool IsDashing = false;

        [ViewVariables(VVAccess.ReadWrite)]
        public Vector2 DashDirection;

        [ViewVariables(VVAccess.ReadWrite)]
        public Vector2 TargetCoordinates;

        [ViewVariables(VVAccess.ReadWrite)]
        public float RemainingDistance;

        public TimeSpan LastDashTime;
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

