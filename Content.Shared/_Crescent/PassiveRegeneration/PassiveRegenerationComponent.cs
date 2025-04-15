using Content.Shared.Damage;

namespace Content.Shared._Crescent.PassiveRegeneration;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class PassiveRegenerationComponent : Component
{
    [DataField("healPerTick", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier HealPerTick = default!;

    [DataField("thirstDrain", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public float thirstDrain = 0.1f;

    [DataField("hungerDrain", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public float hungerDrain = 0.1f;
}
