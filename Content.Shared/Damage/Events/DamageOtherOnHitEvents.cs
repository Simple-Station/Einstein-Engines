using Content.Shared.Damage;

namespace Content.Shared.Damage.Events;

/// <summary>
/// Raised on a throwing weapon to calculate potential damage bonuses or decreases.
/// </summary>
[ByRefEvent]
public record struct GetThrowingDamageEvent(EntityUid Weapon, DamageSpecifier Damage, List<DamageModifierSet> Modifiers, EntityUid? User);

/// <summary>
/// Raised on a throwing weapon when its damage is updated through item toggles.
/// </summary>
public record struct ThrowingDamageToggledEvent(EntityUid Weapon, bool Activated);
