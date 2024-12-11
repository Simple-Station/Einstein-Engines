using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Map;
using System.Numerics;

namespace Content.Shared.Dash
{
    /// <summary>
    ///   Adds an action to dash quickly to the target direction.
    /// </summary>
    [RegisterComponent, NetworkedComponent(), AutoGenerateComponentState(true), AutoGenerateComponentPause]
    public sealed partial class DashComponent : Component
    {
        /// <summary>
        ///   The action ID for the dash.
        /// </summary>
        [DataField]
        public EntProtoId DashAction = "ActionDashFlight";

        [DataField, AutoNetworkedField]
        public EntityUid? DashActionEntity;

        /// <summary>
        ///   What to multiply the entity's current sprint speed (default: 5) by to determine the speed of the dash.
        /// </summary>
        [DataField, AutoNetworkedField]
        public float DashSpeedMultiplier = 2f;

        /// <summary>
        ///   How long should the dash last?
        /// </summary>
        [DataField, AutoNetworkedField]
        public float DashDuration = 0.15f;

        /// <summary>
        ///   How much stamina should the dash cost?
        /// </summary>
        [DataField, AutoNetworkedField]
        public float StaminaCost = 21f;

        /// <summary>
        ///   The <see cref="IGameTiming.CurTime"/> timestamp at which this entity started dashing.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
        [AutoPausedField]
        public TimeSpan? DashStartTime;

        /// <summary>
        ///   Compared to <see cref="IGameTiming.CurTime"/> to land this entity, if any.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
        [AutoPausedField]
        public TimeSpan? DashLandTime;

        /// <summary>
        ///   Whether or not this entity was already landed.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
        public bool Landed = true;

        /// <summary>
        ///   Whether or not this entity is dashing, different from Landed in that its not affected by flight time.
        /// </summary>
        [DataField]
        public bool Dashing = false;

        /// <summary>
        ///   Whether or not to play a sound when the entity starts dashing.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
        public bool PlayStartSound;

        /// <summary>
        ///   Whether or not to play a sound when the entity lands.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
        public bool PlayLandSound;

        /// <summary>
        ///   Sound played when using dash action.
        /// </summary>
        /*[DataField("dashSound"), ViewVariables(VVAccess.ReadWrite)]
        public SoundSpecifier BlinkSound = new SoundPathSpecifier("/Audio/Magic/blink.ogg")
        {
            Params = AudioParams.Default.WithVolume(5f)
        };*/
    }
}

[Serializable, NetSerializable]
public sealed partial class DashDoAfterEvent : SimpleDoAfterEvent
{
}
