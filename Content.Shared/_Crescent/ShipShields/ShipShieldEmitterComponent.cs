using Robust.Shared.Audio;

namespace Content.Shared._Crescent.ShipShields;

[RegisterComponent]
public sealed partial class ShipShieldEmitterComponent : Component
{
    public EntityUid? Shield;
    public EntityUid? Shielded;

    [DataField]
    public float Accumulator;

    [DataField]
    public float Damage = 0f;

    [DataField]
    public float DamageExp = 1.0f;

    [DataField]
    public float HealPerSecond = 250f;

    [DataField]
    public float UnpoweredBonus = 6f;

    // [DataField] //commented because we only have base draw now
    // public float MaxDraw = 150000f;

    [DataField]
    public float PowerDraw = 50000f;

    [DataField]
    public bool Recharging = false;

    [DataField]
    public float DamageLimit = 3500;

    [DataField]
    public float DamageOverloadTimePunishment = 30;

    public float OverloadAccumulator = 0f;
    /// <summary>
    /// On power up, players for all on vessel, pitched down.
    /// </summary>
    [DataField]
    public SoundSpecifier PowerUpSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg");

    [DataField]
    public SoundSpecifier PowerDownSound = new SoundPathSpecifier("/Audio/Effects/teleport_departure.ogg");
}
