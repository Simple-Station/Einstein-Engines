using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Delivery;

/// <summary>
/// Component given to a station to indicate it can have deliveries spawn on it.
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class CargoDeliveryDataComponent : Component
{
    /// <summary>
    /// The time at which the next delivery will spawn.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextDelivery;

    /// <summary>
    /// Minimum cooldown after a delivery spawns.
    /// </summary>
    [DataField]
    public TimeSpan MinDeliveryCooldown = TimeSpan.FromMinutes(3);

    /// <summary>
    /// Maximum cooldown after a delivery spawns.
    /// </summary>
    [DataField]
    public TimeSpan MaxDeliveryCooldown = TimeSpan.FromMinutes(7);


    /// <summary>
    /// The ratio at which deliveries will spawn, based on the amount of people in the crew manifest.
    /// 1 delivery per X players.
    /// </summary>
    [DataField]
    public int PlayerToDeliveryRatio = 7;

    /// <summary>
    /// The minimum amount of deliveries that will spawn.
    /// This is not per spawner unless DistributeRandomly is false.
    /// </summary>
    [DataField]
    public int MinimumDeliverySpawn = 1;

    /// <summary>
    /// Any item that breaks or is destroyed in less than this amount of
    /// damage is one of the types of items considered fragile.
    /// </summary>
    [DataField]
    public int FragileDamageThreshold = 10;

    /// <summary>
    /// Bonus for delivering a fragile package intact.
    /// </summary>
    [DataField]
    public int FragileBonus = 100;

    /// <summary>
    /// Malus for failing to deliver a fragile package intact.
    /// </summary>
    [DataField]
    public int FragileMalus = -100;

    /// <summary>
    /// What's the chance for any one delivery to be marked as priority mail?
    /// </summary>
    [DataField]
    public float PriorityChance = 0.1f;

    /// <summary>
    /// How long until a priority delivery is considered as having failed
    /// if not delivered?
    /// </summary>
    [DataField]
    public TimeSpan PriorityDuration = TimeSpan.FromMinutes(5);

    /// <summary>
    /// What's the bonus for delivering a priority package on time?
    /// </summary>
    [DataField]
    public int PriorityBonus = 250;

    /// <summary>
    /// What's the malus for failing to deliver a priority package?
    /// </summary>
    [DataField]
    public int PriorityMalus = -250;

    /// <summary>
    /// Should deliveries be randomly split between spawners?
    /// If true, the amount of deliveries will be spawned randomly across all spawners.
    /// If false, an amount of mail based on PlayerToDeliveryRatio will be spawned on all spawners.
    /// </summary>
    [DataField]
    public bool DistributeRandomly = true;
}
