using Content.Shared.Damage;

namespace Content.Server.Traits.Assorted;

/// <summary>
///     This is used for traits that modify Oni damage modifiers.
/// </summary>
[RegisterComponent]
public sealed partial class OniDamageModifierComponent : Component
{
    /// <summary>
    ///     Which damage modifiers to override.
    /// </summary>
    [DataField("modifiers", required: true)]
    public DamageModifierSet MeleeModifierReplacers = default!;
}
