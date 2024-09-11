using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Map;
using System.Numerics;

namespace Content.Shared.DeltaV.Harpy
{
    /// <summary>
    ///   Adds an action to fly quickly to a given location when pressed.
    /// </summary>
    [RegisterComponent, NetworkedComponent(), AutoGenerateComponentState(true), AutoGenerateComponentPause]
    public sealed partial class DashComponent : Component
    {
        /// <summary>
        ///   The action id for the flight dash.
        /// </summary>
        [DataField("dashAction")]
        public EntProtoId DashAction = "ActionDashFlight";

        [DataField, AutoNetworkedField]
        public EntityUid? DashActionEntity;

        /// <summary>
        ///   How fast is the user moving?
        /// </summary>

        [DataField("dashSpeed"), AutoNetworkedField]
        public float DashSpeed = 15f;

        /// <summary>
        ///   How long is the user going to be in the air during the dash?
        ///   80% of the dash by default.
        /// </summary>

        [DataField("lungeDuration"), AutoNetworkedField]
        public float LungeDuration = 0.8f;

        /// <summary>
        ///   Adds additional time to each dash so it can complete the distance on shorter dashes,
        ///   and imitate inertia at longer/faster dashes.
        /// </summary>

        [DataField("glideDuration"), AutoNetworkedField]
        public float GlideDuration = 0.15f;

        /// <summary>
        ///   How much stamina should the dash drain?
        /// </summary>

        [DataField("staminaDrain")]
        public float StaminaDrain = 6f;

        /// <summary>
        ///   Delay until the user starts the dash.
        /// </summary>

        [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
        public float ActivationDelay = 0.25f;

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
        ///   Where is this entity dashing to?
        ///   Will be useful for displaying landing indicators and such.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
        public EntityCoordinates TargetCoordinates;

        [DataField("collisionProperties")]
        public CollisionProperties CollisionProperties = new();

        [DataField("damagingProperties")]
        public DamagingProperties? DamagingProperties;

        /// <summary>
        ///   Sound played when using dash action.
        /// </summary>
        /*[DataField("dashSound"), ViewVariables(VVAccess.ReadWrite)]
        public SoundSpecifier BlinkSound = new SoundPathSpecifier("/Audio/Magic/blink.ogg")
        {
            Params = AudioParams.Default.WithVolume(5f)
        };*/
    }


    /// <summary>
    ///   Properties for handling damage to the user when colliding mid dash.
    /// </summary>

    [DataDefinition]
    public sealed partial class CollisionProperties
    {
        /// <summary>
        ///   Can this entity take damage from collisions with hard objects?
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public bool Bonkable = true;

        /// <summary>
        ///   The minimum speed the wearer needs to be traveling to take damage from collision.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public float MinimumSpeed = 3f;

        /// <summary>
        ///   The length of time the wearer is stunned for on collision.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public float StunSeconds = 3f;

        /// <summary>
        /// The time duration before another collision can take place.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public float DamageCooldown = 2f;

        /// <summary>
        /// The damage per increment of speed on collision.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public float SpeedDamage = 1f;
    }

    /// <summary>
    ///   Properties for handling damage to other entities when colliding mid dash.
    /// </summary>
    [DataDefinition]
    public sealed partial class DamagingProperties
    {
        /// <summary>
        ///   Can this entity deal damage to things it passes through?
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public bool Damaging = true;

        /// <summary>
        ///   Does this entity destroy tiles it passes through?
        /// </summary>
        [DataField("destroyTiles"), ViewVariables(VVAccess.ReadWrite)]
        public bool DestroyTiles = false;

        /// <summary>
        ///   Does this entity gib shit it passes through?
        /// </summary>
        [DataField("gibber"), ViewVariables(VVAccess.ReadWrite)]
        public bool Gibber = false;

        /// <summary>
        ///   Does this entity stop the dash when hitting someone?
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public bool StopOnHit = false;

        /// <summary>
        ///   Is the damage from this entity affected by speed?
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public bool AffectedBySpeed = false;

        /// <summary>
        /// The damage multiplier per increment of speed on collision.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public float SpeedDamage = 1f;

        /// <summary>
        ///   How much stamina damage do other entities
        ///   take when colliding with this dash?.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public float StaminaDamage = 2f;

        /// <summary>
        ///   How much damage do other entities
        ///   take when colliding with this dash?.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public float Damage = 2f;

        /// <summary>
        ///   How many bleed stacks do other entities
        ///   take when colliding with this dash?.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public float BleedStacks = 2f;
    }
}

