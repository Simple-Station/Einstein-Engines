using System.Numerics;

namespace Content.Server._Crescent.HeatSeeking;

/// <summary>
/// This is used for storing active data and the configuration of a heat seeking missile.
/// </summary>
[RegisterComponent]
public sealed partial class HeatSeekingComponent : Component
{
    /// <summary>
    /// How far away can this missile see targets
    /// </summary>
    [DataField]
    public float DefaultSeekingRange = 300f;

    [DataField]
    public Angle WeaponArc = Angle.FromDegrees(360);

    /// <summary>
    /// If null it will default to 100.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public Angle? RotationSpeed = 50f;

    /// <summary>
    /// What guidance algorithm should this missile use?
    /// Options are "PredictiveGuidance" and "PurePursuit".
    /// Defaults to "PredictiveGuidance".
    /// </summary>
    [DataField]
    public GuidanceType GuidanceAlgorithm = GuidanceType.PredictiveGuidance;

    /// <summary>
    /// What is this entity targeting?
    /// </summary>
    [DataField]
    public EntityUid? TargetEntity;

    /// <summary>
    /// How fast does the missile accelerate in m/s/s?
    /// </summary>
    [DataField]
    public float Acceleration = 50f;

    /// <summary>
    /// What is the missiles top speed in m/s?
    /// </summary>
    [DataField]
    public float TopSpeed = 50f;

    /// <summary>
    /// What is the missiles initial speed in m/s?
    /// </summary>
    [DataField]
    public float InitialSpeed = 30f;

    /// <summary>
    /// What is the missiles current speed in m/s?
    /// </summary>
    [DataField]
    public float Speed;

    /// <summary>
    /// What is the missiles field of view in degrees?
    /// </summary>
    [DataField("fov")]
    public float FOV = 90f;

    public float oldDistance;

    public Vector2 oldPosition;
}

public enum GuidanceType
{
    PredictiveGuidance = 1<<1,
    PurePursuit = 1<<2
}
