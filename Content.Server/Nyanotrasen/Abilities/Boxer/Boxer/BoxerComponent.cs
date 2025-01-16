using Content.Shared.Damage;

namespace Content.Server.Abilities.Boxer;

/// <summary>
/// Added to the boxer on spawn.
/// </summary>
[RegisterComponent]
public sealed partial class BoxerComponent : Component
{
    [DataField("modifiers", required: true)]
    public DamageModifierSet UnarmedModifiers = default!;

    [DataField("rangeBonus")]
    public float RangeBonus = 1.5f;

    /// <summary>
    /// Damage modifier with boxing glove stam damage.
    /// </summary>
    [DataField("boxingGlovesModifier")]
    public float BoxingGlovesModifier = 1.75f;

    /// <summary>
    ///   Turns the left click into a power attack when the light attack misses.
    /// </summary>
    [DataField]
    public bool HeavyOnLightMiss = true;
}
